using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using jarkos.systems;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_dagger : ModProjectile
    {
        private const int TrailLength = 30;
        private List<Vector2> trailPositions = new List<Vector2>();
        public override string Texture => "Terraria/Images/Projectile_625";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > TrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Player player = Main.player[(int)Projectile.ai[0]];
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] < 60)
            {
                Projectile.velocity *= 0.96f;
            }
            else if (Projectile.localAI[0] == 60)
            {
                if (player.active && !player.dead)
                {
                    Projectile.velocity = (player.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 2f;
                }
            }
            else
            {
                Projectile.velocity *= 1.05f;
                if (Projectile.velocity.Length() > 24f)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 24f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(10, 10, 10).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(Color.White.ToVector4());
                List<Vector2> points = new List<Vector2>();
                points.Add(Projectile.Center);
                points.AddRange(trailPositions);
                TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(25f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, new Rectangle(0,0,1,1), Color.Black, Projectile.rotation, new Vector2(0.5f), new Vector2(8f, 30f), SpriteEffects.None, 0f);
            return false;
        }
    }
}
