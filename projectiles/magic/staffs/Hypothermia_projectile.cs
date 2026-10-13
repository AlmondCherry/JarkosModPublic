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
	public class Hypothermia_projectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults() 
		{
			Projectile.width = 8;
      		Projectile.height = 8;
      		Projectile.aiStyle = 0;
      		Projectile.timeLeft = 600;
      		Projectile.penetrate = 1;
      		Projectile.scale = 1f;
			Projectile.DamageType = DamageClass.Magic;
      		Projectile.ownerHitCheck = true;
      		Projectile.tileCollide = true;
      		Projectile.friendly = true;
		}
		public override void AI()
		{
			if (Projectile.localAI[0] == 0f)
			{
				Projectile.localAI[0] = 1f;
				if (Main.netMode != NetmodeID.Server)
				{
					new StarParticle(Projectile.Center, Vector2.Zero, Color.Cyan, 120, 0.2f, 2f, 0.2f).Spawn();
				}
			}
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
			for (int i = 0; i < 5; i++)
			{
				Projectile.NewProjectile(null, Projectile.Center, Main.rand.NextVector2CircularEdge(4f, 4f), ModContent.ProjectileType<Hypothermia_projectile_mini>(), (int)(Projectile.damage * 0.75f), 0f);
				Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.BlueTorch, speed * 2, Scale: 1.5f);
				d.noGravity = true;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float size = Projectile.scale * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, size, SpriteEffects.None, 0);
			}
			return true;
		}
	}
}
