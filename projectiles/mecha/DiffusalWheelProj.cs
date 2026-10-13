using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.particles;
namespace jarkos.projectiles.mecha
{
    public class DiffusalWheelProj : ModProjectile
    {
        public override string Texture => "jarkos/items/armor/panel/DiffusalWheel";
        private bool initialized = false;
        private bool winLeft = false;
        private float totalRotation = 0f;
        private bool hasGivenBuff = false;
        private int repeatParticlesToSpawn = 0;
        private Color repeatColor = Color.White;
        private bool wasLeftPointer = false;
        private List<ClickGlow> clickGlows = new();
        private class ClickGlow
        {
            public float Rotation;
            public int Time;
            public int MaxTime;
            public Color Color;
            public ClickGlow(float rotation, Color color, int maxTime = 20)
            {
                Rotation = rotation;
                Color = color;
                MaxTime = maxTime;
                Time = 0;
            }
        }
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
            Projectile.netImportant = true;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = player.Center;
            for (int i = clickGlows.Count - 1; i >= 0; i--)
            {
                clickGlows[i].Time++;
                if (clickGlows[i].Time >= clickGlows[i].MaxTime)
                {
                    clickGlows.RemoveAt(i);
                }
            }
            if (!hasGivenBuff)
            {
                int attackSpeedBuff = ModContent.BuffType<buffs.buff.DiffusalAttackSpeedBuff>();
                int defenseBuff = ModContent.BuffType<buffs.buff.DiffusalDefenseBuff>();
                if (player.HasBuff(attackSpeedBuff))
                {
                    int index = player.FindBuffIndex(attackSpeedBuff);
                    if (index != -1)
                    {
                        player.buffTime[index] = Math.Max(player.buffTime[index], Projectile.timeLeft + 10);
                    }
                }
                if (player.HasBuff(defenseBuff))
                {
                    int index = player.FindBuffIndex(defenseBuff);
                    if (index != -1)
                    {
                        player.buffTime[index] = Math.Max(player.buffTime[index], Projectile.timeLeft + 10);
                    }
                }
            }
            if (!initialized)
            {
                initialized = true;
                winLeft = Main.rand.NextBool();
                float targetAngle = winLeft ? Main.rand.NextFloat(MathHelper.Pi + 0.3f, MathHelper.TwoPi - 0.3f)
                                            : Main.rand.NextFloat(0.3f, MathHelper.Pi - 0.3f);
                totalRotation = MathHelper.TwoPi * 4 + targetAngle;
            }
            int timer = 150 - Projectile.timeLeft;
            if (timer <= 120)
            {
                float progress = (float)timer / 120f;
                float easedProgress = 1f - (float)Math.Pow(1f - progress, 3);
                Projectile.rotation = totalRotation * easedProgress;
                float normalizedRot = Projectile.rotation % MathHelper.TwoPi;
                if (normalizedRot < 0)
                    normalizedRot += MathHelper.TwoPi;
                bool currentlyLeft = (normalizedRot >= MathHelper.Pi);
                if (currentlyLeft != wasLeftPointer)
                {
                    wasLeftPointer = currentlyLeft;
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/WheelClick") with { Pitch = Main.rand.NextFloat(-0.05f, 0.05f), Volume = 0.8f }, player.Center);
                    Color clickColor = currentlyLeft ? Color.SkyBlue : Color.Orange;
                    clickGlows.Add(new ClickGlow(Projectile.rotation, clickColor, 20));
                }
            }
            else
            {
                Projectile.rotation = totalRotation;
                if (!hasGivenBuff)
                {
                    hasGivenBuff = true;
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/DWheel_Sound"), player.Center);
                    var modPl = player.GetModPlayer<Jarkos_player>();
                    int currentOutcome = winLeft ? 1 : 2;
                    bool isRepeat = false;
                    if (winLeft)
                    {
                        if (player.HasBuff(ModContent.BuffType<buffs.buff.DiffusalAttackSpeedBuff>()) && modPl.lastDiffusalOutcome == 1)
                        {
                            isRepeat = true;
                        }
                    }
                    else
                    {
                        if (player.HasBuff(ModContent.BuffType<buffs.buff.DiffusalDefenseBuff>()) && modPl.lastDiffusalOutcome == 2)
                        {
                            isRepeat = true;
                        }
                    }
                    if (isRepeat)
                    {
                        modPl.diffusalRepeatTimer = 1800;
                    }
                    else
                    {
                        modPl.diffusalRepeatTimer = 0;
                        if (winLeft)
                        {
                            modPl.diffusalBlueStacks = 0;
                        }
                        else
                        {
                            modPl.diffusalOrangeStacks = 0;
                        }
                    }
                    modPl.lastDiffusalOutcome = currentOutcome;
                    Color color = winLeft ? Color.SkyBlue : Color.Orange;
                    Vector2 wheelCenter = player.Top + new Vector2(0f, -65f);
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.15f, 0.15f);
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                        var p = new Stretching_particle_out(wheelCenter, velocity, color, Main.rand.Next(25, 40), Main.rand.NextFloat(50f, 80f), Main.rand.NextFloat(8f, 14f), 0.02f);
                        p.Spawn();
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                        var p = new StarParticle(wheelCenter, velocity, color, Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.8f), 0.8f, Main.rand.NextFloat(-0.1f, 0.1f));
                        p.Spawn();
                    }
                    if (isRepeat)
                    {
                        repeatParticlesToSpawn = 16;
                        repeatColor = color;
                    }
                     if (winLeft)
                    {
                        modPl.diffusalSilhouetteTimer = 300;
                        modPl.diffusalSilhouetteType = 1;
                        player.ClearBuff(ModContent.BuffType<buffs.buff.DiffusalDefenseBuff>());
                        modPl.diffusalOrangeStacks = 0;
                        modPl.diffusalBlueStacks++;
                        player.AddBuff(ModContent.BuffType<buffs.buff.DiffusalAttackSpeedBuff>(), 1800);
                        int index = player.FindBuffIndex(ModContent.BuffType<buffs.buff.DiffusalAttackSpeedBuff>());
                        if (index != -1)
                        {
                            player.buffTime[index] = 1800;
                        }
                        string dmgInfo = isRepeat ? " (+10% Dmg)" : " (+5% Dmg)";
                        CombatText.NewText(player.Hitbox, Color.SkyBlue, $"Attack Speed +{modPl.diffusalBlueStacks * 3}%!{dmgInfo}", false, false);
                    }
                    else
                    {
                        modPl.diffusalSilhouetteTimer = 300;
                        modPl.diffusalSilhouetteType = 2;
                        player.ClearBuff(ModContent.BuffType<buffs.buff.DiffusalAttackSpeedBuff>());
                        modPl.diffusalBlueStacks = 0;
                        modPl.diffusalOrangeStacks++;
                        player.AddBuff(ModContent.BuffType<buffs.buff.DiffusalDefenseBuff>(), 1800);
                        int index = player.FindBuffIndex(ModContent.BuffType<buffs.buff.DiffusalDefenseBuff>());
                        if (index != -1)
                        {
                            player.buffTime[index] = 1800;
                        }
                        string dmgInfo = isRepeat ? " (+10% Dmg)" : " (+5% Dmg)";
                        CombatText.NewText(player.Hitbox, Color.Orange, $"Defense +{modPl.diffusalOrangeStacks}, Regen +{(modPl.diffusalOrangeStacks * 0.5f):F1}/s!{dmgInfo}", false, false);
                    }
                }
            }
            if (repeatParticlesToSpawn > 0)
            {
                repeatParticlesToSpawn--;
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(180f, 260f);
                var p = new Stretching_particle(spawnPos, player.Center, repeatColor, Main.rand.Next(35, 50), Main.rand.NextFloat(70f, 110f), Main.rand.NextFloat(12f, 18f), 0.02f, player);
                p.Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active) return false;
            Texture2D wheelTex = ModContent.Request<Texture2D>("jarkos/items/armor/panel/DiffusalWheel").Value;
            Texture2D arrowTex = ModContent.Request<Texture2D>("jarkos/items/armor/panel/ArrowWheel").Value;
            Vector2 basePosition = player.Top + new Vector2(0f, -40f) - Main.screenPosition;
            int timer = 150 - Projectile.timeLeft;
            float yOffset = 0f;
            float opacity = 1f;
            float scaleX = 1f;
            if (timer <= 20)
            {
                float progress = timer / 20f;
                float eased = 1f - (float)Math.Pow(1f - progress, 3);
                yOffset = MathHelper.Lerp(40f, 0f, eased);
                opacity = eased;
            }
            else if (timer > 120)
            {
                float progress = MathHelper.Clamp((timer - 120) / 30f, 0f, 1f);
                float eased = 1f - (float)Math.Pow(1f - progress, 3);
                yOffset = MathHelper.Lerp(0f, -60f, eased);
                opacity = 1f - eased;
                scaleX = 1f - eased;
            }
            Vector2 wheelPos = basePosition + new Vector2(0f, -25f + yOffset);
            Vector2 arrowPos = basePosition + new Vector2(0f, yOffset);
            Vector2 wheelOrigin = wheelTex.Size() / 2f;
            if (clickGlows.Count > 0)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                foreach (var glow in clickGlows)
                {
                    float progress = (float)glow.Time / glow.MaxTime;
                    float glowOpacity = (1f - progress) * 0.4f * opacity;
                    float scale = 1.0f + progress * 0.12f;
                    Main.EntitySpriteDraw(wheelTex, wheelPos, null, glow.Color * glowOpacity, glow.Rotation, wheelOrigin, new Vector2(scale * scaleX, scale), SpriteEffects.None, 0);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            if (Projectile.timeLeft <= 30)
            {
                float progress = 1f - (Projectile.timeLeft / 30f);
                float glowOpacity = (1f - progress) * 0.7f * opacity;
                float glowScale = 1.0f + progress * 0.25f;
                Color glowColor = winLeft ? Color.SkyBlue : Color.Orange;
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                Main.EntitySpriteDraw(wheelTex, wheelPos, null, glowColor * glowOpacity, Projectile.rotation, wheelOrigin, new Vector2(glowScale * scaleX, glowScale), SpriteEffects.None, 0);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Main.EntitySpriteDraw(wheelTex, wheelPos, null, Color.White * opacity, Projectile.rotation, wheelOrigin, new Vector2(scaleX, 1f), SpriteEffects.None, 0);
            Vector2 arrowOrigin = new Vector2(arrowTex.Width / 2f, arrowTex.Height);
            Main.EntitySpriteDraw(arrowTex, arrowPos, null, Color.White * opacity, 0f, arrowOrigin, new Vector2(0.5f * scaleX, 0.5f), SpriteEffects.None, 0);
            return false;
        }
    }
}
