using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
	public class NoxtiliumBulletGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public float charge = 0f;
		public const float MaxCharge = 15f;

		public int idleTicks = 0;

		public void OnHitByBullet()
		{
			charge = MathHelper.Clamp(charge + 0.2f, 0f, MaxCharge);
			idleTicks = 0;
		}

		public override void AI(NPC npc)
		{
			if (charge > 0f)
			{
				idleTicks++;

				if (idleTicks >= 60 && idleTicks % 60 == 0)
				{
					int secondsIdle = idleTicks / 60;
					int decayAmount = Math.Min(secondsIdle, 4); 

					charge -= decayAmount;
					if (charge <= 0f)
					{
						charge = 0f;
						idleTicks = 0;
					}
				}
			}
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (charge > 0f)
			{
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}

				int regenPenalty = (int)Math.Ceiling(charge);
				npc.lifeRegen -= regenPenalty;

				if (damage < 1)
				{
					damage = 1;
				}
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			if (charge > 0f)
			{
				float intensity = charge / MaxCharge;
				drawColor = Color.Lerp(drawColor, new Color(180, 80, 255), intensity * 0.45f);

				int chance = (int)MathHelper.Lerp(12f, 2f, intensity);
				if (Main.rand.NextBool(Math.Max(1, chance)))
				{
					Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.PurpleTorch, 0f, -1f, 100, default, 0.8f + intensity * 0.6f);
					d.noGravity = true;
				}
			}
		}
	}
}
