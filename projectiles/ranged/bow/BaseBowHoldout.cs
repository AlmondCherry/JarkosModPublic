using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
namespace jarkos.projectiles.ranged.bow
{
    public abstract class BaseBowHoldout : ModProjectile
    {
        public virtual bool IsAnimated => false;
        public virtual int FrameCount => 1;
        public virtual int AnimSpeed => 5;
        public virtual int SpriteWidth => -1;
        public virtual int SpriteHeight => -1;
        public virtual int ShootDelay => 20;
        public virtual float MuzzleOffset => 25f;
        public virtual float AimResponsiveness => 0.89f;
        public virtual float HoldoutRotationOffset => (float)Math.PI / 2f;
        public virtual bool FlipVerticallyWhenLeft => true;
        public float ShootTimer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public sealed override void SetStaticDefaults()
        {
            if (IsAnimated)
            {
                Main.projFrames[Projectile.type] = FrameCount;
            }
            SafeSetStaticDefaults();
        }
        public virtual void SafeSetStaticDefaults() { }
        public sealed override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            SafeSetDefaults();
        }
        public virtual void SafeSetDefaults() { }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter);
            ShootTimer++;
            if (IsAnimated)
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= AnimSpeed)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= FrameCount)
                    {
                        Projectile.frame = 0;
                    }
                }
            }
            UpdatePlayerVisuals(player, rrp);
            if (Projectile.owner == Main.myPlayer)
            {
                float speedTimesScale = 10f;
                UpdateAim(rrp, speedTimesScale);
                bool stillInUse = player.channel && !player.noItems && !player.CCed;
                if (stillInUse && ShootTimer >= ShootDelay)
                {
                    ShootTimer = 0;
                    Vector2 shootVel = Vector2.Normalize(Projectile.velocity);
                    if (shootVel.HasNaNs())
                        shootVel = -Vector2.UnitY;
                    Vector2 muzzlePos = Projectile.Center + shootVel * MuzzleOffset;
                    SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
                    OnFire(player, muzzlePos, shootVel * player.inventory[player.selectedItem].shootSpeed, Projectile.damage, Projectile.knockBack);
                    Projectile.netUpdate = true;
                }
                else if (!stillInUse && ShootTimer > ShootDelay)
                {
                    Projectile.Kill();
                }
            }
            Projectile.timeLeft = 2;
        }
        public virtual void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, int damage, float knockback)
        {
        }
        private void UpdateAim(Vector2 source, float speed)
        {
            Vector2 aimVector = Vector2.Normalize(Main.MouseWorld - source);
            if (aimVector.HasNaNs())
            {
                aimVector = -Vector2.UnitY;
            }
            aimVector = Vector2.Normalize(Vector2.Lerp(aimVector, Vector2.Normalize(Projectile.velocity), AimResponsiveness));
            aimVector *= speed;
            if (aimVector != Projectile.velocity)
            {
                Projectile.netUpdate = true;
            }
            Projectile.velocity = aimVector;
        }
        private void UpdatePlayerVisuals(Player player, Vector2 rrp)
        {
            Projectile.Center = rrp;
            Projectile.rotation = Projectile.velocity.ToRotation() + HoldoutRotationOffset;
            Projectile.spriteDirection = Projectile.direction;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects eff = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
            {
                eff = FlipVerticallyWhenLeft ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally;
            }
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            int frameW = SpriteWidth > 0 ? SpriteWidth : tex.Width;
            int frameH = SpriteHeight > 0 ? SpriteHeight : tex.Height / (IsAnimated ? FrameCount : 1);
            int texYOffset = frameH * Projectile.frame;
            Vector2 sheetInsertVec = (Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();
            Main.EntitySpriteDraw(tex, sheetInsertVec, new Rectangle(0, texYOffset, frameW, frameH), lightColor, Projectile.rotation, new Vector2(frameW / 2f, frameH / 2f), Projectile.scale, eff, 0);
            return false;
        }
    }
}
