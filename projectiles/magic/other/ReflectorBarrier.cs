using System;
using System.Collections.Generic;
using jarkos.items.weapons.magic.other;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.other
{
    public class ReflectorBarrier : ModProjectile
    {
        public const int DEPLOY = 28;
        public const int HOLD = 70;
        public const int CONVERGE = 58;
        public const int LIFE = DEPLOY + HOLD + CONVERGE;
        public const float HALF = 140f;
        public const float BEAM_HIT_HALF = 10f;
        public const int MAX_CHARGE = 40;
        public const float BLAST_RADIUS = 19f * 16f;

        public override string Texture => "jarkos/projectiles/magic/other/BeamStrip";

        private static Asset<Effect> beamShader;
        private static Asset<Effect> fieldShader;
        private static Asset<Effect> spawnShader;

        private static bool shaderLoadAttempted;
        private const int SIGIL_FADE = 26;

        private readonly HashSet<int> containedNPCs = new();
        private int reflectSoundCooldown;
        private int Age => LIFE - Projectile.timeLeft;
        
        private int Charge { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }

        private bool Deploying => Age < DEPLOY;
        private bool Converging => Age >= DEPLOY + HOLD;
        private float CollapseT => Converging ? (Age - DEPLOY - HOLD) / (float)CONVERGE : 0f;
        
        public float CurrentHalf
        {
            get
            {
                if (Deploying)
                    return HALF * MathSystem.EaseOutBack(Age / (float)DEPLOY);

                if (Converging)
                    return HALF * Math.Max(0.02f, 1f - MathSystem.EaseInCubic(CollapseT));

                return HALF * (1f + 0.015f * (float)Math.Sin(Age * 0.07f));
            }
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = LIFE;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.netImportant = true;
        }
        private Vector2 ToLocal(Vector2 world) => (world - Projectile.Center).RotatedBy(-Projectile.rotation);
        private Vector2 ToWorld(Vector2 local) => Projectile.Center + local.RotatedBy(Projectile.rotation);
        public Vector2[] GetCorners()
        {
            float h = CurrentHalf;
            return new[]
            {
                ToWorld(new Vector2(-h, -h)), ToWorld(new Vector2(h, -h)), ToWorld(new Vector2(h, h)), ToWorld(new Vector2(-h, h)), };
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(-0.12f, 0.12f);
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ReflectorStartOne") with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);
        }
        public override void AI()
        {
            if (reflectSoundCooldown > 0)
                reflectSoundCooldown--;

            UpdateRotation();
            EmitAmbient();
            LightUp();

            if (!Deploying)
            {
                ContainNPCs();
                ReflectProjectiles();
            }

            if (Age == DEPLOY)
            {
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ReflectorStartTwo") with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);

                if (Main.netMode != NetmodeID.Server)
                {
                    Vector2[] cs = GetCorners();
                    for (int i = 0; i < 4; i++)
                    {
                        new Circleedge_tight(cs[i], Vector2.Zero, CrystalReflector.ColorWhite, 14, 0.5f, 0.9f).Spawn();
                        new StarParticleBigger(cs[i], Vector2.Zero, CrystalReflector.ColorWhite, 16, 0.4f, 1f, 0.04f).Spawn();

                        Vector2 next = cs[(i + 1) % 4];
                        Vector2 along = (next - cs[i]).SafeNormalize(Vector2.Zero);

                        for (int k = 0; k < 4; k++)
                        {
                            new Spark(cs[i] + along * (8f + k * 6f), along * Main.rand.NextFloat(5f, 9f), CrystalReflector.ColorIce, 16, Main.rand.NextFloat(0.3f, 0.45f), 0.9f).Spawn();
                            new Spark(next - along * (8f + k * 6f), -along * Main.rand.NextFloat(5f, 9f), CrystalReflector.ColorIce, 16, Main.rand.NextFloat(0.3f, 0.45f), 0.9f).Spawn();
                        }
                    }

                    new Circleedge_novaburst(Projectile.Center, Vector2.Zero, CrystalReflector.ColorViolet, 22, 1.1f, 0.85f).Spawn();
                }
            }
            if (Age == DEPLOY + HOLD)
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ReflectorExplode") with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);

            if (Projectile.timeLeft == 1 && Main.myPlayer == Projectile.owner)
            {
                int blastDamage = (int)(Projectile.damage * (5f + 0.12f * Charge));

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ReflectorBlast>(), blastDamage, 9f, Projectile.owner, BLAST_RADIUS, Charge / (float)MAX_CHARGE);
            }
        }
        private void UpdateRotation()
        {
            if (Converging)
            {
                Projectile.rotation += 0.004f + MathSystem.EaseInCubic(CollapseT) * 0.22f;
            }

            else if (!Deploying)
            {
                Projectile.rotation += 0.004f + 0.002f * (float)Math.Sin(Age * 0.03f);
            }
        }
        private void LightUp()
        {
            Vector2[] c = GetCorners();

            for (int i = 0; i < 4; i++)
            {
                Lighting.AddLight(c[i], 0.35f, 0.55f, 0.9f);
                Lighting.AddLight(Vector2.Lerp(c[i], c[(i + 1) % 4], 0.33f), 0.18f, 0.3f, 0.5f);
                Lighting.AddLight(Vector2.Lerp(c[i], c[(i + 1) % 4], 0.66f), 0.18f, 0.3f, 0.5f);
            }
        }
        private void ContainNPCs()
        {
            float h = CurrentHalf;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                if (npc.boss || npc.realLife >= 0)
                    continue;

                Vector2 local = ToLocal(npc.Center);

                float margin = Math.Min(Math.Max(npc.width, npc.height) * 0.5f, h * 0.5f);
                float limit = h - 6f - margin;

                if (limit <= 8f)
                    limit = 8f;

                bool inside = Math.Abs(local.X) <= limit && Math.Abs(local.Y) <= limit;

                if (inside)
                {
                    containedNPCs.Add(i);

                    if (Converging && CollapseT > 0.35f)
                        npc.velocity += (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero) * 0.25f;
                    continue;
                }

                if (!containedNPCs.Contains(i))
                    continue;

                Vector2 clamped = new Vector2(MathHelper.Clamp(local.X, -limit, limit), MathHelper.Clamp(local.Y, -limit, limit));
                Vector2 localVel = npc.velocity.RotatedBy(-Projectile.rotation);

                if (Math.Abs(local.X) > limit)
                    localVel.X = 0f;

                if (Math.Abs(local.Y) > limit)
                    localVel.Y = 0f;

                npc.Center = ToWorld(clamped);
                npc.velocity = localVel.RotatedBy(Projectile.rotation);

                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
                {
                    Vector2 hit = ToWorld(clamped);

                    for (int k = 0; k < 3; k++)
                    new Spark(hit, Main.rand.NextVector2Circular(2.5f, 2.5f), CrystalReflector.ColorIce, 16, Main.rand.NextFloat(0.25f, 0.45f), 0.85f).Spawn();
                }
            }
            containedNPCs.RemoveWhere(idx => !Main.npc[idx].active);
        }
        private void ReflectProjectiles()
        {
            float h = CurrentHalf - 4f;

            if (h < 16f)
                return;

            int barrierType = Projectile.type;
            int blastType = ModContent.ProjectileType<ReflectorBlast>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                if (!p.active || p.whoAmI == Projectile.whoAmI)
                    continue;
                if (p.type == barrierType || p.type == blastType)
                    continue;
                if (p.minion || p.sentry || p.aiStyle == ProjAIStyleID.Spear || p.aiStyle == ProjAIStyleID.Yoyo)
                    continue;
                if (p.velocity.LengthSquared() < 0.3f)
                    continue;

                Vector2 local = ToLocal(p.Center);

                if (Math.Abs(local.X) > h || Math.Abs(local.Y) > h)
                    continue;

                Vector2 localVel = p.velocity.RotatedBy(-Projectile.rotation);
                Vector2 next = local + localVel;
                bool bounced = false;

                if (Math.Abs(next.X) > h && Math.Sign(localVel.X) == Math.Sign(next.X))
                {
                    localVel.X = -localVel.X;
                    bounced = true;
                }

                if (Math.Abs(next.Y) > h && Math.Sign(localVel.Y) == Math.Sign(next.Y))
                {
                    localVel.Y = -localVel.Y;
                    bounced = true;
                }

                if (!bounced)
                    continue;

                p.velocity = localVel.RotatedBy(Projectile.rotation) * 0.98f;
                p.Center = ToWorld(new Vector2(MathHelper.Clamp(local.X, -h, h), MathHelper.Clamp(local.Y, -h, h)));
                p.netUpdate = true;

                if (Charge < MAX_CHARGE)
                {
                    Charge++;
                    Projectile.netUpdate = true;
                }

                OnReflectEffects(p.Center, p.velocity);
            }
        }
        private void OnReflectEffects(Vector2 pos, Vector2 newVel)
        {
            if (reflectSoundCooldown <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = Main.rand.NextFloat(0.25f, 0.6f), Volume = 0.45f }, pos);
                reflectSoundCooldown = 5;
            }

            if (Main.netMode == NetmodeID.Server)
                return;

            new StarParticle(pos, Vector2.Zero, CrystalReflector.ColorWhite, 18, Main.rand.NextFloat(0.2f, 0.3f), 0.95f).Spawn();
            new Circleedge_tight(pos, Vector2.Zero, CrystalReflector.ColorIce, 12, 0.3f, 0.8f).Spawn();

            for (int k = 0; k < 4; k++)
            new Spark(pos, newVel.SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(1.5f, 3.5f), CrystalReflector.ColorIce, 16, Main.rand.NextFloat(0.25f, 0.45f), 0.9f).Spawn();
        }
        private void EmitAmbient()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Vector2[] c = GetCorners();

            if (Deploying)
            {
                foreach (Vector2 corner in c)
                {
                    if (Main.rand.NextBool(2))
                        new Stretching_particle(Projectile.Center, corner, CrystalReflector.ColorIce, 10, Main.rand.NextFloat(20f, 40f), Main.rand.NextFloat(3f, 5f), 0.02f).Spawn();

                    new StarParticle(corner, Vector2.Zero, CrystalReflector.ColorWhite, 12, Main.rand.NextFloat(0.18f, 0.3f), 0.95f).Spawn();
                    
                    Vector2 back = (Projectile.Center - corner).SafeNormalize(Vector2.Zero);
                    new Spark(corner, back * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.8f, 0.8f), Main.rand.NextBool(3) ? CrystalReflector.ColorViolet : CrystalReflector.ColorIce, 18, Main.rand.NextFloat(0.3f, 0.5f), 0.9f).Spawn();

                    if (Main.rand.NextBool(3))
                        new CrystalShardParticle(corner, back * Main.rand.NextFloat(0.5f, 1.5f), CrystalReflector.ColorIce, 24, Main.rand.NextFloat(0.5f, 0.8f), 0.9f, Main.rand.NextFloat(-0.15f, 0.15f)).Spawn();
                }
                return;
            }

            if (Converging)
            {
                float t = MathSystem.EaseInCubic(CollapseT);
                
                foreach (Vector2 corner in c)
                {
                    Vector2 toCenter = (Projectile.Center - corner).SafeNormalize(Vector2.Zero);
                    new Stretching_particle_out(corner, toCenter * (2f + 8f * t), CrystalReflector.ColorIce, 14, Main.rand.NextFloat(25f, 50f) * (0.5f + t), Main.rand.NextFloat(4f, 6f), 0.02f).Spawn();

                    if (Main.rand.NextBool(2))
                        new CrystalShardParticle(corner, toCenter * Main.rand.NextFloat(2f, 5f), CrystalReflector.ColorWhite, 18, Main.rand.NextFloat(0.5f, 0.9f), 0.95f, Main.rand.NextFloat(-0.3f, 0.3f)).Spawn();
                }

                if (Age % 9 == 0)
                    new Circleedge_reducing(Projectile.Center, Vector2.Zero, CrystalReflector.ColorViolet, 16, 0.8f + t, 0.85f).Spawn();

                if (CollapseT > 0.7f && Main.rand.NextBool(2))
                    new Circular_glow(Projectile.Center, Vector2.Zero, CrystalReflector.ColorWhite, 10, 0.4f + t * 0.6f, 0.7f).Spawn();

                return;
            }

            if (Main.rand.NextBool(3))
            {
                int e = Main.rand.Next(4);
                Vector2 a = c[e], b = c[(e + 1) % 4];
                Vector2 pos = Vector2.Lerp(a, b, Main.rand.NextFloat());

                new Spark(pos, (b - a).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f) * (Main.rand.NextBool() ? 1f : -1f), CrystalReflector.ColorIce, 18, Main.rand.NextFloat(0.25f, 0.4f), 0.8f).Spawn();
            }

            if (Main.rand.NextBool(5))
            {
                int e = Main.rand.Next(4);
                Vector2 pos = Vector2.Lerp(c[e], c[(e + 1) % 4], Main.rand.NextFloat());

                new StarParticle(pos, Vector2.Zero, CrystalReflector.ColorWhite, 20, Main.rand.NextFloat(0.12f, 0.2f), 0.85f).Spawn();
            }

            if (Main.rand.NextBool(8))
            {
                Vector2 corner = c[Main.rand.Next(4)];

                new CrystalShardParticle(corner + Main.rand.NextVector2Circular(6f, 6f), new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(0.2f, 0.8f)), CrystalReflector.ColorIce, 36, Main.rand.NextFloat(0.4f, 0.7f), 0.85f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Deploying)
                return false;

            Vector2[] c = GetCorners();
            Vector2 rectCenter = targetHitbox.Center.ToVector2();
            Rectangle expanded = targetHitbox;

            expanded.Inflate((int)BEAM_HIT_HALF, (int)BEAM_HIT_HALF);

            for (int i = 0; i < 4; i++)
            {
                Vector2 a = c[i], b = c[(i + 1) % 4];
                Vector2 ab = b - a;

                float t = MathHelper.Clamp(Vector2.Dot(rectCenter - a, ab) / ab.LengthSquared(), 0f, 1f);
                Vector2 closest = a + ab * t;

                if (expanded.Contains(closest.ToPoint()))
                    return true;
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.Center.X >= Projectile.Center.X ? -1 : 1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            for (int k = 0; k < 4; k++)
                new Spark(target.Center, Main.rand.NextVector2Circular(3f, 3f), CrystalReflector.ColorIce, 16, Main.rand.NextFloat(0.3f, 0.5f), 0.9f).Spawn();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!shaderLoadAttempted)
            {
                shaderLoadAttempted = true;
                try
                {
                    beamShader = ModContent.Request<Effect>("jarkos/Shaders/PrismBeam", AssetRequestMode.ImmediateLoad);
                }
                catch { beamShader = null; }
                try
                {
                    fieldShader = ModContent.Request<Effect>("jarkos/Shaders/PrismField", AssetRequestMode.ImmediateLoad);
                }
                catch { fieldShader = null; }
                try
                {
                    spawnShader = ModContent.Request<Effect>("jarkos/Shaders/PrismSpawn", AssetRequestMode.ImmediateLoad);
                }
                catch { spawnShader = null; }
            }

            Vector2[] corners = GetCorners();

            float h = CurrentHalf;
            float charge01 = Charge / (float)MAX_CHARGE;
            float collapse = MathSystem.EaseInCubic(CollapseT);
            float intensity = Deploying ? Age / (float)DEPLOY : 1f;

            intensity *= 0.85f + 0.3f * charge01 + 0.6f * collapse;


            DrawBeams(corners, charge01, collapse, intensity);
            DrawCorners(corners, collapse);

            return false;
        }
        private void DrawSpawnSigil()
        {
            if (Age >= DEPLOY + SIGIL_FADE)
                return;

            if (spawnShader == null || spawnShader.Value == null)
                return;

            float progress = Math.Min(1f, Age / (float)DEPLOY);
            float fade = Age < DEPLOY ? 1f : 1f - (Age - DEPLOY) / (float)SIGIL_FADE;

            Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/textures/CrystalNoise", AssetRequestMode.ImmediateLoad).Value;
            
            float scale = HALF * 2.6f / noiseTex.Width;

            Effect fx = spawnShader.Value;
            fx.Parameters["uProgress"]?.SetValue(progress);
            fx.Parameters["uFade"]?.SetValue(fade);
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uColor"]?.SetValue(CrystalReflector.ColorIce.ToVector3());
            fx.Parameters["uSecondaryColor"]?.SetValue(CrystalReflector.ColorViolet.ToVector3());
            fx.Parameters["uNoiseTexture"]?.SetValue(noiseTex);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(noiseTex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, noiseTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawBeams(Vector2[] corners, float charge01, float collapse, float intensity)
        {
            Texture2D strip = ModContent.Request<Texture2D>("jarkos/projectiles/magic/other/BeamStrip", AssetRequestMode.ImmediateLoad).Value;
            Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/textures/CrystalNoise", AssetRequestMode.ImmediateLoad).Value;

            bool useShader = beamShader != null && beamShader.Value != null;
            Effect fx = useShader ? beamShader.Value : null;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);

            if (useShader)
            {
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                fx.Parameters["uCharge"]?.SetValue(charge01);
                fx.Parameters["uIntensity"]?.SetValue(intensity);
                fx.Parameters["uColor"]?.SetValue(CrystalReflector.ColorIce.ToVector3());
                fx.Parameters["uSecondaryColor"]?.SetValue(CrystalReflector.ColorViolet.ToVector3());
                fx.Parameters["uNoiseTexture"]?.SetValue(noiseTex);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 a = corners[i], b = corners[(i + 1) % 4];
                Vector2 ab = b - a;

                float len = ab.Length();

                if (len < 2f)
                    continue;

                float rot = ab.ToRotation();
                float thickness = 30f * (0.9f + 0.2f * charge01 + 0.5f * collapse);

                if (useShader)
                {
                    fx.Parameters["uLength"]?.SetValue(len);
                    fx.Parameters["uPhase"]?.SetValue(i * 1.7f);
                }

                Vector2 scale = new Vector2(len / strip.Width, thickness / strip.Height);
                Vector2 origin = new Vector2(0f, strip.Height / 2f);

                if (useShader)
                {
                    Main.spriteBatch.Draw(strip, a - Main.screenPosition, null, Color.White, rot, origin, scale, SpriteEffects.None, 0f);
                }
                else
                {
                    float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f + i * 1.7f);

                    Main.spriteBatch.Draw(strip, a - Main.screenPosition, null, CrystalReflector.ColorDeep * (0.55f * intensity * pulse), rot, origin, scale * new Vector2(1f, 1.6f), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(strip, a - Main.screenPosition, null, CrystalReflector.ColorIce * (0.9f * intensity * pulse), rot, origin, scale * new Vector2(1f, 0.7f), SpriteEffects.None, 0f);
                }
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawCorners(Vector2[] corners, float collapse)
        {
            Texture2D orb = ModContent.Request<Texture2D>("jarkos/projectiles/magic/other/FrostGlow", AssetRequestMode.ImmediateLoad).Value;
            Texture2D glow = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;

            float gt = Main.GlobalTimeWrappedHourly;
            float deployPop = Deploying ? MathSystem.EaseOutBack(Math.Min(1f, Age / (float)DEPLOY * 1.15f)) : 1f;

            Color ice = CrystalReflector.ColorIce;
            Color violet = CrystalReflector.ColorViolet;
            Color iceAdd = new Color(ice.R, ice.G, ice.B, 0);
            Color violetAdd = new Color(violet.R, violet.G, violet.B, 0);

            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = corners[i] - Main.screenPosition;
                float pulse = 1f + 0.1f * (float)Math.Sin(gt * 4.2f + i * 1.7f);
                float scale = (1.05f + 0.5f * collapse) * pulse * deployPop;

                if (Deploying && Age > 2)
                {
                    Vector2 dir = (corners[i] - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float dist = (corners[i] - Projectile.Center).Length();

                    for (int g = 1; g <= 3; g++)
                    {
                        Vector2 ghost = pos - dir * (dist * 0.12f * g);
                        Main.spriteBatch.Draw(orb, ghost, null, iceAdd * (0.4f / g), 0f, orb.Size() * 0.5f, scale * (1f - 0.15f * g), SpriteEffects.None, 0f);
                    }
                }

                Main.spriteBatch.Draw(orb, pos, null, Color.Lerp(iceAdd, violetAdd, collapse * 0.7f) * (0.55f + 0.35f * collapse), 0f, orb.Size() * 0.5f, scale * 2.1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(orb, pos, null, iceAdd * 0.95f, 0f, orb.Size() * 0.5f, scale * 1.25f, SpriteEffects.None, 0f);

                float flareRot = gt * (0.8f + collapse * 12f) + i * MathHelper.PiOver4;
                float flareLen = scale * (2.6f + 1.2f * collapse);
                Color flareC = new Color(255, 255, 255, 0) * (0.5f + 0.3f * collapse);

                Main.spriteBatch.Draw(orb, pos, null, flareC, flareRot, orb.Size() * 0.5f, new Vector2(flareLen, scale * 0.22f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(orb, pos, null, flareC, flareRot + MathHelper.PiOver2, orb.Size() * 0.5f, new Vector2(flareLen, scale * 0.22f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(orb, pos, null, new Color(255, 255, 255, 0), 0f, orb.Size() * 0.5f, scale * 0.62f, SpriteEffects.None, 0f);

                for (int o = 0; o < 2; o++)
                {
                    float ang = gt * 3.1f * (o == 0 ? 1f : -1f) + i * 2.4f + o * MathHelper.Pi;
                    Vector2 op = pos + ang.ToRotationVector2() * (13f + 3f * pulse) * deployPop;

                    Main.spriteBatch.Draw(orb, op, null, iceAdd * 0.8f, 0f, orb.Size() * 0.5f, 0.22f, SpriteEffects.None, 0f);
                }
            }
            
            if (collapse > 0.55f)
            {
                float t = (collapse - 0.55f) / 0.45f;
                Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * (t * 0.9f), 0f, glow.Size() * 0.5f, 0.2f + t * 1.1f, SpriteEffects.None, 0f);
            }
        }
    }
}
