using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.GameplayChanges
{
    public class BossSummonGlobalItem : GlobalItem
    {
        public override bool ConsumeItem(Item item, Player player)
        {
            if (Jarkos_config.Instance != null && Jarkos_config.Instance.small_gameplay_changes)
            {
                if (IsBossSummonItem(item))
                {
                    return false;
                }
            }
            return base.ConsumeItem(item, player);
        }

        public static bool IsBossSummonItem(Item item)
        {
            if (item == null || item.IsAir) return false;

            switch (item.type)
            {
                case ItemID.SuspiciousLookingEye:
                case ItemID.WormFood:
                case ItemID.BloodySpine:
                case ItemID.SlimeCrown:
                case ItemID.Abeemination:
                case ItemID.GuideVoodooDoll:
                case ItemID.ClothierVoodooDoll:
                case ItemID.MechanicalEye:
                case ItemID.MechanicalWorm:
                case ItemID.MechanicalSkull:
                case ItemID.LihzahrdPowerCell:
                case ItemID.TruffleWorm:
                case ItemID.CelestialSigil:
                case ItemID.DeerThing:
                case ItemID.GoblinBattleStandard:
                case ItemID.PirateMap:
                case ItemID.SnowGlobe:
                case ItemID.SolarTablet:
                case ItemID.NaughtyPresent:
                case ItemID.PumpkinMoonMedallion:
                    return true;
            }

            if (item.ModItem != null)
            {
                string name = item.ModItem.Name.ToLower();
                if (name.Contains("summon"))
                    return true;
            }

            return false;
        }
    }
}
