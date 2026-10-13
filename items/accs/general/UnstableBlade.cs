using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.bullet;

namespace jarkos.items.accs.general
{
	public class UnstableBlade : ModItem
    {
		public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 50);
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<LevitatingBladePlayer>().hasLevitatingBlade = true;
        }
	}
	public class LevitatingBladePlayer : ModPlayer
    {
        public bool hasLevitatingBlade = false;
        private int bladeAttackCooldown = 0;
        private const int ATTACK_COOLDOWN = 40;
        public override void ResetEffects()
        {
            hasLevitatingBlade = false;
        }
        public override void PostUpdate()
        {
            if (hasLevitatingBlade)
            {
                if (!HasActiveBlade())
                {
                    Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[3]), Player.Center, Vector2.Zero, ModContent.ProjectileType<LevitatingBlade_proj>(), GetBladeDamage(), 2f, Player.whoAmI);
                }
                if (bladeAttackCooldown > 0)
                    bladeAttackCooldown--;
            }
        }
        private bool HasActiveBlade()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<LevitatingBlade_proj>())
                {
                    return true;
                }
            }
            return false;
        }
        private int GetBladeDamage()
        {
            return (int)(30 * Player.GetDamage(DamageClass.Summon).Multiplicative);
        }
        public bool CanAttack()
        {
            return bladeAttackCooldown <= 0;
        }
        public void StartAttackCooldown()
        {
            bladeAttackCooldown = ATTACK_COOLDOWN;
        }
    }
}
