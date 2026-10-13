using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using ReLogic.Content;

namespace jarkos.projectiles.melee.true_melee
{
	public class IgnisiumWaveProj : ModProjectile
	{
		public override string Texture => "jarkos/projectiles/melee/true_melee/IgnisiumWave";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 5;
			Projectile.timeLeft = 45;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.light = 0.5f;
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.velocity *= 0.989f;

			if (Main.rand.NextBool(3))
			{
				Color orangeColor = Color.Lerp(new Color(255, 140, 20), new Color(255, 210, 40), Main.rand.NextFloat());
				Vector2 sparkVel = Main.rand.NextVector2Circular(1.5f, 1.5f) - Projectile.velocity * 0.1f;
				new Electric_particle(Projectile.Center, sparkVel, orangeColor, Main.rand.Next(10, 18), Main.rand.NextFloat(0.12f, 0.18f), 0.85f, 0.92f).Spawn();
			}

			Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0.05f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 180);


			for (int i = 0; i < 3; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
				Color orangeColor = Color.Lerp(new Color(255, 140, 20), new Color(255, 210, 40), Main.rand.NextFloat());
				new Electric_particle(target.Center, vel, orangeColor, Main.rand.Next(12, 20), Main.rand.NextFloat(0.14f, 0.22f), 0.85f, 0.91f).Spawn();
			}
			new Circular_glow(target.Center, Vector2.Zero, new Color(255, 140, 20), 14, 0.35f, 0.85f).Spawn();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;
			Texture2D waveTex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = waveTex.Size() / 2f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			float lifeProgress = 1f - (Projectile.timeLeft / 45f);
			float alpha = MathHelper.Clamp(1f - lifeProgress * lifeProgress, 0f, 1f);


			float scaleX = MathHelper.Lerp(1.1f, 0.2f, lifeProgress) * Projectile.scale;
			float scaleY = MathHelper.Lerp(0.85f, 0.12f, lifeProgress) * Projectile.scale;
			Vector2 drawScale = new Vector2(scaleX, scaleY);

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


			Color outerGlowColor = new Color(255, 130, 20) * (0.85f * alpha);
			Color innerCoreColor = Color.White * (0.95f * alpha);


			sb.Draw(waveTex, drawPos, null, outerGlowColor, Projectile.rotation, origin, drawScale * 1.15f, SpriteEffects.None, 0f);

			sb.Draw(waveTex, drawPos, null, innerCoreColor, Projectile.rotation, origin, drawScale, SpriteEffects.None, 0f);

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
