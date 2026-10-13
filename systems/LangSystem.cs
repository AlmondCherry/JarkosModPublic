using System.Collections.Generic;
using jarkos.items.accs.general;
using jarkos.items.accs.mage;
using jarkos.items.accs.ranged;
using jarkos.items.weapons.melee.other_melee;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
namespace jarkos.systems
{
	#region Language System
	public class LangSystem : ModSystem
	{
		public static LocalizedText enterworld_message { get; private set; }
		public static LocalizedText bloodshed_text { get; private set; }
		public static LocalizedText naivety_text { get; private set; }
		public static LocalizedText scourge_text { get; private set; }
		public static LocalizedText mor_text { get; private set; }
		public static LocalizedText purplefire_text { get; private set; }
		public static LocalizedText fragility_text { get; private set; }
		public static LocalizedText omnipotence_text { get; private set; }
		public static LocalizedText significance_text { get; private set; }
		public static LocalizedText TabBasics { get; private set; }
		public static LocalizedText TabRituals { get; private set; }
		public static LocalizedText TabEssences { get; private set; }
		public static LocalizedText TabResearch { get; private set; }
		public static LocalizedText StudiedEssences { get; private set; }
		public static LocalizedText CombinerRecipe { get; private set; }
		public static LocalizedText BaseEssence { get; private set; }
		public static LocalizedText ClickToClose { get; private set; }
		public static LocalizedText CollectButtonText { get; private set; }
		public static LocalizedText DistillerIgnoreTooltip { get; private set; }
		public static LocalizedText AlchTitle { get; private set; }
		public static LocalizedText AlchIngredients { get; private set; }
		public static LocalizedText AlchRecipe { get; private set; }
		public static LocalizedText AlchResult { get; private set; }
		public static LocalizedText AlchCraft { get; private set; }
		public static LocalizedText AlchSuckingMode { get; private set; }
		public static LocalizedText AlchTransmissionMode { get; private set; }
		public static LocalizedText AlchRequires { get; private set; }
		public static LocalizedText AlchResultText { get; private set; }
		public static LocalizedText RecipeName1 { get; private set; }
		public static LocalizedText RecipeDesc1 { get; private set; }
		public static LocalizedText RecipeName2 { get; private set; }
		public static LocalizedText RecipeDesc2 { get; private set; }
		public static LocalizedText RecipeName3 { get; private set; }
		public static LocalizedText RecipeDesc3 { get; private set; }
		public static LocalizedText RecipeName4 { get; private set; }
		public static LocalizedText RecipeDesc4 { get; private set; }
		public static LocalizedText RecipeName5 { get; private set; }
		public static LocalizedText RecipeDesc5 { get; private set; }
		public static LocalizedText RecipeName6 { get; private set; }
		public static LocalizedText RecipeDesc6 { get; private set; }
		public static LocalizedText RecipeName7 { get; private set; }
		public static LocalizedText RecipeDesc7 { get; private set; }
		public static LocalizedText RecipeName8 { get; private set; }
		public static LocalizedText RecipeDesc8 { get; private set; }
		public static LocalizedText RecipeName9 { get; private set; }
		public static LocalizedText RecipeDesc9 { get; private set; }
		public static LocalizedText RecipeName10 { get; private set; }
		public static LocalizedText RecipeDesc10 { get; private set; }
		public static LocalizedText RecipeName11 { get; private set; }
		public static LocalizedText RecipeDesc11 { get; private set; }
		public static LocalizedText PuzzleShapeLaterText { get; private set; }
		public static LocalizedText AquaNode1_Name { get; private set; }
		public static LocalizedText AquaNode1_Desc { get; private set; }
		public static LocalizedText AquaNode2_Name { get; private set; }
		public static LocalizedText AquaNode2_Desc { get; private set; }
		public static LocalizedText EssenceBonus_Acus { get; private set; }
		public static LocalizedText EssenceBonus_Aer { get; private set; }
		public static LocalizedText EssenceBonus_Anima { get; private set; }
		public static LocalizedText EssenceBonus_Aqua { get; private set; }
		public static LocalizedText EssenceBonus_Arena { get; private set; }
		public static LocalizedText EssenceBonus_Caro { get; private set; }
		public static LocalizedText EssenceBonus_Chitinus { get; private set; }
		public static LocalizedText EssenceBonus_Cruor { get; private set; }
		public static LocalizedText EssenceBonus_Fulgur { get; private set; }
		public static LocalizedText EssenceBonus_Glacies { get; private set; }
		public static LocalizedText EssenceBonus_Imperium { get; private set; }
		public static LocalizedText EssenceBonus_Luna { get; private set; }
		public static LocalizedText EssenceBonus_Lux { get; private set; }
		public static LocalizedText EssenceBonus_Mechanus { get; private set; }
		public static LocalizedText EssenceBonus_Nebula { get; private set; }
		public static LocalizedText EssenceBonus_Ordo { get; private set; }
		public static LocalizedText EssenceBonus_Pelagus { get; private set; }
		public static LocalizedText EssenceBonus_Pyros { get; private set; }
		public static LocalizedText EssenceBonus_Regalis { get; private set; }
		public static LocalizedText EssenceBonus_Silva { get; private set; }
		public static LocalizedText EssenceBonus_Solar { get; private set; }
		public static LocalizedText EssenceBonus_Stellaris { get; private set; }
		public static LocalizedText EssenceBonus_Terra { get; private set; }
		public static LocalizedText EssenceBonus_Umbra { get; private set; }
		public static LocalizedText EssenceBonus_Venenum { get; private set; }
		public static LocalizedText EssenceBonus_Vita { get; private set; }
		public static LocalizedText EssenceBonus_Vortex { get; private set; }
		public static LocalizedText AquaNode3_Name { get; private set; }
		public static LocalizedText AquaNode3_Desc { get; private set; }
		public static LocalizedText AquaNode4_Name { get; private set; }
		public static LocalizedText AquaNode4_Desc { get; private set; }
		public static LocalizedText AquaNode5_Name { get; private set; }
		public static LocalizedText AquaNode5_Desc { get; private set; }
		public static LocalizedText AquaNode6_Name { get; private set; }
		public static LocalizedText AquaNode6_Desc { get; private set; }
		public static LocalizedText AquaNode7_Name { get; private set; }
		public static LocalizedText AquaNode7_Desc { get; private set; }
		public static LocalizedText AquaNode8_Name { get; private set; }
		public static LocalizedText AquaNode8_Desc { get; private set; }
		public static LocalizedText AquaSub1Node1_Name { get; private set; }
		public static LocalizedText AquaSub1Node1_Desc { get; private set; }
		public static LocalizedText AquaSub1Node2_Name { get; private set; }
		public static LocalizedText AquaSub1Node2_Desc { get; private set; }
		public static LocalizedText AquaSub1Node3_Name { get; private set; }
		public static LocalizedText AquaSub1Node3_Desc { get; private set; }
		public static LocalizedText VenenumSub1Node1_Name { get; private set; }
		public static LocalizedText VenenumSub1Node1_Desc { get; private set; }
		public static LocalizedText VenenumSub1Node2_Name { get; private set; }
		public static LocalizedText VenenumSub1Node2_Desc { get; private set; }
		public static LocalizedText VenenumSub1Node3_Name { get; private set; }
		public static LocalizedText VenenumSub1Node3_Desc { get; private set; }
		public static LocalizedText PyrosNode1_Name { get; private set; }
		public static LocalizedText PyrosNode1_Desc { get; private set; }
		public static LocalizedText PyrosNode2_Name { get; private set; }
		public static LocalizedText PyrosNode2_Desc { get; private set; }
		public static LocalizedText PyrosNode3_Name { get; private set; }
		public static LocalizedText PyrosNode3_Desc { get; private set; }
		public static LocalizedText PyrosNode4_Name { get; private set; }
		public static LocalizedText PyrosNode4_Desc { get; private set; }
		public static LocalizedText PyrosNode5_Name { get; private set; }
		public static LocalizedText PyrosNode5_Desc { get; private set; }
		public static LocalizedText PyrosNode6_Name { get; private set; }
		public static LocalizedText PyrosNode6_Desc { get; private set; }
		public static LocalizedText PyrosNode7_Name { get; private set; }
		public static LocalizedText PyrosNode7_Desc { get; private set; }
		public static LocalizedText PyrosNode8_Name { get; private set; }
		public static LocalizedText PyrosNode8_Desc { get; private set; }
		public static LocalizedText PyrosSub1Node1_Name { get; private set; }
		public static LocalizedText PyrosSub1Node1_Desc { get; private set; }
		public static LocalizedText PyrosSub1Node2_Name { get; private set; }
		public static LocalizedText PyrosSub1Node2_Desc { get; private set; }
		public static LocalizedText PyrosSub1Node3_Name { get; private set; }
		public static LocalizedText PyrosSub1Node3_Desc { get; private set; }
		public static LocalizedText PyrosSub1Node4_Name { get; private set; }
		public static LocalizedText PyrosSub1Node4_Desc { get; private set; }
		public static LocalizedText VitaNode1_Name { get; private set; }
		public static LocalizedText VitaNode1_Desc { get; private set; }
		public static LocalizedText VitaNode2_Name { get; private set; }
		public static LocalizedText VitaNode2_Desc { get; private set; }
		public static LocalizedText VitaNode3_Name { get; private set; }
		public static LocalizedText VitaNode3_Desc { get; private set; }
		public static LocalizedText VitaNode4_Name { get; private set; }
		public static LocalizedText VitaNode4_Desc { get; private set; }
		public static LocalizedText VitaNode5_Name { get; private set; }
		public static LocalizedText VitaNode5_Desc { get; private set; }
		public static LocalizedText VitaNode6_Name { get; private set; }
		public static LocalizedText VitaNode6_Desc { get; private set; }
		public static LocalizedText VitaNode7_Name { get; private set; }
		public static LocalizedText VitaNode7_Desc { get; private set; }
		public static LocalizedText VitaNode8_Name { get; private set; }
		public static LocalizedText VitaNode8_Desc { get; private set; }
		public static LocalizedText VitaSub1Node1_Name { get; private set; }
		public static LocalizedText VitaSub1Node1_Desc { get; private set; }
		public static LocalizedText VitaSub1Node2_Name { get; private set; }
		public static LocalizedText VitaSub1Node2_Desc { get; private set; }
		public static LocalizedText VitaSub1Node3_Name { get; private set; }
		public static LocalizedText VitaSub1Node3_Desc { get; private set; }
		public static LocalizedText VitaSub1Node4_Name { get; private set; }
		public static LocalizedText VitaSub1Node4_Desc { get; private set; }
		public static LocalizedText VenenumNode1_Name { get; private set; }
		public static LocalizedText VenenumNode1_Desc { get; private set; }
		public static LocalizedText VenenumNode2_Name { get; private set; }
		public static LocalizedText VenenumNode2_Desc { get; private set; }
		public static LocalizedText VenenumNode3_Name { get; private set; }
		public static LocalizedText VenenumNode3_Desc { get; private set; }
		public static LocalizedText VenenumNode4_Name { get; private set; }
		public static LocalizedText VenenumNode4_Desc { get; private set; }
		public static LocalizedText VenenumNode5_Name { get; private set; }
		public static LocalizedText VenenumNode5_Desc { get; private set; }
		public static LocalizedText VenenumNode6_Name { get; private set; }
		public static LocalizedText VenenumNode6_Desc { get; private set; }
		public static LocalizedText VenenumNode7_Name { get; private set; }
		public static LocalizedText VenenumNode7_Desc { get; private set; }
		public static LocalizedText VenenumNode8_Name { get; private set; }
		public static LocalizedText VenenumNode8_Desc { get; private set; }
		public static LocalizedText UmbraNode1_Name { get; private set; }
		public static LocalizedText UmbraNode1_Desc { get; private set; }
		public static LocalizedText UmbraNode2_Name { get; private set; }
		public static LocalizedText UmbraNode2_Desc { get; private set; }
		public static LocalizedText UmbraNode3_Name { get; private set; }
		public static LocalizedText UmbraNode3_Desc { get; private set; }
		public static LocalizedText UmbraNode4_Name { get; private set; }
		public static LocalizedText UmbraNode4_Desc { get; private set; }
		public static LocalizedText UmbraNode5_Name { get; private set; }
		public static LocalizedText UmbraNode5_Desc { get; private set; }
		public static LocalizedText UmbraNode6_Name { get; private set; }
		public static LocalizedText UmbraNode6_Desc { get; private set; }
		public static LocalizedText UmbraNode7_Name { get; private set; }
		public static LocalizedText UmbraNode7_Desc { get; private set; }
		public static LocalizedText UmbraNode8_Name { get; private set; }
		public static LocalizedText UmbraNode8_Desc { get; private set; }
		public static LocalizedText UmbraSub1Node1_Name { get; private set; }
		public static LocalizedText UmbraSub1Node1_Desc { get; private set; }
		public static LocalizedText UmbraSub1Node2_Name { get; private set; }
		public static LocalizedText UmbraSub1Node2_Desc { get; private set; }
		public static LocalizedText UmbraSub1Node3_Name { get; private set; }
		public static LocalizedText UmbraSub1Node3_Desc { get; private set; }
		public static LocalizedText bloodshed_textclick { get; private set; }
		public static LocalizedText naivety_textclick { get; private set; }
		public static LocalizedText scourge_textclick { get; private set; }
		public static LocalizedText mor_textclick { get; private set; }
		public static LocalizedText purplefire_textclick { get; private set; }
		public static LocalizedText fragility_textclick { get; private set; }
		public static LocalizedText omnipotence_textclick { get; private set; }
		public static LocalizedText significance_textclick { get; private set; }
		public static LocalizedText bloodshed_textclick2 { get; private set; }
		public static LocalizedText naivety_textclick2 { get; private set; }
		public static LocalizedText scourge_textclick2 { get; private set; }
		public static LocalizedText mor_textclick2 { get; private set; }
		public static LocalizedText purplefire_textclick2 { get; private set; }
		public static LocalizedText fragility_textclick2 { get; private set; }
		public static LocalizedText omnipotence_textclick2 { get; private set; }
		public static LocalizedText significance_textclick2 { get; private set; }
		public static LocalizedText damage_text { get; private set; }
		public static LocalizedText energyuse_text1 { get; private set; }
		public static LocalizedText energyuse_text2 { get; private set; }
		public static LocalizedText warning_text1 { get; private set; }
		public static LocalizedText antique_gen { get; private set; }
		public static LocalizedText etherionite_gen { get; private set; }
		public static LocalizedText filth_rune_desc { get; private set; }
		public static LocalizedText steampunk_rune_desc { get; private set; }
		public static LocalizedText elder_rune_desc { get; private set; }
		public static LocalizedText flame_rune_desc { get; private set; }
		public static LocalizedText filth_disease_desc { get; private set; }
		public static LocalizedText core_of_trouble_desc { get; private set; }
		public static LocalizedText fishing_mastery { get; private set; }
		public static LocalizedText ForestFruit_desc { get; private set; }
		public static LocalizedText CaveFruit_desc { get; private set; }
		public static LocalizedText ArcaneForgeMaterial { get; private set; }
		public static LocalizedText ArcaneForgeWeapon { get; private set; }
		public static LocalizedText ArcaneForgeParameters { get; private set; }
		public static LocalizedText ArcaneForgeDamage { get; private set; }
		public static LocalizedText ArcaneForgeAttackSpeed { get; private set; }
		public static LocalizedText ArcaneForgeCritChance { get; private set; }
		public static LocalizedText ArcaneForgeKnockback { get; private set; }
		public static LocalizedText ArcaneForgeAdaptate { get; private set; }
		public static LocalizedText ArcaneForgePrefixFog { get; private set; }
		public static LocalizedText ArcaneForgePrefixEther { get; private set; }
		public static LocalizedText ArcaneForgePrefixStar { get; private set; }
		public static LocalizedText ArcaneForgePrefixHeavenly { get; private set; }
		public static LocalizedText ArcaneForgePrefixGodly { get; private set; }
		public static LocalizedText ArcaneForgeHeader { get; private set; }
		public static LocalizedText ArcaneForgeDamageDesc { get; private set; }
		public static LocalizedText ArcaneForgeAttackSpeedDesc { get; private set; }
		public static LocalizedText ArcaneForgeCritChanceDesc { get; private set; }
		public static LocalizedText ArcaneForgeKnockbackDesc { get; private set; }
		public static LocalizedText AlchemicalDescPageOne { get; private set; }
		public static LocalizedText AlchemicalDescPageTwo { get; private set; }
		public static LocalizedText AlchemicalDescPageThree { get; private set; }
		public static LocalizedText AlchemicalDescPageFour { get; private set; }
		public static LocalizedText AlchemicalDescPageFive { get; private set; }
		public static LocalizedText close_msg { get; private set; }
		public static LocalizedText anyBar_msg { get; private set; }
		public static LocalizedText anyFuel_msg { get; private set; }
		public static LocalizedText output_msg { get; private set; }
		public static LocalizedText heating_msg { get; private set; }
		public static LocalizedText rolling_msg { get; private set; }
		public static LocalizedText waiting_msg { get; private set; }
		public static LocalizedText nomaterial_msg { get; private set; }
		public static LocalizedText nofuel_msg { get; private set; }
		public static LocalizedText heatingprocess_msg { get; private set; }
		public static LocalizedText rollingprocess_msg { get; private set; }
		public static LocalizedText KeybindNullText { get; private set; }
		public static LocalizedText Foulmouthed_frog_desc { get; private set; }
		public static LocalizedText GiantDragonfly_desc { get; private set; }
		public static LocalizedText WatcherEye_desc { get; private set; }
		public static LocalizedText ValueDevourer_desc { get; private set; }
		public override void Load()
		{
			string category_ = "Misc";
			enterworld_message ??= Mod.GetLocalization($"{category_}.enterworld_message");
			KeybindNullText ??= Mod.GetLocalization($"{category_}.KeybindNullText");
			string category = "UI";
			bloodshed_text ??= Mod.GetLocalization($"{category}.bloodshed_tooltip");
			naivety_text ??= Mod.GetLocalization($"{category}.naivety_text");
			scourge_text ??= Mod.GetLocalization($"{category}.scourge_text");
			mor_text ??= Mod.GetLocalization($"{category}.mor_text");
			purplefire_text ??= Mod.GetLocalization($"{category}.purplefire_text");
			fragility_text ??= Mod.GetLocalization($"{category}.fragility_text");
			TabBasics ??= Mod.GetLocalization($"{category}.RitualBookUI.Tab1");
			TabRituals ??= Mod.GetLocalization($"{category}.RitualBookUI.Tab2");
			TabEssences ??= Mod.GetLocalization($"{category}.RitualBookUI.Tab3");
			TabResearch ??= Mod.GetLocalization($"{category}.RitualBookUI.Tab4");
			StudiedEssences ??= Mod.GetLocalization($"{category}.RitualBookUI.StudiedEssences");
			CombinerRecipe ??= Mod.GetLocalization($"{category}.RitualBookUI.CombinerRecipe");
			BaseEssence ??= Mod.GetLocalization($"{category}.RitualBookUI.BaseEssence");
			ClickToClose ??= Mod.GetLocalization($"{category}.RitualBookUI.ClickToClose");
			CollectButtonText ??= Mod.GetLocalization($"{category}.CollectButtonText");
			DistillerIgnoreTooltip ??= Mod.GetLocalization($"{category}.DistillerIgnoreTooltip");
			AlchTitle ??= Mod.GetLocalization($"{category}.AlchTitle");
			AlchIngredients ??= Mod.GetLocalization($"{category}.AlchIngredients");
			AlchRecipe ??= Mod.GetLocalization($"{category}.AlchRecipe");
			AlchResult ??= Mod.GetLocalization($"{category}.AlchResult");
			AlchCraft ??= Mod.GetLocalization($"{category}.AlchCraft");
			AlchSuckingMode ??= Mod.GetLocalization($"{category}.AlchSuckingMode");
			AlchTransmissionMode ??= Mod.GetLocalization($"{category}.AlchTransmissionMode");
			AlchRequires ??= Mod.GetLocalization($"{category}.AlchRequires");
			AlchResultText ??= Mod.GetLocalization($"{category}.AlchResultText");
			RecipeName1 ??= Mod.GetLocalization($"{category}.RecipeName1");
			RecipeDesc1 ??= Mod.GetLocalization($"{category}.RecipeDesc1");
			RecipeName2 ??= Mod.GetLocalization($"{category}.RecipeName2");
			RecipeDesc2 ??= Mod.GetLocalization($"{category}.RecipeDesc2");
			RecipeName3 ??= Mod.GetLocalization($"{category}.RecipeName3");
			RecipeDesc3 ??= Mod.GetLocalization($"{category}.RecipeDesc3");
			RecipeName4 ??= Mod.GetLocalization($"{category}.RecipeName4");
			RecipeDesc4 ??= Mod.GetLocalization($"{category}.RecipeDesc4");
			RecipeName5 ??= Mod.GetLocalization($"{category}.RecipeName5");
			RecipeDesc5 ??= Mod.GetLocalization($"{category}.RecipeDesc5");
			RecipeName6 ??= Mod.GetLocalization($"{category}.RecipeName6");
			RecipeDesc6 ??= Mod.GetLocalization($"{category}.RecipeDesc6");
			RecipeName7 ??= Mod.GetLocalization($"{category}.RecipeName7");
			RecipeDesc7 ??= Mod.GetLocalization($"{category}.RecipeDesc7");
			RecipeName8 ??= Mod.GetLocalization($"{category}.RecipeName8");
			RecipeDesc8 ??= Mod.GetLocalization($"{category}.RecipeDesc8");
			RecipeName9 ??= Mod.GetLocalization($"{category}.RecipeName9");
			RecipeDesc9 ??= Mod.GetLocalization($"{category}.RecipeDesc9");
			RecipeName10 ??= Mod.GetLocalization($"{category}.RecipeName10");
			RecipeDesc10 ??= Mod.GetLocalization($"{category}.RecipeDesc10");
			RecipeName11 ??= Mod.GetLocalization($"{category}.RecipeName11");
			RecipeDesc11 ??= Mod.GetLocalization($"{category}.RecipeDesc11");
			PuzzleShapeLaterText ??= Mod.GetLocalization($"{category}.PuzzleShapeLaterText");
			string categoryEssence = "EssenceBonuses";
			EssenceBonus_Acus ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Acus");
			EssenceBonus_Aer ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Aer");
			EssenceBonus_Anima ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Anima");
			EssenceBonus_Aqua ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Aqua");
			EssenceBonus_Arena ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Arena");
			EssenceBonus_Caro ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Caro");
			EssenceBonus_Chitinus ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Chitinus");
			EssenceBonus_Cruor ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Cruor");
			EssenceBonus_Fulgur ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Fulgur");
			EssenceBonus_Glacies ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Glacies");
			EssenceBonus_Imperium ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Imperium");
			EssenceBonus_Luna ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Luna");
			EssenceBonus_Lux ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Lux");
			EssenceBonus_Mechanus ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Mechanus");
			EssenceBonus_Nebula ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Nebula");
			EssenceBonus_Ordo ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Ordo");
			EssenceBonus_Pelagus ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Pelagus");
			EssenceBonus_Pyros ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Pyros");
			EssenceBonus_Regalis ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Regalis");
			EssenceBonus_Silva ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Silva");
			EssenceBonus_Solar ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Solar");
			EssenceBonus_Stellaris ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Stellaris");
			EssenceBonus_Terra ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Terra");
			EssenceBonus_Umbra ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Umbra");
			EssenceBonus_Venenum ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Venenum");
			EssenceBonus_Vita ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Vita");
			EssenceBonus_Vortex ??= Mod.GetLocalization($"{categoryEssence}.EssenceBonus_Vortex");
			string categoryNodes = "Nodes";
			AquaNode1_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaNode1_Name");
			AquaNode1_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaNode1_Desc");
			AquaNode2_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaNode2_Name");
			AquaNode2_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaNode2_Desc");
			AquaNode3_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaNode3_Name");
			AquaNode3_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaNode3_Desc");
			AquaNode4_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaNode4_Name");
			AquaNode4_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaNode4_Desc");
			AquaNode5_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaNode5_Name");
			AquaNode5_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaNode5_Desc");
			AquaNode6_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaNode6_Name");
			AquaNode6_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaNode6_Desc");
			AquaNode7_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaNode7_Name");
			AquaNode7_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaNode7_Desc");
			AquaNode8_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaNode8_Name");
			AquaNode8_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaNode8_Desc");
			AquaSub1Node1_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaSub1Node1_Name");
			AquaSub1Node1_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaSub1Node1_Desc");
			AquaSub1Node2_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaSub1Node2_Name");
			AquaSub1Node2_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaSub1Node2_Desc");
			AquaSub1Node3_Name ??= Mod.GetLocalization($"{categoryNodes}.AquaSub1Node3_Name");
			AquaSub1Node3_Desc ??= Mod.GetLocalization($"{categoryNodes}.AquaSub1Node3_Desc");
			VenenumSub1Node1_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumSub1Node1_Name");
			VenenumSub1Node1_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumSub1Node1_Desc");
			VenenumSub1Node2_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumSub1Node2_Name");
			VenenumSub1Node2_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumSub1Node2_Desc");
			VenenumSub1Node3_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumSub1Node3_Name");
			VenenumSub1Node3_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumSub1Node3_Desc");
			PyrosNode1_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode1_Name");
			PyrosNode1_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode1_Desc");
			PyrosNode2_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode2_Name");
			PyrosNode2_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode2_Desc");
			PyrosNode3_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode3_Name");
			PyrosNode3_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode3_Desc");
			PyrosNode4_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode4_Name");
			PyrosNode4_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode4_Desc");
			PyrosNode5_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode5_Name");
			PyrosNode5_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode5_Desc");
			PyrosNode6_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode6_Name");
			PyrosNode6_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode6_Desc");
			PyrosNode7_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode7_Name");
			PyrosNode7_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode7_Desc");
			PyrosNode8_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode8_Name");
			PyrosNode8_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosNode8_Desc");
			PyrosSub1Node1_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosSub1Node1_Name");
			PyrosSub1Node1_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosSub1Node1_Desc");
			PyrosSub1Node2_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosSub1Node2_Name");
			PyrosSub1Node2_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosSub1Node2_Desc");
			PyrosSub1Node3_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosSub1Node3_Name");
			PyrosSub1Node3_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosSub1Node3_Desc");
			PyrosSub1Node4_Name ??= Mod.GetLocalization($"{categoryNodes}.PyrosSub1Node4_Name");
			PyrosSub1Node4_Desc ??= Mod.GetLocalization($"{categoryNodes}.PyrosSub1Node4_Desc");
			VitaNode1_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaNode1_Name");
			VitaNode1_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaNode1_Desc");
			VitaNode2_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaNode2_Name");
			VitaNode2_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaNode2_Desc");
			VitaNode3_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaNode3_Name");
			VitaNode3_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaNode3_Desc");
			VitaNode4_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaNode4_Name");
			VitaNode4_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaNode4_Desc");
			VitaNode5_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaNode5_Name");
			VitaNode5_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaNode5_Desc");
			VitaNode6_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaNode6_Name");
			VitaNode6_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaNode6_Desc");
			VitaNode7_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaNode7_Name");
			VitaNode7_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaNode7_Desc");
			VitaNode8_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaNode8_Name");
			VitaNode8_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaNode8_Desc");
			VitaSub1Node1_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaSub1Node1_Name");
			VitaSub1Node1_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaSub1Node1_Desc");
			VitaSub1Node2_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaSub1Node2_Name");
			VitaSub1Node2_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaSub1Node2_Desc");
			VitaSub1Node3_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaSub1Node3_Name");
			VitaSub1Node3_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaSub1Node3_Desc");
			VitaSub1Node4_Name ??= Mod.GetLocalization($"{categoryNodes}.VitaSub1Node4_Name");
			VitaSub1Node4_Desc ??= Mod.GetLocalization($"{categoryNodes}.VitaSub1Node4_Desc");
			VenenumNode1_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode1_Name");
			VenenumNode1_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode1_Desc");
			VenenumNode2_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode2_Name");
			VenenumNode2_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode2_Desc");
			VenenumNode3_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode3_Name");
			VenenumNode3_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode3_Desc");
			VenenumNode4_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode4_Name");
			VenenumNode4_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode4_Desc");
			VenenumNode5_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode5_Name");
			VenenumNode5_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode5_Desc");
			VenenumNode6_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode6_Name");
			VenenumNode6_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode6_Desc");
			VenenumNode7_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode7_Name");
			VenenumNode7_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode7_Desc");
			VenenumNode8_Name ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode8_Name");
			VenenumNode8_Desc ??= Mod.GetLocalization($"{categoryNodes}.VenenumNode8_Desc");
			UmbraNode1_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode1_Name");
			UmbraNode1_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode1_Desc");
			UmbraNode2_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode2_Name");
			UmbraNode2_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode2_Desc");
			UmbraNode3_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode3_Name");
			UmbraNode3_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode3_Desc");
			UmbraNode4_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode4_Name");
			UmbraNode4_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode4_Desc");
			UmbraNode5_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode5_Name");
			UmbraNode5_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode5_Desc");
			UmbraNode6_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode6_Name");
			UmbraNode6_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode6_Desc");
			UmbraNode7_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode7_Name");
			UmbraNode7_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode7_Desc");
			UmbraNode8_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode8_Name");
			UmbraNode8_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraNode8_Desc");
			UmbraSub1Node1_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraSub1Node1_Name");
			UmbraSub1Node1_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraSub1Node1_Desc");
			UmbraSub1Node2_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraSub1Node2_Name");
			UmbraSub1Node2_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraSub1Node2_Desc");
			UmbraSub1Node3_Name ??= Mod.GetLocalization($"{categoryNodes}.UmbraSub1Node3_Name");
			UmbraSub1Node3_Desc ??= Mod.GetLocalization($"{categoryNodes}.UmbraSub1Node3_Desc");
			omnipotence_text ??= Mod.GetLocalization($"{category}.omnipotence_text");
			significance_text ??= Mod.GetLocalization($"{category}.significance_text");
			ArcaneForgeMaterial ??= Mod.GetLocalization($"{category}.ArcaneForgeMaterial");
			ArcaneForgeWeapon ??= Mod.GetLocalization($"{category}.ArcaneForgeWeapon");
			ArcaneForgeParameters ??= Mod.GetLocalization($"{category}.ArcaneForgeParameters");
			ArcaneForgeDamage ??= Mod.GetLocalization($"{category}.ArcaneForgeDamage");
			ArcaneForgeAttackSpeed ??= Mod.GetLocalization($"{category}.ArcaneForgeAttackSpeed");
			ArcaneForgeCritChance ??= Mod.GetLocalization($"{category}.ArcaneForgeCritChance");
			ArcaneForgeKnockback ??= Mod.GetLocalization($"{category}.ArcaneForgeKnockback");
			ArcaneForgeAdaptate ??= Mod.GetLocalization($"{category}.ArcaneForgeAdaptate");
			ArcaneForgePrefixFog ??= Mod.GetLocalization($"{category}.ArcaneForgePrefixFog");
			ArcaneForgePrefixEther ??= Mod.GetLocalization($"{category}.ArcaneForgePrefixEther");
			ArcaneForgePrefixStar ??= Mod.GetLocalization($"{category}.ArcaneForgePrefixStar");
			ArcaneForgePrefixHeavenly ??= Mod.GetLocalization($"{category}.ArcaneForgePrefixHeavenly");
			ArcaneForgePrefixGodly ??= Mod.GetLocalization($"{category}.ArcaneForgePrefixGodly");
			ArcaneForgeHeader ??= Mod.GetLocalization($"{category}.ArcaneForgeHeader");
			ArcaneForgeDamageDesc ??= Mod.GetLocalization($"{category}.ArcaneForgeDamageDesc");
			ArcaneForgeAttackSpeedDesc ??= Mod.GetLocalization($"{category}.ArcaneForgeAttackSpeedDesc");
			ArcaneForgeCritChanceDesc ??= Mod.GetLocalization($"{category}.ArcaneForgeCritChanceDesc");
			ArcaneForgeKnockbackDesc ??= Mod.GetLocalization($"{category}.ArcaneForgeKnockbackDesc");
			AlchemicalDescPageOne ??= Mod.GetLocalization($"{category}.AlchemicalDescPageOne");
			AlchemicalDescPageTwo ??= Mod.GetLocalization($"{category}.AlchemicalDescPageTwo");
			AlchemicalDescPageThree ??= Mod.GetLocalization($"{category}.AlchemicalDescPageThree");
			AlchemicalDescPageFour ??= Mod.GetLocalization($"{category}.AlchemicalDescPageFour");
			AlchemicalDescPageFive ??= Mod.GetLocalization($"{category}.AlchemicalDescPageFive");
			close_msg ??= Mod.GetLocalization($"{category}.close_msg");
			anyBar_msg ??= Mod.GetLocalization($"{category}.anyBar_msg");
			anyFuel_msg ??= Mod.GetLocalization($"{category}.anyFuel_msg");
			output_msg ??= Mod.GetLocalization($"{category}.output_msg");
			heating_msg ??= Mod.GetLocalization($"{category}.heating_msg");
			rolling_msg ??= Mod.GetLocalization($"{category}.rolling_msg");
			waiting_msg ??= Mod.GetLocalization($"{category}.waiting_msg");
			nomaterial_msg ??= Mod.GetLocalization($"{category}.nomaterial_msg");
			nofuel_msg ??= Mod.GetLocalization($"{category}.nofuel_msg");
			heatingprocess_msg ??= Mod.GetLocalization($"{category}.heatingprocess_msg");
			rollingprocess_msg ??= Mod.GetLocalization($"{category}.rollingprocess_msg");
			bloodshed_textclick ??= Mod.GetLocalization($"{category}.bloodshed_tooltipclick");
			naivety_textclick ??= Mod.GetLocalization($"{category}.naivety_textclick");
			scourge_textclick ??= Mod.GetLocalization($"{category}.scourge_textclick");
			mor_textclick ??= Mod.GetLocalization($"{category}.mor_textclick");
			purplefire_textclick ??= Mod.GetLocalization($"{category}.purplefire_textclick");
			fragility_textclick ??= Mod.GetLocalization($"{category}.fragility_textclick");
			omnipotence_textclick ??= Mod.GetLocalization($"{category}.omnipotence_textclick");
			significance_textclick ??= Mod.GetLocalization($"{category}.significance_textclick");
			bloodshed_textclick2 ??= Mod.GetLocalization($"{category}.bloodshed_tooltipclick2");
			naivety_textclick2 ??= Mod.GetLocalization($"{category}.naivety_textclick2");
			scourge_textclick2 ??= Mod.GetLocalization($"{category}.scourge_textclick2");
			mor_textclick2 ??= Mod.GetLocalization($"{category}.mor_textclick2");
			purplefire_textclick2 ??= Mod.GetLocalization($"{category}.purplefire_textclick2");
			fragility_textclick2 ??= Mod.GetLocalization($"{category}.fragility_textclick2");
			omnipotence_textclick2 ??= Mod.GetLocalization($"{category}.omnipotence_textclick2");
			significance_textclick2 ??= Mod.GetLocalization($"{category}.significance_textclick2");
			string category2 = "Mechan";
			damage_text ??= Mod.GetLocalization($"{category2}.damage_text");
			energyuse_text1 ??= Mod.GetLocalization($"{category2}.energyuse_text1");
			energyuse_text2 ??= Mod.GetLocalization($"{category2}.energyuse_text2");
			warning_text1 ??= Mod.GetLocalization($"{category2}.warning_text1");
			string category3 = "Worldgen_messages";
			antique_gen ??= Mod.GetLocalization($"{category3}.antique_gen");
			etherionite_gen ??= Mod.GetLocalization($"{category3}.etherionite_gen");
			string category4 = "Runes";
			filth_rune_desc ??= Mod.GetLocalization($"{category4}.filth_rune_desc");
			steampunk_rune_desc ??= Mod.GetLocalization($"{category4}.steampunk_rune_desc");
			elder_rune_desc ??= Mod.GetLocalization($"{category4}.elder_rune_desc");
			flame_rune_desc ??= Mod.GetLocalization($"{category4}.flame_rune_desc");
			string category5 = "BuffDesc";
			filth_disease_desc ??= Mod.GetLocalization($"{category5}.filth_disease_desc");
			string category6 = "MonitormiseryDesc";
			core_of_trouble_desc ??= Mod.GetLocalization($"{category6}.core_of_trouble_desc");
			string category7 = "FishingDesc";
			fishing_mastery ??= Mod.GetLocalization($"{category7}.fishing_mastery");
			ForestFruit_desc ??= Mod.GetLocalization($"{category7}.ForestFruit_desc");
			CaveFruit_desc ??= Mod.GetLocalization($"{category7}.CaveFruit_desc");
			string category8 = "BestiaryDesc";
			Foulmouthed_frog_desc ??= Mod.GetLocalization($"{category8}.Foulmouthed_frog_desc");
			GiantDragonfly_desc ??= Mod.GetLocalization($"{category8}.GiantDragonfly_desc");
			WatcherEye_desc ??= Mod.GetLocalization($"{category8}.WatcherEye_desc");
			ValueDevourer_desc ??= Mod.GetLocalization($"{category8}.ValueDevourer_desc");
		}
	}
	public class Lore_tooltips : GlobalItem
	{
		#region Lore Tooltip System
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (item.type == ModContent.ItemType<EyeOfMage>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.eom_text));
				}
			}
			if (item.type == ModContent.ItemType<UnstableBlade>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.burning_label_text));
				}
			}
			if (item.type == ModContent.ItemType<ArcaneFlower>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.ancient_flower_text));
				}
			}
			if (item.type == ModContent.ItemType<CoreOfTrouble>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.anxiety_text));
				}
			}
			if (item.type == ModContent.ItemType<GauntletOfMage>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.magical_glove_text));
				}
			}
			if (item.type == ModContent.ItemType<StarEarring>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.magical_earring_text));
				}
			}
			if (item.type == ModContent.ItemType<TheDawnMoon>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.moon_dawn_text));
				}
			}
			if (item.type == ModContent.ItemType<HeartOfSerenity>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.heart_of_serenity));
				}
			}
			if (item.type == ModContent.ItemType<DarkPeddler>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.dark_peddler_text));
				}
			}
			if (item.type == ModContent.ItemType<DarkPeddlerTrue>())
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Language_Tooltips.hint_text));
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
				{
					tooltips.Add(new TooltipLine(Mod, "FullTooltip", (string)Language_Tooltips.true_peddler_text));
				}
			}
		}
		#endregion
	}
	public class Language_Tooltips : ModSystem
	{
		#region Vars with text
		public static LocalizedText hint_text { get; private set; }
		public static LocalizedText eom_text { get; private set; }
		public static LocalizedText burning_label_text { get; private set; }
		public static LocalizedText ancient_flower_text { get; private set; }
		public static LocalizedText anxiety_text { get; private set; }
		public static LocalizedText magical_glove_text { get; private set; }
		public static LocalizedText magical_earring_text { get; private set; }
		public static LocalizedText moon_dawn_text { get; private set; }
		public static LocalizedText heart_of_serenity { get; private set; }
		public static LocalizedText dark_peddler_text { get; private set; }
		public static LocalizedText true_peddler_text { get; private set; }
		#endregion
		#region Load text
		public override void Load()
		{
			string category = "LoreTooltips";
			hint_text ??= Mod.GetLocalization($"{category}.hint_text");
			eom_text ??= Mod.GetLocalization($"{category}.eom_text");
			burning_label_text ??= Mod.GetLocalization($"{category}.burning_label_text");
			ancient_flower_text ??= Mod.GetLocalization($"{category}.ancient_flower_text");
			anxiety_text ??= Mod.GetLocalization($"{category}.anxiety_text");
			magical_glove_text ??= Mod.GetLocalization($"{category}.magical_glove_text");
			magical_earring_text ??= Mod.GetLocalization($"{category}.magical_earring_text");
			moon_dawn_text ??= Mod.GetLocalization($"{category}.moon_dawn_text");
			heart_of_serenity ??= Mod.GetLocalization($"{category}.heart_of_serenity");
			dark_peddler_text ??= Mod.GetLocalization($"{category}.dark_peddler_text");
			true_peddler_text ??= Mod.GetLocalization($"{category}.true_peddler_text");
		}
		#endregion
		#endregion
	}
}
