using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using ReLogic.Content;

namespace jarkos.projectiles.magic.staffs
{
    public class ShadowInkProj : ModProjectile
    {
        public override string Texture => "jarkos/textures/Circular_glow";

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
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();


            NPC target = FindTarget(650f);
            if (target != null)
            {
                Vector2 targetDir = Vector2.Normalize(target.Center - Projectile.Center);
                float speed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * speed, 0.10f);
            }


            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, -Projectile.velocity * 0.2f, 100, new Color(30, 0, 50), Main.rand.NextFloat(0.8f, 1.3f));
                dust.noGravity = true;
            }
        }

        private NPC FindTarget(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }

        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = -0.3f }, Projectile.Center);


            float explosionRadius = 90f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) <= explosionRadius)
                {
                    npc.SimpleStrikeNPC(Projectile.damage / 2, 0, false, 0f);
                }
            }


            new Circular_glow(Projectile.Center, Vector2.Zero, new Color(20, 0, 40), 20, 0.7f, 1.2f).Spawn();

            for (int i = 0; i < 18; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, vel, 100, new Color(40, 10, 60), Main.rand.NextFloat(1f, 1.8f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.MagicPixel.Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float width = MathHelper.Lerp(14f, 1f, progress);
                Color trailColor = Color.Lerp(new Color(140, 30, 200), new Color(20, 0, 40), progress) * (1f - progress) * 0.6f;

                sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), trailColor, Projectile.oldRot[i], new Vector2(0f, 0.5f), new Vector2(10f, width), SpriteEffects.None, 0f);
            }


            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            if (glowTex != null)
            {
                Vector2 origin = glowTex.Size() * 0.5f;
                sb.Draw(glowTex, Projectile.Center - Main.screenPosition, null, new Color(140, 40, 200) * 0.8f, 0f, origin, 0.38f, SpriteEffects.None, 0f);
                sb.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Black * 0.9f, 0f, origin, 0.28f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
