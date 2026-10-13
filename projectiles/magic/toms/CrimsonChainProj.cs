using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using jarkos.systems;
using jarkos.particles;

namespace jarkos.projectiles.magic.toms
{
	public class CrimsonChainProj : ModProjectile
	{
		public override string Texture => "jarkos/textures/MagicSphereProj";
		public float Opacity
		{
			get => 1f - Projectile.alpha / 255f;
			set => Projectile.alpha = (int)((1f - value) * 255f);
		}
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.light = 0.35f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        private float timer = 0f;
        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.5f, 0.06f, 0.08f);
            if (timer % 3 == 0)
            {
                new VoidShardParticle(Projectile.Center, -Projectile.velocity * 0.03f, new Color(180, 20, 25), 18, Main.rand.NextFloat(0.4f, 0.8f), 1f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Coins with { Pitch = 0.3f, Volume = 0.4f }, target.Center);
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, 1f);
                d.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.LifeDrain, vel.X, vel.Y, 0, new Color(255, 50, 50), 1.2f);
                d.noGravity = true;
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
                effect.Parameters["uColor"]?.SetValue(new Color(150, 15, 20).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 120, 120).ToVector4());
                TrailRenderer.Draw(
                    effect,
                    points,
                    t => 20f * (1f - t) + 0.5f,
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
            float pulse = 1f + (float)Math.Sin(timer * 0.2f) * 0.12f;
            Color drawCol = new Color(200, 25, 30);
            sb.Draw(tex, pos, null, drawCol * 0.4f * pulse, Projectile.rotation, origin, Projectile.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, drawCol, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                new VoidShardParticle(Projectile.Center, vel, new Color(180, 20, 25), 20, Main.rand.NextFloat(0.7f, 1.2f), 1f).Spawn();
            }
        }
    }
}
