using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Windows;
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
using jarkos;
namespace jarkos.projectiles.ranged.other
{
	public class Bomb : ModProjectile
	{
	private int rippleCount = 5;
	private int rippleSize = 5;
	private int rippleSpeed = 10;
	private float distortStrength = 200f;
public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 8;
			Projectile.height = 20;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 40;
			Projectile.penetrate = 1;
		}
public override void AI()
{
            if (Main.netMode != NetmodeID.Server && !Filters.Scene["Shockwave"].IsActive())
            {
                Filters.Scene.Activate("Shockwave", Projectile.Center).GetShader().UseColor(rippleCount, rippleSize, rippleSpeed).UseTargetPosition(Projectile.Center);
            }
        if (Main.netMode != NetmodeID.Server && Filters.Scene["Shockwave"].IsActive())
        {
            float progress = Projectile.timeLeft;
            Filters.Scene["Shockwave"].GetShader().UseProgress(progress).UseOpacity(distortStrength * (1 - progress / 3f));
        }
    }
public override void OnKill(int timeLeft)
{
    if (Main.netMode != NetmodeID.Server && Filters.Scene["Shockwave"].IsActive())
    {
        Filters.Scene["Shockwave"].Deactivate();
    }
}
	}
}
