using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using jarkos.particles;

namespace jarkos.projectiles.magic.staffs
{
    public class SanguineDownpourSphere : ModProjectile
    {
        public override string Texture => "jarkos/particles/Circular_glow";

        public float ChargeScale => MathHelper.Clamp(Projectile.ai[0], 0.4f, 3.0f);

        private bool hasExploded = false;

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            float scale = ChargeScale;
            Projectile.rotation += 0.12f;


            if (Main.rand.NextFloat() < 0.7f + scale * 0.1f)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(12f * scale, 12f * scale);
                Vector2 dustVel = -Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Blood, dustVel, 100, default, Main.rand.NextFloat(1f, 1.6f) * scale);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Firework_Red, -Projectile.velocity * 0.2f, 100, default, 1.1f);
                fire.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.8f * (scale / 3f), 0.1f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ExplodeSphere();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            ExplodeSphere();
            return true;
        }

        public override void Kill(int timeLeft)
        {
            ExplodeSphere();
        }

        private void ExplodeSphere()
        {
            if (hasExploded)
                return;

            hasExploded = true;
            float scale = ChargeScale;
            float explosionRadius = 75f * scale;
            
            if (Projectile.owner == Main.myPlayer)
            {
                Player player = Main.player[Projectile.owner];
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.CanBeChasedBy())
                    {
                        float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                        if (distSq <= explosionRadius * explosionRadius)
                        {
                            int critChance = player != null ? player.GetWeaponCrit(player.HeldItem) : 10;
                            bool crit = Main.rand.Next(100) < critChance;
                            player.ApplyDamageToNPC(npc, (int)(Projectile.damage * (0.8f + scale * 0.3f)), Projectile.knockBack * (0.8f + scale * 0.3f), Projectile.direction, crit, DamageClass.Magic, true);
                        }
                    }
                }
            }

            float shockwaveScale = Math.Max(0.12f, 0.25f * scale);
            new Circleedge(Projectile.Center, Vector2.Zero, Color.Red, 26, shockwaveScale, 2.5f, 0.04f).Spawn();
            new Circleedge(Projectile.Center, Vector2.Zero, new Color(255, 40, 20), 20, shockwaveScale * 1.5f, 3.0f, -0.04f).Spawn();
            new Circleedge_tight(Projectile.Center, Vector2.Zero, new Color(255, 200, 160), 16, shockwaveScale * 0.8f, 2.0f).Spawn();
            new Stretching_particle_out(Projectile.Center, Vector2.Zero, Color.Red, 22, shockwaveScale * 1.4f, 2f, 1f).Spawn();


            int bloodDustCount = Math.Max(8, (int)(12 * scale));
            for (int i = 0; i < bloodDustCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f * scale, 3f * scale) * Main.rand.NextFloat(0.5f, 1.4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, vel, 100, default, Main.rand.NextFloat(1.1f, 2.2f) * scale);
                d.noGravity = true;
            }

            int fireworkCount = Math.Max(2, (int)(4 * scale));
            for (int i = 0; i < fireworkCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f * scale, 4f * scale);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Firework_Red, vel, 100, default, Main.rand.NextFloat(0.2f, 0.3f) * scale);
                d.noGravity = true;
            }

            float volume = MathHelper.Clamp(0.5f + scale * 0.2f, 0.4f, 1.0f);

            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ThermalScytheHit")  with { Pitch = Main.rand.NextFloat(-0.25f, 0.25f), Volume = volume * 0.8f }, Projectile.Center);

            if (scale >= 1.0f)
            {
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 4f * scale, 10f, 14, 1000f, "SanguineDownpourExplosion"));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            if (glowTex != null)
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float scale = ChargeScale;
                float pulse = 0.95f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.08f;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


                Color outerColor = new Color(255, 30, 20) * 0.9f * pulse;
                Main.spriteBatch.Draw(glowTex, drawPos, null, outerColor, Projectile.rotation, glowTex.Size() * 0.5f, scale * 0.50f * pulse, SpriteEffects.None, 0f);


                Color midColor = new Color(255, 80, 40) * 0.95f * pulse;
                Main.spriteBatch.Draw(glowTex, drawPos, null, midColor, -Projectile.rotation * 1.5f, glowTex.Size() * 0.5f, scale * 0.34f * pulse, SpriteEffects.None, 0f);


                Color innerColor = new Color(255, 240, 200) * 0.95f * pulse;
                Main.spriteBatch.Draw(glowTex, drawPos, null, innerColor, 0f, glowTex.Size() * 0.5f, scale * 0.18f * pulse, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
