using jarkos.particles.storm;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
    public class ChronoPortal : ModProjectile
    {
        public override string Texture => "Jarkos/textures/PortalHook/PortalDisk";

        private const int opentime = 20;
        private const int stabletime = 40;
        private const int closetime = 20;
        private const int totallifetime = opentime + stabletime + closetime;

        private float Timer = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 1;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = totallifetime;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            Timer++;

            NPC target = Main.npc[(int)Projectile.ai[0]];
            if (target.active && !target.friendly)
            {
                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
            }

            if (Timer >= opentime && Timer < opentime + stabletime)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                    offset.X *= 0.4f;
                    offset = offset.RotatedBy(Projectile.rotation);
                    
                    Vector2 particlePos = Projectile.Center + offset;
                    Vector2 particleVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 5f));
                    float size = Main.rand.NextFloat(0.4f, 0.8f);
                    
                    new Fire_particle(particlePos, particleVel, new Color(0, 160, 255), 45, size, 1.5f, Main.rand.NextFloat(-0.04f, 0.04f)).Spawn();
                }
            }

            if (Timer == opentime + 10)
            {
                Vector2 targetPos = target.active && !target.friendly ? target.Center : Projectile.Center + Projectile.rotation.ToRotationVector2() * 100f;
                Vector2 dir = (targetPos - Projectile.Center).SafeNormalize(Vector2.UnitX);

                SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.9f, Pitch = -0.3f }, Projectile.Center);

                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir * 18f, ModContent.ProjectileType<Laser_beam_arrow>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: 1f);
                }
            }
        }

        private float GetVisualScale()
        {
            if (Timer < opentime)
            {
                float t = Timer / opentime;
                return t * t * (3f - 2f * t);
            }
            if (Timer < opentime + stabletime)
            {
                return 1f + (float)Math.Sin(Timer * 0.15f) * 0.05f;
            }
            else
            {
                float t = (totallifetime - Timer) / closetime;
                t = MathHelper.Clamp(t, 0f, 1f);
                return t * t * (3f - 2f * t);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float scale = GetVisualScale();
            if (scale <= 0.001f)
                return false;

            float opacity = 1f;
            if (Timer < opentime)
                opacity = Timer / opentime;
            else if (Timer > opentime + stabletime)
                opacity = (totallifetime - Timer) / closetime;

            opacity = MathHelper.Clamp(opacity, 0f, 1f);

            Effect fx = ModContent.Request<Effect>("jarkos/Shaders/ChronoPortalShader", AssetRequestMode.ImmediateLoad).Value;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (fx != null)
            {
                fx.Parameters["uColor"]?.SetValue(new Color(0, 100, 255).ToVector3());
                fx.Parameters["uSecondaryColor"]?.SetValue(new Color(150, 220, 255).ToVector3());
                fx.Parameters["uOpacity"]?.SetValue(opacity);
                fx.Parameters["uScale"]?.SetValue(scale);
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);

                foreach (EffectPass pass in fx.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }

            Vector2 drawScale = new Vector2(scale * 0.28f, scale * 0.7f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White, Projectile.rotation, origin, drawScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
