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
using Jarkos.projectiles.mecha;
namespace jarkos.projectiles.magic.toms
{
    public class LB_Penta : ModProjectile
    {
        NPC targetNPC = null;
        public static bool reset = false;
        public float inactive_timer = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.timeLeft = 2;
			Projectile.penetrate = -1;
            Projectile.scale = 0.5f;
            Projectile.alpha = 0;
            Projectile.light = 1f;
            Projectile.damage = 0;
        }
        public NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC target = Main.npc[k];
                if (target.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }
            return closestNPC;
        }
		private int manaTimer = 0;

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter);
			Projectile.damage = 0;
			Projectile.ai[0] += 1f;
            Projectile.ai[1] += 1f;
            Projectile.velocity.X = 0f;
            Projectile.rotation += 0.07f;
            if(Projectile.ai[0] < 40)
            {
                Projectile.scale += 0.05f;
                Projectile.alpha += 5;
                if(Projectile.scale > 1.25f)
                {
                    Projectile.scale = 1.25f;
                }
                 if(Projectile.alpha < 1)
                {
                    Projectile.alpha = 0;
                }
            }
            if(reset)
            {
                targetNPC = null;
                reset = false;
            }
            if(Projectile.ai[0] < 10)
            {
                Projectile.Center = player.Center;
            }
            Vector2 Vel = new Vector2(0f, -1f);
            if(Projectile.ai[0] < 10)
            {
                Projectile.velocity += Vel;
            }
            else
            {
                Projectile.velocity *= 0.92f;
            }
			if (Projectile.owner == Main.myPlayer)
			{
				bool StillInUse = player.channel && !player.noItems && !player.CCed;
				if (StillInUse)
				{
                    
                    manaTimer++;
                    if (manaTimer >= 60)
                    {
                        manaTimer = 0;
                        int manaCost = Main.rand.Next(5, 11);
                        if (player.statMana < manaCost)
                        {
                            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f }, Projectile.Center);
                            for (int i = 0; i < 15; i++)
                            {
                                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(16), 32, 32, DustID.PinkTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.4f);
                                d.noGravity = true;
                            }
                            Projectile.Kill();
                            return;
                        }
                        player.statMana -= manaCost;
                        player.manaRegenDelay = 180;
                    }

                    if(Projectile.ai[1] > 10)
                    {
                        Vector2 to_mouse = (Main.MouseWorld - Projectile.Center) * 16f;
                        to_mouse.Normalize();
                        Projectile.NewProjectile(null, Projectile.Center, to_mouse * 16f, ModContent.ProjectileType<Lightburn_beam>(), player.HeldItem.damage, 0f, Projectile.owner);
                        Projectile.ai[1] = 0;
                    }
					Projectile.netUpdate = true;
				}
				else if (!StillInUse)
				{
					Projectile.Kill();
				}
			}
			Projectile.timeLeft = 2000;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			SpriteEffects eff = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
			int texYOffset = frameHeight * Projectile.frame;
            Color clr = new Color(255, 255, 255, Projectile.alpha);
			Vector2 sheetInsertVec = (Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();
			Main.EntitySpriteDraw(tex, sheetInsertVec, new Rectangle(0, texYOffset, tex.Width, frameHeight), clr, Projectile.rotation, new Vector2((float)tex.Width / 2f, (float)frameHeight / 2f), Projectile.scale, eff, 0);
			return false;
		}
    }
}
