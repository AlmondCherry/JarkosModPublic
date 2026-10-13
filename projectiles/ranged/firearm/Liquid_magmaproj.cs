using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Windows;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;
using Terraria.UI.Chat;
using ReLogic.Content;
using Newtonsoft.Json;
using Terraria.GameContent.UI.States;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using jarkos.projectiles.magic.toms;
using Terraria.Graphics;
using System.Runtime.InteropServices;
using jarkos.particles;
using jarkos.particles.storm;
namespace jarkos.projectiles.ranged.firearm
{
    public class Liquid_magmaproj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.light = 0.75f;
            Projectile.scale *= Main.rand.NextFloat(0.05f, 0.15f);
            Projectile.timeLeft = 200;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1f, 0.5f, 0f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.15f;
            if(Projectile.timeLeft < 280)
            {
                Projectile.scale += 0.015f;
                if(Projectile.scale > 0.5f)
                {
                    Projectile.scale = 0.5f;
                }
            }
            if(Main.rand.NextBool(12))
            {
            Color[] colors = new Color[] {Color.Red, Color.Orange, Color.Yellow};
            Vector2 velY = new Vector2(Main.rand.NextFloat(-2.5f, 2.5f), -5);
            float size = Main.rand.NextFloat(0.25f, 0.35f);
			new Fire_particle(Projectile.Center, (Projectile.velocity * 0.5f) + velY, colors[Main.rand.Next(0, 2)], 120, size, 2f, Main.rand.NextFloat(0.1f, 1f)).Spawn();
            new Circular_glow(Projectile.Center, Projectile.velocity + velY, colors[Main.rand.Next(0, 2)], 160, size * 0.1f, 2f, 1f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
            Color[] colors = new Color[] {Color.Red, Color.Orange, Color.Yellow};
            Vector2 velY = new Vector2(0, -8);
            float size = Main.rand.NextFloat(0.95f, 1.25f);
			new Fire_particle(Projectile.Center, velY, colors[Main.rand.Next(0, 2)], 160, size * (Projectile.penetrate * 0.25f), 2f, 1f).Spawn();
            new Circular_glow(Projectile.Center, velY, colors[Main.rand.Next(0, 2)], 160, size * 0.75f * (Projectile.penetrate * 0.25f), 2f, 1f).Spawn();
            SoundEngine.PlaySound(SoundID.Item34, Projectile.position);
		}
        public override bool PreDraw(ref Color lightcolor) {
			Main.spriteBatch.End();
            Color[] colors = new Color[] {Color.Red, Color.Orange};
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle drawRect = new Rectangle((int)Projectile.position.X - (int)Main.screenPosition.X, (int)Projectile.position.Y - (int)Main.screenPosition.Y, texture.Width, texture.Height);
            var origin = texture.Size() / 2f;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, colors[Main.rand.Next(0, 2)], Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			return false;
        }
    }
}
