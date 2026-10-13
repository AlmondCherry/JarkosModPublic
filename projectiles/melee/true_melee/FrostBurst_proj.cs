using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class FrostBurst_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/true_melee/FrostBurst_proj";
        private const int LifeTicks = 22;
        private const int ExpandTicks = 6;
        private const float BaseRadius = 64f;
        private float RadiusScale => Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0];
        private int StageColorIdx => (int)MathHelper.Clamp(Projectile.ai[1], 0f, 3f);
        private float Timer => LifeTicks - Projectile.timeLeft;
        private float MaxRadius => BaseRadius * RadiusScale;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
        }
        public override void SetDefaults()
        {
            Projectile.width = (int)(BaseRadius * 2.5f);
            Projectile.height = (int)(BaseRadius * 2.5f);
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LifeTicks;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void OnSpawn(IEntitySource source)
        {
            float pitchBoost = StageColorIdx == 3 ? -0.20f : 0.10f + 0.08f * (3 - StageColorIdx);
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = pitchBoost, Volume = 0.6f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.30f, Volume = 0.4f }, Projectile.Center);
            int burstDust = 24 + StageColorIdx * 8;
            for (int i = 0; i < burstDust; i++)
            {
                float a = MathHelper.TwoPi * i / burstDust;
                Vector2 dv = a.ToRotationVector2() * Main.rand.NextFloat(2.0f, 7.5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SnowflakeIce, dv, 60, default, Main.rand.NextFloat(1.2f, 2.0f));
                d.noGravity = true;
            }
            for (int i = 0; i < 14 + StageColorIdx * 5; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, dv, 100, default, Main.rand.NextFloat(1.1f, 1.7f));
                d.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.5f, 0.85f, 1.2f);
        }
        public override void AI()
        {
            float fade = MathHelper.Clamp(Projectile.timeLeft / (float)LifeTicks, 0f, 1f);
            Lighting.AddLight(Projectile.Center, 0.4f * fade, 0.7f * fade, 1.0f * fade);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float r = CurrentRadius();
            if (r <= 0f) return false;
            Vector2 npcCenter = new Vector2(targetHitbox.X + targetHitbox.Width / 2f, targetHitbox.Y + targetHitbox.Height / 2f);
            float halfDim = MathF.Max(targetHitbox.Width, targetHitbox.Height) * 0.5f;
            float dist = Vector2.Distance(Projectile.Center, npcCenter);
            return dist <= r + halfDim ? true : null;
        }
        private float CurrentRadius()
        {
            float age = Timer;
            if (age < ExpandTicks)
            {
                float u = age / ExpandTicks;
                return MaxRadius * MathSystem.EaseOutCubic(u);
            }
            return MaxRadius;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 180);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texAsset = TextureAssets.Projectile[Projectile.type];
            if (!texAsset.IsLoaded) return false;
            Texture2D texture = texAsset.Value;
            Vector2 origin = texture.Size() / 2f;
            float t = Timer / (float)LifeTicks;
            float visualScale;
            float alpha;
            if (t < ExpandTicks / (float)LifeTicks)
            {
                float u = Timer / ExpandTicks;
                visualScale = MathSystem.EaseOutCubic(u) * 1.0f;
                alpha = u;
            }
            else
            {
                float fade = 1f - (t - ExpandTicks / (float)LifeTicks) / (1f - ExpandTicks / (float)LifeTicks);
                visualScale = MathHelper.Lerp(1.0f, 1.25f, 1f - fade);
                alpha = fade;
            }
            Color coreColor = StageColorIdx switch
            {
                0 => new Color(180, 220, 250), 1 => new Color(160, 215, 252), 2 => new Color(140, 210, 255), _ => new Color(120, 200, 255), };
            float scaleMul = RadiusScale;
            try
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Vector2 screen = Projectile.Center - Main.screenPosition;
                Color outer = coreColor * (0.40f * alpha);
                Main.spriteBatch.Draw(texture, screen, null, outer, 0f, origin, visualScale * scaleMul * 1.6f, SpriteEffects.None, 0f);
                Color mid = coreColor * (0.55f * alpha);
                Main.spriteBatch.Draw(texture, screen, null, mid, 0f, origin, visualScale * scaleMul * 1.2f, SpriteEffects.None, 0f);
                Color inner = new Color(240, 250, 255) * (0.85f * alpha);
                Main.spriteBatch.Draw(texture, screen, null, inner, 0f, origin, visualScale * scaleMul * 0.85f, SpriteEffects.None, 0f);
            }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
