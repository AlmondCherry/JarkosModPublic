using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Shaders;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using jarkos.projectiles.ranged.firearm;
using Jarkos.trails;
using jarkos.particles;
using System.Collections.Generic;
using jarkos.items.weapons.ranged.firing;
namespace jarkos.projectiles.mecha
{
	public class Astra_bomb_charged : ModProjectile
	{
		private const float MAX_BEAM_LENGTH = 3500f;
        private const int CHARGE_TIME = 90;
        private int state = 0;
        private int chargeTimer = 0;
        private float chargeProgress => (float)chargeTimer / CHARGE_TIME;
        private float beamAlpha = 0f;
        private float beamWidth = 0f;
        private Vector2 beamDirection;
        private Vector2 lockedDirection;
        private float beamLength;
        private List<Vector2> hitPositions = new List<Vector2>();
        private float screenShakeIntensity = 0f;
        private float afterglowAlpha = 1f;
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            switch (state)
            {
                case 0:
                    ChargeState(player);
                    break;
                case 1:
                    FireState(player);
                    break;
                case 2:
                    FadeState();
                    break;
            }
        }
        private void UpdatePlayerDirection(Player player, Vector2 direction)
        {
            int dir = direction.X >= 0 ? 1 : -1;
            player.ChangeDir(dir);
            float rotation = direction.ToRotation();
            if (dir == -1)
            {
                rotation += MathHelper.Pi;
            }
            player.itemRotation = rotation;
            player.heldProj = Projectile.whoAmI;
        }
        private void ChargeState(Player player)
        {
            chargeTimer++;
            Projectile.Center = player.Center;
            beamDirection = Vector2.Normalize(Main.MouseWorld - player.Center);
            UpdatePlayerDirection(player, beamDirection);
            player.itemTime = 2;
            player.itemAnimation = 2;
            if (chargeTimer < CHARGE_TIME)
            {
                if (Main.rand.NextBool(3))
                {
                    float radius = 100f * (1f - chargeProgress) + 30f;
                    Vector2 dustPos = player.Center + Main.rand.NextVector2CircularEdge(radius, radius);
                    Vector2 dustVel = (player.Center - dustPos).SafeNormalize(Vector2.Zero) * (5f + chargeProgress * 10f);
                    Dust chargeDust = Dust.NewDustDirect(dustPos, 1, 1, DustID.Electric, dustVel.X, dustVel.Y, 0, new Color(100, 200, 255), 0.8f + chargeProgress);
                    chargeDust.noGravity = true;
                    chargeDust.fadeIn = 1.5f;
                }
                if (chargeTimer % 20 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with
                    {
                        Pitch = -0.8f + chargeProgress * 0.8f, Volume = 0.3f + chargeProgress * 0.5f
                    }, player.Center);
                }
                Lighting.AddLight(player.Center, 0.3f * chargeProgress, 0.6f * chargeProgress, 1f * chargeProgress);
                screenShakeIntensity = chargeProgress * 4f;
                if (chargeProgress > 0.5f && Main.rand.NextBool(4))
                {
                    Vector2 muzzlePos = player.Center + beamDirection * 50f;
                    Dust Spark = Dust.NewDustDirect(muzzlePos, 1, 1, DustID.Electric, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, new Color(150, 220, 255), 1f);
                    Spark.noGravity = true;
                }
            }
            else
            {
                lockedDirection = beamDirection;
                state = 1;
                Fire(player);
            }
        }
        private void Fire(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = -0.3f, Volume = 1.5f }, player.Center);
            SoundEngine.PlaySound(SoundID.Thunder with { Pitch = 0.2f, Volume = 1f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.2f }, player.Center);
            beamLength = MAX_BEAM_LENGTH;
            Vector2 checkPos = player.Center;
            for (float dist = 0; dist < MAX_BEAM_LENGTH; dist += 16f)
            {
                checkPos = player.Center + lockedDirection * dist;
                if (checkPos.X < 0 || checkPos.X > Main.maxTilesX * 16 || checkPos.Y < 0 || checkPos.Y > Main.maxTilesY * 16)
                {
                    beamLength = dist;
                    break;
                }
            }
            DamageEnemiesAlongBeam(player);
            screenShakeIntensity = 25f;
            beamAlpha = 1f;
            beamWidth = 80f;
            for (int i = 0; i < 50; i++)
            {
                Vector2 dustVel = lockedDirection.RotatedByRandom(0.4f) * Main.rand.NextFloat(5f, 25f);
                Dust muzzleFlash = Dust.NewDustDirect(player.Center + lockedDirection * 50f, 1, 1, DustID.Electric, dustVel.X, dustVel.Y, 0, new Color(150, 220, 255), 2f);
                muzzleFlash.noGravity = true;
            }
            Vector2 beamEnd = player.Center + lockedDirection * beamLength;
            for (int i = 0; i < 30; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(10f, 10f);
                Dust endFlash = Dust.NewDustDirect(beamEnd, 1, 1, DustID.Electric, dustVel.X, dustVel.Y, 0, new Color(100, 180, 255), 1.5f);
                endFlash.noGravity = true;
            }
        }
        private void DamageEnemiesAlongBeam(Player player)
        {
            hitPositions.Clear();
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                Vector2 toNPC = npc.Center - player.Center;
                float projection = Vector2.Dot(toNPC, lockedDirection);
                if (projection < 0 || projection > beamLength)
                    continue;
                Vector2 closestPoint = player.Center + lockedDirection * projection;
                float distance = Vector2.Distance(closestPoint, npc.Center);
                float hitRadius = npc.width / 2f + 40f;
                if (distance < hitRadius)
                {
                    player.ApplyDamageToNPC(npc, Projectile.damage, Projectile.knockBack, (int)lockedDirection.X, false);
                    hitPositions.Add(npc.Center);
                    for (int i = 0; i < 25; i++)
                    {
                        Dust hitDust = Dust.NewDustDirect(npc.Center, 10, 10, DustID.Electric, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 0, new Color(255, 200, 100), 2f);
                        hitDust.noGravity = true;
                    }
                }
            }
        }
        private void FireState(Player player)
        {
            Projectile.Center = player.Center;
            UpdatePlayerDirection(player, lockedDirection);
            beamAlpha -= 0.06f;
            beamWidth *= 0.9f;
            screenShakeIntensity *= 0.8f;
            for (float i = 0; i < beamLength; i += 80f)
            {
                Vector2 lightPos = player.Center + lockedDirection * i;
                Lighting.AddLight(lightPos, 0.5f * beamAlpha, 0.8f * beamAlpha, 1f * beamAlpha);
            }
            if (beamAlpha <= 0)
            {
                state = 2;
                Projectile.timeLeft = 60;
            }
        }
        private void FadeState()
        {
            afterglowAlpha -= 0.05f;
            screenShakeIntensity = 0;
            if (afterglowAlpha <= 0)
            {
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Player player = Main.player[Projectile.owner];
            if (state == 0)
            {
                DrawChargingEffect(spriteBatch, pixel, player);
            }
            else if (state == 1 && beamAlpha > 0)
            {
                DrawMainBeam(spriteBatch, pixel, player);
            }
            if (screenShakeIntensity > 0.5f)
            {
                Main.screenPosition += Main.rand.NextVector2Circular(screenShakeIntensity, screenShakeIntensity);
            }
            return false;
        }
        private void DrawChargingEffect(SpriteBatch spriteBatch, Texture2D pixel, Player player)
        {
            float radius = 80f * (1f - chargeProgress) + 15f;
            int segments = 60;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                Vector2 pos1 = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 pos2 = player.Center + new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * radius;
                pos1 -= Main.screenPosition;
                pos2 -= Main.screenPosition;
                float pulse = 0.5f + 0.5f * (float)Math.Sin(angle * 4 + Main.GameUpdateCount * 0.15f);
                float segmentAlpha = chargeProgress * (0.6f + 0.4f * pulse);
                Color chargeColor = Color.Lerp(new Color(50, 120, 200), new Color(150, 220, 255), pulse) * segmentAlpha;
                DrawLine(spriteBatch, pixel, pos1, pos2, chargeColor, 2f + chargeProgress * 3f);
            }
            Vector2 centerPos = player.Center - Main.screenPosition;
            float centerSize = 5f + chargeProgress * 15f;
            Color centerColor = new Color(200, 230, 255) * chargeProgress;
            spriteBatch.Draw(pixel, centerPos, new Rectangle(0, 0, 1, 1), centerColor, 0f, new Vector2(0.5f, 0.5f), centerSize, SpriteEffects.None, 0);
            if (chargeProgress > 0.2f)
            {
                float aimAlpha = (chargeProgress - 0.2f) / 0.8f * 0.4f;
                float aimLength = Math.Min(MAX_BEAM_LENGTH, 2000f * chargeProgress);
                DrawBeamLayers(spriteBatch, pixel, player.Center - Main.screenPosition, beamDirection, aimLength, aimAlpha, 2f + chargeProgress * 2f, true);
            }
            if (chargeProgress > 0.5f)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4 + Main.GameUpdateCount * 0.05f;
                    Vector2 lineStart = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius * 1.5f;
                    Vector2 lineEnd = player.Center + beamDirection * 30f;
                    lineStart -= Main.screenPosition;
                    lineEnd -= Main.screenPosition;
                    Color lineColor = new Color(100, 200, 255) * (chargeProgress - 0.5f) * 2f * 0.5f;
                    DrawLine(spriteBatch, pixel, lineStart, lineEnd, lineColor, 1.5f);
                }
            }
        }
        private void DrawMainBeam(SpriteBatch spriteBatch, Texture2D pixel, Player player)
        {
            Vector2 startPos = player.Center - Main.screenPosition;
            DrawBeamLayers(spriteBatch, pixel, startPos, lockedDirection, beamLength, beamAlpha * 0.2f, beamWidth * 2.5f, false, new Color(30, 60, 150));
            DrawBeamLayers(spriteBatch, pixel, startPos, lockedDirection, beamLength, beamAlpha * 0.4f, beamWidth * 1.5f, false, new Color(80, 150, 255));
            DrawBeamLayers(spriteBatch, pixel, startPos, lockedDirection, beamLength, beamAlpha * 0.7f, beamWidth * 0.9f, false, new Color(130, 200, 255));
            DrawBeamLayers(spriteBatch, pixel, startPos, lockedDirection, beamLength, beamAlpha, beamWidth * 0.35f, false, Color.White);
            DrawElectricArcs(spriteBatch, pixel, player.Center, lockedDirection, beamLength, beamAlpha);
            foreach (Vector2 hitPos in hitPositions)
            {
                DrawImpactEffect(spriteBatch, pixel, hitPos - Main.screenPosition, beamAlpha);
            }
            Vector2 beamEnd = startPos + lockedDirection * beamLength;
            DrawImpactEffect(spriteBatch, pixel, beamEnd, beamAlpha * 0.7f);
        }
        private void DrawBeamLayers(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 direction, float length, float alpha, float width, bool dashed, Color? overrideColor = null)
        {
            Color beamColor = overrideColor ?? new Color(100, 200, 255);
            beamColor *= alpha;
            float rotation = direction.ToRotation();
            if (dashed)
            {
                for (float i = 0; i < length; i += 50f)
                {
                    Vector2 segmentStart = start + direction * i;
                    float segmentLength = Math.Min(25f, length - i);
                    spriteBatch.Draw(pixel, segmentStart, new Rectangle(0, 0, 1, 1), beamColor, rotation, new Vector2(0, 0.5f), new Vector2(segmentLength, width), SpriteEffects.None, 0);
                }
            }
            else
            {
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), beamColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0);
            }
        }
        private void DrawElectricArcs(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 direction, float length, float alpha)
        {
            if (alpha < 0.2f) return;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            int arcCount = (int)(length / 200f);
            for (int arc = 0; arc < arcCount; arc++)
            {
                float arcOffset = Main.rand.NextFloat(50f, length - 50f);
                Vector2 arcStart = start + direction * arcOffset - Main.screenPosition;
                float arcLength = Main.rand.NextFloat(40f, 100f);
                Vector2 arcEnd = arcStart + perpendicular * arcLength * (Main.rand.NextBool() ? 1 : -1);
                arcEnd += direction * Main.rand.NextFloat(-30f, 30f);
                Color arcColor = new Color(180, 220, 255) * alpha * 0.9f;
                DrawLine(spriteBatch, pixel, arcStart, arcEnd, arcColor, 2f);
                if (Main.rand.NextBool(2))
                {
                    Vector2 branchEnd = arcEnd + Main.rand.NextVector2Circular(30f, 30f);
                    DrawLine(spriteBatch, pixel, arcEnd, branchEnd, arcColor * 0.6f, 1.5f);
                }
            }
        }
        private void DrawImpactEffect(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pos, float alpha)
        {
            float size = 50f * alpha;
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                Vector2 pos1 = pos + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * size;
                Vector2 pos2 = pos + new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * size;
                Color impactColor = new Color(255, 180, 80) * alpha;
                DrawLine(spriteBatch, pixel, pos1, pos2, impactColor, 3f);
            }
            Color centerColor = new Color(255, 220, 150) * alpha;
            spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), centerColor, 0f, new Vector2(0.5f, 0.5f), size * 0.5f, SpriteEffects.None, 0);
        }
        private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 diff = end - start;
            float rotation = diff.ToRotation();
            float length = diff.Length();
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0);
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
    }
}
