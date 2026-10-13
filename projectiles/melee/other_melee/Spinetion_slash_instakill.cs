using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using ReLogic.Content;
using jarkos.systems;

using jarkos.particles;

namespace jarkos.projectiles.melee.other_melee
{
    public class Spinetion_slash_instakill : ModProjectile
    {
        private Vector2 launchDir = Vector2.Zero;
        private bool initialized = false;
        private float timer = 0f;
        private float targetRange = 450f;
        private float waveDirectionSign = 1f;
        private float waveFrequency = 1.5f;
        private const float totalDuration = 32f;
        private const float maxWaveAmplitude = 110f;
        
        private List<Vector2> trail = new List<Vector2>();

        private Player Owner => Main.player[Projectile.owner];

        public override string Texture => "jarkos/projectiles/melee/other_melee/Spinetion_slash";

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.timeLeft = 32;
        }

        public override void AI()
        {
            if (Owner.dead || !Owner.active)
            {
                Projectile.Kill();
                return;
            }
            if (!initialized)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.4f, Volume = 1f }, Projectile.Center);
                launchDir = Projectile.velocity;
                if (launchDir == Vector2.Zero)
                {
                    launchDir = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
                }
                else
                {
                    launchDir.Normalize();
                }
                Projectile.velocity = Vector2.Zero;
                targetRange = Projectile.ai[1];
                if (targetRange <= 0f)
                    targetRange = 450f;
                waveDirectionSign = Main.rand.NextBool() ? 1f : -1f;
                waveFrequency = Main.rand.NextFloat(1.2f, 1.8f);
                initialized = true;
            }
            timer++;
            float progress = timer / totalDuration;
            if (progress >= 1f)
            {
                Projectile.Kill();
                return;
            }
            float distProgress;
            if (progress < 0.4f)
            {
                distProgress = EaseOutQuad(progress / 0.4f);
            }
            else
            {
                distProgress = 1f - EaseInQuad((progress - 0.4f) / 0.6f);
            }
            Vector2 lastPos = Projectile.Center;
            Vector2 targetPos = Owner.MountedCenter + launchDir * (targetRange * distProgress);
            Vector2 perp = launchDir.RotatedBy(MathHelper.PiOver2);
            float currentAmplitude = maxWaveAmplitude * (targetRange / 450f);
            float wavePhase = progress * MathHelper.TwoPi * waveFrequency;
            float amp = currentAmplitude * (float)Math.Sin(progress * MathHelper.Pi);
            
            if (Projectile.ai[2] != -1)
            {
                amp *= (1f - distProgress);
            }
            
            float tipWave = (float)Math.Sin(wavePhase - timer * 0.2f) * amp * waveDirectionSign;
            Projectile.Center = targetPos + perp * tipWave;
            
            trail.Add(Projectile.Center);
            if (trail.Count > 8)
                trail.RemoveAt(0);

            Vector2 moveDelta = Projectile.Center - lastPos;
            if (moveDelta != Vector2.Zero)
            {
                Projectile.rotation = moveDelta.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                Projectile.rotation = launchDir.ToRotation() + MathHelper.PiOver2;
            }
            UpdatePlayerState();
            SpawnDustEffects();
        }

        private void UpdatePlayerState()
        {
            if (Projectile.Center.X > Owner.Center.X)
                Owner.ChangeDir(1);
            else
                Owner.ChangeDir(-1);
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            float rotation = (Projectile.Center - Owner.Center).ToRotation();
            Owner.itemRotation = MathHelper.WrapAngle(rotation - (Owner.direction == -1 ? MathHelper.Pi : 0));
        }

        private float EaseOutQuad(float t)
        {
            return t * (2f - t);
        }

        private float EaseInQuad(float t)
        {
            return t * t;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            
            if (!target.boss && target.lifeMax <= 8000)
            {
                modifiers.FlatBonusDamage += 999999f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Owner.GetModPlayer<jarkos.Screenshake_system>().weak_shake = 2; 
            target.AddBuff(ModContent.BuffType<jarkos.buffs.debuff.BleedingMobDebuff>(), 300);
            SoundEngine.PlaySound(SoundID.NPCHit2, target.position);
            
            
            if (!target.boss && target.lifeMax <= 8000 && target.life <= 0)
            {
                for (int i = 0; i < 2; i++) 
                {
                    target.NPCLoot();
                }
            }

            
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, vel.X, vel.Y, 100, default, Main.rand.NextFloat(1.3f, 2.2f));
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }

            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(9f, 9f);
                Color redColor = Color.Lerp(Color.Red, new Color(255, 60, 60), Main.rand.NextFloat());
                new Spark(target.Center, vel, redColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.8f, 1.4f), 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
            }
        }

        private void SpawnDustEffects()
        {
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 150, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawChain();
            DrawRedTrail();
            return true;
        }

        private void DrawRedTrail()
        {
            if (trail.Count < 2) return;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect pixelShader = Main.pixelShader;

            List<Vector2> reverseTrail = new List<Vector2>(trail);
            reverseTrail.Reverse(); 

            TrailRenderer.Draw(
                pixelShader, 
                reverseTrail, 
                t => MathHelper.Lerp(6f, 0f, t), 
                t => Color.Red * (1f - t) * 0.8f, 
                HealShaders.HealNoise 
            );

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawChain()
        {
            Texture2D chainTexture;
            try
            {
                chainTexture = ModContent.Request<Texture2D>("jarkos/projectiles/melee/other_melee/spinetion_chain", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                chainTexture = TextureAssets.Chain.Value;
            }
            Vector2 perp = launchDir.RotatedBy(MathHelper.PiOver2);
            float progress = timer / totalDuration;
            float distProgress;
            if (progress < 0.4f)
            {
                distProgress = EaseOutQuad(progress / 0.4f);
            }
            else
            {
                distProgress = 1f - EaseInQuad((progress - 0.4f) / 0.6f);
            }
            Vector2 targetPos = Owner.MountedCenter + launchDir * (targetRange * distProgress);
            float currentAmplitude = maxWaveAmplitude * (targetRange / 450f);
            float wavePhase = progress * MathHelper.TwoPi * waveFrequency;
            float distance = Vector2.Distance(Owner.MountedCenter, Projectile.Center);
            int segmentHeight = chainTexture.Height;
            int numSegments = Math.Max((int)(distance / segmentHeight), 1);
            Vector2 origin = new Vector2(chainTexture.Width / 2f, segmentHeight / 2f);
            for (int i = 0; i < numSegments; i++)
            {
                float p = i / (float)numSegments;
                Vector2 segBase = Vector2.Lerp(Owner.MountedCenter, targetPos, p);
                float segAmp = currentAmplitude * (float)Math.Sin(progress * MathHelper.Pi) * p;
                
                if (Projectile.ai[2] != -1)
                {
                    segAmp *= 1f - p * distProgress;
                }

                float segWave = (float)Math.Sin(wavePhase * p - timer * 0.2f) * segAmp * waveDirectionSign;
                Vector2 drawPos = segBase + perp * segWave;
                float nextP = (i + 1) / (float)numSegments;
                Vector2 nextSegBase = Vector2.Lerp(Owner.MountedCenter, targetPos, nextP);
                float nextSegAmp = currentAmplitude * (float)Math.Sin(progress * MathHelper.Pi) * nextP;
                
                if (Projectile.ai[2] != -1)
                {
                    nextSegAmp *= 1f - nextP * distProgress;
                }

                float nextSegWave = (float)Math.Sin(wavePhase * nextP - timer * 0.2f) * nextSegAmp * waveDirectionSign;
                Vector2 nextDrawPos = nextSegBase + perp * nextSegWave;
                float rotation = (nextDrawPos - drawPos).ToRotation() + MathHelper.PiOver2;
                
                Color color = Lighting.GetColor((int)(drawPos.X / 16f), (int)(drawPos.Y / 16f));
                color = Color.Lerp(color, Color.Red, 0.5f);
                Main.EntitySpriteDraw(chainTexture, drawPos - Main.screenPosition, null, color, rotation, origin, 1f, SpriteEffects.None, 0);
            }
        }
        
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }
    }
}
