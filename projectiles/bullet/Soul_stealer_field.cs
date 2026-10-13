using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.bullet
{
    public class Soul_stealer_field : ModProjectile
    {
        public int DroppedSouls => (int)Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1800;
            Projectile.scale = 2f;
            Projectile.netImportant = true;
        }
    }
}
