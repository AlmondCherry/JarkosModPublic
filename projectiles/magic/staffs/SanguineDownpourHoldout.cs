using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using jarkos.particles;

namespace jarkos.projectiles.magic.staffs
{
    public class SanguineDownpourHoldout : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/magic/staffs/SanguineDownpour";

        private const int MAX_CHARGE_TIME = 90;
        private ref float ChargeTimer => ref Projectile.ai[0];
        private bool launched = false;
        private bool playedReadySound = false;

        public float CurrentScale => MathHelper.Clamp(0.4f + (ChargeTimer / (float)MAX_CHARGE_TIME) * 2.6f, 0.4f, 3.0f);

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
                Projectile.velocity = aimDir;
                Projectile.netUpdate = true;
                player.ChangeDir(Main.MouseWorld.X > player.MountedCenter.X ? 1 : -1);
            }

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            float rotation = Projectile.velocity.ToRotation();
            player.itemRotation = MathHelper.WrapAngle(rotation - (player.direction == -1 ? MathHelper.Pi : 0));
            Projectile.Center = player.MountedCenter + Projectile.velocity * 30f;
            Projectile.rotation = rotation + MathHelper.PiOver4;

            Vector2 staffTip = player.Center + Projectile.velocity * 38f;


            if (player.channel && !launched)
            {
                if (ChargeTimer < MAX_CHARGE_TIME)
                {
                    ChargeTimer++;
                }

                float scaleProgress = CurrentScale;



                if (Main.rand.NextFloat() < (0.6f + scaleProgress * 0.1f) / 8f)
                {
                    float radius = 35f * scaleProgress;
                    Vector2 particleStart = staffTip + Main.rand.NextVector2Circular(radius, radius);
                    Vector2 particleVel = (staffTip - particleStart) * 0.14f;
                    Dust dust = Dust.NewDustPerfect(particleStart, DustID.Blood, particleVel, 100, default, Main.rand.NextFloat(0.9f, 1.4f));
                    dust.noGravity = true;
                }


                if (Main.rand.NextFloat() < 0.7f + scaleProgress * 0.3f)
                {
                    float radius = 25f + scaleProgress * 20f;
                    Vector2 particleStart = staffTip + Main.rand.NextVector2Circular(radius, radius);
                    int lifetime = Main.rand.Next(6, 12);
                    Vector2 particleVel = (staffTip - particleStart) / lifetime;
                    float randomScale = Main.rand.NextFloat(0.075f, 0.3f);
                    new Electric_particle(particleStart, particleVel, new Color(255, 30, 20), lifetime, randomScale, 1f, 1f).Spawn();
                }


                if (!playedReadySound && scaleProgress >= 1.75f)
                {
                    playedReadySound = true;
                    SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.8f, Pitch = 0.3f }, staffTip);

                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                        Dust d = Dust.NewDustPerfect(staffTip, DustID.Firework_Red, vel, 100, default, 1.1f);
                        d.noGravity = true;
                    }
                }

                if ((int)ChargeTimer % 20 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.4f * (scaleProgress / 3f), Pitch = -0.2f + (scaleProgress / 3f) * 0.4f }, staffTip);
                }

                if ((int)ChargeTimer == MAX_CHARGE_TIME && Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(staffTip + Main.rand.NextVector2Circular(16f, 16f), DustID.Firework_Red, Vector2.Zero, 100, default, 1.2f);
                    d.noGravity = true;
                }

                Lighting.AddLight(staffTip, 0.7f * (scaleProgress / 3f), 0.1f, 0.1f);
            }
            else if (!launched)
            {

                launched = true;


                if (CurrentScale >= 1.75f)
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 launchVel = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * 16f;
                        float finalScale = CurrentScale;

                        int damage = (int)(Projectile.damage * MathHelper.Lerp(0.8f, 2.2f, (finalScale - 0.4f) / 2.6f));

                        Projectile.NewProjectile(
                            null,
                            staffTip,
                            launchVel,
                            ModContent.ProjectileType<SanguineDownpourSphere>(),
                            damage,
                            Projectile.knockBack * finalScale,
                            Projectile.owner,
                            ai0: finalScale
                        );
                    }

                    SoundEngine.PlaySound(SoundID.Item21 with { Volume = 0.8f, Pitch = -0.2f }, staffTip);
                    SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.6f, Pitch = 0.2f }, staffTip);

                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 vel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 8f);
                        Dust d = Dust.NewDustPerfect(staffTip, DustID.Blood, vel, 100, default, Main.rand.NextFloat(1.2f, 2f));
                        d.noGravity = true;
                    }
                }
                else
                {

                    SoundEngine.PlaySound(SoundID.Item16 with { Volume = 0.4f, Pitch = 0.4f }, staffTip);
                    for (int i = 0; i < 8; i++)
                    {
                        Dust d = Dust.NewDustPerfect(staffTip, DustID.Blood, Main.rand.NextVector2Circular(2f, 2f), 100, default, 0.8f);
                        d.noGravity = true;
                    }
                }

                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = Projectile.velocity.X < 0 ? new Vector2(texture.Width * 0.5f, texture.Height * 0.5f) : new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.None : SpriteEffects.None;

            Main.spriteBatch.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);


            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            if (glowTex != null)
            {
                Vector2 staffTip = Projectile.Center + Projectile.velocity * 38f - Main.screenPosition;
                float currentScale = CurrentScale;
                float pulse = 0.95f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.08f;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


                Color outerColor = new Color(255, 30, 20) * 0.85f * pulse;
                Main.spriteBatch.Draw(glowTex, staffTip, null, outerColor, 0f, glowTex.Size() * 0.5f, currentScale * 0.45f * pulse, SpriteEffects.None, 0f);


                Color midColor = new Color(255, 80, 40) * 0.95f * pulse;
                Main.spriteBatch.Draw(glowTex, staffTip, null, midColor, 0f, glowTex.Size() * 0.5f, currentScale * 0.30f * pulse, SpriteEffects.None, 0f);


                Color innerColor = new Color(255, 240, 200) * 0.95f * pulse;
                Main.spriteBatch.Draw(glowTex, staffTip, null, innerColor, 0f, glowTex.Size() * 0.5f, currentScale * 0.16f * pulse, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
