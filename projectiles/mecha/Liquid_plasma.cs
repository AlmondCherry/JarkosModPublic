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
namespace jarkos.projectiles.mecha
{
	public class Liquid_plasma : ModProjectile
	{
		public override void SetStaticDefaults()
		{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
			Projectile.DamageType = DamageClass.Default;
			Projectile.width = 20;
			Projectile.height = 20;
            Projectile.penetrate = 1;
		}
        public override void OnKill(int timeLeft)
		{
            for (int a = 0; a < 5; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f,2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Clentaminator_Cyan, speed * 2, Scale: 1.50f);
			d.noGravity = false;
            }
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
        }
		 public override void AI(){
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.velocity.Y += 0.09f;
        }
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.spriteDirection == -1)
				spriteEffects = SpriteEffects.FlipHorizontally;
			Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
			Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
			Vector2 origin = sourceRectangle.Size() / 2f;
			float offsetX = 20f;
			origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);
			Color drawColor = Projectile.GetAlpha(lightColor);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
			return false;
		}
	}
}
