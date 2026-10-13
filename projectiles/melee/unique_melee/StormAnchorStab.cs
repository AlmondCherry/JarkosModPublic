using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.systems;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace jarkos.projectiles.melee.unique_melee
{
    public class StormAnchorStab : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/StormAnchor";
        private Player Owner => Main.player[Projectile.owner];
        private int Variant => (int)Projectile.ai[0];
        private float Reach => Projectile.ai[1];
        private float Wobble => Projectile.ai[2];
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 16;
            Projectile.hide = false;
        }
        public override void AI()
        {
            Player player = Owner;
            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }
            if (player.itemAnimation <= 1)
            {
                Projectile.Kill();
                return;
            }
            player.heldProj = Projectile.whoAmI;
            Projectile.timeLeft = 4;
            float t = 1f - player.itemAnimation / (float)player.itemAnimationMax;
            float ext = Extension(t);
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
            float sway = Wobble * (float)Math.Sin(t * MathHelper.TwoPi);
            dir = dir.RotatedBy(sway);
            player.ChangeDir(dir.X >= 0f ? 1 : -1);
            player.itemRotation = (float)Math.Atan2(dir.Y * player.direction, dir.X * player.direction);
            Projectile.Center = player.MountedCenter + dir * (8f + ext * Reach);
            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, dir.ToRotation() - MathHelper.PiOver2);
            if (ext > 0.55f && Main.rand.NextBool(2))
            {
                Vector2 tip = Projectile.Center + dir * 38f;
                Dust d = Dust.NewDustPerfect(tip, DustID.Water, dir * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.6f, 0.6f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.9f, 1.4f);
            }
            if (ext > 0.8f && Main.rand.NextBool(6))
            {
                Dust s = Dust.NewDustPerfect(Projectile.Center + dir * 34f, DustID.Electric, Main.rand.NextVector2Circular(1.2f, 1.2f), 100, default, 0.7f);
                s.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center + dir * 30f, 0.15f, 0.3f, 0.55f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Owner;
            if (player.active && player.whoAmI == Main.myPlayer)
            {
                player.GetModPlayer<PurgatoryPlayer>().AddShake(3.5f, 8);
            }

            SoundEngine.PlaySound(SoundID.Item171 with { Volume = 0.85f, Pitch = Main.rand.NextFloat(-0.1f, 0.2f) }, target.Center);

            Vector2 hitPoint = target.active ? Vector2.Clamp(Projectile.Center, target.Hitbox.TopLeft(), target.Hitbox.BottomRight())
                : Projectile.Center;

            Color blueColor = new Color(30, 90, 240);
            Color cyanColor = new Color(90, 210, 255);

            int sparkCount = Main.rand.Next(10, 16);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(7.5f, 7.5f) + Projectile.velocity * 0.1f;
                Color sparkColor = Main.rand.NextBool() ? blueColor : cyanColor;
                float scale = Main.rand.NextFloat(0.6f, 1.2f);
                int lifetime = Main.rand.Next(25, 45);
                new Spark(hitPoint, sparkVel, sparkColor, lifetime, scale, 0.95f).Spawn();
            }
        }
        private float Extension(float t)
        {
            switch (Variant)
            {
                case 1:
                    if (t < 0.4f)
                    {
                        float p = t / 0.4f;
                        return 1f - (1f - p) * (1f - p) * (1f - p);
                    }
                    if (t < 0.55f)
                        return 1f;
                    float r = (t - 0.55f) / 0.45f;
                    return 1f - r * r;
                case 2:
                    if (t < 0.18f)
                        return -0.22f * (float)Math.Sin(t / 0.18f * MathHelper.PiOver2);
                    float tt = (t - 0.18f) / 0.82f;
                    return (float)Math.Sin(MathHelper.Pi * tt);
                default:
                    return (float)Math.Sin(MathHelper.Pi * t);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            return false;
        }
        public override void CutTiles()
        {
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Terraria.Utils.PlotTileLine(Projectile.Center, Projectile.Center + dir * 44f, 4f, DelegateMethods.CutTiles);
        }
    }
}
