using System;
using System.Collections.Generic;
using System.IO;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class KarmaDash : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/KarmaBlade";
        public const float DASH_RANGE = 336f;
        public const int MARK_DELAY = 24;
        public const int BURST_FRAMES = 4;
        public const int LIFE = MARK_DELAY + BURST_FRAMES + 26;
        private const float LINE_WIDTH = 52f;
        private static Asset<Effect> lineShader;
        private static bool shaderTried;
        private Vector2 dashStart;
        private Vector2 dashEnd;
        private readonly List<int> marked = new();
        private int timer;
        private bool cutDone;
        private bool initialized;
        private int BurnedKarma => (int)Projectile.ai[0];
        private bool InBurst => timer > MARK_DELAY && timer <= MARK_DELAY + BURST_FRAMES;
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = LIFE;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(dashStart);
            writer.WriteVector2(dashEnd);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            dashStart = reader.ReadVector2();
            dashEnd = reader.ReadVector2();
            if (!initialized)
            {
                InitializeVisuals();
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            if (Main.myPlayer == Projectile.owner)
            {
                dashStart = player.Center;
                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
                Vector2 end = dashStart;
                const float STEP = 8f;
                for (float d = STEP; d <= DASH_RANGE; d += STEP)
                {
                    Vector2 probe = dashStart + dir * d;
                    Vector2 corner = probe - new Vector2(player.width * 0.5f, player.height * 0.5f);
                    if (Collision.SolidCollision(corner, player.width, player.height))
                        break;
                    end = probe;
                }
                dashEnd = end;
                Projectile.Center = dashStart;
                player.Center = dashEnd;
                player.velocity *= 0.15f;
                player.fallStart = (int)(player.position.Y / 16f);
                Projectile.netUpdate = true;
                InitializeVisuals();
            }
        }
        private void InitializeVisuals()
        {
            initialized = true;
            Player player = Main.player[Projectile.owner];
            player.immune = true;
            player.immuneTime = MARK_DELAY + 12;
            MarkCorridor();
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.9f, Pitch = 0.8f }, dashStart);
                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.5f, Pitch = 0.5f }, dashEnd);
                int trailDots = (int)(Vector2.Distance(dashStart, dashEnd) / 18f);
                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
                for (int i = 0; i <= trailDots; i++)
                {
                    Vector2 pos = Vector2.Lerp(dashStart, dashEnd, i / (float)Math.Max(trailDots, 1));
                    new Line_glow(pos, dir * Main.rand.NextFloat(0.5f, 1.5f), Main.rand.NextBool(3) ? KarmaBladeSwing.ColorCore : KarmaBladeSwing.ColorMid, 16, Main.rand.NextFloat(0.35f, 0.6f), 0.55f, 0f).Spawn();
                }
            }
        }
        private void MarkCorridor()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || marked.Contains(npc.whoAmI))
                    continue;
                float cp = 0f;
                if (Collision.CheckAABBvLineCollision(npc.Hitbox.TopLeft(), npc.Hitbox.Size(), dashStart, dashEnd, LINE_WIDTH, ref cp))
                {
                    marked.Add(npc.whoAmI);
                    new Circleedge(npc.Center, Vector2.Zero, KarmaBladeSwing.ColorMid, MARK_DELAY, 0.12f, 0.8f).Spawn();
                }
            }
        }
        public override void AI()
        {
            if (!initialized)
                return;
            timer++;
            if (timer <= MARK_DELAY && Main.rand.NextBool(3))
            {
                Vector2 pos = Vector2.Lerp(dashStart, dashEnd, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Main.rand.NextVector2Circular(0.6f, 0.6f), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                d.noGravity = true;
            }
            if (timer == MARK_DELAY + 1)
            {
                Rectangle box = Terraria.Utils.CenteredRectangle((dashStart + dashEnd) * 0.5f, new Vector2(Math.Abs(dashEnd.X - dashStart.X) + LINE_WIDTH, Math.Abs(dashEnd.Y - dashStart.Y) + LINE_WIDTH));
                Projectile.position = box.TopLeft();
                Projectile.width = box.Width;
                Projectile.height = box.Height;
            }
            if (timer == MARK_DELAY + 1 && !cutDone)
            {
                cutDone = true;
                MarkCorridor();
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item101 with { Volume = 1f, Pitch = -0.2f }, dashEnd);
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f, Pitch = -0.35f }, dashEnd);
                    SoundEngine.PlaySound(SoundID.Item84 with { Volume = 0.55f, Pitch = 0.3f }, dashEnd);
                    Vector2 dir = (dashEnd - dashStart).SafeNormalize(Vector2.UnitX);
                    Vector2 perp = dir.RotatedBy(MathHelper.PiOver2);
                    float len = Vector2.Distance(dashStart, dashEnd);
                    int waveDots = (int)(len / 24f);
                    for (int i = 0; i <= waveDots; i++)
                    {
                        float t = i / (float)Math.Max(waveDots, 1);
                        Vector2 pos = Vector2.Lerp(dashStart, dashEnd, t);
                        new Line_glow(pos, perp * Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextBool(3) ? KarmaBladeSwing.ColorCore : KarmaBladeSwing.ColorEdge, 18, Main.rand.NextFloat(0.4f, 0.7f), 0.7f, 0f).Spawn();
                        if (Main.rand.NextBool(2))
                            new Spark(pos, perp * Main.rand.NextFloat(-4f, 4f) + dir * Main.rand.NextFloat(2f), KarmaBladeSwing.ColorMid, 20, Main.rand.NextFloat(0.25f, 0.45f), 0.8f).Spawn();
                    }
                    foreach (int idx in marked)
                    {
                        NPC npc = Main.npc[idx];
                        if (!npc.active)
                            continue;
                        CutVisuals(npc);
                    }
                    Player owner = Main.player[Projectile.owner];
                    new Circular_glow(owner.MountedCenter, Vector2.Zero, KarmaBladeSwing.ColorCore, 16, 0.34f, 0.9f).Spawn();
                    new Circleedge(owner.MountedCenter, Vector2.Zero, KarmaBladeSwing.ColorEdge, 22, 0.30f, 1f).Spawn();
                    new StarParticle(owner.MountedCenter, Vector2.Zero, KarmaBladeSwing.ColorCore, 20, 0.30f, 1f, 0.05f).Spawn();
                    if (Main.LocalPlayer.whoAmI == Projectile.owner)
                    {
                        Main.instance.CameraModifiers.Add(new PunchCameraModifier(dashEnd, dir, marked.Count > 0 ? 12f : 6f, 7f, 16, 1200f, FullName));
                    }
                }
            }
            Lighting.AddLight(Vector2.Lerp(dashStart, dashEnd, 0.5f), KarmaBladeSwing.ColorMid.ToVector3() * 0.4f);
        }
        private void CutVisuals(NPC npc)
        {
            Vector2 center = npc.Center;
            float scale = MathHelper.Clamp(npc.width / 40f, 0.7f, 2f) * (1f + BurnedKarma * 0.06f);
            Vector2 dir = (dashEnd - dashStart).SafeNormalize(Vector2.UnitX);
            new Stretching_particle_out(center, dir * 12f, KarmaBladeSwing.ColorCore, 24, 130f * scale, 5f, 0.02f).Spawn();
            new Stretching_particle_out(center, -dir * 12f, KarmaBladeSwing.ColorCore, 24, 130f * scale, 5f, 0.02f).Spawn();
            for (int c = 0; c < 2; c++)
            {
                float ang = Main.rand.NextFloat(MathHelper.Pi);
                for (int s = -1; s <= 1; s += 2)
                    new Stretching_particle_out(center, ang.ToRotationVector2() * 9f * s, KarmaBladeSwing.ColorEdge, 18, 70f * scale, 3.5f, 0.02f).Spawn();
            }
            new StarParticleHorizontal(center, Vector2.Zero, KarmaBladeSwing.ColorCore, 20, 0.30f * scale, 1f).Spawn();
            new StarParticle(center, Vector2.Zero, KarmaBladeSwing.ColorEdge, 24, 0.26f * scale, 1f, 0.05f).Spawn();
            new Circleedge(center, Vector2.Zero, KarmaBladeSwing.ColorEdge, 20, 0.20f * scale, 0.95f).Spawn();
            new Circular_glow(center, Vector2.Zero, KarmaBladeSwing.ColorMid, 16, 0.30f * scale, 0.85f).Spawn();
            for (int i = 0; i < 10; i++)
                new Spark(center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 10f), Main.rand.NextBool(3) ? KarmaBladeSwing.ColorCore : KarmaBladeSwing.ColorMid, 22, Main.rand.NextFloat(0.3f, 0.55f), 0.9f).Spawn();
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(center, DustID.PurpleTorch, Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(1.1f, 1.7f));
                d.noGravity = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!InBurst)
                return false;
            foreach (int idx in marked)
            {
                NPC npc = Main.npc[idx];
                if (npc.active && targetHitbox.Intersects(npc.Hitbox))
                    return true;
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.45f, Pitch = 0.6f }, target.Center);
        }
        private static bool TryGetLineShader(out Effect fx)
        {
            fx = null;
            if (!shaderTried)
            {
                shaderTried = true;
                try
                {
                    lineShader = ModContent.Request<Effect>("jarkos/Shaders/KarmaLine", AssetRequestMode.ImmediateLoad);
                }
                catch
                {
                    lineShader = null;
                }
            }
            if (lineShader == null || !lineShader.IsLoaded)
                return false;
            fx = lineShader.Value;
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 delta = dashEnd - dashStart;
            float len = delta.Length();
            if (len < 8f)
                return false;
            float fade = timer <= MARK_DELAY
                ? 0.55f + 0.20f * (float)Math.Sin(timer * 0.55f)
                : MathHelper.Clamp(1.25f - (timer - MARK_DELAY) / 16f, 0f, 1f);
            if (fade <= 0.02f)
                return false;
            float finisher = timer <= MARK_DELAY
                ? 0f
                : MathHelper.Clamp(1.2f - (timer - MARK_DELAY) / 10f, 0f, 1f);
            float rot = delta.ToRotation();
            Vector2 mid = (dashStart + dashEnd) * 0.5f - Main.screenPosition;
            if (TryGetLineShader(out Effect fx))
            {
                Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/KarmaNoise").Value;
                const float THICK = 110f;
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                fx.Parameters["uSpan"]?.SetValue(len / 1000f);
                fx.Parameters["uR0"]?.SetValue(3.2f / (THICK * 0.5f));
                fx.Parameters["uR1"]?.SetValue(26f / (THICK * 0.5f));
                fx.Parameters["uFinisher"]?.SetValue(finisher);
                fx.Parameters["uFade"]?.SetValue(fade);
                fx.Parameters["uColorEdge"]?.SetValue(KarmaBladeSwing.ColorEdge.ToVector3());
                fx.Parameters["uColorMid"]?.SetValue(KarmaBladeSwing.ColorMid.ToVector3());
                fx.Parameters["uColorCore"]?.SetValue(KarmaBladeSwing.ColorCore.ToVector3());
                fx.Parameters["uNoiseTexture"]?.SetValue(noise);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(noise, mid, null, Color.White, rot, noise.Size() * 0.5f, new Vector2(len / noise.Width, THICK / noise.Height), SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                return false;
            }
            Texture2D px = TextureAssets.MagicPixel.Value;
            Vector2 origin = new(0f, 0.5f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(px, dashStart - Main.screenPosition, new Rectangle(0, 0, 1, 1), KarmaBladeSwing.ColorMid * (0.5f * fade), rot, origin, new Vector2(len, 7f + finisher * 10f), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(px, dashStart - Main.screenPosition, new Rectangle(0, 0, 1, 1), KarmaBladeSwing.ColorCore * (0.8f * fade), rot, origin, new Vector2(len, 1.6f + finisher * 3f), SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
