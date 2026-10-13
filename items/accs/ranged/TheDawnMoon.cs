using jarkos.rare;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.items.accs.ranged
{
	public class TheDawnMoon : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 34;
			Item.accessory = true;
			Item.rare = ModContent.RarityType<CosmicRarity>();
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) 
		{
            if(Main.dayTime)
			{
				player.GetDamage(DamageClass.Generic) += 0.25f;
				player.GetAttackSpeed(DamageClass.Generic) += 0.25f;
			}
			else
			{
				player.GetDamage(DamageClass.Generic) += 0.12f;
				player.GetCritChance(DamageClass.Generic) += 0.12f;
			}

			player.moveSpeed *= 1.5f;
			player.accRunSpeed *= 1.5f;
			player.maxRunSpeed *= 1.15f;
	    }
    }
}
