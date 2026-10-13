using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace jarkos.GameplayChanges.GoblinRework
{    public class UIReforgeSlotElement : UIElement
    {
        public GoblinReforgeUIState ParentUIState;

        public UIReforgeSlotElement(GoblinReforgeUIState parent)
        {
            ParentUIState = parent;
            Width.Set(52, 0);
            Height.Set(52, 0);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Vector2 position = GetDimensions().Position();

            ItemSlot.Draw(spriteBatch, ref ParentUIState.ReforgeItem, ItemSlot.Context.PrefixItem, position);

            Rectangle slotRect = GetDimensions().ToRectangle();
            if (slotRect.Contains(Main.MouseScreen.ToPoint()))
            {
                Main.LocalPlayer.mouseInterface = true;

                if (ParentUIState.ReforgeItem != null && !ParentUIState.ReforgeItem.IsAir)
                {
                    Main.HoverItem = ParentUIState.ReforgeItem.Clone();
                    Main.hoverItemName = ParentUIState.ReforgeItem.Name;
                }

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Item temp = Main.mouseItem.Clone();
                    Main.mouseItem = (ParentUIState.ReforgeItem != null) ? ParentUIState.ReforgeItem.Clone() : new Item();
                    ParentUIState.ReforgeItem = temp;
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
        }
    }
    public class UIReforgeButtonElement : UIElement
    {
        public GoblinReforgeUIState ParentUIState;
        private float clickTimer = 0f;

        public UIReforgeButtonElement(GoblinReforgeUIState parent)
        {
            ParentUIState = parent;
            Asset<Texture2D> texture = ModContent.Request<Texture2D>("jarkos/UI/ForgeIcon", AssetRequestMode.ImmediateLoad);
            Width.Set(texture.Width(), 0);
            Height.Set(texture.Height(), 0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (clickTimer > 0f)
            {
                clickTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (clickTimer < 0f) clickTimer = 0f;
            }

            Asset<Texture2D> texture = ModContent.Request<Texture2D>("jarkos/UI/ForgeIcon", AssetRequestMode.ImmediateLoad);
            if (Width.Pixels != texture.Width() || Height.Pixels != texture.Height())
            {
                Width.Set(texture.Width(), 0);
                Height.Set(texture.Height(), 0);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Texture2D mainTex = ModContent.Request<Texture2D>("jarkos/UI/ForgeIcon", AssetRequestMode.ImmediateLoad).Value;
            Texture2D lineTex = ModContent.Request<Texture2D>("jarkos/UI/ForgeIconLine", AssetRequestMode.ImmediateLoad).Value;

            Vector2 pos = GetDimensions().Position();

            spriteBatch.Draw(mainTex, pos, Color.White);

            Rectangle btnRect = GetDimensions().ToRectangle();
            bool hovered = btnRect.Contains(Main.MouseScreen.ToPoint());

            if (hovered)
            {
                Main.LocalPlayer.mouseInterface = true;

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    clickTimer = 0.5f;
                    ParentUIState.PerformReforge();
                }
            }

            if (hovered || clickTimer > 0f)
            {
                float t = clickTimer / 0.5f;
                Color lineTint = Color.Lerp(Color.Yellow, Color.White, t);
                spriteBatch.Draw(lineTex, pos, lineTint);
            }
        }
    }

    public class AttributeToggleInfo
    {
        public int Id;
        public string Label;
        public Func<bool> Getter;
        public Action<bool> Setter;
        public UIPanel Panel;
    }

    public class GoblinReforgeUIState : UIState
    {
        public UIPanel MainPanel;
        public Item ReforgeItem = new Item();

        public bool SelectDamage = true;
        public bool SelectCrit = false;
        public bool SelectMoveSpeed = false;
        public bool SelectWingTime = false;
        public bool SelectAttackSpeed = false;
        public bool SelectMeleeSize = false;
        public bool SelectManaCost = false;

        public bool SelectMaxMana = false;
        public bool SelectDebuffReduction = false;
        public bool SelectDefense = false;
        public bool SelectAccDamage = false;
        public bool SelectAccCrit = false;
        public bool SelectCritDamage = false;
        public bool SelectLuck = false;
        public bool SelectMinionSlot = false;

        public bool SelectWhipRange = false;
        public bool SelectArmorPenetration = false;
        public bool SelectShootSpeed = false;

        private UIReforgeButtonElement reforgeBtn;
        private UIText costText;

        private List<AttributeToggleInfo> attributeToggles = new List<AttributeToggleInfo>();

        public override void OnInitialize()
        {
            ReforgeItem.SetDefaults(ItemID.None);

            MainPanel = new UIPanel();
            MainPanel.SetPadding(10);
            MainPanel.Left.Set(95f, 0f);
            MainPanel.Top.Set(320f, 0f);
            MainPanel.Width.Set(430f, 0f);
            MainPanel.Height.Set(220f, 0f);
            MainPanel.BackgroundColor = new Color(20, 30, 50, 235);
            MainPanel.BorderColor = new Color(60, 130, 220);

            UIReforgeSlotElement slotElement = new UIReforgeSlotElement(this);
            slotElement.Left.Set(10, 0);
            slotElement.Top.Set(8, 0);
            MainPanel.Append(slotElement);

            reforgeBtn = new UIReforgeButtonElement(this);
            reforgeBtn.Left.Set(82f, 0);
            reforgeBtn.Top.Set(10f, 0);
            MainPanel.Append(reforgeBtn);

            costText = new UIText("", 0.75f);
            costText.Left.Set(140f, 0);
            costText.Top.Set(22f, 0);
            costText.TextColor = new Color(255, 230, 140);
            MainPanel.Append(costText);

            LocalizedText Selecting1 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.DamageSelecting", () => "Damage 2-12%");
            LocalizedText Selecting2 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.CritSelecting", () => "Critical Strike Chance");
            LocalizedText Selecting3 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.MoveSpeedSelecting", () => "Movement Speed");
            LocalizedText Selecting4 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.FlightSelecting", () => "Flight Duration");
            LocalizedText Selecting5 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.AttackSpeedSelecting", () => "Attack Speed 3-15%");
            LocalizedText Selecting6 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.WeaponScaleSelecting", () => "Melee Weapon Size");
            LocalizedText Selecting7 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.DamageSelecting", () => "Mana Cost 5-25%");

            AddAttributeToggle(1, Selecting1?.Value, () => SelectDamage, v => SelectDamage = v);
            AddAttributeToggle(2, Selecting2?.Value, () => SelectCrit, v => SelectCrit = v);
            AddAttributeToggle(3, Selecting3?.Value, () => SelectMoveSpeed, v => SelectMoveSpeed = v);
            AddAttributeToggle(5, Selecting4?.Value, () => SelectWingTime, v => SelectWingTime = v);
            AddAttributeToggle(6, Selecting5?.Value, () => SelectAttackSpeed, v => SelectAttackSpeed = v);
            AddAttributeToggle(7, Selecting6?.Value, () => SelectMeleeSize, v => SelectMeleeSize = v);
            AddAttributeToggle(8, Selecting7?.Value, () => SelectManaCost, v => SelectManaCost = v);


            LocalizedText Selecting8 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.MaxManaSelecting", () => "Max Mana");
            LocalizedText Selecting9 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.DebuffRedSelecting", () => "Debuff Duration Reduction");
            LocalizedText Selecting10 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.DefenseSelecting", () => "Defense");
            LocalizedText Selecting11 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.ExtraDmgSelecting", () => "Extra Damage");
            LocalizedText Selecting12 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.ExtraCritChanceSelecting", () => "Extra Critical Chance");
            LocalizedText Selecting13 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.CritDmgSelecting", () => "Critical Strike Damage");
            LocalizedText Selecting14 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.LuckSelecting", () => "Luck 0.005-0.05%");
            LocalizedText Selecting15 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.WhipRangeSelecting", () => "Whip Range");
            LocalizedText Selecting16 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.MinionSlotSelecting", () => "Minion Slot");
            LocalizedText Selecting17 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.ArmorPenSelecting", () => "Armor Penetration 1-3");
            LocalizedText Selecting18 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.ShotSpeedSelecting", () => "Shot Speed 5-15%");

            AddAttributeToggle(9, Selecting8?.Value, () => SelectMaxMana, v => SelectMaxMana = v);
            AddAttributeToggle(12, Selecting9?.Value, () => SelectDebuffReduction, v => SelectDebuffReduction = v);
            AddAttributeToggle(13, Selecting10?.Value, () => SelectDefense, v => SelectDefense = v);
            AddAttributeToggle(14, Selecting11?.Value, () => SelectAccDamage, v => SelectAccDamage = v);
            AddAttributeToggle(15, Selecting12?.Value, () => SelectAccCrit, v => SelectAccCrit = v);
            AddAttributeToggle(16, Selecting13?.Value, () => SelectCritDamage, v => SelectCritDamage = v);
            AddAttributeToggle(17, Selecting14?.Value, () => SelectLuck, v => SelectLuck = v);
            AddAttributeToggle(18, Selecting15?.Value, () => SelectWhipRange, v => SelectWhipRange = v);
            AddAttributeToggle(19, Selecting16?.Value, () => SelectMinionSlot, v => SelectMinionSlot = v);
            AddAttributeToggle(20, Selecting17?.Value, () => SelectArmorPenetration, v => SelectArmorPenetration = v);
            AddAttributeToggle(21, Selecting18?.Value, () => SelectShootSpeed, v => SelectShootSpeed = v);

            Append(MainPanel);
        }

        private void AddAttributeToggle(int id, string label, Func<bool> getter, Action<bool> setter)
        {
            UIPanel panel = new UIPanel();
            panel.Left.Set(10, 0);
            panel.Top.Set(0, 0);
            panel.Width.Set(400, 0);
            panel.Height.Set(28, 0);
            panel.SetPadding(2);

            UIText text = new UIText(label, 0.75f);
            text.Left.Set(10, 0);
            text.VAlign = 0.5f;
            panel.Append(text);

            panel.OnLeftClick += (evt, listeningElement) =>
            {
                bool currentlySelected = getter();
                if (!currentlySelected)
                {
                    int maxAllowed = (ReforgeItem != null && ReforgeItem.accessory) ? 2 : 4;
                    if (SelectedCount >= maxAllowed)
                    {
                        LocalizedText msg1 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.MsgWarningOne", () => "You can select a maximum of 2 attributes for accessories");
                        LocalizedText msg2 = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.MsgWarningTwo", () => "You can select a maximum of 4 attributes for weapons");

                        string msg = (ReforgeItem != null && ReforgeItem.accessory) 
                        ? msg1.Value
                        : msg2.Value;

                        Main.NewText(msg, Color.Orange);
                        SoundEngine.PlaySound(SoundID.MenuClose);
                        return;
                    }
                    setter(true);
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else
                {
                    setter(false);
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            };

            panel.OnUpdate += (listeningElement) =>
            {
                text.SetText(label);
                bool selected = getter();
                
                panel.BackgroundColor = selected ? new Color(45, 110, 175, 210) : new Color(25, 35, 55, 190);
                panel.BorderColor = selected ? new Color(120, 200, 255) : new Color(55, 65, 85);
            };

            MainPanel.Append(panel);

            attributeToggles.Add(new AttributeToggleInfo
            {
                Id = id,
                Label = label,
                Getter = getter,
                Setter = setter,
                Panel = panel
            });
        }

        public bool IsAttributeAvailable(int attrId, Item item)
        {
            if (item == null || item.IsAir) return false;
            if (item.ammo > 0 || item.maxStack > 1 || item.consumable) return false;

            switch (attrId)
            {
                case 1:
                case 2:
                case 6:
                case 20:
                    return item.damage > 0 && !item.accessory;

                case 3:
                case 5:
                    return item.accessory;

                case 7:
                    return item.damage > 0 && !item.accessory && (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed);

                case 8:
                    return item.mana > 0;

                case 9:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 19:
                    return item.accessory;

                case 18:
                    return item.damage > 0 && !item.accessory && item.DamageType == DamageClass.SummonMeleeSpeed;

                case 21:
                    return item.damage > 0 && !item.accessory && (item.DamageType == DamageClass.Ranged || item.DamageType == DamageClass.Magic || item.DamageType.CountsAsClass(DamageClass.Ranged) || item.DamageType.CountsAsClass(DamageClass.Magic));

                default:
                    return false;
            }
        }

        private int SelectedCount
        {
            get
            {
                int count = 0;
                foreach (var info in attributeToggles)
                {
                    if (IsAttributeAvailable(info.Id, ReforgeItem) && info.Getter())
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public (float min, float max) CurrentRange
        {
            get
            {
                int count = SelectedCount;
                if (count <= 1) return (0.02f, 0.10f);
                if (count == 2) return (0.02f, 0.06f);
                if (count == 3) return (0.01f, 0.02f);
                return (0.005f, 0.01f);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (MainPanel != null)
            {
                Rectangle panelRect = MainPanel.GetDimensions().ToRectangle();
                if (panelRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                }
            }

            float currentTop = 65f;
            foreach (var info in attributeToggles)
            {
                bool available = IsAttributeAvailable(info.Id, ReforgeItem);
                if (available)
                {
                    info.Panel.Left.Set(10, 0);
                    info.Panel.Top.Set(currentTop, 0);
                    currentTop += 32f;
                }
                else
                {
                    if (info.Getter())
                    {
                        info.Setter(false);
                    }
                    info.Panel.Left.Set(-9999, 0);
                }
            }

            MainPanel.Height.Set(currentTop + 15f, 0);

            if (ReforgeItem != null && !ReforgeItem.IsAir)
            {
                if (ReforgeItem.ammo > 0 || ReforgeItem.maxStack > 1 || ReforgeItem.consumable || (!ReforgeItem.accessory && ReforgeItem.damage <= 0))
                {
                    LocalizedText msg = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.NotSuitableReforge", () => "Item is not suitable for reforge");

                    costText.SetText("Item is not suitable for reforge");
                    return;
                }

                int cost = ReforgeItem.value / 3;
                if (cost < 100) cost = 100;

                int gold = cost / 10000;
                int silver = cost % 10000 / 100;
                int copper = cost % 100;

                LocalizedText cost_msg = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.Cost", () => "Cost:");

                string costStr = cost_msg.Value;

                if (gold > 0) 
                    costStr += $"{gold}g ";

                if (silver > 0) 
                    costStr += $"{silver}s ";

                costStr += $"{copper}c";
                costText.SetText(costStr);
            }
            else
            {
                LocalizedText msg = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.PlaceItemInSlot", () => "Place an item into slot");
                costText.SetText(msg.Value);
            }
        }

        public void PerformReforge()
        {
            if (ReforgeItem == null || ReforgeItem.IsAir)
            {
                LocalizedText msg = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.PlaceItemInSlotFirst", () => "Place an item into the slot first");

                Main.NewText(msg.Value, Color.Orange);
                return;
            }

            if (ReforgeItem.ammo > 0 || ReforgeItem.maxStack > 1 || ReforgeItem.consumable || (!ReforgeItem.accessory && ReforgeItem.damage <= 0))
            {
                LocalizedText msg = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.ItemNotSuitableForReforge", () => "Item is not suitable for reforge!");

                Main.NewText(msg.Value, Color.Orange);
                return;
            }

            if (SelectedCount == 0)
            {
                LocalizedText msg = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.AtLeastOneAttribute", () => "Select at least one attribute!");

                Main.NewText(msg.Value, Color.Orange);
                return;
            }

            int cost = ReforgeItem.value / 3;

            if (cost < 100) cost = 100;

            Player player = Main.LocalPlayer;
            if (!player.BuyItem(cost))
            {
                LocalizedText msg = Language.GetOrRegister("Mods.jarkos.GoblinRegorgeUI.NotEnoughMoney", () => "Not enough coins for reforge!");

                Main.NewText(msg.Value, Color.Red);
                return;
            }

            var (min, max) = CurrentRange;
            var modItem = ReforgeItem.GetGlobalItem<GoblinReforgeGlobalItem>();

            modItem.CustomDamageBonus = 0f;
            modItem.CustomCritBonus = 0f;
            modItem.CustomMoveSpeedBonus = 0f;
            modItem.CustomWingTimeBonus = 0f;
            modItem.CustomAttackSpeedBonus = 0f;
            modItem.CustomMeleeSizeBonus = 0f;
            modItem.CustomManaCostBonus = 0f;
            modItem.CustomMaxManaBonus = 0f;
            modItem.CustomDebuffReductionBonus = 0f;
            modItem.CustomDefenseBonus = 0f;
            modItem.CustomAccDamageBonus = 0f;
            modItem.CustomAccCritBonus = 0f;
            modItem.CustomCritDamageBonus = 0f;
            modItem.CustomLuckBonus = 0f;
            modItem.CustomWhipRangeBonus = 0f;
            modItem.CustomMinionSlotBonus = 0f;
            modItem.CustomArmorPenetrationBonus = 0f;
            modItem.CustomShootSpeedBonus = 0f;

            Random rand = new Random();

            if (SelectDamage && IsAttributeAvailable(1, ReforgeItem))
            {
                float rolledPercent = (float)(0.02f + rand.NextDouble() * (0.12f - 0.02f));
                modItem.CustomDamageBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectCrit && IsAttributeAvailable(2, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomCritBonus = (float)Math.Round(rolledPercent * 100f, 1);
            }
            if (SelectMoveSpeed && IsAttributeAvailable(3, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomMoveSpeedBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectWingTime && IsAttributeAvailable(5, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomWingTimeBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectAttackSpeed && IsAttributeAvailable(6, ReforgeItem))
            {
                float rolledPercent = (float)(0.03f + rand.NextDouble() * (0.15f - 0.03f));
                modItem.CustomAttackSpeedBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectMeleeSize && IsAttributeAvailable(7, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomMeleeSizeBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectManaCost && IsAttributeAvailable(8, ReforgeItem))
            {
                float rolledPercent = (float)(0.05f + rand.NextDouble() * (0.25f - 0.05f));
                modItem.CustomManaCostBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectMaxMana && IsAttributeAvailable(9, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomMaxManaBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectDebuffReduction && IsAttributeAvailable(12, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomDebuffReductionBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectDefense && IsAttributeAvailable(13, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomDefenseBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectAccDamage && IsAttributeAvailable(14, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomAccDamageBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectAccCrit && IsAttributeAvailable(15, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomAccCritBonus = (float)Math.Round(rolledPercent * 100f, 1);
            }
            if (SelectCritDamage && IsAttributeAvailable(16, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomCritDamageBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectLuck && IsAttributeAvailable(17, ReforgeItem))
            {
                float rolledLuck = (float)(0.00005f + rand.NextDouble() * (0.0005f - 0.00005f));
                modItem.CustomLuckBonus = (float)Math.Round(rolledLuck, 6);
            }
            if (SelectWhipRange && IsAttributeAvailable(18, ReforgeItem))
            {
                float rolledPercent = (float)(min + rand.NextDouble() * (max - min));
                modItem.CustomWhipRangeBonus = (float)Math.Round(rolledPercent, 3);
            }
            if (SelectMinionSlot && IsAttributeAvailable(19, ReforgeItem))
            {
                modItem.CustomMinionSlotBonus = 0.2f;
            }
            if (SelectArmorPenetration && IsAttributeAvailable(20, ReforgeItem))
            {
                modItem.CustomArmorPenetrationBonus = rand.Next(1, 4);
            }
            if (SelectShootSpeed && IsAttributeAvailable(21, ReforgeItem))
            {
                float rolledPercent = (float)(0.05f + rand.NextDouble() * (0.15f - 0.05f));
                modItem.CustomShootSpeedBonus = (float)Math.Round(rolledPercent, 3);
            }

            SoundEngine.PlaySound(SoundID.Item37);

            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(player.position, player.width, player.height, DustID.Copper, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            }
        }
    }
}
