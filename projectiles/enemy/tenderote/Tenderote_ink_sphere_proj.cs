using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_ink_sphere_proj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";
        private Effect inkShader;
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server && inkShader == null)
            {
                inkShader = ModContent.Request<Effect>("jarkos/Shaders/TenderoteInkSphere", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            }
            Projectile.ai[0]++;
            if (Projectile.ai[0] % 10 == 0 && Projectile.ai[0] > 30)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                    if (target.active && !target.dead)
                    {
                        int pointIndex = (int)(Projectile.ai[0] / 10) % 5;
                        float angleOffset = MathHelper.TwoPi * (pointIndex / 5f);
                        Vector2 spawnOffset = angleOffset.ToRotationVector2() * 120f;
                        Vector2 spawnPos = Projectile.Center + spawnOffset;
                        Vector2 velocity = Vector2.Normalize(target.Center - spawnPos) * 15f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocity, ModContent.ProjectileType<Tenderote_black_dagger>(), Projectile.damage, 0f, Main.myPlayer, 1f);
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (inkShader != null)
            {
                spriteBatch.End();
                inkShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, inkShader, Main.GameViewMatrix.TransformationMatrix);
                Texture2D tex = TextureAssets.MagicPixel.Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float alpha = 1f;
                if (Projectile.timeLeft < 30) alpha = Projectile.timeLeft / 30f;
                else if (Projectile.timeLeft > 270) alpha = (300 - Projectile.timeLeft) / 30f;
                float scale = 300f;
                if (Projectile.timeLeft > 270)
                {
                    scale = MathHelper.Lerp(300f, 0f, (Projectile.timeLeft - 270f) / 30f);
                }
                else if (Projectile.timeLeft < 30)
                {
                    scale = MathHelper.Lerp(0f, 300f, Projectile.timeLeft / 30f);
                }
                spriteBatch.Draw(tex, drawPos, null, Color.White * alpha, Projectile.rotation, new Vector2(tex.Width * 0.5f, tex.Height * 0.5f), new Vector2(scale / tex.Width, scale / tex.Height), SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
