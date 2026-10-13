using jarkos.particles;
using jarkos.projectiles.summon.staff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.summon.staff
{
	public class TurretItem : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 9999;
            Item.autoReuse = false;
            Item.consumable = false;
        }
        public override bool? UseItem(Player player)
        {
            Vector2 mouseWorld = Main.MouseWorld;
            float closestDist = 100f;
            TurretBase closestTurret = null;
            Projectile closestProj = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.ModProjectile is TurretBase turret)
                {
                    float dist = Vector2.Distance(mouseWorld, proj.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTurret = turret;
                        closestProj = proj;
                    }
                }
            }
            if (closestTurret != null && !closestTurret.IsActivated)
            {
                closestTurret.Activate();
                CombatText.NewText(closestProj.Hitbox, Color.Cyan, "ACTIVATED!", true);
                Item.stack--;
                return true;
            }
            else if (closestTurret != null && closestTurret.IsActivated)
            {
                CombatText.NewText(closestProj.Hitbox, Color.Gray, "Already Active", false);
                SoundEngine.PlaySound(SoundID.MenuTick, player.Center);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.MenuClose, player.Center);
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wire, 20)
                .AddIngredient(ItemID.IronBar, 5)
                .AddIngredient(ItemID.Lens, 2)
                .AddTile(TileID.Anvils)
                .Register();
            CreateRecipe()
                .AddIngredient(ItemID.Wire, 20)
                .AddIngredient(ItemID.LeadBar, 5)
                .AddIngredient(ItemID.Lens, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
	}
}
