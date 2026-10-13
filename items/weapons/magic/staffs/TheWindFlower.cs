using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.staffs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;

namespace jarkos.items.weapons.magic.staffs
{
    public class TheWindFlower : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 38;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.width = 48;
            Item.height = 110;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.scale = 0.8f;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<WindFlowerProj>();
            Item.shootSpeed = 10f;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(2));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Windrose_shot") with { Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, position);
            player.GetModPlayer<WindFlowerPlayer>().greenLightTimer = 60;

            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(6f, -4f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.JungleSpores, 8)
                .AddIngredient(ItemID.Cloud, 75)
                .AddIngredient(ItemID.Feather, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class WindFlowerPlayer : ModPlayer
    {
        public int greenLightTimer = 0;

        public override void ResetEffects()
        {
            if (greenLightTimer > 0)
            {
                greenLightTimer--;
                float progress = greenLightTimer / 60f;
                Lighting.AddLight(Player.Center, 0.1f * progress, 0.85f * progress, 0.25f * progress);
            }
        }
    }
}
