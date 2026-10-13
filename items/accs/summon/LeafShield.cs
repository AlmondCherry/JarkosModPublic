using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
    public class LeafShieldDebuff : ModBuff
    {
        public override string Texture => "jarkos/items/accs/summon/LeafShield";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<LeafShieldGlobalNPC>().leafShieldDebuff = true;
        }
    }

    public class LeafShieldGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool leafShieldDebuff;

        public override void ResetEffects(NPC npc)
        {
            leafShieldDebuff = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (leafShieldDebuff)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= 40;

                if (damage < 4)
                    damage = 4;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (leafShieldDebuff && Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.JungleSpore, 0f, -1f, 100, default, 1.2f);
                d.noGravity = true;
            }
        }
    }

    public class LeafShieldPlayer : ModPlayer
    {
        public bool hasLeafShield;

        public override void ResetEffects()
        {
            hasLeafShield = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasLeafShield)
            {
                target.AddBuff(ModContent.BuffType<LeafShieldDebuff>(), 300);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasLeafShield)
            {
                target.AddBuff(ModContent.BuffType<LeafShieldDebuff>(), 300);
            }
        }
    }

    public class LeafShield : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<LeafShieldPlayer>().hasLeafShield = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddRecipeGroup(RecipeGroupID.Wood, 12)
                .AddIngredient(ItemID.DirtBlock, 12)
                .AddIngredient(ItemID.Gel, 48)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
