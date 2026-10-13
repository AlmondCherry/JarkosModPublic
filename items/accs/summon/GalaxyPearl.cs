using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;

namespace jarkos.items.accs.summon
{
    public class GalaxyPearlPlayer : ModPlayer
    {
        public bool hasGalaxyPearl;
        public int hitStacks;
        public int buffTimer;

        public override void ResetEffects()
        {
            hasGalaxyPearl = false;
        }

        public override void PostUpdateEquips()
        {
            if (hasGalaxyPearl)
            {
                if (buffTimer > 0)
                {
                    buffTimer--;
                    Player.GetDamage(DamageClass.Summon) += hitStacks * 0.01f;

                    if (buffTimer <= 0)
                    {
                        hitStacks = 0;
                    }
                }
            }
            else
            {
                buffTimer = 0;
                hitStacks = 0;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasGalaxyPearl && target.active && !target.friendly)
            {
                AddHitStack();
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasGalaxyPearl && target.active && !target.friendly)
            {
                AddHitStack();
            }
        }

        private void AddHitStack()
        {
            if (hitStacks < 20)
            {
                hitStacks++;
            }
            buffTimer = 300;
            Player.AddBuff(ModContent.BuffType<buffs.buff.SummonPowerBoostBuff>(), 300);
        }
    }

    public class GalaxyPearl : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<GalaxyPearlPlayer>().hasGalaxyPearl = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<FragmentOfNature>(), 70)
                .AddIngredient(ItemID.SummoningPotion, 30)
                .AddIngredient(ModContent.ItemType<Branch>(), 1)
                .AddIngredient(ItemID.RichMahogany, 65)
                .AddRecipeGroup(nameof(ItemID.CrimtaneBar), 24)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
