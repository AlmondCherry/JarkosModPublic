using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class PortalShaderTest : ModProjectile
    {
        private const int MaxLifetime = 180;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = MaxLifetime;
            Projectile.aiStyle = -1;
        }
        public override void AI()
        {
            Projectile.rotation += 0.03f;
            int age = MaxLifetime - Projectile.timeLeft;
            float opacity = 1f;
            if (age < 30)
            {
                opacity = age / 30f;
            }
            else if (Projectile.timeLeft < 30)
            {
                opacity = Projectile.timeLeft / 30f;
            }
            Projectile.Opacity = opacity;
            float progress = (float)age / MaxLifetime;
            float intensity = opacity;
            if (!PortalShaderTestSystem.IsOnScreen(Projectile.Center, padding: 300f))
            {
                PortalShaderTestSystem.Deactivate();
            }
            else
            {
                PortalShaderTestSystem.Activate(Projectile.Center);
                PortalShaderTestSystem.Update(Projectile.Center, progress: progress, intensity: intensity, opacity: opacity, primary: new Color(155, 60, 255), secondary: new Color(60, 220, 255));
            }
            Lighting.AddLight(Projectile.Center, 0.6f * opacity, 0.2f * opacity, 0.9f * opacity);
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(120f, 120f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.PurpleTorch, -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f), Scale: Main.rand.NextFloat(1.0f, 1.5f));
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (CountActiveProjectiles() <= 1)
            {
                PortalShaderTestSystem.Deactivate();
            }
        }
        private int CountActiveProjectiles()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Projectile.type)
                {
                    count++;
                }
            }
            return count;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + 0.06f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8.0f);
            float drawScale = Projectile.scale * pulse;
            Color glowColor = new Color(155, 60, 255, 0) * Projectile.Opacity * 0.4f;
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor, -Projectile.rotation * 0.5f, origin, drawScale * 1.2f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White * Projectile.Opacity, Projectile.rotation, origin, drawScale, SpriteEffects.None, 0);
            return false;
        }
    }
}
