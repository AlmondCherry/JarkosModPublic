using jarkos;
using jarkos.projectiles.mecha;
using Jarkos.trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
	public class Nail01_proj : ModProjectile
	{
        private const int MaxNailsForExplosion = 8;
        private static readonly Color PrimaryColor = new Color(0, 200, 255);
        private static readonly Color SecondaryColor = new Color(150, 50, 255);
        private static readonly Color CoreColor = new Color(255, 255, 255);
        private Vector2[] trailPositions = new Vector2[15];
        private float[] trailRotations = new float[15];
        private bool isStuck = false;
        private int stuckNPC = -1;
        private Vector2 stuckOffset;
        private float stuckRotation;
        private int stuckTimer = 0;
        private const int MaxStuckTime = 300;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.light = 0.4f;
        }
        public override void AI()
        {
            if (isStuck)
            {
                StuckBehavior();
                return;
            }
            FlyingBehavior();
        }
        private void FlyingBehavior()
        {
            UpdateTrail();
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.velocity.Length() < 20f)
            {
                Projectile.velocity *= 1.01f;
            }
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f, DustID.Electric, Projectile.velocity * -0.2f + Main.rand.NextVector2Circular(1f, 1f), 0, Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()), 0.6f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
            if (Main.rand.NextBool(8))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
                Dust Spark = Dust.NewDustPerfect(sparkPos, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.5f);
                Spark.noGravity = true;
            }
        }
        private void UpdateTrail()
        {
            for (int i = trailPositions.Length - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
                trailRotations[i] = trailRotations[i - 1];
            }
            trailPositions[0] = Projectile.Center;
            trailRotations[0] = Projectile.rotation;
        }
        private void StuckBehavior()
        {
            stuckTimer++;
            if (stuckNPC < 0 || stuckNPC >= Main.maxNPCs || !Main.npc[stuckNPC].active)
            {
                Projectile.Kill();
                return;
            }
            NPC npc = Main.npc[stuckNPC];
            Projectile.Center = npc.Center + stuckOffset;
            Projectile.rotation = stuckRotation;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.Electric, Main.rand.NextVector2Circular(1f, 1f), 0, PrimaryColor, 0.5f);
                dust.noGravity = true;
            }
            float pulse = (float)Math.Sin(stuckTimer * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.3f * pulse);
            if (stuckTimer >= MaxStuckTime)
            {
                CreateFallOffEffect();
                Projectile.Kill();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!isStuck && target.life > 0)
            {
                isStuck = true;
                stuckNPC = target.whoAmI;
                stuckOffset = Projectile.Center - target.Center;
                stuckRotation = Projectile.rotation;
                Projectile.penetrate = -1;
                Projectile.damage = 0;
                var nailTracker = target.GetGlobalNPC<NailStackGlobalNPC>();
                nailTracker.AddNail(Projectile.whoAmI);
                CreateHitEffect();
                SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
                if (nailTracker.NailCount >= MaxNailsForExplosion)
                {
                    TriggerExplosion(target, nailTracker);
                }
            }
        }
        private void TriggerExplosion(NPC target, NailStackGlobalNPC nailTracker)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f }, target.Center);
            int explosionDamage = 50 * nailTracker.NailCount;
            target.SimpleStrikeNPC(explosionDamage, 0, false, 0, DamageClass.Ranged, true, Main.player[Projectile.owner].luck);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<NailExplosion>(), explosionDamage / 2, 8f, Projectile.owner);
            CreateExplosionEffects(target.Center);
            nailTracker.RemoveAllNails();
        }
        private void CreateHitEffect()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, dustVel, 0, PrimaryColor, 0.8f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(3f, 3f), 0, SecondaryColor, 1f);
                Spark.noGravity = true;
            }
        }
        private void CreateExplosionEffects(Vector2 position)
        {
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi / 40 * i;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 15f);
                Dust dust = Dust.NewDustPerfect(position, DustID.Electric, velocity, 0, Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()), Main.rand.NextFloat(1f, 2f));
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            for (int i = 0; i < 20; i++)
            {
                Dust flash = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(20f, 20f), DustID.Electric, Main.rand.NextVector2Circular(8f, 8f), 0, CoreColor, 1.5f);
                flash.noGravity = true;
            }
            for (int i = 0; i < 15; i++)
            {
                Dust Spark = Dust.NewDustPerfect(position, DustID.FireworkFountain_Blue, Main.rand.NextVector2Circular(10f, 10f), 0, default, 1.2f);
                Spark.noGravity = true;
            }
        }
        private void CreateFallOffEffect()
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 0, PrimaryColor, 0.5f);
                dust.noGravity = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig with { Pitch = 0.8f, Volume = 0.4f }, Projectile.Center);
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, -oldVelocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.8f) * Main.rand.NextFloat(1f, 3f), 0, PrimaryColor, 0.6f);
                dust.noGravity = true;
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (isStuck)
            {
                DrawStuckNail(lightColor);
            }
            else
            {
                DrawFlyingNail(lightColor);
            }
            return false;
        }
        private void DrawFlyingNail(Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            DrawTrail(spriteBatch, pixel);
            DrawGlow(spriteBatch, pixel);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRect = texture.Bounds;
            Vector2 origin = sourceRect.Size() / 2f;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float glowScale = 1.5f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            spriteBatch.Draw(texture, drawPos, sourceRect, PrimaryColor * 0.6f, Projectile.rotation, origin, Projectile.scale * glowScale, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(texture, drawPos, sourceRect, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < trailPositions.Length - 1; i++)
            {
                if (trailPositions[i] == Vector2.Zero || trailPositions[i + 1] == Vector2.Zero)
                    continue;
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                Vector2 direction = end - start;
                if (direction.Length() < 1f)
                    continue;
                float progress = (float)i / trailPositions.Length;
                float width = MathHelper.Lerp(6f, 1f, progress);
                float alpha = MathHelper.Lerp(0.8f, 0f, progress);
                Color trailColor = Color.Lerp(CoreColor, PrimaryColor, progress) * alpha;
                Color outerColor = Color.Lerp(PrimaryColor, SecondaryColor, progress) * alpha * 0.5f;
                float rotation = direction.ToRotation();
                float length = direction.Length();
                Rectangle outerRect = new Rectangle(0, 0, (int)length, (int)(width * 3));
                Vector2 outerOrigin = new Vector2(0, outerRect.Height / 2f);
                spriteBatch.Draw(pixel, start, outerRect, outerColor, rotation, outerOrigin, 1f, SpriteEffects.None, 0f);
                Rectangle innerRect = new Rectangle(0, 0, (int)length, (int)width);
                Vector2 innerOrigin = new Vector2(0, innerRect.Height / 2f);
                spriteBatch.Draw(pixel, start, innerRect, trailColor, rotation, innerOrigin, 1f, SpriteEffects.None, 0f);
                Rectangle coreRect = new Rectangle(0, 0, (int)length, (int)(width * 0.5f));
                Vector2 coreOrigin = new Vector2(0, coreRect.Height / 2f);
                spriteBatch.Draw(pixel, start, coreRect, CoreColor * alpha, rotation, coreOrigin, 1f, SpriteEffects.None, 0f);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawGlow(SpriteBatch spriteBatch, Texture2D pixel)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 1f;
            for (int i = 0; i < 3; i++)
            {
                float scale = (3 - i) * 4f * pulse;
                float alpha = 0.15f / (i + 1);
                Rectangle glowRect = new Rectangle(0, 0, (int)scale, (int)scale);
                Vector2 origin = new Vector2(scale / 2f, scale / 2f);
                spriteBatch.Draw(pixel, drawPos, glowRect, PrimaryColor * alpha, 0f, origin, 1f, SpriteEffects.None, 0f);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawStuckNail(Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRect = texture.Bounds;
            Vector2 origin = sourceRect.Size() / 2f;
            float pulse = (float)Math.Sin(stuckTimer * 0.15f) * 0.2f + 1f;
            float warningPulse = 1f;
            if (stuckNPC >= 0 && stuckNPC < Main.maxNPCs && Main.npc[stuckNPC].active)
            {
                var nailTracker = Main.npc[stuckNPC].GetGlobalNPC<NailStackGlobalNPC>();
                if (nailTracker.NailCount >= MaxNailsForExplosion - 2)
                {
                    warningPulse = (float)Math.Sin(stuckTimer * 0.5f) * 0.5f + 1.5f;
                }
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Color glowColor = Color.Lerp(PrimaryColor, Color.Red, (warningPulse - 1f) / 0.5f);
            spriteBatch.Draw(texture, drawPos, sourceRect, glowColor * 0.5f * pulse * warningPulse, Projectile.rotation, origin, Projectile.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(texture, drawPos, sourceRect, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}
	#region Explosion
	public class NailExplosion : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/Empty_proj";
        private static readonly Color PrimaryColor = new Color(0, 200, 255);
        private static readonly Color SecondaryColor = new Color(150, 50, 255);
        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            float progress = 1f - (Projectile.timeLeft / 20f);
            float intensity = (1f - progress) * 2f;
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * intensity);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.timeLeft > 15)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float distance = Main.rand.NextFloat(30f, 80f);
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * distance;
                    Dust dust = Dust.NewDustPerfect(pos, DustID.Electric, (Projectile.Center - pos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 6f), 0, Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()), Main.rand.NextFloat(0.8f, 1.5f));
                    dust.noGravity = true;
                }
            }
            if (Projectile.timeLeft == 18)
            {
                for (int i = 0; i < 30; i++)
                {
                    float angle = MathHelper.TwoPi / 30 * i;
                    Vector2 velocity = angle.ToRotationVector2() * 12f;
                    Dust ring = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, velocity, 0, PrimaryColor, 1.5f);
                    ring.noGravity = true;
                    ring.fadeIn = 1.2f;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float progress = 1f - (Projectile.timeLeft / 20f);
            float radius = MathHelper.Lerp(10f, 120f, progress);
            float alpha = 1f - progress;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            int segments = 60;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi / segments * i;
                float nextAngle = MathHelper.TwoPi / segments * (i + 1);
                Vector2 start = drawPos + angle.ToRotationVector2() * radius;
                Vector2 end = drawPos + nextAngle.ToRotationVector2() * radius;
                Vector2 direction = end - start;
                float lineWidth = 8f * alpha;
                float rotation = direction.ToRotation();
                float length = direction.Length() + 1;
                Rectangle rect = new Rectangle(0, 0, (int)length, (int)lineWidth);
                Vector2 origin = new Vector2(0, lineWidth / 2f);
                Color ringColor = Color.Lerp(SecondaryColor, PrimaryColor, (float)i / segments) * alpha;
                spriteBatch.Draw(pixel, start, rect, ringColor, rotation, origin, 1f, SpriteEffects.None, 0f);
            }
            float flashSize = 60f * (1f - progress * 0.5f) * alpha;
            Rectangle flashRect = new Rectangle(0, 0, (int)flashSize, (int)flashSize);
            Vector2 flashOrigin = flashRect.Size() / 2f;
            spriteBatch.Draw(pixel, drawPos, flashRect, Color.White * alpha * 0.8f, 0f, flashOrigin, 1f, SpriteEffects.None, 0f);
            int rayCount = 12;
            for (int i = 0; i < rayCount; i++)
            {
                float angle = MathHelper.TwoPi / rayCount * i + progress * 2f;
                float rayLength = radius * 0.8f;
                float rayWidth = 4f * alpha;
                Vector2 rayStart = drawPos;
                Vector2 rayEnd = drawPos + angle.ToRotationVector2() * rayLength;
                Vector2 rayDir = rayEnd - rayStart;
                Rectangle rayRect = new Rectangle(0, 0, (int)rayLength, (int)rayWidth);
                Vector2 rayOrigin = new Vector2(0, rayWidth / 2f);
                Color rayColor = PrimaryColor * alpha * 0.6f;
                spriteBatch.Draw(pixel, rayStart, rayRect, rayColor, angle, rayOrigin, 1f, SpriteEffects.None, 0f);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
	#endregion
	#region GlobalNPC
	public class NailStackGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        private List<int> stuckNails = new List<int>();
        public int NailCount => stuckNails.Count;
        public void AddNail(int projectileIndex)
        {
            if (!stuckNails.Contains(projectileIndex))
            {
                stuckNails.Add(projectileIndex);
            }
        }
        public void RemoveNail(int projectileIndex)
        {
            stuckNails.Remove(projectileIndex);
        }
        public void RemoveAllNails()
        {
            foreach (int index in stuckNails)
            {
                if (index >= 0 && index < Main.maxProjectiles && Main.projectile[index].active)
                {
                    Main.projectile[index].Kill();
                }
            }
            stuckNails.Clear();
        }
        public override void PostAI(NPC npc)
        {
            stuckNails.RemoveAll(index =>
                index < 0 || index >= Main.maxProjectiles || !Main.projectile[index].active || Main.projectile[index].type != ModContent.ProjectileType<Nail01_proj>());
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (stuckNails.Count >= 5)
            {
                float intensity = (stuckNails.Count - 4) / 4f;
                intensity = MathHelper.Clamp(intensity, 0f, 1f);
                if (Main.rand.NextBool(3))
                {
                    drawColor = Color.Lerp(drawColor, new Color(0, 200, 255), intensity * 0.3f);
                }
            }
        }
    }
	#endregion
}
