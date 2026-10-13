using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.toms;
using Microsoft.Xna.Framework;
using jarkos.particles;
using jarkos.items.materials;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.toms
{
    public class HealingTome : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 50;
            Item.damage = 30;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 7;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.knockBack = 2.5f;
            Item.UseSound = SoundID.Item8;
            Item.shoot = ModContent.ProjectileType<HealingSphereProj>();
            Item.shootSpeed = 10.5f;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 30, 0);
        }
        public override bool CanUseItem(Player player)
        {
            return player.altFunctionUse != 2;
        }
        public override void HoldItem(Player player)
        {
            if (Main.myPlayer != player.whoAmI || Main.dedServ)
                return;
            bool recalling = Main.mouseRight && !player.mouseInterface && !player.dead;
            player.GetModPlayer<HealPlayerTome>().Recalling = recalling;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 vel = velocity.RotatedByRandom(0.06f);
            Projectile.NewProjectile(source, position, vel, type, damage, knockback, player.whoAmI, 0f, Main.rand.NextFloat(MathHelper.TwoPi));
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book)
                .AddIngredient(ModContent.ItemType<FragmentOfNature>(), 20)
                .AddIngredient(ItemID.JungleSpores, 8)
                .AddIngredient(ItemID.RegenerationPotion, 1)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
    public class HealPlayerTome : ModPlayer
    {
        public bool Recalling;
        public override void ResetEffects()
        {
            Recalling = false;
        }
        public override void PostUpdate()
        {
            if (!Recalling || Main.dedServ)
                return;
            Lighting.AddLight(Player.Center, 0.25f, 0.6f, 0.3f);
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = Player.Center + Main.rand.NextVector2Circular(26f, 30f);
                Dust d = Dust.NewDustPerfect(pos, DustID.ChlorophyteWeapon, new Vector2(0f, -Main.rand.NextFloat(0.6f, 1.4f)), 100, default, Main.rand.NextFloat(0.7f, 1.1f));
                d.noGravity = true;
            }
            if (Main.rand.NextBool(14))
            {
                new Spark(Player.Center + Main.rand.NextVector2Circular(24f, 28f), new Vector2(0f, -1f), new Color(160, 255, 170), Main.rand.Next(14, 24), Main.rand.NextFloat(0.25f, 0.45f), 0.8f).Spawn();
            }
        }
    }
}
