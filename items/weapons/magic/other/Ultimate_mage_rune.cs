using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using jarkos.projectiles.magic.other;
using System;

namespace jarkos.items.weapons.magic.other
{
	public class Ultimate_mage_rune : ModItem
	{
		private int attackPhase = 0;
        public override void SetDefaults()
        {
            Item.damage = 95;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 7;
            Item.useAnimation = 7;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item9;
            Item.shoot = ModContent.ProjectileType<Ultimate_mage_rune_proj>();
            Item.shootSpeed = 14f;
            Item.autoReuse = true;
            Item.staff[Item.type] = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position += velocity.SafeNormalize(Vector2.Zero) * 30f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            switch (attackPhase)
            {
                 case 0:
                    ShootCelestialBeam(source, position, velocity, damage, knockback, player);
                    break;
                case 1:
                    ShootCelestialBeam(source, position, velocity, damage, knockback, player);
                    break;
                case 2:
                    ShootCelestialBeam(source, position, velocity, damage, knockback, player);
                    break;
                case 3:
                    ShootCelestialBeam(source, position, velocity, damage, knockback, player);
                    break;
            }
            SpawnRuneActivation(player.Center, attackPhase);
            attackPhase = (attackPhase + 1) % 4;
            return false;
        }
        private void ShootCelestialBeam(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int damage, float knockback, Player player)
        {
            Vector2 target = Main.MouseWorld;
            Vector2 spawnPos = new Vector2(target.X + Main.rand.NextFloat(-50f, 50f), target.Y - 600f);
            Vector2 beamVel = (target - spawnPos).SafeNormalize(Vector2.Zero) * 20f;
            Projectile.NewProjectile(source, spawnPos, beamVel, ModContent.ProjectileType<RuneCelestialBeam>(), (int)(damage * 2.5f), knockback * 2f, player.whoAmI);
            for (int i = 0; i < 30; i++)
            {
                float angle = i * MathHelper.TwoPi / 30f;
                Vector2 dustPos = target + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 40f;
                Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.GoldFlame, 0, -1f, 0, new Color(255, 200, 100), 1.2f);
                dust.noGravity = true;
            }
        }
        private void SpawnRuneActivation(Vector2 center, int phase)
        {
            Color[] phaseColors = new Color[]            {
                new Color(80, 200, 255), new Color(200, 50, 255), new Color(50, 255, 150), new Color(255, 200, 50)
            };
            Color c = phaseColors[phase];
            for (int i = 0; i < 12; i++)
            {
                float angle = i * MathHelper.TwoPi / 12f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                Dust dust = Dust.NewDustDirect(center, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 0, c, 1f);
                dust.noGravity = true;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FireballRune>(), 1)
                .AddIngredient(ModContent.ItemType<EnergyRune>(), 1)
                .AddIngredient(ModContent.ItemType<BeeRune>(), 1)
                .AddIngredient(ModContent.ItemType<BloodRune>(), 1)
                .AddIngredient(ItemID.CrystalShard, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
}
