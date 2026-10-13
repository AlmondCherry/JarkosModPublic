using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using ReLogic.Content;
using System;
namespace Jarkos.projectiles.mecha
{
    public class AtomgunExplosion : ModProjectile
    {
        public override string Texture => "jarkos/particles/Circular_glow";
        public override void SetDefaults()
        {
            Projectile.width = 1500;
            Projectile.height = 1500;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.DamageType = DamageClass.Generic;
        }
        public override void AI()
        {
            if (Projectile.timeLeft == 60)
            {
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.5f, Pitch = -0.5f }, Projectile.Center);
                if (Main.netMode != NetmodeID.Server)
                {
                    Main.LocalPlayer.GetModPlayer<jarkos.systems.PurgatoryPlayer>().AddShake(15f, 40);
                }
            }
            Projectile.scale += 0.2f;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.lifeMax > 500)
            {
                modifiers.FinalDamage *= 0f;
                modifiers.FinalDamage += 300f;
            }
            else
            {
                modifiers.FinalDamage *= 0f;
                modifiers.FinalDamage += 99999f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            float progress = 1f - (Projectile.timeLeft / 60f);
            Texture2D ringTexture = ModContent.Request<Texture2D>("jarkos/particles/Accurate_circle_particle").Value;
            Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/textures/StarTexture").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            for (int i = 0; i < 3; i++)
            {
                float ringProgress = MathHelper.Clamp(progress * 1.5f - i * 0.2f, 0f, 1f);
                if (ringProgress > 0 && ringProgress < 1f)
                {
                    float scale = MathHelper.Lerp(0.1f, 12f, (float)Math.Pow(ringProgress, 0.5f));
                    float alpha = (1f - ringProgress) * (i == 0 ? 1f : 0.6f);
                    Color color = i == 0 ? new Color(255, 200, 50) : (i == 1 ? new Color(255, 100, 50) : new Color(255, 255, 150));
                    spriteBatch.Draw(ringTexture, drawPos, null, color * alpha, 0f, ringTexture.Size() / 2f, scale, SpriteEffects.None, 0f);
                }
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            return false;
        }
    }
}
