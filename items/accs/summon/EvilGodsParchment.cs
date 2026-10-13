using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;
using Microsoft.Xna.Framework;
using jarkos.buffs.debuff;

namespace jarkos.items.accs.summon
{
	public class EvilGodsParchment : ModItem
    {
		public override void SetDefaults()
		{
			Item.width = 42;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<EvilGodsParchmentPlayer>();
			modPlayer.hasEvilGodsParchment = true;
			player.maxMinions += 1;
		}
		public override bool CanEquipAccessory(Player player, int slot, bool modded)
		{
			var jarkosplayer = player.GetModPlayer<Jarkos_player>();
			if (!jarkosplayer.nature_dar && !jarkosplayer.Radiant_Generator)
			{
				return true;
			}
			return false;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.TissueSample), 8)
			.AddIngredient(ModContent.ItemType<ElementalPowder>(), 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}

	public class EvilGodsParchmentGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool BleedingParchDebuff;

        public override void ResetEffects(NPC npc)
        {
            BleedingParchDebuff = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (BleedingParchDebuff && !npc.boss && npc.lifeMax < 10000)
            {
               npc.velocity *= 0.9f;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (BleedingParchDebuff && Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood, 0f, -1f, 100, default, 1.2f);
                d.noGravity = true;
            }
        }
    }

	public class EvilGodsParchmentPlayer : ModPlayer
    {
        public bool hasEvilGodsParchment;

        public override void ResetEffects()
        {
            hasEvilGodsParchment = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasEvilGodsParchment)
            {
                target.AddBuff(ModContent.BuffType<BleedingMobDebuff>(), 300);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasEvilGodsParchment)
            {
                target.AddBuff(ModContent.BuffType<BleedingMobDebuff>(), 300);
            }
        }
    }
}
