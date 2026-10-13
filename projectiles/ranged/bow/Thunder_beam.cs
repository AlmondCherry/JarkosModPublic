using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Shaders;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using jarkos.systems;
namespace jarkos.projectiles.ranged.bow
{
	public class Thunder_beam : ModProjectile
	{
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}
		public override void SetDefaults() {
            Projectile.width = 36;
            Projectile.height = 36;
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.scale = 1f;
            Projectile.penetrate = 1;
            Projectile.light = 0.9f;
            Projectile.timeLeft = 50;
		}
        public override void OnKill(int timeLeft)
        {
            Vector2 start = Projectile.position += new Vector2(-3f, 0f);
            Vector2 end = Projectile.position += new Vector2(3f, -525f);
            LightningSystemLaser.MakeDust(start, end, 226, 0.75f, 80f, 1);
        }
        public override void AI(){
        }
    }
}
