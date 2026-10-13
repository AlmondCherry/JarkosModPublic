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
using jarkos.systems;
namespace jarkos.projectiles.ranged.other
{
	public class Bee_rocket : ModProjectile
	{
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 20;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }
        public override void OnKill(int timeLeft)
        {
            for (int y = 0; y < 15; y++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Honey, speed * 3, Scale: 1f);
                d.noGravity = false;
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(12f, 12f);
                float size = Main.rand.NextFloat(0.4f, 0.7f);
                new Dust1_particle(Projectile.Center, vct, Color.Orange, 160, size, 2f, 0f).Spawn();
            }
            for (int a = 0; a < 5; a++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(2f, 2f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.velocity * vct) / 6, 181, 20, 0f, Projectile.owner, 0f, 0f);
            }
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/smallslime_impact"), Projectile.position);
        }
        private float rotationSpeed = 0.01f;
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if(Projectile.timeLeft > 500)
            {
                Projectile.velocity *= 0.97f;
            }
            else
            {
                NPChoming.HomingProj(Projectile, 1000f, 2f, 490);
                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.Lerp(MathHelper.ToRadians(1), MathHelper.ToRadians(8), rotationSpeed));
            }
            foreach(NPC n in Main.npc)
            {
                if (n.active)
                {
                    float distance = Vector2.Distance(Projectile.Center, n.Center);
                    if (distance < 50f)
                    {
                        Projectile.Kill();
                    }
                }
            }
        }
    }
}
