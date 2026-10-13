using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.systems;

namespace jarkos.projectiles.magic.toms
{
    public class BloodNeedle_tome_proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 220;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;
            Projectile.light = 0.9f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }

        public override void AI()
        {

            if (Projectile.ai[0] == 2f)
            {
                Projectile.velocity.Y += 0.25f;
                Projectile.velocity.X *= 0.95f;
                Projectile.position += Projectile.velocity;
                Projectile.rotation += 0.08f * (Projectile.velocity.X >= 0 ? 1f : -1f);


                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, MathHelper.Clamp(Projectile.timeLeft / 60f, 0f, 1f));

                if (Main.rand.NextBool(5))
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 120, default, 0.9f);
                    d.noGravity = false;
                }
                return;
            }


            if (Projectile.ai[0] == 1f)
            {
                int targetID = (int)Projectile.ai[1];
                if (targetID >= 0 && targetID < Main.maxNPCs)
                {
                    NPC npc = Main.npc[targetID];
                    if (npc.active && !npc.friendly)
                    {
                        Projectile.Center = npc.Center + new Vector2(Projectile.localAI[0], Projectile.localAI[1]);


                        if (Projectile.timeLeft <= 60)
                        {
                            StartFallingOff();
                            return;
                        }

                        if (Projectile.owner == Main.myPlayer)
                        {
                            int stuckCount = 0;
                            for (int i = 0; i < Main.maxProjectiles; i++)
                            {
                                Projectile p = Main.projectile[i];
                                if (p.active && p.type == Projectile.type && (p.ai[0] == 1f || p.ai[0] == 2f) && p.ai[1] == targetID)
                                {
                                    stuckCount++;
                                }
                            }
                            if (stuckCount >= 8)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<BloodSplash_proj>(), Projectile.damage * 2, 8f, Projectile.owner);
                                
                                for (int i = 0; i < Main.maxProjectiles; i++)
                                {
                                    Projectile p = Main.projectile[i];
                                    if (p.active && p.type == Projectile.type && p.ai[1] == targetID)
                                    {
                                        p.Kill();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        StartFallingOff();
                    }
                }
                else
                {
                    StartFallingOff();
                }
            }
            else
            {

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Lighting.AddLight(Projectile.Center, 0.4f, 0.05f, 0.05f);
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, 0, 0, 120, default, 1f);
                    d.noGravity = true;
                    d.velocity = -Projectile.velocity * 0.2f;
                }
            }
        }

        private void StartFallingOff()
        {
            Projectile.ai[0] = 2f;
            Projectile.timeLeft = 60;
            Projectile.extraUpdates = 0;
            Projectile.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(0.5f, 2f));
            Projectile.netUpdate = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] >= 1f) return;
            bool pierce = Main.rand.NextBool();
            if (!pierce || Projectile.penetrate <= 1)
            {
                Projectile.ai[0] = 1f;
                Projectile.ai[1] = target.whoAmI;
                Projectile.localAI[0] = Projectile.Center.X - target.Center.X;
                Projectile.localAI[1] = Projectile.Center.Y - target.Center.Y;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 600;
                Projectile.netUpdate = true;
                Projectile.penetrate = -1;
                Projectile.friendly = false;
                Projectile.tileCollide = false;
                Projectile.extraUpdates = 0;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = 0.5f, Pitch = 0.2f }, target.Center);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 speed = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(4f, 10f);
                    if (speed == Vector2.Zero) speed = Main.rand.NextVector2Circular(5f, 5f);
                    new Spark(target.Center, speed, new Color(255, 20, 20), 20, Main.rand.NextFloat(0.6f, 1.2f), 1f, 0f).Spawn();
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[0] == 0f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, 1.2f);
                    d.noGravity = false;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float alphaFactor = (255 - Projectile.alpha) / 255f;
            if (alphaFactor <= 0.01f) return false;


            if (Projectile.ai[0] == 0f && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f * alphaFactor);
                effect.Parameters["uColor"]?.SetValue((new Color(255, 20, 20) * alphaFactor).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue((new Color(255, 100, 100) * alphaFactor).ToVector4());
                int numPoints = Projectile.oldPos.Length;
                Vector2[] points = new Vector2[numPoints];
                Vector2 half = Projectile.Size * 0.5f;
                for (int i = 0; i < numPoints; i++)
                {
                    points[i] = Projectile.oldPos[i] == Vector2.Zero ? Projectile.Center : Projectile.oldPos[i] + half;
                }
                TrailRenderer.Draw(effect, points, t => 16f * MathF.Pow(1f - t, 0.5f) * MathHelper.Clamp(t * 10f, 0f, 1f), t => Color.White * (1f - t) * alphaFactor, HealShaders.HealNoise);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }


            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            Main.spriteBatch.Draw(texture, drawPos, null, lightColor * alphaFactor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
