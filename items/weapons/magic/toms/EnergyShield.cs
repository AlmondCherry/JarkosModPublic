using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.particles;
using jarkos.projectiles.magic.toms;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.toms
{
	public class EnergyShield : ModItem
	{
		public static readonly Color ColorCyan = new Color(120, 215, 255);
        public static readonly Color ColorDeep = new Color(58, 122, 235);
        public static readonly Color ColorWhite = new Color(232, 250, 255);
        public static readonly Color ColorDanger = new Color(255, 96, 150);
        public override void SetDefaults()
        {
            Item.damage = 90;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 25;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noUseGraphic = true;
            Item.knockBack = 6f;
            Item.UseSound = SoundID.Item28;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<EnergyShieldProj>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 50, 0);
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<EnergyShieldProj>()] == 0 && player.GetModPlayer<EnergyShieldPlayer>().ShieldCooldown <= 0;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 12)
                .AddIngredient(ItemID.SoulofSight, 10)
                .AddIngredient(ItemID.Wire, 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class EnergyShieldPlayer : ModPlayer
    {
        public int ShieldCooldown;
        public override void PostUpdate()
        {
            if (ShieldCooldown > 0)
            {
                ShieldCooldown--;
                if (ShieldCooldown == 0 && Main.netMode != NetmodeID.Server && Player.HeldItem.type == ModContent.ItemType<EnergyShield>())
                {
                    for (int i = 0; i < 8; i++)
                        new Spark(Player.Center + Main.rand.NextVector2Circular(16f, 16f), Main.rand.NextVector2Circular(1.5f, 1.5f), EnergyShield.ColorCyan, 18, Main.rand.NextFloat(0.3f, 0.5f), 0.85f).Spawn();
                    new Circleedge_tight(Player.Center, Vector2.Zero, EnergyShield.ColorWhite, 14, 0.6f, 0.8f).Spawn();
                }
            }
        }
    }
}
