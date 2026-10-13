using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaBallLightning : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        private const int SpawnTime = 90;
        private const int OrbitTime = 180;
        private const int PairGap = 120;
        private const float OrbitRadius = 300f;
        private const float OrbitSpeed = 0.028f;

        public ref float Slot => ref Projectile.ai[0];
        public ref float PairId => ref Projectile.ai[1];
        public ref float Total => ref Projectile.ai[2];
        public ref float Timer => ref Projectile.localAI[0];
        private Vector2 meetPoint;
        private Vector2 smoothCenter;
        private bool initialized;

        private static Texture2D blankTexture;
        private static readonly Color GlowBlue = new Color(80, 170, 255);

        private int HoldTicks => Math.Max(22, 70 - (int)PairId * 12);
        private float AttractAccel => 0.9f * (1f + PairId * 0.4f);
        private float AttractMaxSpeed => 15f + PairId * 3f;

        private int DetachTime => SpawnTime + OrbitTime + (int)PairId * PairGap;
        private int AttractTime => DetachTime + HoldTicks;
        private bool Detached => Timer >= DetachTime;
        private bool Attracting => Timer >= AttractTime;

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Attracting ? null : false;
        }

        private Projectile FindPartner()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && i != Projectile.whoAmI && p.type == Projectile.type
                    && (int)p.ai[1] == (int)PairId)
                    return p;
            }
            return null;
        }

        private void SpawnExplosion(Vector2 pos, int damage)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<CelestiaLightningExplosion>(), damage, 0f);
        }

        public override void AI()
        {
            Timer++;
            Player player = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];

            if (!initialized)
            {
                initialized = true;
                smoothCenter = player.Center;
                Projectile.Center = player.Center;
                Projectile.scale = 0f;
            }

            smoothCenter = Vector2.Lerp(smoothCenter, player.Center, 0.08f);

            if (!Detached)
            {
                float spawnProg = MathHelper.Clamp((Timer - Slot * 6f) / 75f, 0f, 1f);
                float ease = 1f - (float)Math.Pow(1f - spawnProg, 3.0);

                float spiral = (1f - ease) * MathHelper.Pi * 2.5f;
                float radius = OrbitRadius * ease;
                float ang = MathHelper.TwoPi / Math.Max(Total, 1f) * Slot + Timer * OrbitSpeed - spiral;

                Projectile.Center = smoothCenter + ang.ToRotationVector2() * radius;
                Projectile.velocity = Vector2.Zero;
                Projectile.scale = ease;

                if (Main.netMode != NetmodeID.Server && spawnProg > 0f && spawnProg < 1f && Main.rand.NextBool(2))
                {
                    Vector2 from = Projectile.Center + Main.rand.NextVector2CircularEdge(70f, 70f);
                    new Spark(from, (Projectile.Center - from).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 7f), GlowBlue, 14, 0.45f, 1f, 0f).Spawn();
                }
                if (Timer == (int)(Slot * 6f) + 1)
                    SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.4f, Volume = 0.6f }, player.Center);

                if (Timer == DetachTime - 1)
                {
                    SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.3f }, Projectile.Center);

                    if (Main.netMode != NetmodeID.Server)
                        for (int i = 0; i < 8; i++)
                            new Spark(Projectile.Center, Main.rand.NextVector2CircularEdge(5f, 5f), Color.White, 18, 0.5f, 1f, 0f).Spawn();
                }
            }
            else if (!Attracting)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.scale = 1f;

                Projectile partner = FindPartner();
                if (Main.netMode != NetmodeID.Server && partner != null && Main.rand.NextBool(3))
                {
                    Vector2 toPartner = (partner.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    new Spark(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), toPartner * Main.rand.NextFloat(3f, 5f), GlowBlue, 16, 0.4f, 1f, 0f).Spawn();
                }

                if (Timer == AttractTime - 1)
                    SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.5f, Volume = 0.8f }, Projectile.Center);
            }
            else
            {
                if (Timer == AttractTime)
                {
                    meetPoint = player.Center;
                    SoundEngine.PlaySound(SoundID.Item94 with { Pitch = -0.2f }, Projectile.Center);
                }

                Projectile partner = FindPartner();
                if (partner == null)
                {
                    SpawnExplosion(Projectile.Center, Projectile.damage);
                    Projectile.Kill();
                    return;
                }

                float distPartner = Vector2.Distance(Projectile.Center, partner.Center);
                if (distPartner < 52f)
                {
                    Vector2 mid = (Projectile.Center + partner.Center) * 0.5f;
                    SpawnExplosion(mid, (int)(Projectile.damage * 1.4f));

                    partner.Kill();
                    Projectile.Kill();

                    return;
                }

                Vector2 target = Vector2.Distance(Projectile.Center, meetPoint) > 90f ? meetPoint : partner.Center;
                Vector2 steer = (target - Projectile.Center).SafeNormalize(Vector2.Zero);

                Projectile.velocity = Projectile.velocity * 0.95f + steer * AttractAccel;

                if (Projectile.velocity.Length() > AttractMaxSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * AttractMaxSpeed;

                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, partner.Center, Main.rand.NextFloat()), DustID.Clentaminator_Cyan, Main.rand.NextVector2Circular(2f, 2f));
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.6f, 1.2f);
                }
            }

            Lighting.AddLight(Projectile.Center, 0.35f * Projectile.scale, 0.7f * Projectile.scale, 1.1f * Projectile.scale);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D blankTexture = ModContent.Request<Texture2D>("Jarkos/textures/blankTexture", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (Projectile.scale <= 0.01f) return false;

            float charge = 0f;

            if (Detached && !Attracting)
                charge = MathHelper.Clamp((Timer - DetachTime) / HoldTicks, 0f, 1f);

            else if (Attracting)
                charge = 1f;

            Effect shader = ModContent.Request<Effect>("jarkos/Effects/CelestiaBallLightning", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["uSeed"]?.SetValue(Projectile.whoAmI * 7.31f);
            shader.Parameters["uOpacity"]?.SetValue(Projectile.scale);
            shader.Parameters["uCharge"]?.SetValue(charge);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader, Main.GameViewMatrix.ZoomMatrix);

            float size = (200f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f + Projectile.whoAmI) * 8f + charge * 26f) * Projectile.scale;
            Main.spriteBatch.Draw(blankTexture, Projectile.Center - Main.screenPosition, null, Color.White, 0f, new Vector2(0.5f), new Vector2(size), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            DrawOuterArcs(Main.spriteBatch, charge);
            DrawPairArc(Main.spriteBatch);

            Texture2D bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, GlowBlue * ((0.55f + charge * 0.25f) * Projectile.scale), 0f, bloom.Size() * 0.5f, (0.5f + charge * 0.15f) * Projectile.scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }

        private void DrawOuterArcs(SpriteBatch sb, float charge)
        {
            const int arcLife = 16;

            int cycle = (int)(Main.GameUpdateCount / arcLife);
            float frac = Main.GameUpdateCount % arcLife / (float)arcLife;
            float fade = (float)Math.Sin(frac * Math.PI);

            int arcCount = 3 + (int)(charge * 2f) + (Attracting ? 1 : 0);

            for (int i = 0; i < arcCount; i++)
            {
                var rand = new Random(Projectile.whoAmI * 131 + i * 977 + cycle * 53);
                if (rand.NextDouble() < 0.3) continue;

                float ang = (float)(rand.NextDouble() * MathHelper.TwoPi);
                float len = (70f + (float)rand.NextDouble() * 70f + charge * 40f) * Projectile.scale;

                Vector2 start = Projectile.Center + ang.ToRotationVector2() * 30f * Projectile.scale;
                Vector2 end = start + ang.ToRotationVector2() * len;

                DrawLightningBolt(sb, start, end, rand, (0.7f + charge * 0.4f) * fade * Projectile.scale);
            }
        }

        private void DrawPairArc(SpriteBatch sb)
        {
            if (!Detached) return;

            Projectile partner = FindPartner();
            if (partner == null || Projectile.whoAmI > partner.whoAmI) return;

            const int arcLife = 12;
            int cycle = (int)(Main.GameUpdateCount / arcLife);
            float frac = Main.GameUpdateCount % arcLife / (float)arcLife;
            float fade = (float)Math.Sin(frac * Math.PI);

            float intensity = (Attracting ? 1f : 0.35f) * fade;
            var rand = new Random(Projectile.whoAmI * 419 + cycle * 71);

            if (!Attracting && rand.NextDouble() < 0.4) return;

            DrawLightningBolt(sb, Projectile.Center, partner.Center, rand, intensity);
        }
        private static void DrawLightningBolt(SpriteBatch sb, Vector2 start, Vector2 end, Random rand, float intensity)
        {
            const int segs = 10;

            Vector2 dir = end - start;
            Vector2 perp = dir.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            Vector2 prev = start;

            for (int i = 1; i <= segs; i++)
            {
                float t = i / (float)segs;
                float amp = (1f - Math.Abs(t - 0.5f) * 2f) * dir.Length() * 0.12f;

                Vector2 point = start + dir * t + perp * (float)(rand.NextDouble() * 2.0 - 1.0) * amp;
                if (i == segs) point = end;

                DrawLine(sb, prev, point, GlowBlue * (0.45f * intensity), 8f);
                DrawLine(sb, prev, point, GlowBlue * (0.6f * intensity), 3.5f);
                DrawLine(sb, prev, point, Color.White * (0.95f * intensity), 1.6f);
                prev = point;
            }
        }

        private static void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, Color color, float width)
        {
            Vector2 d = b - a;
            sb.Draw(blankTexture, a - Main.screenPosition, null, color, d.ToRotation(), new Vector2(0f, 0.5f), new Vector2(d.Length(), width), SpriteEffects.None, 0f);
        }
    }
}
