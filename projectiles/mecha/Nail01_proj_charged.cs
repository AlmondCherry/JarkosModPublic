using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using jarkos.systems;
using System;

namespace Jarkos.projectiles.mecha
{
    public class Nail01_proj_charged : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Nail01_proj";
        private Vector2[] oldPositions = new Vector2[15];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            for (int i = oldPositions.Length - 1; i > 0; i--)
            {
                oldPositions[i] = oldPositions[i - 1];
            }
            oldPositions[0] = Projectile.Center;
            if (Projectile.ai[0] == 0f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Main.rand.NextBool(4))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.2f);
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                }
            }
            else if (Projectile.ai[0] == 1f)
            {
                int targetIndex = (int)Projectile.ai[1];
                NPC target = Main.npc[targetIndex];
                if (!target.active || target.life <= 0)
                {
                    FallOff();
                    return;
                }
                if (Projectile.timeLeft <= 30)
                {
                    FallOff();
                    return;
                }
                Projectile.Center = target.Center + new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                Projectile.gfxOffY = target.gfxOffY;
                if (Projectile.owner == Main.myPlayer)
                {
                    CheckForExplosion(targetIndex);
                }
            }
            else if (Projectile.ai[0] == 2f)
            {
                Projectile.velocity.Y += 0.4f;
                Projectile.velocity.X *= 0.98f;
                Projectile.rotation += Projectile.velocity.X * 0.05f;
                Projectile.alpha += (int)(255f / 30f);
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }
        private void FallOff()
        {
            Projectile.ai[0] = 2f;
            Projectile.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -1f));
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
        }
        private void CheckForExplosion(int targetIndex)
        {
            List<Projectile> stuckNails = new List<Projectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Projectile.type && p.owner == Projectile.owner && p.ai[0] == 1f && p.ai[1] == targetIndex)
                {
                    stuckNails.Add(p);
                }
            }
            if (stuckNails.Count >= 3)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.npc[targetIndex].Center, Vector2.Zero, ModContent.ProjectileType<NailgunDistortionExplosion>(), Projectile.damage * 3, Projectile.knockBack * 2f, Projectile.owner);
                foreach (Projectile nail in stuckNails)
                {
                    nail.Kill();
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] == 0f)
            {
                Projectile.ai[0] = 1f;
                Projectile.ai[1] = target.whoAmI;
                Projectile.localAI[0] = Projectile.Center.X - target.Center.X;
                Projectile.localAI[1] = Projectile.Center.Y - target.Center.Y;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.timeLeft = 180;
                Projectile.netUpdate = true;
                target.immune[Projectile.owner] = 0;
                SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (Projectile.alpha < 200 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f * (1f - Projectile.alpha / 255f));
                effect.Parameters["uColor"]?.SetValue(new Color(255, 100, 20).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 200, 100).ToVector4());
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < oldPositions.Length; i++)
                {
                    if (oldPositions[i] != Vector2.Zero)
                    {
                        points.Add(oldPositions[i]);
                    }
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(8f, 0f, t), t => Color.White * (1f - t) * (1f - Projectile.alpha / 255f), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Vector2 origin = texture.Size() / 2f;
            Color drawColor = Projectile.GetAlpha(lightColor);
            spriteBatch.Draw(texture, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            if (Projectile.ai[0] == 1f)
            {
                Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                float pulse = 0.6f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f);
                spriteBatch.Draw(glowTex, drawPos, null, new Color(255, 150, 50) * pulse, 0f, glowTex.Size() / 2f, 0.3f * pulse, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
