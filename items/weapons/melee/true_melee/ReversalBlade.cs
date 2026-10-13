using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.melee.true_melee;
using Terraria.Audio;
using jarkos.items.usetiles;
using jarkos.items.materials;
using Jarkos.items.materials;
using jarkos.particles;
using Terraria.DataStructures;

namespace jarkos.items.weapons.melee.true_melee
{
	public class ReversalBlade : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 63;
            Item.DamageType = DamageClass.Melee;
            Item.width = 106;
            Item.height = 108;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<ReversalBladeSwing>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 0, 0);
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ReversalBladePlayer>();

            if (player.altFunctionUse == 2)
            {
                if (modPlayer.riftCooldown > 0 || player.ownedProjectileCounts[ModContent.ProjectileType<ReversalRiftGate>()] > 0)
                    return false;
                Item.useTime = 30;
                Item.useAnimation = 30;
                return true;
            }

            int step = modPlayer.comboStep;
            int swingTime = step == 2 ? 40 : 28;
            Item.useTime = swingTime + 2;
            Item.useAnimation = swingTime + 2;

            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 toCursor = Main.MouseWorld - player.MountedCenter;
                float dist = MathHelper.Clamp(toCursor.Length(), 90f, ReversalRiftGate.CAST_RANGE);
                Vector2 spawnPos = player.MountedCenter + toCursor.SafeNormalize(Vector2.UnitX * player.direction) * dist;

                Projectile.NewProjectile(source, spawnPos, toCursor.SafeNormalize(Vector2.UnitX * player.direction), ModContent.ProjectileType<ReversalRiftGate>(), (int)(damage * 0.55f), knockback * 0.5f, player.whoAmI);
                player.GetModPlayer<ReversalBladePlayer>().riftCooldown = ReversalRiftGate.COOLDOWN;

                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f, Pitch = -0.5f }, spawnPos);
                return false;
            }

            var combo = player.GetModPlayer<ReversalBladePlayer>();
            int step = combo.UseSwing();
            int angDir = step == 1 ? -1 : 1;
            if (step == 2)
                damage = (int)(damage * 1.4f);
                
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
            Projectile.NewProjectile(source, player.MountedCenter, dir, ModContent.ProjectileType<ReversalBladeSwing>(), damage, knockback, player.whoAmI, step, angDir);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 6);
            recipe.AddIngredient(ModContent.ItemType<EssentialPowder>(), 2);
            recipe.AddIngredient(ModContent.ItemType<ResidualShadow>(), 4);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
    public class ReversalBladePlayer : ModPlayer
    {
        public const int COMBO_WINDOW = 70;
        public int comboStep;
        public int comboTimer;
        public int riftCooldown;
        public override void PostUpdate()
        {
            if (comboTimer > 0)
            {
                comboTimer--;
                if (comboTimer == 0)
                    comboStep = 0;
            }
            if (riftCooldown > 0)
            {
                riftCooldown--;
                if (riftCooldown == 0 && Main.myPlayer == Player.whoAmI)
                {
                    new Spark(Player.MountedCenter, -Vector2.UnitY * 2f, ReversalBladeSwing.ColorGold, 24, 0.6f, 0.9f).Spawn();
                    SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.5f, Pitch = 0.3f }, Player.Center);
                }
            }
        }
        public int UseSwing()
        {
            int step = comboStep;
            comboStep = comboStep >= 2 ? 0 : comboStep + 1;
            comboTimer = COMBO_WINDOW;
            return step;
        }
    }
}
