using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using jarkos.UI.Common;

namespace jarkos.UI
{
    public class CosmicBackground
    {
        private Effect _shader;
        private bool _triedLoad;
        private float _time;

        private static readonly Random _rng = new(1337); 

        private struct Star
        {
            public Vector2 Pos;
            public float Phase;
            public int Type;
            public float Scale;
            public float SpeedMod;
        }

        private Star[] _stars;

        private Effect Shader
        {
            get
            {
                if (_triedLoad) return _shader;

                _triedLoad = true;

                try
                {
                    if (ModContent.HasAsset("jarkos/Shaders/Nebula"))
                        _shader = ModContent.Request<Effect>("jarkos/Shaders/Nebula", AssetRequestMode.ImmediateLoad).Value;
                }
                catch { _shader = null; }

                return _shader;
            }
        }

        public void Update(float dt) => _time += dt;

        public void Draw(SpriteBatch sb, TreeCamera cam, int qualityLevel, float opacity = 1f)
        {
            sb.Draw(Common.Draw.Pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(6, 7, 18) * opacity);

            bool shaderDrawn = false;
            var fx = Shader;

            if (fx != null && qualityLevel > 0)
            {
                try
                {
                    fx.Parameters["uTime"]?.SetValue(_time);
                    fx.Parameters["uResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                    fx.Parameters["uPan"]?.SetValue(cam.Offset);
                    fx.Parameters["uZoom"]?.SetValue(cam.Zoom);
                    fx.Parameters["uQuality"]?.SetValue((float)qualityLevel);

                    sb.End();
                    sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, fx, Main.UIScaleMatrix);

                    sb.Draw(Common.Draw.Pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * opacity);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                    
                    shaderDrawn = true;
                }
                catch { }
            }
            if (!shaderDrawn) DrawFallback(sb, cam, Main.screenWidth, Main.screenHeight, qualityLevel, opacity);
            DrawStars(sb, cam, Main.screenWidth, Main.screenHeight, opacity);
        }
        private void DrawFallback(SpriteBatch sb, TreeCamera cam, int width, int height, int qualitylevel, float opacity) 
        {
            int blobs = qualitylevel <= 0 ? 0 : (qualitylevel == 1 ? 4 : 7); 

            for (int i = 0; i < blobs; i++)
            {
                float px = Easing.Breathe(_time, 0.05f + i * 0.02f, i) * width;
                float py = Easing.Breathe(_time, 0.04f + i * 0.015f, i * 2f) * height;
                var col = Color.Lerp(new Color(15, 20, 45), new Color(25, 55, 95), i % 3 / 2f);
                
                Common.Draw.RadialGlow(sb, new Vector2(px, py) - cam.Offset * 0.02f, 260f + i * 40f, col * (0.15f * opacity));
            }
        }
        private void DrawStars(SpriteBatch sb, TreeCamera cam, int width, int height, float opacity)
        {
            if (_stars == null)
            {
                _stars = new Star[250];
                for (int i = 0; i < _stars.Length; i++)
                {
                    _stars[i].Pos = new Vector2(_rng.Next(0, 8000), _rng.Next(0, 8000));
                    _stars[i].Phase = (float)(_rng.NextDouble() * Math.PI * 2);
                    
                    int r = _rng.Next(100);

                    _stars[i].Type = r < 60 ? 0 : (r < 90 ? 1 : 2);
                    _stars[i].Scale = 0.3f + (float)_rng.NextDouble() * 0.5f;
                    _stars[i].SpeedMod = 0.5f + (float)_rng.NextDouble() * 1.0f;
                }
            }

            Texture2D texSmall = null;
            Texture2D texMed = null;
            Texture2D texLarge = null;

            try
            {
                texSmall = ModContent.Request<Texture2D>("jarkos/UI/Textures/StarSmall").Value;
                texMed = ModContent.Request<Texture2D>("jarkos/UI/Textures/StarMedium").Value;
                texLarge = ModContent.Request<Texture2D>("jarkos/UI/Textures/StarLarge").Value;
            }
            catch { return; }

            if (texSmall == null || texMed == null || texLarge == null) return;

            float pSmall = 0.03f;
            float pMed = 0.08f;
            float pLarge = 0.15f;

            for (int i = 0; i < _stars.Length; i++)
            {
                var stars = _stars[i];
                float p = 0;

                Texture2D tex = texSmall;
                
                if (stars.Type == 0) { p = pSmall; tex = texSmall; }
                else if (stars.Type == 1) { p = pMed; tex = texMed; }
                else { p = pLarge; tex = texLarge; }

                float speedMult = (1f + stars.Type * 0.5f) * stars.SpeedMod;
                float driftX = _time * -4f * speedMult;
                float driftY = _time * -6f * speedMult;

                float x = Mod(stars.Pos.X - cam.Offset.X * p + driftX, width + 200) - 100;
                float y = Mod(stars.Pos.Y - cam.Offset.Y * p + driftY, height + 200) - 100;
                
                float maxBrightness = 0.15f + stars.Type * 0.1f;
                float tw = 0.3f + 0.7f * MathF.Sin(_time * (1.2f + stars.Type * 0.4f) + stars.Phase);

                Color col = Color.White * (tw * maxBrightness * opacity);
                sb.Draw(tex, new Vector2(x, y), null, col, 0f, tex.Size() / 2f, stars.Scale, SpriteEffects.None, 0f);
            }
        }
        private static float Mod(float a, float m) 
        { 
            a %= m;
            return a < 0 ? a + m : a; 
        }
    }
}
