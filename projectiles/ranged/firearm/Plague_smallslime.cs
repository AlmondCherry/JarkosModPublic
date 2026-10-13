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
namespace jarkos.projectiles.ranged.firearm
{
    public class Plague_smallslime : ModProjectile
    {
        public float rand_scale = Main.rand.NextFloat(0.001f, 0.009f);
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.tileCollide = true;
            Projectile.width = 24;
            Projectile.height = 34;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 250;
			Projectile.penetrate = -1;
        }
        public override void AI()
        {
            if(Projectile.scale > 0.10f)
            {
            Projectile.scale -= rand_scale;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.16f;
        }
        public override void OnKill(int timeLeft)
        {
            for (int a = 0; a < 15; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f,2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, 54, speed * 3, Scale: 1f);
			d.noGravity = false;
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, Projectile.velocity.X * 0, Projectile.velocity.Y * 0, ModContent.ProjectileType<Smallslime_aura>(), 20, 0f, Projectile.owner, 0f, 0f);
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/smallslime_impact"), Projectile.position);
        }
    }
}
