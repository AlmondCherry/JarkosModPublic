using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.other_melee;
using jarkos.items.usetiles;
using jarkos.items.materials;
using Terraria.Audio;

namespace jarkos.items.weapons.melee.other_melee
{
	public class Antique_melee_axe : ModItem
	{
        public const int MaxAxes = 5;
        private int activeAxes = 0;
        private bool wasHolding = false;
        private int throwCooldown = 0;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 952;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Antique_melee_axe_proj>();
            Item.shootSpeed = 12f;
            Item.channel = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
        }
        public override bool CanUseItem(Player player)
        {
            activeAxes = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<Antique_melee_axe_proj>())
                {
                    activeAxes++;
                }
            }
            return activeAxes < MaxAxes && throwCooldown <= 0;
        }
        public override void HoldItem(Player player)
        {
            if (throwCooldown > 0)
                throwCooldown--;

            bool isHolding = player.channel && player.controlUseItem;

            if (wasHolding && !isHolding)
            {
                RecallAllAxes(player);
            }

            wasHolding = isHolding;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            throwCooldown = 12;
            Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(10));

            Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
            SoundEngine.PlaySound(SoundID.Item71, player.position);
            return false;
        }
        private void RecallAllAxes(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];

                if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<Antique_melee_axe_proj>())
                {
                    var axeProj = proj.ModProjectile as Antique_melee_axe_proj;

                    if (axeProj != null)
                    {
                        axeProj.StartReturning();
                    }
                }
            }
            
           SoundEngine.PlaySound(SoundID.Item122, player.position);
        }
		public override void AddRecipes()
			{
    			CreateRecipe()
        		.AddIngredient(ModContent.ItemType<DarkPeddlerTrue>(), 1)
				.AddIngredient(ModContent.ItemType<AntiqueBar>(), 12)
                .AddIngredient(ModContent.ItemType<AncientFragment>(), 20)
                .AddIngredient(ModContent.ItemType<ElementalFluid>(), 40)
                .AddTile(TileID.MythrilAnvil)
        		.Register();
			}
	}
}
