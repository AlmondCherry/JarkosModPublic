using System;
using jarkos.particles;
using jarkos.particles.storm;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee
{
    public class StormAnchorCharge : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/StormAnchor";
        public const int MAX_CHARGE = 200;
        public const int PULL_TIME = 9;
        internal static readonly Color ColorCyan = new Color(80, 190, 255);
        internal static readonly Color ColorDeepBlue = new Color(45, 110, 230);
        internal static readonly Color ColorFoam = new Color(190, 235, 255);
        private Player Owner => Main.player[Projectile.owner];
        private float Charge01 => MathHelper.Clamp(Projectile.ai[0] / MAX_CHARGE, 0f, 1f);
        private bool raised;
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
        }
        public override void AI()
        {
            Player player = Owner;
            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            Projectile.timeLeft = 4;
            if (Projectile.localAI[0] > 0f)
            {
                ThrowPhase(player);
                return;
            }
            if (Main.myPlayer == Projectile.owner)
            {
                player.ChangeDir(Main.MouseWorld.X >= player.Center.X ? 1 : -1);
                Projectile.spriteDirection = player.direction;
            }
            bool wasFull = Projectile.ai[0] >= MAX_CHARGE;
            if (Projectile.ai[0] < MAX_CHARGE)
                Projectile.ai[0]++;
            float c = Charge01;
            float lift = MathHelper.Lerp(0.33f, 0.42f, c) + (float)Math.Sin(Projectile.ai[0] * 0.11f) * 0.03f * (0.5f + c);
            float tipAngle = player.direction == 1 ? -lift : MathHelper.Pi + lift;
            Projectile.rotation = tipAngle + MathHelper.PiOver2;
            Projectile.Center = player.MountedCenter + new Vector2(player.direction * 4f, -26f - 5f * c);
            if (!raised)
            {
                raised = true;
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.8f, Pitch = -0.2f }, Projectile.Center);
                new Slash_effect(player.Center, new Vector2(0f, -5f), ColorCyan, 14, 0.35f, 0.7f).Spawn();
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (Projectile.Center - player.MountedCenter).ToRotation() - MathHelper.PiOver2);
            Vector2 tipDir = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
            Vector2 tip = Projectile.Center + tipDir * 36f;
            if (Main.rand.NextFloat() < 0.15f + 0.35f * c)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + tipDir * Main.rand.NextFloat(-30f, 30f), DustID.Water, new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(0.5f, 1.5f)));
                d.scale = Main.rand.NextFloat(0.9f, 1.4f);
            }
            if (Main.rand.NextFloat() < 0.3f + 0.55f * c)
            {
                Vector2 pos = Projectile.Center + tipDir * Main.rand.NextFloat(-32f, 32f)
                    + Main.rand.NextVector2Circular(4f, 4f);
                new Bubble_particle(pos, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.8f, 1.8f) * (0.7f + 0.8f * c)), ColorCyan, 28, Main.rand.NextFloat(0.28f, 0.42f) * (0.8f + 0.4f * c), 0.65f, Main.rand.NextFloat(-0.05f, 0.05f)).Spawn();
            }
            if (c > 0.25f && Projectile.ai[0] % 4 == 0)
            {
                Vector2 from = tip + Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(80f, 140f);
                new Stretching_particle(from, tip, Main.rand.NextBool(3) ? ColorFoam : ColorDeepBlue, (int)MathHelper.Lerp(26f, 14f, c), Main.rand.NextFloat(40f, 85f), Main.rand.NextFloat(6f, 9f), 0.02f).Spawn();
            }
            if (Projectile.ai[0] % 9 == 0 && c > 0.1f)
            {
                new Circular_glow(tip, Vector2.Zero, ColorCyan, 20, 0.1f + 0.3f * c, 0.45f + 0.3f * c).Spawn();
            }
            if (c > 0.55f && Main.rand.NextFloat() < 0.35f * c)
            {
                new Spark(tip + Main.rand.NextVector2Circular(14f, 14f), new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), -Main.rand.NextFloat(1f, 3f)), ColorFoam, 30, Main.rand.NextFloat(0.5f, 0.9f), 0.9f).Spawn();
                if (Main.rand.NextBool(4))
                {
                    Dust s = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(14f, 30f), DustID.Electric, new Vector2(0f, -Main.rand.NextFloat(0.5f, 1.5f)), 100, default, 0.8f);
                    s.noGravity = true;
                }
            }
            if (Projectile.ai[0] == MAX_CHARGE / 3 || Projectile.ai[0] == MAX_CHARGE * 2 / 3)
            {
                new Circleedge(tip, Vector2.Zero, ColorCyan, 20, 0.25f, 0.7f).Spawn();
                SoundEngine.PlaySound(SoundID.Drip with { Volume = 0.7f, Pitch = 0.5f }, tip);
            }
            if (!wasFull && Projectile.ai[0] >= MAX_CHARGE)
            {
                SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 1f, Pitch = 0.2f }, Projectile.Center);
                new Circleedge(tip, Vector2.Zero, ColorFoam, 25, 0.35f, 1f).Spawn();
                new Circleedge(tip, Vector2.Zero, ColorDeepBlue, 25, 0.15f, 0.9f, 0.1f).Spawn();
                new StarParticle(tip, Vector2.Zero, ColorFoam, 40, 0.22f, 1f, 0.04f).Spawn();
                for (int i = 0; i < 14; i++)
                {
                    new Stretching_particle_out(tip, Main.rand.NextVector2Circular(7f, 7f), ColorCyan, 35, Main.rand.NextFloat(40f, 90f), Main.rand.NextFloat(7f, 10f), 0.02f).Spawn();
                }
            }
            if (wasFull)
            {
                if (Projectile.ai[0] % 5 == 0)
                    new Circular_glow(tip, Vector2.Zero, ColorCyan, 18, 0.35f, 0.5f).Spawn();
                if (Main.rand.NextBool(9))
                    new StarParticle(tip + Main.rand.NextVector2Circular(12f, 12f), Vector2.Zero, ColorFoam, 30, Main.rand.NextFloat(0.06f, 0.12f), 0.9f, 0.05f).Spawn();
            }
            Lighting.AddLight(Projectile.Center, 0.1f + 0.25f * c, 0.2f + 0.4f * c, 0.4f + 0.6f * c);
            if (Main.myPlayer == Projectile.owner)
            {
                bool holding = PlayerInput.Triggers.Current.MouseRight;
                if (!holding)
                {
                    Projectile.localAI[0] = 1f;
                    Projectile.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.3f }, Projectile.Center);
                }
            }
        }
        private void ThrowPhase(Player player)
        {
            Projectile.localAI[0]++;
            float p = Projectile.localAI[0] / PULL_TIME;
            Vector2 hand = player.MountedCenter + new Vector2(player.direction * 8f, -6f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, hand, MathHelper.Clamp(p, 0f, 1f));
            Vector2 aim = Main.myPlayer == Projectile.owner ? (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction)
                : Vector2.UnitX * player.direction;
            float targetRot = aim.ToRotation() + MathHelper.PiOver2;
            Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, MathHelper.Clamp(0.2f + p * 0.6f, 0f, 1f));
            player.ChangeDir(aim.X >= 0f ? 1 : -1);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, aim.ToRotation() - MathHelper.PiOver2);
            if (Projectile.localAI[0] >= PULL_TIME)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    float c = Charge01;
                    Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
                    float speed = 9f + 9.5f * c;
                    int damage = (int)(Projectile.damage * (0.7f + 1.8f * c));
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, dir * speed, ModContent.ProjectileType<StormAnchorThrown>(), damage, Projectile.knockBack, player.whoAmI, c);
                    new Slash_effect(player.Center, dir * 6f, ColorCyan, 18, 0.45f + 0.35f * c, 0.85f).Spawn();
                    int lines = 6 + (int)(8 * c);
                    for (int i = 0; i < lines; i++)
                    {
                        Vector2 v = dir.RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * Main.rand.NextFloat(6f, 14f) * (0.6f + 0.6f * c);
                        new Stretching_particle_out(player.MountedCenter, v, Main.rand.NextBool(3) ? ColorFoam : ColorCyan, 40, Main.rand.NextFloat(50f, 110f), Main.rand.NextFloat(8f, 12f), 0.02f).Spawn();
                    }
                    if (c > 0.95f)
                        new Circleedge(player.MountedCenter, Vector2.Zero, ColorFoam, 22, 0.3f, 0.9f).Spawn();
                }
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f }, player.Center);
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            float c = Charge01;
            Vector2 shake = c > 0.4f ? Main.rand.NextVector2Circular(c * 1.2f, c * 0.6f)
                : Vector2.Zero;
            Color glow = new Color(80, 160, 255, 0) * (0.35f * c);
            for (int i = 0; i < 4 && c > 0.15f; i++)
            {
                Vector2 off = (MathHelper.PiOver2 * i).ToRotationVector2() * (1.5f + 2f * c);
                Main.EntitySpriteDraw(tex, Projectile.Center + off + shake - Main.screenPosition, null, glow, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(tex, Projectile.Center + shake - Main.screenPosition, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            return false;
        }
    }
}
