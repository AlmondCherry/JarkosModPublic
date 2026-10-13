using jarkos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;

namespace Jarkos.projectiles.mecha
{
    public class Energy_knife_proj01 : ModProjectile
    {
        private const int TrailLength = 20;
        private float pulseTimer = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            pulseTimer += 0.18f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.ai[1]++;
            if (Projectile.ai[1] > 25f)
            {
                Projectile.velocity.Y += 0.1f;
            }

            float pulse = 0.5f + (float)Math.Sin(pulseTimer * 2f) * 0.2f;
            Lighting.AddLight(Projectile.Center, 0.3f * pulse, 0.7f * pulse, 1.0f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            var globalNPC = target.GetGlobalNPC<EnergyChargeGlobalNPC>();
            globalNPC.Electrify(target, Projectile.owner, Projectile.damage);

            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.3f, Volume = 0.7f }, target.Center);
                for (int i = 0; i < 8; i++)
                {
                    Vector2 particleVel = Main.rand.NextVector2Circular(5f, 5f);
                    new Dust1_particle(target.Center, particleVel, Color.Cyan, Main.rand.Next(18, 30), Main.rand.NextFloat(0.25f, 0.4f), 1.0f, 0.92f).Spawn();
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
                for (int i = 0; i < 10; i++)
                {
                    Vector2 particleVel = Main.rand.NextVector2Circular(5f, 5f);
                    new Dust1_particle(Projectile.Center, particleVel, Color.Cyan, Main.rand.Next(15, 30), Main.rand.NextFloat(0.25f, 0.4f), 1.0f, 0.92f).Spawn();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawEnergyTrail();
            DrawGlowingKnife(lightColor);
            return false;
        }

        private void DrawEnergyTrail()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                float trailAlpha = progress * 0.5f;
                Color trailColor = new Color(60, 180, 255) * trailAlpha;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                sb.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, Projectile.scale * (0.5f + progress * 0.5f), SpriteEffects.None, 0f);
            }
        }

        private void DrawGlowingKnife(Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 texOrigin = texture.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(pulseTimer * 2f) * 0.12f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi / 4f * i + pulseTimer;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f * pulse;
                Color glowColor = new Color(80, 180, 255) * 0.25f;
                sb.Draw(texture, drawPos + offset, null, glowColor, Projectile.rotation, texOrigin, Projectile.scale * pulse * 1.1f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Color drawColor = Color.Lerp(lightColor, new Color(180, 220, 255), 0.4f);
            sb.Draw(texture, drawPos, null, drawColor, Projectile.rotation, texOrigin, Projectile.scale, SpriteEffects.None, 0f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(200, 230, 255, 180);
        }
    }

    #region Electrified GlobalNPC
    public class EnergyChargeGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int electrifiedTimer = 0;
        public int lightningTimer = 0;
        public int ownerPlayer = -1;
        public int baseDamage = 0;
        private const int ElectrifiedDuration = 300;
        private const float ChainRange = 400f;

        public void Electrify(NPC npc, int player, int damage)
        {
            ownerPlayer = player;
            baseDamage = Math.Max(baseDamage, damage);
            electrifiedTimer = ElectrifiedDuration;
            npc.netUpdate = true;
        }

        public override void ResetEffects(NPC npc)
        {
            if (electrifiedTimer > 0)
            {
                electrifiedTimer--;
            }
        }

        public override void AI(NPC npc)
        {
            if (electrifiedTimer <= 0) return;

            lightningTimer++;


            if (lightningTimer % 60 == 0 && ownerPlayer >= 0 && ownerPlayer < Main.maxPlayers)
            {
                Player player = Main.player[ownerPlayer];
                if (player.active)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC other = Main.npc[i];
                        if (other.active && other.CanBeChasedBy() && other.whoAmI != npc.whoAmI)
                        {
                            var otherGlobal = other.GetGlobalNPC<EnergyChargeGlobalNPC>();
                            if (otherGlobal.electrifiedTimer > 0 && npc.whoAmI < other.whoAmI)
                            {
                                float dist = Vector2.Distance(npc.Center, other.Center);
                                if (dist < ChainRange)
                                {
                                    int chainDmg = (int)(baseDamage * 0.75f);

                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        player.ApplyDamageToNPC(other, chainDmg, 1f, 0, false);
                                        player.ApplyDamageToNPC(npc, chainDmg, 1f, 0, false);
                                    }


                                    if (Main.netMode != NetmodeID.Server)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.5f, Pitch = 0.2f }, npc.Center);
                                        Projectile.NewProjectile(
                                            player.GetSource_FromThis(),
                                            other.Center,
                                            Vector2.Zero,
                                            ModContent.ProjectileType<Energy_knife_proj02>(),
                                            0,
                                            0f,
                                            ownerPlayer,
                                            ai0: npc.Center.X,
                                            ai1: npc.Center.Y
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            }


            if (Main.rand.NextBool(3))
            {
                Vector2 pos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
                new Spark(pos, Main.rand.NextVector2Circular(2f, 2f), Color.Cyan, Main.rand.Next(15, 25), Main.rand.NextFloat(0.3f, 0.45f), 1f, 0.93f).Spawn();
            }

            Lighting.AddLight(npc.Center, 0.15f, 0.45f, 0.9f);
        }
    }
    #endregion
}
