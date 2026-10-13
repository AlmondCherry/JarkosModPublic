using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.systems;
using Terraria.GameContent;

namespace jarkos.projectiles.melee.true_melee
{
    public class IgnisiumSword_star : ModProjectile
    {
        public override string Texture => "jarkos/particles/StarParticle";
        
        private List<Vector2> trailPositions = new List<Vector2>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.alpha = 255;
            Projectile.scale = 0.25f;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 3)
            {
                
                return;
            }

            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
            }

            Projectile.rotation += 0.2f * Projectile.direction;
            
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 10)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.SolarFlare, Projectile.velocity * -0.2f, 0, default, 1f);
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.4f, 0.1f);

            Projectile.ai[0]++;

            if (Projectile.ai[0] == 1)
            {
                
                Projectile.ai[1] = Main.rand.NextBool() ? 1f : -1f;
            }

            if (Projectile.ai[0] < 45)
            {
                
                Projectile.velocity = Projectile.velocity.RotatedBy(0.15f * Projectile.ai[1]) * 0.93f;
            }
            else
            {
                
                NPC closest = null;
                float closestDist = 800f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closest = npc;
                        }
                    }
                }

                if (closest != null)
                {
                    Vector2 targetDir = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = MathHelper.Lerp(Projectile.velocity.Length(), 16f, 0.05f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * speed, 0.08f);

                    if (closestDist < 40f && Projectile.timeLeft > 3)
                    {
                        Projectile.Kill();
                    }
                }
                else
                {
                    
                    if (Projectile.velocity.Length() < 12f)
                    {
                        Projectile.velocity *= 1.05f;
                    }
                }
            }
        }

        public override bool? CanDamage()
        {
            if (Projectile.ai[0] < 45 || Projectile.timeLeft == 3)
                return false;
            return base.CanDamage();
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.penetrate == 1)
            {
                
                Projectile.penetrate = -1;
                Projectile.position = Projectile.Center;
                Projectile.width = 150;
                Projectile.height = 150;
                Projectile.Center = Projectile.position;
                Projectile.Damage();
                
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0.1f }, Projectile.Center);
                
                for (int i = 0; i < 40; i++)
                {
                    float angle = MathHelper.TwoPi / 40f * i;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 10f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, vel, 0, default, Main.rand.NextFloat(1f, 2.5f));
                    dust.noGravity = true;
                }
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0, default, 2f);
                    dust.noGravity = false;
                }
                
                
                if (Main.netMode != NetmodeID.Server)
                {
                    Terraria.Graphics.CameraModifiers.PunchCameraModifier modifier = new Terraria.Graphics.CameraModifiers.PunchCameraModifier(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f), 8f, 4f, 20, 800f, Projectile.Name);
                    Main.instance.CameraModifiers.Add(modifier);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft == 3) return false;

            if (trailPositions.Count > 1)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

                Effect effect = HealShaders.Stream;
                if (effect != null && HealShaders.HealNoise != null)
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    effect.Parameters["uOpacity"]?.SetValue(1.0f);
                    effect.Parameters["uColor"]?.SetValue(new Color(255, 150, 20, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(255, 200, 100, 255).ToVector4());
                    TrailRenderer.Draw(effect, trailPositions.ToArray(), t => MathHelper.Lerp(12f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                else
                {
                    Texture2D pixel = TextureAssets.MagicPixel.Value;
                    for (int i = 0; i < trailPositions.Count - 1; i++)
                    {
                        Vector2 pos1 = trailPositions[i] - Main.screenPosition;
                        Vector2 pos2 = trailPositions[i + 1] - Main.screenPosition;
                        float dist = Vector2.Distance(pos1, pos2);
                        float rot = (pos2 - pos1).ToRotation();
                        float progress = (float)i / trailPositions.Count;
                        Color color = new Color(255, 150, 20, 0) * (1f - progress);
                        Main.spriteBatch.Draw(pixel, pos1, new Rectangle(0, 0, 1, 1), color, rot, new Vector2(0, 0.5f), new Vector2(dist, 6f * (1f - progress)), SpriteEffects.None, 0f);
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color drawColor = new Color(255, 150, 40) * ((255 - Projectile.alpha) / 255f);
            
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
