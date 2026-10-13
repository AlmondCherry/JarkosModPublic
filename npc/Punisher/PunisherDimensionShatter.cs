using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using jarkos.particles;
using Terraria.GameContent;
using Terraria.Audio;
using System;

namespace jarkos.npc.Punisher
{
    public class PunisherDimensionShatter : ModProjectile
    {
        public override string Texture => "jarkos/npc/Punisher/Punisher";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 80;
            Projectile.alpha = 255;
            Projectile.hide = false;
        }
        public ref float AI_Timer => ref Projectile.ai[0];
        public ref float Angle => ref Projectile.ai[1];
        public ref float Length => ref Projectile.ai[2];
        public override void AI()
        {
            AI_Timer++;
            if (AI_Timer < 60)
            {
                if (Projectile.alpha > 50)
                {
                    Projectile.alpha -= 5;
                }
            }
            if (AI_Timer == 60)
            {
                Projectile.alpha = 0;

                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.5f }, Projectile.Center);
                Vector2 dir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));

                for (int i = 0; i < Length; i += 10)
                {
                    Vector2 pos = Projectile.Center - dir * (Length / 2f) + dir * i;
                    new Stretching_particle_out(pos, Main.rand.NextVector2Circular(5f, 5f), Color.Purple, 20, Main.rand.NextFloat(20f, 40f), Main.rand.NextFloat(2f, 4f), 0f).Spawn();
                }
            }
            if (AI_Timer > 60)
            {
                Projectile.alpha += 15;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (AI_Timer >= 60 && AI_Timer <= 65)
            {
                float collisionPoint = 0f;
                Vector2 dir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
                Vector2 start = Projectile.Center - dir * (Length / 2f);
                Vector2 end = Projectile.Center + dir * (Length / 2f);
                
                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 20f, ref collisionPoint);
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(0.5f, 0.5f);
            float progress = 1f;

            if (AI_Timer < 60) progress = AI_Timer / 60f;
            else progress = 1f - ((AI_Timer - 60f) / 20f);
            if (progress < 0) progress = 0;
            if (AI_Timer >= 60 && AI_Timer <= 65)
                progress *= 2.0f;

            float maxThickness = 12f * progress;
            float coreThickness = 4f * progress;
            float length = Length;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            int layers = 6;
            for (int i = 0; i < layers; i++)
            {
                float layerFactor = (float)i / (layers - 1);
                float currentLength = length * MathHelper.Lerp(1f, 0.2f, layerFactor);
                float currentThickness = maxThickness * MathHelper.Lerp(0.1f, 1f, layerFactor);

                Color glowColor = new Color(150, 0, 255, 0) * (0.3f * progress);
                Main.EntitySpriteDraw(texture, drawPos, new Rectangle(0, 0, 1, 1), glowColor, Angle, origin, new Vector2(currentLength, currentThickness), SpriteEffects.None, 0);
            }
            
            Main.EntitySpriteDraw(texture, drawPos, new Rectangle(0, 0, 1, 1), Color.White * progress, Angle, origin, new Vector2(length * 0.9f, coreThickness), SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            return false;
        }
    }
}
