using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;
using jarkos.items.usetiles;
using jarkos.buffs.debuff;

namespace jarkos.items.accs.species
{
	public class EmblemOfRage : ModItem
    {
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 42;
			Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FuryEmblemPlayer>().furyEmblemEquipped = true;
            player.statDefense *= 0.8f;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 20)
			.AddIngredient(ItemID.SoulofFright, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class FuryEmblemPlayer : ModPlayer
    {
        public bool furyEmblemEquipped;
        private NPC markedTarget = null;
        private int markStacks = 0;
        private const int MaxStacks = 10;
        public override void ResetEffects()
        {
            furyEmblemEquipped = false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!furyEmblemEquipped) return;
            if (target.immortal || target.friendly || target.life <= 0) return;

            if (markedTarget != null && markedTarget != target && markedTarget.active)
            {
                ResetMarkOnTarget(markedTarget);
            }

            markedTarget = target;

            if (markStacks < MaxStacks)
            {
                markStacks++;
            }

            ApplyMarkDebuff(target, markStacks);

            if (markStacks > 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.RedTorch);
                    dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                    dust.scale = 1f + (markStacks * 0.1f);
                    dust.noGravity = true;
                }
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            OnHitNPC(target, hit, damageDone);
        }
        private void ApplyMarkDebuff(NPC target, int stacks)
        {
            if (target.HasBuff(ModContent.BuffType<FuryMarkDebuff>()))
            {
                target.DelBuff(target.FindBuffIndex(ModContent.BuffType<FuryMarkDebuff>()));
            }

            target.AddBuff(ModContent.BuffType<FuryMarkDebuff>(), 300);
            
            FuryMarkGlobalNPC markNPC = target.GetGlobalNPC<FuryMarkGlobalNPC>();
            markNPC.furyMarkStacks = stacks;
        }
        private void ResetMarkOnTarget(NPC target)
        {
            if (target.active)
            {
                target.DelBuff(target.FindBuffIndex(ModContent.BuffType<FuryMarkDebuff>()));
                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Smoke);
                    dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    dust.scale = 1.2f;
                    dust.noGravity = true;
                }
            }
            markStacks = 0;
            markedTarget = null;
        }
        public override void PostUpdate()
        {
            if (markedTarget != null && (!markedTarget.active || !markedTarget.HasBuff(ModContent.BuffType<FuryMarkDebuff>())))
            {
                markStacks = 0;
                markedTarget = null;
            }
            if (markedTarget != null && markedTarget.active && furyEmblemEquipped)
            {
                Lighting.AddLight(markedTarget.Center, 0.8f * (markStacks / 10f), 0.1f, 0.1f);
            }
        }
        public (NPC target, int stacks) GetMarkInfo()
        {
            return (markedTarget, markStacks);
        }
    }
}
