using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
    public abstract class BaseRangedHoldout : ModProjectile
    {
        protected Player Owner => Main.player[Projectile.owner];
        protected int shootTimer = 0;
        protected float recoil = 0f;
        protected bool firstShot = true;
        public virtual Vector2 HoldoutOrigin => new Vector2(16f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public virtual float RecoilDecay => 0.8f;
        public virtual float BaseRecoil => 8f;
        public virtual float MuzzleForwardOffset => 40f;
        public virtual float MuzzleVerticalOffset => -4f;
        public virtual void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot) { }
        public virtual bool KeepAliveCondition(Player player) => player.channel;
        public virtual int FirstShotDelay => 0;
        public virtual float CustomShootSpeed => -1f;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.hide = true;
            SafeSetDefaults();
            shootTimer = -FirstShotDelay;
        }
        public virtual void SafeSetDefaults() {}
        private float initialSpeed = -1f;
        public override void AI()
        {
            if (initialSpeed < 0f)
            {
                initialSpeed = Projectile.velocity.Length();
            }
            Player player = Owner;
            if (player.dead || !player.active || !KeepAliveCondition(player) || player.noItems || player.CCed)
            {
                OnKillCondition(player);
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 2;
            Vector2 mountedCenter = player.RotatedRelativePoint(player.MountedCenter);
            Projectile.Center = mountedCenter;
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 aimDirection = GetAimDirection(player, mountedCenter);
                Projectile.velocity = aimDirection;
                Projectile.direction = aimDirection.X > 0 ? 1 : -1;
                Projectile.netUpdate = true;
            }
            Projectile.spriteDirection = Projectile.direction;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            int useTime = GetUseTime(player);
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            float armRotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
            if (shootTimer == 0 && ShouldFire(player))
            {
                float speed = player.HeldItem.shootSpeed;
                int damage = player.HeldItem.damage;
                float knockback = player.HeldItem.knockBack;
                int projToShoot = (int)Projectile.ai[0];
                if (firstShot)
                {
                    speed = player.HeldItem.shootSpeed;
                    if (player.HeldItem.useAmmo != AmmoID.None && initialSpeed > 0f)
                    {
                        speed = initialSpeed;
                    }
                    damage = player.GetWeaponDamage(player.HeldItem);
                    knockback = player.GetWeaponKnockback(player.HeldItem, player.HeldItem.knockBack);
                }
                else if (ConsumeAmmo())
                {
                    bool canShoot = player.PickAmmo(player.HeldItem, out projToShoot, out speed, out damage, out knockback, out int _);
                    if (!canShoot)
                    {
                        Projectile.Kill();
                        return;
                    }
                    damage = player.GetWeaponDamage(player.HeldItem);
                }
                firstShot = false;
                if (CustomShootSpeed > 0f)
                {
                    speed = CustomShootSpeed;
                }
                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                    recoil = BaseRecoil;
                    OnFire(player, muzzlePos, Projectile.velocity, speed, damage, knockback, projToShoot);
                }
            }
            shootTimer++;
            if (shootTimer >= useTime)
            {
                shootTimer = 0;
            }
            recoil *= RecoilDecay;
            UpdateExtra(player);
        }
        public virtual Vector2 GetAimDirection(Player player, Vector2 mountedCenter)
        {
            return (Main.MouseWorld - mountedCenter).SafeNormalize(Vector2.UnitX);
        }
        public virtual int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 25 : useTime;
        }
        public virtual bool ShouldFire(Player player) => true;
        public virtual bool ConsumeAmmo() => true;
        public virtual void OnKillCondition(Player player) {}
        public virtual void UpdateExtra(Player player) {}
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float currentRecoil = recoil;
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil;
            float rotation = Projectile.velocity.ToRotation();
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
            PostDrawHoldout(drawPos, rotation, spriteEffects);
            return false;
        }
        public virtual void PostDrawHoldout(Vector2 drawPos, float rotation, SpriteEffects spriteEffects) {}
    }
}
