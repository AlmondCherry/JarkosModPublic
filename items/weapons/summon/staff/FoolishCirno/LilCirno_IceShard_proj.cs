using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.FoolishCirno
{
	public class LilCirno_IceShard_proj : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/FoolishCirno/LilCirno_IceShard";

		private bool isInitialized = false;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			if (!isInitialized)
			{
				isInitialized = true;
				if (Projectile.ai[0] == 1f)
				{
					Projectile.scale = 2.0f;
					Projectile.width = (int)(Projectile.width * 2.0f);
					Projectile.height = (int)(Projectile.height * 2.0f);
					Projectile.damage = (int)(Projectile.damage * 3.0f);

					SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.85f, Pitch = 0.5f }, Projectile.Center);
					for (int i = 0; i < 12; i++)
					{
						Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.5f);
						d.noGravity = true;
					}
				}
			}

			NPC target = FindTarget();
			if (target != null)
			{
				Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), targetDir, 0.14f) * 12f;
			}

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, Projectile.scale * 0.9f);
				d.noGravity = true;
				d.velocity = Projectile.velocity * 0.1f;
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
			SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.1f }, Projectile.Center);
			int count = Projectile.ai[0] == 1f ? 16 : 8;
			for (int i = 0; i < count; i++)
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(4f * Projectile.scale, 4f * Projectile.scale), 0, default, 1.2f * Projectile.scale);
				d.noGravity = true;
			}
		}
	}
}
