using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.mecha;
using Jarkos.items.materials;
using jarkos.items.materials;
using Terraria.DataStructures;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Torped : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 26;
            Item.damage = 42;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.autoReuse = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<Torped_Holdout>();
            Item.shootSpeed = 14f;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Torped_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
		public override void AddRecipes() {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<DiffusalBar>(), 6)
            .AddIngredient(ModContent.ItemType<ThickSilk>(), 2)
            .AddIngredient(ItemID.HallowedBar, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
		}
	}
	public class TorpedoCannonPlayer : ModPlayer
    {
        public readonly int BurstSize = 4;
        public readonly int FireRate = 6;
        public readonly int MaxCooldown = 80;
        public int BurstCounter = 0;
        public int CooldownTimer = 0;
        public int FireTimer = 0;
        public bool IsOverheated = false;
        public bool IsActive = false;
        public override void UpdateDead()
        {
            ResetAll();
        }
        public override void OnRespawn()
        {
            ResetAll();
        }
        private void ResetAll()
        {
            BurstCounter = 0;
            CooldownTimer = 0;
            FireTimer = 0;
            IsOverheated = false;
            IsActive = false;
        }
    }
}
