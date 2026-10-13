using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Linq;
using jarkos.projectiles.bobbers;
using System.Collections.Generic;
using jarkos.rare;
using Terraria.Localization;

namespace jarkos.items.tools
{
	public class Antique_fishing_rod : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 45;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<Antique_bobber_proj>();
            Item.rare = ModContent.RarityType<SpeedRarity>();
            Item.value = Item.sellPrice(gold: 2, silver: 50);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int bobberCount = 5;
            float spreadAngle = 15f;

            for (int i = 0; i < bobberCount; i++)
            {
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(spreadAngle));
                newVelocity *= 1f - Main.rand.NextFloat(0.1f);
                Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<AntiqueFishingPlayer>();

            if (modPlayer.antiqueBobberCount > 0)
            {
                int bonus = modPlayer.antiqueBobberCount * 3;
                TooltipLine line = new TooltipLine(Mod, "ActiveBonus", Language.GetTextValue("Mods.jarkos.Items.Antique_fishing_rod.Tooltip", modPlayer.antiqueBobberCount, bonus));
                tooltips.Add(line);
            }
        }
	}
	public class AntiqueFishingPlayer : ModPlayer
    {
        public int antiqueBobberCount = 0;
        public override void ResetEffects()
        {
            antiqueBobberCount = 0;
        }
        public override void PreUpdate()
        {
            antiqueBobberCount = Main.projectile
                .Where(p => p.active && p.owner == Player.whoAmI && p.type == ModContent.ProjectileType<Antique_bobber_proj>() && p.wet)
                .Count();
        }
        public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel)
        {
            if (antiqueBobberCount > 0)
            {
                fishingLevel += antiqueBobberCount * 3;
            }
        }
        public override void ModifyCaughtFish(Item fish)
        {
            if (antiqueBobberCount >= 5 && Main.rand.NextBool(5))
            {
                fish.stack *= 2;
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.GoldCoin, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
                }
            }
        }
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            if (antiqueBobberCount >= 5)
            {
                if (Main.rand.NextBool(10) && attempt.common)
                {
                    if (attempt.inLava)
                        itemDrop = ItemID.LavaCrate;
                    else if (attempt.inHoney)
                        itemDrop = ItemID.IronCrate;
                    else if (attempt.heightLevel == 0)
                        itemDrop = ItemID.FloatingIslandFishingCrate;
                    else
                        itemDrop = ItemID.WoodenCrate;
                }
            }
        }
    }
}
