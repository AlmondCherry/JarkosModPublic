using System;
using System.Collections.Generic;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace jarkos.systems
{
    public static class TrailRenderer
    {
        public delegate float WidthFunction(float t);
        public delegate Color ColorFunction(float t);
        private static VertexPositionColorTexture[] vertexCache = new VertexPositionColorTexture[256];

        public static void Draw(Effect effect, IReadOnlyList<Vector2> worldPoints, WidthFunction width, ColorFunction color, Texture2D noise)
        {
            List<Vector2> points = new(worldPoints.Count);
            for (int i = 0; i < worldPoints.Count; i++)
            {
                Vector2 pt = worldPoints[i];
                if (pt != Vector2.Zero)
                {
                    if (points.Count == 0 || Vector2.DistanceSquared(points[points.Count - 1], pt) > 4f)
                    {
                        points.Add(pt);
                    }
                }
            }

            if (points.Count < 2)
                return;

            int subdivisions = Jarkos_config.Instance?.TrailQuality ?? 8;

            List<Vector2> finalPoints;
            if (subdivisions <= 2 || points.Count < 3)
            {
                finalPoints = points;
            }
            else
            {
                int segments = points.Count - 1;
                int subCount = subdivisions / 2;
                if (subCount < 1) subCount = 1;

                finalPoints = new List<Vector2>(segments * subCount + 1);

                for (int i = 0; i < segments; i++)
                {
                    Vector2 p0 = points[Math.Max(0, i - 1)];
                    Vector2 p1 = points[i];
                    Vector2 p2 = points[Math.Min(segments, i + 1)];
                    Vector2 p3 = points[Math.Min(segments, i + 2)];

                    for (int j = 0; j < subCount; j++)
                    {
                        float t = j / (float)subCount;
                        finalPoints.Add(Vector2.CatmullRom(p0, p1, p2, p3, t));
                    }
                }
                finalPoints.Add(points[segments]);
            }

            int vertexCount = finalPoints.Count * 2;

            if (vertexCache.Length < vertexCount)
                vertexCache = new VertexPositionColorTexture[vertexCount * 2];

            for (int i = 0; i < finalPoints.Count; i++)
            {
                float t = i / (float)(finalPoints.Count - 1);
                Vector2 direction = i == finalPoints.Count - 1 ? finalPoints[i] - finalPoints[i - 1] : finalPoints[i + 1] - finalPoints[i];
                
                if (direction == Vector2.Zero)
                    direction = Vector2.UnitX;
                direction.Normalize();

                Vector2 normal = new(-direction.Y, direction.X);
                Vector2 screenPos = finalPoints[i] - Main.screenPosition;
                float halfWidth = MathF.Max(width(t), 0.5f) * 0.5f;
                Color vertexColor = color(t);

                vertexCache[i * 2] = new VertexPositionColorTexture(new Vector3(screenPos + normal * halfWidth, 0f), vertexColor, new Vector2(t, 0f));
                vertexCache[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(screenPos - normal * halfWidth, 0f), vertexColor, new Vector2(t, 1f));
            }

            GraphicsDevice device = Main.instance.GraphicsDevice;
            BlendState oldBlend = device.BlendState;
            RasterizerState oldRasterizer = device.RasterizerState;
            DepthStencilState oldDepth = device.DepthStencilState;

            device.BlendState = BlendState.Additive;
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;

            Texture2D textureToUse = noise ?? TextureAssets.Extra[ExtrasID.VortexBlack]?.Value ?? TextureAssets.MagicPixel.Value;

            if (effect != null)
            {
                if (textureToUse != null)
                {
                    device.Textures[0] = textureToUse;
                    device.SamplerStates[0] = SamplerState.LinearWrap;
                }
                effect.Parameters["uTransform"]?.SetValue(DevilShaders.WorldViewProjection);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertexCache, 0, vertexCount - 2);
                }
            }
            else
            {
                BasicEffect basicEffect = new BasicEffect(device)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = textureToUse != null,
                    Texture = textureToUse,
                    World = Matrix.Identity,
                    View = Matrix.Identity,
                    Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1)
                };

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertexCache, 0, vertexCount - 2);
                }
            }

            device.BlendState = oldBlend;
            device.RasterizerState = oldRasterizer;
            device.DepthStencilState = oldDepth;
        }

        public static void DrawOnScreen(Effect effect, IReadOnlyList<Vector2> worldPoints, WidthFunction width, ColorFunction color, Texture2D noise)
        {
            List<Vector2> points = new(worldPoints.Count);
            
            for (int i = 0; i < worldPoints.Count; i++)
            {
                Vector2 pt = worldPoints[i];
                if (pt != Vector2.Zero)
                {
                    if (points.Count == 0 || Vector2.DistanceSquared(points[points.Count - 1], pt) > 4f)
                    {
                        points.Add(pt);
                    }
                }
            }

            if (points.Count < 2)
                return;

            int subdivisions = Jarkos_config.Instance?.TrailQuality ?? 8;

            List<Vector2> finalPoints;
            if (subdivisions <= 2 || points.Count < 3)
            {
                finalPoints = points;
            }
            else
            {
                int segments = points.Count - 1;
                int subCount = subdivisions / 2;
                if (subCount < 1) subCount = 1;

                finalPoints = new List<Vector2>(segments * subCount + 1);

                for (int i = 0; i < segments; i++)
                {
                    Vector2 p0 = points[Math.Max(0, i - 1)];
                    Vector2 p1 = points[i];
                    Vector2 p2 = points[Math.Min(segments, i + 1)];
                    Vector2 p3 = points[Math.Min(segments, i + 2)];

                    for (int j = 0; j < subCount; j++)
                    {
                        float t = j / (float)subCount;
                        finalPoints.Add(Vector2.CatmullRom(p0, p1, p2, p3, t));
                    }
                }
                finalPoints.Add(points[segments]);
            }

            int vertexCount = finalPoints.Count * 2;

            if (vertexCache.Length < vertexCount)
                vertexCache = new VertexPositionColorTexture[vertexCount * 2];

            for (int i = 0; i < finalPoints.Count; i++)
            {
                float t = i / (float)(finalPoints.Count - 1);
                Vector2 direction = i == finalPoints.Count - 1 ? finalPoints[i] - finalPoints[i - 1] : finalPoints[i + 1] - finalPoints[i];
                
                if (direction == Vector2.Zero)
                    direction = Vector2.UnitX;
                direction.Normalize();

                Vector2 normal = new(-direction.Y, direction.X);
                Vector2 screenPos = finalPoints[i] - Main.screenPosition;
                float halfWidth = MathF.Max(width(t), 0.5f) * 0.5f;
                Color vertexColor = color(t);

                vertexCache[i * 2] = new VertexPositionColorTexture(new Vector3(screenPos + normal * halfWidth, 0f), vertexColor, new Vector2(t, 0f));
                vertexCache[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(screenPos - normal * halfWidth, 0f), vertexColor, new Vector2(t, 1f));
            }

            GraphicsDevice device = Main.instance.GraphicsDevice;
            BlendState oldBlend = device.BlendState;
            RasterizerState oldRasterizer = device.RasterizerState;
            DepthStencilState oldDepth = device.DepthStencilState;

            device.BlendState = BlendState.Additive;
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;

            Texture2D textureToUse = noise ?? TextureAssets.Extra[ExtrasID.VortexBlack]?.Value ?? TextureAssets.MagicPixel.Value;

            if (effect != null)
            {
                if (textureToUse != null)
                {
                    device.Textures[0] = textureToUse;
                    device.SamplerStates[0] = SamplerState.LinearWrap;
                }
                effect.Parameters["uTransform"]?.SetValue(DevilShaders.WorldViewProjection);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertexCache, 0, vertexCount - 2);
                }
            }
            else
            {
                BasicEffect basicEffect = new BasicEffect(device)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = textureToUse != null,
                    Texture = textureToUse,
                    World = Matrix.Identity,
                    View = Matrix.Identity,
                    Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1)
                };

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertexCache, 0, vertexCount - 2);
                }
            }

            device.BlendState = oldBlend;
            device.RasterizerState = oldRasterizer;
            device.DepthStencilState = oldDepth;
        }

        public static void DrawDirect(Effect effect, IReadOnlyList<Vector2> points, int count, WidthFunction width, ColorFunction color, Texture2D noise)
        {
            if (points == null || count < 2)
                return;

            int vertexCount = count * 2;
            if (vertexCache.Length < vertexCount)
                vertexCache = new VertexPositionColorTexture[vertexCount * 2];

            Vector2 screenPos = Main.screenPosition;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                Vector2 direction = i == count - 1 ? points[i] - points[i - 1] : points[i + 1] - points[i];
                if (direction == Vector2.Zero)
                    direction = Vector2.UnitX;
                direction.Normalize();

                Vector2 normal = new(-direction.Y, direction.X);
                Vector2 drawPos = points[i] - screenPos;
                float halfWidth = MathF.Max(width(t), 0.5f) * 0.5f;
                Color vertexColor = color(t);

                vertexCache[i * 2] = new VertexPositionColorTexture(new Vector3(drawPos + normal * halfWidth, 0f), vertexColor, new Vector2(t, 0f));
                vertexCache[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(drawPos - normal * halfWidth, 0f), vertexColor, new Vector2(t, 1f));
            }

            GraphicsDevice device = Main.instance.GraphicsDevice;
            BlendState oldBlend = device.BlendState;
            RasterizerState oldRasterizer = device.RasterizerState;
            DepthStencilState oldDepth = device.DepthStencilState;

            device.BlendState = BlendState.Additive;
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;

            Texture2D textureToUse = noise ?? TextureAssets.Extra[ExtrasID.VortexBlack]?.Value ?? TextureAssets.MagicPixel.Value;

            if (effect != null)
            {
                if (textureToUse != null)
                {
                    device.Textures[0] = textureToUse;
                    device.SamplerStates[0] = SamplerState.LinearWrap;
                }
                effect.Parameters["uTransform"]?.SetValue(DevilShaders.WorldViewProjection);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertexCache, 0, vertexCount - 2);
                }
            }
            else
            {
                BasicEffect basicEffect = new BasicEffect(device)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = textureToUse != null,
                    Texture = textureToUse,
                    World = Matrix.Identity,
                    View = Matrix.Identity,
                    Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1)
                };

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertexCache, 0, vertexCount - 2);
                }
            }

            device.BlendState = oldBlend;
            device.RasterizerState = oldRasterizer;
            device.DepthStencilState = oldDepth;
        }
    }
}
