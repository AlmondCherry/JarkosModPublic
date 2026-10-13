using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
	public class Sunfate_fireball : ModProjectile
	{
		public override void SetStaticDefaults()
		{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
			Projectile.width = 40;
			Projectile.height = 20;
            Projectile.scale = 1f;
			Projectile.damage = 5;
            Projectile.penetrate = 1;
		}
        public override void OnKill(int timeLeft)
		{
            for (int a = 0; a < 45; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f,2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, 55, speed * 2, Scale: 1.50f);
			d.noGravity = true;
            }
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
		        Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<Sunfate_explosion>(), (int)(Projectile.damage * 1), 0f, Projectile.owner, 0f, 0f);
        }
		 public override void AI(){
			Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.spriteDirection == -1) {
				Projectile.rotation += MathHelper.Pi;
			}
            if (Main.rand.NextBool(1, 9)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, 55, Scale: 0.8f);
            dust.velocity *= 10; }
            if (Main.rand.NextBool(1, 9)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, 55, Scale: 0.4f);
            dust.velocity *= 10; }
            if (Main.rand.NextBool(1, 9)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, 55, Scale: 0.6f);
            dust.velocity *= 10; }
			if (Main.rand.NextBool(1, 9)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, 55, Scale: 0.3f);
            dust.velocity *= 10; }
			if (Main.rand.NextBool(1, 9)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, 55, Scale: 0.7f);
            dust.velocity *= 10; }
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
