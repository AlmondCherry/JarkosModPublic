using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.systems
{
    public class FrogLangSystem : ModSystem
    {
        public static LocalizedText[] Swears { get; private set; }
        public static LocalizedText[] Croaks { get; private set; }
        public override void Load()
        {
            string category = "NPCs.Foulmouthed_frog.Chat";

            Swears = new LocalizedText[]
            {
                Mod.GetLocalization($"{category}.Swear1"), Mod.GetLocalization($"{category}.Swear2"), Mod.GetLocalization($"{category}.Swear3"), Mod.GetLocalization($"{category}.Swear4"), Mod.GetLocalization($"{category}.Swear5")
            };
            Croaks = new LocalizedText[]
            {
                Mod.GetLocalization($"{category}.Croak1"), Mod.GetLocalization($"{category}.Croak2"), Mod.GetLocalization($"{category}.Croak3")
            };
        }
    }
}
