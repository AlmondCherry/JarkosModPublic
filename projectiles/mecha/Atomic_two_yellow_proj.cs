using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.mecha
{
    public class Atomic_two_yellow_proj : ModProjectile
    {
        public override string Texture => "jarkos/textures/MagicSphereProj";

        private Vector2[] oldPositions = new Vector2[20];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.light = 0.9f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {

            for (int i = oldPositions.Length - 1; i > 0; i--)
            {
                oldPositions[i] = oldPositions[i - 1];
            }
            oldPositions[0] = Projectile.Center;

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 1f, 1f, 0.3f);

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 0, new Color(255, 255, 160), 0.9f);
                spark.noGravity = true;
                spark.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration += target.defense / 2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.YellowTorch, dustVel, 0, default, 1.5f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCHit53 with { Volume = 0.5f }, Projectile.Center);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
            Lighting.AddLight(Projectile.Center, 2f, 2f, 0.5f);
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi / 15 * i;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, velocity, 0, default, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;


            if (oldPositions.Length > 0)
            {
                oldPositions[0] = Projectile.Center;
            }

            Texture2D sphereTex = ModContent.Request<Texture2D>("jarkos/textures/MagicSphereProj", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 sphereOrigin = sphereTex.Size() / 2f;


            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 235, 30).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 200).ToVector4());

                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < oldPositions.Length; i++)
                {
                    if (oldPositions[i] != Vector2.Zero)
                    {
                        points.Add(oldPositions[i]);
                    }
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(14f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }


            if (glowTex != null)
            {
                Vector2 glowOrigin = glowTex.Size() / 2f;
                Color lightYellowGlow = new Color(255, 245, 150) * 0.5f;
                float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;
                spriteBatch.Draw(glowTex, drawPos, null, lightYellowGlow, 0f, glowOrigin, Projectile.scale * 0.7f * pulse, SpriteEffects.None, 0f);
            }


            float pulseScale = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.1f;
            Color mainYellow = new Color(255, 230, 20);
            Color brightYellow = new Color(255, 255, 180);

            spriteBatch.Draw(sphereTex, drawPos, null, mainYellow, Projectile.rotation, sphereOrigin, Projectile.scale * pulseScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(sphereTex, drawPos, null, brightYellow, Projectile.rotation, sphereOrigin, Projectile.scale * 0.5f * pulseScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(sphereTex, drawPos, null, Color.White, Projectile.rotation, sphereOrigin, Projectile.scale * 0.25f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
