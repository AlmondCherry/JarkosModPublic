using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using jarkos.core.Data;
using System.Linq;

namespace jarkos.items
{
    public abstract class BaseRuneItem : ModItem
    {
        public EssenceType[] Types;
        public int[] Amounts;
        public abstract int MaxCapacity { get; }
        public abstract int MaxSlots { get; }
        public int TotalEssence => Amounts != null ? Amounts.Sum() : 0;
        public override void SetDefaults()
        {
            Types = new EssenceType[MaxSlots];
            Amounts = new int[MaxSlots];
            for (int i = 0; i < MaxSlots; i++) Types[i] = (EssenceType)(-1);
            SafeSetDefaults();
        }
        public virtual void SafeSetDefaults() {}
        public override void SaveData(TagCompound tag)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                tag[$"Type{i}"] = (int)Types[i];
                tag[$"Amount{i}"] = Amounts[i];
            }
        }
        public override void LoadData(TagCompound tag)
        {
            if (this is InitialRuneItem && tag.ContainsKey("Type1"))
            {
                Types[0] = (EssenceType)tag.GetInt("Type1");
                Amounts[0] = tag.GetInt("Amount1");
                if (tag.ContainsKey("Type2"))
                {
                    Types[1] = (EssenceType)tag.GetInt("Type2");
                    Amounts[1] = tag.GetInt("Amount2");
                }
            }
            for (int i = 0; i < MaxSlots; i++)
            {
                if (tag.ContainsKey($"Type{i}"))
                {
                    Types[i] = (EssenceType)tag.GetInt($"Type{i}");
                    Amounts[i] = tag.GetInt($"Amount{i}");
                }
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "RuneCapacity", $"Capacity: {TotalEssence} / {MaxCapacity}"));
            for (int i = 0; i < MaxSlots; i++)
            {
                if (Types[i] != (EssenceType)(-1) && Amounts[i] > 0)
                {
                    tooltips.Add(new TooltipLine(Mod, $"RuneType{i}", $"Contains: {Amounts[i]} {EssencePalette.DisplayName(Types[i])}")
                    {
                        OverrideColor = EssencePalette.ColorOf(Types[i])
                    });
                }
            }
        }
    }
}
