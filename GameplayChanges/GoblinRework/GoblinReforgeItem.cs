using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.GameplayChanges.GoblinRework
{
    public class GoblinReforgeGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public float CustomDamageBonus;
        public float CustomCritBonus;
        public float CustomMoveSpeedBonus;
        public float CustomWingTimeBonus;
        public float CustomAttackSpeedBonus;
        public float CustomMeleeSizeBonus;
        public float CustomManaCostBonus;

        public float CustomMaxManaBonus;
        public float CustomDebuffReductionBonus;
        public float CustomDefenseBonus;
        public float CustomAccDamageBonus;
        public float CustomAccCritBonus;
        public float CustomCritDamageBonus;
        public float CustomLuckBonus;
        public float CustomMinionSlotBonus;

        public float CustomWhipRangeBonus;
        public float CustomArmorPenetrationBonus;
        public float CustomShootSpeedBonus;
        public bool HasCustomReforge => 
        CustomDamageBonus > 0 
        || CustomCritBonus > 0 
        || CustomMoveSpeedBonus > 0 
        || CustomWingTimeBonus > 0 
        || CustomAttackSpeedBonus > 0 
        || CustomMeleeSizeBonus > 0 
        || CustomManaCostBonus > 0 
        || CustomMaxManaBonus > 0 
        || CustomDebuffReductionBonus > 0 
        || CustomDefenseBonus > 0 
        || CustomAccDamageBonus > 0 
        || CustomAccCritBonus > 0 
        || CustomCritDamageBonus > 0 
        || CustomLuckBonus > 0 
        || CustomWhipRangeBonus > 0 
        || CustomMinionSlotBonus > 0 
        || CustomArmorPenetrationBonus > 0 
        || CustomShootSpeedBonus > 0;

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            GoblinReforgeGlobalItem myClone = (GoblinReforgeGlobalItem)base.Clone(item, itemClone);

            myClone.CustomDamageBonus = CustomDamageBonus; //all gains are transmitted on clone
            myClone.CustomCritBonus = CustomCritBonus;
            myClone.CustomMoveSpeedBonus = CustomMoveSpeedBonus;
            myClone.CustomWingTimeBonus = CustomWingTimeBonus;
            myClone.CustomAttackSpeedBonus = CustomAttackSpeedBonus;
            myClone.CustomMeleeSizeBonus = CustomMeleeSizeBonus;
            myClone.CustomManaCostBonus = CustomManaCostBonus;

            myClone.CustomMaxManaBonus = CustomMaxManaBonus;
            myClone.CustomDebuffReductionBonus = CustomDebuffReductionBonus;
            myClone.CustomDefenseBonus = CustomDefenseBonus;
            myClone.CustomAccDamageBonus = CustomAccDamageBonus;
            myClone.CustomAccCritBonus = CustomAccCritBonus;
            myClone.CustomCritDamageBonus = CustomCritDamageBonus;
            myClone.CustomLuckBonus = CustomLuckBonus;
            myClone.CustomMinionSlotBonus = CustomMinionSlotBonus;

            myClone.CustomWhipRangeBonus = CustomWhipRangeBonus;
            myClone.CustomArmorPenetrationBonus = CustomArmorPenetrationBonus;
            myClone.CustomShootSpeedBonus = CustomShootSpeedBonus;

            return myClone;
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (CustomDamageBonus > 0) tag["CustomDamageBonus"] = CustomDamageBonus;
            if (CustomCritBonus > 0) tag["CustomCritBonus"] = CustomCritBonus;
            if (CustomMoveSpeedBonus > 0) tag["CustomMoveSpeedBonus"] = CustomMoveSpeedBonus;
            if (CustomWingTimeBonus > 0) tag["CustomWingTimeBonus"] = CustomWingTimeBonus;
            if (CustomAttackSpeedBonus > 0) tag["CustomAttackSpeedBonus"] = CustomAttackSpeedBonus;
            if (CustomMeleeSizeBonus > 0) tag["CustomMeleeSizeBonus"] = CustomMeleeSizeBonus;
            if (CustomManaCostBonus > 0) tag["CustomManaCostBonus"] = CustomManaCostBonus;

            if (CustomMaxManaBonus > 0) tag["CustomMaxManaBonus"] = CustomMaxManaBonus;
            if (CustomDebuffReductionBonus > 0) tag["CustomDebuffReductionBonus"] = CustomDebuffReductionBonus;
            if (CustomDefenseBonus > 0) tag["CustomDefenseBonus"] = CustomDefenseBonus;
            if (CustomAccDamageBonus > 0) tag["CustomAccDamageBonus"] = CustomAccDamageBonus;
            if (CustomAccCritBonus > 0) tag["CustomAccCritBonus"] = CustomAccCritBonus;
            if (CustomCritDamageBonus > 0) tag["CustomCritDamageBonus"] = CustomCritDamageBonus;
            if (CustomLuckBonus > 0) tag["CustomLuckBonus"] = CustomLuckBonus;
            if (CustomMinionSlotBonus > 0) tag["CustomMinionSlotBonus"] = CustomMinionSlotBonus;

            if (CustomWhipRangeBonus > 0) tag["CustomWhipRangeBonus"] = CustomWhipRangeBonus;
            if (CustomArmorPenetrationBonus > 0) tag["CustomArmorPenetrationBonus"] = CustomArmorPenetrationBonus;
            if (CustomShootSpeedBonus > 0) tag["CustomShootSpeedBonus"] = CustomShootSpeedBonus;
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            CustomDamageBonus = tag.GetFloat("CustomDamageBonus");
            CustomCritBonus = tag.GetFloat("CustomCritBonus");
            CustomMoveSpeedBonus = tag.GetFloat("CustomMoveSpeedBonus");
            CustomWingTimeBonus = tag.GetFloat("CustomWingTimeBonus");
            CustomAttackSpeedBonus = tag.GetFloat("CustomAttackSpeedBonus");
            CustomMeleeSizeBonus = tag.GetFloat("CustomMeleeSizeBonus");
            CustomManaCostBonus = tag.GetFloat("CustomManaCostBonus");

            CustomMaxManaBonus = tag.GetFloat("CustomMaxManaBonus");
            CustomDebuffReductionBonus = tag.GetFloat("CustomDebuffReductionBonus");
            CustomDefenseBonus = tag.GetFloat("CustomDefenseBonus");
            CustomAccDamageBonus = tag.GetFloat("CustomAccDamageBonus");
            CustomAccCritBonus = tag.GetFloat("CustomAccCritBonus");
            CustomCritDamageBonus = tag.GetFloat("CustomCritDamageBonus");
            CustomLuckBonus = tag.GetFloat("CustomLuckBonus");
            CustomMinionSlotBonus = tag.GetFloat("CustomMinionSlotBonus");

            CustomWhipRangeBonus = tag.GetFloat("CustomWhipRangeBonus");
            CustomArmorPenetrationBonus = tag.GetFloat("CustomArmorPenetrationBonus");
            CustomShootSpeedBonus = tag.GetFloat("CustomShootSpeedBonus");
        }

        public override void HoldItem(Item item, Player player)
        {
            if (CustomWhipRangeBonus > 0 && item.DamageType == DamageClass.SummonMeleeSpeed)
            {
                player.whipRangeMultiplier += CustomWhipRangeBonus;
            }
            if (CustomArmorPenetrationBonus > 0)
            {
                player.GetArmorPenetration(DamageClass.Generic) += CustomArmorPenetrationBonus;
            }
        }

        public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
        {
            if (CustomManaCostBonus > 0)
            {
                mult *= 1f - CustomManaCostBonus;
            }
        }
        public override float UseSpeedMultiplier(Item item, Player player)
        {
            if (CustomAttackSpeedBonus > 0)
            {
                return 1f + CustomAttackSpeedBonus;
            }
            return 1f;
        }

        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (CustomShootSpeedBonus > 0)
            {
                velocity *= 1f + CustomShootSpeedBonus;
            }
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (CustomDamageBonus > 0) player.GetDamage(DamageClass.Generic) += CustomDamageBonus;

            if (CustomCritBonus > 0) player.GetCritChance(DamageClass.Generic) += CustomCritBonus;

            if (CustomMoveSpeedBonus > 0) player.moveSpeed += CustomMoveSpeedBonus;

            if (CustomWingTimeBonus > 0 && player.wingTimeMax > 0)
            {
                player.wingTimeMax += (int)(player.wingTimeMax * CustomWingTimeBonus);
            }

            if (CustomAttackSpeedBonus > 0) player.GetAttackSpeed(DamageClass.Generic) += CustomAttackSpeedBonus;

            if (CustomMeleeSizeBonus > 0)
            {
                player.GetModPlayer<GoblinForgingPlayer>().MeleeSizeMultBonus += CustomMeleeSizeBonus;
            }

            if (CustomManaCostBonus > 0) player.manaCost -= CustomManaCostBonus;

            if (CustomMaxManaBonus > 0) player.statManaMax2 += (int)(player.statManaMax * CustomMaxManaBonus);

            if (CustomDebuffReductionBonus > 0)
            {
                player.GetModPlayer<GoblinForgingPlayer>().CustomDebuffReductionTotal += CustomDebuffReductionBonus;
            }

            if (CustomDefenseBonus > 0)
            {
                player.statDefense += Math.Max(1, player.statDefense * CustomDefenseBonus);
            }

            if (CustomAccDamageBonus > 0) player.GetDamage(DamageClass.Generic) += CustomAccDamageBonus;

            if (CustomAccCritBonus > 0) player.GetCritChance(DamageClass.Generic) += CustomAccCritBonus;

            if (CustomCritDamageBonus > 0) player.GetModPlayer<GoblinForgingPlayer>().CustomCritDamageTotal += CustomCritDamageBonus;

            if (CustomLuckBonus > 0) player.luck += CustomLuckBonus;

            if (CustomMinionSlotBonus > 0)
            {
                player.GetModPlayer<GoblinForgingPlayer>().UnholyMinionBonus += CustomMinionSlotBonus;
            }
        }

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (CustomDamageBonus > 0)
            {
                damage += CustomDamageBonus;
            }
        }

        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            if (CustomCritBonus > 0)
            {
                crit += CustomCritBonus;
            }
        }

        public override void ModifyItemScale(Item item, Player player, ref float scale)
        {
            if (CustomMeleeSizeBonus > 0 && (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed))
            {
                scale *= 1f + CustomMeleeSizeBonus;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (HasCustomReforge)
            {
                Color modifierColor = new Color(120, 230, 120);

                if (CustomDamageBonus > 0)
                {
                    string text = $"+{CustomDamageBonus * 100:0.#}% " + GReforgeLanguageSystem.GRdamage;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeDamage", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomCritBonus > 0)
                {
                    string text = $"+{CustomCritBonus:0.#}% " + GReforgeLanguageSystem.GRcrit;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeCrit", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomMoveSpeedBonus > 0)
                {
                    string text = $"+{CustomMoveSpeedBonus * 100:0.#}% " + GReforgeLanguageSystem.GRmovespeed;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeMoveSpeed", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomWingTimeBonus > 0)
                {
                    string text = $"+{CustomWingTimeBonus * 100:0.#}% " + GReforgeLanguageSystem.GRwingtime;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeWingTime", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomAttackSpeedBonus > 0)
                {
                    string text = $"+{CustomAttackSpeedBonus * 100:0.#}% " + GReforgeLanguageSystem.GRattackspeed;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeAttackSpeed", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomMeleeSizeBonus > 0)
                {
                    string text = $"+{CustomMeleeSizeBonus * 100:0.#}% " + GReforgeLanguageSystem.GRscale;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeMeleeSize", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomManaCostBonus > 0)
                {
                    string text = $"-{CustomManaCostBonus * 100:0.#}% " + GReforgeLanguageSystem.GRmanacost;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeManaCost", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomArmorPenetrationBonus > 0)
                {
                    string text = $"+{(int)CustomArmorPenetrationBonus} " + GReforgeLanguageSystem.GRpenetration;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeArmorPenetration", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomShootSpeedBonus > 0)
                {
                    string text = $"+{CustomShootSpeedBonus * 100:0.#}% " + GReforgeLanguageSystem.GRspeedproj;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeShootSpeed", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomMaxManaBonus > 0)
                {
                    string text = $"+{CustomMaxManaBonus * 100:0.#}% " + GReforgeLanguageSystem.GRmaxmana;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeMaxMana", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomDebuffReductionBonus > 0)
                {
                    string text = $"-{CustomDebuffReductionBonus * 100:0.#}% " + GReforgeLanguageSystem.GRdebufftime;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeDebuffReduction", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomDefenseBonus > 0)
                {
                    string text = $"+{CustomDefenseBonus * 100:0.#}% " + GReforgeLanguageSystem.GRdefense;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeDefense", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomAccDamageBonus > 0) //accs
                {
                    string text = $"+{CustomAccDamageBonus * 100:0.#}% " + GReforgeLanguageSystem.GRAccs_damage;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeAccDamage", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomAccCritBonus > 0)
                {
                    string text = $"+{CustomAccCritBonus:0.#}% " + GReforgeLanguageSystem.GRAccs_critchance;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeAccCrit", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomCritDamageBonus > 0)
                {
                    string text = $"+{CustomCritDamageBonus * 100:0.#}% " + GReforgeLanguageSystem.GRAccs_critdamage;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeCritDamage", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomLuckBonus > 0)
                {
                    string text = $"+{CustomLuckBonus * 100:0.###}% " + GReforgeLanguageSystem.GRAccs_luck;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeLuck", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomMinionSlotBonus > 0)
                {
                    string text = $"+{CustomMinionSlotBonus:0.#} " + GReforgeLanguageSystem.GRAccs_minionslot;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeMinionSlot", text) { IsModifier = true, OverrideColor = modifierColor });
                }
                if (CustomWhipRangeBonus > 0)
                {
                    string text = $"+{CustomWhipRangeBonus * 100:0.#}% " + GReforgeLanguageSystem.GRAccs_whiprange;
                    tooltips.Add(new TooltipLine(Mod, "CustomReforgeWhipRange", text) { IsModifier = true, OverrideColor = modifierColor });
                }
            }
        }
    }
    public class GReforgeLanguageSystem : ModSystem
	{
		public static LocalizedText GRdamage { get; private set; }
		public static LocalizedText GRcrit { get; private set; }
        public static LocalizedText GRmovespeed { get; private set; }
        public static LocalizedText GRwingtime { get; private set; }
        public static LocalizedText GRattackspeed { get; private set; }
		public static LocalizedText GRscale { get; private set; }
        public static LocalizedText GRmanacost { get; private set; }
		public static LocalizedText GRpenetration { get; private set; }
        public static LocalizedText GRspeedproj { get; private set; }
        public static LocalizedText GRmaxmana { get; private set; }
		public static LocalizedText GRdebufftime { get; private set; }
        public static LocalizedText GRdefense { get; private set; }
		public static LocalizedText GRAccs_damage { get; private set; }
        public static LocalizedText GRAccs_critchance { get; private set; }
		public static LocalizedText GRAccs_critdamage { get; private set; }
        public static LocalizedText GRAccs_luck { get; private set; }
        public static LocalizedText GRAccs_minionslot { get; private set; }
        public static LocalizedText GRAccs_whiprange { get; private set; }

        public override void Load()
        {
            string category = "GoblinRegorgeRework";
			GRdamage ??= Mod.GetLocalization($"{category}.GRdamage");
			GRcrit ??= Mod.GetLocalization($"{category}.GRcrit");
			GRmovespeed ??= Mod.GetLocalization($"{category}.GRmovespeed");
			GRwingtime ??= Mod.GetLocalization($"{category}.GRwingtime");
			GRattackspeed ??= Mod.GetLocalization($"{category}.GRattackspeed");
			GRscale ??= Mod.GetLocalization($"{category}.GRscale");
            GRmanacost ??= Mod.GetLocalization($"{category}.GRmanacost");
			GRpenetration ??= Mod.GetLocalization($"{category}.GRpenetration");
			GRspeedproj ??= Mod.GetLocalization($"{category}.GRspeedproj");
			GRmaxmana ??= Mod.GetLocalization($"{category}.GRmaxmana");
            GRdebufftime ??= Mod.GetLocalization($"{category}.GRdebufftime");
            GRdefense ??= Mod.GetLocalization($"{category}.GRdefense");
            GRAccs_damage ??= Mod.GetLocalization($"{category}.GRAccs_damage");
			GRAccs_critchance ??= Mod.GetLocalization($"{category}.GRAccs_critchance");
			GRAccs_critdamage ??= Mod.GetLocalization($"{category}.GRAccs_critdamage");
			GRAccs_luck ??= Mod.GetLocalization($"{category}.GRAccs_luck");
            GRAccs_minionslot ??= Mod.GetLocalization($"{category}.GRAccs_minionslot");
            GRAccs_whiprange ??= Mod.GetLocalization($"{category}.GRAccs_whiprange");
        }
	}
}
