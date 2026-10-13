using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.buffs.buff;

namespace jarkos.items.potions
{
	public class Madness_potion : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
			ItemID.Sets.DrinkParticleColors[Type] = new Color[3]
			{
				new Color(240, 240, 240), new Color(200, 200, 200), new Color(140, 140, 140)
			};
		}
		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 36;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = 30;
			Item.consumable = true;
			Item.buffType = ModContent.BuffType<Madness_potion_buff>();
			Item.buffTime = 28000;
		}
        public override bool? UseItem(Player player)
        {
			var modPl = player.GetModPlayer<Jarkos_player>();

			modPl.random_endurance = Main.rand.NextFloat(0.5f, 0.75f);
			modPl.random_defense = Main.rand.NextFloat(0.25f, 0.75f);
			modPl.random_attack_speed = Main.rand.NextFloat(0.10f, 0.25f);
			modPl.random_damage = Main.rand.NextFloat(0.15f, 0.35f);
			
            return true;
        }
	}
}
