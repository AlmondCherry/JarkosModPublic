using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using jarkos.systems;
using Terraria.GameContent;

namespace jarkos.projectiles.ranged.firearm.beamrifle
{
    public class Antique_beam_side_proj : ModProjectile
    {
        private List<Vector2> trailPositions;
        private List<float> trailRotations;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void OnSpawn(IEntitySource source)
        {
            trailPositions = new List<Vector2>();
            trailRotations = new List<float>();
        }
        public override void AI()
        {
            if (trailPositions != null)
            {
                trailPositions.Insert(0, Projectile.Center);
                trailRotations.Insert(0, Projectile.rotation);
                if (trailPositions.Count > 20)
                {
                    trailPositions.RemoveAt(trailPositions.Count - 1);
                    trailRotations.RemoveAt(trailRotations.Count - 1);
                }
            }
            if (Projectile.ai[0] == 0f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, 0f, 0f, 0, default, 1.5f);
                d.noGravity = true;
                float homingRadius = 400f;
                NPC target = null;
                float closestDist = homingRadius;
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            target = npc;
                        }
                    }
                }
                if (target != null)
                {
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 24f, 0.1f);
                }
                if (Projectile.timeLeft < 400 && target == null)
                {
                    StartPulsar();
                }
            }
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation += 0.2f;
                Projectile.friendly = false;
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), 0, 0, DustID.YellowTorch);
                d.noGravity = true;
                d.velocity = (Projectile.Center - d.position) * 0.1f;
                Projectile.ai[1]++;
                if (Projectile.ai[1] >= 8)
                {
                    Projectile.ai[1] = 0;
                    Projectile.ai[2]++;
                    SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
                    if (Projectile.owner == Main.myPlayer)
                    {
                        float baseAngle = Projectile.ai[2] * MathHelper.ToRadians(30f);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 vel = (baseAngle + MathHelper.PiOver2 * i).ToRotationVector2() * 16f;
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<Antique_beam_mini>(), (int)(Projectile.damage * 0.5f), 0f, Projectile.owner);
                        }
                    }
                    if (Projectile.ai[2] >= 5)
                    {
                        Projectile.Kill();
                    }
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] == 0f)
            {
                StartPulsar();
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 0f)
            {
                StartPulsar();
                return false;
            }
            return false;
        }
        private void StartPulsar()
        {
            Projectile.ai[0] = 1f;
            Projectile.ai[1] = 0f;
            Projectile.ai[2] = 0f;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions != null && trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                try
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    effect.Parameters["uOpacity"]?.SetValue(1.0f);
                    effect.Parameters["uColor"]?.SetValue(new Vector4(1f, 0.9f, 0.2f, 1f));
                    effect.Parameters["uColorCore"]?.SetValue(new Vector4(1f, 1f, 0.8f, 1f));
                    TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(12f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                finally
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
