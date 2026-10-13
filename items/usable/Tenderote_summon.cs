using jarkos.items.materials;
using jarkos.npc.tenderote;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
  public class Tenderote_summon : ModItem
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
    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<Tenderote_head>());
    public override bool? UseItem(Player player)
    {
      NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Tenderote_head>());
      return new bool?(true);
    }
		public override void AddRecipes() {
			CreateRecipe()
        .AddIngredient(ModContent.ItemType<FragmentOfFear>(), 10)
        .AddIngredient(ItemID.LunarBar, 50)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
