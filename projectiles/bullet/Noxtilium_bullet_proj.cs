using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using jarkos.particles;

namespace jarkos.projectiles.bullet
{
	public class Noxtilium_bullet_proj : ModProjectile
	{
        private static readonly Color NightColor = new Color(150, 100, 255);
        private static readonly Color DayColor = new Color(100, 100, 120);
        private bool IsNightTime => !Main.dayTime;
        private float DamageMultiplier => IsNightTime ? 1.5f : 0.7f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;
            Projectile.light = 0.3f;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (IsNightTime)
            {
                NightTimeEffects();
            }
            else
            {
                DayTimeEffects();
            }
        }
        private void NightTimeEffects()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            if (Main.rand.NextBool(4))
            {
                Dust starDust = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.5f, 1, 1, DustID.PurpleCrystalShard, 0f, 0f, 150, default, 0.8f);
                starDust.noGravity = true;
                starDust.velocity = Projectile.velocity * 0.1f;
            }
            Lighting.AddLight(Projectile.Center, NightColor.ToVector3() * 0.6f);
        }
        private void DayTimeEffects()
        {
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
            Lighting.AddLight(Projectile.Center, DayColor.ToVector3() * 0.2f);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= DamageMultiplier;
            if (IsNightTime)
            {
                modifiers.CritDamage += 0.25f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<NoxtiliumBulletGlobalNPC>().OnHitByBullet();

            int starCount = Main.rand.Next(1, 2);
            for (int i = 0; i < starCount; i++)
            {
                float arcAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 starVel = arcAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                float curveDir = Main.rand.NextBool() ? Main.rand.NextFloat(0.06f, 0.12f) : Main.rand.NextFloat(-0.12f, -0.06f);
                Color purpleStarColor = Color.Lerp(new Color(180, 70, 255), new Color(230, 140, 255), Main.rand.NextFloat());

                new CurvedStarParticle(target.Center, starVel, purpleStarColor, Main.rand.Next(30, 45), Main.rand.NextFloat(0.05f, 0.2f), 1f, curveDir, Main.rand.NextFloat(-0.15f, 0.15f)).Spawn();
            }

            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/NoxtiliumAmmoImpact") with {Pitch = Main.rand.NextFloat(-0.2f, 0.2f)}, target.position);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, IsNightTime ? DustID.PurpleTorch : DustID.Smoke, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color trailColor = IsNightTime ? NightColor : DayColor;
            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
                Color color = trailColor * progress * 0.5f;

                float scale = Projectile.scale * progress;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
            return true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (IsNightTime)
            {
                return new Color(200, 150, 255, 200);
            }
            return new Color(150, 150, 160, 200);
        }
    }
}
