using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;

namespace jarkos.projectiles.bullet
{
    internal class KnockbackExplosiveProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 7;
		}
		public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 35;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.boss)
            {
                modifiers.FinalDamage *= 0.6f;
            }
            modifiers.Knockback *= 2.5f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.FireworkFountain_Yellow);
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                dust.noGravity = true;
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (target.friendly || target.immortal)
                return false;

            return base.CanHitNPC(target);
        }
        public override void AI()
        {
            if (Projectile.timeLeft == 35)
            {
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
                    dust.velocity = Main.rand.NextVector2Circular(12f, 12f);
                    dust.scale = 2f;
                    dust.noGravity = true;
                    Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
                    dust2.velocity = Main.rand.NextVector2Circular(8f, 8f);
                    dust2.scale = 1.5f;
                }
            }

            ++Projectile.frameCounter;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                ++Projectile.frame;
            }
            if (Projectile.frame >= 7)
            {
                Projectile.Kill();
            }
            Projectile.scale = Projectile.timeLeft / 15f;
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                Main.spriteBatch.End();
                Texture2D glowTexture2 = ModContent.Request<Texture2D>("jarkos/projectiles/bullet/KnockbackExplosiveProjectile_glow").Value;

                Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
                int frameHeight = texture.Height / Main.projFrames[Projectile.type];

                Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
                Vector2 originGlowTwo = new Vector2(glowTexture2.Width / 2f, glowTexture2.Height / 2f);
                Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                float glowIntensity = 1f;
                float drawScale = 1f;
                Vector2 true_scale = new Vector2(drawScale);
                float rotation = Projectile.rotation;

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
                Color glowColor = new Color(255, 50, 50) * glowIntensity * 0.5f;

                Main.EntitySpriteDraw(glowTexture2, drawPos, null, glowColor, rotation, originGlowTwo, true_scale, SpriteEffects.None, 0);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);

                Main.EntitySpriteDraw(texture, drawPos, sourceRect, Color.White, rotation, origin, true_scale, SpriteEffects.None, 0);
                return false;
            }
        }
    }
}
