using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
    public class NobleShieldDebuff : ModBuff
    {
        public override string Texture => "jarkos/items/accs/summon/NobleShieldoftheGodofNature";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<NobleShieldGlobalNPC>().nobleDebuff = true;
        }
    }

    public class NobleShieldGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool nobleDebuff;

        public override void ResetEffects(NPC npc)
        {
            nobleDebuff = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (nobleDebuff)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= 60;

                if (damage < 6)
                    damage = 6;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (nobleDebuff && Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.JungleSpore, 0f, -1f, 100, default, 1.3f);
                d.noGravity = true;
            }
        }
    }

    public class NobleShieldPlayer : ModPlayer
    {
        public bool hasNobleShield;

        public override void ResetEffects()
        {
            hasNobleShield = false;
        }

        public override void PostUpdateEquips()
        {
            if (!hasNobleShield)
                return;

            Player.noKnockback = true;

            if (Player.HasBuff(BuffID.Bewitched))
            {
                Player.maxMinions -= 1;
            }

            bool equippedDistorted = false;
            bool equippedLeaf = false;
            bool equippedBranch = false;

            for (int i = 3; i < 10 + Player.extraAccessorySlots; i++)
            {
                Item armor = Player.armor[i];
                if (armor != null && !armor.IsAir)
                {
                    if (armor.type == ModContent.ItemType<DistortedShieldofCthulhu>()) equippedDistorted = true;
                    else if (armor.type == ModContent.ItemType<LeafShield>()) equippedLeaf = true;
                    else if (armor.type == ModContent.ItemType<Branch>()) equippedBranch = true;
                }
            }

            if (equippedDistorted)
            {
                Player.GetModPlayer<DistortedShieldPlayer>().hasDistortedShield = false;
                Player.GetDamage(DamageClass.Summon) -= 0.08f;
                Player.maxMinions -= 2;
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
            if (hasNobleShield)
            {
                target.AddBuff(ModContent.BuffType<NobleShieldDebuff>(), 300);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasNobleShield)
            {
                target.AddBuff(ModContent.BuffType<NobleShieldDebuff>(), 300);
            }
        }
    }

    public class NobleShieldoftheGodofNature : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<NobleShieldPlayer>().hasNobleShield = true;
            player.GetDamage(DamageClass.Summon) += 0.12f;
            player.maxMinions += 3;
            player.maxTurrets += 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<DistortedShieldofCthulhu>(), 1)
                .AddIngredient(ItemID.CobaltShield, 1)
                .AddRecipeGroup("jarkos:OldOnesArmyAccessories", 1)
                .AddIngredient(ItemID.Bone, 50)
                .AddIngredient(ItemID.BewitchingTable, 1)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }

    public class OldOnesArmyRecipeGroupSystem : ModSystem
    {
        public override void AddRecipeGroups()
        {
            RecipeGroup group = new RecipeGroup(() => "Any accessory old army",
                ItemID.HuntressBuckler,
                ItemID.ApprenticeScarf,
                ItemID.SquireShield,
                ItemID.MonkBelt
            );
            RecipeGroup.RegisterGroup("jarkos:OldOnesArmyAccessories", group);
        }
    }
}
