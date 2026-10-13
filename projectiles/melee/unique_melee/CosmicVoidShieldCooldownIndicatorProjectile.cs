using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public class CosmicVoidShieldCooldownIndicatorProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/MagicPixel";
    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = false;
        Projectile.timeLeft = 1;
        Projectile.alpha = 255;
    }
    public override void AI()
    {
        Projectile.Kill();
    }
    public override bool PreDraw(ref Color lightColor) => false;
}
