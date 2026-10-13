using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.particles;

namespace jarkos.buffs.buff
{
	public class Lifeforcing : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.statLife < player.statLifeMax2)
			{
				player.statLife += 1;
			}
			if (Main.rand.NextBool(4))
			{
				Vector2 vec = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-24.5f, -8.75f));
				Vector2 vec2 = new Vector2(Main.rand.NextFloat(-45f, 45f), 0);
				new StarParticleHorizontal(player.Center + vec2, vec, Color.LimeGreen, 90, 0.01f, 1.5f, 0f).Spawn();
				new StarParticleVertical(player.Center + vec2, vec, Color.LimeGreen, 90, 0.03f, 1.5f, 0f).Spawn();
			}
			if (Main.rand.NextBool(10))
			{
				new Circular_glow(player.Center, Vector2.Zero, Color.LimeGreen, 100, 0.2f, 1.25f, 0f).Spawn();
			}
		}
	}
}
