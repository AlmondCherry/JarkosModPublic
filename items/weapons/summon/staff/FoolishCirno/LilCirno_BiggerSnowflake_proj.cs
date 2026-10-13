using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;

namespace jarkos.items.weapons.summon.staff.FoolishCirno
{
	public class LilCirno_BiggerSnowflake_proj : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/FoolishCirno/LilCirno_BiggerSnowflake";

		private float currentSpeed = 3f;

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 360;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.rotation += 0.25f * Projectile.direction;

			if (currentSpeed < 16f)
			{
				currentSpeed += 0.35f;
			}

			NPC target = FindTarget();
			if (target != null)
			{
				Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), targetDir, 0.12f) * currentSpeed;
			}
			else
			{
				Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * currentSpeed;
			}

			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, 1.2f);
				d.noGravity = true;
				d.velocity = Projectile.velocity * 0.15f;
			}
		}

		private NPC FindTarget()
		{
			float maxDist = 650f;
			NPC target = null;
			float closestDist = maxDist;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
				{
					float dist = Vector2.Distance(Projectile.Center, npc.Center);
					if (dist < closestDist)
					{
						closestDist = dist;
						target = npc;
					}
				}
			}
			return target;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.7f, Pitch = 0.1f }, Projectile.Center);

			new Circular_glow(Projectile.Center, Vector2.Zero, new Color(120, 220, 255), 35, 1.0f, 1.5f).Spawn();

			for (int i = 0; i < 28; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(5.5f, 5.5f);
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, vel, 100, default, Main.rand.NextFloat(1.4f, 2.2f));
				d.noGravity = true;
			}

			for (int i = 0; i < 18; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(4.0f, 4.0f);
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Snow, vel, 120, default, Main.rand.NextFloat(1.0f, 1.6f));
				d.noGravity = true;
			}

			float fogRadius = 140f;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && !npc.dontTakeDamage && !npc.boss && Vector2.Distance(Projectile.Center, npc.Center) <= fogRadius)
				{
					npc.velocity *= 0.5f;
					npc.AddBuff(BuffID.Chilled, 180);
					npc.AddBuff(BuffID.Frostburn, 180);
				}
			}
		}
	}
}
