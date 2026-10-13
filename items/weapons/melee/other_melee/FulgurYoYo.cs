using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using jarkos.projectiles.melee.other_melee;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.other_melee
{
	public class FulgurYoYo : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.Yoyo[Item.type] = true;
			ItemID.Sets.GamepadExtraRange[Item.type] = 20;
			ItemID.Sets.GamepadSmartQuickReach[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.width = 30;
			Item.height = 26;
			Item.useAnimation = 9;
			Item.useTime = 9;
			Item.shootSpeed = 48f;
			Item.knockBack = 2.5f;
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.shoot = ProjectileType<Lazatep_proj>();

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
		}
		private static readonly int[] unwantedPrefixes = new int[] { PrefixID.Terrible, PrefixID.Dull, PrefixID.Shameful, PrefixID.Annoying, PrefixID.Broken, PrefixID.Damaged, PrefixID.Shoddy };
		public override bool AllowPrefix(int pre) 
		{
			if (Array.IndexOf(unwantedPrefixes, pre) > -1) 
			{
				return false;
			}
			return true;
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FulgurBar>(), 8)
			.AddIngredient(ItemID.Cobweb, 24)
			.AddTile(TileID.Anvils)
			.Register();
            }
		}
	}
