using Jarkos.trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.systems;
namespace jarkos.projectiles.magic.other
{
	public class FrostPlasma : ModProjectile
	{
		private const float HomingRange = 600f;
        private const float Speed = 8.5f;
        private const int HomingDelay = 25;
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float WavePhase => Projectile.ai[1];
        private static ReLogic.Content.Asset<Texture2D> frostGlow;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 70;
            ProjectileID.Sets.TrailingMode[Type] = 2;

            if (!Main.dedServ)
                frostGlow = ModContent.Request<Texture2D>("jarkos/textures/FrostGlow");
        }
        public override void Unload()
        {
            frostGlow = null;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, 0.2f, 0.45f, 0.7f);

            NPC target = FindTarget();
            float distance = target != null ? Projectile.Distance(target.Center) : HomingRange;

            if (target != null && Timer > HomingDelay)
            {
                float currentAngle = Projectile.velocity.ToRotation();
                float targetAngle = (target.Center - Projectile.Center).ToRotation();
                float turnStrength = MathHelper.Lerp(0.012f, 0.035f, Terraria.Utils.GetLerpValue(HomingDelay, 130f, Timer, true));
                float newAngle = Terraria.Utils.AngleLerp(currentAngle, targetAngle, turnStrength);

                Projectile.velocity = newAngle.ToRotationVector2() * Speed;
            }

            else if (Timer > HomingDelay)
            {
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * Speed;
            }

            float weaveStrength = 0.012f * Terraria.Utils.GetLerpValue(40f, 170f, distance, true);

            if (weaveStrength > 0.001f)
                Projectile.velocity = Projectile.velocity.RotatedBy(MathF.Sin(Timer * 0.09f + WavePhase) * weaveStrength);

            if (Timer % 5 == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, Main.rand.NextFloat(0.7f, 1.1f));
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(16))
            {
                new Spark(Projectile.Center, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.7f, 0.7f), new Color(150, 220, 255), Main.rand.Next(12, 20), Main.rand.NextFloat(0.3f, 0.5f), 0.8f).Spawn();
            }
        }
        private NPC FindTarget()
        {
            NPC best = null;
            float bestDistance = HomingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active || !npc.CanBeChasedBy(Projectile))
                    continue;

                float distance = Projectile.Distance(npc.Center);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = npc;
                }
            }
            return best;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 300);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = -0.2f }, Projectile.Center);

            new Circular_glow(Projectile.Center, Vector2.Zero, new Color(170, 225, 255), 16, 0.55f, 0.85f).Spawn();
            new Circleedge(Projectile.Center, Vector2.Zero, new Color(120, 200, 255), 18, 0.22f, 0.75f).Spawn();

            for (int i = 0; i < 7; i++)
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), new Color(160, 220, 255), Main.rand.Next(14, 26), Main.rand.NextFloat(0.35f, 0.7f), 0.9f).Spawn();
            }

            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(3.5f, 3.5f), 0, default, Main.rand.NextFloat(0.9f, 1.5f));
                dust.noGravity = true;
            }

            for (int i = 0; i < 4; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustID.Snow, Main.rand.NextVector2Circular(2f, 2f), 80, default, Main.rand.NextFloat(0.8f, 1.2f));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawHead();

            return false;
        }
        private void DrawTrail()
        {
            if (jarkos.systems.FrostShaders.FrostStream != null)
                DrawShaderTrail();
            else
                DrawFallbackTrail();
        }
        private void DrawShaderTrail()
        {
            Effect effect = jarkos.systems.FrostShaders.FrostStream;
            Main.spriteBatch.End();

            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["uOpacity"]?.SetValue(1f);
            effect.Parameters["uColor"]?.SetValue(new Color(60, 140, 230).ToVector4());
            effect.Parameters["uColorCore"]?.SetValue(new Color(215, 245, 255).ToVector4());

            Vector2 half = Projectile.Size * 0.5f;
            Vector2[] points = new Vector2[Projectile.oldPos.Length + 1];
            points[0] = Projectile.Center;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
                points[i + 1] = Projectile.oldPos[i] == Vector2.Zero ? Vector2.Zero : Projectile.oldPos[i] + half;

            jarkos.systems.TrailRenderer.Draw(effect, points, t => 19f * MathF.Pow(1f - t, 1.0f) + 0.6f, t => Color.White * (1f - t), jarkos.systems.FrostShaders.FrostNoise);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawFallbackTrail()
        {
            Texture2D glow = frostGlow?.Value;

            if (glow == null)
                return;

            Vector2 half = Projectile.Size * 0.5f;
            Vector2 glowOrigin = glow.Size() * 0.5f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            int length = Projectile.oldPos.Length;
            int steps = length * 2 - 1;

            for (int s = steps - 1; s >= 0; s--)
            {
                int i = s / 2;
                Vector2 posWorld;

                if ((s & 1) == 0)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero)
                        continue;
                    posWorld = Projectile.oldPos[i];
                }
                else
                {
                    if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                        continue;
                    posWorld = Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i + 1], 0.5f);
                }

                float t = s / (float)steps;
                float taper = MathF.Pow(1f - t, 1.35f);

                Vector2 pos = posWorld + half - Main.screenPosition;

                Main.spriteBatch.Draw(glow, pos, null, new Color(90, 170, 255) * (taper * 0.26f), 0f, glowOrigin, 0.8f * taper + 0.05f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glow, pos, null, new Color(220, 245, 255) * (taper * taper * 0.28f), 0f, glowOrigin, 0.4f * taper + 0.03f, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawHead()
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            float pulse = 1f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 10f + WavePhase);

            Main.EntitySpriteDraw(texture, drawPos, null, new Color(120, 200, 255, 0) * 0.5f, Projectile.rotation, origin, 1.6f * pulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, new Color(255, 255, 255, 40), Projectile.rotation, origin, pulse, SpriteEffects.None, 0);
        }
    }
}
