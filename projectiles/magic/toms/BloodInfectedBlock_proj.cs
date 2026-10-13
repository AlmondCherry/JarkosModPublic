using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.buffs.debuff;
namespace jarkos.projectiles.magic.toms
{
    public class BloodInfectedBlock_proj : ModProjectile
    {
        private const int Life = 300;
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
            Projectile.hide = false;
        }
        public override void AI()
        {
            if (Projectile.timeLeft > Life - 30)
                Projectile.alpha = (int)MathHelper.Lerp(255, 0, (Life - Projectile.timeLeft) / 30f);
            else if (Projectile.timeLeft < 30)
                Projectile.alpha = (int)MathHelper.Lerp(0, 255, 1f - Projectile.timeLeft / 30f);
            else
                Projectile.alpha = 0;
            Lighting.AddLight(Projectile.Center, 0.4f * (1f - Projectile.alpha / 255f), 0.05f, 0.05f);
            if (Main.rand.NextBool(8))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, -Main.rand.NextFloat(1f, 2f), 100, default, 1f);
                d.noGravity = true;
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    Rectangle blockHitbox = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y - 16, Projectile.width, Projectile.height + 16);
                    if (npc.Hitbox.Intersects(blockHitbox))
                    {
                        npc.AddBuff(ModContent.BuffType<BloodFieldDebuff>(), 120);
                    }
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/BlockGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0, -10f);
            float fade = 1f - Projectile.alpha / 255f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + Projectile.whoAmI);
            float scaleX = 24f / tex.Width * pulse;
            float scaleY = 32f / tex.Height * pulse;
            Main.spriteBatch.Draw(tex, drawPos, null, new Color(255, 20, 20) * fade * 0.9f, 0f, tex.Size() * 0.5f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, drawPos, null, new Color(255, 60, 60) * fade * 0.6f, 0f, tex.Size() * 0.5f, new Vector2(scaleX * 0.6f, scaleY * 0.6f), SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
