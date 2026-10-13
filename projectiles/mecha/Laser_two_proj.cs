using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
	public class Laser_two_proj : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Default;
			Projectile.penetrate = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = true;
			Projectile.scale = 1f;
			Projectile.timeLeft = 200;
			Projectile.damage = 2;
			Projectile.light = 0.85f;
			Projectile.aiStyle = -1;
			DrawOffsetX = -(int)((10 / 2) - (Projectile.Size.X / 2));
			DrawOriginOffsetY = -(int)((36 / 2) - (Projectile.Size.Y / 2));
		}
		 public override void AI(){
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Main.rand.NextBool(1))
			{
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.GemEmerald, Scale: 0.7f);
            dust.velocity /= 20;
			dust.noGravity = true;
			}
        }
		public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(Projectile.type);
			 Draw(Projectile);
			return true;
		}
		public void Draw(Projectile projectile)
        {
            Texture2D line = ModContent.Request<Texture2D>("jarkos/textures/Flare", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                float progress = 1 - (float)i / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.8f;
                Vector2 AfterImagePos = Projectile.oldPos[i] - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f);
                float startScale = projectile.ai[2] + sineScale;
                Color col = Color.LimeGreen;
                float easedFadeValue = progress * progress;
                Vector2 lineScale = new Vector2(1.25f, 0.3f + 0.4f * progress);
                Vector2 lineScale2 = new Vector2(1.25f, 0.05f + 0.05f * progress);
                Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.4f * easedFadeValue, projectile.velocity.ToRotation(), line.Size() / 2f, lineScale2 * projectile.scale, SpriteEffects.None);
                Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 0.5f * easedFadeValue, projectile.velocity.ToRotation(), line.Size() / 2f, lineScale * startScale, SpriteEffects.None);
                Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.5f * easedFadeValue, projectile.velocity.ToRotation(), line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);
            }
        }
}
}
