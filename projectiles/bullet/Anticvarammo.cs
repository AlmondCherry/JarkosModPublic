using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace jarkos.projectiles.bullet
{
	public class Anticvarammo : ModProjectile
	{
		private int mode = 1;
		private int speedtimer = 0;
		NPC targetNPC = null;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 2;
			Projectile.height = 30;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
		      mode--;
		  if (mode < 0)
		    {
		        Projectile.Kill();
		    }
		    else
		    {
				Projectile.damage -= Projectile.damage / 2;
		        Projectile.ai[0] += 0.1f;
		        if (Projectile.velocity.X != oldVelocity.X)
		        {
		           Projectile.velocity.X = -oldVelocity.X;
		        }
		        if (Projectile.velocity.Y != oldVelocity.Y)
		        {
	            Projectile.velocity.Y = -oldVelocity.Y;
		        }
		        Projectile.velocity *= 1f;
		        SoundEngine.PlaySound(SoundID.Item8, Projectile.position);
		    }
		    return false;
		}
        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
			float maxDetectRadius = 250f;
            if (targetNPC == null && mode < 1)
            {
                targetNPC = FindClosestNPC(maxDetectRadius);
                return;
            }
			if (mode < 1){
            Projectile.velocity += (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
			}
			else if(Projectile.timeLeft !< Projectile.timeLeft * 0.995f){
			Projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.position) * 26f;
			}
			if (mode < 1){
            Projectile.velocity /= 1.05f;
			}
        }
		public override void OnKill(int timeLeft)
        {
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        public NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC target = Main.npc[k];
                if (target.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }
            return closestNPC;
        }
    }
}
