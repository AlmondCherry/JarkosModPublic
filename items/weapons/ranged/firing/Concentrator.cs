using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using jarkos.projectiles.mecha;
using jarkos.systems;
using jarkos.items.materials;
using Jarkos.items.usetiles;

namespace Jarkos.items.weapons.ranged.firing
{
	public class Concentrator : ModItem
	{
		public int concentrationStacks = 0;
        public int concentrationTimer = 0;
        public const int MaxStacks = 10;
        public const int DecayTime = 30;
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 22;
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Concentrator_Holdout>();
            Item.shootSpeed = 16f;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 5, 0);
        }
        public override void HoldItem(Player player)
        {
            if (concentrationTimer > 0)
            {
                concentrationTimer--;
            }
            else if (concentrationStacks > 0)
            {
                concentrationStacks--;
                concentrationTimer = DecayTime;
            }
            if (concentrationStacks > 0)
            {
                float intensity = (float)concentrationStacks / MaxStacks;
                Lighting.AddLight(player.Center, 0.1f * intensity, 0.3f * intensity, 0.5f * intensity);
                if (Main.rand.NextBool(10 - concentrationStacks / 2))
                {
                    Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Electric, 0f, 0f, 100, new Color(100, 180, 255), 0.1f + intensity * 0.25f);
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                }
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i].Name == "Damage")
                {
                    tooltips[i].Text = Item.damage.ToString() + "-" + "16" + " " + (string)LangSystem.damage_text;
                    tooltips[i].OverrideColor = Color.White;
                }
            }
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }
		public override void AddRecipes() {
		CreateRecipe(1)
		.AddIngredient(ModContent.ItemType<SteelBar>(), 4)
		.AddIngredient(ItemID.Lens, 2)
		.AddTile(TileID.Anvils)
		.Register();
		}
	}
}
