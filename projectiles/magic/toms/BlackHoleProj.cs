using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using jarkos.particles;
using ReLogic.Content;
using jarkos.items.weapons.magic.toms;
using Terraria.Graphics.CameraModifiers;
namespace jarkos.projectiles.magic.toms
{
	public class BlackHoleProj : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/magic/toms/BlackHole";
        private const int SPAWN = 66;
        private const int FEED_MAX = 60 * 9;
        private const int COLLAPSE = 55;
        private const int BOOM = 78;
        private const float BASE_R = 98f;
        private float Phase { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private float Timer { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        private float Growth { get => Projectile.ai[1]; set => Projectile.ai[1] = MathHelper.Clamp(value, 0f, 1f); }
        private Player Owner => Main.player[Projectile.owner];
        private float Spawn01 => Phase > 0 ? 1f : MathHelper.Clamp(Timer / SPAWN, 0f, 1f);
        private float Collapse01 => Phase == 2 ? MathHelper.Clamp(Timer / COLLAPSE, 0f, 1f) : (Phase == 3 ? 1f : 0f);
        private float Boom01 => Phase == 3 ? MathHelper.Clamp(Timer / BOOM, 0f, 1f) : 0f;
        private float VisR
        {
            get
            {
                float t = Spawn01;
                float back = 1f + 2.7f * (t - 1f) * (t - 1f) * (t - 1f) + 1.7f * (t - 1f) * (t - 1f);
                float r = BASE_R * (1f + 0.85f * Growth) * back;
                r *= 1f - 0.28f * Collapse01;
                return Math.Max(r, 1f);
            }
        }
        private float VisRBase => BASE_R * (1f + 0.85f * Growth);
        private float SuckR => 360f * (1f + 0.7f * Growth) * (Phase == 2 ? 2.1f : 1f);
        private float EatR => VisR * 0.42f;
        private static Asset<Effect> diskShader;
        private static Asset<Effect> burstShader;
        private static bool shaderLoadAttempted;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = SPAWN + FEED_MAX + COLLAPSE + BOOM + 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 11;
            Projectile.netImportant = true;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float r = VisR * 0.62f;
            Vector2 c = Projectile.Center;
            Vector2 nearest = new Vector2(MathHelper.Clamp(c.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(c.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.DistanceSquared(c, nearest) <= r * r;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= 1f + 1.4f * Growth + 0.8f * Collapse01;
            modifiers.HitDirectionOverride = Math.Sign(Projectile.Center.X - target.Center.X);
        }
        public override void AI()
        {
            if (Phase < 3)
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, COLLAPSE + BOOM + 10);
            Timer++;
            switch ((int)Phase)
            {
                case 0: AISpawn(); break;
                case 1: AIFeed(); break;
                case 2: AICollapse(); break;
                case 3: AIBoom(); break;
            }
            Color glow = Color.Lerp(BlackHole.ColorMid, Color.White, Collapse01);
            float intensity = (0.8f + 1.2f * Growth + 1.5f * Collapse01) * Spawn01;
            Lighting.AddLight(Projectile.Center, glow.ToVector3() * intensity);
            UpdateScreenShader();
        }
        private void AISpawn()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                int invertTick = (int)(SPAWN * 0.45f);
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f, Volume = 0.9f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.5f, Volume = 0.6f }, Projectile.Center);
                    new Circular_glow(Projectile.Center, Vector2.Zero, Color.White, 14, 0.9f, 0.9f).Spawn();
                }
                if (Timer < invertTick)
                {
                    float k = Timer / (float)invertTick;
                    if (Timer % 10 == 0)
                    {
                        float ringR = VisRBase * (1.9f - 1.1f * k);
                        int count = 10;
                        for (int i = 0; i < count; i++)
                        {
                            float a = MathHelper.TwoPi * i / count + Main.rand.NextFloat(0.3f);
                            Vector2 at = Projectile.Center + a.ToRotationVector2() * ringR;
                            Vector2 vel = (Projectile.Center - at).SafeNormalize(Vector2.Zero)
                                * (3.5f + 5.5f * k);
                            new Spark(at, vel, Main.rand.NextBool(3) ? BlackHole.ColorMid : Color.White, 22, Main.rand.NextFloat(0.25f, 0.45f), 0.85f).Spawn();
                        }
                    }
                    if (Timer % 3 == 0)
                    {
                        Vector2 dir = Main.rand.NextVector2Unit();
                        Vector2 from = Projectile.Center + dir * Main.rand.NextFloat(160f, 340f);
                        new VoidShardParticle(from, -dir * Main.rand.NextFloat(4f, 8f) * (0.5f + k), Color.Lerp(Color.White, BlackHole.ColorHot, Main.rand.NextFloat()), 26, Main.rand.NextFloat(0.5f, 0.9f), 0.85f).Spawn();
                    }
                    if (Timer % Math.Max(2, 8 - (int)(k * 6f)) == 0)
                        new StarParticleBigger(Projectile.Center, Main.rand.NextVector2Circular(0.6f, 0.6f), Color.White, 10, Main.rand.NextFloat(0.18f, 0.3f) * (0.7f + k), 0.95f, 0.04f).Spawn();
                }
                if (Timer == invertTick)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.7f, Volume = 1.2f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Item92 with { Pitch = -0.8f, Volume = 0.9f }, Projectile.Center);
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 12f, 7f, 24, 1800f, "jarkosBlackHoleSpawn"));
                    new Circleedge_reducing(Projectile.Center, Vector2.Zero, Color.White, 22, 1.8f, 0.95f).Spawn();
                    new Circular_glow(Projectile.Center, Vector2.Zero, BlackHole.ColorMid, 18, 1.3f, 0.85f).Spawn();
                    for (int i = 0; i < 14; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 9f);
                        new Stretching_particle_out(Projectile.Center, vel, i % 3 == 0 ? BlackHole.ColorHot : Color.White, 14, Main.rand.NextFloat(22f, 40f), Main.rand.NextFloat(2.2f, 3.6f), 0.02f).Spawn();
                    }
                }
                if (Timer > invertTick && Timer % 2 == 0)
                    new AccretionStreakParticle(Projectile, Main.rand.NextFloat(VisR * 0.9f, VisR * 1.6f), VisR * 0.3f, Color.Lerp(BlackHole.ColorHot, BlackHole.ColorMid, Main.rand.NextFloat()), 70, Main.rand.NextFloat(0.5f, 0.9f), 0.6f).Spawn();
            }
            if (Timer >= SPAWN)
            {
                Phase = 1;
                Timer = 0;
                if (Main.netMode != NetmodeID.Server)
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 0.7f }, Projectile.Center);
            }
        }
        private void AIFeed()
        {
            Player owner = Owner;
            if (owner.active && !owner.dead)
            {
                owner.itemTime = 2;
                owner.itemAnimation = 2;
                owner.ChangeDir(Projectile.Center.X > owner.Center.X ? 1 : -1);
            }
            bool manaOk = true;
            if (Timer % 12 == 0 && Main.myPlayer == Projectile.owner)
            {
                manaOk = owner.CheckMana(6, true);
                owner.manaRegenDelay = Math.Max(owner.manaRegenDelay, 30);
            }
            if (Main.myPlayer == Projectile.owner
                && (!owner.channel || !manaOk || Timer >= FEED_MAX || owner.dead || !owner.active))
            {
                Phase = 2;
                Timer = 0;
                Projectile.netUpdate = true;
                if (Main.netMode != NetmodeID.Server)
                    SoundEngine.PlaySound(SoundID.Item103 with { Pitch = 0.25f, Volume = 1f }, Projectile.Center);
                return;
            }
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 want = (Main.MouseWorld - Projectile.Center) * 0.045f;
                if (want.Length() > 7.5f)
                    want = want.SafeNormalize(Vector2.Zero) * 7.5f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, want, 0.12f);
                if (Timer % 8 == 0)
                    Projectile.netUpdate = true;
            }
            SuckNPCs(false);
            EmitAmbient(1f);
        }
        private void AICollapse()
        {
            Projectile.velocity *= 0.9f;
            SuckNPCs(true);
            EmitAmbient(2.6f);
            if ((int)Timer == COLLAPSE / 2 && Main.netMode != NetmodeID.Server)
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.4f, Volume = 1.1f }, Projectile.Center);
            if (Timer >= COLLAPSE)
            {
                Phase = 3;
                Timer = 0;
                Projectile.netUpdate = true;
                Detonate();
            }
        }
        private void Detonate()
        {
            float boomR = 330f * (1f + 0.8f * Growth);
            if (Main.myPlayer == Projectile.owner)
            {
                int dmg = (int)(Projectile.damage * 4.5f * (1f + 1.3f * Growth));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlackHoleBoom>(), dmg, 11f, Projectile.owner, boomR);
            }
            if (Main.netMode == NetmodeID.Server)
                return;
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.5f, Volume = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.1f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item92 with { Pitch = -0.6f, Volume = 0.9f }, Projectile.Center);
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 24f * (1f + 0.6f * Growth), 7f, 34, 2600f, "jarkosBlackHole"));
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.7f, Volume = 1f }, Projectile.Center);
            new Circleedge_reducing(Projectile.Center, Vector2.Zero, Color.White, 24, 2.2f, 0.95f).Spawn();
            new Circular_glow(Projectile.Center, Vector2.Zero, BlackHole.ColorHot, 18, 1.5f, 0.9f).Spawn();
            for (int i = 0; i < 26; i++)
            {
                Vector2 dir = Main.rand.NextVector2Unit();
                new Stretching_particle_out(Projectile.Center + dir * 14f, dir * Main.rand.NextFloat(10f, 22f), i % 3 == 0 ? Color.White : Color.Lerp(BlackHole.ColorHot, BlackHole.ColorMid, Main.rand.NextFloat()), 22, Main.rand.NextFloat(30f, 70f), Main.rand.NextFloat(3f, 6f), 0.02f).Spawn();
                new Spark(Projectile.Center + dir * 10f, dir * Main.rand.NextFloat(7f, 19f), Main.rand.NextBool(3) ? BlackHole.ColorEdge : BlackHole.ColorHot, 26, Main.rand.NextFloat(0.35f, 0.65f), 0.85f).Spawn();
            }
            for (int i = 0; i < 12; i++)
            {
                Vector2 dir = Main.rand.NextVector2Unit();
                new VoidShardParticle(Projectile.Center + dir * 20f, dir * Main.rand.NextFloat(5f, 13f), Color.White, 40, Main.rand.NextFloat(0.8f, 1.4f), 0.95f).Spawn();
            }
            for (int i = 0; i < 6; i++)
                new StarParticleBigger(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), Main.rand.NextVector2Circular(4f, 4f), Color.White, 24, Main.rand.NextFloat(0.3f, 0.5f), 0.95f, 0.05f).Spawn();
        }
        private void AIBoom()
        {
            Projectile.velocity = Vector2.Zero;
            if ((int)Timer == (int)(BOOM * 0.22f) && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item92 with { Pitch = -0.8f, Volume = 1f }, Projectile.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 13f * (1f + 0.5f * Growth), 6f, 24, 2200f, "jarkosBlackHole2"));
                new Circleedge_reducing(Projectile.Center, Vector2.Zero, BlackHole.ColorEdge, 28, 1.9f, 0.8f).Spawn();
                for (int i = 0; i < 16; i++)
                {
                    Vector2 dir = Main.rand.NextVector2Unit();
                    new Stretching_particle_out(Projectile.Center + dir * 30f, dir * Main.rand.NextFloat(6f, 14f), i % 2 == 0 ? BlackHole.ColorEdge : BlackHole.ColorMid, 26, Main.rand.NextFloat(25f, 55f), Main.rand.NextFloat(2.5f, 5f), 0.02f).Spawn();
                    new Spark(Projectile.Center + dir * 24f, dir * Main.rand.NextFloat(5f, 12f), BlackHole.ColorHot, 30, Main.rand.NextFloat(0.3f, 0.55f), 0.7f).Spawn();
                }
            }
            if (Timer >= BOOM)
                Projectile.Kill();
        }
        private void SuckNPCs(bool collapse)
        {
            float suckR = SuckR;
            float pullMul = collapse ? 3f : 1f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float d = Vector2.Distance(npc.Center, Projectile.Center);
                if (d > suckR)
                    continue;
                Vector2 dir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                if (!npc.boss && npc.knockBackResist > 0.01f)
                {
                    float k = 1f - d / suckR;
                    float pull = (0.45f + 9f * k * k) * pullMul * npc.knockBackResist;
                    npc.velocity = npc.velocity * 0.92f + dir * pull * 0.35f;
                    if (npc.velocity.Length() > 16f)
                        npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 16f;
                    npc.netUpdate = true;
                    if (d < EatR && Main.myPlayer == Projectile.owner)
                        EatNPC(npc);
                }
            }
        }
        private void EatNPC(NPC npc)
        {
            int bite = npc.lifeMax * 2 + npc.defense * 10 + 500;
            npc.SimpleStrikeNPC(bite, Math.Sign(Projectile.Center.X - npc.Center.X), false, 0f, DamageClass.Magic, true, Owner.luck);
            Growth += MathHelper.Clamp(npc.lifeMax / 380f, 0.05f, 0.28f);
            Projectile.netUpdate = true;
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item103 with { Pitch = 0.5f, Volume = 0.7f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = -0.3f, Volume = 0.45f }, Projectile.Center);
                new Circleedge_tight(Projectile.Center, Vector2.Zero, BlackHole.ColorHot, 14, 0.55f * (1f + Growth), 0.6f).Spawn();
                for (int i = 0; i < 5; i++)
                {
                    Vector2 dir = Main.rand.NextVector2Unit();
                    new VoidShardParticle(npc.Center, dir * Main.rand.NextFloat(1.5f, 3.5f) * 0.4f
                        + (Projectile.Center - npc.Center) * 0.04f, Color.White, 26, Main.rand.NextFloat(0.5f, 0.9f), 0.8f).Spawn();
                }
                new AccretionStreakParticle(Projectile, VisR * 1.4f, VisR * 0.3f, Color.White, 60, 1.1f, 0.85f).Spawn();
            }
        }
        private void EmitAmbient(float rate)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            if (Main.rand.NextFloat() < 0.5f * rate)
                new AccretionStreakParticle(Projectile, Main.rand.NextFloat(VisR * 1.1f, VisR * 2.1f), VisR * 0.32f, Color.Lerp(Color.Lerp(BlackHole.ColorHot, BlackHole.ColorEdge, Main.rand.NextFloat()), Color.White, Collapse01 * 0.6f), 80, Main.rand.NextFloat(0.45f, 0.9f) * (1f + 0.4f * Growth), 0.55f).Spawn();
            if (Main.rand.NextFloat() < 0.10f * rate)
            {
                Vector2 dir = Main.rand.NextVector2Unit();
                Vector2 from = Projectile.Center + dir * Main.rand.NextFloat(SuckR * 0.45f, SuckR * 0.8f);
                new VoidShardParticle(from, -dir * Main.rand.NextFloat(2f, 4f), Color.White, 46, Main.rand.NextFloat(0.5f, 1f), 0.75f).Spawn();
            }
            if (Main.rand.NextFloat() < 0.18f * rate)
            {
                Vector2 dir = Main.rand.NextVector2Unit();
                new Spark(Projectile.Center + dir * VisR * 0.34f, dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(2f, 5f), Main.rand.NextBool(4) ? Color.White : BlackHole.ColorHot, 13, Main.rand.NextFloat(0.2f, 0.35f), 0.45f).Spawn();
            }
        }
        private void UpdateScreenShader()
        {
            if (Main.dedServ || Main.myPlayer != Projectile.owner || !Projectile.active)
                return;
            BlackHoleScreenShader.HoleWorldPos = Projectile.Center;
            BlackHoleScreenShader.Grow = Growth;
            float canvasR0 = BASE_R * (1f + 0.85f * Growth) * (1f - 0.28f * Collapse01);
            float quadHalf0 = Math.Max(VisR, canvasR0) * 1.9f;
            float openK = Phase == 0 ? MathHelper.Clamp((Spawn01 - 0.45f) / 0.55f, 0f, 1f) : 1f;
            BlackHoleScreenShader.ShadowR = 0.33f * quadHalf0 * openK * (1f - Boom01);
            BlackHoleScreenShader.Lens = (0.55f + 0.75f * Growth) * Spawn01 * (1f + 1.6f * Collapse01) * (1f - Boom01);
            BlackHoleScreenShader.Boom = Boom01;
            if (Filters.Scene[BlackHoleScreenSystem.FilterName] != null
                && !Filters.Scene[BlackHoleScreenSystem.FilterName].IsActive())
                Filters.Scene.Activate(BlackHoleScreenSystem.FilterName, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ || Main.myPlayer != Projectile.owner)
                return;
            BlackHoleScreenShader.Lens = 0f;
            BlackHoleScreenShader.Boom = 0f;
            BlackHoleScreenShader.ShadowR = 0f;
            if (Filters.Scene[BlackHoleScreenSystem.FilterName] != null
                && Filters.Scene[BlackHoleScreenSystem.FilterName].IsActive())
                Filters.Scene.Deactivate(BlackHoleScreenSystem.FilterName);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!shaderLoadAttempted)
            {
                shaderLoadAttempted = true;
                try
                {
                    diskShader = ModContent.Request<Effect>("jarkos/Shaders/BlackHoleDisk", AssetRequestMode.ImmediateLoad);
                }
                catch { diskShader = null; }
                try
                {
                    burstShader = ModContent.Request<Effect>("jarkos/Shaders/BlackHoleBurst", AssetRequestMode.ImmediateLoad);
                }
                catch { burstShader = null; }
            }
            Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/VoidNoise", AssetRequestMode.ImmediateLoad).Value;
            if (diskShader?.Value != null && Phase < 3)
                DrawDisk(noiseTex);
            else if (diskShader?.Value == null && Phase < 3)
                DrawFallback();
            if (Phase == 3 && burstShader?.Value != null)
                DrawBurst(noiseTex);
            return false;
        }
        private void SetDiskParams(Effect fx, Texture2D noiseTex)
        {
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uSpawn"]?.SetValue(Spawn01);
            fx.Parameters["uCollapse"]?.SetValue(Collapse01);
            fx.Parameters["uBoom"]?.SetValue(Boom01);
            fx.Parameters["uGrow"]?.SetValue(Growth);
            fx.Parameters["uColorHot"]?.SetValue(BlackHole.ColorHot.ToVector3());
            fx.Parameters["uColorMid"]?.SetValue(BlackHole.ColorMid.ToVector3());
            fx.Parameters["uColorEdge"]?.SetValue(BlackHole.ColorEdge.ToVector3());
            fx.Parameters["uNoiseTexture"]?.SetValue(noiseTex);
        }
        private void DrawDisk(Texture2D noiseTex)
        {
            Effect fx = diskShader.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float canvasR = BASE_R * (1f + 0.85f * Growth) * (1f - 0.28f * Collapse01);
            float quadHalf = Math.Max(VisR, canvasR) * 1.9f;
            float quadScale = quadHalf * 2f / noiseTex.Width;
            SetDiskParams(fx, noiseTex);
            fx.Parameters["uMode"]?.SetValue(0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(noiseTex, drawPos, null, Color.White, 0f, noiseTex.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
            fx.Parameters["uMode"]?.SetValue(1f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(noiseTex, drawPos, null, Color.White, 0f, noiseTex.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawBurst(Texture2D noiseTex)
        {
            Effect fx = burstShader.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float quadHalf = 420f * (1f + 0.8f * Growth);
            float quadScale = quadHalf * 2f / noiseTex.Width;
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uBoom"]?.SetValue(Boom01);
            fx.Parameters["uGrow"]?.SetValue(Growth);
            fx.Parameters["uColorHot"]?.SetValue(BlackHole.ColorHot.ToVector3());
            fx.Parameters["uColorEdge"]?.SetValue(BlackHole.ColorEdge.ToVector3());
            fx.Parameters["uNoiseTexture"]?.SetValue(noiseTex);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(noiseTex, drawPos, null, Color.White, 0f, noiseTex.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawFallback()
        {
            Texture2D tex = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/BlackHole", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float s = VisR * 2f / tex.Width * (1f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f));
            Main.spriteBatch.Draw(tex, drawPos, null, Color.White * Spawn01, Main.GlobalTimeWrappedHourly * 1.5f, tex.Size() * 0.5f, s, SpriteEffects.None, 0f);
        }
    }
    public class BlackHoleBoom : ModProjectile
    {
        public override string Texture => "jarkos/particles/VoidShardParticle";
        private float BoomR => Projectile.ai[0] > 0 ? Projectile.ai[0] : 330f;
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.knockBack = 11f;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 c = Projectile.Center;
            Vector2 nearest = new Vector2(MathHelper.Clamp(c.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(c.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.DistanceSquared(c, nearest) <= BoomR * BoomR;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
        }
        public override bool PreDraw(ref Color lightColor) => false;
    }
}
