using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee
{
    public class SunfateHealRay : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; 

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9; 
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int playerIndex = (int)Projectile.ai[0];
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers) 
                return false;

            Player player = Main.player[playerIndex];
            if (!player.active || player.dead) 
                return false;

            float progress = Projectile.timeLeft / 9f; 

            Vector2 start = Projectile.Center;
            Vector2 end = player.MountedCenter;

            
            Effect effect = HealShaders.Stream;
            if (effect != null && HealShaders.HealNoise != null)
            {
                Main.spriteBatch.End();

                
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 12f);
                effect.Parameters["uOpacity"]?.SetValue(progress);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 130, 0).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(Color.White.ToVector4());

                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i <= 8; i++)
                {
                    points.Add(Vector2.Lerp(start, end, i / 8f));
                }

                TrailRenderer.Draw(effect, points.ToArray(), t => 8f * progress, color => Color.White, HealShaders.HealNoise);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(player.Center, DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.2f);
                d.noGravity = true;
            }

            return false;
        }
    }
}
