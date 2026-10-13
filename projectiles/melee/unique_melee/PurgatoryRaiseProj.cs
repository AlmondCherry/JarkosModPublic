using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee
{
    public class PurgatoryRaiseProj : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/PurgatorySpear";

        public const int RaiseDuration = 30;
        public const int HoldDuration = 120;
        public const int LowerDuration = 30;
        public const int TotalDuration = RaiseDuration + HoldDuration + LowerDuration;

        private float currentAlpha = 1f;

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TotalDuration;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }


            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            int elapsed = TotalDuration - Projectile.timeLeft;
            float rotation;

            if (elapsed < RaiseDuration)
            {

                float t = elapsed / (float)RaiseDuration;
                float easeOutT = 1f - (1f - t) * (1f - t); 

                float startAngle = (player.direction > 0) ? 0.2f : MathHelper.Pi - 0.2f;
                float targetAngle = -MathHelper.PiOver2;

                rotation = MathHelper.Lerp(startAngle, targetAngle, easeOutT);
                currentAlpha = 1f;
            }
            else if (elapsed < RaiseDuration + HoldDuration)
            {

                rotation = -MathHelper.PiOver2;
                currentAlpha = 1f;
            }
            else
            {

                float t = (elapsed - (RaiseDuration + HoldDuration)) / (float)LowerDuration;
                float easeInT = t * t;

                float startAngle = -MathHelper.PiOver2;
                float targetAngle = (player.direction > 0) ? 0.6f : MathHelper.Pi - 0.6f;

                rotation = MathHelper.Lerp(startAngle, targetAngle, easeInT);
                currentAlpha = MathHelper.Clamp(1f - t, 0f, 1f);
            }

            Projectile.rotation = rotation;
            Projectile.Center = player.Center;


            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D spearTex = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 armPosition = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            armPosition.Y += player.gfxOffY;

            Vector2 origin;
            float rotationOffset;
            SpriteEffects effects;

            if (player.direction > 0)
            {
                origin = new Vector2(6f, spearTex.Height - 6f);
                rotationOffset = MathHelper.ToRadians(45f);
                effects = SpriteEffects.None;
            }
            else
            {
                origin = new Vector2(spearTex.Width - 6f, spearTex.Height - 6f);
                rotationOffset = MathHelper.ToRadians(135f);
                effects = SpriteEffects.FlipHorizontally;
            }

            Vector2 drawPos = armPosition - Main.screenPosition;
            Color drawColor = lightColor * currentAlpha;

            Main.spriteBatch.Draw(spearTex, drawPos, null, drawColor, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0f);


            if (currentAlpha > 0.1f && Main.rand.NextBool(2))
            {
                Vector2 tipPos = armPosition + Projectile.rotation.ToRotationVector2() * (spearTex.Height * 1.1f);
                Dust d = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(6f, 6f), DustID.GoldFlame, new Vector2(0, -Main.rand.NextFloat(1f, 3f)), 100, default, Main.rand.NextFloat(0.9f, 1.4f) * currentAlpha);
                d.noGravity = true;
            }

            return false;
        }
    }
}
