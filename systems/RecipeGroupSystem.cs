using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
namespace jarkos
{
	public class RecipeGroupSystem : ModSystem
	{
		public override void AddRecipeGroups()
		{
			RecipeGroup AnyGoldAxe = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.GoldAxe)}", 3482, 3518);
			RecipeGroup.RegisterGroup(nameof(ItemID.GoldAxe), AnyGoldAxe);
			RecipeGroup AnyIchor = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.Ichor)}", 1332, 522);
			RecipeGroup.RegisterGroup(nameof(ItemID.Ichor), AnyIchor);
			RecipeGroup AnyEvilMaterial = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.Vertebrae)}", 1330, 68);
			RecipeGroup.RegisterGroup(nameof(ItemID.Vertebrae), AnyEvilMaterial);
			RecipeGroup AnyEvilCloth = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.TissueSample)}", 1329, 86);
			RecipeGroup.RegisterGroup(nameof(ItemID.TissueSample), AnyEvilCloth);
			RecipeGroup AnyEvilSeeds = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CrimsonSeeds)}", 2171, 59);
			RecipeGroup.RegisterGroup(nameof(ItemID.CrimsonSeeds), AnyEvilSeeds);
			RecipeGroup AnyTitaniumBar = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.TitaniumBar)}", 1198, 391);
			RecipeGroup.RegisterGroup(nameof(ItemID.TitaniumBar), AnyTitaniumBar);
			RecipeGroup AnyMythrilBar = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.MythrilBar)}", 382, 1191);
			RecipeGroup.RegisterGroup(nameof(ItemID.MythrilBar), AnyMythrilBar);
			RecipeGroup AnyCobaltBar = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CobaltBar)}", 381, 1184);
			RecipeGroup.RegisterGroup(nameof(ItemID.CobaltBar), AnyCobaltBar);
			RecipeGroup AnyEvilBar = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CrimtaneBar)}", 1257, 57);
			RecipeGroup.RegisterGroup(nameof(ItemID.CrimtaneBar), AnyEvilBar);
			RecipeGroup AnyGoldBar = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.GoldBar)}", 19, 706);
			RecipeGroup.RegisterGroup(nameof(ItemID.GoldBar), AnyGoldBar);
			RecipeGroup AnyGoldPickaxe = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.GoldPickaxe)}", 3521, 3485);
			RecipeGroup.RegisterGroup(nameof(ItemID.GoldPickaxe), AnyGoldPickaxe);
			RecipeGroup AnyGoldHammer = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.GoldHammer)}", 3517, 3481);
			RecipeGroup.RegisterGroup(nameof(ItemID.GoldHammer), AnyGoldHammer);
			RecipeGroup AnySilverBar = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}", 21, 705);
			RecipeGroup.RegisterGroup(nameof(ItemID.SilverBar), AnySilverBar);
			RecipeGroup AnyIronBar = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.IronBar)}", 22, 704);
			RecipeGroup.RegisterGroup(nameof(ItemID.IronBar), AnyIronBar);
			RecipeGroup AnyIronOre = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.IronOre)}", 11, 700);
			RecipeGroup.RegisterGroup(nameof(ItemID.IronOre), AnyIronOre);
			RecipeGroup AnyCopperBar = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CopperBar)}", 20, 703);
			RecipeGroup.RegisterGroup(nameof(ItemID.CopperBar), AnyCopperBar);
			RecipeGroup AnyMythrilOre = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.MythrilOre)}", 365, 1105);
			RecipeGroup.RegisterGroup(nameof(ItemID.MythrilOre), AnyMythrilOre);
		}
	}
}
