using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
    public class HoldoutDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            return player.heldProj >= 0 && Main.projectile[player.heldProj].active && Main.projectile[player.heldProj].ModProjectile is Piercer_Holdout;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            Projectile proj = Main.projectile[player.heldProj];
            Piercer_Holdout holdout = proj.ModProjectile as Piercer_Holdout;
            if (holdout == null) return;
            SpriteEffects spriteEffects = proj.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(holdout.Texture).Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("jarkos/items/weapons/ranged/firing/piercer_light", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            float currentRecoil = holdout.CurrentRecoil;
            Vector2 shakeOffset = Vector2.Zero;
            if (currentRecoil > 1f)
            {
                shakeOffset = Main.rand.NextVector2Circular(currentRecoil * 0.4f, currentRecoil * 0.4f);
            }
            Vector2 drawPos = proj.Center - Main.screenPosition - proj.velocity * currentRecoil + shakeOffset;
            float baseRotation = proj.velocity.ToRotation();
            float rotation = baseRotation;
            if (holdout.IsAltFire && holdout.ShootTimer < Piercer_Holdout.SpinDuration)
            {
                float progress = (float)holdout.ShootTimer / Piercer_Holdout.SpinDuration;
                float easeOut = 1f - (float)Math.Pow(1f - progress, 3);
                float extraRotation = easeOut * MathHelper.TwoPi * -1f;
                rotation += extraRotation * proj.direction;
                for (int i = 0; i < 6; i++)
                {
                    Vector2 blurPos = drawPos + Main.rand.NextVector2Circular(4f, 4f);
                    DrawData blurData = new DrawData(texture, blurPos, null, Color.LightCyan * 0.4f * (1f - progress), rotation - (i * 0.15f * proj.direction), holdout.HoldoutOrigin, proj.scale, spriteEffects, 0);
                    drawInfo.DrawDataCache.Add(blurData);
                }
            }
            float outlinePulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.2f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.PiOver2 * i).ToRotationVector2() * 2f * outlinePulse;
                DrawData outlineData = new DrawData(texture2, drawPos + offset, null, Color.White * 0.5f, rotation, holdout.HoldoutOrigin, proj.scale, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(outlineData);
            }
            DrawData mainData = new DrawData(texture, drawPos, null, drawInfo.colorArmorBody, rotation, holdout.HoldoutOrigin, proj.scale, spriteEffects, 0);
            drawInfo.DrawDataCache.Add(mainData);
        }
    }
}
