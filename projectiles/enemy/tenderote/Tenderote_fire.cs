using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_fire : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.rotation = 0f;
            if (Projectile.ai[0] == 0f)
            {
                if (Projectile.timeLeft > 120)
                {
                    Player target = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
                    if (target.active && !target.dead)
                    {
                        Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 15f, 0.015f);
                    }
                }
                else
                {
                    Projectile.velocity *= 0.95f;
                    Projectile.hostile = false;
                }
            }
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.localAI[0]++;
                if (Projectile.localAI[0] < 60)
                {
                    Projectile.velocity *= 0.9f;
                }
                else if (Projectile.localAI[0] == 60)
                {
                    Projectile.velocity = Projectile.ai[1].ToRotationVector2() * 8f;
                }
                else
                {
                    Projectile.velocity *= 1.02f;
                    if (Projectile.velocity.Length() > 25f) Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 25f;
                    Projectile.velocity = Projectile.velocity.RotatedBy(0.02f);
                }
            }
            Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 0.5f);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.OnFire, 180);
            target.AddBuff(ModContent.BuffType<buffs.debuff.TenderodWeaknessDebuff>(), 600);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Effect fireShader = null;
            try
            {
                fireShader = ModContent.Request<Effect>("jarkos/Shaders/TenderoteFire", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            }
            catch {}
            if (fireShader != null)
            {
                spriteBatch.End();
                fireShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, fireShader, Main.GameViewMatrix.TransformationMatrix);
                float opacity = 1f;
                if (Projectile.ai[0] == 0f && Projectile.timeLeft <= 120)
                {
                    opacity = Projectile.timeLeft / 120f;
                }
                Texture2D tex = TextureAssets.MagicPixel.Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float drawWidth = 60f;
                float drawHeight = 100f;
                Vector2 scale = new Vector2(drawWidth / tex.Width, drawHeight / tex.Height);
                Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.8f);
                spriteBatch.Draw(tex, drawPos, null, Color.White * opacity, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                return false;
            }
            else
            {
                if (Projectile.timeLeft % 60 == 0) Main.NewText("TenderoteFire shader failed to load!", Color.Red);
            }
            return false;
        }
    }
}
