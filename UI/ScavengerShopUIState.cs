using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using jarkos.items.materials;
using System;
using jarkos.items.usable;
using Jarkos.items.materials;
using jarkos.items.accs.bloodmage;
using jarkos.npc.CradleMobs;
using ReLogic.Content;
using Terraria.Localization;
using jarkos.systems;

namespace jarkos.ui
{
    public class ScavengerShopUIState : UIState
    {
        public UIPanel mainContainer;
        private UIList itemList;
        private UIScrollbar scrollbar;
        private UIPanel centerPanel;
        private CenteredUIImage selectedItemImage;
        private UIText selectedItemName;
        private UIText selectedDescText;
        private UIPanel buyButton;
        private UIText buyText;
        private UIText priceText;
        private UIPanel rightPanel;
        private UIText quantityText;
        private ScavengerShopItem selectedItem;
        private int quantity = 1;
        private List<ScavengerShopItem> availableItems;
        private List<FlyingPollen> flyingPollens = new List<FlyingPollen>();
        private List<UIGore> flyingGores = new List<UIGore>();
        private List<UIPurchaseParticle> customParticles = new List<UIPurchaseParticle>();
        private bool isAnimatingPurchase = false;
        private int animationTicks = 0;
        private int openAnimTimer = 0;
        private const int Phase1Duration = 25;
        private const int Phase2Duration = 25;
        public CharacterPanel charPanel;
        public enum ScavengerEmotion { Neutral, Angry, Funny }
        private ScavengerEmotion currentEmotion = ScavengerEmotion.Neutral;
        private int funnyTimer = 0;
        private float emotionAnimTimer = 0f;
        private const float EmotionAnimDuration = 20f;
        public override void OnInitialize()
        {
            availableItems = new List<ScavengerShopItem> 
            {
                new ScavengerShopItem(ModContent.ItemType<ReturnScroll>(), 550), 
                new ScavengerShopItem(ModContent.ItemType<ReforgeScroll>(), 1250),
                new ScavengerShopItem(ModContent.ItemType<RuneEnchantmentScroll>(), 4500),
                new ScavengerShopItem(ModContent.ItemType<Canned_food>(), 355),
                new ScavengerShopItem(ModContent.ItemType<BundleOfMedicines>(), 775),
                new ScavengerShopItem(ModContent.ItemType<Nasty_cocktail>(), 425), 
                new ScavengerShopItem(ModContent.ItemType<DeliciousBait>(), 2515),
                new ScavengerShopItem(ModContent.ItemType<EssenceJarItem>(), 3150),
                new ScavengerShopItem(ModContent.ItemType<TightGripHook>(), 12200),
                new ScavengerShopItem(ModContent.ItemType<BiomechanicalHeart>(), 15500)
            };
            if (Main.hardMode)
            {
                availableItems.Add(new ScavengerShopItem(ModContent.ItemType<jarkos.items.accs.general.BiomaterialConverter>(), 6500));
            }
            if (NPC.downedMoonlord)
            {
                availableItems.Add(new ScavengerShopItem(ModContent.ItemType<Moonlord_bags>(), 210));
            }

            mainContainer = new UIPanel();
            mainContainer.Width.Set(416f, 0f);
            mainContainer.Height.Set(260f, 0f);
            mainContainer.HAlign = 0.5f;
            mainContainer.VAlign = 1.3f;
            mainContainer.BackgroundColor = Color.Transparent;
            mainContainer.BorderColor = Color.Transparent;
            Append(mainContainer);

            var bgAsset = ModContent.Request<Texture2D>("jarkos/UI/ScavengerUIPanelBG");
            var borderAsset = ModContent.Request<Texture2D>("jarkos/UI/ScavengerUIPanel");
            var neutralAsset = ModContent.Request<Texture2D>("jarkos/UI/ScavengerNeutral");
            var angryAsset = ModContent.Request<Texture2D>("jarkos/UI/ScavengerAngry");
            var funnyAsset = ModContent.Request<Texture2D>("jarkos/UI/ScavengerFunny");

            charPanel = new CharacterPanel(bgAsset, borderAsset, neutralAsset, angryAsset, funnyAsset);
            charPanel.Width.Set(270f, 0f);
            charPanel.Height.Set(200f, 0f);
            charPanel.HAlign = 0.5f;
            charPanel.VAlign = 0.5f;
            charPanel.Opacity = 0f;
            Append(charPanel);

            UIPanel leftPanel = new UIPanel();
            leftPanel.Width.Set(78f, 0f);
            leftPanel.Height.Set(234f, 0f);
            leftPanel.Left.Set(6f, 0f);
            leftPanel.Top.Set(6f, 0f);
            leftPanel.BorderColor = Color.Orange;
            leftPanel.BackgroundColor = new Color(20, 20, 30, 200);
            mainContainer.Append(leftPanel);

            itemList = new UIList();
            itemList.Width.Set(52f, 0f);
            itemList.Height.Set(100f, 1f);
            leftPanel.Append(itemList);

            scrollbar = new UIScrollbar();
            scrollbar.SetView(100f, 1000f);
            scrollbar.Height.Set(0f, 1f);
            scrollbar.Left.Set(48f, 0f);
            leftPanel.Append(scrollbar);

            itemList.SetScrollbar(scrollbar);
            
            foreach (var item in availableItems)
            {
                UIPanel itemSlot = new UIPanel();
                itemSlot.Width.Set(39f, 0f);
                itemSlot.Height.Set(39f, 0f);
                itemSlot.BorderColor = Color.Yellow;
                itemSlot.BackgroundColor = new Color(40, 40, 50, 200);
                itemSlot.OnMouseOver += (evt, el) => ((UIPanel)el).BackgroundColor = new Color(60, 60, 70, 200);
                itemSlot.OnMouseOut += (evt, el) => ((UIPanel)el).BackgroundColor = new Color(40, 40, 50, 200);
                itemSlot.OnLeftClick += (evt, el) => SelectItem(item);
                Main.instance.LoadItem(item.ItemType);

                Texture2D tex = TextureAssets.Item[item.ItemType].Value;
                CenteredUIImage img = new CenteredUIImage(tex, 29f);

                img.HAlign = 0.5f;
                img.VAlign = 0.5f;

                itemSlot.Append(img);
                itemList.Add(itemSlot);
            }

            centerPanel = new UIPanel();
            centerPanel.Width.Set(182f, 0f);
            centerPanel.Height.Set(234f, 0f);
            centerPanel.Left.Set(91f, 0f);
            centerPanel.Top.Set(6f, 0f);
            centerPanel.BorderColor = Color.Yellow;
            centerPanel.BackgroundColor = new Color(20, 20, 30, 200);
            mainContainer.Append(centerPanel);

            UIPanel imagePanel = new UIPanel();
            imagePanel.Width.Set(52f, 0f);
            imagePanel.Height.Set(52f, 0f);
            imagePanel.HAlign = 0.5f;
            imagePanel.Top.Set(13f, 0f);
            imagePanel.BorderColor = Color.Yellow;
            centerPanel.Append(imagePanel);

            selectedItemImage = new CenteredUIImage(TextureAssets.MagicPixel.Value, 45f);
            selectedItemImage.HAlign = 0.5f;
            selectedItemImage.VAlign = 0.5f;
            imagePanel.Append(selectedItemImage);

            selectedItemName = new UIText(ScavengerShopLangSystem.SelectProductText.Value, 0.35f, true);
            selectedItemName.HAlign = 0.5f;
            selectedItemName.Top.Set(71f, 0f);
            centerPanel.Append(selectedItemName);

            UIPanel descPanel = new UIPanel();
            descPanel.Width.Set(156f, 0f);
            descPanel.Height.Set(65f, 0f);
            descPanel.HAlign = 0.5f;
            descPanel.Top.Set(97f, 0f);
            descPanel.BorderColor = Color.Purple;
            centerPanel.Append(descPanel);

            selectedDescText = new UIText("", 0.65f);
            selectedDescText.IsWrapped = true;
            selectedDescText.Width.Set(143f, 0f);
            selectedDescText.Height.Set(57f, 0f);
            selectedDescText.HAlign = 0.5f;
            selectedDescText.Top.Set(2f, 0f);
            descPanel.Append(selectedDescText);

            buyButton = new UIPanel();
            buyButton.Width.Set(104f, 0f);
            buyButton.Height.Set(32f, 0f);
            buyButton.HAlign = 0.5f;
            buyButton.Top.Set(182f, 0f);
            buyButton.BorderColor = Color.Blue;
            buyButton.BackgroundColor = new Color(40, 60, 150);
            buyButton.OnMouseOver += (evt, el) => ((UIPanel)el).BackgroundColor = new Color(60, 80, 180);
            buyButton.OnMouseOut += (evt, el) => ((UIPanel)el).BackgroundColor = new Color(40, 60, 150);
            buyButton.OnLeftClick += BuyClicked;
            centerPanel.Append(buyButton);

            buyText = new UIText(ScavengerShopLangSystem.BuyText.Value, 0.75f);
            buyText.HAlign = 0.1f;
            buyText.VAlign = 0.5f;
            buyButton.Append(buyText);

            priceText = new UIText("0", 0.75f);
            priceText.HAlign = 0.65f;
            priceText.VAlign = 0.5f;
            priceText.TextColor = Color.Yellow;
            buyButton.Append(priceText);

            Main.instance.LoadItem(ModContent.ItemType<WastefulnessPollen>());
            CenteredUIImage pollenIcon = new CenteredUIImage(TextureAssets.Item[ModContent.ItemType<WastefulnessPollen>()].Value, 15f);
            pollenIcon.HAlign = 0.95f;
            pollenIcon.VAlign = 0.5f;
            buyButton.Append(pollenIcon);

            rightPanel = new UIPanel();
            rightPanel.Width.Set(117f, 0f);
            rightPanel.Height.Set(117f, 0f);
            rightPanel.Left.Set(275f, 0f);
            rightPanel.Top.Set(6f, 0f);
            rightPanel.BorderColor = Color.Orange;
            rightPanel.BackgroundColor = new Color(20, 20, 30, 200);
            mainContainer.Append(rightPanel);

            UIText qtyLabel = new UIText(ScavengerShopLangSystem.QuantityText.Value, 0.35f, true);
            qtyLabel.HAlign = 0.4f;
            qtyLabel.Top.Set(13f, 0f);
            rightPanel.Append(qtyLabel);

            quantityText = new UIText("1", 1.0f);
            quantityText.HAlign = 0.4f;
            quantityText.Top.Set(45f, 0f);
            rightPanel.Append(quantityText);

            Color btnNormal = new Color(40, 40, 50, 200);
            Color btnHover = new Color(60, 60, 70, 200);

            UIPanel plusBtn = new UIPanel();
            plusBtn.BackgroundColor = btnNormal;
            plusBtn.OnMouseOver += (evt, el) => ((UIPanel)el).BackgroundColor = btnHover;
            plusBtn.OnMouseOut += (evt, el) => ((UIPanel)el).BackgroundColor = btnNormal;
            plusBtn.Width.Set(26f, 0f);
            plusBtn.Height.Set(26f, 0f);
            plusBtn.Left.Set(68f, 0f);
            plusBtn.Top.Set(39f, 0f);
            plusBtn.OnLeftClick += (evt, el) => ChangeQuantity(1);
            UIText plusText = new UIText("+", 1.0f);
            plusText.HAlign = 0.5f;
            plusText.VAlign = 0.5f;
            plusBtn.Append(plusText);

            rightPanel.Append(plusBtn);

            UIPanel minusBtn = new UIPanel();
            minusBtn.BackgroundColor = btnNormal;
            minusBtn.OnMouseOver += (evt, el) => ((UIPanel)el).BackgroundColor = btnHover;
            minusBtn.OnMouseOut += (evt, el) => ((UIPanel)el).BackgroundColor = btnNormal;
            minusBtn.Width.Set(26f, 0f);
            minusBtn.Height.Set(26f, 0f);
            minusBtn.Left.Set(8f, 0f);
            minusBtn.Top.Set(39f, 0f);
            minusBtn.OnLeftClick += (evt, el) => ChangeQuantity(-1);
            UIText minusText = new UIText("-", 1.0f);
            minusText.HAlign = 0.5f;
            minusText.VAlign = 0.5f;
            minusBtn.Append(minusText);

            rightPanel.Append(minusBtn);

            UIPanel plus10Btn = new UIPanel();
            plus10Btn.BackgroundColor = btnNormal;
            plus10Btn.OnMouseOver += (evt, el) => ((UIPanel)el).BackgroundColor = btnHover;
            plus10Btn.OnMouseOut += (evt, el) => ((UIPanel)el).BackgroundColor = btnNormal;
            plus10Btn.Width.Set(32f, 0f);
            plus10Btn.Height.Set(19f, 0f);
            plus10Btn.Left.Set(63f, 0f);
            plus10Btn.Top.Set(78f, 0f);
            plus10Btn.OnLeftClick += (evt, el) => ChangeQuantity(10);
            UIText plus10Text = new UIText("+10", 0.75f);
            plus10Text.HAlign = 0.5f;
            plus10Text.VAlign = 0.5f;
            plus10Btn.Append(plus10Text);
            
            rightPanel.Append(plus10Btn);

            UIPanel minus10Btn = new UIPanel();
            minus10Btn.BackgroundColor = btnNormal;
            minus10Btn.OnMouseOver += (evt, el) => ((UIPanel)el).BackgroundColor = btnHover;
            minus10Btn.OnMouseOut += (evt, el) => ((UIPanel)el).BackgroundColor = btnNormal;
            minus10Btn.Width.Set(32f, 0f);
            minus10Btn.Height.Set(19f, 0f);
            minus10Btn.Left.Set(8f, 0f);
            minus10Btn.Top.Set(78f, 0f);
            minus10Btn.OnLeftClick += (evt, el) => ChangeQuantity(-10);
            UIText minus10Text = new UIText("-10", 0.75f);
            minus10Text.HAlign = 0.5f;
            minus10Text.VAlign = 0.5f;

            minus10Btn.Append(minus10Text);

            rightPanel.Append(minus10Btn);

            SelectItem(availableItems[0]);
            SelectItem(availableItems[0]);
        }
        private void SelectItem(ScavengerShopItem item)
        {
            if (isAnimatingPurchase) return;
            selectedItem = item;
            quantity = 1;
            UpdateDisplay();
        }
        private void ChangeQuantity(int delta)
        {
            if (isAnimatingPurchase || selectedItem == null) return;
            quantity += delta;
            if (quantity < 1) quantity = 1;
            if (quantity > 999) quantity = 999;
            UpdateDisplay();
        }
        private int GetCurrentPrice(ScavengerShopItem item)
        {
            float multiplier = ScavengerSystem.IsAngryAtPlayer ? 1.7f : 1.0f;
            float configMult = Jarkos_config.Instance != null ? Jarkos_config.Instance.ScavengerPriceMultiplier : 1.0f;
            int price = (int)Math.Ceiling(item.PollenPrice * multiplier * configMult);
            return Math.Max(1, price);
        }
        private string GetItemDescription(int itemType)
        {
            Item dummy = new Item();
            dummy.SetDefaults(itemType);

            List<string> tooltipLines = new List<string>();
            var tooltip = Lang.GetTooltip(dummy.type);
            if (tooltip != null && tooltip.Lines > 0)
            {
                for (int i = 0; i < tooltip.Lines; i++)
                {
                    string line = tooltip.GetLine(i);
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        tooltipLines.Add(line);
                    }
                }
            }

            if (tooltipLines.Count > 0)
            {
                return string.Join("\n", tooltipLines);
            }

            if (dummy.ModItem != null)
            {
                string key = $"Mods.jarkos.Items.{dummy.ModItem.Name}.Tooltip";
                if (Language.Exists(key))
                {
                    string val = Language.GetTextValue(key);
                    if (!string.IsNullOrWhiteSpace(val) && val != key)
                    {
                        return val;
                    }
                }
            }

            return "";
        }

        private void UpdateDisplay()
        {
            if (selectedItem != null)
            {
                Main.instance.LoadItem(selectedItem.ItemType);
                Item dummy = new Item();
                dummy.SetDefaults(selectedItem.ItemType);
                int price = GetCurrentPrice(selectedItem);
                int totalCost = price * quantity;
                Texture2D tex2 = TextureAssets.Item[selectedItem.ItemType].Value;
                selectedItemImage.SetImage(tex2);
                selectedItemName.SetText(dummy.Name);
                selectedDescText.SetText(GetItemDescription(selectedItem.ItemType));
                quantityText.SetText(quantity.ToString());
                priceText.SetText(totalCost.ToString());
            }
        }
        private void BuyClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            if (isAnimatingPurchase || selectedItem == null) return;
            int price = GetCurrentPrice(selectedItem);
            int totalCost = price * quantity;
            int playerPollenCount = CountPollen(Main.LocalPlayer);
            if (playerPollenCount >= totalCost)
            {
                ConsumePollen(Main.LocalPlayer, totalCost);
                isAnimatingPurchase = true;
                animationTicks = 0;
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f });
                Vector2 inventoryStart = new Vector2(50f, 50f);
                int pollenSlot = -1;
                for (int i = 0; i < 50; i++)
                {
                    if (Main.LocalPlayer.inventory[i].type == ModContent.ItemType<WastefulnessPollen>())
                    {
                        pollenSlot = i;
                        break;
                    }
                }
                if (pollenSlot != -1)
                {
                    float contextX = 20f;
                    float contextY = 20f;
                    if (pollenSlot >= 10)
                    {
                        contextY += 42f + 56f * (pollenSlot / 10 - 1);
                        contextX += 56f * (pollenSlot % 10);
                    }
                    else
                    {
                        contextX += 56f * pollenSlot;
                    }
                    inventoryStart = new Vector2(contextX, contextY) * Main.UIScale;
                }
                int pollenCount = Math.Min(20, totalCost);
                for (int i = 0; i < pollenCount; i++)
                {
                    flyingPollens.Add(new FlyingPollen
                    {
                        Position = inventoryStart, Velocity = Main.rand.NextVector2Circular(8f, 8f) - new Vector2(0f, 5f), Timer = Main.rand.Next(15, 25)
                    });
                }
            }
            else
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                Main.NewText(ScavengerShopLangSystem.NotEnoughPollenText.Value, 255, 50, 50);
            }
        }
        private int CountPollen(Player player)
        {
            int count = 0;
            foreach (var item in player.inventory)
            {
                if (item.type == ModContent.ItemType<WastefulnessPollen>())
                {
                    count += item.stack;
                }
            }
            return count;
        }
        private void ConsumePollen(Player player, int amount)
        {
            int remaining = amount;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];
                if (item.type == ModContent.ItemType<WastefulnessPollen>())
                {
                    if (item.stack >= remaining)
                    {
                        item.stack -= remaining;
                        if (item.stack <= 0) item.TurnToAir();
                        break;
                    }
                    else
                    {
                        remaining -= item.stack;
                        item.TurnToAir();
                    }
                }
            }
        }
        private void FinishPurchase()
        {
            isAnimatingPurchase = false;
            flyingPollens.Clear();
            flyingGores.Clear();
            customParticles.Clear();
            funnyTimer = 180;
            if (ScavengerSystem.IsAngryAtPlayer)
            {
                ScavengerSystem.IsAngryAtPlayer = false;
                UpdateDisplay();
            }
            Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), selectedItem.ItemType, quantity);
            ScavengerUISystem.TrySpawnPurchaseEffect(buyButton.GetDimensions().Center());
        }
        private void ShatterPollen(Vector2 pos)
        {
            Vector2 targetPos = buyButton.GetDimensions().Center();
            for (int i = 1; i <= 4; i++)
            {
                flyingGores.Add(new UIGore
                {
                    Start = pos, Target = targetPos, Position = pos, Progress = 0f, Speed = Main.rand.NextFloat(0.015f, 0.025f), Amplitude = Main.rand.NextFloat(40f, 120f) * (Main.rand.NextBool() ? 1f : -1f), Phase = Main.rand.NextFloat(MathHelper.TwoPi), Texture = ModContent.Request<Texture2D>("jarkos/Gores/WastefulnessPollenGore" + i).Value, Trail = new List<Vector2>(), Rotation = Main.rand.NextFloat(MathHelper.TwoPi)
                });
            }
            for (int i = 0; i < 10; i++)
            {
                customParticles.Add(new UIPurchaseParticle
                {
                    Position = pos, Velocity = Main.rand.NextVector2Circular(6f, 6f), Color = Main.rand.NextBool() ? Color.Yellow : Color.White, Scale = Main.rand.NextFloat(0.5f, 1.2f), MaxTime = 25, TimeLeft = 25, Rotation = Main.rand.NextFloat(MathHelper.TwoPi)
                });
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (openAnimTimer < Phase1Duration + Phase2Duration)
            {
                openAnimTimer++;
                if (openAnimTimer <= Phase1Duration)
                {
                    float t = (float)openAnimTimer / Phase1Duration;
                    float ease = 1f - (float)Math.Pow(1f - t, 3);
                    mainContainer.VAlign = MathHelper.Lerp(1.3f, 0.5f, ease);
                    mainContainer.Left.Set(0f, 0f);
                    charPanel.Left.Set(900f, 0f);
                    charPanel.VAlign = 0.5f;
                    charPanel.Opacity = 0f;
                }
                else
                {
                    float t = (float)(openAnimTimer - Phase1Duration) / Phase2Duration;
                    float ease = 1f - (float)Math.Pow(1f - t, 3);
                    mainContainer.VAlign = 0.5f;
                    mainContainer.Left.Set(MathHelper.Lerp(0f, -100f, ease), 0f);
                    charPanel.Left.Set(MathHelper.Lerp(450f, 263f, ease), 0f);
                    charPanel.VAlign = 0.5f;
                    charPanel.Opacity = ease;
                }
                mainContainer.Recalculate();
                charPanel.Recalculate();
            }
            else if (openAnimTimer == Phase1Duration + Phase2Duration)
            {
                openAnimTimer++;
                mainContainer.VAlign = 0.5f;
                mainContainer.Left.Set(-100f, 0f);
                charPanel.Left.Set(263f, 0f);
                charPanel.VAlign = 0.5f;
                charPanel.Opacity = 1f;
                mainContainer.Recalculate();
                charPanel.Recalculate();
            }
            ScavengerEmotion desiredEmotion = ScavengerEmotion.Neutral;
            if (ScavengerSystem.IsAngryAtPlayer)
            {
                desiredEmotion = ScavengerEmotion.Angry;
            }
            else if (funnyTimer > 0)
            {
                desiredEmotion = ScavengerEmotion.Funny;
            }
            if (desiredEmotion != currentEmotion)
            {
                currentEmotion = desiredEmotion;
                emotionAnimTimer = EmotionAnimDuration;
            }
            if (funnyTimer > 0)
            {
                funnyTimer--;
            }
            if (emotionAnimTimer > 0)
            {
                emotionAnimTimer--;
            }
            charPanel.CurrentEmotion = currentEmotion;
            charPanel.EmotionAnimTimer = emotionAnimTimer;
            if (isAnimatingPurchase)
            {
                animationTicks++;
                bool goresFlying = false;
                for (int i = flyingPollens.Count - 1; i >= 0; i--)
                {
                    var p = flyingPollens[i];
                    p.Position += p.Velocity;
                    p.Velocity *= 0.85f;
                    p.Timer--;
                    if (p.Timer <= 0)
                    {
                        ShatterPollen(p.Position);
                        flyingPollens.RemoveAt(i);
                    }
                }
                for (int i = customParticles.Count - 1; i >= 0; i--)
                {
                    var p = customParticles[i];
                    p.Position += p.Velocity;
                    p.Velocity *= 0.9f;
                    p.TimeLeft--;

                    p.Rotation += 0.1f;
                    if (p.TimeLeft <= 0) customParticles.RemoveAt(i);
                }
                foreach (var g in flyingGores)
                {
                    if (g.Progress < 1f)
                    {
                        goresFlying = true;
                        g.Progress += g.Speed;
                        if (g.Progress > 1f) g.Progress = 1f;

                        Vector2 directLine = Vector2.Lerp(g.Start, g.Target, g.Progress);
                        Vector2 perp = new Vector2(-(g.Target - g.Start).Y, (g.Target - g.Start).X);
                        if (perp != Vector2.Zero) perp.Normalize();

                        float wave = (float)Math.Sin(g.Progress * Math.PI * 2f + g.Phase) * g.Amplitude;
                        float taper = (float)Math.Sin(g.Progress * Math.PI);
                        
                        g.Position = directLine + perp * wave * taper;
                        g.Rotation += 0.2f;
                        g.Trail.Add(g.Position);

                        if (g.Trail.Count > 15) g.Trail.RemoveAt(0);
                    }
                }
                if (flyingPollens.Count == 0 && !goresFlying && animationTicks > 20)
                {
                    FinishPurchase();
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (isAnimatingPurchase)
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                foreach (var p in customParticles)
                {
                    float fade = (float)p.TimeLeft / p.MaxTime;
                    spriteBatch.Draw(pixel, p.Position, new Rectangle(0, 0, 4, 4), p.Color * fade, p.Rotation, new Vector2(2f, 2f), p.Scale, SpriteEffects.None, 0f);
                }
                Main.instance.LoadItem(ModContent.ItemType<WastefulnessPollen>());
                Texture2D pollenTex = TextureAssets.Item[ModContent.ItemType<WastefulnessPollen>()].Value;
                foreach (var p in flyingPollens)
                {
                    spriteBatch.Draw(pollenTex, p.Position, null, Color.White, 0f, pollenTex.Size() / 2f, 1f, SpriteEffects.None, 0f);
                }
                foreach (var g in flyingGores)
                {
                    if (g.Progress < 1f)
                    {
                        for (int i = 0; i < g.Trail.Count; i++)
                        {
                            float trailFade = (float)i / g.Trail.Count;
                            spriteBatch.Draw(pixel, g.Trail[i], new Rectangle(0, 0, 4, 4), Color.Yellow * trailFade * 0.8f, 0f, new Vector2(2f, 2f), 1.5f * trailFade, SpriteEffects.None, 0f);
                        }
                        spriteBatch.Draw(g.Texture, g.Position, null, Color.Yellow * 0.6f, g.Rotation, g.Texture.Size() / 2f, 1.4f, SpriteEffects.None, 0f);
                        spriteBatch.Draw(g.Texture, g.Position, null, Color.White, g.Rotation, g.Texture.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    }
                }
            }
        }
        private class FlyingPollen
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public int Timer;
        }
        private class UIGore
        {
            public Vector2 Start;
            public Vector2 Target;
            public Vector2 Position;
            public float Progress;
            public float Speed;
            public float Amplitude;
            public float Phase;
            public Texture2D Texture;
            public List<Vector2> Trail;
            public float Rotation;
        }
        private class UIPurchaseParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Color Color;
            public float Scale;
            public int TimeLeft;
            public int MaxTime;
            public float Rotation;
        }
        private class CenteredUIImage : UIElement
        {
            private Texture2D _texture;
            public float MaxSize { get; set; }
            public CenteredUIImage(Texture2D texture, float maxSize)
            {
                _texture = texture;
                MaxSize = maxSize;
                Width.Set(maxSize, 0f);
                Height.Set(maxSize, 0f);
            }
            public void SetImage(Texture2D texture)
            {
                _texture = texture;
            }
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (_texture == null) return;
                CalculatedStyle dimensions = GetDimensions();
                float scale = 1f;
                if (_texture.Width > MaxSize || _texture.Height > MaxSize)
                    scale = MaxSize / Math.Max(_texture.Width, _texture.Height);
                Vector2 position = dimensions.Position() + new Vector2(dimensions.Width, dimensions.Height) / 2f;
                Vector2 origin = new Vector2(_texture.Width, _texture.Height) / 2f;
                spriteBatch.Draw(_texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
        public class CharacterPanel : UIElement
        {
            private Asset<Texture2D> _bgAsset;
            private Asset<Texture2D> _borderAsset;
            private Asset<Texture2D> _neutralAsset;
            private Asset<Texture2D> _angryAsset;
            private Asset<Texture2D> _funnyAsset;
            public float Opacity { get; set; } = 0f;
            public ScavengerEmotion CurrentEmotion { get; set; } = ScavengerEmotion.Neutral;
            public float EmotionAnimTimer { get; set; } = 0f;
            public float EmotionAnimDuration { get; set; } = 20f;
            public CharacterPanel(Asset<Texture2D> bgAsset, Asset<Texture2D> borderAsset, Asset<Texture2D> neutralAsset, Asset<Texture2D> angryAsset, Asset<Texture2D> funnyAsset)
            {
                _bgAsset = bgAsset;
                _borderAsset = borderAsset;
                _neutralAsset = neutralAsset;
                _angryAsset = angryAsset;
                _funnyAsset = funnyAsset;
                Width.Set(270f, 0f);
                Height.Set(200f, 0f);
            }
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (Opacity <= 0f) return;
                CalculatedStyle dimensions = GetDimensions();
                Vector2 pos = dimensions.Position();
                if (_bgAsset != null && _bgAsset.IsLoaded && _bgAsset.Value != null)
                {
                    spriteBatch.Draw(_bgAsset.Value, new Rectangle((int)pos.X, (int)pos.Y, (int)dimensions.Width, (int)dimensions.Height), Color.Purple * 0.5f * Opacity);
                }
                Asset<Texture2D> activeEmotionAsset = null;
                switch (CurrentEmotion)
                {
                    case ScavengerEmotion.Angry:
                        activeEmotionAsset = _angryAsset;
                        break;
                    case ScavengerEmotion.Funny:
                        activeEmotionAsset = _funnyAsset;
                        break;
                    default:
                        activeEmotionAsset = _neutralAsset;
                        break;
                }
                if (activeEmotionAsset != null && activeEmotionAsset.IsLoaded && activeEmotionAsset.Value != null)
                {
                    float yOffset = 0f;
                    if (EmotionAnimTimer > 0f)
                    {
                        float t = 1f - (EmotionAnimTimer / EmotionAnimDuration);
                        float ease = 1f - (float)Math.Pow(1f - t, 3);
                        
                        yOffset = MathHelper.Lerp(30f, 0f, ease);
                    }
                    spriteBatch.Draw(activeEmotionAsset.Value, new Rectangle((int)pos.X, (int)(pos.Y + yOffset), (int)dimensions.Width, (int)dimensions.Height), Color.White * Opacity);
                }
                if (_borderAsset != null && _borderAsset.IsLoaded && _borderAsset.Value != null)
                {
                    spriteBatch.Draw(_borderAsset.Value, new Rectangle((int)pos.X, (int)pos.Y, (int)dimensions.Width, (int)dimensions.Height), Color.White * Opacity);
                }
            }
        }
    }
}
