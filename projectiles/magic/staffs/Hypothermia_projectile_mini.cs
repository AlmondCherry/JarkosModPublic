using jarkos.buffs.debuff;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.magic.staffs
{
	public class Hypothermia_projectile_mini : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults() {
			Projectile.width = 8;
      		Projectile.height = 8;
      		Projectile.aiStyle = 0;
      		Projectile.timeLeft = 600;
      		Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Magic;
      		Projectile.scale = 0.5f;
      		Projectile.ownerHitCheck = true;
      		Projectile.tileCollide = true;
      		Projectile.friendly = true;
		}
		public override void AI()
		{
			Projectile.velocity.Y += 0.19f;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(1))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.BlueTorch, Scale: 0.7f);
				dust.velocity /= 20;
				dust.noGravity = true;
			}
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(ModContent.BuffType<Hypothermia_debuff>(), 600);
			for (int i = 0; i < 3; i++)
			{
				Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.BlueTorch, speed * 2, Scale: 1.5f);
				d.noGravity = true;
			}
		}
        public override void OnKill(int timeLeft)
        {
			new Circular_glow(Projectile.Center, Vector2.Zero, Color.Cyan, 120, 0.1f, 2f, 0f).Spawn();
            for (int i = 0; i < 5; i++)
			{
				Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.BlueTorch, speed * 2, Scale: 1.5f);
				d.noGravity = true;
			}
        }
		public override bool PreDraw(ref Color lightColor)
		{
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float size = Projectile.scale * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, size, SpriteEffects.None, 0);
			}
			return true;
		}
	}
}
