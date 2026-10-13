using jarkos.items.materials;
using jarkos.npc.Punisher;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
  public class Beetle_mirror : ModItem
  {
    public override void SetStaticDefaults()
    {
      CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }
    public override void SetDefaults()
    {
      Item.width = 40;
      Item.height = 40;
      Item.maxStack = 20;
      Item.useAnimation = 45;
      Item.useTime = 45;
      Item.useStyle = ItemUseStyleID.HoldUp;
      Item.UseSound = new SoundStyle?(SoundID.Item44);
      Item.consumable = false;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<Punisher>());

    public override bool? UseItem(Player player)
    {
      player.TeleportationPotion();
      return new bool?(true);
    }

    public override void AddRecipes()
    {
			CreateRecipe(1)
			.AddIngredient(ItemID.MagicMirror, 1)
			.AddIngredient(ModContent.ItemType<ChitinBeetle>(), 2)
			.AddIngredient(ModContent.ItemType<ThickSilk>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
