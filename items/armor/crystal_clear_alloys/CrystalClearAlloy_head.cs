using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.Audio;
using jarkos.items.materials;
using jarkos.items.usetiles;
using Microsoft.Xna.Framework;
using System;

namespace jarkos.items.armor.crystal_clear_alloys
{
    [AutoloadEquip(EquipType.Head)]
    public class CrystalClearAlloy_head : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 24;
            Item.defense = 28;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 10, 0, 0);
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<CrystalClearAlloy_breast>() && legs.type == ModContent.ItemType<CrystalClearAlloy_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.GetModPlayer<CrystalClearAlloyPlayer>();
            modPlayer.hasCrystalClearSet = true;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Ranged) += 0.20f;
            player.GetDamage(DamageClass.Magic) += 0.20f;
            player.GetCritChance(DamageClass.Ranged) += 10;
            player.GetCritChance(DamageClass.Magic) += 10;
            player.manaCost -= 0.15f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<CrystalClearAlloy>(), 1)
                .AddIngredient(ModContent.ItemType<LunarCrystal>(), 8)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class CrystalClearAlloyPlayer : ModPlayer
    {
        public bool hasCrystalClearSet = false;
        public int focusActiveTimer = 0;
        public int focusCooldownTimer = 0;
        public override void ResetEffects()
        {
            hasCrystalClearSet = false;
        }
        public override void PostUpdateEquips()
        {
            if (focusActiveTimer > 0)
            {
                focusActiveTimer--;
                Player.manaCost = 0f;
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.PinkCrystalShard, 0f, -1.5f, 100, default, 1.1f);
                    d.noGravity = true;
                    d.velocity.X *= 0.3f;
                }
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.BlueCrystalShard, 0f, -1.5f, 100, default, 1.1f);
                    d.noGravity = true;
                    d.velocity.X *= 0.3f;
                }
            }
            if (focusCooldownTimer > 0)
            {
                focusCooldownTimer--;
            }
            if (hasCrystalClearSet && CrystalClearAlloysSystem.FocusKeybind != null && CrystalClearAlloysSystem.FocusKeybind.JustPressed)
            {
                if (focusCooldownTimer <= 0)
                {
                    focusActiveTimer = 600;
                    focusCooldownTimer = 4800;
                    SoundEngine.PlaySound(SoundID.Item29, Player.Center);

                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(5f, 5f);
                        Dust d = Dust.NewDustDirect(Player.Center, 0, 0, DustID.PinkCrystalShard, speed.X, speed.Y, 100, default, 1.3f);
                        d.noGravity = true;
                        Vector2 speed2 = Main.rand.NextVector2Circular(5f, 5f);
                        Dust d2 = Dust.NewDustDirect(Player.Center, 0, 0, DustID.BlueCrystalShard, speed2.X, speed2.Y, 100, default, 1.3f);
                        d2.noGravity = true;
                    }
                }
            }
        }
        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            if (focusActiveTimer > 0)
            {
                return false;
            }
            return base.CanConsumeAmmo(weapon, ammo);
        }
    }
    public class CrystalClearAlloysSystem : ModSystem
    {
        public static ModKeybind FocusKeybind { get; private set; }
        public override void Load()
        {
            FocusKeybind = KeybindLoader.RegisterKeybind(Mod, "Crystal Clear Focus", "H");
        }
        public override void Unload()
        {
            FocusKeybind = null;
        }
    }
}
