using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.systems;
using Terraria.GameContent;

namespace jarkos.projectiles.melee.true_melee
{
    public class IgnisiumSword_silhouette : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/IgnisiumSword";

        private List<Vector2> trailPositions = new List<Vector2>();

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.35f * Projectile.direction;
            
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 15)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.SolarFlare, Projectile.velocity * 0.2f, 0, default, 1.2f);
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.4f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.5f);
                dust.noGravity = true;
            }

            if (Projectile.owner == Main.myPlayer)
            {
                int starsCount = Main.rand.Next(2, 4);
                for (int i = 0; i < starsCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel, ModContent.ProjectileType<IgnisiumSword_star>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions.Count > 1)
            {
                if (HealShaders.Stream != null && HealShaders.HealNoise != null)
                {
                    Effect effect = HealShaders.Stream;
                    Main.spriteBatch.End();
                    try
                    {
                        effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                        effect.Parameters["uOpacity"]?.SetValue(1.5f);
                        effect.Parameters["uColor"]?.SetValue(new Color(255, 120, 20, 255).ToVector4());
                        effect.Parameters["uColorCore"]?.SetValue(new Color(255, 200, 100, 255).ToVector4());
                        TrailRenderer.Draw(effect, trailPositions.ToArray(), t => MathHelper.Lerp(24f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                    }
                    finally
                    {
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
                    }
                }
                else
                {
                    Texture2D pixel = TextureAssets.MagicPixel.Value;
                    for (int i = 0; i < trailPositions.Count - 1; i++)
                    {
                        Vector2 pos1 = trailPositions[i] - Main.screenPosition;
                        Vector2 pos2 = trailPositions[i + 1] - Main.screenPosition;
                        float dist = Vector2.Distance(pos1, pos2);
                        float rot = (pos2 - pos1).ToRotation();
                        float progress = (float)i / trailPositions.Count;
                        Color color = new Color(255, 120, 20, 0) * (1f - progress);
                        Main.spriteBatch.Draw(pixel, pos1, new Rectangle(0, 0, 1, 1), color, rot, new Vector2(0, 0.5f), new Vector2(dist, 10f * (1f - progress)), SpriteEffects.None, 0f);
                    }
                }
            }

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale * 0.2f, SpriteEffects.None, 0f);
            
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 150, 50, 0) * 0.5f, Projectile.rotation, origin, Projectile.scale * 0.35f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
