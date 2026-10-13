using System;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class ReversalRiftGate : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/true_melee/RiftRing";
        public const float CAST_RANGE = 420f;
        public const int COOLDOWN = 360;
        private const int OPEN_TIME = 14;
        private const int ACTIVE_TIME = 150;
        private const int COLLAPSE_TIME = 18;
        private const int LIFETIME = OPEN_TIME + ACTIVE_TIME + COLLAPSE_TIME;
        private const float HIT_RADIUS = 130f;
        private const float PULL_RADIUS = 300f;
        private const float REVERSE_RADIUS = 230f;
        private const float BURST_RADIUS = 240f;
        private static Asset<Effect> gateShader;
        private static bool gateShaderTried;
        private int timer;
        private bool collapseStarted;
        private bool Collapsing => timer >= OPEN_TIME + ACTIVE_TIME;
        private float OpenK
        {
            get
            {
                if (timer < OPEN_TIME)
                {
                    float t = timer / (float)OPEN_TIME;
                    return 1f - (1f - t) * (1f - t);
                }
                if (Collapsing)
                {
                    float t = (timer - OPEN_TIME - ACTIVE_TIME) / (float)COLLAPSE_TIME;
                    return (1f - t) * (1f - t);
                }
                return 1f;
            }
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = LIFETIME;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.rotation);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.rotation = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Projectile.netUpdate = true;
            }
            Projectile.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item84 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
                new Circleedge(Projectile.Center, Vector2.Zero, ReversalBladeSwing.ColorGold, 16, 0.2f, 0.9f).Spawn();
                new Circular_glow(Projectile.Center, Vector2.Zero, ReversalBladeSwing.ColorCyan, 14, 0.3f, 0.7f).Spawn();
            }
        }
        public override void AI()
        {
            timer++;
            float open = OpenK;
            Lighting.AddLight(Projectile.Center, ReversalBladeSwing.ColorGold.ToVector3() * 0.45f * open);
            Lighting.AddLight(Projectile.Center, ReversalBladeSwing.ColorCyan.ToVector3() * 0.35f * open);
            if (Collapsing)
            {
                CollapseAI();
                return;
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.boss || npc.knockBackResist <= 0f || npc.dontTakeDamage)
                    continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist > PULL_RADIUS || dist < 16f)
                    continue;
                Vector2 pull = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                float power = (1f - dist / PULL_RADIUS) * 0.55f * npc.knockBackResist * open;
                npc.velocity += pull * power;
                if (npc.velocity.Length() > 9f)
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 9f;
            }
            if (Main.myPlayer == Projectile.owner && open > 0.6f)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active || !p.hostile || p.friendly || p.damage <= 0)
                        continue;
                    if (Vector2.Distance(p.Center, Projectile.Center) > REVERSE_RADIUS)
                        continue;
                    p.hostile = false;
                    p.friendly = true;
                    p.owner = Projectile.owner;
                    p.damage = Math.Max(p.damage, Projectile.damage);
                    p.DamageType = DamageClass.Melee;
                    p.velocity = -p.velocity * 1.1f;
                    p.netUpdate = true;
                    if (Main.netMode != NetmodeID.Server)
                    {
                        SoundEngine.PlaySound(SoundID.Item150 with { Volume = 0.4f, Pitch = 0.6f }, p.Center);
                        new Circleedge(p.Center, Vector2.Zero, ReversalBladeSwing.ColorGold, 14, 0.1f, 0.9f).Spawn();
                        new Spark(p.Center, -p.velocity * 0.2f, ReversalBladeSwing.ColorCyan, 18, 0.5f, 0.9f).Spawn();
                    }
                }
            }
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(2) && open > 0.5f)
                {
                    float a = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 from = Projectile.Center + a.ToRotationVector2() * Main.rand.NextFloat(120f, 230f);
                    new Stretching_particle_out(from, (Projectile.Center - from) * 0.10f, Main.rand.NextBool(3) ? ReversalBladeSwing.ColorGold : ReversalBladeSwing.ColorBlade, 22, Main.rand.NextFloat(35f, 70f), Main.rand.NextFloat(4f, 6f), 0.02f).Spawn();
                }
                if (Main.rand.NextBool(3) && open > 0.5f)
                {
                    Vector2 rim = Projectile.Center + (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2()
                        .RotatedBy(Main.rand.NextBool() ? MathHelper.PiOver2 : -MathHelper.PiOver2)
                        * Main.rand.NextFloat(60f, 90f);
                    new Spark(rim, Main.rand.NextVector2Circular(1.5f, 1.5f), ReversalBladeSwing.ColorGold, 20, Main.rand.NextFloat(0.3f, 0.55f), 0.85f).Spawn();
                }
                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(50f, 80f) * open, DustID.IceTorch, Main.rand.NextVector2Circular(1f, 1f), 0, default, Main.rand.NextFloat(1f, 1.6f));
                    d.noGravity = true;
                }
            }
        }
        private void CollapseAI()
        {
            if (!collapseStarted)
            {
                collapseStarted = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.7f, Pitch = 0.25f }, Projectile.Center);
                    if (Main.LocalPlayer.whoAmI == Projectile.owner)
                    {
                        Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 8f, 7f, 14, 900f, FullName));
                    }
                    new Circleedge(Projectile.Center, Vector2.Zero, ReversalBladeSwing.ColorGold, 26, 0.34f, 0.95f, 0.1f).Spawn();
                    new Circular_glow(Projectile.Center, Vector2.Zero, ReversalBladeSwing.ColorCyan, 22, 0.5f, 0.85f).Spawn();
                    new StarParticle(Projectile.Center, Vector2.Zero, ReversalBladeSwing.ColorCore, 28, 0.22f, 1f, 0.05f).Spawn();
                    for (int i = 0; i < 14; i++)
                    {
                        Vector2 v = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 11f);
                        new Spark(Projectile.Center, v, Main.rand.NextBool(3) ? ReversalBladeSwing.ColorGold : ReversalBladeSwing.ColorCyan, 26, Main.rand.NextFloat(0.4f, 0.7f), 0.9f).Spawn();
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 v = Main.rand.NextVector2Unit() * Main.rand.NextFloat(7f, 12f);
                        new Stretching_particle_out(Projectile.Center, v, ReversalBladeSwing.ColorGold, 24, Main.rand.NextFloat(50f, 90f), Main.rand.NextFloat(5f, 8f), 0.02f).Spawn();
                    }
                }
                for (int i = 0; i < Projectile.localNPCImmunity.Length; i++)
                    Projectile.localNPCImmunity[i] = 0;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Collapsing ? BURST_RADIUS : HIT_RADIUS * OpenK;
            if (radius < 20f)
                return false;
            Vector2 nearest = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.Distance(Projectile.Center, nearest) <= radius;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Collapsing)
            {
                modifiers.SourceDamage *= 2.2f;
                modifiers.Knockback *= 2f;
                modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
            }
            else
            {
                modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            new Spark(target.Center, Main.rand.NextVector2Circular(3f, 3f), ReversalBladeSwing.ColorCyan, 18, 0.45f, 0.85f).Spawn();
            if (Collapsing)
                new Circleedge(target.Center, Vector2.Zero, ReversalBladeSwing.ColorGold, 16, 0.14f, 0.9f).Spawn();
        }
        private static bool TryGetGateShader(out Effect fx)
        {
            if (!gateShaderTried)
            {
                gateShaderTried = true;
                try
                {
                    gateShader = ModContent.Request<Effect>("jarkos/Shaders/RiftGate", AssetRequestMode.ImmediateLoad);
                }
                catch { gateShader = null; }
            }
            fx = gateShader != null ? gateShader.Value : null;
            return fx != null;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float open = OpenK;
            if (open <= 0.02f)
                return false;
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/RiftNoise").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            if (TryGetGateShader(out Effect fx))
            {
                float quadHalf = HIT_RADIUS * 1.75f;
                float quadScale = quadHalf * 2f / noise.Width;
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                fx.Parameters["uFade"]?.SetValue(Collapsing ? 0.85f + 0.5f * (1f - open) : 0.85f);
                fx.Parameters["uOpen"]?.SetValue(open);
                fx.Parameters["uNoiseTexture"]?.SetValue(noise);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(noise, drawPos, null, Color.White, Projectile.rotation, noise.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                return false;
            }
            Texture2D ring = TextureAssets.Projectile[Type].Value;
            Vector2 origin = ring.Size() / 2f;
            float baseScale = HIT_RADIUS * 2.4f / ring.Width;
            float pulse = 1f + 0.06f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 9f);
            Main.spriteBatch.Draw(ring, drawPos, null, new Color(255, 190, 80, 0) * (0.75f * open), Projectile.rotation, origin, new Vector2(baseScale * 0.35f * open, baseScale) * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(ring, drawPos, null, new Color(125, 233, 247, 0) * (0.5f * open), Projectile.rotation, origin, new Vector2(baseScale * 0.22f * open, baseScale * 0.8f) * pulse, SpriteEffects.None, 0f);
            return false;
        }
    }
}
