using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.trails;
using jarkos.particles;
using jarkos.UI.Common;

namespace Jarkos.projectiles.mecha
{
    public class Energy_knife_proj02 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_1";

        private List<Vector2> _lightningPoints = new List<Vector2>();
        private int timer = 0;
        private const int MaxLifetime = 20;

        private static Primitivedraw _outerTrail;
        private static Primitivedraw _innerTrail;

        public override void SetStaticDefaults()
        {

            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2000;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            timer++;

            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Vector2 end = Projectile.Center;

            if (_lightningPoints.Count == 0 || timer % 3 == 0)
            {
                GenerateLightningPoints(start, end);
            }

            float alpha = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);

            if (timer == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Electric_particle(start, Main.rand.NextVector2Circular(4f, 4f), new Color(100, 200, 255), Main.rand.Next(15, 25), Main.rand.NextFloat(0.35f, 0.6f), 1f, 0.92f).Spawn();
                    new Electric_particle(end, Main.rand.NextVector2Circular(4f, 4f), new Color(100, 200, 255), Main.rand.Next(15, 25), MathHelper.Lerp(0.35f, 0.6f, Main.rand.NextFloat()), 1f, 0.92f).Spawn();
                }
            }

            foreach (var pt in _lightningPoints)
            {
                Lighting.AddLight(pt, 0.2f * alpha, 0.5f * alpha, 0.95f * alpha);
            }
        }

        private void GenerateLightningPoints(Vector2 start, Vector2 end)
        {
            _lightningPoints.Clear();
            Vector2 dir = end - start;
            float dist = dir.Length();
            if (dist <= 1f) return;

            dir.Normalize();
            Vector2 normal = new Vector2(-dir.Y, dir.X);

            int segments = Math.Max(4, (int)(dist / 24f));
            _lightningPoints.Add(start);

            for (int s = 1; s < segments; s++)
            {
                float frac = s / (float)segments;
                Vector2 basePos = Vector2.Lerp(start, end, frac);
                float offset = Main.rand.NextFloat(-14f, 14f) * MathF.Sin(frac * MathF.PI);
                _lightningPoints.Add(basePos + normal * offset);
            }
            _lightningPoints.Add(end);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_lightningPoints.Count < 2) return false;

            float alpha = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);

            if (_outerTrail == null)
            {
                _outerTrail = new Primitivedraw(t => 14f, t => Color.Cyan);
            }
            if (_innerTrail == null)
            {
                _innerTrail = new Primitivedraw(t => 5f, t => Color.White);
            }

            _outerTrail.WidthFunc = t => 14f * MathF.Sin(t * MathF.PI) * alpha;
            _outerTrail.ColorFunc = t => Color.Lerp(Color.Cyan, Color.DeepSkyBlue, t) * (0.85f * alpha);

            _innerTrail.WidthFunc = t => 5f * MathF.Sin(t * MathF.PI) * alpha;
            _innerTrail.ColorFunc = t => Color.White * (0.95f * alpha);

            _outerTrail.DrawPixelPrims(_lightningPoints, -Main.screenPosition, _lightningPoints.Count);
            _innerTrail.DrawPixelPrims(_lightningPoints, -Main.screenPosition, _lightningPoints.Count);


            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 node1 = _lightningPoints[0] - Main.screenPosition;
            Vector2 node2 = _lightningPoints[_lightningPoints.Count - 1] - Main.screenPosition;

            Draw.RadialGlow(sb, node1, 28f * alpha, Color.Cyan * (0.85f * alpha));
            Draw.RadialGlow(sb, node1, 14f * alpha, Color.White * (0.90f * alpha));

            Draw.RadialGlow(sb, node2, 28f * alpha, Color.Cyan * (0.85f * alpha));
            Draw.RadialGlow(sb, node2, 14f * alpha, Color.White * (0.90f * alpha));

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
