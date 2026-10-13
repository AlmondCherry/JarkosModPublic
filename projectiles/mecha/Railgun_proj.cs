using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos;
using jarkos.systems;
using jarkos.systems;
namespace Jarkos.projectiles.mecha
{
    public class Railgun_proj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.light = 0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.rotation = Projectile.velocity.ToRotation();
                float distance = 3000f;
                Projectile.ai[0] = distance;

                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.8f, Pitch = -0.2f }, Projectile.Center);
                    Vector2 hitPoint = Projectile.Center + Projectile.velocity * distance;
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 vel = -Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 10f);
                        Dust dust = Dust.NewDustPerfect(hitPoint, DustID.Electric, vel, 0, default, 1.5f);
                        dust.noGravity = true;
                        if (i % 2 == 0)
                        {
                            new Spark(hitPoint, vel * 1.5f, Color.Cyan, 25, Main.rand.NextFloat(0.5f, 1.5f), 1f, 0f).Spawn();
                        }
                    }
                    float ringInterval = 120f;
                    for (float i = 60f; i < distance - 30f; i += ringInterval)
                    {
                        Vector2 ringPos = Projectile.Center + Projectile.velocity * i;
                        new Circleedge_tight(ringPos, Vector2.Zero, Color.Cyan, 12, 1.5f, 1.5f).Spawn();
                    }
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 vel = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(4f, 10f);
                        Dust.NewDustPerfect(Projectile.Center, DustID.Electric, vel, 0, default, 1.2f).noGravity = true;
                    }
                }
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 1.6f, 2.4f);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            Vector2 endPoint = Projectile.Center + Projectile.velocity * Projectile.ai[0];
            float thickness = 120f * (Projectile.timeLeft / 120f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, endPoint, thickness, ref point);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 speed = Projectile.velocity.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(4f, 10f);
                    new Spark(target.Center, speed, Color.Cyan, 15, Main.rand.NextFloat(0.5f, 1.0f), 1f, 0f).Spawn();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float distance = Math.Max(Projectile.ai[0], 3000f);
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                float lifeRatio = Projectile.timeLeft / 120f;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 15f);
                effect.Parameters["uOpacity"]?.SetValue(2f * lifeRatio);
                effect.Parameters["uColor"]?.SetValue(new Color(0, 200, 255).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(Color.White.ToVector4());
                int numPoints = 60;
                Vector2[] points = new Vector2[numPoints];
                for (int i = 0; i < numPoints; i++)
                {
                    points[i] = Projectile.Center + Projectile.velocity * (distance * (i / (float)(numPoints - 1)));
                }
                TrailRenderer.Draw(effect, points, t => 120f * lifeRatio * MathHelper.Clamp(t * 20f, 0f, 1f), t => Color.White * (1f - t), HealShaders.HealNoise);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else if (Main.netMode != NetmodeID.Server)
            {
                Texture2D beam = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/BeamStrip", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                float lifeRatio = Projectile.timeLeft / 120f;
                Color c = Color.Cyan * lifeRatio;
                Vector2 origin = new Vector2(0f, beam.Height / 2f);
                Main.spriteBatch.Draw(beam, Projectile.Center - Main.screenPosition, null, c, Projectile.rotation, origin, new Vector2(distance / beam.Width, (120f * lifeRatio) / beam.Height), SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
