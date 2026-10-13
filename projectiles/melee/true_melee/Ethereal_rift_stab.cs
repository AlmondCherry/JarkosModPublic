using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class Ethereal_rift_stab : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/Ethereal_rift";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.timeLeft = 30;
            Projectile.hide = true; 
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }
            
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.velocity.X *= 0.8f;
            
            Vector2 stabDir = new Vector2(0, 1);
            
            float progress = 1f - (Projectile.timeLeft / 30f); 
            
            float offset = MathHelper.Lerp(-40f, 40f, (float)Math.Pow(progress, 2));
            
            Projectile.Center = player.MountedCenter + stabDir * offset;
            Projectile.rotation = stabDir.ToRotation() + MathHelper.PiOver4;
            
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, stabDir.ToRotation() - MathHelper.PiOver2);
            
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 6)
                {
                    Projectile.frame = 5; 
                }
            }
            if (Projectile.timeLeft == 6)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
                    if (Main.LocalPlayer.whoAmI == Projectile.owner)
                    {
                        Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake += 2;
                    }
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2Circular(10f, 2f);
                        dustVel.Y -= 2f;
                        Dust d = Dust.NewDustDirect(player.Bottom - new Vector2(20, 10), 40, 20, DustID.BlueFairy, dustVel.X, dustVel.Y, 100, default, 1.5f);
                        d.noGravity = true;
                    }
                }

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 domainPos = player.Bottom;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), domainPos, Vector2.Zero, ModContent.ProjectileType<Ethereal_rift_domain>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ethereal_rift_planted>(), 0, 0f, Projectile.owner, Projectile.rotation);
                }

                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / 6;
            Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, tex.Width, frameHeight);
            
            Vector2 drawOrigin = new Vector2(tex.Width * 0.5f, frameHeight * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            Main.spriteBatch.Draw(tex, drawPos, frameRect, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, drawPos, frameRect, Ethereal_rift_dante_swing.ColorCyan * 0.5f, Projectile.rotation, drawOrigin, Projectile.scale * 1.1f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
