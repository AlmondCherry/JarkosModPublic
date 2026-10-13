using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using jarkos.projectiles.melee.true_melee;
using Microsoft.Xna.Framework.Graphics;
using System;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Retribution : ModItem
	{
		public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 45;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.UseSound = SoundID.NPCDeath1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Retribution_slash>();
            Item.shootSpeed = 14f;
            Item.mana = 12;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool CanUseItem(Player player)
        {
            BloodThreadPlayer modPlayer = player.GetModPlayer<BloodThreadPlayer>();
            if (modPlayer.HasAttachedThreads)
            {
                if (Main.rand.NextBool(10))
                {
                    Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20, 20);
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.Blood);
                    dust.velocity = Vector2.Zero;
                    dust.scale = 0.8f;
                    dust.noGravity = true;
                    dust.alpha = 150;
                }
                return false;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int numThreads = 5;
            float spread = MathHelper.ToRadians(30);

            for (int i = 0; i < numThreads; i++)
            {
                float angle = velocity.ToRotation() - spread / 2 + spread * ((float)i / (numThreads - 1));
                Vector2 newVelocity = angle.ToRotationVector2() * velocity.Length();
                newVelocity *= 1f + Main.rand.NextFloat(-0.1f, 0.1f);
                Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI, ai0: i);
            }

            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(0.3f, 0.8f);
                Dust dust = Dust.NewDustDirect(position, 10, 10, DustID.Blood, dustVel.X, dustVel.Y);
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.3f }, position);
            return false;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position += velocity.SafeNormalize(Vector2.Zero) * 20f;
        }
        public override void HoldItem(Player player)
        {
            BloodThreadPlayer modPlayer = player.GetModPlayer<BloodThreadPlayer>();
            if (modPlayer.HasAttachedThreads)
            {
                float pulse = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f;
                Lighting.AddLight(player.Center, 1.0f * pulse, 0.1f * pulse, 0.1f * pulse);
                if (Main.rand.NextBool(5))
                {
                    Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(50, 50);
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.LifeDrain);
                    dust.velocity = (player.Center - dustPos) * 0.08f;
                    dust.scale = 1.0f;
                    dust.noGravity = true;
                }
            }
            else
            {
                Lighting.AddLight(player.Center, 0.8f, 0.1f, 0.1f);
                if (Main.rand.NextBool(10))
                {
                    Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(30, 30);
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.Blood);
                    dust.velocity = (player.Center - dustPos) * 0.05f;
                    dust.scale = 1.2f;
                    dust.noGravity = true;
                }
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 50, 50, 200);
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Lighting.AddLight(Item.Center, 0.5f, 0.05f, 0.05f);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 10)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddIngredient(ItemID.SpiderFang, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
	public class BloodThreadPlayer : ModPlayer
    {
        public bool HasAttachedThreads => GetAttachedThreadCount() > 0;
        public int GetAttachedThreadCount()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<Retribution_slash>() && proj.owner == Player.whoAmI)
                {
                    var threadProj = proj.ModProjectile as Retribution_slash;
                    if (threadProj != null && threadProj.IsAttached)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        public void DetachAllThreads()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<Retribution_slash>() && proj.owner == Player.whoAmI)
                {
                    var threadProj = proj.ModProjectile as Retribution_slash;
                    if (threadProj != null && threadProj.IsAttached)
                    {
                        threadProj.ForceDetach();
                    }
                }
            }
        }
    }
}
