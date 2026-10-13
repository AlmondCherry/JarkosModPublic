using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using jarkos.projectiles.bobbers;
using Terraria.Audio;

namespace jarkos.items.tools
{
	public class Mechanical_fishing_rod : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 25;
            Item.shootSpeed = 12f;
            Item.shoot = ModContent.ProjectileType<Mechanical_bobber_proj>();
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
            Item.autoReuse = false;
        }
        public override void HoldItem(Player player)
        {
            if (player.TryGetModPlayer<MechanicalFishingPlayer>(out var modPlayer))
            {
                modPlayer.holdingMechanicalPole = true;
            }
        }
	}
	public class MechanicalFishingPlayer : ModPlayer
    {
        public bool holdingMechanicalPole = false;
        private int catchTimer = 0;
        private int recastTimer = 0;
        private bool waitingToRecast = false;
        private const int CATCH_DELAY = 45;
        private const int RECAST_DELAY = 90;
        public override void ResetEffects()
        {
            holdingMechanicalPole = false;
        }
        public override void PreUpdate()
        {
            if (!holdingMechanicalPole)
            {
                ResetTimers();
                return;
            }
            if (Player.whoAmI != Main.myPlayer)
                return;
            Projectile bobber = FindMechanicalBobber();
            if (bobber != null)
            {
                waitingToRecast = false;
                recastTimer = 0;
                if (bobber.ai[0] < 0f)
                {
                    catchTimer++;
                    if (catchTimer >= CATCH_DELAY)
                    {
                        bobber.ai[0] = 1f;
                        bobber.ai[1] = 0f;
                        bobber.FishingCheck();
                        bobber.Kill();
                        SoundEngine.PlaySound(SoundID.Item37, bobber.Center);
                        waitingToRecast = true;
                        recastTimer = RECAST_DELAY;
                        catchTimer = 0;
                    }
                }
                else
                {
                    catchTimer = 0;
                }
            }
            else
            {
                catchTimer = 0;
                if (waitingToRecast)
                {
                    recastTimer--;
                    if (recastTimer <= 0)
                    {
                        waitingToRecast = false;
                        AutoCast();
                    }
                }
            }
        }
        private void AutoCast()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;
            if (!holdingMechanicalPole)
                return;
            if (FindMechanicalBobber() != null)
                return;

            Vector2 velocity = Main.MouseWorld - Player.Center;
            velocity.Normalize();
            velocity *= Player.HeldItem.shootSpeed;
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));

            Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, velocity, ModContent.ProjectileType<Mechanical_bobber_proj>(), 0, 0f, Player.whoAmI);
            
            Player.itemAnimation = Player.HeldItem.useAnimation;
            Player.itemTime = Player.HeldItem.useTime;
            
            SoundEngine.PlaySound(SoundID.Item1, Player.Center);
        }
        private Projectile FindMechanicalBobber()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<Mechanical_bobber_proj>())
                {
                    return proj;
                }
            }
            return null;
        }
        private void ResetTimers()
        {
            catchTimer = 0;
            recastTimer = 0;
            waitingToRecast = false;
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            ResetTimers();
        }
    }
}
