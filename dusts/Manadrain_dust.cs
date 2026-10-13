using jarkos.buffs.buff;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace  jarkos.dusts
{
	public class Manadrain_dust : ModDust
	{
		private float timer = 0;
		public override void OnSpawn(Dust dust)
		{
			timer = 0;

			dust.velocity *= 0.4f;
			dust.noGravity = true;
			dust.noLight = true;
			dust.scale *= 2.75f;
		}
		public override bool Update(Dust dust)
		{
			timer += 0.0062f;
			float speed = MathHelper.SmoothStep(2f, 12f, timer);
			Main.NewText(speed);
			
			Player player = Main.LocalPlayer;
			Vector2 veltoplayer = player.Center - dust.position;
			veltoplayer.Normalize();

			dust.velocity = veltoplayer * speed * Main.rand.NextFloat(0.55f, 1.45f);
			dust.position += dust.velocity;
			dust.rotation += dust.velocity.X * 0.15f;
			dust.scale *= 0.99f;

			float light = 0.35f * dust.scale;

			Lighting.AddLight(dust.position, light, light, light);
			if (dust.scale < 0.25f)
			{
				dust.active = false;
			}
			float distance = Vector2.Distance(player.Center, dust.position);

			if (distance < 50f)
            {
                dust.active = false;
				player.statMana += 3;
				player.AddBuff(ModContent.BuffType<Magic_powered>(), 960);
				
				CombatText.NewText(player.Hitbox, Color.Aqua, "1", false, true);
            }
			return false;
		}
	}
}
