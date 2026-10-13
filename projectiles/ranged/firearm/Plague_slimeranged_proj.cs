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
    public class Plague_slimeranged_proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.tileCollide = true;
            Projectile.width = 21;
            Projectile.height = 52;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 800;
			Projectile.penetrate = 1;
        }
        public override void AI()
        {
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			new Circleedge(target.Center, Vector2.Zero, Color.MediumPurple, 60, 0.35f, 1f, 1f).Spawn();
		}
        public override void OnKill(int timeLeft)
        {
            for (int a = 0; a < 15; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f,2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, DustID.DarkCelestial, speed * 3, Scale: 2f);
			d.noGravity = true;
            }
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
    }
}
