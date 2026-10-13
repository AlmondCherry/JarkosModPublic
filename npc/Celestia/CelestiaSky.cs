using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;

namespace jarkos.npc.Celestia
{
    public class CelestiaSky : CustomSky
    {
        private bool isActive;
        private float intensity;

        public static float CurrentIntensity { get; private set; }
        public static float Phase1Intensity { get; set; }

        public override void Update(GameTime gameTime)
        {
            intensity = MathHelper.Clamp(intensity + (isActive ? 0.035f : -0.012f), 0f, 1f);
            CurrentIntensity = intensity;
        }

        public override Color OnTileColor(Color inColor)
        {
            float total = MathHelper.Clamp(CurrentIntensity + Phase1Intensity, 0f, 1f);
            return Color.Lerp(inColor, new Color(170, 205, 255), total * 0.12f);
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) { }

        public override float GetCloudAlpha()
        {
            float total = MathHelper.Clamp(CurrentIntensity + Phase1Intensity, 0f, 1f);
            return 1f - total;
        }

        public override void Activate(Vector2 position, params object[] args) => isActive = true;
        public override void Deactivate(params object[] args) => isActive = false;
        public override void Reset() => isActive = false;
        public override bool IsActive() => isActive || intensity > 0.01f || Phase1Intensity > 0.01f;
    }
}
