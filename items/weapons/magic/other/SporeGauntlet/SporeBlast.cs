using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.weapons.magic.other.SporeGauntlet
{
    public class SporeBlast : ModProjectile
    {
        private const int Life = 38;
        private float Radius => Projectile.ai[0];
        private int Style => (int)Projectile.ai[1];
        private float Progress => 1f - Projectile.timeLeft / (float)Life;
        public override string Texture => "jarkos/items/weapons/magic/other/SporeGauntlet/SporeSoft";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Life;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float r = Radius * Math.Min(1f, Progress * 2.2f + 0.25f);
            return Vector2.Distance(targetHitbox.Center.ToVector2(), Projectile.Center) <= r;
        }
        public override void OnSpawn(IEntitySource source)
        {
            int style = Style;
            SoundEngine.PlaySound(style == 2 ? SoundID.Item14 with { Volume = 1f, Pitch = -0.4f }
                : style == 1 ? SoundID.Item62 with { Volume = 0.8f }
                : SoundID.Item62 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);

            if (style == 2)
                SoundEngine.PlaySound(SoundID.Item62 with { Volume = 1f, Pitch = -0.6f }, Projectile.Center);

            float punch = style == 2 ? 14f : style == 1 ? 5f : 2f;
            
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), punch, 6f, 14 + style * 10, 1800f, "SporeBlast"));
            int dusts = 18 + style * 30;
            for (int i = 0; i < dusts; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(1f, 1f) * (Radius * 0.02f + 2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.ChlorophyteWeapon, v, 80, default, Main.rand.NextFloat(1f, 2.2f));
                d.noGravity = true;
                if (Main.rand.NextBool(3))
                {
                    Dust g = Dust.NewDustPerfect(Projectile.Center, DustID.JungleSpore, v * 0.6f + new Vector2(0f, -1.5f), 60, default, Main.rand.NextFloat(0.9f, 1.6f));
                    g.noGravity = false;
                }
            }
            if (style == 2)
            {
                for (int i = 0; i < 40; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 18f), 0, default, 2f);
                    d.noGravity = true;
                }
            }
        }
        public override void AI()
        {
            Color c = RingColor();
            Lighting.AddLight(Projectile.Center, c.ToVector3() * (1.6f - Progress) * (1f + Style));
            float prog = Progress;
            float ringR = Radius * 1.3f * (0.08f + prog * 0.84f);
            int points = 10 + Style * 8;
            for (int i = 0; i < points; i++)
            {
                float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * (ringR + Main.rand.NextFloat(-6f, 6f));
                Dust d = Dust.NewDustPerfect(pos, DustID.ChlorophyteWeapon, ang.ToRotationVector2() * (1.2f + prog * 2f), 60, default, (1.7f - prog) * (Style == 2 ? 1.6f : 1.1f));
                d.noGravity = true;
            }
        }
        private Color RingColor()
        {
            if (Style == 0) return new Color(140, 230, 100);
            if (Style == 1) return new Color(120, 255, 90);
            return new Color(255, 200, 90);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeNoise").Value;
            Texture2D ringTex = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeRingTex").Value;
            Texture2D soft = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeSoft").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float prog = Progress;
            Color col = RingColor();
            float quadSize = Radius * 2.6f;
            Main.spriteBatch.End();
            if (SporeShaders.Ring != null)
            {
                SporeShaders.Ring.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                SporeShaders.Ring.Parameters["uProgress"]?.SetValue(prog);
                SporeShaders.Ring.Parameters["uColor"]?.SetValue(col.ToVector3());
                SporeShaders.Ring.Parameters["uSeed"]?.SetValue(Projectile.whoAmI * 0.137f);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, SporeShaders.Ring, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(noise, drawPos, null, Color.White, 0f, noise.Size() * 0.5f, quadSize / noise.Width, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                float wob = 1f + 0.03f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 40f + Projectile.whoAmI);
                float ringScale = (0.10f + prog * 1.0f) * quadSize / ringTex.Width * wob;
                float fade = MathHelper.Clamp(1.1f - prog * 1.1f, 0f, 1f);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(ringTex, drawPos, null, Color.Black * (0.55f * fade), prog * 0.6f, ringTex.Size() * 0.5f, ringScale * 1.10f, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(ringTex, drawPos, null, col * (1.5f * fade), prog * 0.6f, ringTex.Size() * 0.5f, ringScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(ringTex, drawPos, null, Color.White * (0.6f * fade), -prog * 0.4f, ringTex.Size() * 0.5f, ringScale * 0.94f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(ringTex, drawPos, null, col * (0.7f * fade), prog, ringTex.Size() * 0.5f, ringScale * 0.72f, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            float flash = MathHelper.Clamp(1f - prog * 2.8f, 0f, 1f);
            if (flash > 0f)
            {
                Main.spriteBatch.Draw(soft, drawPos, null, Color.White * flash, 0f, soft.Size() * 0.5f, quadSize * 0.35f / soft.Width * (0.6f + prog * 2f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(soft, drawPos, null, col * (flash * 0.9f), 0f, soft.Size() * 0.5f, quadSize * 0.6f / soft.Width * (0.6f + prog * 2f), SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
