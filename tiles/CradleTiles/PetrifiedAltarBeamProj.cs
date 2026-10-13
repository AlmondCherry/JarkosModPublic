using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Jarkos.trails;
using System;
namespace jarkos.tiles.CradleTiles
{
    public class PetrifiedAltarBeamProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_1";
        private Primitivedraw beamTrail;
        public List<Vector2> segmentPoints = new List<Vector2>();
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = PetrifiedAltarTileEntity.RitualDurationTicks;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            segmentPoints.Clear();
            Vector2 start = Projectile.Center;
            float maxTime = PetrifiedAltarTileEntity.RitualDurationTicks;
            float timeAlive = maxTime - Projectile.timeLeft;
            float heightMultiplier = 1f;
            if (timeAlive < 30f) {
                heightMultiplier = timeAlive / 30f;
            } else if (Projectile.timeLeft < 30f) {
                heightMultiplier = Projectile.timeLeft / 30f;
            }
            Vector2 end = start + new Vector2(0, -1200f * heightMultiplier);
            int segments = 20;
            for (int i = 0; i <= segments; i++)
            {
                float wave = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + (i * 0.5f)) * 4f * heightMultiplier;
                segmentPoints.Add(Vector2.Lerp(start, end, i / (float)segments) + new Vector2(wave, 0));
            }
            Lighting.AddLight(Projectile.Center, 0.8f, 0.5f, 0.2f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (segmentPoints.Count < 2) return false;
            if (beamTrail == null)
            {
                beamTrail = new Primitivedraw(interpolant => 45f * GetScale(), interpolant => new Color(255, 165, 70) * GetScale(), GameShaders.Misc["Jarkos:LaserShader"]);
            }
            float scale = GetScale();
            MiscShaderData shader = GameShaders.Misc["Jarkos:LaserShader"];
            shader.UseColor(new Color(255, 200, 100));
            shader.UseSecondaryColor(new Color(255, 120, 30));
            shader.UseOpacity(scale);
            Main.spriteBatch.End();
            beamTrail.DrawPrims(segmentPoints, -Main.screenPosition, segmentPoints.Count);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private float GetScale()
        {
            float maxTime = PetrifiedAltarTileEntity.RitualDurationTicks;
            float timeAlive = maxTime - Projectile.timeLeft;
            if (timeAlive < 30f)
            {
                return timeAlive / 30f;
            }
            else if (Projectile.timeLeft < 30f)
            {
                return Projectile.timeLeft / 30f;
            }
            return 1f;
        }
    }
}
