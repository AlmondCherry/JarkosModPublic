using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using Terraria.GameContent.Drawing;
namespace jarkos.projectiles.magic.other
{
	public class Star01proj : ModProjectile
	{
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.damage = 1;
            Projectile.timeLeft = 600;
		}
		 public override void AI(){
         if (Main.rand.NextBool(1, 2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, 133, Scale: 0.2f);
            dust.velocity /= 1; }
            if (Main.rand.NextBool(1, 2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, 133, Scale: 0.4f);
            dust.velocity /= 10; }
        }
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings { PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox) }, Projectile.owner);
			hit.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
		}
}
}
