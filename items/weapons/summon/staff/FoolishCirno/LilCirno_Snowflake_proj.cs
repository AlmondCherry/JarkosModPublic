using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.FoolishCirno
{
	public class LilCirno_Snowflake_proj : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/FoolishCirno/LilCirno_Snowflake";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.rotation += 0.2f * Projectile.direction;

			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, 0.9f);
				d.noGravity = true;
				d.velocity = Projectile.velocity * 0.2f;
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
			for (int i = 0; i < 8; i++)
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.1f);
				d.noGravity = true;
			}
		}
	}
}
