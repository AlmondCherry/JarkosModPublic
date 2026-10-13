using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.magic.staffs
{
	public class Hellish_bolt : ModProjectile
	{
		public override string Texture => "jarkos/textures/MagicSphereProj";
		private const int TrailMax = 22;
        private Vector2[] trailCache = new Vector2[TrailMax];
        private bool trailInitialized = false;
        private Color starColor;
        private bool colorChosen = false;
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }
        public override void AI()
        {
            if (!colorChosen)
            {
                int variant = Main.rand.Next(4);
                switch (variant)
                {
                    case 0: starColor = new Color(120, 160, 255); break;
                    case 1: starColor = new Color(180, 130, 255); break;
                    case 2: starColor = new Color(160, 220, 255); break;
                    case 3: starColor = new Color(220, 200, 255); break;
                }
                colorChosen = true;
            }
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailMax; i++)
                    trailCache[i] = Projectile.Center;
                trailInitialized = true;
            }
            for (int i = TrailMax - 1; i > 0; i--)
                trailCache[i] = trailCache[i - 1];
            trailCache[0] = Projectile.Center;
            Projectile.ai[0]++;
            Projectile.rotation += 0.2f;
            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.2f) * 0.25f + 0.75f;
            Lighting.AddLight(Projectile.Center, starColor.R / 255f * 0.5f * pulse, starColor.G / 255f * 0.5f * pulse, starColor.B / 255f * 0.5f * pulse);
            if (Projectile.ai[0] > 10)
                DoHoming();
            SpawnSmallStarDust();
            if (Projectile.timeLeft < 40)
                Projectile.alpha = (int)MathHelper.Lerp(0, 255, 1f - Projectile.timeLeft / 40f);
        }
        private void DoHoming()
        {
            float detectionRange = 350f;
            float homingStrength = 0.06f;
            float maxSpeed = 16f;
            NPC closest = null;
            float closestDist = detectionRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            if (closest != null)
            {
                Vector2 desired = Vector2.Normalize(closest.Center - Projectile.Center) * maxSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, homingStrength);
            }
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }
            if (Projectile.velocity.Length() < maxSpeed * 0.8f)
                Projectile.velocity *= 1.02f;
        }
        private void SpawnSmallStarDust()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                new jarkos.particles.StarParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), vel, starColor, 20, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!trailInitialized)
                return false;
            SpriteBatch sb = Main.spriteBatch;
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < TrailMax; i++)
            {
                if (trailCache[i] != Vector2.Zero)
                    points.Add(trailCache[i]);
            }
            float globalAlpha = (255 - Projectile.alpha) / 255f;
            if (points.Count >= 2 && HealShaders.Stream != null)
            {
                sb.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                effect.Parameters["uOpacity"]?.SetValue(globalAlpha);
                effect.Parameters["uColor"]?.SetValue(starColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 255).ToVector4());
                TrailRenderer.Draw(
                    effect,
                    points,
                    t => 16f * (1f - t) + 0.5f,
                    t => Color.White * (1f - t),
                    HealShaders.HealNoise
                );
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.2f) * 0.12f;
            sb.Draw(tex, pos, null, starColor * 0.4f * pulse * globalAlpha, Projectile.rotation, origin, Projectile.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, starColor * globalAlpha, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Color.White * 0.8f * globalAlpha, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4, 4);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.BlueCrystalShard, vel, 50, default, Main.rand.NextFloat(0.7f, 1.3f));
                d.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item25, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                new StarParticle(Projectile.Center, vel, starColor, 25, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
            }
            Lighting.AddLight(Projectile.Center, starColor.R / 255f, starColor.G / 255f, starColor.B / 255f);
        }
        public override Color? GetAlpha(Color lightColor)
            => Color.White * ((255 - Projectile.alpha) / 255f);
}
}
