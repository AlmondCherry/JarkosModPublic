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
using Terraria.DataStructures;
using jarkos.buffs.buff;
namespace jarkos.projectiles.bullet
{
	public class Clockwithtimer_proj : ModProjectile
    {
        private Player playerr => Main.player[Projectile.owner];
        private float strength_shake = 0.25f;
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 150;
            Projectile.scale = 1f;
            Projectile.damage = 0;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.light = 0.5f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ticking") with {Pitch = Main.rand.NextFloat(-0.05f, 0.05f)}, Projectile.position);
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.ai[0]++;
            if(Projectile.ai[0] > 30)
            {
                strength_shake++;
                Projectile.ai[0] = 0;
            }
            Vector2 vec = new Vector2(0, -75);
            Vector2 vec2 = new Vector2(Main.rand.NextFloat(-strength_shake * 1.5f, strength_shake * 1.5f), Main.rand.NextFloat(-strength_shake, strength_shake));
            Projectile.Center = player.Center + vec;
            Projectile.Center = player.Center + vec + vec2;
        }
        public override void OnKill(int timeLeft)
        {
            int oneGore = Mod.Find<ModGore>("goreclock1").Type;
            int twoGore = Mod.Find<ModGore>("goreclock2").Type;
            int threeGore = Mod.Find<ModGore>("goreclock3").Type;
			int fourGore = Mod.Find<ModGore>("goreclock4").Type;
            for (int i = 0; i < 1; i++)
            {
                Gore.NewGore(null, Projectile.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), oneGore);
                Gore.NewGore(null, Projectile.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), twoGore);
            }
            for (int i = 0; i < 3; i++)
            {
                Gore.NewGore(null, Projectile.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), threeGore);
                Gore.NewGore(null, Projectile.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), fourGore);
            }
            playerr.AddBuff(ModContent.BuffType<Lifeforcing>(), 460);
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/breaking_clock") with {Pitch = Main.rand.NextFloat(-0.05f, 0.05f)}, Projectile.position);
        }
    }
}
