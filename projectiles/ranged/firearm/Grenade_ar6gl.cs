using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Windows;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;
using Terraria.UI.Chat;
using ReLogic.Content;
using Newtonsoft.Json;
using Terraria.GameContent.UI.States;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using jarkos.projectiles.magic.toms;
using Terraria.Graphics;
using System.Runtime.InteropServices;
using jarkos.particles.storm;
using jarkos.particles;
using jarkos.projectiles.ranged.other;
namespace jarkos.projectiles.ranged.firearm
{
	public class Grenade_ar6gl : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 250;
			Projectile.penetrate = 1;
		}
        public override void OnKill(int timeLeft)
		{
			for (int a = 0; a < 5; a++)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
				float rand_size = Main.rand.NextFloat(0.5f, 1.25f);
				new Spark(Projectile.Center, vel, Color.Cyan, 300, rand_size, 2f, 0f).Spawn();
            }
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player player = Main.LocalPlayer;
			float rand_size = Main.rand.NextFloat(0.5f, 1.25f);
            new Circleedge(Projectile.Center, Vector2.Zero, Color.CadetBlue, 180, rand_size, 2f, 0f).Spawn();
            new StarParticle(Projectile.Center, Vector2.Zero, Color.CadetBlue, 180, 0.3f, 2f, 0.09f).Spawn();
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/short_burst_sound"), Projectile.position);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity * 0, ProjectileType<Rocketmk2_hitbox>(), Projectile.damage, 0f, Projectile.owner, 0f, 0f);
        }
		public override void AI()
		{
			Projectile.rotation += 0.1f;
			Projectile.velocity.Y += 0.15f;
        }
	}
}
