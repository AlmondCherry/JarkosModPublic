using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using jarkos.items.weapons.summon.whip;
using jarkos.systems;

namespace jarkos.projectiles.summon.whip
{
    public class Reborn_proj : ModProjectile
    {
        private enum WhipState
        {
            Charging, Attacking, Retracting
        }
        private List<Vector2> whipPoints = new List<Vector2>();
        private List<Vector2> trailHistory = new List<Vector2>();
        private const int MaxTrailHistory = 60;
        private const int WhipSegments = 32;
        private const float MaxChargeTime = 180f;
        private float chargeTimer = 0f;
        private float spinAngle = 0f;
        private float spinSpeed = 0.08f;
        private float whipLength = 80f;
        private float attackProgress = 0f;
        private float attackSpeed = 0f;
        private float attackAngle = 0f;
        private bool hasDealtDamage = false;
        private WhipState currentState = WhipState.Charging;
        private Vector2 impactPoint = Vector2.Zero;
        private float impactRadius = 0f;
        private int impactTimer = 0;
        private const int ImpactDuration = 15;
        private float glowIntensity = 0f;
        private float chargeRatio => MathHelper.Clamp(chargeTimer / MaxChargeTime, 0f, 1f);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 0;
            Projectile.hide = false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.MountedCenter;
            Projectile.timeLeft = 2;
            Vector2 toMouse = Main.MouseWorld - player.MountedCenter;
            float mouseAngle = toMouse.ToRotation();
            player.heldProj = Projectile.whoAmI;
            switch (currentState)
            {
                case WhipState.Charging:
                    HandleCharging(player, mouseAngle);
                    break;
                case WhipState.Attacking:
                    HandleAttacking(player);
                    break;
                case WhipState.Retracting:
                    HandleRetracting(player);
                    break;
            }
            float armAngle = currentState == WhipState.Charging ? mouseAngle : attackAngle;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armAngle - MathHelper.PiOver2);
            player.itemRotation = (float)Math.Atan2(toMouse.Y * player.direction, toMouse.X * player.direction);
            if (Projectile.owner == Main.myPlayer)
            {
                player.itemTime = 2;
                player.itemAnimation = 2;
            }
            UpdateWhipPoints(player);
            if (whipPoints.Count > 0)
            {
                Vector2 tipPos = whipPoints[whipPoints.Count - 1];
                trailHistory.Insert(0, tipPos);
                if (trailHistory.Count > MaxTrailHistory)
                    trailHistory.RemoveAt(trailHistory.Count - 1);
            }
            if (impactTimer > 0)
            {
                impactTimer--;
                SpawnImpactParticles();
            }
        }
        private void HandleCharging(Player player, float mouseAngle)
        {
            bool channeling = player.channel && player.HeldItem.type == ModContent.ItemType<Reborn>();
            if (channeling && chargeTimer < MaxChargeTime)
            {
                chargeTimer++;
                float baseSpeed = 0.08f;
                float maxSpeed = 0.35f;
                spinSpeed = MathHelper.Lerp(baseSpeed, maxSpeed, chargeRatio);
                float baseLength = 80f;
                float maxLength = 220f;
                whipLength = MathHelper.Lerp(baseLength, maxLength, chargeRatio);
                spinAngle += spinSpeed;
                if (spinAngle > MathHelper.TwoPi)
                    spinAngle -= MathHelper.TwoPi;
                glowIntensity = MathHelper.Lerp(0.3f, 1.5f, chargeRatio);
                if (chargeTimer % 30 == 0 && chargeTimer > 30)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.3f + chargeRatio * 0.4f, Pitch = -0.5f + chargeRatio * 1.0f }, player.Center);
                }
                player.ChangeDir(Main.MouseWorld.X > player.Center.X ? 1 : -1);
                if (chargeTimer > 20 && Main.rand.NextBool(Math.Max(1, 5 - (int)(chargeRatio * 4))))
                {
                    Vector2 tipPos = GetSpinTipPosition(player);
                    Color dustColor = GetChargeColor(chargeTimer * 0.03f);
                    int d = Dust.NewDust(tipPos - new Vector2(4), 8, 8, DustID.RainbowMk2, 0, 0, 150, dustColor, 0.6f + chargeRatio * 0.8f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.4f;
                }
            }
            else
            {
                StartAttack(player, mouseAngle);
            }
        }
        private void StartAttack(Player player, float mouseAngle)
        {
            currentState = WhipState.Attacking;
            attackProgress = 0f;
            hasDealtDamage = false;
            attackSpeed = MathHelper.Lerp(0.06f, 0.04f, chargeRatio);
            attackAngle = mouseAngle;
            float damageMultiplier = MathHelper.Lerp(1f, 4f, chargeRatio);
            Projectile.damage = (int)(Main.player[Projectile.owner].HeldItem.damage * damageMultiplier);
            whipLength = MathHelper.Lerp(120f, 350f, chargeRatio);
            impactRadius = MathHelper.Lerp(30f, 80f, chargeRatio);
            SoundEngine.PlaySound(SoundID.Item153 with { Volume = 0.6f + chargeRatio * 0.6f, Pitch = 0.2f - chargeRatio * 0.3f }, player.Center);
            player.ChangeDir(Main.MouseWorld.X > player.Center.X ? 1 : -1);
        }
        private void HandleAttacking(Player player)
        {
            attackProgress += attackSpeed;
            glowIntensity = MathHelper.Lerp(glowIntensity, 0.5f, 0.05f);
            if (attackProgress >= 1f)
            {
                if (!hasDealtDamage)
                {
                    hasDealtDamage = true;
                    if (whipPoints.Count > 0)
                    {
                        impactPoint = whipPoints[whipPoints.Count - 1];
                        impactTimer = ImpactDuration;
                        DealAreaDamage(player);
                        SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f + chargeRatio * 0.5f, Pitch = 0.5f }, impactPoint);
                        SpawnImpactBurst();
                    }
                }
                currentState = WhipState.Retracting;
                attackProgress = 0f;
            }
        }
        private void HandleRetracting(Player player)
        {
            attackProgress += 0.07f;
            glowIntensity = MathHelper.Lerp(glowIntensity, 0f, 0.1f);
            whipLength = MathHelper.Lerp(whipLength, 0f, 0.1f);
            if (attackProgress >= 1f || whipLength < 5f)
            {
                Projectile.Kill();
            }
        }
        private Vector2 GetSpinTipPosition(Player player)
        {
            Vector2 center = player.MountedCenter + new Vector2(0, -30f);
            float radiusX = whipLength * 0.5f;
            float radiusY = whipLength * 0.25f;
            return center + new Vector2((float)Math.Cos(spinAngle) * radiusX, (float)Math.Sin(spinAngle) * radiusY - 10f);
        }
        private void UpdateWhipPoints(Player player)
        {
            whipPoints.Clear();
            Vector2 origin = player.MountedCenter + new Vector2(0, 2f);
            switch (currentState)
            {
                case WhipState.Charging:
                    GenerateSpinPoints(player, origin);
                    break;
                case WhipState.Attacking:
                    GenerateAttackPoints(player, origin);
                    break;
                case WhipState.Retracting:
                    GenerateRetractPoints(player, origin);
                    break;
            }
            if (whipPoints.Count >= 4)
            {
                List<Vector2> smoothed = new List<Vector2>();
                for (int i = 0; i < whipPoints.Count - 1; i++)
                {
                    Vector2 p0 = whipPoints[Math.Max(0, i - 1)];
                    Vector2 p1 = whipPoints[i];
                    Vector2 p2 = whipPoints[Math.Min(whipPoints.Count - 1, i + 1)];
                    Vector2 p3 = whipPoints[Math.Min(whipPoints.Count - 1, i + 2)];
                    int subdivisions = 4;
                    for (int j = 0; j < subdivisions; j++)
                    {
                        float t = (float)j / subdivisions;
                        smoothed.Add(CatmullRom(p0, p1, p2, p3, t));
                    }
                }
                smoothed.Add(whipPoints[whipPoints.Count - 1]);
                whipPoints = smoothed;
            }
        }
        private void GenerateSpinPoints(Player player, Vector2 origin)
        {
            Vector2 handlePos = origin;
            Vector2 tipPos = GetSpinTipPosition(player);
            int segments = WhipSegments;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(handlePos, tipPos, t);
                float sag = (float)Math.Sin(t * MathHelper.Pi) * (20f + chargeRatio * 10f);
                float wave = (float)Math.Sin(t * MathHelper.Pi * 3f - spinAngle * 2f) * (3f + chargeRatio * 5f) * t;
                Vector2 dir = tipPos - handlePos;
                dir = dir == Vector2.Zero ? Vector2.UnitX : dir;
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                perp.Normalize();
                basePos += new Vector2(0, sag * (1f - chargeRatio * 0.5f));
                basePos += perp * wave;
                whipPoints.Add(basePos);
            }
        }
        private void GenerateAttackPoints(Player player, Vector2 origin)
        {
            Vector2 handlePos = origin;
            float easedProgress = MathSystem.EaseOutQuart(attackProgress);
            float currentLength = whipLength * easedProgress;
            float whipCurveAngle = attackAngle + (float)Math.Sin(attackProgress * MathHelper.Pi) * 0.3f * player.direction;
            Vector2 endPoint = handlePos + new Vector2((float)Math.Cos(whipCurveAngle) * currentLength, (float)Math.Sin(whipCurveAngle) * currentLength);
            int segments = WhipSegments;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(handlePos, endPoint, t);
                float whipBend = (float)Math.Sin(t * MathHelper.Pi * 2f - attackProgress * MathHelper.Pi * 3f);
                whipBend *= (1f - attackProgress) * 25f * t;
                Vector2 dir = endPoint - handlePos;
                if (dir == Vector2.Zero) dir = Vector2.UnitX;
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                perp.Normalize();
                basePos += perp * whipBend;
                float sagAmount = (1f - easedProgress) * 15f;
                float sag = (float)Math.Sin(t * MathHelper.Pi) * sagAmount;
                basePos.Y += sag;
                whipPoints.Add(basePos);
            }
        }
        private void GenerateRetractPoints(Player player, Vector2 origin)
        {
            Vector2 handlePos = origin;
            float retractEase = MathSystem.EaseInQuad(attackProgress);
            float currentLength = whipLength * (1f - retractEase);
            float retractAngle = attackAngle + (float)Math.Sin(attackProgress * MathHelper.Pi * 0.5f) * 0.2f;
            Vector2 endPoint = handlePos + new Vector2((float)Math.Cos(retractAngle) * currentLength, (float)Math.Sin(retractAngle) * currentLength);
            int segments = WhipSegments;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(handlePos, endPoint, t);
                float wave = (float)Math.Sin(t * MathHelper.Pi * 1.5f + attackProgress * 5f) * 8f * (1f - retractEase) * t;
                Vector2 dir = endPoint - handlePos;
                if (dir == Vector2.Zero) dir = Vector2.UnitX;
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                perp.Normalize();
                basePos += perp * wave;
                whipPoints.Add(basePos);
            }
        }
        private void DealAreaDamage(Player player)
        {
            float radius = impactRadius;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                float dist = Vector2.Distance(npc.Center, impactPoint);
                if (dist <= radius)
                {
                    Vector2 hitDirection = npc.Center - impactPoint;
                    hitDirection.Normalize();
                    int dir = hitDirection.X > 0 ? 1 : -1;
                    npc.SimpleStrikeNPC(Projectile.damage, dir, true, Projectile.knockBack * (1f + chargeRatio), Projectile.DamageType);
                    if (Main.player[Projectile.owner].HeldItem.DamageType == DamageClass.SummonMeleeSpeed)
                    {
                        npc.AddBuff(BuffID.BlandWhipEnemyDebuff, 240);
                    }
                }
            }
        }
        private void SpawnImpactBurst()
        {
            int particleCount = (int)(20 + chargeRatio * 40);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 10f + chargeRatio * 8f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                float phase = (float)i / particleCount;
                Color color = GetChargeColor(phase + chargeTimer * 0.02f);
                int d = Dust.NewDust(impactPoint - new Vector2(4), 8, 8, DustID.RainbowMk2, vel.X, vel.Y, 100, color, 1f + chargeRatio * 1.5f);
                Main.dust[d].noGravity = true;
            }
            for (int i = 0; i < 16; i++)
            {
                float angle = i / 16f * MathHelper.TwoPi;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (5f + chargeRatio * 6f);
                Color color = GetChargeColor(i / 16f);
                int d = Dust.NewDust(impactPoint, 0, 0, DustID.FireworksRGB, vel.X, vel.Y, 0, color, 1.5f);
                Main.dust[d].noGravity = true;
            }
            if (chargeRatio > 0.5f)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = i / 12f * MathHelper.TwoPi;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                    int g = Gore.NewGore(Projectile.GetSource_FromThis(), impactPoint, vel, Main.rand.Next(61, 64));
                    Main.gore[g].scale = 0.5f + chargeRatio * 0.5f;
                }
            }
        }
        private void SpawnImpactParticles()
        {
            if (impactTimer <= 0) return;
            float progress = 1f - (float)impactTimer / ImpactDuration;
            float currentRadius = impactRadius * MathSystem.EaseOutQuad(progress);
            int count = Math.Max(1, (int)(4 * (1f - progress)));
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = impactPoint + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * currentRadius * Main.rand.NextFloat(0.5f, 1f);
                Color color = GetChargeColor(Main.rand.NextFloat() + chargeTimer * 0.01f);
                int d = Dust.NewDust(pos - new Vector2(2), 4, 4, DustID.RainbowMk2, 0, -1f, 150, color, 0.5f + chargeRatio * 0.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.3f;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (currentState == WhipState.Attacking)
            {
                for (int i = whipPoints.Count / 2; i < whipPoints.Count; i++)
                {
                    if (whipPoints[i] != Vector2.Zero)
                    {
                        Rectangle pointBox = new Rectangle((int)whipPoints[i].X - 8, (int)whipPoints[i].Y - 8, 16, 16);
                        if (pointBox.Intersects(targetHitbox))
                            return true;
                    }
                }
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (whipPoints.Count < 2)
                return false;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawWhipGlow(sb, pixel);
            DrawTrailGlow(sb, pixel);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawWhipBody(sb, pixel);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawWhipBody(SpriteBatch sb, Texture2D pixel)
        {
            if (HealShaders.Stream == null || whipPoints.Count < 2)
            {
                return;
            }
            Effect effect = HealShaders.Stream;
            sb.End();
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2.5f);
            effect.Parameters["uOpacity"]?.SetValue((1f - Projectile.alpha / 255f) * 2.5f);
            float timeCycle = Main.GlobalTimeWrappedHourly * 1.5f;
            Color outerColor = GetChargeColor(timeCycle) * 3f;
            Color innerColor = Color.Lerp(GetChargeColor(timeCycle + 0.3f), Color.White, 0.5f) * 4f;
            effect.Parameters["uColor"]?.SetValue(outerColor.ToVector4());
            effect.Parameters["uColorCore"]?.SetValue(innerColor.ToVector4());
            var rawPoints = new List<Vector2>();
            for (int i = 0; i < whipPoints.Count; i++)
            {
                Vector2 pt = whipPoints[i];
                if (rawPoints.Count == 0 || Vector2.DistanceSquared(rawPoints[rawPoints.Count - 1], pt) > 4f)
                {
                    rawPoints.Add(pt);
                }
            }
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            TrailRenderer.Draw(effect, rawPoints, t => MathHelper.Lerp(8f, 2f, t) * (1f + chargeRatio * 0.5f), t => Color.White, HealShaders.HealNoise);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawWhipGlow(SpriteBatch sb, Texture2D pixel)
        {
            Rectangle src = new Rectangle(0, 0, 1, 1);
            for (int i = 0; i < whipPoints.Count - 1; i++)
            {
                float t = (float)i / whipPoints.Count;
                Vector2 start = whipPoints[i] - Main.screenPosition;
                Vector2 end = whipPoints[i + 1] - Main.screenPosition;
                Vector2 diff = end - start;
                float len = diff.Length();
                if (len < 0.5f) continue;
                float rotation = diff.ToRotation();
                float glowWidth = MathHelper.Lerp(20f, 8f, t) * (0.8f + glowIntensity * 0.8f);
                float colorPhase = t * 2f + Main.GameUpdateCount * 0.025f;
                Color glowColor = GetChargeColor(colorPhase) * (0.5f + glowIntensity * 0.6f);
                sb.Draw(pixel, start, src, glowColor, rotation, new Vector2(0, 0.5f), new Vector2(len, glowWidth), SpriteEffects.None, 0f);
            }
        }
        private void DrawTrailGlow(SpriteBatch sb, Texture2D pixel)
        {
            if (trailHistory.Count < 2) return;
            Rectangle src = new Rectangle(0, 0, 1, 1);
            int drawCount = Math.Min(trailHistory.Count, MaxTrailHistory);
            for (int i = 0; i < drawCount - 1; i++)
            {
                float t = (float)i / drawCount;
                Vector2 start = trailHistory[i] - Main.screenPosition;
                Vector2 end = trailHistory[i + 1] - Main.screenPosition;
                Vector2 diff = end - start;
                float len = diff.Length();
                if (len < 0.5f || len > 100f) continue;
                float rotation = diff.ToRotation();
                float fade = 1f - t;
                fade = fade * fade * fade;
                float trailWidth = MathHelper.Lerp(4f, 20f, fade) * (0.5f + glowIntensity * 0.5f);
                float colorPhase = t * 3f + Main.GameUpdateCount * 0.03f;
                Color trailColor = GetChargeColor(colorPhase) * (fade * 0.8f);
                sb.Draw(pixel, start, src, trailColor, rotation, new Vector2(0, 0.5f), new Vector2(len, trailWidth), SpriteEffects.None, 0f);
            }
        }
        public override void PostDraw(Color lightColor)
        {
            for (int i = 0; i < whipPoints.Count; i += 5)
            {
                float t = (float)i / whipPoints.Count;
                Color lc = GetChargeColor(t + Main.GameUpdateCount * 0.02f);
                float intensity = (0.2f + glowIntensity * 0.3f) * (1f - t * 0.5f);
                Lighting.AddLight(whipPoints[i], lc.ToVector3() * intensity);
            }
            if (whipPoints.Count > 0)
            {
                Vector2 tip = whipPoints[whipPoints.Count - 1];
                Color tipLight = GetChargeColor(Main.GameUpdateCount * 0.03f);
                Lighting.AddLight(tip, tipLight.ToVector3() * (0.4f + glowIntensity * 0.6f));
            }
            if (impactTimer > 0)
            {
                float impactFade = (float)impactTimer / ImpactDuration;
                Color impactLight = GetChargeColor(Main.GameUpdateCount * 0.05f);
                Lighting.AddLight(impactPoint, impactLight.ToVector3() * impactFade * (0.8f + chargeRatio));
            }
        }
        private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float x = 0.5f * ((2f * p1.X) + (-p0.X + p2.X) * t + (2f * p0.X - 5f * p1.X + 4f * p2.X - p3.X) * t2 + (-p0.X + 3f * p1.X - 3f * p2.X + p3.X) * t3);
            float y = 0.5f * ((2f * p1.Y) + (-p0.Y + p2.Y) * t + (2f * p0.Y - 5f * p1.Y + 4f * p2.Y - p3.Y) * t2 + (-p0.Y + 3f * p1.Y - 3f * p2.Y + p3.Y) * t3);
            return new Vector2(x, y);
        }
        private Color GetChargeColor(float phase)
        {
            float r = (float)(Math.Sin(phase * MathHelper.TwoPi) * 0.5f + 0.5f);
            float g = (float)(Math.Sin(phase * MathHelper.TwoPi + MathHelper.TwoPi / 3f) * 0.5f + 0.5f);
            float b = (float)(Math.Sin(phase * MathHelper.TwoPi + 2f * MathHelper.TwoPi / 3f) * 0.5f + 0.5f);
            float saturationBoost = 1f + chargeRatio * 0.5f;
            r = MathHelper.Clamp(r * saturationBoost, 0f, 1f);
            g = MathHelper.Clamp(g * saturationBoost, 0f, 1f);
            b = MathHelper.Clamp(b * saturationBoost, 0f, 1f);
            return new Color(r, g, b);
        }
    }
}
