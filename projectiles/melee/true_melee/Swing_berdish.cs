using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Swing_berdish : ModProjectile {
		private float FlySpeed => 15f;
        private Player Owner => Main.player[Projectile.owner];
        private ref float AttackAngle => ref Projectile.ai[0];
        private ref float AttackTimer => ref Projectile.ai[1];
        private int CloneDirection => Projectile.Center.X < Main.MouseWorld.X ? 1 : -1;
        private const int FRAME_WIDTH = 40;
        private const int FRAME_HEIGHT = 56;
        private const int TOTAL_FRAMES = 20;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            Player player = Owner;
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            Vector2 targetPos = Main.MouseWorld;
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();
            if (distance > 20f)
            {
                direction.Normalize();
                Vector2 targetVelocity = direction * FlySpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.08f);
                if (Projectile.velocity.Length() > FlySpeed)
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= FlySpeed;
                }
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
            AttackTimer++;
            float progress = (AttackTimer % 30f) / 30f;
            AttackAngle = MathHelper.Lerp(-MathHelper.Pi * 0.75f, MathHelper.PiOver4, (float)Math.Sin(progress * MathHelper.Pi));
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, -Projectile.velocity.X * 0.3f, -Projectile.velocity.Y * 0.3f, 150, default, 1.2f);
                dust.noGravity = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float swordLength = 70f;
            Vector2 swordBase = Projectile.Center;
            float angle = AttackAngle + (CloneDirection == -1 ? MathHelper.Pi : 0);
            Vector2 swordTip = swordBase + angle.ToRotationVector2() * swordLength * CloneDirection;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), swordBase, swordTip, 24f, ref collisionPoint);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.8f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.Shadowflame, dustVel.X, dustVel.Y, 100, default, 1.8f);
                dust.noGravity = true;
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawShadowClone();
            return false;
        }
        private void DrawShadowClone()
        {
            Player player = Owner;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            int direction = CloneDirection;
            SpriteEffects spriteEffects = direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color shadowColor = new Color(100, 50, 150);
            float alpha = 0.75f;
            for (int trail = 4; trail >= 1; trail--)
            {
                float trailAlpha = (5 - trail) / 12f;
                Vector2 trailOffset = -Projectile.velocity * trail * 0.4f;
                DrawFullPlayer(spriteBatch, player, drawPosition + trailOffset, spriteEffects, shadowColor, trailAlpha);
            }
            DrawFullPlayer(spriteBatch, player, drawPosition, spriteEffects, shadowColor, alpha);
            DrawGlowingEyes(spriteBatch, drawPosition, spriteEffects);
            DrawSword(spriteBatch, drawPosition, direction);
        }
        private Rectangle GetSafeFrame(int frameY)
        {
            int frameIndex = frameY / FRAME_HEIGHT;
            frameIndex = Math.Clamp(frameIndex, 0, TOTAL_FRAMES - 1);
            return new Rectangle(0, frameIndex * FRAME_HEIGHT, FRAME_WIDTH, FRAME_HEIGHT);
        }
        private void DrawFullPlayer(SpriteBatch spriteBatch, Player player, Vector2 position, SpriteEffects effects, Color shadowColor, float alpha)
        {
            int skin = player.skinVariant;
            Rectangle bodyFrame = GetSafeFrame(player.bodyFrame.Y);
            Rectangle legFrame = GetSafeFrame(player.legFrame.Y);
            Vector2 origin = new Vector2(FRAME_WIDTH / 2f, FRAME_HEIGHT - 4);
            Color skinCol = MixShadow(player.skinColor, shadowColor, alpha);
            Color hairCol = MixShadow(player.hairColor, shadowColor, alpha);
            Color eyeCol = MixShadow(player.eyeColor, shadowColor, alpha);
            Color shirtCol = MixShadow(player.shirtColor, shadowColor, alpha);
            Color underShirtCol = MixShadow(player.underShirtColor, shadowColor, alpha);
            Color pantsCol = MixShadow(player.pantsColor, shadowColor, alpha);
            Color shoeCol = MixShadow(player.shoeColor, shadowColor, alpha);
            Color armorCol = MixShadow(Color.White, shadowColor, alpha);
            DrawWings(spriteBatch, player, position, effects, shadowColor, alpha);
            DrawPlayerLayer(spriteBatch, skin, 10, position, legFrame, skinCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 11, position, legFrame, pantsCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 12, position, legFrame, shoeCol, origin, effects);
            DrawLegArmor(spriteBatch, player, position, legFrame, origin, effects, armorCol);
            DrawPlayerLayer(spriteBatch, skin, 7, position, bodyFrame, skinCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 13, position, bodyFrame, underShirtCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 3, position, bodyFrame, skinCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 4, position, bodyFrame, underShirtCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 6, position, bodyFrame, shirtCol, origin, effects);
            DrawBodyArmor(spriteBatch, player, position, bodyFrame, origin, effects, armorCol);
            DrawPlayerLayer(spriteBatch, skin, 0, position, bodyFrame, skinCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 1, position, bodyFrame, Color.White * alpha * 0.7f, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 2, position, bodyFrame, eyeCol, origin, effects);
            DrawHair(spriteBatch, player, position, bodyFrame, origin, effects, hairCol);
            DrawHeadArmor(spriteBatch, player, position, bodyFrame, origin, effects, armorCol);
            DrawPlayerLayer(spriteBatch, skin, 8, position, bodyFrame, skinCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 9, position, bodyFrame, skinCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 5, position, bodyFrame, skinCol, origin, effects);
            DrawPlayerLayer(spriteBatch, skin, 14, position, bodyFrame, shirtCol, origin, effects);
            DrawArmArmor(spriteBatch, player, position, bodyFrame, origin, effects, armorCol);
            DrawAccessories(spriteBatch, player, position, bodyFrame, legFrame, origin, effects, armorCol);
        }
        private Texture2D GetTextureSafe(ReLogic.Content.Asset<Texture2D> asset)
        {
            if (asset == null)
                return null;
            try
            {
                return asset.Value;
            }
            catch
            {
                return null;
            }
        }
        private void DrawPlayerLayer(SpriteBatch spriteBatch, int skinVariant, int layerIndex, Vector2 position, Rectangle frame, Color color, Vector2 origin, SpriteEffects effects)
        {
            try
            {
                if (skinVariant < 0 || layerIndex < 0)
                    return;
                if (skinVariant >= TextureAssets.Players.GetLength(0))
                    return;
                if (layerIndex >= TextureAssets.Players.GetLength(1))
                    return;
                var asset = TextureAssets.Players[skinVariant, layerIndex];
                Texture2D texture = GetTextureSafe(asset);
                if (texture == null)
                    return;
                Rectangle safeFrame = frame;
                if (safeFrame.Y + safeFrame.Height > texture.Height)
                {
                    int maxFrameIndex = (texture.Height / FRAME_HEIGHT) - 1;
                    safeFrame.Y = Math.Max(0, maxFrameIndex) * FRAME_HEIGHT;
                }
                spriteBatch.Draw(texture, position, safeFrame, color, 0f, origin, 1f, effects, 0f);
            }
            catch
            {
            }
        }
        private void DrawHair(SpriteBatch spriteBatch, Player player, Vector2 position, Rectangle bodyFrame, Vector2 origin, SpriteEffects effects, Color hairCol)
        {
            try
            {
                int hairID = player.hair;
                if (hairID < 0 || hairID >= TextureAssets.PlayerHair.Length)
                    return;
                var asset = TextureAssets.PlayerHair[hairID];
                Texture2D texture = GetTextureSafe(asset);
                if (texture == null)
                    return;
                Rectangle safeFrame = bodyFrame;
                if (safeFrame.Y + safeFrame.Height > texture.Height)
                {
                    safeFrame.Y = 0;
                }
                spriteBatch.Draw(texture, position, safeFrame, hairCol, 0f, origin, 1f, effects, 0f);
            }
            catch
            {
            }
        }
        private void DrawHeadArmor(SpriteBatch spriteBatch, Player player, Vector2 position, Rectangle bodyFrame, Vector2 origin, SpriteEffects effects, Color armorCol)
        {
            try
            {
                if (player.head <= 0 || player.head >= TextureAssets.ArmorHead.Length)
                    return;
                Main.instance.LoadArmorHead(player.head);
                var asset = TextureAssets.ArmorHead[player.head];
                Texture2D texture = GetTextureSafe(asset);
                if (texture == null)
                    return;
                Rectangle safeFrame = bodyFrame;
                if (safeFrame.Y + safeFrame.Height > texture.Height)
                {
                    safeFrame.Y = 0;
                }
                spriteBatch.Draw(texture, position, safeFrame, armorCol, 0f, origin, 1f, effects, 0f);
            }
            catch
            {
            }
        }
        private void DrawBodyArmor(SpriteBatch spriteBatch, Player player, Vector2 position, Rectangle bodyFrame, Vector2 origin, SpriteEffects effects, Color armorCol)
        {
            try
            {
                if (player.body <= 0 || player.body >= TextureAssets.ArmorBodyComposite.Length)
                    return;
                Main.instance.LoadArmorBody(player.body);
                var asset = TextureAssets.ArmorBodyComposite[player.body];
                Texture2D texture = GetTextureSafe(asset);
                if (texture == null)
                    return;
                Rectangle safeFrame = bodyFrame;
                if (safeFrame.Y + safeFrame.Height > texture.Height)
                {
                    safeFrame.Y = 0;
                }
                spriteBatch.Draw(texture, position, safeFrame, armorCol, 0f, origin, 1f, effects, 0f);
            }
            catch
            {
            }
        }
        private void DrawLegArmor(SpriteBatch spriteBatch, Player player, Vector2 position, Rectangle legFrame, Vector2 origin, SpriteEffects effects, Color armorCol)
        {
            try
            {
                if (player.legs <= 0 || player.legs >= TextureAssets.ArmorLeg.Length)
                    return;
                Main.instance.LoadArmorLegs(player.legs);
                var asset = TextureAssets.ArmorLeg[player.legs];
                Texture2D texture = GetTextureSafe(asset);
                if (texture == null)
                    return;
                Rectangle safeFrame = legFrame;
                if (safeFrame.Y + safeFrame.Height > texture.Height)
                {
                    safeFrame.Y = 0;
                }
                spriteBatch.Draw(texture, position, safeFrame, armorCol, 0f, origin, 1f, effects, 0f);
            }
            catch
            {
            }
        }
        private void DrawArmArmor(SpriteBatch spriteBatch, Player player, Vector2 position, Rectangle bodyFrame, Vector2 origin, SpriteEffects effects, Color armorCol)
        {
            try
            {
                if (player.body <= 0 || player.body >= TextureAssets.ArmorArm.Length)
                    return;
                Main.instance.LoadArmorBody(player.body);
                var asset = TextureAssets.ArmorArm[player.body];
                Texture2D texture = GetTextureSafe(asset);
                if (texture == null)
                    return;
                Rectangle safeFrame = bodyFrame;
                if (safeFrame.Y + safeFrame.Height > texture.Height)
                {
                    safeFrame.Y = 0;
                }
                spriteBatch.Draw(texture, position, safeFrame, armorCol, 0f, origin, 1f, effects, 0f);
            }
            catch
            {
            }
        }
        private void DrawAccessories(SpriteBatch spriteBatch, Player player, Vector2 position, Rectangle bodyFrame, Rectangle legFrame, Vector2 origin, SpriteEffects effects, Color armorCol)
        {
            for (int i = 3; i < player.armor.Length && i < 10; i++)
            {
                Item acc = player.armor[i];
                if (acc == null || acc.IsAir)
                    continue;
                try
                {
                    if (acc.backSlot > 0 && acc.backSlot < TextureAssets.AccBack.Length)
                    {
                        Main.instance.LoadAccBack(acc.backSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccBack[acc.backSlot], position, bodyFrame, armorCol, origin, effects);
                    }
                    if (acc.frontSlot > 0 && acc.frontSlot < TextureAssets.AccFront.Length)
                    {
                        Main.instance.LoadAccFront(acc.frontSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccFront[acc.frontSlot], position, bodyFrame, armorCol, origin, effects);
                    }
                    if (acc.neckSlot > 0 && acc.neckSlot < TextureAssets.AccNeck.Length)
                    {
                        Main.instance.LoadAccNeck(acc.neckSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccNeck[acc.neckSlot], position, bodyFrame, armorCol, origin, effects);
                    }
                    if (acc.faceSlot > 0 && acc.faceSlot < TextureAssets.AccFace.Length)
                    {
                        Main.instance.LoadAccFace(acc.faceSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccFace[acc.faceSlot], position, bodyFrame, armorCol, origin, effects);
                    }
                    if (acc.shieldSlot > 0 && acc.shieldSlot < TextureAssets.AccShield.Length)
                    {
                        Main.instance.LoadAccShield(acc.shieldSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccShield[acc.shieldSlot], position, bodyFrame, armorCol, origin, effects);
                    }
                    if (acc.shoeSlot > 0 && acc.shoeSlot < TextureAssets.AccShoes.Length)
                    {
                        Main.instance.LoadAccShoes(acc.shoeSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccShoes[acc.shoeSlot], position, legFrame, armorCol, origin, effects);
                    }
                    if (acc.waistSlot > 0 && acc.waistSlot < TextureAssets.AccWaist.Length)
                    {
                        Main.instance.LoadAccWaist(acc.waistSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccWaist[acc.waistSlot], position, bodyFrame, armorCol, origin, effects);
                    }
                    if (acc.handOnSlot > 0 && acc.handOnSlot < TextureAssets.AccHandsOn.Length)
                    {
                        Main.instance.LoadAccHandsOn(acc.handOnSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccHandsOn[acc.handOnSlot], position, bodyFrame, armorCol, origin, effects);
                    }
                    if (acc.handOffSlot > 0 && acc.handOffSlot < TextureAssets.AccHandsOff.Length)
                    {
                        Main.instance.LoadAccHandsOff(acc.handOffSlot);
                        DrawAccessoryTexture(spriteBatch, TextureAssets.AccHandsOff[acc.handOffSlot], position, bodyFrame, armorCol, origin, effects);
                    }
                }
                catch
                {
                }
            }
        }
        private void DrawAccessoryTexture(SpriteBatch spriteBatch, ReLogic.Content.Asset<Texture2D> asset, Vector2 position, Rectangle frame, Color color, Vector2 origin, SpriteEffects effects)
        {
            Texture2D texture = GetTextureSafe(asset);
            if (texture == null)
                return;
            Rectangle safeFrame = frame;
            if (safeFrame.Y + safeFrame.Height > texture.Height)
            {
                safeFrame.Y = 0;
            }
            spriteBatch.Draw(texture, position, safeFrame, color, 0f, origin, 1f, effects, 0f);
        }
        private void DrawWings(SpriteBatch spriteBatch, Player player, Vector2 position, SpriteEffects effects, Color shadowColor, float alpha)
		{
			try
			{
				if (player.wings <= 0 || player.wings >= TextureAssets.Wings.Length)
					return;
				Main.instance.LoadWings(player.wings);
				var asset = TextureAssets.Wings[player.wings];
				Texture2D wingsTex = GetTextureSafe(asset);
				if (wingsTex == null)
					return;
				int textureHeight = wingsTex.Height;
				int textureWidth = wingsTex.Width;
				int frameHeight;
				int totalFrames;
				if (textureHeight % 152 == 0)
				{
					frameHeight = 152;
					totalFrames = textureHeight / 152;
				}
				else if (textureHeight % 140 == 0)
				{
					frameHeight = 140;
					totalFrames = textureHeight / 140;
				}
				else if (textureHeight % 160 == 0)
				{
					frameHeight = 160;
					totalFrames = textureHeight / 160;
				}
				else
				{
					totalFrames = 4;
					frameHeight = textureHeight / totalFrames;
					if (frameHeight < 50)
					{
						totalFrames = 1;
						frameHeight = textureHeight;
					}
				}
				int currentFrame = player.wingFrame;
				if (currentFrame < 0)
					currentFrame = 0;
				if (currentFrame >= totalFrames)
					currentFrame = totalFrames - 1;
				int frameY = currentFrame * frameHeight;
				if (frameY < 0)
					frameY = 0;
				if (frameY + frameHeight > textureHeight)
				{
					frameY = Math.Max(0, textureHeight - frameHeight);
					if (frameY < 0)
						frameY = 0;
					if (frameHeight > textureHeight)
						frameHeight = textureHeight;
				}
				Rectangle wingsRect = new Rectangle(0, frameY, textureWidth, frameHeight);
				Vector2 wingsOrigin = new Vector2(textureWidth / 2f, frameHeight / 2f);
				float xOffset = effects == SpriteEffects.FlipHorizontally ? 5f : -5f;
				Vector2 wingsOffset = new Vector2(xOffset, -12);
				Color wingsColor = MixShadow(Color.White, shadowColor, alpha) * 0.85f;
				spriteBatch.Draw(wingsTex, position + wingsOffset, wingsRect, wingsColor, 0f, wingsOrigin, 1f, effects, 0f);
			}
			catch
			{
			}
		}
        private Color MixShadow(Color original, Color shadow, float alpha)
        {
            Color mixed = Color.Lerp(original, shadow, 0.6f);
            return mixed * alpha;
        }
        private void DrawGlowingEyes(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effects)
        {
            try
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                float eyeX = effects == SpriteEffects.FlipHorizontally ? 3f : -3f;
                Vector2 leftEye = position + new Vector2(eyeX - 2, -18);
                Vector2 rightEye = position + new Vector2(eyeX + 2, -18);
                float pulse = 0.8f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6) * 0.2f;
                Color glowColor = new Color(200, 80, 255) * pulse;
                for (int i = 4; i >= 1; i--)
                {
                    float glowSize = 2 + i * 2;
                    float glowAlpha = (5 - i) / 10f;
                    Color glow = glowColor * glowAlpha;
                    spriteBatch.Draw(pixel, leftEye, new Rectangle(0, 0, 1, 1), glow, 0f, new Vector2(0.5f), glowSize, SpriteEffects.None, 0f);
                    spriteBatch.Draw(pixel, rightEye, new Rectangle(0, 0, 1, 1), glow, 0f, new Vector2(0.5f), glowSize, SpriteEffects.None, 0f);
                }
                spriteBatch.Draw(pixel, leftEye, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f), 2f, SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, rightEye, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f), 2f, SpriteEffects.None, 0f);
            }
            catch
            {
            }
        }
        private void DrawSword(SpriteBatch spriteBatch, Vector2 position, int direction)
        {
            try
            {
                Player player = Owner;
                Item heldItem = player.HeldItem;
                if (heldItem == null || heldItem.IsAir)
                    return;
                Main.instance.LoadItem(heldItem.type);
                var itemAsset = TextureAssets.Item[heldItem.type];
                Texture2D swordTex = GetTextureSafe(itemAsset);
                if (swordTex == null)
                    return;
                float swordAngle = AttackAngle;
                if (direction == -1)
                {
                    swordAngle = MathHelper.Pi - swordAngle;
                }
                Color swordColor = new Color(150, 80, 200, 180);
                Vector2 swordOrigin = direction == 1
                    ? new Vector2(0, swordTex.Height)
                    : new Vector2(swordTex.Width, swordTex.Height);
                SpriteEffects swordEffects = direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Vector2 swordPos = position + new Vector2(direction * 6, -10);
                for (int i = 6; i >= 1; i--)
                {
                    float trailAngle = swordAngle - direction * i * 0.1f;
                    float trailAlpha = (7 - i) / 14f;
                    Color trailColor = swordColor * trailAlpha;
                    spriteBatch.Draw(swordTex, swordPos, null, trailColor, trailAngle, swordOrigin, heldItem.scale, swordEffects, 0f);
                }
                spriteBatch.Draw(swordTex, swordPos, null, swordColor, swordAngle, swordOrigin, heldItem.scale, swordEffects, 0f);
                Color glowColor = new Color(180, 100, 255, 30);
                spriteBatch.Draw(swordTex, swordPos, null, glowColor, swordAngle, swordOrigin, heldItem.scale * 1.1f, swordEffects, 0f);
            }
            catch
            {
            }
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Shadowflame, dustVel.X, dustVel.Y, 100, default, 1.5f);
                dust.noGravity = true;
            }
        }
	}
}
