using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.magic.toms;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.toms
{
	public class BlackHole : ModItem
	{
		public static readonly Color ColorHot = new Color(255, 226, 168);
        public static readonly Color ColorMid = new Color(255, 142, 48);
        public static readonly Color ColorEdge = new Color(208, 92, 38);
        public static readonly Color ColorVoid = new Color(26, 12, 38);
        public override void SetDefaults()
        {
            Item.damage = 260;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 25;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 8f;

            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();

            Item.UseSound = SoundID.Item117;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<BlackHoleProj>();
            Item.shootSpeed = 0f;
            Item.noMelee = true;
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<BlackHoleProj>()] == 0;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpellTome)
                .AddIngredient(ItemID.FragmentNebula, 14)
                .AddIngredient(ItemID.LunarBar, 12)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
