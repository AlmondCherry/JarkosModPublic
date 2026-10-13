using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;

namespace jarkos.projectiles.bullet
{
	public class Echo_arrow : ModProjectile
	{
        private const int TRAIL_LENGTH = 22;
        private const float HOMING_RANGE = 350f;
        private const float HOMING_STRENGTH = 0.06f;
        private const float MAX_SPEED = 16f;
        private const float PULSE_SPEED = 0.12f;
        private ref float Timer => ref Projectile.localAI[0];
        private ref float PulsePhase => ref Projectile.localAI[1];
        private Vector2[] trailPos = new Vector2[TRAIL_LENGTH];
        private bool trailInit = false;
        private static readonly Color BrightYellow = new(255, 240, 80);
        private static readonly Color WarmGold = new(255, 200, 50);
        private static readonly Color PaleYellow = new(255, 255, 180);
        private static readonly Color DeepGold = new(220, 160, 30);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
            Projectile.arrow = true;
        }
        public override void AI()
        {
            Timer++;
            PulsePhase += PULSE_SPEED;
            Projectile.velocity.Y -= 0.05f * Projectile.velocity.Y.CompareTo(0);
            NPC target = FindClosestEnemy();

            if (target != null)
            {
                HomingBehavior(target);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            UpdateTrail();

            float pulseIntensity = 0.4f + MathF.Sin(PulsePhase) * 0.15f;

            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.9f, 0.4f) * pulseIntensity);

            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame);
                d.velocity = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                d.scale = Main.rand.NextFloat(0.4f, 0.8f);
                d.noGravity = true;
                d.alpha = 100;
            }

            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 30f));
            }
        }
        private NPC FindClosestEnemy()
        {
            float closest = HOMING_RANGE;
            NPC result = null;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closest && Collision.CanHitLine(Projectile.Center, 1, 1, npc.position, npc.width, npc.height))
                {
                    closest = dist;
                    result = npc;
                }
            }
            return result;
        }
        private void HomingBehavior(NPC target)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            float distance = toTarget.Length();

            if (distance < 1f) return;
            toTarget.Normalize();

            float strength = HOMING_STRENGTH;
            if (distance < 150f)
                strength *= 1.5f;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * MAX_SPEED, strength);
            if (Projectile.velocity.Length() > MAX_SPEED)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * MAX_SPEED;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(target.Center, 1, 1, DustID.GoldFlame);
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.scale = Main.rand.NextFloat(0.8f, 1.4f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/beam_impact_emp21") with { Volume = 0.55f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, target.Center);
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame);
                d.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
                d.scale = Main.rand.NextFloat(0.6f, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.9f, 0.3f) * 0.6f);
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        private void UpdateTrail()
        {
            if (!trailInit)
            {
                for (int i = 0; i < TRAIL_LENGTH; i++)
                    trailPos[i] = Projectile.Center;
                trailInit = true;
            }

            for (int i = TRAIL_LENGTH - 1; i > 0; i--)
                trailPos[i] = trailPos[i - 1];

            trailPos[0] = Projectile.Center;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float opacity = 1f - Projectile.alpha / 255f;
            if (opacity <= 0.01f) return false;

            SpriteBatch sb = Main.spriteBatch;
            DrawTrail(sb, opacity);
            DrawArrowGlow(sb, opacity);
            DrawArrowCore(sb, opacity);

            return false;
        }
        private void DrawTrail(SpriteBatch sb, float opacity)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 1; i < TRAIL_LENGTH; i++)
            {
                Vector2 curr = trailPos[i - 1];
                Vector2 prev = trailPos[i];
                if (curr == prev) continue;

                float progress = (float)i / TRAIL_LENGTH;
                Vector2 diff = curr - prev;
                float segLen = diff.Length();
                float rot = diff.ToRotation();

                float segAlpha = (1f - progress) * opacity;
                float pulse = 1f + MathF.Sin(PulsePhase * 2f + i * 0.5f) * 0.2f;
                float outerWidth = 14f * (1f - progress * 0.6f) * pulse;
                Color outerCol = DeepGold * segAlpha * 0.15f;

                sb.Draw(pixel, curr - Main.screenPosition, new Rectangle(0, 0, 1, 1), outerCol, rot, new Vector2(0.5f, 0.5f), new Vector2(segLen, outerWidth), SpriteEffects.None, 0f);

                float midWidth = 6f * (1f - progress * 0.5f) * pulse;
                Color midCol = WarmGold * segAlpha * 0.4f;
                sb.Draw(pixel, curr - Main.screenPosition, new Rectangle(0, 0, 1, 1), midCol, rot, new Vector2(0.5f, 0.5f), new Vector2(segLen, midWidth), SpriteEffects.None, 0f);

                float coreWidth = 2f * (1f - progress * 0.7f) * pulse;
                Color coreCol = PaleYellow * segAlpha * 0.6f;
                sb.Draw(pixel, curr - Main.screenPosition, new Rectangle(0, 0, 1, 1), coreCol, rot, new Vector2(0.5f, 0.5f), new Vector2(segLen, coreWidth), SpriteEffects.None, 0f);
            }

            for (int i = 3; i < TRAIL_LENGTH - 1; i += 4)
            {
                float progress = (float)i / TRAIL_LENGTH;
                float dotAlpha = (1f - progress) * opacity;
                float flicker = 0.5f + MathF.Sin(PulsePhase * 3f + i * 1.8f) * 0.5f;
                float dotSize = (2f + flicker * 2.5f) * (1f - progress);

                Color dotCol = BrightYellow * dotAlpha * flicker * 0.5f;
                sb.Draw(pixel, trailPos[i] - Main.screenPosition, new Rectangle(0, 0, 1, 1), dotCol, MathHelper.PiOver4, new Vector2(0.5f, 0.5f), new Vector2(dotSize, dotSize), SpriteEffects.None, 0f);
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawArrowGlow(SpriteBatch sb, float opacity)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float pulse = 1f + MathF.Sin(PulsePhase) * 0.25f;
            float outerSize = 22f * pulse * opacity;

            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), DeepGold * 0.15f * opacity, Projectile.rotation + MathHelper.PiOver4, new Vector2(0.5f, 0.5f), new Vector2(outerSize, outerSize), SpriteEffects.None, 0f);
            float midSize = 14f * pulse * opacity;

            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), WarmGold * 0.3f * opacity, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(midSize, midSize), SpriteEffects.None, 0f);
            float coreSize = 6f * pulse * opacity;

            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), PaleYellow * 0.7f * opacity, 0f, new Vector2(0.5f, 0.5f), new Vector2(coreSize, coreSize), SpriteEffects.None, 0f);
            float flareLen = 16f * pulse * opacity;
            float flareWidth = 3f * opacity;
            
            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), BrightYellow * 0.35f * opacity, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(flareLen, flareWidth), SpriteEffects.None, 0f);
            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), BrightYellow * 0.2f * opacity, Projectile.rotation + MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(flareLen * 0.5f, flareWidth * 0.7f), SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawArrowCore(SpriteBatch sb, float opacity)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            float pulse = 1f + MathF.Sin(PulsePhase * 1.5f) * 0.1f;
            float tipLen = 12f * pulse;
            float tipWidth = 3f * pulse;
            Color tipColor = Color.Lerp(BrightYellow, PaleYellow, 0.5f) * opacity;
            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), tipColor, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(tipLen, tipWidth), SpriteEffects.None, 0f);
            float tailLen = 8f * pulse;
            float tailWidth = 1.5f;
            
            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), WarmGold * opacity * 0.8f, Projectile.rotation + MathHelper.Pi, new Vector2(0f, 0.5f), new Vector2(tailLen, tailWidth), SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
