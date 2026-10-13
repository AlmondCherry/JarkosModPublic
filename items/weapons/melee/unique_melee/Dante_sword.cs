using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.unique_melee;
using Jarkos.items.usetiles;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
namespace jarkos.items.weapons.melee.unique_melee
{
	public class Dante_sword : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 90;
			Item.DamageType = DamageClass.Melee;
			Item.width = 62;
			Item.height = 68;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 2;
			Item.shoot = ModContent.ProjectileType<Dante_sword_swing>();
			Item.shootSpeed = 1f;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.shootsEveryUse = true;
			Item.autoReuse = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 0, 40, 0);
		}
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Dante_sword_thrown>()] > 0 || player.ownedProjectileCounts[ModContent.ProjectileType<Dante_sword_charge>()] > 0)
                return false;
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 30;
                Item.useAnimation = 30;
                return true;
            }
            int swingTime = 30;
            Item.useTime = swingTime + 2;
            Item.useAnimation = swingTime + 2;
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<Dante_sword_charge>(), damage, knockback, player.whoAmI);
                return false;
            }
            var combo = player.GetModPlayer<DanteSwordPlayer>();
            int step = combo.UseSwing();
            int angDir = step == 1 ? -1 : 1;
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
            Projectile.NewProjectile(source, player.MountedCenter, dir, ModContent.ProjectileType<Dante_sword_swing>(), damage, knockback, player.whoAmI, step, angDir);
            return false;
        }
		public override void AddRecipes()
		{
    		CreateRecipe()
			.AddRecipeGroup(nameof(ItemID.IronBar), 12)
			.AddIngredient(ItemID.SoulofNight, 20)
			.AddTile(TileID.MythrilAnvil)
        	.Register();
		}
	}
    public class DanteSwordPlayer : ModPlayer
    {
        public const int COMBO_WINDOW = 70;
        public int comboStep;
        public int comboTimer;
        public override void PostUpdate()
        {
            if (comboTimer > 0)
            {
                comboTimer--;
                if (comboTimer == 0)
                    comboStep = 0;
            }
        }
        public int UseSwing()
        {
            int step = comboStep;
            comboStep = comboStep >= 1 ? 0 : comboStep + 1;
            comboTimer = COMBO_WINDOW;
            return step;
        }
    }
}
