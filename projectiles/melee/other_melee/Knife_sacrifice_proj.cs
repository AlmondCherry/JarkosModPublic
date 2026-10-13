using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Windows;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;
using Terraria.UI.Chat;
using ReLogic.Content;
using Newtonsoft.Json;
using Terraria.GameContent.UI.States;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using jarkos.projectiles.magic.toms;
using Terraria.Graphics;
using System.Runtime.InteropServices;
using jarkos.systems;
using jarkos.buffs;
using jarkos.particles;
namespace jarkos.projectiles.melee.other_melee
{
	public class Knife_sacrifice_proj : ModProjectile
	{
        private const int MaxTrailLength = 20;
        private const int MaxStuckTime = 150;
        private const int DamageTickInterval = 50;
        private const int DissolveTime = 40;
        private enum DaggerState
        {
            Flying = 0, StuckInEnemy = 1, StuckInTile = 2, Dissolving = 3
        }
        private DaggerState CurrentState
        {
            get => (DaggerState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        private int TargetWhoAmI
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private DaggerState oldState = DaggerState.Flying;
        private Vector2 stuckOffset;
        private float stuckRotation;
        private int stuckTimer;
        private int dissolveTimer;
        private float wobbleAngle;
        private List<TrailPoint> trailPoints = new List<TrailPoint>();
        private struct TrailPoint
        {
            public Vector2 Position;
            public float Rotation;
            public float Width;
            public TrailPoint(Vector2 pos, float rot, float width)
            {
                Position = pos;
                Rotation = rot;
                Width = width;
            }
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            if (CurrentState != oldState)
            {
                if (CurrentState == DaggerState.StuckInEnemy)
                {
                    if (TargetWhoAmI >= 0 && TargetWhoAmI < Main.maxNPCs && Main.npc[TargetWhoAmI].active)
                    {
                        stuckOffset = Projectile.Center - Main.npc[TargetWhoAmI].Center;
                    }
                    stuckRotation = Projectile.rotation;
                    stuckTimer = 0;
                    wobbleAngle = 0;
                    Projectile.penetrate = -1;
                    
                    SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.8f, Pitch = -0.3f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);
                    SpawnBloodBurst(15);
                    SpawnImpactSparks();
                }
                else if (CurrentState == DaggerState.StuckInTile)
                {
                    stuckRotation = Projectile.rotation;
                    stuckTimer = 0;
                    wobbleAngle = 0;
                    SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 vel = -Projectile.velocity.RotatedByRandom(1.0f) * Main.rand.NextFloat(0.1f, 0.5f);
                        Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Iron, vel.X, vel.Y, 50, default, 0.8f);
                    }
                }
                else if (CurrentState == DaggerState.Dissolving)
                {
                    dissolveTimer = 0;
                    SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = Main.rand.NextFloat(0.7f, 1.0f) }, Projectile.Center);
                    SpawnDissolveExplosion();
                }
                oldState = CurrentState;
            }

            switch (CurrentState)
            {
                case DaggerState.Flying:
                    FlyingBehavior();
                    break;
                case DaggerState.StuckInEnemy:
                    StuckInEnemyBehavior();
                    break;
                case DaggerState.StuckInTile:
                    StuckInTileBehavior();
                    break;
                case DaggerState.Dissolving:
                    DissolvingBehavior();
                    break;
            }
        }
        private void FlyingBehavior()
        {
            UpdateTrailPoints();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.velocity.Y += 0.12f;
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y, 18f);
            if (Math.Abs(Projectile.velocity.X) > 0.1f)
                Projectile.velocity.X *= 0.995f;
            SpawnFlyingEffects();
            float speed = Projectile.velocity.Length();
            float lightIntensity = MathHelper.Clamp(speed / 20f, 0.2f, 0.5f);
            Lighting.AddLight(Projectile.Center, 0.4f * lightIntensity, 0.3f * lightIntensity, 0.15f * lightIntensity);
        }
        private void UpdateTrailPoints()
        {
            float currentWidth = 8f * (Projectile.velocity.Length() / 16f);
            currentWidth = MathHelper.Clamp(currentWidth, 4f, 10f);
            trailPoints.Insert(0, new TrailPoint(Projectile.Center, Projectile.rotation, currentWidth));
            while (trailPoints.Count > MaxTrailLength)
            {
                trailPoints.RemoveAt(trailPoints.Count - 1);
            }
        }
        private void SpawnFlyingEffects()
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Center - Projectile.velocity * 0.5f;
                Dust dust = Dust.NewDustDirect(dustPos - new Vector2(4), 8, 8, DustID.GoldFlame, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 0.9f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            if (Main.rand.NextBool(4))
            {
                Vector2 sparkVel = -Projectile.velocity.RotatedByRandom(0.5f) * 0.1f;
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Enchanted_Gold, sparkVel.X, sparkVel.Y, 0, default, 0.6f);
                Spark.noGravity = true;
            }
        }
        private void StuckInEnemyBehavior()
        {
            NPC target = Main.npc[TargetWhoAmI];
            if (!target.active || target.life <= 0)
            {
                StartDissolving();
                return;
            }
            Projectile.Center = target.Center + stuckOffset;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            wobbleAngle += 0.15f;
            Projectile.rotation = stuckRotation + (float)Math.Sin(wobbleAngle) * 0.05f;
            stuckTimer++;
            if (stuckTimer % DamageTickInterval == 0)
            {
                ApplyStuckDamage(target);
            }
            SpawnStuckEffects(target);
            float pulse = 0.3f + 0.15f * (float)Math.Sin(stuckTimer * 0.15f);
            Lighting.AddLight(Projectile.Center, 0.5f * pulse, 0.2f * pulse, 0.1f * pulse);
            if (stuckTimer >= MaxStuckTime)
            {
                StartDissolving();
            }
            FadeOutTrail();
        }
        private void ApplyStuckDamage(NPC target)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int tickDamage = Math.Max(Projectile.damage / 4, 9);
                NPC.HitInfo hitInfo = new NPC.HitInfo
                {
                    Damage = tickDamage, Knockback = 0f, HitDirection = 0, Crit = Main.rand.Next(100) < Projectile.CritChance
                };
                target.StrikeNPC(hitInfo);
                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
            }
            SoundEngine.PlaySound(SoundID.NPCHit1 with
            {
                Volume = 0.4f, Pitch = 0.3f + Main.rand.NextFloat(0.2f)
            }, Projectile.Center);
            SpawnBloodBurst(6);
        }
        private void SpawnStuckEffects(NPC target)
        {
            if (Main.rand.NextBool(8))
            {
                Dust blood = Dust.NewDustDirect(Projectile.Center - new Vector2(3), 6, 6, DustID.Blood, 0f, Main.rand.NextFloat(1f, 2f), 0, default, 1f);
                blood.noGravity = false;
            }
            if (Main.rand.NextBool(6))
            {
                Dust glow = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), 0, 0, DustID.RedTorch, 0, 0, 100, default, 0.5f);
                glow.noGravity = true;
                glow.fadeIn = 0.5f;
            }
        }
        private void FadeOutTrail()
        {
            if (trailPoints.Count > 0 && stuckTimer % 2 == 0)
            {
                trailPoints.RemoveAt(trailPoints.Count - 1);
            }
        }
        private void StuckInTileBehavior()
        {
            Projectile.velocity = Vector2.Zero;
            stuckTimer++;
            wobbleAngle += 0.1f;
            float wobble = (float)Math.Sin(wobbleAngle) * 0.03f * Math.Max(0, 1f - stuckTimer / 60f);
            Projectile.rotation = stuckRotation + wobble;
            if (Main.rand.NextBool(20))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 0, default, 0.4f);
                Spark.noGravity = true;
            }
            float fade = Math.Max(0, 1f - stuckTimer / 120f);
            Lighting.AddLight(Projectile.Center, 0.2f * fade, 0.15f * fade, 0.05f * fade);
            if (stuckTimer >= 90)
            {
                StartDissolving();
            }
            FadeOutTrail();
        }
        private void StartDissolving()
        {
            if (CurrentState == DaggerState.Dissolving)
                return;
            CurrentState = DaggerState.Dissolving;
        }
        private void SpawnDissolveExplosion()
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust shard = Dust.NewDustDirect(Projectile.Center - new Vector2(4), 8, 8, DustID.Iron, vel.X, vel.Y, 50, default, Main.rand.NextFloat(0.8f, 1.2f));
                shard.noGravity = Main.rand.NextBool(3);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f);
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldCoin, vel.X, vel.Y, 0, default, 0.7f);
                Spark.noGravity = true;
                Spark.fadeIn = 1.3f;
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust magic = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Enchanted_Gold, vel.X, vel.Y, 100, default, 0.9f);
                magic.noGravity = true;
            }
        }
        private void DissolvingBehavior()
        {
            dissolveTimer++;
            float progress = (float)dissolveTimer / DissolveTime;
            Projectile.alpha = (int)(255 * progress);
            Projectile.velocity.Y += 0.4f;
            Projectile.velocity.X *= 0.92f;
            Projectile.rotation += Projectile.velocity.X * 0.05f;
            Projectile.scale = 1f - progress * 0.5f;
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Iron, Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f, 100 + (int)(155 * progress), default, 0.5f * (1f - progress));
            }
            if (dissolveTimer >= DissolveTime)
            {
                Projectile.Kill();
            }
            while (trailPoints.Count > 0)
            {
                trailPoints.RemoveAt(trailPoints.Count - 1);
                if (trailPoints.Count <= DissolveTime - dissolveTimer)
                    break;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CurrentState != DaggerState.Flying)
                return;
            
            CurrentState = DaggerState.StuckInEnemy;
            TargetWhoAmI = target.whoAmI;
            stuckOffset = Projectile.Center - target.Center;
            stuckRotation = Projectile.rotation;
            stuckTimer = 0;
            wobbleAngle = 0;
            Projectile.penetrate = -1;
            Projectile.damage = hit.Damage;

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.netUpdate = true;
            }
        }
        private void SpawnBloodBurst(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = -Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.1f, 0.4f);
                vel += Main.rand.NextVector2Circular(2f, 2f);
                Dust blood = Dust.NewDustDirect(Projectile.Center - new Vector2(4), 8, 8, DustID.Blood, vel.X, vel.Y, 0, default, Main.rand.NextFloat(1.0f, 1.5f));
                blood.noGravity = Main.rand.NextBool(3);
            }
        }
        private void SpawnImpactSparks()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = -Projectile.velocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.2f, 0.6f);
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, vel.X, vel.Y, 0, default, 0.5f);
                Spark.noGravity = true;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Dust gold = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldCoin, vel.X, vel.Y, 0, default, 0.6f);
                gold.noGravity = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (CurrentState != DaggerState.Flying)
                return false;
            
            if (Projectile.owner == Main.myPlayer)
            {
                CurrentState = DaggerState.StuckInTile;
                Projectile.netUpdate = true;
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawAdvancedTrail(spriteBatch);
            DrawGlow(spriteBatch);
            DrawDagger(spriteBatch, lightColor);
            return false;
        }
        private void DrawAdvancedTrail(SpriteBatch spriteBatch)
        {
            if (trailPoints.Count < 2)
                return;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < trailPoints.Count - 1; i++)
            {
                TrailPoint current = trailPoints[i];
                TrailPoint next = trailPoints[i + 1];
                float progress = (float)i / trailPoints.Count;
                Vector2 direction = next.Position - current.Position;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                float width = MathHelper.Lerp(current.Width, 1f, progress);
                float alpha = MathHelper.Lerp(0.9f, 0f, progress);
                alpha *= (CurrentState == DaggerState.Flying) ? 1f : 0.5f;
                Color innerColor, outerColor;
                if (progress < 0.3f)
                {
                    innerColor = Color.Lerp(new Color(255, 230, 150), new Color(255, 180, 80), progress / 0.3f);
                    outerColor = Color.Lerp(new Color(255, 200, 100), new Color(200, 100, 50), progress / 0.3f);
                }
                else if (progress < 0.7f)
                {
                    float t = (progress - 0.3f) / 0.4f;
                    innerColor = Color.Lerp(new Color(255, 180, 80), new Color(180, 60, 30), t);
                    outerColor = Color.Lerp(new Color(200, 100, 50), new Color(100, 30, 20), t);
                }
                else
                {
                    float t = (progress - 0.7f) / 0.3f;
                    innerColor = Color.Lerp(new Color(180, 60, 30), new Color(80, 20, 10), t);
                    outerColor = Color.Lerp(new Color(100, 30, 20), new Color(40, 10, 5), t);
                }
                DrawTrailSegment(spriteBatch, pixel, current.Position, rotation, length, width * 1.5f, outerColor * alpha * 0.4f);
                DrawTrailSegment(spriteBatch, pixel, current.Position, rotation, length, width, outerColor * alpha * 0.7f);
                if (progress < 0.5f)
                {
                    DrawTrailSegment(spriteBatch, pixel, current.Position, rotation, length, width * 0.4f, innerColor * alpha);
                }
                if (i < 3)
                {
                    Color coreColor = new Color(255, 255, 220) * alpha * 0.6f;
                    DrawTrailSegment(spriteBatch, pixel, current.Position, rotation, length, width * 0.2f, coreColor);
                }
            }
        }
        private void DrawTrailSegment(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, float rotation, float length, float width, Color color)
        {
            spriteBatch.Draw(texture, position - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
        }
        private void DrawGlow(SpriteBatch spriteBatch)
        {
            if (CurrentState == DaggerState.Dissolving)
                return;
            Texture2D glowTexture = TextureAssets.Extra[98].Value;
            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            float baseScale = 0.35f * pulse;
            Color glowColor;
            switch (CurrentState)
            {
                case DaggerState.Flying:
                    glowColor = new Color(255, 200, 100) * 0.25f;
                    break;
                case DaggerState.StuckInEnemy:
                    float stuckPulse = 0.3f + 0.2f * (float)Math.Sin(stuckTimer * 0.2f);
                    glowColor = new Color(255, 100, 80) * stuckPulse;
                    baseScale *= 1.2f;
                    break;
                default:
                    glowColor = new Color(255, 200, 100) * 0.15f;
                    break;
            }
            glowColor *= (255 - Projectile.alpha) / 255f;
            spriteBatch.Draw(glowTexture, Projectile.Center - Main.screenPosition, null, glowColor, 0f, glowTexture.Size() / 2f, baseScale * Projectile.scale, SpriteEffects.None, 0f);
        }
        private void DrawDagger(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            float opacity = (255 - Projectile.alpha) / 255f;
            Color drawColor = lightColor * opacity;
            if (CurrentState == DaggerState.StuckInEnemy)
            {
                float glow = 0.3f + 0.1f * (float)Math.Sin(stuckTimer * 0.15f);
                drawColor = Color.Lerp(drawColor, new Color(255, 150, 150), glow);
            }
            if (CurrentState == DaggerState.Flying && Projectile.velocity.Length() > 5f)
            {
                for (int i = 1; i < 4; i++)
                {
                    float afterimageAlpha = 0.3f - i * 0.08f;
                    Vector2 afterimagePos = Projectile.Center - Projectile.velocity * i * 0.5f;
                    spriteBatch.Draw(texture, afterimagePos - Main.screenPosition, null, drawColor * afterimageAlpha, Projectile.rotation, origin, Projectile.scale * (1f - i * 0.05f), SpriteEffects.None, 0f);
                }
            }
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            float opacity = (255 - Projectile.alpha) / 255f;
            return new Color((int)(lightColor.R * opacity), (int)(lightColor.G * opacity), (int)(lightColor.B * opacity), (int)(lightColor.A * opacity));
        }
    }
}
