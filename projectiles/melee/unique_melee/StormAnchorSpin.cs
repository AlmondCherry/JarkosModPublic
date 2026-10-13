using System;
using jarkos.particles;
using jarkos.particles.storm;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee
{
    public class StormAnchorSpin : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/StormAnchor";
        private const int STATE_FLY = 0;
        private const int STATE_SPIN = 1;
        private const int STATE_RETURN = 2;
        public const int SPIN_TIME = 130;
        public const float HIT_RADIUS = 58f;
        private const int FISH_EVERY = 18;
        private static Asset<Effect> spinShader;
        private static bool shaderLoadAttempted;
        private int State { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private int stateTimer;
        private float spinFade;
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (player.active && player.whoAmI == Main.myPlayer)
            {
                player.GetModPlayer<PurgatoryPlayer>().AddShake(2.5f, 6);
            }

            SoundEngine.PlaySound(SoundID.Item171 with { Volume = 0.7f, Pitch = Main.rand.NextFloat(0.0f, 0.3f) }, target.Center);

            Vector2 hitPoint = target.active
                ? Vector2.Clamp(Projectile.Center, target.Hitbox.TopLeft(), target.Hitbox.BottomRight())
                : Projectile.Center;

            Color blueColor = new Color(30, 90, 240);
            Color cyanColor = new Color(90, 210, 255);

            int sparkCount = Main.rand.Next(6, 12);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6.5f, 6.5f);
                Color color = Main.rand.NextBool() ? blueColor : cyanColor;
                float scale = Main.rand.NextFloat(0.5f, 1.0f);
                int lifetime = Main.rand.Next(20, 38);
                new Spark(hitPoint, sparkVel, color, lifetime, scale, 0.9f).Spawn();
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Projectile.velocity = dir * 16f;
            SoundEngine.PlaySound(SoundID.Item7 with { Volume = 0.9f, Pitch = 0.1f }, Projectile.Center);
            new Slash_effect(Projectile.Center, dir * 7f, StormAnchorCharge.ColorCyan, 18, 0.5f, 0.9f).Spawn();
            new Circleedge(Projectile.Center, Vector2.Zero, StormAnchorCharge.ColorDeepBlue, 20, 0.2f, 0.8f).Spawn();
            for (int i = 0; i < 10; i++)
            {
                Vector2 v = dir.RotatedBy(Main.rand.NextFloat(-0.45f, 0.45f)) * Main.rand.NextFloat(7f, 13f);
                new Stretching_particle_out(Projectile.Center, v, Main.rand.NextBool(3) ? StormAnchorCharge.ColorFoam : StormAnchorCharge.ColorCyan, 38, Main.rand.NextFloat(50f, 100f), Main.rand.NextFloat(8f, 11f), 0.02f).Spawn();
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            switch (State)
            {
                case STATE_FLY: FlyAI(); break;
                case STATE_SPIN: SpinAI(player); break;
                case STATE_RETURN: ReturnAI(player); break;
            }
        }
        private void FlyAI()
        {
            stateTimer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 toMouse = Main.MouseWorld - Projectile.Center;
                if (toMouse.Length() < 30f || stateTimer > 45)
                {
                    StartSpin();
                    return;
                }
                float speed = MathHelper.Clamp(toMouse.Length() * 0.16f + 5f, 7f, 17f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse.SafeNormalize(Vector2.UnitX) * speed, 0.3f);
                if (stateTimer % 8 == 0)
                    Projectile.netUpdate = true;
            }
            if (Main.rand.NextBool(2))
            {
                new Stretching_particle_out(Projectile.Center, -Projectile.velocity * 0.45f, StormAnchorCharge.ColorCyan, 26, Main.rand.NextFloat(35f, 65f), Main.rand.NextFloat(6f, 9f), 0.02f).Spawn();
            }
            if (Main.rand.NextBool(3))
            {
                new Bubble_particle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), -Projectile.velocity * 0.1f, StormAnchorCharge.ColorCyan, 24, Main.rand.NextFloat(0.24f, 0.36f), 0.6f, 0.04f).Spawn();
            }
        }
        private void StartSpin()
        {
            State = STATE_SPIN;
            stateTimer = 0;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            SoundEngine.PlaySound(SoundID.Item84 with { Volume = 0.8f, Pitch = -0.2f }, Projectile.Center);
            new Circleedge(Projectile.Center, Vector2.Zero, StormAnchorCharge.ColorFoam, 22, 0.3f, 0.9f).Spawn();
            new Circleedge(Projectile.Center, Vector2.Zero, StormAnchorCharge.ColorDeepBlue, 22, 0.15f, 0.8f, 0.12f).Spawn();
            new StarParticle(Projectile.Center, Vector2.Zero, StormAnchorCharge.ColorFoam, 35, 0.18f, 1f, 0.05f).Spawn();
            Projectile.netUpdate = true;
        }
        private void SpinAI(Player player)
        {
            stateTimer++;
            Projectile.timeLeft = 120;
            float prog = stateTimer / (float)SPIN_TIME;
            spinFade = Math.Min(1f, stateTimer / 14f) * (1f - MathHelper.Clamp((prog - 0.85f) / 0.15f, 0f, 1f));
            float spinSpeed = MathHelper.Lerp(0.18f, 0.45f, MathHelper.Clamp(prog * 3f, 0f, 1f));
            Projectile.rotation += spinSpeed;
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 target = Main.MouseWorld;
                Vector2 fromPlayer = target - player.MountedCenter;
                if (fromPlayer.Length() > 420f)
                    target = player.MountedCenter + fromPlayer.SafeNormalize(Vector2.UnitX) * 420f;
                Projectile.Center = Vector2.Lerp(Projectile.Center, target, 0.16f);
                if (stateTimer % 8 == 0)
                    Projectile.netUpdate = true;
            }
            if (Main.rand.NextBool(2))
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edge = Projectile.Center + a.ToRotationVector2() * HIT_RADIUS * Main.rand.NextFloat(0.8f, 1f);
                Dust d = Dust.NewDustPerfect(edge, DustID.Water, a.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(2f, 4f), 0, default, Main.rand.NextFloat(1f, 1.6f));
                d.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edge = Projectile.Center + a.ToRotationVector2() * HIT_RADIUS;
                Vector2 fling = a.ToRotationVector2().RotatedBy(MathHelper.PiOver2)
                    * Main.rand.NextFloat(3.5f, 6f) - new Vector2(0f, 2f);
                new Raindrop_particle(edge, fling, StormAnchorCharge.ColorCyan, 30, Main.rand.NextFloat(0.5f, 0.9f), 0.85f).Spawn();
            }
            if (Main.rand.NextBool(4))
            {
                new Bubble_particle(Projectile.Center + Main.rand.NextVector2Circular(HIT_RADIUS * 0.7f, HIT_RADIUS * 0.7f), new Vector2(0f, -Main.rand.NextFloat(0.8f, 1.6f)), StormAnchorCharge.ColorCyan, 28, Main.rand.NextFloat(0.26f, 0.4f), 0.6f, 0.04f).Spawn();
            }
            if (Main.rand.NextBool(12))
            {
                new StarParticle(Projectile.Center + Main.rand.NextVector2Circular(HIT_RADIUS * 0.8f, HIT_RADIUS * 0.8f), Vector2.Zero, StormAnchorCharge.ColorFoam, 25, Main.rand.NextFloat(0.05f, 0.1f), 0.9f, 0.06f).Spawn();
            }
            if (Main.rand.NextBool(10))
            {
                Dust s = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), DustID.Electric, Main.rand.NextVector2Circular(1.5f, 1.5f), 100, default, 0.7f);
                s.noGravity = true;
            }
            if (Main.myPlayer == Projectile.owner && stateTimer % FISH_EVERY == 0 && stateTimer >= 12)
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spawn = Projectile.Center + a.ToRotationVector2() * HIT_RADIUS * 0.7f;
                Vector2 vel = a.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * 8.5f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawn, vel, ModContent.ProjectileType<StormFish>(), (int)(Projectile.damage * 0.55f), 3f, Projectile.owner);
                SoundEngine.PlaySound(SoundID.Item86 with { Volume = 0.45f, Pitch = 0.4f }, spawn);
            }
            Lighting.AddLight(Projectile.Center, 0.3f, 0.55f, 0.95f);
            if (stateTimer >= SPIN_TIME)
            {
                State = STATE_RETURN;
                stateTimer = 0;
                Projectile.netUpdate = true;
            }
        }
        private void ReturnAI(Player player)
        {
            stateTimer++;
            Projectile.timeLeft = 120;
            spinFade = Math.Max(0f, spinFade - 0.08f);
            Vector2 toPlayer = player.MountedCenter - Projectile.Center;
            if (toPlayer.Length() < 30f)
            {
                SoundEngine.PlaySound(SoundID.Grab with { Volume = 0.7f }, player.Center);
                Projectile.Kill();
                return;
            }
            float speed = MathHelper.Lerp(6f, 18f, MathHelper.Clamp(stateTimer / 45f, 0f, 1f));
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer.SafeNormalize(Vector2.UnitX) * speed, 0.12f);
            Projectile.rotation += 0.2f;
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Water, -Projectile.velocity * 0.1f, 0, default, 1.2f);
                d.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                new Stretching_particle_out(Projectile.Center, -Projectile.velocity * 0.4f, StormAnchorCharge.ColorCyan, 24, Main.rand.NextFloat(30f, 55f), Main.rand.NextFloat(5f, 8f), 0.02f).Spawn();
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (State == STATE_FLY)
                StartSpin();
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (State != STATE_SPIN)
                return null;
            Vector2 nearest = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.Distance(Projectile.Center, nearest) <= HIT_RADIUS;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            if (spinFade > 0.01f)
            {
                Texture2D ring = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/SpinRing").Value;
                float half = ring.Width / 2f;
                float scale = (HIT_RADIUS + 16f) / (half * 0.85f);
                if (!shaderLoadAttempted)
                {
                    shaderLoadAttempted = true;
                    try
                    {
                        spinShader = ModContent.Request<Effect>("jarkos/Shaders/SpinTrail", AssetRequestMode.ImmediateLoad);
                    }
                    catch { spinShader = null; }
                }
                if (spinShader != null && spinShader.Value != null)
                {
                    Effect fx = spinShader.Value;
                    fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 0.65f);
                    fx.Parameters["uFade"]?.SetValue(spinFade);
                    fx.Parameters["uNoiseTexture"]?.SetValue(ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/WaveNoise", AssetRequestMode.ImmediateLoad).Value);
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
                    Main.spriteBatch.Draw(ring, drawPos, null, Color.White, 0f, new Vector2(half, half), scale, SpriteEffects.None, 0f);
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                else
                {
                    Main.spriteBatch.Draw(ring, drawPos, null, new Color(120, 190, 255, 0) * (0.7f * spinFade), Main.GlobalTimeWrappedHourly * 4f, new Vector2(half, half), scale, SpriteEffects.None, 0f);
                }
            }
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            if (State == STATE_SPIN || State == STATE_RETURN)
            {
                for (int i = 1; i <= 4; i++)
                {
                    Color c = new Color(80, 160, 255, 0) * (0.3f * (1f - i / 5f)) * Math.Max(spinFade, 0.4f);
                    Main.EntitySpriteDraw(tex, drawPos, null, c, Projectile.rotation - i * 0.22f, origin, 1f, SpriteEffects.None, 0);
                }
            }
            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            return false;
        }
    }
}
