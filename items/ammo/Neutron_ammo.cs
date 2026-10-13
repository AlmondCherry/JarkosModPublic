using jarkos.projectiles.bullet;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Neutron_ammo : ModItem
	{
		public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 14;
            Item.damage = 18;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(copper: 20);
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.shoot = ModContent.ProjectileType<Neutron_bullet>();
            Item.shootSpeed = 8f;
            Item.ammo = AmmoID.Bullet;
        }
		public override void AddRecipes()
        {
            CreateRecipe(100)
                .AddIngredient(ItemID.MusketBall, 100)
                .AddIngredient(ItemID.SoulofMight, 1)
                .AddIngredient(ItemID.Meteorite, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
	public class NeutronDamageGlobalNPC : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.type == ModContent.ProjectileType<Neutron_bullet>())
            {
                modifiers.ScalingArmorPenetration += 0.5f;
            }
            if (projectile.type == ModContent.ProjectileType<Neutron_arrow_proj>())
            {
                modifiers.ScalingArmorPenetration += 0.3f;
            }
        }
    }
}
