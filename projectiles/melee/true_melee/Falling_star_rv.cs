using System;
using System.Collections.Generic;
using jarkos.buffs.debuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Falling_star_rv : ModProjectile
	{
		private float Timer => Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.velocity.Length() < 25f)
            {
                Projectile.velocity *= 1.02f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust star = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.2f, 0, 0, DustID.BlueTorch, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 100, default, 1.3f);
                star.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 0.6f, 1f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FrostSlowDebuff>(), 90);
            target.AddBuff(BuffID.Frostburn2, 60);
            SoundEngine.PlaySound(SoundID.Item10, target.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.BlueTorch, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 100, default, 1.5f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = TextureAssets.Extra[98].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glow.Size() / 2;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                Color trailColor = new Color(150, 200, 255) * (1f - progress) * 0.5f;
                sb.Draw(glow, trailPos, null, trailColor, 0, origin, (1f - progress * 0.5f) * 0.6f, SpriteEffects.None, 0);
            }
            sb.Draw(glow, pos, null, new Color(100, 180, 255) * 0.6f, 0, origin, 1.2f, SpriteEffects.None, 0);
            sb.Draw(glow, pos, null, new Color(200, 230, 255) * 0.8f, 0, origin, 0.5f, SpriteEffects.None, 0);
            sb.Draw(glow, pos, null, Color.White * 0.9f, 0, origin, 0.25f, SpriteEffects.None, 0);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
	}
}
