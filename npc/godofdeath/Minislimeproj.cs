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
using jarkos.systems;
using jarkos.buffs;
namespace jarkos.npc.godofdeath
{
	public class Minislimeproj : ModProjectile
	{
		float direction = Main.rand.NextFloat(-1f,1f);
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 6;
			Projectile.height = 12;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 5;
		}
        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
			Projectile.velocity.Y = Projectile.velocity.Y + (0.2293803f);
        	Projectile.rotation = Terraria.Utils.ToRotation(Projectile.velocity) - 1.570796f;
			Projectile.velocity *= 1.02f;
        }
    }
}
