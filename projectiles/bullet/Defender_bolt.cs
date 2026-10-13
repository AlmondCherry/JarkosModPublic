using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.bullet
{
	public class Defender_bolt : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = true;
			Projectile.scale = 1f;
			Projectile.timeLeft = 600;
			Projectile.light = 1f;

			DrawOffsetX = -(int)((12 / 2) - (Projectile.Size.X / 2));
			DrawOriginOffsetY = -(int)((28 / 2) - (Projectile.Size.Y / 2));
		}
		public override void AI(){
			Player player = Main.player[Projectile.owner];
			Projectile.damage = player.HeldItem.damage * 25 / 100 + 10;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(1))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.YellowTorch, Scale: 0.7f);
				dust.velocity /= 20;
				dust.noGravity = true;
			}
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/toxic_impact") with {Pitch = Main.rand.NextFloat(-0.05f, 0.05f)}, Projectile.position);
			for (int i = 0; i < 5; i++)
			{
				new Raindrop_particle(Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), Color.Orange, 120, 0.95f, 4f, 0f).Spawn();
				Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.GreenTorch, speed * 2, Scale: 1.5f);
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
				
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}
			return true;
		}
	}
}
