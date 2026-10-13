using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.ranged.bow;
using jarkos.items.materials;

namespace jarkos.items.weapons.ranged.bows
{
	public class Echo_bow : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 52;
            Item.damage = 52;
            Item.knockBack = 2.5f;
            Item.DamageType = DamageClass.Ranged;
            Item.useAmmo = AmmoID.Arrow;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<EchoBowHoldout>();
            Item.shootSpeed = 14f;
            Item.UseSound = null;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] > 0;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<EchoBowHoldout>();
        }

        public override Vector2? HoldoutOffset() => new Vector2(-2, 0);

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Ignis_bow>(), 1)
                .AddIngredient(ModContent.ItemType<Death_bow>(), 1)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ModContent.ItemType<PulsatingFragment>(), 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
}
