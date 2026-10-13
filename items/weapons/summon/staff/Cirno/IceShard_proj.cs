using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.particles.storm;
using ReLogic.Content;

namespace jarkos.items.weapons.summon.staff.Cirno
{
	public class CirnoIceShard_proj : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/Cirno/IceShard";

		private float currentSpeed = 4f;
		private bool isLevel5Variant => Projectile.ai[0] == 1f;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
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
			if (isLevel5Variant)
			{
				Projectile.scale = 2.0f;
			}

			currentSpeed = MathHelper.Clamp(currentSpeed + 0.35f, 4f, 22f);
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			NPC target = FindClosestNPC(700f);
			if (target != null)
			{
				Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * currentSpeed, 0.09f);
			}
			else
			{
				Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * currentSpeed;
			}

			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, isLevel5Variant ? 1.4f : 1f);
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
			target.AddBuff(BuffID.Frostburn, 180);
			TriggerColdExplosion();
		}

		public override void OnKill(int timeLeft)
		{
			TriggerColdExplosion();
		}

		private bool exploded = false;
		private void TriggerColdExplosion()
		{
			if (exploded) return;
			exploded = true;

			Vector2 pos = Projectile.Center;
			Color cyanColor = new Color(60, 220, 255);
			float particleMult = isLevel5Variant ? 1.8f : 1.0f;

			new Circular_glow(pos, Vector2.Zero, cyanColor, 22, isLevel5Variant ? 0.95f : 0.65f, 0.9f).Spawn();

			int pCount = (int)(10 * particleMult);
			for (int i = 0; i < pCount; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
				new Fire_particle(pos, vel, cyanColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.3f, 0.55f) * particleMult, 0.9f, 0.05f).Spawn();
			}

			for (int i = 0; i < pCount; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
				new Electric_particle(pos, vel, cyanColor, Main.rand.Next(18, 32), Main.rand.NextFloat(0.25f, 0.5f) * particleMult, 0.9f, 0.92f).Spawn();
			}

			for (int i = 0; i < (int)(12 * particleMult); i++)
			{
				Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, Main.rand.NextVector2Circular(6f, 6f), 0, default, 1.3f);
				d.noGravity = true;
			}

			SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.8f, Pitch = 0.2f }, pos);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;
			string texPath = isLevel5Variant ? "jarkos/items/weapons/summon/staff/Cirno/BiggerIceShard" : "jarkos/items/weapons/summon/staff/Cirno/IceShard";
			Texture2D tex = ModContent.Request<Texture2D>(texPath, AssetRequestMode.ImmediateLoad).Value;
			if (tex == null) return false;

			Vector2 origin = tex.Size() / 2f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			float scale = isLevel5Variant ? 2.0f : 1.0f;

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero) continue;
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				Color trailColor = new Color(80, 220, 255) * (progress * 0.5f);
				Vector2 oldDrawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				sb.Draw(tex, oldDrawPos, null, trailColor, Projectile.oldRot[i], origin, scale * (0.6f + progress * 0.4f), SpriteEffects.None, 0f);
			}

			sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
