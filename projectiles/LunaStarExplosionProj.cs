using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Audio;

namespace jarkos.projectiles
{
    public class LunaStarExplosionProj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/bullet/StarLightExplosion";

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
            Projectile.DamageType = DamageClass.Generic;
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
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.BlueTorch);
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                dust.noGravity = true;
                dust.scale = 1.5f;
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
                for (int i = 0; i < 25; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
                    dust.velocity = Main.rand.NextVector2Circular(12f, 12f);
                    dust.scale = 2f;
                    dust.noGravity = true;

                    Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
                    dust2.velocity = Main.rand.NextVector2Circular(8f, 8f);
                    dust2.scale = 1.4f;
                    dust2.noGravity = true;
                }
            }

            ++Projectile.frameCounter;
            if (Projectile.frameCounter >= 5)
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
            Main.spriteBatch.End();

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];

            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float glowIntensity = Projectile.timeLeft / 35f;
            float drawScale = Projectile.scale;
            Vector2 trueScale = new Vector2(drawScale);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            
            Color glowColor = new Color(0, 195, 255) * glowIntensity * 0.7f;
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, glowColor, Projectile.rotation, origin, trueScale * 1.3f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, RasterizerState.CullNone, null, Main.Camera.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(texture, drawPos, sourceRect, Color.White, Projectile.rotation, origin, trueScale, SpriteEffects.None, 0);
            return false;
        }
    }
}
