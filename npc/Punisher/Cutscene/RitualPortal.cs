using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.npc.Punisher.Cutscene
{
    public class RitualPortal : ModProjectile
    {
        public const int OPEN_TIME = 140;
        public const int CLOSE_TIME = 70;
        public const int LIFETIME = 380;
        private static Asset<Effect> portalEffect;
        private static Asset<Texture2D> noiseTex;
        private static bool effectLoadAttempted;
        private bool closeBurstDone;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LIFETIME;
            Projectile.aiStyle = -1;
        }
        private Vector2 BottomPos => Projectile.Center + new Vector2(0f, 132f);
        private float OpenProgress()
        {
            float open = MathHelper.Clamp(Projectile.ai[0] / OPEN_TIME, 0f, 1f);
            return 1f - (1f - open) * (1f - open);
        }
        private float CloseProgress()
        {
            if (Projectile.timeLeft >= CLOSE_TIME)
                return 1f;
            float t = Projectile.timeLeft / (float)CLOSE_TIME;
            return t * t;
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            float open = OpenProgress();
            float close = CloseProgress();
            float vis = open * close;
            Lighting.AddLight(Projectile.Center, 1.2f * vis, 0.5f * vis, 1.6f * vis);
            if (open < 1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust g = Dust.NewDustDirect(BottomPos + new Vector2(Main.rand.NextFloat(-45f, 45f), -6f), 4, 4, Main.rand.NextBool() ? DustID.Torch : DustID.Shadowflame);
                    g.noGravity = true;
                    g.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 6f) * (0.3f + open));
                    g.scale = Main.rand.NextFloat(1.2f, 2f);
                }
                if (Main.rand.NextBool(3))
                {
                    Dust rock = Dust.NewDustDirect(BottomPos + new Vector2(Main.rand.NextFloat(-50f, 50f), -4f), 6, 4, DustID.Dirt);
                    rock.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2f, 5f));
                    rock.scale = Main.rand.NextFloat(1f, 1.6f);
                }
            }
            if (vis > 0.2f && close > 0.9f)
            {
                if (Main.rand.NextBool(2))
                {
                    Dust f = Dust.NewDustDirect(Projectile.Center - new Vector2(30f, 70f) * open, (int)(60 * open), (int)(140 * open), DustID.Torch);
                    f.noGravity = true;
                    f.velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(1f, 3f));
                    f.scale = Main.rand.NextFloat(1.2f, 1.9f);
                }
                if (Main.rand.NextBool(3))
                {
                    Dust s = Dust.NewDustDirect(Projectile.Center - new Vector2(20f, 40f) * open, (int)(40 * open), (int)(80 * open), DustID.Shadowflame);
                    s.noGravity = true;
                    s.velocity *= 0.5f;
                }
                if (Main.rand.NextBool(4))
                {
                    Dust w = Dust.NewDustDirect(BottomPos + new Vector2(-25f * open, -30f), (int)(50 * open), 10, DustID.Water);
                    w.velocity = new Vector2(0f, Main.rand.NextFloat(1f, 2.5f));
                }
            }
            if (Projectile.timeLeft < CLOSE_TIME)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 from = Projectile.Center + Main.rand.NextVector2CircularEdge(140f, 160f);
                    Dust d = Dust.NewDustPerfect(from, DustID.Shadowflame, (Projectile.Center - from) * 0.08f, 0, default, 1.5f);
                    d.noGravity = true;
                }
                if (Projectile.timeLeft == CLOSE_TIME - 1)
                    SoundEngine.PlaySound(SoundID.Item104, Projectile.Center);
                if (Projectile.timeLeft <= 4 && !closeBurstDone)
                {
                    closeBurstDone = true;
                    SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
                    for (int i = 0; i < 60; i++)
                    {
                        Dust sp = Dust.NewDustPerfect(Projectile.Center + new Vector2(0f, Main.rand.NextFloat(-110f, 110f)), Main.rand.NextBool() ? DustID.Torch : DustID.GemSapphire, new Vector2(Main.rand.NextFloat(-5f, 5f), -Main.rand.NextFloat(1f, 6f)), 0, default, Main.rand.NextFloat(1.2f, 2.2f));
                        sp.noGravity = false;
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float open = OpenProgress();
            float close = CloseProgress();
            if (open <= 0.01f || close <= 0.01f)
                return false;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = BottomPos - Main.screenPosition + new Vector2(0f, 12f);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height * 0.89f);
            float sx = MathHelper.Clamp(open * 1.6f - 0.6f, 0.05f, 1f) * 0.85f;
            float sy = open;
            sx *= MathHelper.Clamp(close * 1.4f - 0.4f, 0.03f, 1f);
            if (close < 0.3f)
                sy *= close / 0.3f;
            Vector2 scale = new Vector2(sx, sy) * 1.3f;
            float heat = close < 0.6f ? (0.6f - close) / 0.6f : 0f;
            Effect fx = TryGetEffect();
            SpriteBatch sb = Main.spriteBatch;
            if (fx != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                fx.Parameters["uProgress"]?.SetValue(open);
                fx.Parameters["uNoiseTexture"]?.SetValue(GetNoise());
                fx.CurrentTechnique.Passes[0].Apply();
                sb.Draw(tex, drawPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                if (heat > 0f)
                    sb.Draw(tex, drawPos, null, Color.White * (heat * 0.9f), 0f, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f);
                sb.Draw(tex, drawPos, null, Color.White * (open * close), 0f, origin, scale * pulse, SpriteEffects.None, 0f);
                sb.Draw(tex, drawPos, null, new Color(255, 150, 60) * (0.55f * open * close), 0f, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);
                if (heat > 0f)
                    sb.Draw(tex, drawPos, null, Color.White * heat, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            return false;
        }
        private static Effect TryGetEffect()
        {
            if (Main.dedServ)
                return null;
            if (!effectLoadAttempted)
            {
                effectLoadAttempted = true;
                if (ModContent.HasAsset("jarkos/npc/Punisher/Cutscene/Portal"))
                    portalEffect = ModContent.Request<Effect>("jarkos/npc/Punisher/Cutscene/Portal", AssetRequestMode.ImmediateLoad);
            }
            return portalEffect?.Value;
        }
        private static Texture2D GetNoise()
        {
            noiseTex ??= ModContent.Request<Texture2D>("jarkos/npc/Punisher/Cutscene/PortalNoise", AssetRequestMode.ImmediateLoad);
            return noiseTex.Value;
        }
        public override void Unload()
        {
            portalEffect = null;
            noiseTex = null;
            effectLoadAttempted = false;
        }
    }
}
