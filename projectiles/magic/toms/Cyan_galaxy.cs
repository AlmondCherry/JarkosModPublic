using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using jarkos.particles;
using jarkos.buffs.debuff;
using jarkos.systems;
namespace jarkos.projectiles.magic.toms
{
	public class Cyan_galaxy : ModProjectile
	{
		NPC targetNPC = null;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.width = 20;
			Projectile.height = 20;
            Projectile.scale = 1f;
			Projectile.damage = 20;
            Projectile.penetrate = 1;
			Projectile.light = 1.25f;
			Projectile.timeLeft = 150;
			Projectile.tileCollide = false;
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            if (!Jarkos_npc.IsBossOrBossPart(target))
            {
                target.AddBuff(ModContent.BuffType<BlueTetherDebuff>(), 360);
                target.GetGlobalNPC<Jarkos_npc>().cyan_projectile_damage = hit.SourceDamage > 0 ? hit.SourceDamage : Projectile.damage;
            }
			for (int i = 0; i < 15; ++i)
				{
			int dust = Dust.NewDust(Projectile.Center, 0, 0, 88);
			Main.dust[dust].position = Projectile.Center - new Vector2(2, 2);
			Main.dust[dust].scale = 1.5f;
            Main.dust[dust].velocity *= Main.rand.Next(1,2);;
                }
		}
		 public override bool OnTileCollide(Vector2 oldVelocity)
		{
		  if (Projectile.timeLeft > 5)
		    {
				 Projectile.ai[0] += 0.1f;
		        if (Projectile.velocity.X != oldVelocity.X)
		        {
		           Projectile.velocity.X = -oldVelocity.X;
		        }
		        if (Projectile.velocity.Y != oldVelocity.Y)
		        {
	            Projectile.velocity.Y = -oldVelocity.Y;
		        }
		        Projectile.velocity *= .7f;
		        SoundEngine.PlaySound(SoundID.Item8, Projectile.position);
			}
		    return false;
		}
        public override void AI()
        {
            Projectile.rotation += 0.10f * (float)Projectile.direction;
            if (Projectile.localAI[0] == 0f && Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[0] = Projectile.velocity.X;
                Projectile.localAI[1] = Projectile.velocity.Y;
            }
            float timeAlive = 150f - Projectile.timeLeft;
            NPC target = FindClosestNPC(600f);
            if (target != null)
            {
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = new Vector2(Projectile.localAI[0], Projectile.localAI[1]).Length();
                if (speed < 12f) speed = 12f;
                Vector2 desiredVel = targetDir * speed;
                Vector2 currentBase = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                currentBase = Vector2.Lerp(currentBase, desiredVel, 0.05f);
                Projectile.localAI[0] = currentBase.X;
                Projectile.localAI[1] = currentBase.Y;
            }
            Vector2 baseVel = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            Vector2 perp = new Vector2(-baseVel.Y, baseVel.X).SafeNormalize(Vector2.Zero);
            float weaveAmp = 12f * (float)Math.Sin(timeAlive * 0.08f);
            float waveSpeed = -weaveAmp * (float)Math.Cos(timeAlive * 0.16f);
            Projectile.velocity = baseVel + perp * waveSpeed;
            float z = (float)Math.Sin(timeAlive * 0.12f + MathHelper.Pi);
            Projectile.scale = 1.0f * (1f + z * 0.25f);
            if (Main.rand.NextBool(3))
            {
                new StarParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Vector2.Zero, Color.Cyan, 40, 0.05f, 0.8f, 0.1f).Spawn();
            }
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
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }
            return closestNPC;
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
                effect.Parameters["uOpacity"]?.SetValue(1f);
                effect.Parameters["uColor"]?.SetValue(new Color(0, 150, 220).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(180, 240, 255).ToVector4());
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
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/CircleGlow").Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Color glowCol = new Color(153, 255, 255) * 0.4f;
            sb.Draw(glowTex, Projectile.Center - Main.screenPosition, null, glowCol, 0f, glowOrigin, Projectile.scale * 1.5f, SpriteEffects.None, 0f);
            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = texture2D13.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            sb.Draw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, Color.White, Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
}
}
