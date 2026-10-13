using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using Jarkos.items.materials;
using Terraria.GameContent;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Eternal_reign : ModItem
	{
        public override void SetDefaults()
        {
            Item.damage = 68;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Eternal_reign_swing>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.HasBuff(ModContent.BuffType<buffs.debuff.Eternal_reign_cd>()))
                {
                    return false;
                }
                Item.useTime = 40;
                Item.useAnimation = 40;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.UseSound = null;
                Item.shoot = ModContent.ProjectileType<Eternal_reign_thrown>();
                Item.shootSpeed = 18f;
                Item.noMelee = true;
                Item.noUseGraphic = true;
            }
            else
            {
                Item.useTime = 25;
                Item.useAnimation = 25;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.UseSound = SoundID.Item1;
                Item.shoot = ModContent.ProjectileType<Eternal_reign_swing>();
                Item.shootSpeed = 14f;
                Item.noMelee = true;
                Item.noUseGraphic = true;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 targetPos = Main.MouseWorld;
                Vector2 direction = targetPos - player.Center;
                if (direction.Length() > 0)
                    direction.Normalize();
                Projectile.NewProjectile(source, player.Center, direction * Item.shootSpeed, ModContent.ProjectileType<Eternal_reign_thrown>(), (int)(damage * 1.5f), knockback, player.whoAmI);
                player.AddBuff(ModContent.BuffType<buffs.debuff.Eternal_reign_cd>(), 60 * 60);
                return false;
            }
            else
            {
                Vector2 dir = velocity;
                if (dir.Length() > 0)
                    dir.Normalize();

                var combo = player.GetModPlayer<unique_melee.DanteSwordPlayer>();
                int step = combo.UseSwing();
                int angDir = step == 1 ? -1 : 1;

                Projectile.NewProjectile(source, player.Center, dir, ModContent.ProjectileType<Eternal_reign_swing>(), damage, knockback, player.whoAmI, step, angDir);
                return false;
            }
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.CursedTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, default, Main.rand.NextFloat(1.0f, 1.8f));
                d.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.GreenTorch, 0f, 0f, 150, default, Main.rand.NextFloat(0.6f, 1.2f));
                d.noGravity = true;
            }
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Venom, 180);
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 pos = Item.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects;
            float glowPulse = (float)Math.Sin(time * 0.08f) * 0.15f + 0.5f;

            spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(50, 255, 30) * glowPulse * 0.3f, rotation, new Vector2(0.5f, 0.5f), new Vector2(50f, 50f) * scale, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ChlorophyteBar, 12);
            recipe.AddIngredient(ModContent.ItemType<ResidualShadow>(), 8);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
}
