using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using jarkos.projectiles.melee.unique_melee;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.unique_melee
{
	public class Slovo_berserkera : ModItem
	{
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 47;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Wordofberserker_proj>();
            Item.shootSpeed = 1f;
            Item.channel = false;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 30, 0);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<MagnetarPlayer>();
            if (player.altFunctionUse == 2)
            {
                if (modPlayer.electricStormCooldown > 0)
                {
                    return false;
                }
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 45;
                Item.useAnimation = 45;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 28;
                Item.useAnimation = 28;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<MagnetarPlayer>();
            if (player.altFunctionUse == 2)
            {
                modPlayer.electricStormCooldown = 1800;
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<BerserkerStormGenerator>(), damage, knockback, player.whoAmI);
                return false;
            }
            float rotation = (Main.MouseWorld - player.Center).ToRotation();
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, rotation, player.direction);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.3f }, player.Center);
            return false;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (Main.raining)
            {
                damage += 0.25f;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddIngredient(ModContent.ItemType<EssentialPowder>(), 2)
            .AddIngredient(ModContent.ItemType<PulsatingFragment>(), 4)
            .AddTile(TileID.MythrilAnvil)
            .Register();
        }
	}
	public class MagnetarPlayer : ModPlayer
    {
        private float screenShakeIntensity = 0f;
        private int screenShakeDuration = 0;
        public bool isPulling = false;
        public bool canLevitateTiles = false;
        public int electricStormCooldown = 0;
        public void AddScreenShake(float intensity, int duration)
        {
            if (intensity > screenShakeIntensity)
            {
                screenShakeIntensity = intensity;
                screenShakeDuration = duration;
            }
        }
        public override void ModifyScreenPosition()
        {
            if (screenShakeDuration > 0)
            {
                screenShakeDuration--;
                float progress = (float)screenShakeDuration / 20f;
                float currentIntensity = screenShakeIntensity * progress;
                Main.screenPosition += new Vector2(Main.rand.NextFloat(-currentIntensity, currentIntensity), Main.rand.NextFloat(-currentIntensity, currentIntensity));
                if (screenShakeDuration <= 0)
                {
                    screenShakeIntensity = 0f;
                }
            }
        }
        public override void PostUpdate()
        {
            if (electricStormCooldown > 0)
            {
                electricStormCooldown--;
            }
            if (Player.HeldItem.type == ModContent.ItemType<Slovo_berserkera>())
            {
                float pullRadius = 200f;
                foreach (Item item in Main.item)
                {
                    if (!item.active || item.Distance(Player.Center) > pullRadius)
                        continue;
                    if (IsMetallicItem(item))
                    {
                        Vector2 direction = (Player.Center - item.Center).SafeNormalize(Vector2.Zero);
                        item.velocity += direction * 0.5f;
                        item.velocity *= 0.95f;
                        if (Main.rand.NextBool(10))
                        {
                            Dust dust = Dust.NewDustPerfect(item.Center, DustID.Electric, direction * 2f, 0, Color.Cyan, 0.5f);
                            dust.noGravity = true;
                        }
                    }
                }
                if (Main.rand.NextBool(5))
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(50f, 50f);
                    Dust dust = Dust.NewDustPerfect(Player.Center + offset, DustID.Electric, -offset * 0.02f, 0, Color.Cyan, 0.6f);
                    dust.noGravity = true;
                }
            }
        }
        private bool IsMetallicItem(Item item)
        {
            if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin)
                return true;
            if (item.Name.ToLower().Contains("bar") || item.Name.ToLower().Contains("ingot") || item.Name.ToLower().Contains("ore"))
                return true;
            string name = item.Name.ToLower();
            if (name.Contains("iron") || name.Contains("copper") || name.Contains("silver") || name.Contains("gold") || name.Contains("platinum") || name.Contains("titanium") || name.Contains("adamantite") || name.Contains("cobalt") || name.Contains("mythril") || name.Contains("orichalcum") || name.Contains("palladium") || name.Contains("chlorophyte") || name.Contains("luminite") || name.Contains("meteor"))
                return true;
            return false;
        }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (Player.HeldItem.type == ModContent.ItemType<Slovo_berserkera>())
            {
                Lighting.AddLight(Player.Center, 0.2f, 0.5f, 0.8f);
            }
        }
    }
}
