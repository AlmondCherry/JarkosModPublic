using System;
using System.Collections.Generic;
using System.Linq;
using jarkos.buffs.buff;
using jarkos.buffs.debuff;
using jarkos.items.weapons.melee.other_melee;
using jarkos.items.weapons.melee.true_melee;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.projectiles.melee.true_melee;
using jarkos.projectiles.melee.unique_melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace jarkos.systems
{
    public interface IRMBCooldown
    {
        bool GetRMBCooldown(Player player, out int currentCooldown, out int maxCooldown);
    }

    public class RMBCooldownPlayer : ModPlayer
    {
        public readonly Dictionary<int, (int current, int max)> Cooldowns = new();

        public static void SetRMBCooldown(Player player, int itemType, int cooldownTicks, int maxCooldownTicks = 0)
        {
            if (player == null || cooldownTicks <= 0) return;

            var modPlayer = player.GetModPlayer<RMBCooldownPlayer>();
            int max = maxCooldownTicks > 0 ? maxCooldownTicks : cooldownTicks;

            modPlayer.Cooldowns[itemType] = (cooldownTicks, max);
        }

        public bool GetCooldown(int itemType, out int current, out int max)
        {
            if (Cooldowns.TryGetValue(itemType, out var cd) && cd.current > 0)
            {
                current = cd.current;
                max = cd.max;
                return true;
            }
            current = 0;
            max = 0;
            return false;
        }

        public override void PostUpdate()
        {
            if (Cooldowns.Count == 0) return;

            var keys = Cooldowns.Keys.ToList();
            foreach (var k in keys)
            {
                var (c, m) = Cooldowns[k];
                if (c > 1) Cooldowns[k] = (c - 1, m);
                else Cooldowns.Remove(k);
            }
        }

        public override void OnRespawn()
        {
            Cooldowns.Clear();
        }
    }

    public class RMBCooldownGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public override bool? UseItem(Item item, Player player)
        {
            if (player.altFunctionUse == 2 && player.whoAmI == Main.myPlayer)
            {
                if (item.type == ModContent.ItemType<Fiend_wings>())
                {
                    RMBCooldownPlayer.SetRMBCooldown(player, item.type, 160, 160);
                }
                else if (item.type == ModContent.ItemType<Mage_slayer>())
                {
                    RMBCooldownPlayer.SetRMBCooldown(player, item.type, 36, 36);
                }
            }
            return base.UseItem(item, player);
        }

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active || item == null || item.IsAir) return;

            int currentCd = 0;
            int maxCd = 1;
            bool active = false;

            if (item.ModItem is IRMBCooldown rmbItem)
            {
                active = rmbItem.GetRMBCooldown(player, out currentCd, out maxCd) && currentCd > 0;
            }

            if (!active)
            {
                var rmbPlayer = player.GetModPlayer<RMBCooldownPlayer>();
                active = rmbPlayer.GetCooldown(item.type, out currentCd, out maxCd) && currentCd > 0;
            }

            if (!active)
            {
                active = CheckCustomWeaponPlayerCooldowns(player, item, out currentCd, out maxCd);
            }

            if (!active || currentCd <= 0 || maxCd <= 0) return;

            float progress = MathHelper.Clamp((float)currentCd / maxCd, 0f, 1f);

            float itemWidth = frame.Width * scale;
            float itemHeight = frame.Height * scale;

            float shadowHeight = itemHeight * progress;
            float shadowTop = position.Y + itemHeight / 2f - shadowHeight;
            float shadowLeft = position.X - itemWidth / 2f;

            Rectangle shadowRect = new Rectangle((int)shadowLeft, (int)shadowTop, (int)itemWidth, (int)shadowHeight);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, shadowRect, Color.Black * 0.75f);

            int borderH = Math.Max(1, (int)(2 * scale));
            Rectangle borderRect = new Rectangle((int)shadowLeft, (int)shadowTop, (int)itemWidth, borderH);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, borderRect, Color.Cyan * 0.9f);

            if (currentCd > 10)
            {
                float secs = currentCd / 60f;
                string text = secs >= 10f ? $"{secs:F0}s" : $"{secs:F1}s";

                Vector2 textSize = FontAssets.ItemStack.Value.MeasureString(text) * 0.7f;
                Vector2 textPos = new Vector2(position.X - textSize.X / 2f, shadowTop - textSize.Y / 2f);

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, textPos, Color.White, 0f, Vector2.Zero, new Vector2(0.7f));
            }
        }

        private static bool CheckCustomWeaponPlayerCooldowns(Player player, Item item, out int current, out int max)
        {
            current = 0;
            max = 1;

            if (item.type == ModContent.ItemType<Thermal_scythe>())
            {
                var p = player.GetModPlayer<Jarkos_player>();
                if (p != null && p.thermalScytheCooldown > 0)
                {
                    current = p.thermalScytheCooldown;
                    max = 300;
                    return true;
                }
            }
            if (item.type == ModContent.ItemType<Spinetion>())
            {
                var p = player.GetModPlayer<SpinetionPlayer>();
                if (p != null && p.instakillCooldown > 0)
                {
                    current = p.instakillCooldown;
                    max = 1800;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<Pyromelt>())
            {
                var p = player.GetModPlayer<PyromeltPlayer>();
                if (p != null && p.throwCooldown > 0)
                {
                    current = p.throwCooldown;
                    max = 3600;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<ReversalBlade>())
            {
                var p = player.GetModPlayer<ReversalBladePlayer>();
                if (p != null && p.riftCooldown > 0)
                {
                    current = p.riftCooldown;
                    max = ReversalRiftGate.COOLDOWN;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<Eternal_reign>())
            {
                int buffType = ModContent.BuffType<Eternal_reign_cd>();
                int idx = player.FindBuffIndex(buffType);
                if (idx >= 0)
                {
                    current = player.buffTime[idx];
                    max = 3600;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<Sunfate>())
            {
                int buffType = ModContent.BuffType<SunfateLaserCooldown>();
                int idx = player.FindBuffIndex(buffType);
                if (idx >= 0)
                {
                    current = player.buffTime[idx];
                    max = 3600;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<BladeOfWar>())
            {
                var p = player.GetModPlayer<LegionBladePlayer>();
                if (p != null && p.UltimateCooldown > 0)
                {
                    current = p.UltimateCooldown;
                    max = 600;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<AqueousFlow>())
            {
                var p = player.GetModPlayer<AqueousPathPlayer>();
                if (p != null && p.Cooldown > 0)
                {
                    current = p.Cooldown;
                    max = 600;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<PurgatorySpear>())
            {
                var p = player.GetModPlayer<PurgatoryPlayer>();
                if (p != null && p.AbilityCooldown > 0)
                {
                    current = p.AbilityCooldown;
                    max = 1200;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<Slovo_berserkera>())
            {
                var p = player.GetModPlayer<MagnetarPlayer>();
                if (p != null && p.electricStormCooldown > 0)
                {
                    current = p.electricStormCooldown;
                    max = 1800;
                    return true;
                }
            }
            else if (item.type == ModContent.ItemType<Ethereal_rift>())
            {
                int domainType = ModContent.ProjectileType<Ethereal_rift_domain>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == domainType)
                    {
                        current = proj.timeLeft;
                        max = 1200;
                        return true;
                    }
                }
            }
            else if (item.type == ModContent.ItemType<CircularSaw>())
            {
                int sawType = ModContent.ProjectileType<Circular_saw_slash>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == sawType)
                    {
                        current = proj.timeLeft;
                        max = 180;
                        return true;
                    }
                }
            }
            else if (item.type == ModContent.ItemType<Dante_sword>())
            {
                int cType = ModContent.ProjectileType<Dante_sword_charge>();
                int tType = ModContent.ProjectileType<Dante_sword_thrown>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && (proj.type == cType || proj.type == tType))
                    {
                        current = proj.timeLeft;
                        max = 180;
                        return true;
                    }
                }
            }
            else if (item.type == ModContent.ItemType<StormAnchor>())
            {
                int cType = ModContent.ProjectileType<StormAnchorCharge>();
                int tType = ModContent.ProjectileType<StormAnchorSpin>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && (proj.type == cType || proj.type == tType))
                    {
                        current = proj.timeLeft;
                        max = 180;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
