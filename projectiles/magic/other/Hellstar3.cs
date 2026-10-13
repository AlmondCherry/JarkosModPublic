using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;
using jarkos.projectiles.magic.scrolls;

namespace jarkos.projectiles.magic.other
{
	public class Hellstar3 : ModProjectile
	{
        public float revolutionAngle;
		public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
            Projectile.netImportant = true;
            Projectile.width = 42;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.light = 0.75f;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            Projectile.velocity *= 0.96f;

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 tocursor = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.rotation = tocursor.ToRotation() + MathHelper.PiOver2;

                if (owner.channel && !owner.noItems && !owner.CCed && !owner.dead)
                {
                    Projectile.ai[0]++;

                    if (Projectile.ai[0] == 25)
                    {
                        int fireball = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, tocursor * 8f, ModContent.ProjectileType<Fireball>(), Projectile.damage, 1f, Projectile.owner);
                        if (fireball >= 0 && fireball < Main.maxProjectiles)
                        {
                            Main.projectile[fireball].netUpdate = true;
                        }
                    }

                    if (Projectile.ai[0] > 40)
                    {
                        Projectile.Kill();
                    }

                    Projectile.timeLeft = 900;
                }
                else
                {
                    Projectile.Kill();
                }

                if (Projectile.ai[0] % 5 == 0 || Projectile.ai[0] == 25)
                {
                    Projectile.netUpdate = true;
                }
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 5)
            {
                Projectile.frame = 0;
            }
        }
    }
}
