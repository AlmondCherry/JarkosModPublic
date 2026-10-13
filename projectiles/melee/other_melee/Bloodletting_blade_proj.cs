using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.other_melee
{
    public class Bloodletting_blade_proj : ModProjectile
    {
        public int heal_amount = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.light = 0.85f;
            Projectile.tileCollide = false;
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.ai[1] == 0)
            {
                Projectile.ai[1] = Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
            }
            Projectile.rotation += Projectile.direction * -0.9f;
            Projectile.ai[0]++;
            const int maxTime = 25;
            Vector2 DistanceOffset = new Vector2(800 * (float)Math.Sin(Projectile.ai[0] * Math.PI / maxTime), 0).RotatedBy(Projectile.velocity.ToRotation());
			DistanceOffset = DistanceOffset.RotatedBy(Projectile.ai[1] / (maxTime / 1.2f));
            Projectile.Center = Main.player[Projectile.owner].Center + DistanceOffset;
            if (Projectile.ai[0] > maxTime)
            {
                if(heal_amount > 1)
                {
                    if(heal_amount > 10)
                    {
                        heal_amount = 10;
                    }
                    player.Heal(heal_amount / 2);
                }
                Projectile.Kill();
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Projectile.Distance(new Vector2(targetHitbox.Center.X, targetHitbox.Center.Y)) < 51;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            heal_amount++;
                if (Projectile.owner == Main.myPlayer)
                {
                    SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
                }
                Projectile.netUpdate = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Color color26 = Color.Red;
            color26 = Projectile.GetAlpha(color26);
            for (float i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i += 0.33f)
            {
                int max0 = Math.Max((int)i - 1, 0);
                Vector2 center = Vector2.Lerp(Projectile.oldPos[(int)i], Projectile.oldPos[max0], 1 - i % 1);
                if (i < 4)
                {
                    Color color27 = color26;
                    color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - (i * 1.5f)) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    Main.EntitySpriteDraw(texture2D13, center + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, Projectile.oldRot[(int)i], origin2, Projectile.scale, SpriteEffects.None, 0);
                }
                 }
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
