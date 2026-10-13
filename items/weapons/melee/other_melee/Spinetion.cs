using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.other_melee;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.other_melee
{
	public class Spinetion : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 52;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<Spinetion_slash>();
            Item.shootSpeed = 18f;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 50, 0);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<SpinetionPlayer>();
            if (player.altFunctionUse == 2)
            {
                if (modPlayer.instakillCooldown > 0)
                    return false;
                return player.ownedProjectileCounts[ModContent.ProjectileType<Spinetion_slash_instakill>()] < 1;
            }
            return player.ownedProjectileCounts[ModContent.ProjectileType<Spinetion_slash>()] < 1;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPos = player.Center;
            
            int targetNPCIndex = -1;
            float closestDist = 200f * 200f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distSq = Vector2.DistanceSquared(Main.MouseWorld, npc.Center);
                    if (distSq < closestDist)
                    {
                        closestDist = distSq;
                        targetNPCIndex = i;
                    }
                }
            }

            Vector2 launchDir = velocity;

            float targetRange = 450f;
            if (targetNPCIndex != -1)
            {
                Vector2 toTarget = Main.npc[targetNPCIndex].Center - player.Center;
                launchDir = toTarget.SafeNormalize(velocity);
                targetRange = toTarget.Length();
            }
            else
            {
                float distance = Vector2.Distance(player.Center, Main.MouseWorld);
                targetRange = MathHelper.Clamp(distance, 150f, 550f);
            }

            if (player.altFunctionUse == 2)
            {
                var modPlayer = player.GetModPlayer<SpinetionPlayer>();
                modPlayer.instakillCooldown = 1500;
                Projectile.NewProjectile(source, spawnPos, launchDir * 18f, ModContent.ProjectileType<Spinetion_slash_instakill>(), damage * 2, knockback, player.whoAmI, 0f, targetRange, targetNPCIndex);
            }
            else
            {
                Projectile.NewProjectile(source, spawnPos, launchDir * 18f, ModContent.ProjectileType<Spinetion_slash>(), damage, knockback, player.whoAmI, 0f, targetRange, targetNPCIndex);
            }

            return false;
        }
		public override void AddRecipes()
		{
    		CreateRecipe()
        	.AddIngredient(ModContent.ItemType<ThickSilk>(), 8)
			.AddIngredient(ItemID.Bone, 225)
			.AddTile(TileID.Anvils)
        	.Register();
		}
    }

    public class SpinetionPlayer : ModPlayer
    {
        public int instakillCooldown = 0;

        public override void PostUpdate()
        {
            if (instakillCooldown > 0)
                instakillCooldown--;
        }
    }
}
