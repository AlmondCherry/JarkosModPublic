using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using jarkos.particles;
namespace jarkos
{
	public class Jarkos_globalprojectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		public bool hitted_by_LB_Penta = false;
        public override void SetDefaults(Projectile projectile)
        {
			var instance = ModContent.GetInstance<Jarkos_player>();
            if(projectile.type == ProjectileID.RubyBolt || projectile.type == ProjectileID.DiamondBolt)
			{
				projectile.tileCollide = false;
			}
			if(projectile.type == ProjectileID.SporeCloud)
			{
				projectile.tileCollide = true;
			}
        }
        public override void AI(Projectile projectile)
		{
			Player player = Main.player[projectile.owner];

			if (projectile.type is ProjectileID.AmethystBolt or ProjectileID.TopazBolt or ProjectileID.TopazBolt or ProjectileID.SapphireBolt or ProjectileID.EmeraldBolt or ProjectileID.RubyBolt or ProjectileID.DiamondBolt or ProjectileID.AmberBolt && player.GetModPlayer<Jarkos_player>().gemstaffs_buff == true)
			{
				HomingAI(projectile);
			}
		}
		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			if (projectile.type == ProjectileID.SporeCloud)
			{
				if (projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f && projectile.type == ProjectileID.SporeCloud)
				{
					projectile.velocity.X = oldVelocity.X * 0.55f;
				}
				if (projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f && projectile.type == ProjectileID.SporeCloud)
				{
					projectile.velocity.Y = oldVelocity.Y * 0.55f;
				}
				return false;
			}
			return true;
		}
		private void HomingAI(Projectile projectile)
		{
			Player player = Main.player[projectile.owner];
			NPC targetNPC = null;
			float maxDetectRadius = 500f;
            if (targetNPC == null)
            {
            	float sqrMaxDetectDistance = maxDetectRadius * maxDetectRadius;
            	for (int k = 0; k < Main.maxNPCs; k++)
            	{
                NPC target = Main.npc[k];

                if (target.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        targetNPC = target;
                    }
                }
            	}
            }
			if (projectile.timeLeft !< projectile.timeLeft * 0.90f) 
			{
				projectile.velocity /= 3.5f;
			}

			if(targetNPC != null) 
			{
				projectile.velocity *= 0.94f;
            	projectile.velocity += (targetNPC.Center - projectile.Center).SafeNormalize(Vector2.Zero) / 2;
			}

			else if(projectile.timeLeft !< projectile.timeLeft * 0.95f)
			{
				Vector2 speed = Vector2.Normalize(Main.MouseWorld - player.position) * 16f;
				projectile.velocity += speed;
				return;
			}
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
		}
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
			Player player = Main.player[projectile.owner];

            if(player.GetModPlayer<Jarkos_player>().bloodyamulet == true && projectile.DamageType == DamageClass.Melee)
			{
				player.Heal(Main.rand.Next(1, 5));
			}
			if (player.GetModPlayer<Jarkos_player>().goldengorn == true && projectile.DamageType == DamageClass.Melee)
			{
				player.GetModPlayer<Jarkos_player>().goldengornstrength++;
			}
			else
			{
				player.GetModPlayer<Jarkos_player>().goldengornstrength = 1;
			}
			if (projectile.type == ModContent.ProjectileType<Lightburn_beam>())
			{
				Jarkos_npc globalNPC = target.GetGlobalNPC<Jarkos_npc>();
				hitted_by_LB_Penta = globalNPC.LB_Penta_target;
				globalNPC.LB_Penta_target = true;
			}
			if(projectile.type == ProjectileID.CrystalPulse)
			{
				float size = Main.rand.NextFloat(0.75f, 0.85f);
				new Circleedge_reducing(target.Center, Vector2.Zero, Color.MediumPurple, 300, size * 2f, 2f, 0f).Spawn();

				for(int i = 0; i < 3; i++)
				{
					Vector2 pos = projectile.Center;
					Vector2 vel = projectile.velocity.RotatedByRandom(6);
					Projectile.NewProjectile(null, position: pos, vel, ProjectileID.StarWrath, projectile.damage / 4, 1, player.whoAmI);
				}
			}
        }
    }
}
