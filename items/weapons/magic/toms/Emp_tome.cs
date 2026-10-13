using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.magic.toms;
using jarkos.items.materials;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.magic.toms
{
	public class Emp_tome : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 18;
			Item.DamageType = DamageClass.Magic;
            Item.width = 46;
            Item.height = 74;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 18;
            Item.damage = 147;
            Item.autoReuse = true;

			Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();

            Item.noMelee = true;
			Item.scale = 1f;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.channel = true;
			Item.mana = 5;
            Item.shoot = ModContent.ProjectileType<LB_Penta>();
			Item.shootSpeed = 1f;

            Item.value = Item.sellPrice(0, 50, 10, 0);
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            damage.Base = 0;
        }
       public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0f);
		}
        public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 16)
			.AddIngredient(ModContent.ItemType<EnchantedFragment>(), 12)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 15)
			.AddTile(TileID.Bookcases)
			.Register();
        }
    }
}
