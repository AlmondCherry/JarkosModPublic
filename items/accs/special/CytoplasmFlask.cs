using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.special
{
	public class CytoplasmFlask : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 7));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 38;
			Item.accessory = true;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(silver: 50);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<CytoplasmFlaskPlayer>().hasCytoplasmFlask = true;
		}
	}

	public class CytoplasmFlaskPlayer : ModPlayer
	{
		public bool hasCytoplasmFlask;
		private bool wasOnGround;

		public override void ResetEffects()
		{
			hasCytoplasmFlask = false;
		}

		public override void PostUpdateEquips()
		{
			if (!hasCytoplasmFlask)
			{
				wasOnGround = false;
				return;
			}

			Player.GetJumpState<CytoplasmJump>().Enable();

			bool currentlyOnGround = Player.velocity.Y == 0f || Player.sliding || Player.grappling[0] >= 0;

			if (wasOnGround && !currentlyOnGround && Player.velocity.Y < 0f)
			{
				if (Player.whoAmI == Main.myPlayer)
				{
					int sphereType = ModContent.ProjectileType<CytoplasmSphere>();
					for (int i = 0; i < Main.maxProjectiles; i++)
					{
						Projectile p = Main.projectile[i];
						if (p.active && p.type == sphereType && p.owner == Player.whoAmI)
						{
							p.Kill();
						}
					}

					Vector2 spawnPos = Player.Bottom + new Vector2(0f, 16f);
					Projectile.NewProjectile(
						Player.GetSource_FromThis("CytoplasmFlask"),
						spawnPos,
						new Vector2(Player.velocity.X, Player.velocity.Y * 0.5f),
						sphereType,
						0,
						0f,
						Player.whoAmI
					);
				}
			}

			wasOnGround = currentlyOnGround;
		}

		public override void PreUpdateMovement()
		{
			if (!hasCytoplasmFlask) return;

			bool currentlyOnGround = Player.velocity.Y == 0f || Player.sliding || Player.grappling[0] >= 0;

			if (!currentlyOnGround && Player.controlJump && Player.releaseJump)
			{
				CytoplasmSphere sphere = GetTouchingCytoplasmSphere();
				if (sphere != null)
				{
					TriggerCytoplasmJump(sphere);
					Player.releaseJump = false;
				}
			}
		}

		public CytoplasmSphere GetTouchingCytoplasmSphere()
		{
			int sphereType = ModContent.ProjectileType<CytoplasmSphere>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.type == sphereType && p.owner == Player.whoAmI)
				{
					if (p.ModProjectile is CytoplasmSphere sphere && !sphere.used && p.alpha < 220)
					{
						Rectangle playerHitbox = Player.Hitbox;
						playerHitbox.Inflate(24, 24);
						if (playerHitbox.Intersects(p.Hitbox) || Vector2.Distance(Player.Center, p.Center) <= 64f)
						{
							return sphere;
						}
					}
				}
			}
			return null;
		}

		public void TriggerCytoplasmJump(CytoplasmSphere sphere)
		{
			if (sphere != null)
			{
				sphere.PopEffect();
				sphere.Projectile.Kill();
			}

			Player.velocity.Y = -13.5f;

			float horizontalBoost = 6.5f;
			Player.velocity.X += Player.direction * horizontalBoost;
			Player.velocity.X = MathHelper.Clamp(Player.velocity.X, -16f, 16f);

			Player.fallStart = (int)(Player.position.Y / 16f);
		}
	}

	public class CytoplasmJump : ExtraJump
	{
		public override Position GetDefaultPosition() => new After(CloudInABottle);

		public override float GetDurationMultiplier(Player player) => 0.5f;

		public override void OnStarted(Player player, ref bool playSound)
		{
			playSound = false;

			var modPlayer = player.GetModPlayer<CytoplasmFlaskPlayer>();
			CytoplasmSphere sphere = modPlayer.GetTouchingCytoplasmSphere();

			if (sphere != null)
			{
				modPlayer.TriggerCytoplasmJump(sphere);
			}
		}
	}

	public class CytoplasmSphere : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;
		public bool used;
		private const int TotalLifetime = 180;

		public override void SetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Default;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = TotalLifetime;
			Projectile.netImportant = true;
			Projectile.alpha = 0;
		}

		public override bool? CanDamage() => false;

		public override void AI()
		{
			if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
			{
				Projectile.Kill();
				return;
			}
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			Projectile.velocity.X = owner.velocity.X;

			float targetY = owner.Bottom.Y + 28f;
			float diffY = targetY - Projectile.Center.Y;
			Projectile.velocity.Y = MathHelper.Clamp(diffY * 0.12f, -10f, 10f);

			if (Projectile.timeLeft <= 30)
			{
				float fade = (30 - Projectile.timeLeft) / 30f;
				Projectile.alpha = (int)(fade * 255);
			}

			if (Main.rand.NextBool(2) && Projectile.alpha < 200)
			{
				Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(18f, 18f);
				Dust dust = Dust.NewDustPerfect(spawnPos, DustID.ToxicBubble, Main.rand.NextVector2Circular(1f, 1f), 100, default, 1.1f);
				dust.noGravity = true;
			}

			Lighting.AddLight(Projectile.Center, 0.2f, 0.8f, 0.4f);
		}

		public void PopEffect()
		{
			if (used) return;
			used = true;

			SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.7f, Pitch = 0.4f }, Projectile.Center);
			SoundEngine.PlaySound(SoundID.Item85 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);

			for (int i = 0; i < 30; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.ToxicBubble, vel, 80, default, 1.4f);
				dust.noGravity = true;
			}
			for (int i = 0; i < 20; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenFairy, vel, 100, default, 1.2f);
				dust.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);

			Vector2 center = Projectile.Center - Main.screenPosition;
			float opacity = (255 - Projectile.alpha) / 255f;
			if (opacity < 0f) opacity = 0f;

			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow").Value;
			Rectangle sourceRect = new Rectangle(0, 0, glowTex.Width, glowTex.Height);
			Vector2 origin = new Vector2(glowTex.Width / 2f, glowTex.Height / 2f);

			float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.1f;
			float scale = Projectile.width / (float)glowTex.Width * 2.2f * pulse;

			Color auraColor = new Color(50, 255, 150) * 0.5f * opacity;
			Color coreColor = new Color(180, 255, 200) * 0.8f * opacity;

			Main.spriteBatch.Draw(glowTex, center, sourceRect, auraColor, 0f, origin, scale * 1.3f, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(glowTex, center, sourceRect, coreColor, 0f, origin, scale * 0.7f, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
}
