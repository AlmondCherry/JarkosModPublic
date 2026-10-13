using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;

namespace jarkos.projectiles.melee.other_melee
{
    public class Kharon_hammerproj : ModProjectile
    {
        private int bounceCount
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private bool isChargingUp
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        private float chargeTimer = 0f;
        private HashSet<int> hitNPCs = new HashSet<int>();
        private List<ShieldShockwave> shockwaves = new List<ShieldShockwave>();


        private Vector2 bounceStartPos;
        private int targetNPCIndex = -1;
        private float flightTimer = 0f;
        private float totalFlightTime = 30f;
        private bool isSingleTargetBounce = false;

        private class ShieldShockwave
        {
            public Vector2 Position;
            public float Scale;
            public float MaxScale;
            public float Opacity;
            public float Rotation;
            public Color WaveColor;

            public ShieldShockwave(Vector2 pos, float maxScale, Color color)
            {
                Position = pos;
                Scale = 0.1f;
                MaxScale = maxScale;
                Opacity = 1f;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                WaveColor = color;
            }

            public bool Update()
            {
                Scale += (MaxScale - Scale) * 0.14f + 0.04f;
                Rotation += 0.03f;
                float progress = Scale / MaxScale;
                Opacity = MathHelper.Clamp(1f - progress, 0f, 1f);
                return progress >= 0.98f;
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.timeLeft = 1200;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.2f, 0.9f, 0.4f);


            for (int i = shockwaves.Count - 1; i >= 0; i--)
            {
                if (shockwaves[i].Update())
                {
                    shockwaves.RemoveAt(i);
                }
            }


            if (isChargingUp)
            {
                chargeTimer++;
                float progress = MathHelper.Clamp(chargeTimer / 30f, 0f, 1f);

                Projectile.velocity.X *= 0.85f;
                Projectile.velocity.Y = -3.2f * (1f - progress);

                Projectile.rotation += 0.25f + progress * 0.2f;
                Lighting.AddLight(Projectile.Center, 2.0f * progress, 1.4f * progress, 0.4f * progress);

                if (Main.rand.NextBool(2))
                {
                    Vector2 particleOffset = Main.rand.NextVector2Circular(60f, 60f);
                    Vector2 particleVel = (Projectile.Center - (Projectile.Center + particleOffset)) * 0.12f;
                    new Spark(Projectile.Center + particleOffset, particleVel, Color.Gold, 18, 0.8f, 0.9f, 0.1f).Spawn();
                }

                if (chargeTimer >= 30f)
                {
                    TriggerExplosion(isFinalBounce: true);
                }

                return;
            }


            if (bounceCount == 0 || targetNPCIndex == -1)
            {
                Projectile.velocity.Y += 0.42f;
                if (Projectile.velocity.Y > 18f)
                    Projectile.velocity.Y = 18f;

                float speed = Projectile.velocity.Length();
                float sDir = Projectile.velocity.X >= 0f ? 1f : -1f;
                Projectile.rotation += sDir * (0.06f + speed * 0.035f);

                if (speed > 4f && Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.2f);
                    dust.velocity = Projectile.velocity * -0.2f;
                    dust.noGravity = true;
                }
                return;
            }


            NPC targetNPC = Main.npc[targetNPCIndex];
            if (!targetNPC.active || targetNPC.friendly || targetNPC.dontTakeDamage || targetNPC.life <= 0)
            {
                targetNPCIndex = -1;
                return;
            }

            flightTimer++;
            float t = MathHelper.Clamp(flightTimer / totalFlightTime, 0f, 1f);

            Vector2 p0 = bounceStartPos;
            Vector2 p1 = targetNPC.Center;

            Vector2 mid = (p0 + p1) * 0.5f;
            float dist = Vector2.Distance(p0, p1);
            float arcHeight = MathF.Max(90f, dist * 0.38f);
            Vector2 controlPoint = mid - new Vector2(0f, arcHeight);


            float oneMinusT = 1f - t;
            Vector2 targetParabolaPos = (oneMinusT * oneMinusT * p0) + (2f * oneMinusT * t * controlPoint) + (t * t * p1);

            Vector2 newVelocity = targetParabolaPos - Projectile.Center;
            Projectile.velocity = newVelocity;

            float currentSpeed = newVelocity.Length();
            float spinDir = p1.X >= p0.X ? 1f : -1f;
            Projectile.rotation += spinDir * (0.08f + currentSpeed * 0.035f);

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 100, default, 1.2f);
                dust.velocity = -newVelocity * 0.15f;
                dust.noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TriggerExplosion(isFinalBounce: false);
            return true;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

            float bounceDamageMult = 1f + (bounceCount / 25f) * 1.5f;
            modifiers.SourceDamage *= bounceDamageMult;


            if (isSingleTargetBounce)
            {
                modifiers.SourceDamage *= 0.75f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            int sparkCount = Main.rand.Next(4, 7);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Color greenSparkColor = new Color(50, 255, 100);

            for (int i = 0; i < sparkCount; i++)
            {
                float angle = baseAngle + (MathHelper.TwoPi / sparkCount) * i + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = Main.rand.NextFloat(5f, 9f);
                Vector2 sparkVel = angle.ToRotationVector2() * speed;
                int lifetime = Main.rand.Next(16, 26);
                float scale = Main.rand.NextFloat(0.9f, 1.4f);
                new Spark(target.Center, sparkVel, greenSparkColor, lifetime, scale, 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
            }

            if (isChargingUp) return;

            bounceCount++;
            hitNPCs.Add(target.whoAmI);


            if (bounceCount >= 25)
            {
                isChargingUp = true;
                chargeTimer = 0f;
                targetNPCIndex = -1;
                Projectile.tileCollide = false;
                Projectile.velocity = new Vector2(0f, -3.5f);

                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.9f, Pitch = 0.3f }, Projectile.Center);
                return;
            }


            int nextTarget = FindNextTarget(target);

            if (nextTarget != -1)
            {
                targetNPCIndex = nextTarget;
                bounceStartPos = Projectile.Center;
                flightTimer = 0f;

                NPC nextNPC = Main.npc[nextTarget];
                float dist = Vector2.Distance(bounceStartPos, nextNPC.Center);


                float baseSpeed = 18f + (bounceCount * 3.5f);
                float minTime = MathF.Max(3f, 20f - bounceCount * 0.7f);
                float maxTime = MathF.Max(8f, 55f - bounceCount * 1.9f);
                totalFlightTime = MathHelper.Clamp(dist / baseSpeed, minTime, maxTime);

                SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.8f, Pitch = MathHelper.Clamp(0.04f * bounceCount, 0f, 1f) }, Projectile.Center);

                for (int i = 0; i < 12; i++)
                {
                    Dust spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame);
                    spark.velocity = Main.rand.NextVector2Circular(4f, 4f);
                    spark.scale = 1.3f;
                    spark.noGravity = true;
                }
            }
            else
            {
                targetNPCIndex = -1;
                Projectile.velocity = new Vector2(-Projectile.velocity.X * 0.6f, -7f);
                SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.7f }, Projectile.Center);
            }
        }

        private int FindNextTarget(NPC currentTarget)
        {
            float searchRadius = 850f;


            int bestUnhitTarget = -1;
            float closestUnhitDist = searchRadius;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0 && npc.whoAmI != currentTarget.whoAmI)
                {
                    if (!hitNPCs.Contains(npc.whoAmI))
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestUnhitDist)
                        {
                            closestUnhitDist = dist;
                            bestUnhitTarget = npc.whoAmI;
                        }
                    }
                }
            }

            if (bestUnhitTarget != -1)
            {
                isSingleTargetBounce = false;
                return bestUnhitTarget;
            }


            int bestOtherTarget = -1;
            float closestOtherDist = searchRadius;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0 && npc.whoAmI != currentTarget.whoAmI)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestOtherDist)
                    {
                        closestOtherDist = dist;
                        bestOtherTarget = npc.whoAmI;
                    }
                }
            }

            if (bestOtherTarget != -1)
            {
                isSingleTargetBounce = false;
                return bestOtherTarget;
            }


            if (currentTarget != null && currentTarget.active && !currentTarget.friendly && !currentTarget.dontTakeDamage && currentTarget.life > 0)
            {
                float dist = Vector2.Distance(Projectile.Center, currentTarget.Center);
                if (dist <= searchRadius)
                {
                    isSingleTargetBounce = true;
                    return currentTarget.whoAmI;
                }
            }

            isSingleTargetBounce = false;
            return -1;
        }

        private void TriggerExplosion(bool isFinalBounce)
        {
            Vector2 explodeCenter = Projectile.Center;
            float bounceFactor = MathHelper.Clamp(bounceCount / 25f, 0f, 1f);
            float explodeRadius = (isFinalBounce ? 320f : 180f) * (1f + bounceFactor * 1.2f);

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.2f + bounceFactor * 0.4f, Pitch = -0.3f - bounceFactor * 0.2f }, explodeCenter);
            SoundEngine.PlaySound(SoundID.Item88 with { Volume = 1f + bounceFactor * 0.5f, Pitch = 0.1f }, explodeCenter);

            Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake++;

            float shockwaveScale = (isFinalBounce ? 3.8f : 2.2f) * (1f + bounceFactor * 1.2f);
            shockwaves.Add(new ShieldShockwave(explodeCenter, shockwaveScale, isFinalBounce ? Color.Gold : Color.OrangeRed));


            float maxTargetScale = explodeRadius / 70f;
            int ringCount = isFinalBounce ? 8 : 3;

            for (int r = 0; r < ringCount; r++)
            {
                Color ringColor = Color.Lerp(new Color(60, 255, 120), Color.Green, r / (float)ringCount);
                float startScale = 0.2f + r * 0.15f;
                float finalScale = maxTargetScale * (0.75f + r * 0.25f);
                int ringLifetime = 24 + r * 6;

                var ring = new Circleedge(explodeCenter, Vector2.Zero, ringColor, ringLifetime, startScale, 1f, 0.04f);
                ring.ExpansionRate = (finalScale - startScale) / 14f;
                ring.Spawn();
            }

            int sparkCount = (int)((isFinalBounce ? 35 : 18) * (1f + bounceFactor * 1.5f));
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular((isFinalBounce ? 14f : 8f) * (1f + bounceFactor * 0.5f), (isFinalBounce ? 14f : 8f) * (1f + bounceFactor * 0.5f));
                Color pColor = Color.Lerp(Color.Gold, Color.OrangeRed, Main.rand.NextFloat());
                new Spark(explodeCenter, vel, pColor, Main.rand.Next(20, 40), Main.rand.NextFloat(1f, 2.2f), 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
            }

            int novaCount = (int)((isFinalBounce ? 16 : 8) * (1f + bounceFactor * 1.5f));
            for (int i = 0; i < novaCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f * (1f + bounceFactor * 0.5f), 10f * (1f + bounceFactor * 0.5f));
                new Nova_particle(explodeCenter, vel, Color.Gold * 0.9f, 25, Main.rand.NextFloat(0.8f, 1.8f), 1f).Spawn();
            }

            int starCount = (int)((isFinalBounce ? 12 : 6) * (1f + bounceFactor * 1.5f));
            for (int i = 0; i < starCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(9f * (1f + bounceFactor * 0.5f), 9f * (1f + bounceFactor * 0.5f));
                new StarParticle(explodeCenter, vel, Color.LightGoldenrodYellow, 30, Main.rand.NextFloat(0.6f, 1.5f), 1f).Spawn();
            }

            int fireCount = (int)(30 * (1f + bounceFactor * 1.5f));
            for (int i = 0; i < fireCount; i++)
            {
                Dust fire = Dust.NewDustDirect(explodeCenter - new Vector2(20), 40, 40, DustID.Torch, 0f, 0f, 100, default, 2.2f * (1f + bounceFactor * 0.5f));
                fire.velocity = Main.rand.NextVector2Circular(11f * (1f + bounceFactor * 0.5f), 11f * (1f + bounceFactor * 0.5f));
                fire.noGravity = true;
            }

            int smokeCount = (int)(20 * (1f + bounceFactor * 1.5f));
            for (int i = 0; i < smokeCount; i++)
            {
                Dust smoke = Dust.NewDustDirect(explodeCenter - new Vector2(20), 40, 40, DustID.Smoke, 0f, 0f, 100, default, 1.8f * (1f + bounceFactor * 0.5f));
                smoke.velocity = Main.rand.NextVector2Circular(7f * (1f + bounceFactor * 0.5f), 7f * (1f + bounceFactor * 0.5f));
            }

            if (Main.myPlayer == Projectile.owner)
            {
                int explodeDamage = (int)(Projectile.damage * (isFinalBounce ? 3.0f : 1.5f) * (1f + bounceFactor * 2.0f));

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0)
                    {
                        if (Vector2.Distance(explodeCenter, npc.Center) <= explodeRadius)
                        {
                            NPC.HitInfo hit = new NPC.HitInfo
                            {
                                Damage = explodeDamage,
                                Knockback = 9f * (1f + bounceFactor * 0.5f),
                                HitDirection = (npc.Center.X > explodeCenter.X) ? 1 : -1,
                                Crit = Main.rand.NextBool(3)
                            };
                            npc.StrikeNPC(hit);
                        }
                    }
                }
            }

            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            if (shockwaves.Count > 0)
            {
                Texture2D shieldTex = ModContent.Request<Texture2D>("jarkos/textures/shield_field", AssetRequestMode.ImmediateLoad).Value;
                if (shieldTex != null)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    Vector2 shieldOrigin = shieldTex.Size() / 2f;
                    foreach (var wave in shockwaves)
                    {
                        Vector2 waveDrawPos = wave.Position - Main.screenPosition;
                        Color waveColor = wave.WaveColor * wave.Opacity;
                        sb.Draw(shieldTex, waveDrawPos, null, waveColor, wave.Rotation, shieldOrigin, wave.Scale, SpriteEffects.None, 0f);
                    }

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float bounceScaleMult = 1f + (bounceCount / 25f) * 1.0f;
            float drawScale = Projectile.scale * bounceScaleMult * (isChargingUp ? 1f + (chargeTimer / 30f) * 0.4f : 1f);


            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            if (glowTex != null)
            {
                float pulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.22f) * 0.12f;
                Vector2 glowOrigin = glowTex.Size() * 0.5f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Color lightGreenColor = new Color(80, 255, 140) * 0.75f * pulse;
                sb.Draw(glowTex, drawPos, null, lightGreenColor, 0f, glowOrigin, drawScale * 1.35f * pulse, SpriteEffects.None, 0f);

                Color brightCoreColor = new Color(200, 255, 230) * 0.85f * pulse;
                sb.Draw(glowTex, drawPos, null, brightCoreColor, 0f, glowOrigin, drawScale * 0.65f * pulse, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }


            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 oldPos = Projectile.oldPos[i];
                if (oldPos == Vector2.Zero) continue;

                float oldRot = Projectile.oldRot[i];
                float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
                float trailAlpha = progress * 0.45f;
                Vector2 oldDrawPos = oldPos + Projectile.Size * 0.5f - Main.screenPosition;

                Color greenGhostColor = new Color(50, 255, 110) * trailAlpha;
                sb.Draw(texture, oldDrawPos, null, greenGhostColor, oldRot, origin, drawScale, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color drawColor = isChargingUp ? Color.Lerp(lightColor, Color.Gold, chargeTimer / 30f) : lightColor;

            sb.Draw(texture, drawPos, null, drawColor, Projectile.rotation, origin, drawScale, SpriteEffects.None, 0f);


            Texture2D glowmaskTex = ModContent.Request<Texture2D>("jarkos/textures/glowmasks/kharon_hammer_glowmask", AssetRequestMode.ImmediateLoad).Value;
            if (glowmaskTex != null)
            {
                Vector2 glowmaskOrigin = glowmaskTex.Size() * 0.5f;
                Color glowmaskColor = isChargingUp ? Color.Lerp(Color.White, Color.Gold, chargeTimer / 30f) : Color.White;
                sb.Draw(glowmaskTex, drawPos, null, glowmaskColor, Projectile.rotation, glowmaskOrigin, drawScale, SpriteEffects.None, 0f);
            }

            if (isChargingUp)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                float glowAlpha = chargeTimer / 30f;
                sb.Draw(texture, drawPos, null, Color.Gold * glowAlpha, Projectile.rotation, origin, drawScale * 1.15f, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
