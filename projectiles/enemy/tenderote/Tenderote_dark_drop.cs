using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using jarkos.systems;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_dark_drop : ModProjectile
    {
        private const int TrailLength = 20;
        private List<Vector2> trailPositions = new List<Vector2>();
        public override string Texture => "Terraria/Images/Projectile_1";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > TrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float depth = Projectile.ai[1];
            if (depth <= 0f) depth = 1f;
            Projectile.velocity.Y *= 1.05f;
            float maxSpeed = 15f * depth;
            if (Projectile.velocity.Y > maxSpeed)
                Projectile.velocity.Y = maxSpeed;
            NPC boss = Main.npc[(int)Projectile.ai[0]];
            if (boss.active && boss.type == ModContent.NPCType<npc.tenderote.Tenderote_head>() && boss.ai[0] == 3f)
            {
                if (Vector2.Distance(Projectile.Center, boss.Center) < 100f)
                {
                    Projectile.Kill();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            float depth = Projectile.ai[1];
            if (depth <= 0f) depth = 1f;
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 5f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 255, 255).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(Color.White.ToVector4());
                List<Vector2> points = new List<Vector2>();
                points.Add(Projectile.Center);
                points.AddRange(trailPositions);
                TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(12f * depth, 0f, t), t => Color.White * (1f - t) * (depth / 2f), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, new Rectangle(0,0,1,1), Color.White * (depth / 2f), Projectile.rotation, new Vector2(0.5f), new Vector2(10f * depth, 16f * depth), SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, new Rectangle(0,0,1,1), Color.Black * (depth / 2f), Projectile.rotation, new Vector2(0.5f), new Vector2(6f * depth, 12f * depth), SpriteEffects.None, 0f);
            return false;
        }
    }
}
