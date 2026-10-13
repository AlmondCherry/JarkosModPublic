using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Punisher
{
    public class CosmicSlash : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public ref float Timer => ref Projectile.ai[0];
        public ref float SlashLength => ref Projectile.ai[1];
        private const int Phase1_Warning = 60;
        private const int Phase2_Active = 15;
        private const int Phase3_Fade = 15;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Phase1_Warning + Phase2_Active + Phase3_Fade;
        }
        public override void AI()
        {
            Timer++;
            if (SlashLength == 0) SlashLength = 1500f;
            if (Timer == Phase1_Warning)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f, Pitch = -0.5f }, Projectile.Center);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Timer < Phase1_Warning || Timer > Phase1_Warning + Phase2_Active)
            {
                return false;
            }
            float collisionPoint = 0f;
            Vector2 start = Projectile.Center - Projectile.rotation.ToRotationVector2() * (SlashLength / 2f);
            Vector2 end = Projectile.Center + Projectile.rotation.ToRotationVector2() * (SlashLength / 2f);
            float thickness = 25f;
            
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, thickness, ref collisionPoint);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("jarkos/npc/Punisher/smoke_burst", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Effect slashEffect = ModContent.Request<Effect>("jarkos/npc/Punisher/Effects/CosmicSlash", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            
            slashEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            float width = SlashLength;
            float height = 2f;
            float alpha = 1f;

            if (Timer < Phase1_Warning)
            {
                height = 2f;
                alpha = 0.5f + 0.3f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 15f);
                slashEffect.Parameters["uPhase"]?.SetValue(0f);
            }
            else if (Timer < Phase1_Warning + Phase2_Active)
            {
                float progress = (Timer - Phase1_Warning) / Phase2_Active;
                height = 25f + (float)Math.Sin(progress * MathHelper.Pi) * 15f;
                slashEffect.Parameters["uPhase"]?.SetValue(1f);
            }
            else
            {
                float progress = (Timer - Phase1_Warning - Phase2_Active) / Phase3_Fade;
                height = MathHelper.Lerp(25f, 0f, progress);
                alpha = MathHelper.Lerp(1f, 0f, progress);
                slashEffect.Parameters["uPhase"]?.SetValue(2f);
            }
            slashEffect.Parameters["uAlpha"]?.SetValue(alpha);

            Vector2 scale = new Vector2(width / tex.Width, height / tex.Height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, slashEffect, Main.GameViewMatrix.ZoomMatrix);
            Main.spriteBatch.Draw(tex, pos, null, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            return false;
        }
    }
}
