using System;
using jarkos.buffs.buff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee
{
    public class PurgatorySanctuaryProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Misc/Perlin";

        public const int MaxLifetime = 1200;
        public const float SanctuaryRadius = 360f;

        public override void SetDefaults()
        {
            Projectile.width = (int)(SanctuaryRadius * 2f);
            Projectile.height = (int)(SanctuaryRadius * 2f);
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = MaxLifetime;
            Projectile.netImportant = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }


            Projectile.Center = owner.Center;
            Projectile.velocity = Vector2.Zero;

            int buffType = ModContent.BuffType<PurgatorySanctuaryBuff>();


            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead)
                {
                    float distance = Vector2.Distance(player.Center, Projectile.Center);
                    if (distance <= SanctuaryRadius)
                    {
                        player.AddBuff(buffType, 4);
                    }
                }
            }


            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 borderPos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(0f, SanctuaryRadius);
                Dust d = Dust.NewDustPerfect(borderPos, DustID.GoldFlame, Main.rand.NextVector2Circular(0.8f, 0.8f), 100, default, Main.rand.NextFloat(1.1f, 1.6f));
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            Vector2 screenCenter = Projectile.Center - Main.screenPosition;

            float fadeAlpha = MathHelper.Clamp(Projectile.timeLeft / 30f, 0f, 1f);
            if (Projectile.timeLeft > MaxLifetime - 30)
            {
                fadeAlpha *= (MaxLifetime - Projectile.timeLeft) / 30f;
            }


            Texture2D texture = TextureAssets.MagicPixel.Value;
            float drawRadius = SanctuaryRadius;
            Rectangle drawRect = new Rectangle(
                (int)(screenCenter.X - drawRadius),
                (int)(screenCenter.Y - drawRadius),
                (int)(drawRadius * 2f),
                (int)(drawRadius * 2f)
            );

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


            if (GameShaders.Misc.TryGetValue("Jarkos:PurgatoryAuraShader", out MiscShaderData shader) && shader != null)
            {
                shader.UseColor(new Vector3(1.0f, 0.82f, 0.2f));
                shader.UseSecondaryColor(new Vector3(1.0f, 0.96f, 0.5f));
                shader.UseOpacity(fadeAlpha);
                shader.UseSaturation((float)Main.timeForVisualEffects * 0.05f);
                shader.Apply(null);
            }

            sb.Draw(texture, drawRect, Color.White * fadeAlpha);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
