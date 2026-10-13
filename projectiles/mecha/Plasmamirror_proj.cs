using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.DataStructures;
namespace jarkos.projectiles.mecha
{
	public class Plasmamirror_proj : ModProjectile
	{
        private float shieldIntensity = 0f;
        private float resonanceCharge = 0f;
        private List<RippleEffect> ripples = new List<RippleEffect>();
        private float perfectTimingWindow = 0f;
        private bool justActivated = true;
        private const float SHIELD_WIDTH = 120f;
        private const float SHIELD_HEIGHT = 180f;
        private const int START_ENERGY_COST = 50;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = (int)SHIELD_WIDTH;
            Projectile.height = (int)SHIELD_HEIGHT;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 999999;
            Projectile.DamageType = DamageClass.Default;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.channel || player.dead || !player.active)
            {
                if (shieldIntensity > 0)
                {
                    SpawnDissipateEffects();
                }
                Projectile.Kill();
                return;
            }
            if (justActivated)
            {
                perfectTimingWindow = 15f;
                justActivated = false;
            }
            if (perfectTimingWindow > 0)
                perfectTimingWindow--;
            Vector2 targetPos = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 80f;
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.3f);
            Projectile.rotation = (Main.MouseWorld - player.Center).ToRotation();
            shieldIntensity = MathHelper.Lerp(shieldIntensity, 1f, 0.15f);
            Projectile.alpha = (int)(255 * (1f - shieldIntensity));
            UpdateRipples();
            ReflectProjectiles(player);
            ReflectMeleeAttacks(player);
            if (resonanceCharge >= 100f)
            {
                TriggerPlasmaResonance(player);
                resonanceCharge = 0f;
            }
            SpawnAmbientEffects();
            player.ChangeDir(Main.MouseWorld.X > player.Center.X ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
        }
        private void ReflectProjectiles(Player player)
        {
            Rectangle shieldHitbox = GetShieldHitbox();
            foreach (Projectile proj in Main.projectile)
            {
                if (!proj.active || proj.whoAmI == Projectile.whoAmI) continue;
                if (!proj.hostile) continue;
                if (proj.GetGlobalProjectile<PlasmaReflectedMarker>().wasReflected) continue;
                if (shieldHitbox.Intersects(proj.Hitbox))
                {
                    bool isCritical = perfectTimingWindow > 0 || Main.rand.NextFloat() < 0.15f;
                    float damageMultiplier = isCritical ? 3.5f : 1.5f;
                    int reflectedDamage = (int)(Projectile.damage * damageMultiplier);
                    Vector2 reflectDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    int newProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), proj.Center, reflectDirection * 20f, ModContent.ProjectileType<Reflected_projectile>(), reflectedDamage, Projectile.knockBack * 2f, player.whoAmI, isCritical ? 1 : 0);
                    AddRipple(proj.Center - Projectile.Center, isCritical);
                    resonanceCharge += isCritical ? 35f : 20f;
                    SpawnReflectionEffects(proj.Center, isCritical);
                    SoundEngine.PlaySound(isCritical ?
                        SoundID.Item125 with { Pitch = 0.5f } :
                        SoundID.Item101, proj.Center);
                    proj.Kill();
                }
            }
        }
        private void ReflectMeleeAttacks(Player player)
        {
            Rectangle shieldHitbox = GetShieldHitbox();
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (shieldHitbox.Intersects(npc.Hitbox) && npc.velocity.Length() > 2f)
                {
                    bool isCritical = perfectTimingWindow > 0;
                    float damageMultiplier = isCritical ? 2.5f : 1.2f;
                    Vector2 knockbackDir = (npc.Center - player.Center).SafeNormalize(Vector2.UnitX);
                    npc.velocity = knockbackDir * (isCritical ? 25f : 15f);
                    int damage = (int)(Projectile.damage * damageMultiplier);
                    npc.SimpleStrikeNPC(damage, player.direction, isCritical, Projectile.knockBack * 2f);
                    AddRipple(npc.Center - Projectile.Center, isCritical);
                    SpawnMeleeReflectEffects(npc.Center, isCritical);
                    resonanceCharge += isCritical ? 25f : 15f;
                    npc.immune[player.whoAmI] = 30;
                    SoundEngine.PlaySound(SoundID.NPCHit53 with { Pitch = 0.3f }, npc.Center);
                }
            }
        }
        private Rectangle GetShieldHitbox()
        {
            Vector2 shieldDirection = Projectile.rotation.ToRotationVector2();
            Vector2 perpendicular = shieldDirection.RotatedBy(MathHelper.PiOver2);
            Vector2 center = Projectile.Center;
            return new Rectangle((int)(center.X - SHIELD_WIDTH / 2), (int)(center.Y - SHIELD_HEIGHT / 2), (int)SHIELD_WIDTH, (int)SHIELD_HEIGHT);
        }
        private void TriggerPlasmaResonance(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item162 with { Pitch = -0.3f, Volume = 1.5f }, Projectile.Center);
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi / 36f * i;
                Vector2 velocity = angle.ToRotationVector2() * 15f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<Plasmawave_resonance_proj>(), Projectile.damage * 2, Projectile.knockBack * 3f, player.whoAmI);
            }
            for (int i = 0; i < 100; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(20f, 20f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, dustVel, 0, default, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                player.GetModPlayer<Screenshake_system>().stronger_shake++;
            }
        }
        private void AddRipple(Vector2 localPosition, bool isCritical)
        {
            ripples.Add(new RippleEffect            {
                Position = localPosition, Intensity = isCritical ? 2f : 1f, Timer = 30f, MaxTimer = 30f, IsCritical = isCritical
            });
        }
        private void UpdateRipples()
        {
            for (int i = ripples.Count - 1; i >= 0; i--)
            {
                ripples[i].Timer--;
                if (ripples[i].Timer <= 0)
                    ripples.RemoveAt(i);
            }
        }
        private void SpawnAmbientEffects()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 perpendicular = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2();
                Vector2 offset = perpendicular * Main.rand.NextFloat(-SHIELD_HEIGHT / 2, SHIELD_HEIGHT / 2);
                Vector2 pos = Projectile.Center + offset;
                Color plasmaColor = GetPlasmaColor(Main.GameUpdateCount * 0.05f + offset.Y * 0.01f);
                Dust dust = Dust.NewDustPerfect(pos, DustID.FireworksRGB, perpendicular * Main.rand.NextFloat(-1f, 1f), 0, plasmaColor, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
            if (Main.rand.NextBool(10))
            {
                SpawnElectricArc();
            }
            if (resonanceCharge > 50f)
            {
                float intensity = resonanceCharge / 100f;
                Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * intensity * 2f);
            }
        }
        private void SpawnElectricArc()
        {
            Vector2 start = Projectile.Center + Main.rand.NextVector2Circular(SHIELD_WIDTH / 2, SHIELD_HEIGHT / 2);
            Vector2 end = Projectile.Center + Main.rand.NextVector2Circular(SHIELD_WIDTH / 2, SHIELD_HEIGHT / 2);
            int segments = 8;
            Vector2 lastPos = start;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)(i + 1) / segments;
                Vector2 targetPos = Vector2.Lerp(start, end, t);
                targetPos += Main.rand.NextVector2Circular(10f, 10f);
                for (int j = 0; j < 3; j++)
                {
                    Vector2 dustPos = Vector2.Lerp(lastPos, targetPos, j / 3f);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.Electric, Vector2.Zero, 0, default, 0.8f);
                    dust.noGravity = true;
                }
                lastPos = targetPos;
            }
        }
        private void SpawnReflectionEffects(Vector2 position, bool isCritical)
        {
            int dustCount = isCritical ? 40 : 20;
            Color baseColor = isCritical ? Color.Gold : Color.Cyan;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(isCritical ? 15f : 8f, isCritical ? 15f : 8f);
                Dust dust = Dust.NewDustPerfect(position, DustID.FireworksRGB, velocity, 0, baseColor, isCritical ? 2f : 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
            Lighting.AddLight(position, (isCritical ? Color.Gold : Color.Cyan).ToVector3() * 3f);
            if (isCritical)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20f * i;
                    Vector2 vel = angle.ToRotationVector2() * 10f;
                    Dust dust = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, default, 2f);
                    dust.noGravity = true;
                }
            }
        }
        private void SpawnMeleeReflectEffects(Vector2 position, bool isCritical)
        {
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi / 24f * i;
                Vector2 vel = angle.ToRotationVector2() * (isCritical ? 12f : 6f);
                Dust dust = Dust.NewDustPerfect(position, DustID.Electric, vel, 0, default, isCritical ? 2.5f : 1.8f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        private void SpawnDissipateEffects()
        {
            for (int i = 0; i < 50; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(SHIELD_WIDTH / 2, SHIELD_HEIGHT / 2);
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, default, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }
        private Color GetPlasmaColor(float time)
        {
            float r = (float)(Math.Sin(time) * 0.5f + 0.5f);
            float g = (float)(Math.Sin(time + MathHelper.TwoPi / 3f) * 0.5f + 0.5f);
            float b = (float)(Math.Sin(time + MathHelper.TwoPi * 2f / 3f) * 0.5f + 0.5f);
            return new Color((int)(100 + r * 155), (int)(200 + g * 55), (int)(220 + b * 35));
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawPlasmaShield(spriteBatch);
            DrawRipples(spriteBatch);
            DrawResonanceGlow(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawPlasmaShield(SpriteBatch spriteBatch)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/projectiles/mecha/Plasmamirror_proj").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.02f;
            for (int layer = 0; layer < 5; layer++)
            {
                float layerOffset = layer * 0.2f;
                float layerScale = 1f + layer * 0.1f;
                float layerAlpha = shieldIntensity * (1f - layer * 0.15f) * 0.6f;
                Color layerColor = GetPlasmaColor(time + layerOffset);
                layerColor *= layerAlpha;
                float wobble = (float)Math.Sin(time * 3f + layer) * 5f;
                Vector2 offset = new Vector2(wobble, 0).RotatedBy(Projectile.rotation);
                spriteBatch.Draw(glowTex, drawPos + offset, null, layerColor, Projectile.rotation, glowTex.Size() / 2f, new Vector2(0.3f, SHIELD_HEIGHT / glowTex.Height) * layerScale, SpriteEffects.None, 0f);
            }
            Color coreColor = Color.White * shieldIntensity * 0.8f;
            spriteBatch.Draw(glowTex, drawPos, null, coreColor, Projectile.rotation, glowTex.Size() / 2f, new Vector2(0.1f, SHIELD_HEIGHT / glowTex.Height * 0.9f), SpriteEffects.None, 0f);
        }
        private void DrawRipples(SpriteBatch spriteBatch)
        {
            Texture2D rippleTex = ModContent.Request<Texture2D>("jarkos/projectiles/mecha/Plasmamirror_proj").Value;
            foreach (var ripple in ripples)
            {
                float progress = 1f - (ripple.Timer / ripple.MaxTimer);
                float scale = 0.5f + progress * 1.5f;
                float alpha = (1f - progress) * ripple.Intensity;
                Color rippleColor = ripple.IsCritical ? Color.Gold : Color.Cyan;
                rippleColor *= alpha;
                Vector2 drawPos = Projectile.Center + ripple.Position - Main.screenPosition;
                spriteBatch.Draw(rippleTex, drawPos, null, rippleColor, Projectile.rotation, rippleTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
        }
        private void DrawResonanceGlow(SpriteBatch spriteBatch)
        {
            if (resonanceCharge <= 0) return;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/projectiles/mecha/Plasmamirror_proj").Value;
            float intensity = resonanceCharge / 100f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f + 0.8f;
            Color glowColor = Color.Lerp(Color.Cyan, Color.Magenta, intensity) * intensity * pulse;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(glowTex, drawPos, null, glowColor, Projectile.rotation, glowTex.Size() / 2f, 1.5f + intensity * 0.5f, SpriteEffects.None, 0f);
        }
    }
    public class RippleEffect
    {
        public Vector2 Position;
        public float Intensity;
        public float Timer;
        public float MaxTimer;
        public bool IsCritical;
    }
    public class PlasmaReflectedMarker : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool wasReflected = false;
    }
     public class PlasmaMirrorPlayer : ModPlayer
    {
        public bool holdingMirror = false;
        private float auraTimer = 0f;
        public override void PostUpdate()
        {
            if (holdingMirror)
            {
                auraTimer += 0.1f;
                SpawnPlayerAura();
            }
        }
        private void SpawnPlayerAura()
        {
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = 30f + Main.rand.NextFloat(20f);
                Vector2 pos = Player.Center + angle.ToRotationVector2() * dist;
                Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * 2f;
                Color auraColor = Main.hslToRgb((auraTimer * 0.05f) % 1f, 0.8f, 0.6f);
                Dust d = Dust.NewDustPerfect(pos, DustID.FireworksRGB, vel, 0, auraColor, 1f);
                d.noGravity = true;
            }
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = Player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 20f);
                Vector2 vel = new Vector2(0, -Main.rand.NextFloat(2f, 4f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Clentaminator_Cyan, vel, 0, default, 1.2f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }
            if (Main.rand.NextBool(8))
            {
                Vector2 pos = Player.Bottom + new Vector2(Main.rand.NextFloat(-15f, 15f), 0);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, new Vector2(Main.rand.NextFloat(-1f, 1f), -2f), 0, default, 0.8f);
                d.noGravity = true;
            }
            float pulse = (float)Math.Sin(auraTimer) * 0.2f + 0.5f;
            Lighting.AddLight(Player.Center, new Vector3(0.2f, 0.5f, 0.7f) * pulse);
        }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (holdingMirror)
            {
                fullBright = true;
                float pulse = (float)Math.Sin(auraTimer * 2f) * 0.1f;
                r = 0.8f + pulse;
                g = 0.9f + pulse;
                b = 1f;
            }
        }
    }
}
