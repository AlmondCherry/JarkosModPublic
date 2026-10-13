using Jarkos.trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class Dante_sword_disk : ModProjectile
    {
        public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.light = 0.85f;
            Projectile.tileCollide = false;
            Projectile.width = 125;
            Projectile.height = 125;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
			Projectile.scale = 1f;
			Projectile.alpha = 120;
            Projectile.light = 1f;
        }
        public override void AI()
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.ai[1] = Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
            }
            Projectile.rotation += Projectile.direction * -0.5f;
            Projectile.ai[0]++;
            const int maxTime = 45;
            Vector2 DistanceOffset = new Vector2(800 * (float)Math.Sin(Projectile.ai[0] * Math.PI / maxTime), 0).RotatedBy(Projectile.velocity.ToRotation());
			DistanceOffset = DistanceOffset.RotatedBy(Projectile.ai[1] / (maxTime / 1.2f));
            Projectile.Center = Main.player[Projectile.owner].Center + DistanceOffset;
            if (Projectile.ai[0] > maxTime)
                Projectile.Kill();
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Projectile.Distance(new Vector2(targetHitbox.Center.X, targetHitbox.Center.Y)) < 101;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
                if (Projectile.owner == Main.myPlayer)
                {
                    SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
                }
                Projectile.netUpdate = true;
        }
        public Primitivedraw TrailDrawer { get; private set; } = null;
        public float WidthFunction(float completionRatio)
        {
            float width_trail = 1f;
            float baseWidth = Projectile.scale * (Projectile.width / 4) * width_trail;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }
        public Color ColorFunction(float completionRatio)
        {
            return Color.Lerp(new Color(213, 213, 213, 125), Color.Transparent, completionRatio) * 0.7f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            TrailDrawer ??= new Primitivedraw(WidthFunction, ColorFunction, GameShaders.Misc["Jarkos:LaserShader"]);
            GameShaders.Misc["Jarkos:LaserShader"].SetShaderTexture(Trailreg.test);
            TrailDrawer.DrawPrims(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 220);
            return true;
        }
    }
}
