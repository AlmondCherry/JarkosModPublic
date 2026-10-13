using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
    public class AmuletDruidPlayer : ModPlayer
    {
        public bool hasAmuletDruid;
        public int whipBuffTimer;

        public override void ResetEffects()
        {
            hasAmuletDruid = false;
        }

        public override void PostUpdateEquips()
        {
            if (!hasAmuletDruid)
                return;

            if (whipBuffTimer > 0)
            {
                whipBuffTimer--;

                Player.GetDamage(DamageClass.Summon) += 0.08f;
                Player.GetAttackSpeed(DamageClass.Summon) += 0.06f;
                Player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.06f;

                Player.lifeRegen += 2;
            }

            bool equippedEssence = false;
            bool equippedPapyrus = false;
            bool equippedScroll = false;
            bool equippedBeetle = false;

            for (int i = 3; i < 10 + Player.extraAccessorySlots; i++)
            {
                Item armor = Player.armor[i];
                if (armor != null && !armor.IsAir)
                {
                    if (armor.type == ModContent.ItemType<EssenceDruid>()) equippedEssence = true;
                    else if (armor.type == ItemID.PapyrusScarab) equippedPapyrus = true;
                    else if (armor.type == ItemID.NecromanticScroll) equippedScroll = true;
                    else if (armor.type == ItemID.HerculesBeetle) equippedBeetle = true;
                }
            }

            if (equippedEssence)
            {
                Player.GetDamage(DamageClass.Summon) -= 0.14f;
                Player.maxMinions -= 2;
                Player.GetKnockback(DamageClass.Summon) -= 2f;
            }
            if (equippedPapyrus)
            {
                Player.GetDamage(DamageClass.Summon) -= 0.15f;
                Player.maxMinions -= 1;
                Player.GetKnockback(DamageClass.Summon) -= 2f;
            }
            if (equippedScroll)
            {
                Player.GetDamage(DamageClass.Summon) -= 0.10f;
                Player.maxMinions -= 1;
            }
            if (equippedBeetle)
            {
                Player.GetDamage(DamageClass.Summon) -= 0.15f;
                Player.GetKnockback(DamageClass.Summon) -= 2f;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasAmuletDruid && (proj.DamageType == DamageClass.SummonMeleeSpeed || ProjectileID.Sets.IsAWhip[proj.type]))
            {
                whipBuffTimer = 180;

                for (int i = 0; i < 8; i++)
                {
                    Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.JungleSpore,
                        Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.3f);
                    d.noGravity = true;
                }
            }
        }
    }

    public class AmuletDruid : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(gold: 15);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<AmuletDruidPlayer>().hasAmuletDruid = true;
            player.GetDamage(DamageClass.Summon) += 0.16f;
            player.maxMinions += 3;
            player.GetKnockback(DamageClass.Summon) += 2f;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<CelestialShell>(), 1)
                .AddIngredient(ModContent.ItemType<EssenceDruid>(), 1)
                .AddIngredient(ItemID.LunarBar, 14)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
