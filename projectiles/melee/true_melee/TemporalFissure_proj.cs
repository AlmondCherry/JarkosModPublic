using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class TemporalFissure_proj : ModProjectile
    {
        private const int MaxLifetime = 180;
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity *= 0.95f;
            Projectile.alpha = (int)(Math.Sin(Projectile.timeLeft * 0.2f) * 100 + 50);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.Next(-25, 25), Main.rand.Next(-25, 25)), DustID.GemAmethyst, new Vector2(0, Main.rand.NextFloat(-0.5f, 0.5f)), 200, default, 1.5f);
                dust.noGravity = true;
            }
            CheckFissureIntersections();
        }
        private void CheckFissureIntersections()
        {
            foreach (Projectile other in Main.projectile)
            {
                if (other.active && other.type == Projectile.type && other.whoAmI != Projectile.whoAmI)
                {
                    float distance = Vector2.Distance(Projectile.Center, other.Center);
                    if (distance < 80f)
                    {
                        SoundEngine.PlaySound(SoundID.Item92, Projectile.Center);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TemporalFissure_anomaly>(), Projectile.damage * 2, Projectile.knockBack, Projectile.owner);
                        other.Kill();
                        Projectile.Kill();
                    }
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0 && Main.rand.NextFloat() < 0.2f)
            {
                CreateEchoNPC(target);
            }
        }
        private void CreateEchoNPC(NPC original)
        {
            NPC echo = Main.npc[NPC.NewNPC(Projectile.GetSource_FromThis(), (int)original.Center.X, (int)original.Center.Y, original.type)];
            echo.life = (int)(echo.lifeMax * 0.3f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/TemporalFissure_proj").Value;
            Color color = new Color(180, 70, 255, 0) * (Projectile.alpha / 255f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
     }
