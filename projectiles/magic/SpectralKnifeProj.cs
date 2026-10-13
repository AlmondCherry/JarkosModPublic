using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos;

namespace jarkos.projectiles.magic
{
    public class SpectralKnifeProj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Phantom_proj01";

        public bool IsEmpowered => Projectile.ai[0] == 1f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        private float _randomScale = 0f;

        public override void AI()
        {
            if (_randomScale == 0f)
            {
                _randomScale = Main.rand.NextFloat(0.7f, 1.4f);
                if (IsEmpowered) _randomScale *= 1.25f;
                Projectile.scale = _randomScale;
            }

            Projectile.ai[1]++;
            if (Projectile.ai[1] > 12f)
            {
                Projectile.tileCollide = true;
            }

            
            float searchRadius = IsEmpowered ? 550f : 400f;
            NPC target = FindClosestNPC(searchRadius);
            if (target != null)
            {
                Vector2 targetDir = Vector2.Normalize(target.Center - Projectile.Center);
                float turnSpeed = IsEmpowered ? 0.15f : 0.09f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * (IsEmpowered ? 22f : 18f), turnSpeed);
            }

            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            
            if (Main.netMode != NetmodeID.Server)
            {
                Color particleColor = IsEmpowered ? Color.Cyan : new Color(100, 220, 255);
                Color endColor = IsEmpowered ? Color.DeepSkyBlue : Color.Turquoise;

                
                Vector2 offset = Main.rand.NextVector2Circular(3f, 3f);
                float pScale = IsEmpowered ? Main.rand.NextFloat(0.35f, 0.65f) : Main.rand.NextFloat(0.2f, 0.45f);
                new SpectralPlasmaParticle(Projectile.Center + offset, -Projectile.velocity * 0.15f, particleColor, endColor, 18, pScale, 0.9f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();

                if (Main.rand.NextBool(12))
                {
                    new Spark(Projectile.Center, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), particleColor, 14, pScale * 0.8f, 0.8f).Spawn();
                }

                if (IsEmpowered && Main.rand.NextBool(8))
                {
                    new StarParticle(Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), Color.White, 15, 0.25f, 0.9f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }
            }

            Lighting.AddLight(Projectile.Center, IsEmpowered ? 0.3f : 0.15f, 0.7f, 0.9f);
        }

        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float minDistance = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<Screenshake_system>();
            if (modPlayer != null)
            {
                modPlayer.weak_shake = IsEmpowered ? 2.5f : 1.2f;
            }

            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/phantom_javelin_impact") with { Pitch = Main.rand.NextFloat(-0.2f, 0.4f), Volume = 0.7f }, Projectile.position);

            target.AddBuff(BuffID.Frostburn, 180);
            if (IsEmpowered)
            {
                target.AddBuff(BuffID.ShadowFlame, 120);
            }

            
            if (Main.netMode != NetmodeID.Server)
            {
                int count = IsEmpowered ? 4 : 2;
                for (int i = 0; i < count; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    Color startCol = IsEmpowered ? Color.Cyan : Color.DeepSkyBlue;
                    Color endCol = Color.Turquoise;
                    new SpectralPlasmaParticle(Projectile.Center, vel, startCol, endCol, 22, Main.rand.NextFloat(0.3f, 0.7f), 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }

                
                int fireCount = Main.rand.Next(2, 4);
                for (int i = 0; i < fireCount; i++)
                {
                    Vector2 fVel = Main.rand.NextVector2Circular(7f, 7f);
                    new jarkos.particles.storm.Fire_particle(Projectile.Center, fVel, Color.Cyan, 200, Main.rand.NextFloat(0.8f, 1.3f), 1f, Main.rand.NextFloat(-0.15f, 0.15f)).Spawn();
                }

                
                int elecCount = Main.rand.Next(2, 4);
                for (int i = 0; i < elecCount; i++)
                {
                    Vector2 eVel = Main.rand.NextVector2Circular(9f, 9f);
                    new jarkos.particles.Electric_particle(Projectile.Center, eVel, new Color(50, 200, 255), Main.rand.Next(60, 112), Main.rand.NextFloat(0.45f, 0.75f), 1f, 0.92f).Spawn();
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                new SpectralRingParticle(Projectile.Center, Color.DeepSkyBlue * 0.7f, 12, 0.1f, 0.35f, 0.7f).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                float progress = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                Color color = (IsEmpowered ? Color.Cyan : Color.DeepSkyBlue) * progress * 0.65f;
                float scale = Projectile.scale * (0.8f + 0.3f * progress) * (IsEmpowered ? 1.3f : 1.0f);
                float rot = Projectile.oldRot[k] != 0f ? Projectile.oldRot[k] : Projectile.rotation;

                spriteBatch.Draw(texture, drawPos, null, color, rot, drawOrigin, scale, SpriteEffects.None, 0f);
            }

            
            Vector2 mainDrawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color mainColor = IsEmpowered ? Color.White : new Color(200, 240, 255);
            float mainScale = Projectile.scale * (IsEmpowered ? 1.25f : 1.0f);
            spriteBatch.Draw(texture, mainDrawPos, null, mainColor, Projectile.rotation, drawOrigin, mainScale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            return false;
        }
    }
}
