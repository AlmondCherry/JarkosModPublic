using jarkos;
using jarkos.projectiles.mecha;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
namespace jarkos.projectiles.melee.other_melee
{
	public class Purple_squeze : ModProjectile
	{
		private float rand2 = Main.rand.NextFloat(-180, 180);
		private float randSize = Main.rand.NextFloat(0.02f, 0.15f);
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.scale = 1.333f;
			Projectile.damage = 5;
		}
		public override void AI()
        {
			Projectile.velocity.Y += 0.09f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
            if (Projectile.owner == Main.myPlayer)
            {
	        for (int i = 0; i < 2; i++)
	            {
				Vector2 y2 = Main.rand.NextVector2Unit(rand2, rand2);
	            }
            }
		}
		public override void PostAI() {
            if (Main.rand.NextBool()) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.DarkCelestial);
				dust.noGravity = true;
				dust.scale = 1.6f;
			}
		}
	}
}
