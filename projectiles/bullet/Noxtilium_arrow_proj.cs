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
	public class Noxtilium_arrow_proj : ModProjectile
	{
		private static readonly Color NightColor = new Color(150, 100, 255);
        private static readonly Color DayColor = new Color(100, 100, 120);
        private bool IsNightTime => !Main.dayTime;
        private ref float TargetWhoAmI => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 1200;
            Projectile.extraUpdates = 1;
            Projectile.arrow = true;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (IsNightTime)
            {
                NightTimeAI();
            }
            else
            {
                DayTimeAI();
            }
            CreateTrailEffects();
        }
        private void NightTimeAI()
        {
            NightVisualEffects();
        }
        private void DayTimeAI()
        {
            Projectile.velocity.Y += 0.15f;
            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }
            Projectile.velocity.X *= 0.99f;
            DayVisualEffects();
        }
        private void NightVisualEffects()
        {
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, default, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
            if (Main.rand.NextBool(8))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 1, 1, DustID.Enchanted_Pink, 0f, 0f, 0, default, 0.6f);
                Spark.noGravity = true;
                Spark.velocity = -Projectile.velocity * 0.2f;
            }
            if (TargetWhoAmI >= 0)
            {
                float rotationAngle = Main.GameUpdateCount * 0.15f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = rotationAngle + MathHelper.Pi * i;
                    Vector2 offset = angle.ToRotationVector2() * 8f;
                    Dust orbit = Dust.NewDustDirect(Projectile.position + offset, 1, 1, DustID.Shadowflame, 0f, 0f, 100, default, 0.6f);
                    orbit.noGravity = true;
                    orbit.velocity = Projectile.velocity * 0.5f;
                }
            }
            Lighting.AddLight(Projectile.Center, NightColor.ToVector3() * 0.7f);
        }
        private void DayVisualEffects()
        {
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, default, 0.5f);
                dust.noGravity = true;
                dust.velocity *= 0.1f;
            }
            Lighting.AddLight(Projectile.Center, DayColor.ToVector3() * 0.15f);
        }
        private void CreateTrailEffects()
        {
            if (Main.rand.NextBool(3) && IsNightTime)
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                Dust trail = Dust.NewDustDirect(trailPos, 1, 1, DustID.PurpleCrystalShard, Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f), 100, default, Main.rand.NextFloat(0.4f, 0.8f));
                trail.noGravity = true;
                trail.fadeIn = 1.1f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var gNPC = target.GetGlobalNPC<NoxtiliumInfectedGlobalNPC>();
            gNPC.isInfected = true;
            gNPC.infectedTimer = 360;
            gNPC.sourceDamage = Projectile.damage;
            gNPC.ownerIndex = Projectile.owner;

            int starCount = Main.rand.Next(2, 4);
            for (int i = 0; i < starCount; i++)
            {
                float arcAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 starVel = arcAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                float curveDir = Main.rand.NextBool() ? Main.rand.NextFloat(0.06f, 0.12f) : Main.rand.NextFloat(-0.12f, -0.06f);
                Color purpleStarColor = Color.Lerp(new Color(180, 70, 255), new Color(230, 140, 255), Main.rand.NextFloat());

                new CurvedStarParticle(target.Center, starVel, purpleStarColor, Main.rand.Next(30, 45), Main.rand.NextFloat(0.1f, 0.3f), 1f, curveDir, Main.rand.NextFloat(-0.15f, 0.15f)).Spawn();
            }

            if (IsNightTime)
            {
                CreateNightHitEffect(target.Center);
            }
            else
            {
                CreateDayHitEffect(target.Center);
            }
        }
        private void CreateNightHitEffect(Vector2 position)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/NoxtiliumAmmoImpact") with {Pitch = Main.rand.NextFloat(-0.2f, 0.2f)}, position);
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.PurpleTorch, velocity.X, velocity.Y, 100, default, 1.4f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6 * i;
                Vector2 velocity = angle.ToRotationVector2() * 3f;
                Dust crystal = Dust.NewDustDirect(position, 1, 1, DustID.PurpleCrystalShard, velocity.X, velocity.Y, 0, default, 1.2f);
                crystal.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Dust magic = Dust.NewDustDirect(position, 1, 1, DustID.Enchanted_Pink, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, default, 0.8f);
                magic.noGravity = true;
                magic.fadeIn = 1.3f;
            }
        }
        private void CreateDayHitEffect(Vector2 position)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/NoxtiliumAmmoImpact") with {Pitch = Main.rand.NextFloat(-0.2f, 0.2f)}, position);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.Smoke, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 0.8f);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            int dustType = IsNightTime ? DustID.PurpleTorch : DustID.Smoke;
            int dustCount = IsNightTime ? 10 : 4;

            for (int i = 0; i < dustCount; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, default, IsNightTime ? 1f : 0.6f);
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
                float alpha = progress * (IsNightTime ? 0.6f : 0.3f);
                Color color = trailColor * alpha;
                float scale = Projectile.scale * (0.5f + progress * 0.5f);
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
            return true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (IsNightTime)
            {
                return new Color(220, 180, 255, 200);
            }
            return new Color(140, 140, 150, 200);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (IsNightTime)
            {
                modifiers.FinalDamage *= 1.2f;
            }
            else
            {
                modifiers.FinalDamage *= 0.8f;
            }
        }
	}
}
