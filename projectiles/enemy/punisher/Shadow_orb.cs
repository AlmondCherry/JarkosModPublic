using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using jarkos.particles;
namespace jarkos.projectiles.enemy.punisher
{
	public class Shadow_orb : ModProjectile
	{
		private float time_to_rotate = 0f;
		private bool stoppin = false;
		public override void SetStaticDefaults()
		{
		}
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);
			AIType = ProjectileID.Bullet;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 180;
			Projectile.penetrate = 1;
			Projectile.light = 1f;
		}
		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, 0f, 1f, 1f);
			Player player = Main.player[Projectile.owner];
			Vector2 vel = new Vector2(-12, 0);
			Vector2 pos_x = new Vector2(75, 0);
			Projectile.velocity *= 0.978f;
			if (Projectile.timeLeft == 10)
			{
				new Line_glow_smaller(Projectile.Center + pos_x, vel, Color.HotPink, 300, 0.45f, 2f, 0f).Spawn();
			}
		}
		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
				Dust d = Dust.NewDustPerfect(Projectile.position, 54, speed * 2.5f, Scale: 1f);
				d.noGravity = false;
			}
			Vector2 vel = new Vector2(0, 0);
			for (int i = 0; i < 5; i++)
			{
				if (i == 0)
				{
					vel = new Vector2(0, 1);
				}
				if (i == 1)
				{
					vel = new Vector2(-1, 0);
				}
				if (i == 2)
				{
					vel = new Vector2(0, 1);
				}
				if (i == 3)
				{
					vel = new Vector2(1, 0);
				}
				if (i == 4)
				{
					vel = new Vector2(0, -1);
				}
				if (i == 5)
				{
					vel = new Vector2(0, 1);
				}
				Projectile.NewProjectile(null, Projectile.Center, vel * 12, ModContent.ProjectileType<Shadow_bolt>(), 10, 0f);
			}
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
		}
		public override bool PreDraw(ref Color lightcolor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D texture_line_hint = ModContent.Request<Texture2D>("jarkos/projectiles/enemy/tenderote/hint_line_double", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var origin = texture_line_hint.Size() / 2f;
			if (!stoppin)
			{
				time_to_rotate += 4;
			}
			if (time_to_rotate > 90)
			{
				time_to_rotate = 90;
				stoppin = true;
			}
			Main.NewText(time_to_rotate);
			Main.EntitySpriteDraw(texture_line_hint, Projectile.Center - Main.screenPosition, null, Color.HotPink, -MathHelper.PiOver2 + MathHelper.ToRadians(time_to_rotate), origin, Projectile.scale, SpriteEffects.None, 0f);
			Main.EntitySpriteDraw(texture_line_hint, Projectile.Center - Main.screenPosition, null, Color.HotPink, (float)Math.PI + MathHelper.ToRadians(time_to_rotate), origin, Projectile.scale, SpriteEffects.None, 0f);
            return true;
        }
    }
}
