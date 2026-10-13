using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.summon.whip
{
	public class Ancient_whip_root : ModProjectile
	{
        private ref float Timer => ref Projectile.localAI[0];
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 40;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 50;
        }
        public override void AI()
        {
            Timer++;
            if (Timer < 10)
            {
                Projectile.velocity.Y *= 0.92f;
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
            if (Projectile.timeLeft < 15)
            {
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 15f));
            }
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.JungleGrass);
                d.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                d.scale = Main.rand.NextFloat(0.2f, 0.6f);
                d.noGravity = true;
                d.alpha = 80;
            }
            if (Timer <= 8)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust dirt = Dust.NewDustDirect(Projectile.Bottom + new Vector2(Main.rand.NextFloat(-15, 15), 0), 1, 1, DustID.Mud);
                    dirt.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                    dirt.scale = 0.3f;
                    dirt.noGravity = false;
                }
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.15f, 0.4f, 0.1f));
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height);
            float opacity = 1f - Projectile.alpha / 255f;
            Color drawColor = Color.Lerp(lightColor, new Color(80, 200, 60), 0.3f) * opacity;

            Main.EntitySpriteDraw(texture, Projectile.Bottom - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Color glowColor = new Color(100, 255, 60, 0) * 0.3f * opacity;

            Main.EntitySpriteDraw(texture, Projectile.Bottom - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GreenTorch);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.scale = Main.rand.NextFloat(0.1f, 0.2f);
                d.noGravity = true;
            }
        }
    }
}
