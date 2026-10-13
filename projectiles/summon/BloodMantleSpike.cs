using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon
{
    public class BloodMantleSpike : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BloodRain;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] < 20f)
            {
                float curveAmount = 0.08f * Projectile.ai[1];
                Projectile.velocity = Projectile.velocity.RotatedBy(curveAmount);
                Projectile.velocity *= 1.02f;
            }
            else
            {
                NPC target = FindClosestNPC(700f);
                if (target != null)
                {
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float currentSpeed = Projectile.velocity.Length();
                    float targetSpeed = MathHelper.Clamp(currentSpeed + 0.4f, 10f, 18f);
                    Vector2 desiredVelocity = targetDir * targetSpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.07f);
                }
                else
                {
                    if (Projectile.velocity.Length() < 10f)
                    {
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
                    }
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 1.2f);
                d.noGravity = true;
                d.velocity *= 0.1f;
            }
            Lighting.AddLight(Projectile.Center, 0.5f, 0.05f, 0.05f);
        }
        private NPC FindClosestNPC(float maxDetectRadius)
        {
            NPC closestNPC = null;
            float closestDistance = maxDetectRadius;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                if (target.active && !target.friendly && target.lifeMax > 5 && target.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(Projectile.Center, target.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = target;
                    }
                }
            }
            return closestNPC;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (player.active && !player.dead)
            {
                int heal = 2;
                player.statLife += heal;
                player.HealEffect(heal, true);
                if (player.statLife > player.statLifeMax2)
                {
                    player.statLife = player.statLifeMax2;
                }
            }
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Pitch = 0.5f, Volume = 0.8f }, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Vector2 speed = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 8f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, speed.X, speed.Y, 100, default, 1.4f);
                d.noGravity = Main.rand.NextBool();
                Vector2 sparkSpeed = Main.rand.NextVector2Circular(3f, 3f);
                Dust d2 = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, sparkSpeed.X, sparkSpeed.Y, 100, default, 1.0f);
                d2.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Texture2D noise = HealShaders.HealNoise;
                Main.spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 20, 20).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 120, 100).ToVector4());
                int numPoints = Projectile.oldPos.Length;
                Vector2[] points = new Vector2[numPoints];
                Vector2 half = Projectile.Size * 0.5f;
                for (int i = 0; i < numPoints; i++)
                {
                    points[i] = Projectile.oldPos[i] == Vector2.Zero ? Projectile.Center : Projectile.oldPos[i] + half;
                }
                TrailRenderer.Draw(effect, points, t => 12f * MathF.Pow(1f - t, 0.5f), t => Color.White * (1f - t), noise);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow").Value;
            if (glowTex != null)
            {
                Color glowColor = new Color(255, 40, 40, 0) * 0.7f;
                Vector2 glowOrigin = glowTex.Size() / 2f;
                Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, glowOrigin, 0.45f, SpriteEffects.None, 0);
            }
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 150, 150), Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            return false;
        }
    }
}
