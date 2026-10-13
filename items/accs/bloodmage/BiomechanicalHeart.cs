using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.bullet;

namespace jarkos.items.accs.bloodmage
{
	public class BiomechanicalHeart : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PulsatingHeartPlayer>().hasHeart = true;
        }
    }
    public class PulsatingHeartPlayer : ModPlayer
    {
        public bool hasHeart;
        private int heartProjectileIndex = -1;
        public override void ResetEffects()
        {
            hasHeart = false;
        }
        public override void PostUpdateEquips()
        {
            float healthRatio = (float)Player.statLife / Player.statLifeMax2;
            bool shouldExist = hasHeart && healthRatio < 0.75f && !Player.dead;

            bool heartAlive = heartProjectileIndex >= 0
                && heartProjectileIndex < Main.maxProjectiles
                && Main.projectile[heartProjectileIndex].active
                && Main.projectile[heartProjectileIndex].type == ModContent.ProjectileType<Biomechanical_heart_proj>()
                && Main.projectile[heartProjectileIndex].owner == Player.whoAmI;

            if (shouldExist && !heartAlive)
            {
                int idx = Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[0]), Player.Center, Vector2.Zero, ModContent.ProjectileType<Biomechanical_heart_proj>(), 0, 0f, Player.whoAmI);
                heartProjectileIndex = idx;
            }
            else if (!shouldExist && heartAlive)
            {
                var proj = Main.projectile[heartProjectileIndex];
                if (proj.ModProjectile is Biomechanical_heart_proj heart)
                {
                    heart.StartDespawn();
                }
            }
            if (heartAlive)
            {
                var proj = Main.projectile[heartProjectileIndex];
                
                if (proj.ModProjectile is Biomechanical_heart_proj heart)
                {
                    float power = 1f - (healthRatio / 0.75f);
                    if (power < 0f) power = 0f;
                    if (power > 1f) power = 1f;

                    heart.powerFactor = power;
                }
            }
        }
    }
}
