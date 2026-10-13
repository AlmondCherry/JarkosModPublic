using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameInput;
using jarkos.projectiles.melee.unique_melee;

namespace jarkos.items.weapons.melee.unique_melee
{
    public class ImpactWave_sword : ModItem
    {
        private int attackMode = 0;
        private int swingCounter = 0;
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 2953;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;

            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<jarkos.rare.CosmicRarity>();
            
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ImpactWave_sword_proj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;
        }
        public override void HoldItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return;
            int delta = PlayerInput.MouseInfo.ScrollWheelValue - PlayerInput.MouseInfoOld.ScrollWheelValue;
            if (delta > 0)
                attackMode = 1;
            else if (delta < 0)
                attackMode = 0;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            bool isMegaSwing = swingCounter % 5 == 0;
            float direction = swingCounter % 2 == 0 ? 1f : -1f;
            float variant = attackMode + (isMegaSwing ? 10f : 0f);
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI, ai0: variant, ai1: direction);
            if (isMegaSwing)
            {
                SoundEngine.PlaySound(SoundID.Item105, player.Center);
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust d = Dust.NewDustDirect(player.Center, 0, 0, DustID.BlueFairy, dustVel.X, dustVel.Y, 150, default, 1.8f);
                    d.noGravity = true;
                }
            }
            return false;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 pos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.BlueFairy, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
    }
}
