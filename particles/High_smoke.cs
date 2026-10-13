using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace jarkos.particles
{
	public class High_smoke : Particles
	{
		public float Spin;
		public float ScaleGrowthRate = 1.002f;
		private float _initialOpacity;

		public override BlendState DrawBlendState => BlendState.Additive;
		public override string TexturePath => "jarkos/particles/High_smoke";

		public High_smoke(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
		{
			Position = position;
			Velocity = velocity;
			Color = color;
			Scale = scale;
			Frame = Main.rand.Next(FrameCount);
			Lifetime = lifetime;
			Opacity = opacity;
			_initialOpacity = opacity;
			Spin = rotationSpeed;
		}

		public override void Update()
		{
			Velocity *= 0.92f;

			ScaleGrowthRate += 0.000035f;
			Scale *= ScaleGrowthRate;
			float progress = MathHelper.Clamp(LifetimeRatio, 0f, 1f);
			Opacity = MathHelper.Lerp(_initialOpacity, 0f, progress * progress);
			
			if (Opacity <= 0.01f || Time >= Lifetime)
			{
				Kill();
			}

			Rotation += Spin;
		}
	}
}
