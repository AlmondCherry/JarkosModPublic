using jarkos.npc.godofdeath;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
  public class Godofdeath_summon : ModItem
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
    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<God_of_death>());
    public override bool? UseItem(Player player)
    {
      NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<God_of_death>());
      return new bool?(true);
    }
		public override void AddRecipes() {
			CreateRecipe()
        .AddIngredient(ItemID.Obsidian, 10)
        .AddIngredient(ItemID.ManaCrystal, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
