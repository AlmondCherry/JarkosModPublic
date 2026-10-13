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
namespace jarkos.projectiles.magic.other
{
	public class Ultimate_mage_rune_proj : ModProjectile
	{
		public override string Texture => "jarkos/textures/MagicSphereProj";
		public float Opacity
		{
			get => 1f - Projectile.alpha / 255f;
			set => Projectile.alpha = (int)((1f - value) * 255f);
		}
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.5f;
        }
        private float glowTimer = 0f;
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            glowTimer++;
            Lighting.AddLight(Projectile.Center, 0.3f, 0.7f, 1f);
            if (Main.rand.NextBool(2))
            {
                new particles.StarParticle(Projectile.Center, -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.05f, 0.1f), new Color(80, 200, 255), 18, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            List<Vector2> points = new List<Vector2>();
            Vector2 halfSize = Projectile.Size * 0.5f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    points.Add(Projectile.oldPos[i] + halfSize);
                }
            }
            if (points.Count >= 2 && HealShaders.Stream != null)
            {
                sb.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2.5f);
                effect.Parameters["uOpacity"]?.SetValue(Opacity);
                effect.Parameters["uColor"]?.SetValue(new Color(40, 140, 255).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(180, 230, 255).ToVector4());
                TrailRenderer.Draw(effect, points, t => 20f * (1f - t) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
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
            float pulse = 1f + (float)Math.Sin(glowTimer * 0.2f) * 0.12f;
            Color drawCol = new Color(60, 160, 255);
            sb.Draw(tex, pos, null, drawCol * 0.4f * pulse, Projectile.rotation, origin, Projectile.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, drawCol, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                new particles.StarParticle(Projectile.Center, vel, new Color(80, 200, 255), 20, Main.rand.NextFloat(0.15f, 0.25f), 1f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                new particles.StarParticle(target.Center, vel, new Color(80, 200, 255), 15, Main.rand.NextFloat(0.15f, 0.25f), 1f).Spawn();
            }
        }
    }
    #region Rune Rift
    public class RuneRiftOrb : ModProjectile
    {
        public override string Texture => "jarkos/textures/MagicSphereProj";
        public float Opacity
        {
            get => 1f - Projectile.alpha / 255f;
            set => Projectile.alpha = (int)((1f - value) * 255f);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.7f;
        }
        private float timer = 0f;
        private float orbRotation = 0f;
        public override void AI()
        {
            timer++;
            orbRotation += 0.2f;
            Projectile.velocity *= 0.98f;
            Projectile.rotation += 0.1f;
            float pulse = (float)Math.Sin(timer * 0.15f) * 0.3f + 0.8f;
            Lighting.AddLight(Projectile.Center, 0.7f * pulse, 0.2f * pulse, 1f * pulse);
            if (Main.rand.NextBool(3))
            {
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                new jarkos.particles.VoidShardParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), vel, new Color(180, 60, 255), 20, Main.rand.NextFloat(0.5f, 0.8f), 1f).Spawn();
            }
            NPC closest = FindTarget(300f);
            if (closest != null && timer > 15)
            {
                Vector2 desired = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 11f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.08f);
            }
        }
        private NPC FindTarget(float range)
        {
            NPC best = null;
            float bestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = npc;
                    }
                }
            }
            return best;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item9 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
            if (Projectile.owner == Main.myPlayer)
            {
                int shardCount = 8;
                for (int i = 0; i < shardCount; i++)
                {
                    float angle = i * MathHelper.TwoPi / shardCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel, ModContent.ProjectileType<RuneRiftShard>(), (int)(Projectile.damage * 0.45f), 1f, Projectile.owner);
                }
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                new jarkos.particles.VoidShardParticle(Projectile.Center, vel, new Color(180, 60, 255), 30, Main.rand.NextFloat(0.9f, 1.6f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            List<Vector2> points = new List<Vector2>();
            Vector2 halfSize = Projectile.Size * 0.5f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    points.Add(Projectile.oldPos[i] + halfSize);
                }
            }
            if (points.Count >= 2 && HealShaders.Stream != null)
            {
                sb.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
                effect.Parameters["uOpacity"]?.SetValue(Opacity);
                effect.Parameters["uColor"]?.SetValue(new Color(100, 20, 160).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(230, 180, 255).ToVector4());
                TrailRenderer.Draw(effect, points, t => 28f * (1f - t) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
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
            float pulse = 1f + (float)Math.Sin(timer * 0.12f) * 0.15f;
            Color drawCol = new Color(180, 60, 255);
            sb.Draw(tex, pos, null, drawCol * 0.4f * pulse, Projectile.rotation, origin, Projectile.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, drawCol, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
    #endregion
    #region Rune Rift Shard
    public class RuneRiftShard : ModProjectile
    {
        public override string Texture => "jarkos/textures/MagicSphereProj";
        public float Opacity
        {
            get => 1f - Projectile.alpha / 255f;
            set => Projectile.alpha = (int)((1f - value) * 255f);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.3f;
            Projectile.extraUpdates = 1;
        }
        private float timer = 0f;
        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.97f;
            Projectile.velocity.Y += 0.08f;
            if (timer > 10)
            {
                NPC closest = null;
                float closestDist = 250f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist) { closestDist = dist; closest = npc; }
                    }
                }
                if (closest != null)
                {
                    Vector2 desired = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero)
                        * Math.Max(Projectile.velocity.Length(), 6f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.08f);
                }
            }
            if (Main.rand.NextBool(3))
            {
                new particles.VoidShardParticle(Projectile.Center, Vector2.Zero, new Color(160, 50, 255), 15, Main.rand.NextFloat(0.4f, 0.7f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            List<Vector2> points = new List<Vector2>();
            Vector2 halfSize = Projectile.Size * 0.5f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    points.Add(Projectile.oldPos[i] + halfSize);
                }
            }
            if (points.Count >= 2 && HealShaders.Stream != null)
            {
                sb.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                effect.Parameters["uOpacity"]?.SetValue(Opacity);
                effect.Parameters["uColor"]?.SetValue(new Color(120, 30, 200).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(230, 180, 255).ToVector4());
                TrailRenderer.Draw(effect, points, t => 12f * (1f - t) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawCol = new Color(180, 60, 255);
            sb.Draw(tex, drawPos, null, drawCol * 0.4f, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, drawCol, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                new particles.VoidShardParticle(Projectile.Center, vel, new Color(160, 50, 255), 20, Main.rand.NextFloat(0.5f, 0.9f), 1f).Spawn();
            }
        }
    }
    #endregion
    #region Rune Spiral Blade
    public class RuneSpiralBlade : ModProjectile
    {
        public override string Texture => "jarkos/textures/MagicSphereProj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 22;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.4f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        private Vector2 spawnCenter;
        private bool spawned = false;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!spawned)
            {
                spawnCenter = owner.Center;
                spawned = true;
            }
            Projectile.ai[1]++;

            float time = Projectile.ai[1];
            float startAngle = Projectile.ai[0];
            float spiralRadius = 30f + time * 2.5f;
            float spiralSpeed = 0.12f - time * 0.0003f;
            float currentAngle = startAngle + time * spiralSpeed;

            Vector2 forwardDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 center = spawnCenter + forwardDir * time * 4f;
            Vector2 targetPos = center + new Vector2((float)Math.Cos(currentAngle) * spiralRadius, (float)Math.Sin(currentAngle) * spiralRadius);
            Projectile.velocity = (targetPos - Projectile.Center) * 0.3f;
            Projectile.rotation += 0.35f;

            Lighting.AddLight(Projectile.Center, 0.2f, 0.8f, 0.5f);

            if (Main.rand.NextBool(3))
            {
                new particles.StarParticle(Projectile.Center, Vector2.Zero, new Color(50, 255, 150), 16, Main.rand.NextFloat(0.4f, 0.8f), 1f).Spawn();
            }
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)MathHelper.Lerp(0, 255, 1f - (Projectile.timeLeft / 30f));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            List<Vector2> points = new List<Vector2>();
            Vector2 halfSize = Projectile.Size * 0.5f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    points.Add(Projectile.oldPos[i] + halfSize);
                }
            }
            float alpha = 1f - (Projectile.alpha / 255f);
            if (points.Count >= 2 && HealShaders.Stream != null)
            {
                sb.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2.5f);
                effect.Parameters["uOpacity"]?.SetValue(alpha);
                effect.Parameters["uColor"]?.SetValue(new Color(20, 120, 60).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(180, 255, 220).ToVector4());
                TrailRenderer.Draw(effect, points, t => 22f * (1f - t) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
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
            float pulse = 1f + (float)Math.Sin(Projectile.ai[1] * 0.2f) * 0.12f;
            Color drawCol = new Color(50, 220, 120);
            sb.Draw(tex, pos, null, drawCol * 0.4f * pulse * alpha, Projectile.rotation, origin, Projectile.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, drawCol * alpha, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Color.White * 0.8f * alpha, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                new particles.StarParticle(Projectile.Center, vel, new Color(50, 255, 150), 20, Main.rand.NextFloat(0.6f, 1.1f), 1f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                new particles.StarParticle(target.Center, vel, new Color(50, 255, 150), 15, Main.rand.NextFloat(0.5f, 1.0f), 1f).Spawn();
            }
        }
    }
    #endregion
    #region Rune Celestial Beam
    public class RuneCelestialBeam : ModProjectile
    {
        public override string Texture => "jarkos/textures/MagicSphereProj";
        public float Opacity
        {
            get => 1f - Projectile.alpha / 255f;
            set => Projectile.alpha = (int)((1f - value) * 255f);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 35;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        private float timer = 0f;
        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            float pulse = (float)Math.Sin(timer * 0.15f) * 0.3f + 1f;
            Lighting.AddLight(Projectile.Center, 1f * pulse, 0.8f * pulse, 0.3f * pulse);
            if (Main.rand.NextBool(2))
            {
                new particles.StarParticle(Projectile.Center, -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.05f, 0.1f), new Color(255, 210, 100), 20, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item9 with { Pitch = -0.2f, Volume = 0.8f }, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                new particles.StarParticle(Projectile.Center, vel, new Color(255, 210, 100), 30, Main.rand.NextFloat(0.15f, 0.25f), 1f).Spawn();
            }
            if (Projectile.owner == Main.myPlayer)
            {
                float aoeRadius = 120f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < aoeRadius)
                        {
                            Main.player[Projectile.owner].ApplyDamageToNPC(npc, Projectile.damage / 2, 4f, 0, false);
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            List<Vector2> points = new List<Vector2>();
            Vector2 halfSize = Projectile.Size * 0.5f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    points.Add(Projectile.oldPos[i] + halfSize);
                }
            }
            if (points.Count >= 2 && HealShaders.Stream != null)
            {
                sb.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                effect.Parameters["uOpacity"]?.SetValue(Opacity);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 160, 20).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 240, 200).ToVector4());
                TrailRenderer.Draw(effect, points, t => 24f * (1f - t) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
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
            float pulse = 1f + (float)Math.Sin(timer * 0.2f) * 0.15f;
            Color drawCol = new Color(255, 200, 50);
            
            sb.Draw(tex, pos, null, drawCol * 0.4f * pulse, Projectile.rotation, origin, Projectile.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, drawCol, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                new particles.StarParticle(target.Center, vel, new Color(255, 210, 100), 20, Main.rand.NextFloat(0.15f, 0.25f), 1f).Spawn();
            }
        }
    }
    #endregion
}
