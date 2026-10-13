using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles
{
	public class TightGripHookProjectile : ModProjectile
	{
		public const float MaxRange = 800f;

		private enum Phase
		{
			Launch = 0,
			Retreat = 1,
			LatchedTile = 2,
			LatchedNPC = 3
		}

		public override string Texture => "jarkos/textures/PortalHook/TightGripHook";

		public override void SetStaticDefaults()
		{
			Main.projHook[Type] = true;
			ProjectileID.Sets.SingleGrappleHook[Type] = true;
		}

		public override bool? CanUseGrapple(Player player)
		{
			int myType = Type;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == myType)
					return false;
			}
			return true;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks)
		{
			numHooks = 1;
		}

		public override void GrapplePullSpeed(Player player, ref float speed)
		{
			speed = 20f;
		}

		public override void GrappleRetreatSpeed(Player player, ref float speed)
		{
			speed = 28f;
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 3600;
			Projectile.tileCollide = true;
			Projectile.netImportant = true;
			Projectile.ignoreWater = true;
		}

		private Phase CurrentPhase
		{
			get => (Phase)(int)Projectile.ai[0];
			set => Projectile.ai[0] = (int)value;
		}
		private ref float RopeLength => ref Projectile.ai[1];
		private ref float LatchedNPCIndex => ref Projectile.localAI[0];
		private Vector2 latchedNPCOffset = Vector2.Zero;

		public override bool? CanDamage() => false;
		public override bool ShouldUpdatePosition() => CurrentPhase == Phase.Launch || CurrentPhase == Phase.Retreat;
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (CurrentPhase == Phase.Launch)
			{
				Player owner = Main.player[Projectile.owner];
				int tileX = (int)(Projectile.Center.X / 16f);
				int tileY = (int)(Projectile.Center.Y / 16f);
				if (oldVelocity.X != Projectile.velocity.X)
				{
					if (oldVelocity.X > 0) tileX = (int)((Projectile.position.X + Projectile.width + 2f) / 16f);
					else if (oldVelocity.X < 0) tileX = (int)((Projectile.position.X - 2f) / 16f);
				}
				if (oldVelocity.Y != Projectile.velocity.Y)
				{
					if (oldVelocity.Y > 0) tileY = (int)((Projectile.position.Y + Projectile.height + 2f) / 16f);
					else if (oldVelocity.Y < 0) tileY = (int)((Projectile.position.Y - 2f) / 16f);
				}
				Projectile.Center = new Vector2(tileX * 16f + 8f, tileY * 16f + 8f);
				Projectile.velocity = Vector2.Zero;
				CurrentPhase = Phase.LatchedTile;
				RopeLength = Vector2.Distance(owner.MountedCenter, Projectile.Center);
				Projectile.netUpdate = true;
				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.2f }, Projectile.Center);
				}
			}
			return false;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead || owner.outOfRange)
			{
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			Vector2 ownerToHook = Projectile.Center - owner.MountedCenter;
			Projectile.rotation = ownerToHook.ToRotation() + MathHelper.PiOver2;

			switch (CurrentPhase)
			{
				case Phase.Launch:
				{
					float dist = Vector2.Distance(Projectile.Center, owner.MountedCenter);
					if (dist >= MaxRange)
					{
						CurrentPhase = Phase.Retreat;
						Projectile.netUpdate = true;
						break;
					}
					else if (LatchTileOrPlatform())
					{
						Projectile.velocity = Vector2.Zero;
						CurrentPhase = Phase.LatchedTile;
						RopeLength = dist;
						Projectile.netUpdate = true;

						if (dist < 40f)
						{
							Projectile.Kill();
							return;
						}
					}

					for (int i = 0; i < Main.maxNPCs; i++)
					{
						NPC npc = Main.npc[i];
						if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5 && Projectile.Hitbox.Intersects(npc.Hitbox))
						{
							LatchedNPCIndex = npc.whoAmI;
							latchedNPCOffset = Projectile.Center - npc.Center;
							RopeLength = Vector2.Distance(owner.MountedCenter, Projectile.Center);
							CurrentPhase = Phase.LatchedNPC;
							Projectile.velocity = Vector2.Zero;
							Projectile.netUpdate = true;
							if (Main.netMode != NetmodeID.Server)
							{
								SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.4f }, Projectile.Center);
							}
							break;
						}
					}
					break;
				}

				case Phase.Retreat:
				{
					Vector2 toOwner = owner.MountedCenter - Projectile.Center;
					float dist = toOwner.Length();
					if (dist < 20f)
					{
						Projectile.Kill();
						return;
					}
					Projectile.velocity = Vector2.Normalize(toOwner) * 28f;
					break;
				}

				case Phase.LatchedTile:
				{
					owner.GoingDownWithGrapple = true;
					if (owner.controlHook)
					{
						CurrentPhase = Phase.Retreat;
						Projectile.netUpdate = true;
						break;
					}

					Vector2 toHook = Projectile.Center - owner.MountedCenter;
					if (toHook.Y > 0f)
					{
						owner.GoingDownWithGrapple = true;
						owner.releaseDown = true;
						if (owner.velocity.Y >= 0f)
						{
							owner.position.Y += 2f;
						}
					}
					float currentDist = toHook.Length();

					if (currentDist < 40f)
					{
						Projectile.Kill();
						return;
					}
					if (RopeLength <= 0f) RopeLength = currentDist;

					if (owner.controlDown)
					{
						if (RopeLength < 750f)
						{
							RopeLength = Math.Min(RopeLength + 6f, 750f);
						}
					}
					else if (currentDist < RopeLength)
					{
						RopeLength = Math.Max(currentDist, 30f);
					}
					else if (currentDist > RopeLength + 1f)
					{
						Vector2 pullDir = toHook / Math.Max(currentDist, 0.0001f);
						owner.velocity += pullDir * 1.5f;
						owner.fallStart = (int)(owner.position.Y / 16f);
					}
					owner.ChangeDir(Projectile.Center.X > owner.Center.X ? 1 : -1);
					break;
				}

				case Phase.LatchedNPC:
				{
					owner.GoingDownWithGrapple = true;
					int npcIdx = (int)LatchedNPCIndex;
					if (npcIdx < 0 || npcIdx >= Main.maxNPCs)
					{
						CurrentPhase = Phase.Retreat;
						break;
					}

					NPC latched = Main.npc[npcIdx];
					if (!latched.active || latched.life <= 0 || owner.controlHook)
					{
						CurrentPhase = Phase.Retreat;
						Projectile.netUpdate = true;
						break;
					}

					Projectile.Center = latched.Center + latchedNPCOffset;

					Vector2 toHook = Projectile.Center - owner.MountedCenter;
					if (toHook.Y > 0f)
					{
						owner.GoingDownWithGrapple = true;
						owner.releaseDown = true;
						if (owner.velocity.Y >= 0f)
						{
							owner.position.Y += 2f;
						}
					}
					float currentDist = toHook.Length();

					if (RopeLength <= 0f) RopeLength = currentDist;

					const float MinRopeLength = 30f;
					const float MaxRopeLength = 750f;

					if (owner.controlDown)
					{
						if (RopeLength < MaxRopeLength)
						{
							RopeLength = Math.Min(RopeLength + 6f, MaxRopeLength);
						}
					}
					else if (currentDist < RopeLength)
					{
						RopeLength = Math.Max(currentDist, MinRopeLength);
					}

					if (currentDist > RopeLength + 1f)
					{
						Vector2 pullDir = toHook / Math.Max(currentDist, 0.0001f);
						owner.velocity += pullDir * 1.8f;
						owner.fallStart = (int)(owner.position.Y / 16f);
					}
					owner.ChangeDir(Projectile.Center.X > owner.Center.X ? 1 : -1);
					break;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player owner = Main.player[Projectile.owner];
			Vector2 playerCenter = owner.MountedCenter;
			Vector2 hookCenter = Projectile.Center;
			Vector2 diff = hookCenter - playerCenter;
			float length = diff.Length();
			if (length == 0f) return false;

			Vector2 step = diff / length;
			float drawn = 0f;

			while (drawn < length)
			{
				Vector2 pos = playerCenter + step * drawn;
				Color color = Lighting.GetColor((int)(pos.X / 16f), (int)(pos.Y / 16f), Color.LightSteelBlue);
				Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, pos - Main.screenPosition, new Rectangle(0, 0, 2, 2), color, 0f, new Vector2(1, 1), 1f, SpriteEffects.None, 0);
				drawn += 4f;
			}
			return true;
		}

		private bool LatchTileOrPlatform()
		{
			Point tileCoords = Projectile.Center.ToTileCoordinates();
			int startX = (int)MathHelper.Clamp(tileCoords.X - 1, 0, Main.maxTilesX - 1);
			int endX = (int)MathHelper.Clamp(tileCoords.X + 1, 0, Main.maxTilesX - 1);
			int startY = (int)MathHelper.Clamp(tileCoords.Y - 1, 0, Main.maxTilesY - 1);
			int endY = (int)MathHelper.Clamp(tileCoords.Y + 1, 0, Main.maxTilesY - 1);

			for (int x = startX; x <= endX; x++)
			{
				for (int y = startY; y <= endY; y++)
				{
					Tile tile = Main.tile[x, y];
					if (tile.HasTile && !tile.IsActuated)
					{
						bool isSolid = Main.tileSolid[tile.TileType];
						bool isPlatform = TileID.Sets.Platforms[tile.TileType];

						if (isSolid || isPlatform)
						{
							Rectangle tileRect = new Rectangle(x * 16, y * 16, 16, 16);
							if (Projectile.Hitbox.Intersects(tileRect))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}
	}
}
