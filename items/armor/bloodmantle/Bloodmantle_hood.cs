using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System;
using jarkos.projectiles.summon;

namespace jarkos.items.armor.bloodmantle
{
    [AutoloadEquip(EquipType.Head)]
    public class Bloodmantle_hood : ModItem
    {
        public static LocalizedText SetBonusText { get; private set; }
        public override void SetStaticDefaults()
        {
            SetBonusText = this.GetLocalization(nameof(SetBonusText));
        }
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 6;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<Bloodmantle_breastplace>() && legs.type == ModContent.ItemType<Bloodmantle_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.GetModPlayer<BloodMantlePlayer>();
            modPlayer.hasBloodMantleSet = true;
            player.setBonus = SetBonusText.Value;
        }
        public override void UpdateEquip(Player player)
        {
            player.maxMinions += 1;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrimtaneBar, 8);
            recipe.AddIngredient(ItemID.TissueSample, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class BloodMantlePlayer : ModPlayer
    {
        public bool hasBloodMantleSet = false;
        public int BloodRageStacks = 0;
        public bool isBloodRageActive = false;
        public int BloodRageTimeLeft = 0;
        public override void ResetEffects()
        {
            hasBloodMantleSet = false;
        }
        public override void PostUpdateEquips()
        {
            if (!hasBloodMantleSet) return;
            Player.maxMinions += 1;
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            if (!hasBloodMantleSet) return;
            if (Main.myPlayer == Player.whoAmI)
            {
                int spikeType = ModContent.ProjectileType<BloodMantleSpike>();
                int baseDamage = 25;
                float knockback = 2f;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    bool isMinion = proj.active && proj.owner == Player.whoAmI && (proj.minion || proj.minionSlots > 0f);

                    if (isMinion)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                            Vector2 launchVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5.5f;
                            int pIndex = Projectile.NewProjectile(null, proj.Center, launchVelocity, spikeType, baseDamage, knockback, Player.whoAmI);
                            
                            if (pIndex != Main.maxProjectiles)
                            {
                                Main.projectile[pIndex].ai[1] = (k == 0) ? 1f : -1f;
                            }
                        }
                    }
                }
            }
        }
    }
}
