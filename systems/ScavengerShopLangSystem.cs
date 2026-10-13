using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.systems
{
    public class ScavengerShopLangSystem : ModSystem
    {
        public static LocalizedText SelectProductText { get; private set; }
        public static LocalizedText BuyText { get; private set; }
        public static LocalizedText QuantityText { get; private set; }
        public static LocalizedText MaterialDescText { get; private set; }
        public static LocalizedText NotEnoughPollenText { get; private set; }
        public static LocalizedText NPCNameText { get; private set; }
        public static LocalizedText ChatButtonText { get; private set; }
        public static LocalizedText Dialog1Text { get; private set; }
        public static LocalizedText Dialog2Text { get; private set; }
        public static LocalizedText Dialog3Text { get; private set; }
        public static LocalizedText AngryDialog1Text { get; private set; }
        public static LocalizedText AngryDialog2Text { get; private set; }
        public static LocalizedText SpawnMessageText { get; private set; }
        public static LocalizedText EssencesButtonText { get; private set; }
        public static LocalizedText EssenceChanceDialogText { get; private set; }
        public override void Load()
        {
            string category = "ScavengerShop";
            SelectProductText ??= Mod.GetLocalization($"{category}.SelectProductText");
            BuyText ??= Mod.GetLocalization($"{category}.BuyText");
            QuantityText ??= Mod.GetLocalization($"{category}.QuantityText");
            MaterialDescText ??= Mod.GetLocalization($"{category}.MaterialDescText");
            NotEnoughPollenText ??= Mod.GetLocalization($"{category}.NotEnoughPollenText");
            NPCNameText ??= Mod.GetLocalization($"{category}.NPCNameText");
            ChatButtonText ??= Mod.GetLocalization($"{category}.ChatButtonText");
            Dialog1Text ??= Mod.GetLocalization($"{category}.Dialog1Text");
            Dialog2Text ??= Mod.GetLocalization($"{category}.Dialog2Text");
            Dialog3Text ??= Mod.GetLocalization($"{category}.Dialog3Text");
            AngryDialog1Text ??= Mod.GetLocalization($"{category}.AngryDialog1Text");
            AngryDialog2Text ??= Mod.GetLocalization($"{category}.AngryDialog2Text");
            SpawnMessageText ??= Mod.GetLocalization($"{category}.SpawnMessageText");
            EssencesButtonText ??= Mod.GetLocalization($"{category}.EssencesButtonText");
            EssenceChanceDialogText ??= Mod.GetLocalization($"{category}.EssenceChanceDialogText");
        }
    }
}
