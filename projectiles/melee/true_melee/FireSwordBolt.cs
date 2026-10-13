using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.particles.storm;

namespace jarkos.projectiles.melee.true_melee
{
    public class FireSwordBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 22;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 210;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
        }
        private ref float WavePhase => ref Projectile.ai[0];
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 1.1f, 0.55f, 0.12f);
            float waveFade = MathHelper.Clamp((210 - Projectile.timeLeft) / 30f, 0f, 1f);
            float wave = MathF.Sin(Projectile.timeLeft * 0.25f + WavePhase) * 0.35f * waveFade;
            Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Projectile.position += perp * wave * 2.2f;

            if (Projectile.timeLeft < 175)
            {
                NPC target = FindTarget(340f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = Projectile.velocity.Length();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * speed, 0.045f);
                }
            }
            if (Main.netMode != NetmodeID.Server)
            {
                if (Projectile.timeLeft % 2 == 0)
                {
                    float swirl = Main.GlobalTimeWrappedHourly * 18f + WavePhase;
                    Vector2 orbit = swirl.ToRotationVector2() * 10f;
                    new Fire_particle(Projectile.Center + orbit, -Projectile.velocity * 0.15f, new Color(255, 140, 10), lifetime: Main.rand.Next(6, 10), scale: Main.rand.NextFloat(0.3f, 0.5f), opacity: 0.8f).Spawn();
                }
                if (Main.rand.NextBool(2))
                {
                    new Fire_particle(Projectile.Center, -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1.2f, 1.2f), new Color(255, 140, 10), lifetime: Main.rand.Next(6, 10), scale: Main.rand.NextFloat(0.35f, 0.6f), opacity: 0.8f).Spawn();
                }
                if (Main.rand.NextBool(10))
                {
                    new Spark_gravit(Projectile.Center, Main.rand.NextVector2Circular(1.5f, 1.5f), new Color(255, 80, 0), lifetime: Main.rand.Next(8, 12), scale: Main.rand.NextFloat(0.2f, 0.35f), opacity: 0.9f).Spawn();
                }
            }
        }
        private NPC FindTarget(float range)
        {
            NPC best = null;
            float bestDist = range;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(Projectile))
                    continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = npc;
                }
            }
            return best;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = (MathHelper.TwoPi / 12f * i).ToRotationVector2() * Main.rand.NextFloat(2f, 6f);
                    new Spark(Projectile.Center, vel, new Color(255, 180, 20), lifetime: Main.rand.Next(6, 12), scale: Main.rand.NextFloat(0.35f, 0.7f), opacity: 1f).Spawn();
                }
            }
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FireExplosion>(), (int)(Projectile.damage * 1.1f), Projectile.knockBack, Projectile.owner);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            int len = ProjectileID.Sets.TrailCacheLength[Type];

            for (int i = len - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;
                float t = 1f - i / (float)len;
                Color c = Color.Lerp(new Color(180, 30, 0, 0), new Color(255, 160, 40, 0), t) * (t * 0.8f);
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                Main.spriteBatch.Draw(tex, pos, null, c, Projectile.oldRot[i], origin, Projectile.scale * (0.4f + t * 0.8f), SpriteEffects.None, 0f);
            }
            float pulse = 1f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 20f + WavePhase);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 220, 120, 0), Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
