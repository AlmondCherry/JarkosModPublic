using System.Collections.Generic;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Jarkos.projectiles.mecha
{
	public class Pierc_proj : ModProjectile
	{
        private const int TrailLength = 25;
        private List<Vector2> trailPositions = new List<Vector2>();
        public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 5;
            Projectile.tileCollide = false;
            Projectile.ArmorPenetration = 10;
            Projectile.penetrate = 1;
			Projectile.light = 1f;
		}
		  public override void AI(){
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > TrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(2))
            {
                new Spark(Projectile.Center, Projectile.velocity / 4, Color.Yellow, 200, Main.rand.NextFloat(0.15f, 0.45f), 1f, 0f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			new StarParticle(Projectile.Center, Vector2.Zero, Color.Yellow, 30, 0.15f, 175f, 25f).Spawn();
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
                effect.Parameters["uOpacity"]?.SetValue(1.2f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 200, 0).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 100).ToVector4());
                List<Vector2> points = new List<Vector2>();
                points.Add(Projectile.Center);
                points.AddRange(trailPositions);
                TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(18f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
    }
}
