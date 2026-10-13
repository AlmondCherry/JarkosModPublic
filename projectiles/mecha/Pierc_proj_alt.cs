using System.Collections.Generic;
using jarkos;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.mecha
{
	public class Pierc_proj_alt : ModProjectile
	{
        private const int TrailLength = 20;
        private List<Vector2> trailPositions = new List<Vector2>();
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
			Projectile.DamageType = DamageClass.Default;
            Projectile.scale = 1.50f;
			Projectile.damage = 130;
            Projectile.penetrate = 1;
			Projectile.light = 1f;
		}
		  public override void AI(){
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > TrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
            if (Main.rand.NextBool(1)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.MagicMirror, Scale: 0.7f);
            dust.velocity /= 20; }
        }
        public override void OnKill(int timeLeft)
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<Screenshake_system>();
			modPlayer.weak_shake = 1f;
			for (int n = 0; n < 45; ++n)
			{
			    int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.WhiteTorch);
			    Main.dust[dust].position = Projectile.Center - new Vector2(2, 2);
			    Main.dust[dust].scale = 0.80f;
                Main.dust[dust].velocity *= Main.rand.Next(2,4);
            }
            if (Projectile.owner == Main.myPlayer)
            {
				new Circular_glow(Projectile.Center, Vector2.Zero, Color.LightCyan, 140, 0.15f, 150f, 0f).Spawn();
		        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Piercer_explosion_ring>(), (int)(Projectile.damage * 3), Projectile.knockBack * 2f, Projectile.owner);
            }
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.4f);
                effect.Parameters["uColor"]?.SetValue(new Color(0, 200, 255).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(150, 240, 255).ToVector4());
                List<Vector2> points = new List<Vector2>();
                points.Add(Projectile.Center);
                points.AddRange(trailPositions);
                TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(28f * Projectile.scale, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
}
}
