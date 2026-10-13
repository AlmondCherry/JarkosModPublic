using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
	public class SpookynightSeal : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 56;
			Item.height = 54;
			Item.accessory = true;
			Item.rare = ItemRarityID.Cyan;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPl = player.GetModPlayer<SpookynightSealPlayer>();
			modPl.Spooky_Seal = true;
			
			player.GetDamage(DamageClass.Summon) += 0.08f;
			player.GetDamage(DamageClass.SummonMeleeSpeed) += 0.08f;
			player.maxMinions += 1;
		}
	}

	public class SpookynightSealPlayer : ModPlayer
	{
		public bool Spooky_Seal = false;

        public override void ResetEffects()
        {
            Spooky_Seal = false;
        }
	}
}
