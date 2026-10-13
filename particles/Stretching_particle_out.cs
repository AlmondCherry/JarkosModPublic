using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using System;
using Terraria.Graphics.Shaders;
using jarkos.systems;
using Terraria.DataStructures;

namespace jarkos.particles
{
    public class Stretching_particle_out : Particles
    {
        public float PixelationAmount;
        public float InitialLength;
        public float InitialThickness;
        public override string TexturePath => "jarkos/particles/Stretching_particle_out";
        public override BlendState DrawBlendState => BlendState.Additive;
        public Stretching_particle_out(Vector2 position, Vector2 velocity, Color color, int lifetime, float initialLength, float initialThickness, float pixelationAmount)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Lifetime = lifetime;
            InitialLength = initialLength;
            InitialThickness = initialThickness;
            PixelationAmount = pixelationAmount;
            Opacity = 1f;
        }
        public override void Update()
        {
            float progress = (float)Time / Lifetime;
            Opacity = MathHelper.Lerp(1f, 0f, (float)Math.Pow(progress, 3));
            if (Time >= Lifetime)
            {
                Kill();
            }
        }
        public override void Draw()
        {
            float progress = (float)Time / Lifetime;
            float currentLength = MathHelper.Lerp(InitialLength, 0f, progress);
            float currentThickness = MathHelper.Lerp(InitialThickness, InitialThickness * 0.4f, progress);
            if (currentLength <= 0.1f || Opacity <= 0.05f)
                return;
            Texture2D tex = Texture;
            MiscShaderData shader = GameShaders.Misc[NoxShaderSystem.NoxPixelTrailShaderKey]
                .UseSecondaryColor(Color)
                .UseOpacity(PixelationAmount)
                .UseShaderSpecificData(new Vector4(0.04f, 0.42f, 0.35f, 0f));
            Vector2 direction = Velocity;
            if (direction != Vector2.Zero)
                direction.Normalize();
            else
                direction = Vector2.UnitX;
            float rotation = direction.ToRotation();
            Vector2 origin = new Vector2(0f, tex.Height / 2f);
            Vector2 scale = new Vector2(currentLength / tex.Width, currentThickness / tex.Height);
            DrawData drawData = new DrawData(tex, Position - Main.screenPosition, null, Color * Opacity, rotation, origin, scale, SpriteEffects.None, 0);
            shader.Apply(drawData);
            drawData.Draw(Main.spriteBatch);
        }
    }
}
