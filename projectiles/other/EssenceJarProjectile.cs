using System.Collections.Generic;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.other
{
	public class EssenceJarProjectile : ModProjectile
	{
		public override string Texture => "jarkos/items/potions/Strength_Essence";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 20;
			Projectile.aiStyle = 2;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 15; i++)
				{
					Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
					d.scale = Main.rand.NextFloat(0.8f, 1.3f);
				}
			}

			if (Main.netMode == NetmodeID.MultiplayerClient) return;

			Player owner = Main.player[Projectile.owner];
			if (!owner.active) return;

			ResearchPlayer rp = owner.GetModPlayer<ResearchPlayer>();
			List<EssenceType> availableEssences = new List<EssenceType>();

			foreach (EssenceType et in System.Enum.GetValues(typeof(EssenceType)))
			{
				if (rp.DiscoveredEssences.Contains((int)et))
				{
					availableEssences.Add(et);
				}
			}

			if (availableEssences.Count == 0)
			{
				availableEssences.Add(EssenceType.Aer);
				availableEssences.Add(EssenceType.Aqua);
				availableEssences.Add(EssenceType.Vita);
				availableEssences.Add(EssenceType.Terra);
			}


			int targetTypeCount = Main.rand.Next(2, 7);
			if (targetTypeCount > availableEssences.Count)
			{
				targetTypeCount = availableEssences.Count;
			}

			List<EssenceType> selectedTypes = new List<EssenceType>();
			List<EssenceType> pool = new List<EssenceType>(availableEssences);

			for (int i = 0; i < targetTypeCount && pool.Count > 0; i++)
			{
				int index = Main.rand.Next(pool.Count);
				selectedTypes.Add(pool[index]);
				pool.RemoveAt(index);
			}


			int totalOrbs = Main.rand.Next(5, 21);
			int[] orbCounts = new int[selectedTypes.Count];


			for (int i = 0; i < selectedTypes.Count; i++)
			{
				orbCounts[i] = 1;
			}

			int remaining = totalOrbs - selectedTypes.Count;
			for (int r = 0; r < remaining; r++)
			{
				int rIdx = Main.rand.Next(selectedTypes.Count);
				orbCounts[rIdx]++;
			}

			for (int i = 0; i < selectedTypes.Count; i++)
			{
				EssenceType type = selectedTypes[i];
				int count = orbCounts[i];
				for (int k = 0; k < count; k++)
				{
					Vector2 spawnVel = Main.rand.NextVector2Circular(7f, 7f) - new Vector2(0, 3f);
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						spawnVel,
						ModContent.ProjectileType<EssenceOrbProjectile>(),
						0,
						0f,
						Main.myPlayer,
						(int)type,
						Projectile.owner
					);
				}
			}
		}
	}
}
