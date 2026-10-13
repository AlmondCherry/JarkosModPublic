using jarkos.buffs.debuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles
{
	public class GalvanicHookProjectile : ModProjectile
	{
		public const float MaxRange = 680f;
		public const float PullSpeed = 24f;
		public const float ExplosionRadius = 220f;

		private enum Phase
		{
			Launch = 0,
			Retreat = 1,
			Latched = 2
		}

		public override string Texture => "jarkos/textures/PortalHook/GalvanicHook";

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
			speed = PullSpeed;
		}

		public override void GrappleRetreatSpeed(Player player, ref float speed)
		{
			speed = 30f;
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
			set => Projectile.ai[0] = (float)(int)value;
		}

		private ref float HasExploded => ref Projectile.ai[1];

		public override bool? CanDamage() => false;

		public override bool ShouldUpdatePosition() => CurrentPhase == Phase.Launch || CurrentPhase == Phase.Retreat;

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (CurrentPhase == Phase.Launch)
			{
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
				CurrentPhase = Phase.Latched;
				TriggerElectricExplosion();
				Projectile.netUpdate = true;
			}
			return false;
		}

		private void TriggerElectricExplosion()
		{
			if (HasExploded != 0f)
				return;

			HasExploded = 1f;

			if (Main.netMode != NetmodeID.Server)
			{
				SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.2f, Volume = 0.9f }, Projectile.Center);

				for (int i = 0; i < 36; i++)
				{
					float angle = i * (MathHelper.TwoPi / 36f);
					Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Main.rand.NextFloat(4f, 11f);
					Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, vel, Scale: Main.rand.NextFloat(0.9f, 1.4f));
					d.noGravity = true;
				}

				for (int i = 0; i < 20; i++)
				{
					Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
					Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, vel, Scale: Main.rand.NextFloat(0.8f, 1.3f));
					d.noGravity = true;
				}
			}

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5)
				{
					float dist = Vector2.Distance(Projectile.Center, npc.Center);
					if (dist <= ExplosionRadius)
					{
						Vector2 knockDir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
						float knockStrength = MathHelper.Lerp(14f, 4f, dist / ExplosionRadius);
						npc.velocity += knockDir * knockStrength;

						npc.AddBuff(ModContent.BuffType<GalvanicSlowDebuff>(), 60);
					}
				}
			}
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

			switch (CurrentPhase)
			{
				case Phase.Launch:
				{
					float dist = Vector2.Distance(Projectile.Center, owner.MountedCenter);
					if (dist >= MaxRange)
					{
						CurrentPhase = Phase.Retreat;
						Projectile.netUpdate = true;
					}
					else if (LatchTileOrPlatform())
					{
						Projectile.velocity = Vector2.Zero;
						CurrentPhase = Phase.Latched;
						TriggerElectricExplosion();
						Projectile.netUpdate = true;

						if (dist < 40f)
						{
							Projectile.Kill();
							return;
						}
					}

					if (Main.rand.NextBool(2))
					{
						Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Projectile.velocity * 0.2f, Scale: 0.8f);
						d.noGravity = true;
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
					Projectile.velocity = Vector2.Normalize(toOwner) * 30f;
					break;
				}
				case Phase.Latched:
				{
					owner.GoingDownWithGrapple = true;
					if (HasExploded == 0f)
					{
						TriggerElectricExplosion();
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

					Vector2 dir = toHook / Math.Max(currentDist, 0.0001f);
					owner.velocity = dir * PullSpeed;
					owner.fallStart = (int)(owner.position.Y / 16f);
					owner.ChangeDir(Projectile.Center.X > owner.Center.X ? 1 : -1);

					if (Main.rand.NextBool(3))
					{
						Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), Scale: 0.7f);
						d.noGravity = true;
					}
					break;
				}
			}

			Vector2 ownerToHook = Projectile.Center - owner.MountedCenter;
			Projectile.rotation = ownerToHook.ToRotation() + MathHelper.PiOver2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player owner = Main.player[Projectile.owner];
			DrawChain(owner);

			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 origin = tex.Size() * 0.5f;
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
			return false;
		}

		private void DrawChain(Player owner)
		{
			Texture2D chain = TextureAssets.Chain.Value;
			Vector2 playerCenter = owner.MountedCenter;
			Vector2 hookCenter = Projectile.Center;
			Vector2 segment = hookCenter - playerCenter;
			float distance = segment.Length();
			if (distance < 1f)
				return;

			float rotation = segment.ToRotation() - MathHelper.PiOver2;
			Vector2 step = Vector2.Normalize(segment) * chain.Height;
			Vector2 pos = playerCenter;
			int safety = 256;

			while (distance > chain.Height && safety-- > 0)
			{
				Color color = Lighting.GetColor(pos.ToTileCoordinates());
				Main.EntitySpriteDraw(chain, pos - Main.screenPosition, null, color, rotation, chain.Size() * 0.5f, 1f, SpriteEffects.None, 0);
				pos += step;
				distance -= chain.Height;
			}
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
