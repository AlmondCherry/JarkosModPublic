using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.GameplayChanges.GoblinRework
{
    public class UnholyPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override bool CanRoll(Item item) => true;

        public override void ModifyValue(ref float valueMult) => valueMult *= 1.15f;
    }
    public class VirtuousPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override bool CanRoll(Item item) => true;

        public override void ModifyValue(ref float valueMult) => valueMult *= 1.15f;
    }
    public class EthicalPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override bool CanRoll(Item item) => true;

        public override void ModifyValue(ref float valueMult) => valueMult *= 1.15f;
    }
    public class LewdPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override bool CanRoll(Item item) => true;

        public override void ModifyValue(ref float valueMult) => valueMult *= 1.18f;
    }
    public class PunishingPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override bool CanRoll(Item item) => true;

        public override void ModifyValue(ref float valueMult) => valueMult *= 1.15f;
    }
    public class HealerPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override bool CanRoll(Item item) => true;

        public override void ModifyValue(ref float valueMult) => valueMult *= 1.15f;
    }
    public class UnbreakablePrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override bool CanRoll(Item item) => true;

        public override void ModifyValue(ref float valueMult) => valueMult *= 1.15f;
    }
    public class AlchemicalPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override bool CanRoll(Item item) => true;

        public override void ModifyValue(ref float valueMult) => valueMult *= 1.15f;
    }
    public class GoblinForgingPlayer : ModPlayer
    {
        public float UnholyMinionBonus;
        public float LewdRegenBonus;
        public float ProjSpeedBonus;
        public float WingTimeMultBonus;
        public float MeleeSizeMultBonus;

        public int PunishingBonusCount;
        public int HealerBonusCount;
        public int HealerTimer;
        public int AlchemicalBonusCount;
        public float AlchemicalTimer;
        public float CustomDebuffReductionTotal;
        public float CustomCritDamageTotal;

        public override void ResetEffects()
        {
            UnholyMinionBonus = 0f;
            LewdRegenBonus = 0f;
            ProjSpeedBonus = 0f;
            WingTimeMultBonus = 0f;
            MeleeSizeMultBonus = 0f;
            CustomDebuffReductionTotal = 0f;
            CustomCritDamageTotal = 0f;

            PunishingBonusCount = 0;
            HealerBonusCount = 0;
            AlchemicalBonusCount = 0;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (CustomCritDamageTotal > 0f)
            {
                modifiers.CritDamage += CustomCritDamageTotal;
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (PunishingBonusCount > 0 && info.Damage > 0 && info.DamageSource.SourceNPCIndex >= 0 && info.DamageSource.SourceNPCIndex < Main.maxNPCs)
            {
                NPC targetNPC = Main.npc[info.DamageSource.SourceNPCIndex];
                if (targetNPC.active && !targetNPC.friendly)
                {
                    int reflectDamage = (int)(info.Damage * 0.025f * PunishingBonusCount);
                    if (reflectDamage < 1) reflectDamage = 1;
                    Player.ApplyDamageToNPC(targetNPC, reflectDamage, 0f, 0, false);
                }
            }

            if (HealerBonusCount > 0 && info.Damage > 0)
            {
                HealerTimer = 300;
            }
        }

        private int[] lastBuffTime;

        public override void PreUpdateBuffs()
        {
            if (AlchemicalBonusCount > 0 || CustomDebuffReductionTotal > 0f)
            {
                if (lastBuffTime == null || lastBuffTime.Length != Player.MaxBuffs)
                {
                    lastBuffTime = new int[Player.MaxBuffs];
                }

                float reductionMult = 1f - (0.025f * AlchemicalBonusCount + CustomDebuffReductionTotal);
                if (reductionMult < 0.1f) reductionMult = 0.1f;

                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    int type = Player.buffType[i];
                    int time = Player.buffTime[i];

                    if (type > 0 && time > 0 && Main.debuff[type])
                    {

                        if (type != BuffID.PotionSickness && type != BuffID.ManaSickness)
                        {

                            if (time > lastBuffTime[i] + 5)
                            {
                                int newTime = (int)(time * reductionMult);
                                Player.buffTime[i] = newTime;
                            }
                        }
                    }
                    lastBuffTime[i] = Player.buffTime[i];
                }
            }
        }

        public override void PostUpdateEquips()
        {
            if (UnholyMinionBonus > 0f)
            {
                Player.maxMinions += (int)UnholyMinionBonus;
            }
            if (LewdRegenBonus > 0f)
            {
                Player.lifeRegen += (int)LewdRegenBonus;
            }
            if (WingTimeMultBonus > 0f && Player.wingTimeMax > 0)
            {
                Player.wingTimeMax = (int)(Player.wingTimeMax * (1f + WingTimeMultBonus));
            }

            if (HealerTimer > 0)
            {
                HealerTimer--;
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen += (int)Math.Max(1, Math.Ceiling(Player.lifeRegen * 0.075f * HealerBonusCount));
                }
                else
                {
                    Player.lifeRegen += 1;
                }
                Player.manaRegenBonus += (int)Math.Max(1, Math.Ceiling(Player.manaRegenBonus * 0.075f * HealerBonusCount));
            }

            if (AlchemicalBonusCount > 0)
            {
                AlchemicalTimer += 0.025f * AlchemicalBonusCount;
                while (AlchemicalTimer >= 1.0f)
                {
                    AlchemicalTimer -= 1.0f;
                    for (int i = 0; i < Player.MaxBuffs; i++)
                    {
                        int type = Player.buffType[i];
                        if (type > 0 && Player.buffTime[i] > 0 && Main.debuff[type])
                        {
                            if (type != BuffID.PotionSickness && type != BuffID.ManaSickness)
                            {
                                Player.buffTime[i]--;
                            }
                        }
                    }
                }
            }
        }
    }
    public class GoblinForgingGlobalItem : GlobalItem
    {
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            float projSpeedBonus = player.GetModPlayer<GoblinForgingPlayer>().ProjSpeedBonus;
            if (projSpeedBonus > 0f)
            {
                velocity *= 1f + projSpeedBonus;
            }
        }

        public override void ModifyItemScale(Item item, Player player, ref float scale)
        {
            if (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed)
            {
                float sizeBonus = player.GetModPlayer<GoblinForgingPlayer>().MeleeSizeMultBonus;
                if (sizeBonus > 0f)
                {
                    scale *= 1f + sizeBonus;
                }
            }
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {

            if (item.prefix == ModContent.PrefixType<UnholyPrefix>())
            {
                player.GetModPlayer<GoblinForgingPlayer>().UnholyMinionBonus += 0.2f;
            }
            else if (item.prefix == ModContent.PrefixType<VirtuousPrefix>())
            {
                player.GetArmorPenetration(DamageClass.Generic) += 1f;
            }
            else if (item.prefix == ModContent.PrefixType<EthicalPrefix>())
            {
                player.whipRangeMultiplier += 0.10f;
            }
            else if (item.prefix == ModContent.PrefixType<LewdPrefix>())
            {
                player.statLifeMax2 += 5;
                player.GetModPlayer<GoblinForgingPlayer>().LewdRegenBonus += 0.6f; 
            }
            else if (item.prefix == ModContent.PrefixType<PunishingPrefix>())
            {
                player.endurance -= 0.04f;
                player.GetModPlayer<GoblinForgingPlayer>().PunishingBonusCount++;
            }
            else if (item.prefix == ModContent.PrefixType<HealerPrefix>())
            {
                player.GetModPlayer<GoblinForgingPlayer>().HealerBonusCount++;
            }
            else if (item.prefix == ModContent.PrefixType<UnbreakablePrefix>())
            {
                player.endurance += 0.015f;
                player.GetDamage(DamageClass.Generic) -= 0.02f;
            }
            else if (item.prefix == ModContent.PrefixType<AlchemicalPrefix>())
            {
                player.GetModPlayer<GoblinForgingPlayer>().AlchemicalBonusCount++;
            }
            else if (item.prefix == PrefixID.Jagged)
            {

                player.GetDamage(DamageClass.Generic) += 0.01f;
                player.GetCritChance(DamageClass.Generic) += 0.25f;
            }
            else if (item.prefix == PrefixID.Spiked)
            {

                player.GetDamage(DamageClass.Generic) += 0.01f;
                player.GetCritChance(DamageClass.Generic) += 0.5f;
            }
            else if (item.prefix == PrefixID.Angry)
            {

                player.GetDamage(DamageClass.Generic) += 0.005f;
                player.GetCritChance(DamageClass.Generic) += 0.75f;
            }
            else if (item.prefix == PrefixID.Menacing)
            {

                player.GetCritChance(DamageClass.Generic) += 1.0f;
            }
            else if (item.prefix == PrefixID.Hard)
            {

                player.GetKnockback(DamageClass.Generic) += 0.05f;
            }
            else if (item.prefix == PrefixID.Guarding)
            {

                player.GetKnockback(DamageClass.Generic) += 0.10f;
            }
            else if (item.prefix == PrefixID.Armored)
            {

                player.GetKnockback(DamageClass.Generic) += 0.15f;
            }
            else if (item.prefix == PrefixID.Warding)
            {

                player.GetKnockback(DamageClass.Generic) += 0.20f;
            }


            else if (item.prefix == PrefixID.Precise)
            {

                player.GetModPlayer<GoblinForgingPlayer>().ProjSpeedBonus += 0.02f;
            }
            else if (item.prefix == PrefixID.Lucky)
            {

                player.GetModPlayer<GoblinForgingPlayer>().ProjSpeedBonus += 0.035f;
            }
            else if (item.prefix == PrefixID.Brisk)
            {

                player.moveSpeed += 0.01f;
                player.GetModPlayer<GoblinForgingPlayer>().WingTimeMultBonus += 0.025f;
            }
            else if (item.prefix == PrefixID.Fleeting)
            {

                player.moveSpeed += 0.01f;
                player.GetModPlayer<GoblinForgingPlayer>().WingTimeMultBonus += 0.03f;
            }
            else if (item.prefix == PrefixID.Hasty2)
            {

                player.moveSpeed += 0.02f;
                player.GetModPlayer<GoblinForgingPlayer>().WingTimeMultBonus += 0.045f;
            }
            else if (item.prefix == PrefixID.Quick2)
            {

                player.moveSpeed += 0.02f;
                player.GetModPlayer<GoblinForgingPlayer>().WingTimeMultBonus += 0.05f;
            }
            else if (item.prefix == PrefixID.Wild)
            {

                player.GetAttackSpeed(DamageClass.Melee) += 0.01f;
                player.GetModPlayer<GoblinForgingPlayer>().MeleeSizeMultBonus += 0.015f;
            }
            else if (item.prefix == PrefixID.Rash)
            {

                player.GetAttackSpeed(DamageClass.Melee) += 0.015f;
                player.GetModPlayer<GoblinForgingPlayer>().MeleeSizeMultBonus += 0.03f;
            }
            else if (item.prefix == PrefixID.Intrepid)
            {

                player.GetAttackSpeed(DamageClass.Melee) += 0.02f;
                player.GetModPlayer<GoblinForgingPlayer>().MeleeSizeMultBonus += 0.045f;
            }
            else if (item.prefix == PrefixID.Violent)
            {

                player.GetAttackSpeed(DamageClass.Melee) += 0.025f;
                player.GetModPlayer<GoblinForgingPlayer>().MeleeSizeMultBonus += 0.06f;
            }
            else if (item.prefix == PrefixID.Arcane)
            {

                player.manaRegenBonus += 1;
                player.manaCost -= 0.04f;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.accessory)
            {
                Color modifierColor = new Color(120, 190, 120);

                if (item.prefix == PrefixID.Arcane)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMana")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Arcane.Value;
                        }
                    }
                }

                if (item.prefix == PrefixID.Jagged)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccDamage")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Jagged.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Spiked)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccDamage")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Spiked.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Angry)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccDamage")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Angry.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Menacing)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccDamage")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Menacing.Value;
                        }
                    }
                }


                else if (item.prefix == PrefixID.Hard)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccDefense")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Hard.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Guarding)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccDefense")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Guarding.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Armored)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccDefense")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Armored.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Warding)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccDefense")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Warding.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Precise)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccCrit")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Precise.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Lucky)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccCrit")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Lucky.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Brisk)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMoveSpeed")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Brisk.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Fleeting)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMoveSpeed")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Fleeting.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Hasty2)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMoveSpeed")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Hasty2.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Quick2)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMoveSpeed")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Quick2.Value;
                        }
                    }
                }


                else if (item.prefix == PrefixID.Wild)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMeleeSpeed")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Wild.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Rash)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMeleeSpeed")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Rash.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Intrepid)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMeleeSpeed")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Intrepid.Value;
                        }
                    }
                }
                else if (item.prefix == PrefixID.Violent)
                {
                    foreach (var line in tooltips)
                    {
                        if (line.Name == "PrefixAccMeleeSpeed")
                        {
                            line.Text = GReforgeTTLanguageSystem.GR_Violent.Value;
                        }
                    }
                }
                else if (item.prefix == ModContent.PrefixType<UnholyPrefix>())
                {
                    TooltipLine line = new TooltipLine(Mod, "PrefixUnholy", GReforgeTTLanguageSystem.GR_Unholy.Value)
                    {
                        IsModifier = true,
                        OverrideColor = modifierColor
                    };
                    tooltips.Add(line);
                }
                else if (item.prefix == ModContent.PrefixType<VirtuousPrefix>())
                {
                    TooltipLine line = new TooltipLine(Mod, "PrefixVirtuous", GReforgeTTLanguageSystem.GR_Virtuous.Value)
                    {
                        IsModifier = true,
                        OverrideColor = modifierColor
                    };
                    tooltips.Add(line);
                }
                else if (item.prefix == ModContent.PrefixType<EthicalPrefix>())
                {
                    TooltipLine line = new TooltipLine(Mod, "PrefixEthical", GReforgeTTLanguageSystem.GR_Ethical.Value)
                    {
                        IsModifier = true,
                        OverrideColor = modifierColor
                    };
                    tooltips.Add(line);
                }
                else if (item.prefix == ModContent.PrefixType<LewdPrefix>())
                {
                    TooltipLine lineHp = new TooltipLine(Mod, "PrefixLewdHP", GReforgeTTLanguageSystem.GR_LewdHP.Value)
                    {
                        IsModifier = true,
                        OverrideColor = modifierColor
                    };
                    TooltipLine lineRegen = new TooltipLine(Mod, "PrefixLewdRegen", GReforgeTTLanguageSystem.GR_LewdRegen.Value)
                    {
                        IsModifier = true,
                        OverrideColor = modifierColor
                    };

                    tooltips.Add(lineHp);
                    tooltips.Add(lineRegen);
                }
                else if (item.prefix == ModContent.PrefixType<PunishingPrefix>())
                {
                    tooltips.Add(new TooltipLine(Mod, "PrefixPunishing1", GReforgeTTLanguageSystem.GR_Punishing1.Value) { IsModifier = true, OverrideColor = modifierColor });
                    tooltips.Add(new TooltipLine(Mod, "PrefixPunishing2", GReforgeTTLanguageSystem.GR_Punishing2.Value) { IsModifier = true, OverrideColor = new Color(220, 100, 100) });
                }
                else if (item.prefix == ModContent.PrefixType<HealerPrefix>())
                {
                    tooltips.Add(new TooltipLine(Mod, "PrefixHealer", GReforgeTTLanguageSystem.GR_Healer.Value) { IsModifier = true, OverrideColor = modifierColor });
                }
                else if (item.prefix == ModContent.PrefixType<UnbreakablePrefix>())
                {
                    tooltips.Add(new TooltipLine(Mod, "PrefixUnbreakable1", GReforgeTTLanguageSystem.GR_Unbreakable1.Value) { IsModifier = true, OverrideColor = modifierColor });
                    tooltips.Add(new TooltipLine(Mod, "PrefixUnbreakable2", GReforgeTTLanguageSystem.GR_Unbreakable2.Value) { IsModifier = true, OverrideColor = new Color(220, 100, 100) });
                }
                else if (item.prefix == ModContent.PrefixType<AlchemicalPrefix>())
                {
                    tooltips.Add(new TooltipLine(Mod, "PrefixAlchemical", GReforgeTTLanguageSystem.GR_Alchemical.Value) { IsModifier = true, OverrideColor = modifierColor });
                }
            }
        }
    }

    public class GReforgeTTLanguageSystem : ModSystem
	{
		public static LocalizedText GR_Arcane { get; private set; }
		public static LocalizedText GR_Jagged { get; private set; }
		public static LocalizedText GR_Spiked { get; private set; }
		public static LocalizedText GR_Angry { get; private set; }
		public static LocalizedText GR_Menacing { get; private set; }
		public static LocalizedText GR_Hard { get; private set; }
		public static LocalizedText GR_Guarding { get; private set; }
		public static LocalizedText GR_Armored { get; private set; }
		public static LocalizedText GR_Warding { get; private set; }
		public static LocalizedText GR_Precise { get; private set; }
		public static LocalizedText GR_Lucky { get; private set; }
		public static LocalizedText GR_Brisk { get; private set; }
		public static LocalizedText GR_Fleeting { get; private set; }
		public static LocalizedText GR_Hasty2 { get; private set; }
		public static LocalizedText GR_Quick2 { get; private set; }
		public static LocalizedText GR_Wild { get; private set; }
		public static LocalizedText GR_Rash { get; private set; }
		public static LocalizedText GR_Intrepid { get; private set; }
		public static LocalizedText GR_Violent { get; private set; }
		public static LocalizedText GR_Unholy { get; private set; }
		public static LocalizedText GR_Virtuous { get; private set; }
		public static LocalizedText GR_Ethical { get; private set; }
		public static LocalizedText GR_LewdHP { get; private set; }
		public static LocalizedText GR_LewdRegen { get; private set; }
		public static LocalizedText GR_Punishing1 { get; private set; }
		public static LocalizedText GR_Punishing2 { get; private set; }
		public static LocalizedText GR_Healer { get; private set; }
		public static LocalizedText GR_Unbreakable1 { get; private set; }
		public static LocalizedText GR_Unbreakable2 { get; private set; }
		public static LocalizedText GR_Alchemical { get; private set; }

        public override void Load()
        {
            string category = "GoblinRegorgeRework";

			GR_Arcane ??= Mod.GetLocalization($"{category}.GR_Arcane");
			GR_Jagged ??= Mod.GetLocalization($"{category}.GR_Jagged");
			GR_Spiked ??= Mod.GetLocalization($"{category}.GR_Spiked");
			GR_Angry ??= Mod.GetLocalization($"{category}.GR_Angry");
			GR_Menacing ??= Mod.GetLocalization($"{category}.GR_Menacing");
			GR_Hard ??= Mod.GetLocalization($"{category}.GR_Hard");
			GR_Guarding ??= Mod.GetLocalization($"{category}.GR_Guarding");
			GR_Armored ??= Mod.GetLocalization($"{category}.GR_Armored");
			GR_Warding ??= Mod.GetLocalization($"{category}.GR_Warding");
			GR_Precise ??= Mod.GetLocalization($"{category}.GR_Precise");
			GR_Lucky ??= Mod.GetLocalization($"{category}.GR_Lucky");
			GR_Brisk ??= Mod.GetLocalization($"{category}.GR_Brisk");
			GR_Fleeting ??= Mod.GetLocalization($"{category}.GR_Fleeting");
			GR_Hasty2 ??= Mod.GetLocalization($"{category}.GR_Hasty2");
			GR_Quick2 ??= Mod.GetLocalization($"{category}.GR_Quick2");
			GR_Wild ??= Mod.GetLocalization($"{category}.GR_Wild");
			GR_Rash ??= Mod.GetLocalization($"{category}.GR_Rash");
			GR_Intrepid ??= Mod.GetLocalization($"{category}.GR_Intrepid");
			GR_Violent ??= Mod.GetLocalization($"{category}.GR_Violent");
			GR_Unholy ??= Mod.GetLocalization($"{category}.GR_Unholy");
			GR_Virtuous ??= Mod.GetLocalization($"{category}.GR_Virtuous");
			GR_Ethical ??= Mod.GetLocalization($"{category}.GR_Ethical");
			GR_LewdHP ??= Mod.GetLocalization($"{category}.GR_LewdHP");
			GR_LewdRegen ??= Mod.GetLocalization($"{category}.GR_LewdRegen");
			GR_Punishing1 ??= Mod.GetLocalization($"{category}.GR_Punishing1");
			GR_Punishing2 ??= Mod.GetLocalization($"{category}.GR_Punishing2");
			GR_Healer ??= Mod.GetLocalization($"{category}.GR_Healer");
			GR_Unbreakable1 ??= Mod.GetLocalization($"{category}.GR_Unbreakable1");
			GR_Unbreakable2 ??= Mod.GetLocalization($"{category}.GR_Unbreakable2");
			GR_Alchemical ??= Mod.GetLocalization($"{category}.GR_Alchemical");
        }
	}
}
