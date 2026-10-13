using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace jarkos.items.accs.persts
{
	public class RadiantGem : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 9));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 56;
			Item.accessory = true;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<Jarkos_player>();
			modPlayer.gemstaffs_buff = true;

			player.GetDamage(DamageClass.Magic) += 0.40f;
			player.GetAttackSpeed(DamageClass.Magic) += 0.15f;
			player.GetCritChance(DamageClass.Magic) += 15;
			
			player.manaRegen += 10;
			player.statManaMax2 += 150;
			player.manaCost -= 0.25f;
			player.aggro -= 25;
		}
	}
}
