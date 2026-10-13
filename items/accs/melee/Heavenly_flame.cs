using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using jarkos.rare;

namespace jarkos.items.accs.melee
{
	public class Heavenly_flame : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 72;
			Item.height = 118;
			Item.accessory = true;
			Item.rare = ModContent.RarityType<StormRarity>();
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPl = player.GetModPlayer<Heavenly_flame_player>();
			modPl.Heavenly_flame_accs = true;
			player.GetDamage(DamageClass.Melee) += 0.25f;
		}
	}
	public class Heavenly_flame_player : ModPlayer
	{
		public bool Heavenly_flame_accs = false;
		public override void ResetEffects()
		{
			Heavenly_flame_accs = false;
		}
	}
}
