using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.magic.staffs
{
    public class Rainfire_bolt : ModProjectile
    {
        public override string Texture => "jarkos/textures/MagicSphereProj";

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
            Projectile.light = 0.9f;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;


            Player player = Main.player[Projectile.owner];
            float manaRatio = (player.active && !player.dead && player.statManaMax2 > 0) ? (float)player.statMana / player.statManaMax2 : 1f;
            float manaSpeedMult = MathHelper.Lerp(2f, 1f, MathHelper.Clamp(manaRatio, 0f, 1f));


            float homingRadius = 450f;
            NPC targetNPC = null;
            float closestDist = homingRadius;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {

                    if (Projectile.Center.Y > npc.Center.Y)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targetNPC = npc;
                        }
                    }
                }
            }

            if (targetNPC != null)
            {

                Vector2 targetDir = (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = Math.Max(Projectile.velocity.Length(), 16f * manaSpeedMult);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * speed, 0.14f);

                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * 0.2f, 100, default, 1.4f);
                    d.noGravity = true;
                }
            }
            else
            {

                Projectile.velocity.Y += 0.12f * manaSpeedMult;
            }


            if (Main.rand.NextBool(3))
            {
                Color pColor = Main.rand.NextBool() ? new Color(255, 140, 20) : new Color(255, 60, 20);
                new jarkos.particles.Dust1_particle(Projectile.Center, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f), pColor, 15, Main.rand.NextFloat(0.1f, 0.5f), 1f).Spawn();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(5f, 5f);
                Color pColor = Main.rand.NextBool() ? new Color(255, 160, 30) : new Color(255, 80, 20);
                new jarkos.particles.Dust1_particle(target.Center, speed, pColor, 20, Main.rand.NextFloat(0.1f, 0.5f), 1.2f).Spawn();
            }
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.2f }, target.Center);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ScalingArmorPenetration += 1f;
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
                effect.Parameters["uOpacity"]?.SetValue(1f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 100, 20).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 230, 120).ToVector4());
                TrailRenderer.Draw(
                    effect,
                    points,
                    t => 14f * (1f - t) + 0.5f,
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
            Color glowCol = new Color(255, 140, 20) * 0.45f;


            sb.Draw(glowTex, Projectile.Center - Main.screenPosition, null, glowCol, 0f, glowOrigin, Projectile.scale * 0.433f, SpriteEffects.None, 0f);

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = texture.Size() / 2f;
            sb.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
