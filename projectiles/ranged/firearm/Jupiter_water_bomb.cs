using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.ranged.firearm
{
    public class Jupiter_water_bomb : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.scale = 1.5f;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.15f;
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Water, 0f, 0f, 100, default, 1.8f);
                d.noGravity = true;
                d.velocity = -Projectile.velocity * 0.3f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item21, Projectile.Center);
            for (int i = 0; i < 40; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Water, vel.X, vel.Y, 100, default, 2f);
                d.noGravity = Main.rand.NextBool();
            }
            if (Projectile.owner == Main.myPlayer)
            {
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.CanBeChasedBy())
                    {
                        if (Vector2.Distance(Projectile.Center, npc.Center) < 120f)
                        {
                            npc.SimpleStrikeNPC(Projectile.damage, 0, false, Projectile.knockBack);
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(glowTex, drawPos, null, new Color(50, 150, 255, 255) * 1.5f, Projectile.rotation, glowTex.Size() / 2f, Projectile.scale * pulse * 0.25f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(glowTex, drawPos, null, new Color(255, 255, 255, 255) * 0.8f, Projectile.rotation, glowTex.Size() / 2f, Projectile.scale * pulse * 0.05f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(mainTex, drawPos, null, lightColor, Projectile.rotation, mainTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
