using System;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class StormAnchorThrown : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/StormAnchor";
        private const int STATE_FLY = 0;
        private const int STATE_STUCK = 1;
        private const int STATE_RETURN = 2;
        private const int STICK_TIME = 28;
        private const float RETURN_TURN = 0.055f;
        private float Charge => Projectile.ai[0];
        private int State { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }
        private int StuckTarget { get => (int)Projectile.ai[2]; set => Projectile.ai[2] = value; }
        private int stateTimer;
        private Vector2 stuckOffset;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 24;
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
                case STATE_STUCK: StuckAI(player); break;
                case STATE_RETURN: ReturnAI(player); break;
            }
        }
        private void FlyAI()
        {
            if (Projectile.timeLeft < 560)
            {
                Projectile.velocity.Y += 0.06f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextFloat() < 0.35f + 0.4f * Charge)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f + Main.rand.NextVector2Circular(4f, 4f), DustID.Water, -Projectile.velocity * 0.1f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.9f, 1.5f) * (0.7f + 0.5f * Charge);
            }
            if (Charge > 0.6f && Main.rand.NextBool(8))
            {
                Dust s = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(1f, 1f), 100, default, 0.7f);
                s.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.15f + 0.2f * Charge, 0.3f + 0.3f * Charge, 0.6f);
        }
        private void StuckAI(Player player)
        {
            Projectile.timeLeft = 120;
            stateTimer++;
            if (StuckTarget >= 0)
            {
                NPC npc = Main.npc[StuckTarget];
                if (npc.active && !npc.dontTakeDamage)
                {
                    Projectile.Center = npc.Center + stuckOffset;
                }
                else
                {
                    StartReturn(player);
                    return;
                }
            }
            if (stateTimer < 10)
                Projectile.rotation += Main.rand.NextFloat(-0.06f, 0.06f) * (10 - stateTimer) / 10f;
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.Water, new Vector2(0f, -1f), 0, default, 1.1f);
                d.noGravity = true;
            }
            if (stateTimer >= STICK_TIME)
                StartReturn(player);
        }
        private void StartReturn(Player player)
        {
            State = STATE_RETURN;
            stateTimer = 0;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.damage = (int)(Projectile.damage * 0.6f);
            Vector2 toPlayer = (player.MountedCenter - Projectile.Center).SafeNormalize(Vector2.UnitY);
            Vector2 side = toPlayer.RotatedBy(MathHelper.PiOver2 * (Main.rand.NextBool() ? 1 : -1));
            Projectile.velocity = (side * 0.9f - toPlayer * 0.45f - Vector2.UnitY * 0.6f)
                .SafeNormalize(-Vector2.UnitY) * 7.5f;
            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
            Projectile.netUpdate = true;
        }
        private void ReturnAI(Player player)
        {
            Projectile.timeLeft = 120;
            stateTimer++;
            Vector2 toPlayer = player.MountedCenter - Projectile.Center;
            float dist = toPlayer.Length();
            if (dist < 28f)
            {
                SoundEngine.PlaySound(SoundID.Grab with { Volume = 0.7f }, player.Center);
                for (int i = 0; i < 8; i++)
                {
                    Dust d = Dust.NewDustPerfect(player.MountedCenter, DustID.Water, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.2f);
                    d.noGravity = true;
                }
                Projectile.Kill();
                return;
            }
            float speed = MathHelper.Lerp(7.5f, 19f, MathHelper.Clamp(stateTimer / 70f, 0f, 1f));
            float turn = RETURN_TURN * (1f + stateTimer / 60f);
            Vector2 desired = toPlayer.SafeNormalize(Vector2.UnitX);
            Vector2 current = Projectile.velocity.SafeNormalize(desired);
            float angle = MathHelper.WrapAngle(desired.ToRotation() - current.ToRotation());
            float step = MathHelper.Clamp(angle, -turn, turn);
            Projectile.velocity = current.RotatedBy(step) * speed;
            float targetRot = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, 0.2f);
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.Water, -Projectile.velocity * 0.08f, 0, default, 1.3f);
                d.noGravity = true;
            }
            if (Main.rand.NextBool(2))
            {
                new jarkos.particles.Stretching_particle_out(Projectile.Center, -Projectile.velocity * 0.35f, StormAnchorCharge.ColorCyan, 24, Main.rand.NextFloat(30f, 60f), Main.rand.NextFloat(5f, 8f), 0.02f).Spawn();
            }
            Lighting.AddLight(Projectile.Center, 0.15f, 0.3f, 0.55f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (player.active && player.whoAmI == Main.myPlayer)
            {
                player.GetModPlayer<PurgatoryPlayer>().AddShake(4.5f, 10);
            }

            SoundEngine.PlaySound(SoundID.Item171 with { Volume = 0.9f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, target.Center);

            Vector2 hitPoint = target.active
                ? Vector2.Clamp(Projectile.Center, target.Hitbox.TopLeft(), target.Hitbox.BottomRight())
                : Projectile.Center;

            Color blueColor = new Color(30, 90, 240);
            Color cyanColor = new Color(90, 210, 255);

            int sparkCount = Main.rand.Next(12, 18);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(8.5f, 8.5f);
                Color color = Main.rand.NextBool() ? blueColor : cyanColor;
                float scale = Main.rand.NextFloat(0.6f, 1.3f);
                int lifetime = Main.rand.Next(25, 45);
                new Spark(hitPoint, sparkVel, color, lifetime, scale, 0.95f).Spawn();
            }

            if (State != STATE_FLY)
                return;
            Splash(Projectile.Center);
            State = STATE_STUCK;
            stateTimer = 0;
            StuckTarget = target.whoAmI;
            stuckOffset = Projectile.Center - target.Center;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.damage = (int)(Projectile.damage * 0.35f);
            Projectile.netUpdate = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (State != STATE_FLY)
                return false;
            Splash(Projectile.Center);
            State = STATE_STUCK;
            stateTimer = 0;
            StuckTarget = -1;
            Projectile.Center += oldVelocity.SafeNormalize(Vector2.Zero) * 6f;
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = oldVelocity.ToRotation() + MathHelper.PiOver2;
            Projectile.friendly = false;
            Projectile.netUpdate = true;
            return false;
        }
        private void Splash(Vector2 impact)
        {
            SoundEngine.PlaySound(SoundID.Splash with { Volume = 1f, Pitch = -0.2f }, impact);
            for (int i = 0; i < 26; i++)
            {
                Dust d = Dust.NewDustPerfect(impact, DustID.Water, Main.rand.NextVector2Circular(5f, 5f) - new Vector2(0f, 2f), 0, default, Main.rand.NextFloat(1.2f, 2f));
                d.noGravity = Main.rand.NextBool();
            }
            if (Main.myPlayer != Projectile.owner)
                return;
            float c = Charge;
            int waveDamage = (int)(Projectile.damage * (0.45f + 0.45f * c));
            float radius = 130f + 230f * c;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), impact, Vector2.Zero, ModContent.ProjectileType<StormWave>(), waveDamage, 8f, Projectile.owner, radius, c);
            Player player = Main.player[Projectile.owner];
            if (player.active && !player.dead)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<StormWave>(), waveDamage / 3, 4f, Projectile.owner, 70f + 60f * c, c * 0.6f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            if (State != STATE_STUCK)
            {
                for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero)
                        continue;
                    float f = 1f - i / (float)Projectile.oldPos.Length;
                    Color c = new Color(70, 140, 255, 0) * (0.35f * f * (0.4f + 0.6f * Charge));
                    Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    Main.EntitySpriteDraw(tex, pos, null, c, Projectile.oldRot[i], origin, 1f, SpriteEffects.None, 0);
                }
            }
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            return false;
        }
    }
}
