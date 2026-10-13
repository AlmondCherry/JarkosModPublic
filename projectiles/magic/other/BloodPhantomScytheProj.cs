using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using jarkos.particles;
using Terraria.Audio;
using jarkos.systems;
using System.Collections.Generic;

namespace jarkos.projectiles.magic.other
{
    public class BloodPhantomScytheProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 420;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 20;
        }
        public override void AI()
        {
            float speed = Projectile.velocity.Length();
            Projectile.rotation += (0.15f + speed * 0.035f) * Projectile.direction;

            if (Projectile.ai[0] == 0)
            {
                Projectile.velocity *= 0.94f;
                NPC target = null;
                float closestDist = 800f;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                        {
                            closestDist = dist;
                            target = npc;
                        }
                    }
                }

                if (target != null && speed < 6f)
                {
                    Projectile.ai[0] = 1;
                    Projectile.ai[1] = target.whoAmI;
                    Projectile.netUpdate = true;
                }
            }
            else if (Projectile.ai[0] == 1)
            {
                NPC target = Main.npc[(int)Projectile.ai[1]];

                if (target.active && !target.dontTakeDamage)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();

                    float maxSpeed = 24f;

                    Vector2 targetVelocity = direction * maxSpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.12f);
                }
                else
                {
                    Projectile.ai[0] = 0;
                }
            }
            else if (Projectile.ai[0] == 2)
            {
                Projectile.friendly = false;
                Projectile.velocity *= 0.82f;
                Projectile.alpha += 12;

                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 0, 100, default, 1f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] != 2)
            {
                Projectile.ai[0] = 2;
                Projectile.netUpdate = true;

                if (!Main.dedServ)
                {
                    new Circleedge(Projectile.Center, Vector2.Zero, Color.DarkRed, 40, 0.5f, 1f, 0f).Spawn();

                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2Circular(8f, 8f);
                        new Spark(Projectile.Center, sparkVel, Color.Red, 35, Main.rand.NextFloat(0.8f, 1.2f), 1f, 0f).Spawn();
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                List<Vector2> trailPositions = new List<Vector2>();
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                        trailPositions.Add(Projectile.oldPos[i] + Projectile.Size / 2f);
                }
                if (trailPositions.Count > 1)
                {
                    Effect effect = HealShaders.Stream;
                    Main.spriteBatch.End();
                    try
                    {
                        effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                        effect.Parameters["uOpacity"]?.SetValue(1.5f);
                        effect.Parameters["uColor"]?.SetValue(new Vector4(1f, 0.1f, 0.1f, 1f));
                        effect.Parameters["uColorCore"]?.SetValue(new Vector4(1f, 0.6f, 0.6f, 1f));
                        TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(24f, 0f, t), t => Color.White * (1f - t) * ((255 - Projectile.alpha) / 255f), HealShaders.HealNoise);
                    }
                    finally
                    {
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }
            }

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Vector2 origin = texture.Size() / 2f;
            Color drawColor = new Color(255, 50, 50, 100) * ((255 - Projectile.alpha) / 255f);

            Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale * 1.3f, SpriteEffects.None, 0);
            return false;
        }
    }
}
