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
namespace jarkos.projectiles.magic.toms
{
	public class SacrificeOrb : ModProjectile
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
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                new jarkos.particles.VoidShardParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), vel, new Color(200, 20, 30), 20, Main.rand.NextFloat(0.5f, 0.9f), 1f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            int numProjectiles = 6;
            float rotation = MathHelper.TwoPi / numProjectiles;
            for (int i = 0; i < numProjectiles; i++)
            {
                Vector2 perturbedSpeed = new Vector2(0, -8f).RotatedBy(rotation * i);
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<BloodHomingProj>(), (int)(Projectile.damage * 0.7f), Projectile.knockBack, Projectile.owner);
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                new jarkos.particles.VoidShardParticle(Projectile.Center, vel, new Color(220, 15, 25), 30, Main.rand.NextFloat(0.9f, 1.5f), 1f).Spawn();
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
                effect.Parameters["uColor"]?.SetValue(new Color(180, 0, 0).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 100, 100).ToVector4());
                TrailRenderer.Draw(
                    effect,
                    points,
                    t => 24f * (1f - t) + 0.5f,
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
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
            Color drawCol = Color.Red;
            sb.Draw(tex, pos, null, drawCol * 0.4f * pulse, Projectile.rotation, origin, Projectile.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, drawCol, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
    public class BloodHomingProj : ModProjectile
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
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                new jarkos.particles.VoidShardParticle(Projectile.Center, Vector2.Zero, new Color(180, 20, 20), 15, Main.rand.NextFloat(0.4f, 0.7f), 1f).Spawn();
            }
            NPC closestNPC = FindClosestNPC(600f);
            if (closestNPC != null)
            {
                Vector2 direction = closestNPC.Center - Projectile.Center;
                direction.Normalize();
                float speed = 12f;
                Vector2 targetVelocity = direction * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.04f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                new jarkos.particles.VoidShardParticle(Projectile.Center, vel, new Color(180, 20, 20), 20, Main.rand.NextFloat(0.6f, 1.0f), 1f).Spawn();
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
                effect.Parameters["uColor"]?.SetValue(new Color(150, 0, 0).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 80, 80).ToVector4());
                TrailRenderer.Draw(
                    effect,
                    points,
                    t => 12f * (1f - t) + 0.5f,
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawCol = new Color(200, 20, 20);
            sb.Draw(tex, drawPos, null, drawCol * 0.4f, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, drawCol, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC target = Main.npc[k];
                if (target.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        if (Collision.CanHit(Projectile.Center, 1, 1, target.Center, 1, 1))
                        {
                            sqrMaxDetectDistance = sqrDistanceToTarget;
                            closestNPC = target;
                        }
                    }
                }
            }
            return closestNPC;
        }
    }
}
