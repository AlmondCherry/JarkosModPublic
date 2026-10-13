using Terraria;
using Terraria.ModLoader;

namespace  jarkos.dusts
{
	public class Square_dust : ModDust
	{
		public override void OnSpawn(Dust dust)
		{
			dust.velocity *= 0.4f;
			dust.noGravity = true;
			dust.noLight = true;
			dust.scale *= 2.75f;
		}
		public override bool Update(Dust dust)
		{
			dust.velocity *= 0.98f;
			dust.position += dust.velocity;
			dust.scale *= 0.99f;
			float light = 0.75f * dust.scale;

			Lighting.AddLight(dust.position, light, light, light);
			if (dust.scale < 0.25f)
			{
				dust.active = false;
			}
			return false;
		}
	}
}
