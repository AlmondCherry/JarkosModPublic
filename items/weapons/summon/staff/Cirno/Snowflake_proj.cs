using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.Cirno
{
	public class Snowflake_proj : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/Cirno/Snowflake";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			float speed = Projectile.velocity.Length();
			Projectile.rotation += speed * 0.04f * (Projectile.velocity.X >= 0 ? 1 : -1);

			Player player = Main.player[Projectile.owner];
			NPC target = FindTarget(player);

			if (target != null)
			{
				Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
				float currentSpeed = MathHelper.Clamp(speed, 6f, 14f);
				
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * currentSpeed, 0.09f);
			}

			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, 0.9f);
				d.noGravity = true;
				d.velocity *= 0.2f;
			}
		}

		private NPC FindTarget(Player player)
		{
			float maxDetectDist = 650f;
			NPC target = null;

			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) < maxDetectDist)
				{
					return npc;
				}
			}

			float closestDist = maxDetectDist;
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

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Frostburn, 120);

			SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.4f }, Projectile.Center);
			for (int i = 0; i < 6; i++)
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.1f);
				d.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.4f, Pitch = 0.2f }, Projectile.Center);
			for (int i = 0; i < 5; i++)
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1f);
				d.noGravity = true;
			}
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
				Color trailColor = new Color(120, 220, 255) * (progress * 0.4f);
				Vector2 oldDrawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				sb.Draw(tex, oldDrawPos, null, trailColor, Projectile.oldRot[i], origin, Projectile.scale * (0.6f + progress * 0.4f), SpriteEffects.None, 0f);
			}

			sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
