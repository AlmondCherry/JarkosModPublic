using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace jarkos.systems
{
    public class ThermalScytheDashTrailLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Leggings);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.active && !drawInfo.drawPlayer.dead && drawInfo.shadow == 0f;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPl = player.GetModPlayer<Jarkos_player>();

            if (modPl.thermalDashTrailPoints == null || modPl.thermalDashTrailPoints.Count < 2 || modPl.thermalDashTrailAlpha <= 0f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            if (pixel == null) return;

            var points = modPl.thermalDashTrailPoints;
            int n = points.Count;
            float alpha = modPl.thermalDashTrailAlpha;
            Vector2 origin = new Vector2(0.5f, 0.5f);

            for (int layer = 0; layer < 2; layer++)
            {
                bool isRedBackLayer = layer == 0;
                Color baseColor = isRedBackLayer ? new Color(255, 30, 20) : Color.White;
                Color drawColor = baseColor * alpha;

                float maxW = isRedBackLayer ? 16f : 8f;
                float minW = isRedBackLayer ? 2f : 1f;

                for (int i = 0; i < n - 1; i++)
                {
                    Vector2 a = points[i];
                    Vector2 b = points[i + 1];
                    Vector2 delta = b - a;
                    float length = delta.Length();
                    if (length < 0.5f) continue;

                    Vector2 mid = (a + b) * 0.5f;
                    float angle = delta.ToRotation();

                    float t = (i + 0.5f) / (n - 1);
                    float segmentWidth = MathHelper.Lerp(minW, maxW, MathF.Pow(t, 1.1f));

                    Vector2 drawPos = mid - Main.screenPosition;
                    Vector2 scale = new Vector2(length + 1.5f, segmentWidth);

                    drawInfo.DrawDataCache.Add(new DrawData(pixel, drawPos, null, drawColor, angle, origin, scale, SpriteEffects.None, 0));
                }
            }
        }
    }
}
