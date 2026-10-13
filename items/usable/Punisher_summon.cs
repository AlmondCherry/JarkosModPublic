using jarkos.npc.Punisher;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
  public class Punisher_summon : ModItem
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
      NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Punisher>());
      return new bool?(true);
    }
	}
}
