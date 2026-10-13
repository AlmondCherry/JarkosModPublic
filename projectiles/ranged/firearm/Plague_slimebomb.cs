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
    public class Plague_slimebomb : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.tileCollide = true;
            Projectile.width = 21;
            Projectile.height = 22;
            Projectile.scale = 0.5f;
            Projectile.timeLeft = 80;
            Projectile.damage = 0;
			Projectile.penetrate = -1;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.velocity *= 0.95f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.damage = 0;
            if(Projectile.scale < 2f)
            {
            Projectile.scale += 0.0098f;
            }
            Vector2 pos = new Vector2(Main.rand.NextFloat(-100, 100), Main.rand.NextFloat(-100, 100));
            Dust d = Dust.NewDustPerfect(Projectile.Center + pos, 54, null, Scale: Projectile.scale);
			d.noGravity = true;
            d.velocity = Vector2.Normalize(Projectile.Center - d.position) * 9;
        }
        public override void OnKill(int timeLeft)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<Screenshake_system>();
            for (int i = 0; i < 3; i++) {
            new Circleedge_slimebomb(Projectile.Center, Vector2.Zero, Color.Brown, 95, 0.25f, 1f, 1f).Spawn();
            new Circleedge_slimebomb(Projectile.Center, Vector2.Zero, Color.Brown, 80, 0.25f, 1f, 1f).Spawn();
            new Circleedge_slimebomb(Projectile.Center, Vector2.Zero, Color.Brown, 65, 0.25f, 1f, 1f).Spawn();
            }
            for (int i = 0; i < 20; i++) {
            Vector2 vct = Main.rand.NextVector2CircularEdge(12f, 12f);
            float size = Main.rand.NextFloat(0.4f, 0.7f);
            new Dust1_particle(Projectile.Center, vct, Color.Brown, 160, size, 2f, 0f).Spawn();
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, Projectile.velocity.X * 0, Projectile.velocity.Y * 0, ModContent.ProjectileType<Slimebomb_aura>(), 70, 0f, Projectile.owner, 0f, 0f);
			modPlayer.regular_shake = 1f;
            for (int i = 0; i < 15; i++) {
            Vector2 rand_vct = Main.rand.NextVector2CircularEdge(4f, 4f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rand_vct, ModContent.ProjectileType<Plague_smallslime>(), 20, 0f, Projectile.owner, 0f, 0f);
            }
            for (int a = 0; a < 25; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f,2f);
			Dust d = Dust.NewDustPerfect(Projectile.Center, 54, speed * 5, Scale: 1f);
			d.noGravity = true;
            }
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/slimebomb_impact_s"), Projectile.position);
        }
    }
}
