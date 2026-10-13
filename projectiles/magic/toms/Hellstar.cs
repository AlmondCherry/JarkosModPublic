using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.buffs.debuff;
using jarkos.systems;
using Terraria.DataStructures;
using jarkos.particles;

namespace Jarkos.projectiles.magic.toms
{
    public class Hellstar : ModProjectile
    {
        private const int TrailLength = 20;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private bool trailInitialized = false;

        private float LiveTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        private float ColorOffset
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.4f;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                ColorOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Projectile.netUpdate = true;
            }
        }

        public override void AI()
        {
            LiveTimer++;

            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
                trailInitialized = true;
            }

            for (int i = TrailLength - 1; i > 0; i--)
                trailPositions[i] = trailPositions[i - 1];
            trailPositions[0] = Projectile.Center;

            Projectile.rotation += 0.3f * Math.Sign(Projectile.velocity.X);

            float wave = (float)Math.Sin(LiveTimer * 0.15f + ColorOffset) * 0.8f;
            Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
            Projectile.position += perpendicular * wave;

            if (LiveTimer > 15)
            {
                NPC closest = FindClosestNPC(300f);
                if (closest != null)
                {
                    Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.03f);
                }
            }

            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.OrangeTorch;
                Dust trail = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 0, 0, dustType, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f, 100, default, Main.rand.NextFloat(0.8f, 1.5f));
                trail.noGravity = true;
                trail.fadeIn = 1f;
            }

            if (Main.rand.NextBool(6))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 50, default, 0.8f);
                Spark.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.4f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            target.AddBuff(ModContent.BuffType<Hellfire_debuff>(), 180);


            List<NPC> validTargets = new List<NPC>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0 && npc.whoAmI != target.whoAmI)
                {
                    if (Vector2.Distance(target.Center, npc.Center) <= 750f)
                    {
                        validTargets.Add(npc);
                    }
                }
            }

            if (validTargets.Count > 0 && Projectile.owner == Main.myPlayer)
            {
                NPC randomTarget = validTargets[Main.rand.Next(validTargets.Count)];


                int laserDamage = (int)(damageDone * 1.20f);

                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<TomOfHellLaser>(),
                    laserDamage,
                    0f,
                    Projectile.owner,
                    ai0: target.whoAmI,
                    ai1: randomTarget.whoAmI
                );
            }

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    Color pColor = Main.rand.NextBool() ? new Color(255, 140, 20) : new Color(255, 60, 10);
                    new Raindrop_particle(target.Center, vel, pColor, 18, Main.rand.NextFloat(0.1f, 0.3f), 1f).Spawn();
                }

                if (Main.GameUpdateCount % 4 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item45 with { Volume = 0.15f, Pitch = Main.rand.NextFloat(0.2f, 0.8f) }, target.Center);
                }
            }
        }

        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    new Raindrop_particle(Projectile.Center, vel, new Color(255, 100, 10), 12, Main.rand.NextFloat(0.1f, 0.3f), 1f).Spawn();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            List<Vector2> points = new List<Vector2>();
            Vector2 halfSize = Projectile.Size * 0.5f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    points.Add(Projectile.oldPos[i] + halfSize);
                }
            }

            if (points.Count >= 2 && HealShaders.Stream != null)
            {
                sb.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                effect.Parameters["uOpacity"]?.SetValue(1f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 60, 10).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 200, 80).ToVector4());

                TrailRenderer.Draw(
                    effect,
                    points,
                    t => 18f * (1f - t) + 0.5f,
                    t => Color.White * (1f - t),
                    HealShaders.HealNoise
                );

                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            DrawStarGlow();
            DrawStar(lightColor);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        private void DrawStarGlow()
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Vector2 glowOrigin = new Vector2(magicPixel.Width / 2f, magicPixel.Height / 2f);
            float pulse = 1f + (float)Math.Sin(LiveTimer * 0.18f + ColorOffset) * 0.25f;

            Color[] glowColors = {
                new Color(255, 255, 200), new Color(255, 180, 60), new Color(255, 100, 20), new Color(200, 40, 5)
            };

            for (int layer = 0; layer < 4; layer++)
            {
                float size = (12f + layer * 10f) * pulse;
                float layerAlpha = 0.2f / (1f + layer * 0.6f);
                Vector2 glowScale = new Vector2(size / magicPixel.Width, size / magicPixel.Height);
                Main.EntitySpriteDraw(magicPixel, Projectile.Center - Main.screenPosition, null, glowColors[layer] * layerAlpha, Projectile.rotation * 0.5f, glowOrigin, glowScale, SpriteEffects.None, 0);
            }

            for (int ray = 0; ray < 4; ray++)
            {
                float angle = MathHelper.PiOver4 * ray + LiveTimer * 0.05f + ColorOffset;
                float rayLength = (20f + (float)Math.Sin(LiveTimer * 0.2f + ray) * 8f) * pulse;
                float rayWidth = 2f;
                Vector2 rayScale = new Vector2(rayLength / magicPixel.Width, rayWidth / magicPixel.Height);
                Color rayColor = new Color(255, 200, 80) * 0.15f;
                Main.EntitySpriteDraw(magicPixel, Projectile.Center - Main.screenPosition, null, rayColor, angle, new Vector2(0, magicPixel.Height / 2f), rayScale, SpriteEffects.None, 0);
            }
        }

        private void DrawStar(Color lightColor)
        {
            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float brightness = 1.2f;
            Color drawColor = lightColor * brightness;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            float addPulse = 0.35f + (float)Math.Sin(LiveTimer * 0.15f + ColorOffset) * 0.15f;
            Color additiveColor = new Color(255, 140, 40) * addPulse;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, additiveColor, Projectile.rotation, origin, Projectile.scale * 1.08f, SpriteEffects.None, 0);
        }
    }

    #region laser ToH
    public class TomOfHellLaser : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/Empty_proj";

        private Vector2 startPos;
        private Vector2 endPos;
        private bool hasHit = false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 16;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            int startNPCIndex = (int)Projectile.ai[0];
            int targetNPCIndex = (int)Projectile.ai[1];

            if (startNPCIndex >= 0 && startNPCIndex < Main.maxNPCs && Main.npc[startNPCIndex].active)
            {
                startPos = Main.npc[startNPCIndex].Center;
            }

            if (targetNPCIndex >= 0 && targetNPCIndex < Main.maxNPCs && Main.npc[targetNPCIndex].active)
            {
                endPos = Main.npc[targetNPCIndex].Center;
            }

            if (startPos != Vector2.Zero && endPos != Vector2.Zero)
            {
                Projectile.Center = (startPos + endPos) * 0.5f;
            }


            if (!hasHit && Main.myPlayer == Projectile.owner)
            {
                hasHit = true;

                if (targetNPCIndex >= 0 && targetNPCIndex < Main.maxNPCs)
                {
                    NPC targetNPC = Main.npc[targetNPCIndex];
                    if (targetNPC.active && !targetNPC.friendly && !targetNPC.dontTakeDamage && targetNPC.life > 0)
                    {
                        NPC.HitInfo hit = new NPC.HitInfo
                        {
                            Damage = Projectile.damage,
                            Knockback = 1f,
                            HitDirection = (endPos.X > startPos.X) ? 1 : -1,
                            Crit = Main.rand.NextBool(4)
                        };
                        targetNPC.StrikeNPC(hit);
                        targetNPC.AddBuff(ModContent.BuffType<Hellfire_debuff>(), 180);
                    }
                }

                SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.45f, Pitch = 0.2f }, startPos);
            }

            if (Main.netMode != NetmodeID.Server && startPos != Vector2.Zero && endPos != Vector2.Zero)
            {
                for (int i = 0; i < 3; i++)
                {
                    float lerpFactor = Main.rand.NextFloat();
                    Vector2 sparkPos = Vector2.Lerp(startPos, endPos, lerpFactor);
                    Dust dust = Dust.NewDustDirect(sparkPos, 0, 0, DustID.OrangeTorch, 0f, 0f, 100, default, 1.2f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (startPos == Vector2.Zero || endPos == Vector2.Zero) return false;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;

            Vector2 diff = endPos - startPos;
            float length = diff.Length();
            float rotation = diff.ToRotation();
            float alpha = Projectile.timeLeft / 16f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 origin = new Vector2(0f, magicPixel.Height / 2f);


            float redWidth = 10f * alpha;
            Vector2 redScale = new Vector2(length / magicPixel.Width, redWidth / magicPixel.Height);
            Color redColor = new Color(255, 30, 20) * alpha;
            sb.Draw(magicPixel, startPos - Main.screenPosition, null, redColor, rotation, origin, redScale, SpriteEffects.None, 0f);


            float whiteWidth = 4.5f * alpha;
            Vector2 whiteScale = new Vector2(length / magicPixel.Width, whiteWidth / magicPixel.Height);
            Color whiteColor = Color.White * alpha;
            sb.Draw(magicPixel, startPos - Main.screenPosition, null, whiteColor, rotation, origin, whiteScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
    #endregion
}
