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
    public class Slimebomb_aura : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.tileCollide = false;
            Projectile.width = 750;
            Projectile.height = 750;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 15;
			Projectile.penetrate = -1;
            Projectile.light = 0.5f;
        }
        public override void AI()
        {
            Projectile.velocity *= 0f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int a = 0; a < 30; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(1f,1f);
			Dust d = Dust.NewDustPerfect(target.Center, 54, null, Scale: 1f);
			d.noGravity = true;
            d.velocity.Y += 0.15f;
            }
        }
    }
}
