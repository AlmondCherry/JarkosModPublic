using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.bullet
{
	public class Bloody_bolt_impulse : ModProjectile
	{
		private float amplitude = 15f;
    	private float frequency = 0.1f;
    	private Vector2 initialVelocity;
    	private float travelDistance = 0f;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
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
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Projectile.damage = player.HeldItem.damage * 25 / 100 + 10;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Projectile.ai[0] == 0)
			{
				initialVelocity = Projectile.velocity;
				Projectile.ai[0] = 1;
			}
			travelDistance += initialVelocity.Length();
			Projectile.position += initialVelocity;
			Vector2 Position_new;
			Vector2 Position_new_negative;
			Vector2 sineOffset;
			if (Math.Abs(initialVelocity.X) > Math.Abs(initialVelocity.Y))
			{
				sineOffset = new Vector2(0, amplitude * (float)Math.Sin(travelDistance * frequency));
			}
			else
			{
				sineOffset = new Vector2(amplitude * (float)Math.Sin(travelDistance * frequency), 0);
			}
			Vector2 sineOffsetNegative;
			if (Math.Abs(initialVelocity.X) < Math.Abs(initialVelocity.Y))
			{
				sineOffsetNegative = new Vector2(0, amplitude * (float)Math.Sin(travelDistance * frequency));
			}
			else
			{
				sineOffsetNegative = new Vector2(amplitude * (float)Math.Sin(travelDistance * frequency), 0);
			}
			Projectile.position += sineOffset;
			Position_new = Projectile.position;
			Position_new_negative = Projectile.position - sineOffset;
			Dust dust = Dust.NewDustDirect(Position_new, Projectile.height, Projectile.width, DustID.Blood, Scale: 2f, SpeedX: Projectile.velocity.X, SpeedY: Projectile.velocity.Y);
			dust.velocity /= 20;
			dust.noGravity = true;
			Dust dust2 = Dust.NewDustDirect(Position_new_negative, Projectile.height, Projectile.width, DustID.Blood, Scale: 2f, SpeedX: Projectile.velocity.X, SpeedY: Projectile.velocity.Y);
			dust2.velocity /= 20;
			dust2.noGravity = true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/toxic_impact") with {Pitch = Main.rand.NextFloat(-0.05f, 0.05f)}, Projectile.position);
			for (int i = 0; i < 5; i++)
			{
				new Raindrop_particle(Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), Color.Red, 120, 0.95f, 4f, 0f).Spawn();
				Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Blood, speed * 2, Scale: 1.5f);
				d.noGravity = true;
			}
		}
	}
}
