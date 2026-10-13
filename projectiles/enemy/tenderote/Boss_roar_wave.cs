using System;
using jarkos.npc.tenderote;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Boss_roar_wave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";
        private float maxRadius = 2500f;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
        }
        public override void AI()
        {
            float currentRadius = (1f - (Projectile.timeLeft / 120f)) * maxRadius;
            Projectile.width = (int)(currentRadius * 2);
            Projectile.height = (int)(currentRadius * 2);
            NPC boss = Main.npc[(int)Projectile.ai[0]];
            if (boss.active && boss.type == ModContent.NPCType<Tenderote_head>())
            {
                Projectile.Center = boss.Center;
            }
            if (Main.rand.NextBool(3))
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(currentRadius, currentRadius), DustID.WhiteTorch, Vector2.Zero);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int bossIndex = (int)Projectile.ai[0];
            if (bossIndex >= 0 && bossIndex < Main.maxNPCs && Main.npc[bossIndex].active)
            {
                NPC boss = Main.npc[bossIndex];
                float distanceToBoss = Vector2.Distance(targetHitbox.Center.ToVector2(), boss.Center);
                if (distanceToBoss <= 300f)
                {
                    return false;
                }
            }
            float currentRadius = (1f - (Projectile.timeLeft / 120f)) * maxRadius;
            float distance = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            if (Math.Abs(distance - currentRadius) < 60f)
            {
                return true;
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/StormWave").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float half = tex.Width / 2f;
            float progress = 1f - (Projectile.timeLeft / 120f);
            float scale = maxRadius / (half * 0.82f);
            float currentScale = scale * progress;
            Effect ringShader = ModContent.Request<Effect>("jarkos/Shaders/SporeRing", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/WaveNoise", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (ringShader != null)
            {
                ringShader.Parameters["uProgress"]?.SetValue(progress);
                ringShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3.5f);
                ringShader.Parameters["uColor"]?.SetValue(new Vector3(0.6f, 0.6f, 0.6f));
                ringShader.Parameters["uSeed"]?.SetValue(Projectile.identity * 0.1f);
                ringShader.Parameters["uImage0"]?.SetValue(noise);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, ringShader, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                float alpha = 1f - progress;
                spriteBatch.Draw(tex, drawPos, null, Color.White * alpha, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
