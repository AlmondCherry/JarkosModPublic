using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

namespace jarkos.projectiles.bullet
{
	public class Neutron_bullet : ModProjectile
	{
		private List<Vector2> trailPositions = new List<Vector2>();
		private const int TRAIL_LENGTH = 10;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);
			AIType = ProjectileID.Bullet;
			Projectile.width = 6;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 1200;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.extraUpdates = 2;
			Projectile.ArmorPenetration = 30;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			UpdateTrail();
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3, 3), DustID.WhiteTorch, Projectile.velocity * 0.1f, 0, default, Main.rand.NextFloat(0.8f, 1.2f));
				d.noGravity = true;
			}

			Lighting.AddLight(Projectile.Center, 0.4f, 0.45f, 0.5f);
		}

		private void UpdateTrail()
		{
			trailPositions.Insert(0, Projectile.Center);
			while (trailPositions.Count > TRAIL_LENGTH)
			{
				trailPositions.RemoveAt(trailPositions.Count - 1);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			var chargeNPC = target.GetGlobalNPC<NeutronChargeGlobalNPC>();

			chargeNPC.charge += 5f;
			chargeNPC.idleTicks = 0;
			if (chargeNPC.charge > 200f) chargeNPC.charge = 200f;

			chargeNPC.arrowDamage = Projectile.damage;
			chargeNPC.ownerIndex = Projectile.owner;
			if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Main.player[Projectile.owner].active)
			{
				chargeNPC.heldItemDamage = Main.player[Projectile.owner].HeldItem.damage;
			}

			if (chargeNPC.charge >= 200f && !chargeNPC.explosionTriggered)
			{
				chargeNPC.explosionTriggered = true;
				chargeNPC.explosionPhaseTimer = 0;
			}

			for (int i = 0; i < 12; i++)
			{
				Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
				Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, speed, 0, default, 1.2f);
				d.noGravity = true;
			}

			SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.7f, Pitch = 0.4f }, target.Center);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawTrail();
			DrawBulletWithGlow(lightColor);
			return false;
		}

		private void DrawTrail()
		{
			if (trailPositions.Count < 2) return;

			SpriteBatch sb = Main.spriteBatch;
			Texture2D pixel = TextureAssets.MagicPixel.Value;

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			for (int i = 0; i < trailPositions.Count - 1; i++)
			{
				float t0 = (float)i / trailPositions.Count;
				Vector2 start = trailPositions[i];
				Vector2 end = trailPositions[i + 1];

				float glowWidth = MathHelper.Lerp(12f, 1f, t0);
				float coreWidth = MathHelper.Lerp(4f, 0.5f, t0);

				Color glowColor = new Color(180, 190, 205) * ((1f - t0) * 0.45f);
				Color coreColor = Color.Lerp(Color.White, new Color(220, 225, 235), t0) * ((1f - t0) * 0.85f);

				DrawSegmentLine(sb, pixel, start, end, glowWidth, glowColor);
				DrawSegmentLine(sb, pixel, start, end, coreWidth, coreColor);
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawBulletWithGlow(Color lightColor)
		{
			Texture2D bulletTexture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = bulletTexture.Size() * 0.5f;

			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			Vector2 glowOrigin = glowTex.Size() * 0.5f;
			Color silverGlow = new Color(200, 205, 215) * 0.55f;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Main.EntitySpriteDraw(glowTex, drawPos, null, silverGlow, 0f, glowOrigin, 0.4f, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(bulletTexture, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 6; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(2.5f, 2.5f), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
				dust.noGravity = true;
			}
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}

		private static void DrawSegmentLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
		{
			Vector2 diff = end - start;
			float length = diff.Length();
			if (length < 0.1f) return;

			float rotation = diff.ToRotation();
			Vector2 origin = new Vector2(0f, 0.5f);
			Vector2 scale = new Vector2(length, width);
			
			sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, scale, SpriteEffects.None, 0f);
		}
	}
}
