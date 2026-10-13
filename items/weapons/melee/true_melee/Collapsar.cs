using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.usetiles;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Collapsar : ModItem
    {
        public const int UltCooldownTicks = 60 * 25;

        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 66;

            Item.damage = 120;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6.5f;
            Item.crit = 10;

            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;

            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.shoot = ModContent.ProjectileType<CollapsarSwing>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 5);

            Item.UseSound = null;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.HasBuff<CollapsarCooldown>())
                    return false;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<CollapsarUltimate>()] > 0)
                    return false;

                Item.useTime = CollapsarUltimate.TotalDuration;
                Item.useAnimation = CollapsarUltimate.TotalDuration;
                Item.autoReuse = false;
            }
            else
            {
                Item.useTime = 36;
                Item.useAnimation = 36;
                Item.autoReuse = true;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                player.AddBuff(ModContent.BuffType<CollapsarCooldown>(), UltCooldownTicks);

                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                    ModContent.ProjectileType<CollapsarUltimate>(),
                    (int)(damage * 1.5f), knockback, player.whoAmI);
                return false;
            }

            float swingDir = player.GetModPlayer<CollapsarPlayer>().FlipSwing();

            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI, 0f, swingDir);
            return false;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Player player = Main.LocalPlayer;
            int buffIndex = player.FindBuffIndex(ModContent.BuffType<CollapsarCooldown>());
            if (buffIndex < 0)
                return;

            int buffTime = player.buffTime[buffIndex];
            if (buffTime <= 0)
                return;

            float progress = MathHelper.Clamp(buffTime / (float)UltCooldownTicks, 0f, 1f);
            if (progress <= 0f)
                return;

            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Vector2 topLeft = position - origin * scale;
            float totalWidth = frame.Width * scale;
            float totalHeight = frame.Height * scale;
            float overlayHeight = totalHeight * progress;

            spriteBatch.Draw(texture, position, frame, Color.Black * 0.6f * progress, 0f, origin, scale, SpriteEffects.None, 0f);

            Rectangle overlayRect = new((int)topLeft.X, (int)topLeft.Y, (int)totalWidth, (int)overlayHeight);
            spriteBatch.Draw(pixel, overlayRect, Color.Black * 0.55f);

            Rectangle lineRect = new((int)topLeft.X, (int)(topLeft.Y + overlayHeight - 2f), (int)totalWidth, 2);
            spriteBatch.Draw(pixel, lineRect, Color.Cyan * 0.85f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 4)
                .AddIngredient(ModContent.ItemType<ElementalPowder>(), 2)
                .AddIngredient(ModContent.ItemType<BulbSunEnergy>(), 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class CollapsarPlayer : ModPlayer
    {
        private int _swingDir = 1;

        public float FlipSwing()
        {
            _swingDir = -_swingDir;
            return _swingDir;
        }
    }
}
