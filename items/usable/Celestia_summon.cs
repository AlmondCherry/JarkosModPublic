using Microsoft.Xna.Framework;
using jarkos.items.materials;
using jarkos.npc.Celestia;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
  public class Celestia_summon : ModItem
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

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<Celestia>());

    public override bool? UseItem(Player player)
    {
      if (Main.netMode != NetmodeID.MultiplayerClient)
      {
        int type = ModContent.NPCType<Celestia>();
        Vector2 spawnPos = player.Center + new Vector2(0f, -1100f);
        int npcIndex = NPC.NewNPC(player.GetSource_ItemUse(Item), (int)spawnPos.X, (int)spawnPos.Y, type);
        if (npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
        {
          Main.npc[npcIndex].ai[0] = 30f;
          Main.npc[npcIndex].ai[1] = 0f;
          Main.npc[npcIndex].target = player.whoAmI;
          Main.npc[npcIndex].netUpdate = true;
        }
      }
      else
      {
        NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Celestia>());
      }
      return new bool?(true);
    }
    
		public override void AddRecipes()
    {
			CreateRecipe()
        .AddIngredient(ModContent.ItemType<NitoziumPlate>(), 2)
        .AddIngredient(ModContent.ItemType<FragmentOfFear>(), 50)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
