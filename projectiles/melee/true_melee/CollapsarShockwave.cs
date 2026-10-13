using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class CollapsarShockwave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private const int Lifetime = 55;
        private const float MaxRadius = 900f;
        private const float FrontWidth = 90f;
        private const int FadeInTicks = 8;
        private const int FadeOutTicks = 14;

        private static Effect _shockwaveEffect;
        private static bool _effectLoadFailed;

        private ref float Timer => ref Projectile.ai[0];

        private float Progress => 1f - MathF.Pow(1f - MathHelper.Clamp(Timer / Lifetime, 0f, 1f), 2.6f);
        private float Radius => MaxRadius * Progress;

        private float Opacity
        {
            get
            {
                float fadeIn = MathHelper.Clamp(Timer / FadeInTicks, 0f, 1f);
                float fadeOut = MathHelper.Clamp((Lifetime - Timer) / FadeOutTicks, 0f, 1f);

                return fadeIn * fadeOut;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            if (Timer == 0)
                SoundEngine.PlaySound(SoundID.Item113 with { Volume = 0.9f, Pitch = -0.35f },
                    Projectile.Center);

            for (int i = 0; i < 4; i++)
            {
                float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * Radius;
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric,
                    ang.ToRotationVector2() * 3f, 0, Color.Cyan, 1.3f * Opacity);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.6f * Opacity, 1f * Opacity, 1.4f * Opacity);
            Timer++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float dist = Vector2.Distance(targetHitbox.Center.ToVector2(), Projectile.Center);
            return dist <= Radius && dist >= Radius - FrontWidth;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.Center.X >= Projectile.Center.X ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElectroStun>(), 30);
            for (int i = 0; i < 14; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Electric,
                    (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX)
                        .RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 9f),
                    0, Color.Cyan, 1.5f);
                d.noGravity = true;
            }
        }

        private static Effect GetEffect()
        {
            if (_effectLoadFailed)
                return null;
            if (_shockwaveEffect != null)
                return _shockwaveEffect;

            try
            {
                _shockwaveEffect = ModContent.Request<Effect>(
                    "Jarkos/Shaders/CollapsarShockwave",
                    AssetRequestMode.ImmediateLoad).Value;
            }
            catch (Exception e)
            {
                _effectLoadFailed = true;
            }
            return _shockwaveEffect;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Effect fx = GetEffect();

            if (fx == null)
            {
                DrawFallbackRing();
                return false;
            }

            fx.Parameters["uProgress"].SetValue(Progress);
            fx.Parameters["uOpacity"].SetValue(Opacity);
            fx.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uColor"].SetValue(new Vector3(0.30f, 0.95f, 1.00f));
            fx.Parameters["uSecondaryColor"].SetValue(new Vector3(0.45f, 0.10f, 1.00f));

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 topLeft = Projectile.Center - new Vector2(MaxRadius) - Main.screenPosition;
            Main.spriteBatch.Draw(pixel,
                new Rectangle((int)topLeft.X, (int)topLeft.Y,
                    (int)(MaxRadius * 2f), (int)(MaxRadius * 2f)),
                new Rectangle(0, 0, 1, 1), Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawFallbackRing()
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            const int segments = 64;
            float r = Radius;
            float segLen = MathHelper.TwoPi * r / segments + 2f;

            for (int i = 0; i < segments; i++)
            {
                float ang = MathHelper.TwoPi * i / segments;
                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * r - Main.screenPosition;

                Main.spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1),
                    new Color(40, 160, 255, 0) * (0.5f * Opacity),
                    ang + MathHelper.PiOver2, new Vector2(0.5f, 0.5f),
                    new Vector2(segLen, 26f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1),
                    new Color(255, 255, 255, 0) * Opacity,
                    ang + MathHelper.PiOver2, new Vector2(0.5f, 0.5f),
                    new Vector2(segLen, 8f), SpriteEffects.None, 0f);
            }
        }
    }
}
