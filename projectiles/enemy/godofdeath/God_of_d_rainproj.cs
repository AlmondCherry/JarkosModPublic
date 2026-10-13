using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.godofdeath
{
	public class God_of_d_rainproj : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_98";
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 16;
			Projectile.height = 36;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			CooldownSlot = 1;
		}
		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathF.PI / 2f;
			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 150, default, 0.9f);
				d.noGravity = true;
				d.velocity = Projectile.velocity * 0.2f;
			}
		}
		public override void OnKill(int timeLeft)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Vector2 spawnPos = Projectile.Center;
				int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<God_of_d_voidpoolproj>(), 20, 0f, Main.myPlayer);
				if (proj >= 0 && proj < Main.maxProjectiles)
				{
					Main.projectile[proj].netUpdate = true;
				}
			}
			if (Main.netMode != NetmodeID.Server)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 0.3f, Pitch = 0.3f }, Projectile.Center);
				for (int i = 0; i < 8; i++)
				{
					Vector2 speed = Main.rand.NextVector2Circular(4f, 4f) - Projectile.velocity * 0.15f;
					new jarkos.particles.VoidShardParticle(Projectile.Center, speed, Color.Black, 20, 0.8f, 0.94f).Spawn();
					Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, speed.X, speed.Y, 120, default, 1.1f);
					d.noGravity = true;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 oldDrawPos = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;
				Color color = new Color(130, 20, 220) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.4f;
				Main.spriteBatch.Draw(texture, oldDrawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}
			Main.spriteBatch.Draw(texture, drawPos, null, Color.Black, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
