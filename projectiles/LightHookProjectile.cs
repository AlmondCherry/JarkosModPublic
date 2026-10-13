using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles
{
	public class LightHookProjectile : ModProjectile
	{
		public const float MaxRange = 750f;
		public const float PullSpeed = 25f;
		public const float HighlightRadius = 500f;

		private enum Phase
		{
			Launch = 0,
			Retreat = 1,
			Latched = 2
		}

		public override string Texture => "jarkos/textures/PortalHook/LightHook";

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
				{
					p.Kill();
					return false;
				}
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
			Projectile.timeLeft = 7200;
			Projectile.tileCollide = true;
			Projectile.netImportant = true;
			Projectile.ignoreWater = true;
		}

		private Phase CurrentPhase
		{
			get => (Phase)(int)Projectile.ai[0];
			set => Projectile.ai[0] = (float)(int)value;
		}

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
				Projectile.netUpdate = true;
				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.4f, Volume = 0.8f }, Projectile.Center);
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

			Lighting.AddLight(Projectile.Center, 1.4f, 1.3f, 0.8f);

			HighlightOresAroundHook();

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
						Projectile.netUpdate = true;

						if (dist < 40f)
						{
							Projectile.Kill();
							return;
						}
					}

					if (Main.rand.NextBool(2))
					{
						Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, Projectile.velocity * 0.2f, Scale: 0.9f);
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
					Projectile.timeLeft = 7200;

					if (owner.whoAmI == Main.myPlayer)
					{
						if (owner.controlHook || owner.controlJump)
						{
							Projectile.Kill();
							return;
						}
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

					Vector2 dir = toHook / currentDist;
					owner.velocity = dir * PullSpeed;
					owner.fallStart = (int)(owner.position.Y / 16f);
					owner.ChangeDir(Projectile.Center.X > owner.Center.X ? 1 : -1);

					if (Main.rand.NextBool(3))
					{
						Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.GoldCoin, Vector2.Zero, Scale: 0.8f);
						d.noGravity = true;
					}
					break;
				}
			}

			Vector2 ownerToHook = Projectile.Center - owner.MountedCenter;
			Projectile.rotation = ownerToHook.ToRotation() + MathHelper.PiOver2;
		}

		private void HighlightOresAroundHook()
		{
			Vector2 center = Projectile.Center;
			int radiusInTiles = (int)(HighlightRadius / 16f);
			int startX = (int)MathHelper.Clamp((int)(center.X / 16f) - radiusInTiles, 0, Main.maxTilesX - 1);
			int endX = (int)MathHelper.Clamp((int)(center.X / 16f) + radiusInTiles, 0, Main.maxTilesX - 1);
			int startY = (int)MathHelper.Clamp((int)(center.Y / 16f) - radiusInTiles, 0, Main.maxTilesY - 1);
			int endY = (int)MathHelper.Clamp((int)(center.Y / 16f) + radiusInTiles, 0, Main.maxTilesY - 1);

			float radiusSq = HighlightRadius * HighlightRadius;

			for (int x = startX; x <= endX; x++)
			{
				for (int y = startY; y <= endY; y++)
				{
					Vector2 tileWorldPos = new Vector2(x * 16f + 8f, y * 16f + 8f);
					if (Vector2.DistanceSquared(center, tileWorldPos) <= radiusSq)
					{
						Tile tile = Main.tile[x, y];
						if (tile.HasTile && (Main.tileSpelunker[tile.TileType] || TileID.Sets.Ore[tile.TileType]))
						{
							Lighting.AddLight(new Vector2(x * 16f, y * 16f), 0.95f, 0.95f, 0.4f);
							if (Main.rand.NextBool(150))
							{
								Dust d = Dust.NewDustPerfect(tileWorldPos, DustID.GoldCoin, Vector2.Zero, Scale: 0.6f);
								d.noGravity = true;
							}
						}
					}
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player owner = Main.player[Projectile.owner];
			DrawChain(owner);

			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 origin = tex.Size() * 0.5f;
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0);
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
				Color color = Color.White;
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
