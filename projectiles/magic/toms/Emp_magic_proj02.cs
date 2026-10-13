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
using Jarkos.trails;
using jarkos.particles;
using jarkos.systems;
namespace jarkos.projectiles.magic.toms
{
	public class Emp_magic_proj02 : ModProjectile
	{
        private int explosion_dust = 0;
        private float sizechange = 0;
        private int degrees = 0;
		NPC targetNPC = null;
        float direction = Main.rand.NextFloat(-1f,1f);
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.netImportant = true;
            Projectile.tileCollide = false;
            Projectile.width = 4;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.damage = 1;
            Projectile.timeLeft = 180;
            Projectile.light = 1f;
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int a = 0; a < 15; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f * sizechange,2f * sizechange);
			Dust d = Dust.NewDustPerfect(Projectile.position, DustID.YellowTorch, speed * 5, Scale: 1f);
			d.noGravity = true;
            }
            for (int i = 0; i < 1; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(8f, 8f);
                float size = Main.rand.NextFloat(0.1f, 0.3f);
                new StarParticle(target.Center, vct, Color.Yellow, 140, size / 16, 2f, 0f).Spawn();
            }
            for (int i = 0; i < 1; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(8f, 8f);
                float size = Main.rand.NextFloat(0.1f, 0.3f);
                new StarParticle(target.Center, vct, Color.Orange, 140, size / 12, 2f, 0f).Spawn();
            }
            for (int i = 0; i < 1; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(8f, 8f);
                float size = Main.rand.NextFloat(0.1f, 0.3f);
                new StarParticle(target.Center, vct, Color.IndianRed, 140, size / 8, 2f, 0f).Spawn();
            }
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
		}
        public override void AI(){
        Projectile.velocity.Y = Projectile.velocity.Y + (0.5f * direction);
        degrees++;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        explosion_dust++;
        if(explosion_dust == 15 || explosion_dust == 30 || explosion_dust == 45 || explosion_dust == 60) {
          sizechange++;
        }
        if(explosion_dust == 15 || explosion_dust == 30 || explosion_dust == 45 || explosion_dust == 60) {
           for (int a = 0; a < 15; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(0.5f * sizechange,1f * sizechange);
			Dust d = Dust.NewDustPerfect(Projectile.position, DustID.YellowTorch, speed * 1, Scale: 1f);
			d.noGravity = true;
			}
        }
        NPChoming.HomingProj(Projectile, 1000f, 3f, 180);
        }
        public Primitivedraw TrailDrawer { get; private set; } = null;
        public float WidthFunction(float completionRatio)
        {
            float width_trail = 25f;
            float baseWidth = Projectile.scale * (Projectile.width / 4) * width_trail;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }
        public Color ColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Yellow, Color.Transparent, completionRatio) * 0.7f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            TrailDrawer ??= new Primitivedraw(WidthFunction, ColorFunction, GameShaders.Misc["Jarkos:LaserShader"]);
            GameShaders.Misc["Jarkos:LaserShader"].SetShaderTexture(Trailreg.test);
            TrailDrawer.DrawPrims(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 155);
            return true;
        }
    }
}
