using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.buffs.debuff;
using jarkos.particles;
using Terraria.Audio;
namespace jarkos.projectiles.bullet
{
	public class Black_feather_arrow : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (Main.rand.NextBool(2) && Projectile.timeLeft > 590)
			{
				new Spark(Projectile.Center, Projectile.velocity * 0.75f, Color.White, 120, 0.5f + (600 - Projectile.timeLeft) * 0.002f, 2f, 0f).Spawn();
			}
        }
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 5; i++)
			{
				Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.WhiteTorch, speed * 2.5f, Scale: 3.5f);
				d.noGravity = true;
			}
			var global_npc = target.GetGlobalNPC<Jarkos_npc>();
			if (!target.HasBuff(ModContent.BuffType<Feather_black_debuff>()) && !global_npc.shadow_pact_debuff_already_used)
			{
				target.AddBuff(ModContent.BuffType<Feather_black_debuff>(), 200);
			}
			if (!target.HasBuff(ModContent.BuffType<Feather_black_debuff>()) && global_npc.shadow_pact_debuff_already_used && target.boss && global_npc.cooldown_shadow_pact > 1200)
			{
				target.AddBuff(ModContent.BuffType<Feather_black_debuff>(), 200);
			}
			if (!target.HasBuff(ModContent.BuffType<Feather_black_debuff>()) && target.type == NPCID.TargetDummy)
			{
				target.AddBuff(ModContent.BuffType<Feather_black_debuff>(), 200);
			}
        }

		public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}
