using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
using ReLogic.Content;
using jarkos.particles;
using jarkos.systems;

namespace jarkos.projectiles.bullet
{
	public class Neutron_arrow_proj : ModProjectile
	{
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int TRAIL_LENGTH = 8;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 1200;
            Projectile.tileCollide = true;
            Projectile.arrow = true;
            Projectile.aiStyle = 0;
            Projectile.extraUpdates = 1;
            Projectile.ArmorPenetration = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            UpdateTrail();
            Projectile.velocity.Y += 0.08f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.WhiteTorch, Projectile.velocity * 0.1f, 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.45f, 0.5f);
        }

        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            while (trailPositions.Count > TRAIL_LENGTH)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var chargeNPC = target.GetGlobalNPC<NeutronChargeGlobalNPC>();
            chargeNPC.charge += 20f;
            chargeNPC.idleTicks = 0;
            if (chargeNPC.charge > 200f) chargeNPC.charge = 200f;

            chargeNPC.arrowDamage = Projectile.damage;
            chargeNPC.ownerIndex = Projectile.owner;

            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Main.player[Projectile.owner].active)
            {
                chargeNPC.heldItemDamage = Main.player[Projectile.owner].HeldItem.damage;
            }

            if (chargeNPC.charge >= 200f && !chargeNPC.explosionTriggered)
            {
                chargeNPC.explosionTriggered = true;
                chargeNPC.explosionPhaseTimer = 0;
            }

            for (int i = 0; i < 12; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, speed, 0, default, 1.2f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.7f, Pitch = 0.4f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawArrowWithGlow(lightColor);
            return false;
        }

        private void DrawTrail()
        {
            if (trailPositions.Count < 2) return;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float t0 = (float)i / trailPositions.Count;
                Vector2 start = trailPositions[i];
                Vector2 end = trailPositions[i + 1];

                float glowWidth = MathHelper.Lerp(14f, 1f, t0);
                float coreWidth = MathHelper.Lerp(4.5f, 0.5f, t0);

                Color glowColor = new Color(180, 190, 205) * ((1f - t0) * 0.45f);
                Color coreColor = Color.Lerp(Color.White, new Color(220, 225, 235), t0) * ((1f - t0) * 0.85f);

                DrawSegmentLine(sb, pixel, start, end, glowWidth, glowColor);
                DrawSegmentLine(sb, pixel, start, end, coreWidth, coreColor);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawArrowWithGlow(Color lightColor)
        {
            Texture2D arrowTexture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = arrowTexture.Size() * 0.5f;

            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;
            Color silverGlow = new Color(200, 205, 215) * 0.55f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(glowTex, drawPos, null, silverGlow, 0f, glowOrigin, 0.45f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(arrowTexture, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(2.5f, 2.5f), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }

        private static void DrawSegmentLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            if (length < 0.1f) return;

            float rotation = diff.ToRotation();
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);

            sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }

    public class NeutronChargeGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public float charge = 0f;
        public bool explosionTriggered = false;
        public int explosionPhaseTimer = 0;
        public float barOpacity = 0f;
        public int idleTicks = 0;
        public int arrowDamage = 0;
        public int heldItemDamage = 0;
        public int ownerIndex = -1;

        public override void ResetEffects(NPC npc)
        {
            if (charge <= 0f && !explosionTriggered)
            {
                barOpacity = MathHelper.Lerp(barOpacity, 0f, 0.1f);
            }
        }

        public override void AI(NPC npc)
        {
            if (charge > 0f && !explosionTriggered)
            {
                barOpacity = MathHelper.Lerp(barOpacity, 1f, 0.12f);
                idleTicks++;

                if (idleTicks >= 60 && idleTicks % 60 == 0)
                {
                    int secondsIdle = idleTicks / 60;
                    int decayAmount = secondsIdle switch
                    {
                        1 => 2,
                        2 => 3,
                        3 => 4,
                        4 => 5,
                        _ => 10
                    };

                    charge -= decayAmount;
                    if (charge <= 0f)
                    {
                        charge = 0f;
                        idleTicks = 0;
                    }
                }
            }


            if (charge >= 200f && !explosionTriggered)
            {
                explosionTriggered = true;
                explosionPhaseTimer = 0;
                charge = 200f;
            }


            if (explosionTriggered)
            {
                barOpacity = MathHelper.Lerp(barOpacity, 0f, 0.08f);
                explosionPhaseTimer++;


                if (explosionPhaseTimer <= 90)
                {
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 offset = Main.rand.NextVector2CircularEdge(160f, 160f) * Main.rand.NextFloat(0.8f, 1.3f);
                        Vector2 particlePos = npc.Center + offset;
                        Vector2 targetPos = npc.Center;
                        Color silverColor = Color.Lerp(new Color(200, 205, 215), Color.White, Main.rand.NextFloat());

                        new Stretching_particle(particlePos, targetPos, silverColor, lifetime: 16, initialLength: 35f, initialThickness: 12f, pixelationAmount: 0.1f).Spawn();
                    }

                    if (Main.rand.NextBool(3))
                    {
                        Dust d = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(80, 80), DustID.WhiteTorch, (npc.Center - (npc.Center + Main.rand.NextVector2Circular(80, 80))).SafeNormalize(Vector2.Zero) * 6f, 0, default, 1.4f);
                        d.noGravity = true;
                    }
                }
                else if (explosionPhaseTimer == 91)
                {

                    TriggerSupernovaExplosion(npc);
                    charge = 0f;
                    explosionTriggered = false;
                    explosionPhaseTimer = 0;
                }
            }
        }

        private void TriggerSupernovaExplosion(NPC npc)
        {
            SunfateScreenSystem.TriggerContrastEffect();
            if (Main.netMode != NetmodeID.Server)
            {
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake += 5;
            }

            float baseDmg = 200f;
            float bonusDmg = (heldItemDamage + arrowDamage) * 0.75f;
            int finalExplosionDamage = (int)(baseDmg + bonusDmg);

            Vector2 expCenter = npc.Center;
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 1f, Pitch = -0.3f }, expCenter);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.9f, Pitch = -0.4f }, expCenter);

            new Circular_glow(expCenter, Vector2.Zero, new Color(220, 225, 235), 450, 0.8f, 0.95f).Spawn();

            for (int i = 0; i < 40; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(16f, 16f);
                Dust d = Dust.NewDustPerfect(expCenter, DustID.WhiteTorch, vel, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                d.noGravity = true;
            }
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(14f, 14f);
                Dust d = Dust.NewDustPerfect(expCenter, DustID.Smoke, vel, 100, Color.LightGray, Main.rand.NextFloat(1.2f, 2f));
                d.noGravity = true;
            }

            if (ownerIndex >= 0 && ownerIndex < Main.maxPlayers && Main.player[ownerIndex].active)
            {
                foreach (NPC target in Main.npc)
                {
                    if (target.active && !target.friendly && !target.dontTakeDamage && !target.immortal)
                    {
                        float dist = Vector2.Distance(expCenter, target.Center);
                        if (dist <= 450f)
                        {
                            NPC.HitInfo hit = target.CalculateHitInfo(finalExplosionDamage, 0, false, 0f, DamageClass.Ranged, true);
                            target.StrikeNPC(hit);
                            NetMessage.SendStrikeNPC(target, hit);
                        }
                    }
                }
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (barOpacity <= 0.01f || (charge <= 0f && !explosionTriggered)) return;

            Vector2 barPos = npc.Top - screenPos - new Vector2(0f, 20f);
            int barWidth = 56;
            int barHeight = 8;
            Rectangle bgRect = new Rectangle((int)barPos.X - barWidth / 2, (int)barPos.Y - barHeight / 2, barWidth, barHeight);
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            spriteBatch.Draw(pixel, bgRect, Color.Black * (0.85f * barOpacity));

            float fillRatio = MathHelper.Clamp(charge / 200f, 0f, 1f);
            int fillWidth = (int)((barWidth - 4) * fillRatio);

            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle(bgRect.X + 2, bgRect.Y + 2, fillWidth, barHeight - 4);
                Color barColor = new Color(210, 215, 225) * barOpacity;
                spriteBatch.Draw(pixel, fillRect, barColor);
            }
        }
    }
}
