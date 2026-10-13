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
	public class Pencal_knife : ModProjectile
	{
        private enum DaggerState
        {
            Flying = 0, Slowing = 1, Piercing = 2
        }
        private DaggerState CurrentState
        {
            get => (DaggerState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        private float StateTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private int hitEnemyIndex
        {
            get => (int)Projectile.ai[2] - 1;
            set => Projectile.ai[2] = value + 1;
        }
        private DaggerState oldState = DaggerState.Flying;
        private List<Vector2> trailPositions = new List<Vector2>();
        private List<float> trailRotations = new List<float>();
        private const int MaxTrailLength = 25;
        private Vector2 savedDirection = Vector2.Zero;
        private const float SlowDownDuration = 18f;
        private const float PierceSpeed = 32f;
        private const float SearchRadius = 600f;
        private const float DamageMultiplier = 1.6f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            UpdateTrailPositions();

            if (CurrentState != oldState)
            {
                if (CurrentState == DaggerState.Slowing)
                {
                    savedDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                }
                else if (CurrentState == DaggerState.Piercing)
                {
                    SoundEngine.PlaySound(SoundID.Item66 with { Pitch = 0.3f, Volume = 0.9f }, Projectile.Center);
                    CreatePierceEffect();
                }
                oldState = CurrentState;
            }

            switch (CurrentState)
            {
                case DaggerState.Flying:
                    AI_Flying();
                    break;
                case DaggerState.Slowing:
                    AI_Slowing();
                    break;
                case DaggerState.Piercing:
                    AI_Piercing();
                    break;
            }
            if (Projectile.velocity.Length() > 0.1f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
            CreateEffects();
        }
        private void UpdateTrailPositions()
        {
            trailPositions.Insert(0, Projectile.Center);
            trailRotations.Insert(0, Projectile.rotation);
            while (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
                trailRotations.RemoveAt(trailRotations.Count - 1);
            }
        }
        private void AI_Flying()
        {
            Projectile.velocity.Y += 0.08f;
            if (Projectile.velocity.Y > 18f)
                Projectile.velocity.Y = 18f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.15f, 0.5f));
        }
        private void AI_Slowing()
        {
            StateTimer++;
            float slowFactor = MathHelper.Lerp(0.92f, 0.75f, StateTimer / SlowDownDuration);
            Projectile.velocity *= slowFactor;
            float pulse = 0.5f + 0.5f * (float)Math.Sin(StateTimer * 0.5f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f + pulse * 0.4f, 0.2f, 0.8f + pulse * 0.2f));
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(10, 10), 20, 20, DustID.WhiteTorch, dustVel.X, dustVel.Y, 150, default, 1.3f);
                dust.noGravity = true;
                dust.velocity = (Projectile.Center - dust.position) * 0.1f;
            }
            if (StateTimer >= SlowDownDuration)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    TransitionToPiercing();
                    Projectile.netUpdate = true;
                }
            }
        }
        private void TransitionToPiercing()
        {
            CurrentState = DaggerState.Piercing;
            StateTimer = 0;
            NPC target = FindBestTarget();
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(savedDirection);
                Projectile.velocity = direction * PierceSpeed;
            }
            else
            {
                Projectile.velocity = savedDirection * PierceSpeed;
            }
            Projectile.damage = (int)(Projectile.damage * DamageMultiplier);
        }
        private void AI_Piercing()
        {
            StateTimer++;
            float intensity = 1f - (StateTimer / 60f) * 0.3f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.9f * intensity, 0.4f * intensity, 1.0f * intensity));
            if (StateTimer > 40)
            {
                Projectile.velocity *= 0.97f;
            }
            if (Projectile.velocity.Length() < 1.5f && StateTimer > 20)
            {
                Projectile.Kill();
            }
            if (StateTimer < 10)
            {
                Projectile.tileCollide = false;
            }
            else
            {
                Projectile.tileCollide = true;
            }
        }
        private NPC FindBestTarget()
        {
            NPC bestTarget = null;
            float bestScore = float.MaxValue;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy() || npc.dontTakeDamage)
                    continue;
                if (npc.whoAmI == hitEnemyIndex)
                    continue;
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance > SearchRadius)
                    continue;
                Vector2 toNpc = (npc.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float dotProduct = Vector2.Dot(savedDirection, toNpc);
                float score = distance - (dotProduct * 200f);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = npc;
                }
            }
            return bestTarget;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CurrentState == DaggerState.Flying)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    CurrentState = DaggerState.Slowing;
                    StateTimer = 0;
                    hitEnemyIndex = target.whoAmI;
                    Projectile.netUpdate = true;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity, ModContent.ProjectileType<Pencal_knife_hit_visual>(), 0, 0f, Projectile.owner, 0f);
                }
            }
            else if (CurrentState == DaggerState.Piercing)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity, ModContent.ProjectileType<Pencal_knife_hit_visual>(), 0, 0f, Projectile.owner, 1f);
                }
            }
        }
        private void CreateEffects()
        {
            int dustChance = CurrentState switch
            {
                DaggerState.Flying => 4, DaggerState.Slowing => 2, DaggerState.Piercing => 1, _ => 4
            };
            if (Main.rand.NextBool(dustChance))
            {
                int dustType = CurrentState == DaggerState.Piercing
                    ? DustID.WhiteTorch
                    : DustID.WhiteTorch;
                float scale = CurrentState == DaggerState.Piercing ? 1.6f : 1.1f;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, scale);
                dust.noGravity = true;
            }
        }
        private void CreateHitEffect(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(position - new Vector2(8, 8), 16, 16, DustID.WhiteTorch, velocity.X, velocity.Y, 100, default, 1.4f);
                dust.noGravity = true;
            }
        }
        private void CreatePierceEffect()
        {
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi / 24f * i;
                Vector2 velocity = angle.ToRotationVector2() * 6f;
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.WhiteTorch, velocity.X, velocity.Y, 50, default, 1.8f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Vector2 pos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * i * 8;
                Dust dust = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.WhiteTorch, 0, 0, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.1f;
            }
        }
        private void CreatePierceHitEffect(Vector2 position)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 8f);
                Dust dust = Dust.NewDustDirect(position - new Vector2(8, 8), 16, 16, DustID.WhiteTorch, velocity.X, velocity.Y, 80, default, 1.6f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawGlow();
            DrawDagger(lightColor);
            return false;
        }
        private void DrawTrail()
        {
            if (trailPositions.Count < 2)
                return;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * (1f - progress);
                float baseWidth = CurrentState switch
                {
                    DaggerState.Piercing => 14f, DaggerState.Slowing => 10f, _ => 6f
                };
                float width = MathHelper.Lerp(baseWidth, 1f, progress);
                Color trailColor = GetTrailColor(progress) * alpha;
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 0.5f)
                    continue;
                float rotation = direction.ToRotation();
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                if (CurrentState == DaggerState.Piercing && i < 8)
                {
                    Color coreColor = Color.White * alpha * 0.6f;
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, width * 0.4f), SpriteEffects.None, 0f);
                }
            }
        }
        private Color GetTrailColor(float progress)
        {
            return CurrentState switch
            {
                DaggerState.Piercing => Color.Lerp(new Color(255, 255, 255), new Color(255, 255, 255), progress), DaggerState.Slowing => Color.Lerp(new Color(255, 255, 255), new Color(255, 255, 255), progress), _ => Color.Lerp(new Color(255, 255, 255), new Color(255, 255, 255), progress)
            };
        }
        private void DrawGlow()
        {
            if (CurrentState == DaggerState.Flying)
                return;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;
            float glowIntensity = CurrentState == DaggerState.Piercing ? 0.7f : 0.4f;
            float glowScale = CurrentState == DaggerState.Piercing ? 1.4f : 1.2f;
            Color glowColor = new Color(255, 255, 255, 0) * glowIntensity;
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6f * i + Main.GameUpdateCount * 0.05f;
                Vector2 offset = angle.ToRotationVector2() * 4f;
                Main.EntitySpriteDraw(texture, position + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale * glowScale, SpriteEffects.None, 0);
            }
        }
        private void DrawDagger(Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color drawColor = CurrentState == DaggerState.Piercing
                ? Color.White
                : lightColor;
            Main.EntitySpriteDraw(texture, position, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with
            {
                Pitch = -0.5f, Volume = 0.6f
            }, Projectile.Center);
            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(10, 10), 20, 20, DustID.WhiteTorch, velocity.X, velocity.Y, 100, default, 1.5f);
                dust.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return CurrentState == DaggerState.Piercing
                ? Color.White
                : null;
        }
    }

    public class Pencal_knife_hit_visual : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }
        public override void AI()
        {
            if (Projectile.timeLeft == 2 && Main.netMode != NetmodeID.Server)
            {
                if (Projectile.ai[0] == 0f) 
                {
                    SoundEngine.PlaySound(SoundID.Item100 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                        Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.WhiteTorch, velocity.X, velocity.Y, 100, default, 1.4f);
                        dust.noGravity = true;
                    }
                }
                else 
                {
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.4f, Volume = 0.8f }, Projectile.Center);
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 8f);
                        Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.WhiteTorch, velocity.X, velocity.Y, 80, default, 1.6f);
                        dust.noGravity = true;
                    }
                }
            }
        }
    }
}
