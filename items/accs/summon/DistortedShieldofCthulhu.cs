using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
    public class DistortedShieldDebuff : ModBuff
    {
        public override string Texture => "jarkos/items/accs/summon/DistortedShieldofCthulhu";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<DistortedShieldGlobalNPC>().distortedDebuff = true;
        }
    }

    public class DistortedShieldGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool distortedDebuff;

        public override void ResetEffects(NPC npc)
        {
            distortedDebuff = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (distortedDebuff)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= 50;

                if (damage < 5)
                    damage = 5;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (distortedDebuff && Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Corruption, 0f, -1f, 100, default, 1.2f);
                d.noGravity = true;
            }
        }
    }

    public class DistortedShieldPlayer : ModPlayer
    {
        public bool hasDistortedShield;

        public override void ResetEffects()
        {
            hasDistortedShield = false;
        }

        public override void PostUpdateEquips()
        {
            if (!hasDistortedShield)
                return;

            bool equippedLeaf = false;
            bool equippedBranch = false;

            for (int i = 3; i < 10 + Player.extraAccessorySlots; i++)
            {
                Item armor = Player.armor[i];
                if (armor != null && !armor.IsAir)
                {
                    if (armor.type == ModContent.ItemType<LeafShield>()) equippedLeaf = true;
                    else if (armor.type == ModContent.ItemType<Branch>()) equippedBranch = true;
                }
            }

            if (equippedLeaf)
            {
                Player.GetModPlayer<LeafShieldPlayer>().hasLeafShield = false;
            }

            if (equippedBranch)
            {
                Player.GetDamage(DamageClass.Summon) -= 0.05f;
                Player.maxMinions -= 1;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasDistortedShield)
            {
                target.AddBuff(ModContent.BuffType<DistortedShieldDebuff>(), 300);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasDistortedShield)
            {
                target.AddBuff(ModContent.BuffType<DistortedShieldDebuff>(), 300);
            }
        }
    }

    public class DistortedShieldofCthulhu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DistortedShieldPlayer>().hasDistortedShield = true;
            player.GetDamage(DamageClass.Summon) += 0.08f;
            player.maxMinions += 2;
        }

        public override void OnCreated(ItemCreationContext context)
        {
            if (context is RecipeItemCreationContext)
            {
                SoundEngine.PlaySound(SoundID.Roar);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<Branch>(), 2)
                .AddIngredient(ModContent.ItemType<LeafShield>(), 1)
                .AddIngredient(ItemID.EoCShield, 1)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
