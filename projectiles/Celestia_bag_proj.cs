using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.particles;
using jarkos.items.weapons.melee.other_melee;
using jarkos.items.materials;
using jarkos.items.accs.ranged;
using jarkos.items.weapons.melee.true_melee;
using jarkos.items.weapons.melee.unique_melee;

namespace jarkos.projectiles
{
	public class Celestia_bag_proj : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = 52;
			Projectile.height = 42;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 100;
			Projectile.penetrate = -1;
			Projectile.damage = 0;
		}
        public override void AI()
        {
			Projectile.velocity.X *= 0;
			Projectile.velocity.Y *= 0.95f;

			if(Main.rand.NextBool(4))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.PinkTorch, Scale: 1f);
            	dust.velocity = Projectile.velocity;
				dust.noGravity = true;
				Dust dust1 = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.BlueTorch, Scale: 1f);
            	dust1.velocity = Projectile.velocity;
				dust1.noGravity = true;
			}

			if(Main.rand.NextBool(12))
			{
				Vector2 velY = new Vector2(0, -4);
				float size = Main.rand.NextFloat(0.15f, 0.30f);

				new Circular_glow(Projectile.Center, (Projectile.velocity * 0.5f) + velY, Color.Pink, 80, size, 2f, 0f).Spawn();
				SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/celestia_bag_punches") with {Pitch = Main.rand.NextFloat(0, 2)}, Projectile.position);
			}
        }
		public override void OnKill(int timeLeft)
		{
			Player player = Main.LocalPlayer;

            for (int a = 0; a < 20; a++) 
			{
				int[] colors = new int[] {59, 242};
				Vector2 speed = Main.rand.NextVector2Circular(2, 2);
				Dust d = Dust.NewDustPerfect(Projectile.position, colors[Main.rand.Next(0, 2)], speed * 4, Scale: 2.5f);
				d.noGravity = true;
            }

			player.QuickSpawnItem(null, ModContent.ItemType<CelestiaStar>(), Main.rand.Next(6, 8));
			if(Main.rand.NextBool(4))
			{
				player.QuickSpawnItem(null, ModContent.ItemType<TheDawnMoon>(), 1);
			}
			if(Main.rand.NextBool(4))
			{
				player.QuickSpawnItem(null, ModContent.ItemType<GloveOfCelestia>(), 1);
			}
			if(Main.rand.NextBool(4))
			{
				player.QuickSpawnItem(null, ModContent.ItemType<Ethereal_rift>(), 1);
			}
			if(Main.rand.NextBool(4))
			{
				player.QuickSpawnItem(null, ModContent.ItemType<ImpactWave_sword>(), 1);
			}
			int upGore = Mod.Find<ModGore>("celestia_bag_proj_gore_up").Type;
			int downGore = Mod.Find<ModGore>("celestia_bag_proj_gore_down").Type;

			for (int i = 0; i < 2; i++) 
			{
				Gore.NewGore(null, Projectile.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), upGore);
				Gore.NewGore(null, Projectile.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), downGore);
				SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/celestia_bag_breaking") with {Pitch = Main.rand.NextFloat(-2, 0)}, Projectile.position);
			}
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/celestia_bag_breaking") with {Pitch = Main.rand.NextFloat(-2, 0)}, Projectile.position);
		}
    }
}
