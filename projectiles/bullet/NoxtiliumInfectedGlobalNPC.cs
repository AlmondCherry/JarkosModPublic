using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
	public class NoxtiliumInfectedGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool isInfected = false;
		public int infectedTimer = 0;
		public int sourceDamage = 0;
		public int ownerIndex = -1;

		public override void ResetEffects(NPC npc)
		{
			if (infectedTimer > 0)
			{
				infectedTimer--;
				if (infectedTimer <= 0)
				{
					isInfected = false;
				}
			}
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (isInfected)
			{
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}


				npc.lifeRegen -= 16;
				if (damage < 3)
				{
					damage = 3;
				}
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			if (isInfected)
			{

				drawColor = Color.Lerp(drawColor, new Color(180, 100, 255), 0.35f);

				if (Main.rand.NextBool(4))
				{
					Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.PurpleTorch, 0f, -1f, 100, default, 1.2f);
					d.noGravity = true;
				}
			}
		}

		public override void OnKill(NPC npc)
		{
			if (isInfected && Main.netMode != NetmodeID.Server)
			{

				int sporeCount = Main.rand.Next(5, 9);
				int dmg = sourceDamage > 0 ? (int)(sourceDamage * 0.6f) : 22;
				if (dmg < 10) dmg = 10;

				Player owner = (ownerIndex >= 0 && ownerIndex < Main.maxPlayers && Main.player[ownerIndex].active) ? Main.player[ownerIndex] : Main.LocalPlayer;

				for (int i = 0; i < sporeCount; i++)
				{
					Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
					Projectile.NewProjectile(npc.GetSource_Death(), npc.Center, vel, ModContent.ProjectileType<NoxtiliumSporeProj>(), dmg, 1f, owner.whoAmI);
				}
			}
		}
	}
}
