using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.particles.storm;

namespace jarkos.items.weapons.summon.staff.Cirno
{
	public class BiggerSnowflake_proj : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/Cirno/BiggerSnowflake";

		private float currentSpeed = 3f;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			currentSpeed = MathHelper.Clamp(currentSpeed + 0.25f, 3f, 15f);
			Projectile.rotation += 0.12f;

			NPC target = FindClosestNPC(650f);
			if (target != null)
			{
				Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * currentSpeed, 0.07f);
			}
			else
			{
				Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * currentSpeed;
			}

			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, 1f);
				d.noGravity = true;
				d.velocity *= 0.2f;
			}
		}

		private NPC FindClosestNPC(float maxDistance)
		{
			NPC closest = null;
			float closestDist = maxDistance;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
				{
					float dist = Vector2.Distance(Projectile.Center, npc.Center);
					if (dist < closestDist)
					{
						closestDist = dist;
						closest = npc;
					}
				}
			}
			return closest;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			ExplodeAoE();
		}

		public override void OnKill(int timeLeft)
		{
			ExplodeAoE();
		}

		private bool exploded = false;
		private void ExplodeAoE()
		{
			if (exploded) return;
			exploded = true;

			Vector2 pos = Projectile.Center;

			float aoeRadius = 150f;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && !npc.dontTakeDamage && Vector2.Distance(pos, npc.Center) <= aoeRadius)
				{
					npc.AddBuff(BuffID.Frostburn, 180);
					if (Projectile.owner == Main.myPlayer)
					{
						npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, DamageClass.Summon);
					}
				}
			}

			Color cyanColor = new Color(60, 220, 255);
			for (int i = 0; i < 8; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
				new Fire_particle(pos, vel, cyanColor, Main.rand.Next(20, 32), Main.rand.NextFloat(0.35f, 0.55f), 0.9f, 0.05f).Spawn();
			}

			for (int i = 0; i < 8; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
				new Electric_particle(pos, vel, cyanColor, Main.rand.Next(18, 30), Main.rand.NextFloat(0.3f, 0.5f), 0.9f, 0.92f).Spawn();
			}

			new Circular_glow(pos, Vector2.Zero, cyanColor, 20, 0.65f, 0.9f).Spawn();
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.3f }, pos);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = tex.Size() / 2f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero) continue;
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				Color trailColor = new Color(80, 200, 255) * (progress * 0.45f);
				Vector2 oldDrawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				sb.Draw(tex, oldDrawPos, null, trailColor, Projectile.oldRot[i], origin, Projectile.scale * (0.6f + progress * 0.4f), SpriteEffects.None, 0f);
			}

			sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
