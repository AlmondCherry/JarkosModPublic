using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;

namespace jarkos.projectiles.bullet
{
	public class NoxtiliumSporeProj : ModProjectile
	{
		public override string Texture => "jarkos/textures/MagicSphereProj";

		private int LiveTimer
		{
			get => (int)Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void AI()
		{
			LiveTimer++;

			Projectile.velocity *= 0.94f;
			Projectile.rotation += 0.05f;


			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 0, 0, DustID.PurpleTorch, 0f, 0f, 100, default, Main.rand.NextFloat(0.8f, 1.3f));
				d.noGravity = true;
				d.velocity = Main.rand.NextVector2Circular(1f, 1f);
			}

			if (Main.rand.NextBool(6))
			{
				Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Shadowflame, 0f, 0f, 100, default, 0.7f);
				d.noGravity = true;
				d.velocity *= 0.3f;
			}

			Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.7f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Venom, 300);
			target.AddBuff(BuffID.Poisoned, 300);

			for (int i = 0; i < 8; i++)
			{
				Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleCrystalShard, speed, Scale: 1.2f);
				d.noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.4f, Pitch = 0.5f }, Projectile.Center);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;

			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = mainTex.Size() / 2f;

			float alpha = 1f;
			if (Projectile.timeLeft < 30)
			{
				alpha = Projectile.timeLeft / 30f;
			}

			float pulse = 1f + (float)Math.Sin(LiveTimer * 0.12f + Projectile.whoAmI * 1.5f) * 0.18f;
			float scale = Projectile.scale * 0.75f * pulse;

			SpriteBatch sb = Main.spriteBatch;


			if (glowTex != null)
			{
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Vector2 glowOrigin = glowTex.Size() / 2f;
				float glowScale = 0.325f * pulse;
				Color purpleGlow = new Color(170, 40, 255) * (0.375f * alpha);
				Color lightVioletCore = new Color(220, 120, 255) * (0.225f * alpha);

				sb.Draw(glowTex, drawPos, null, purpleGlow, 0f, glowOrigin, glowScale, SpriteEffects.None, 0f);
				sb.Draw(glowTex, drawPos, null, lightVioletCore, 0f, glowOrigin, glowScale * 0.5f, SpriteEffects.None, 0f);

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			Color sporeColor = new Color(220, 160, 255) * alpha;
			sb.Draw(mainTex, drawPos, null, sporeColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);

			return false;
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 5; i++)
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1f);
				d.noGravity = true;
			}
		}
	}
}
