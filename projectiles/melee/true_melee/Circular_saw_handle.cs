using System;
using System.Collections.Generic;
using System.IO;
using jarkos.items.weapons.melee.true_melee;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	public class Circular_saw_handle : ModProjectile
    {
        private const int MAX_CHARGE_TICKS = 120;
        private const float HANDLE_DISTANCE = 50f;
        private const float MAX_SHAKE = 2f;
        private ref float Timer => ref Projectile.localAI[0];
        private int bladeIndex = -1;
        private int soundTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 60;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 999999;
            Projectile.alpha = 0;
            Projectile.hide = true;
            Projectile.netImportant = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.rotation);
            writer.Write(Projectile.localAI[0]); 
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.rotation = reader.ReadSingle();
            Projectile.localAI[0] = reader.ReadSingle(); 
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            bool stillUsing = owner.active
                && !owner.dead
                && owner.channel
                && owner.HeldItem.type == ModContent.ItemType<CircularSaw>();

            if (!stillUsing)
            {
                KillBlade();
                Projectile.Kill();
                return;
            }

            Timer++;

            float chargeProgress = MathHelper.Clamp(Timer / MAX_CHARGE_TICKS, 0f, 1f);

            Vector2 direction;
            float angle;

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 toMouse = Main.MouseWorld - owner.Center;
                direction = toMouse.SafeNormalize(Vector2.UnitX);
                angle = direction.ToRotation();

                if (MathF.Abs(Projectile.rotation - angle) > 0.01f || (int)Timer % 15 == 0)
                {
                    Projectile.rotation = angle;
                    Projectile.netUpdate = true;
                }
                else
                {
                    Projectile.rotation = angle;
                }
            }
            else
            {
                angle = Projectile.rotation;
                direction = angle.ToRotationVector2();
            }

            owner.direction = (direction.X >= 0) ? 1 : -1;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, angle - MathHelper.PiOver2);
            owner.heldProj = Projectile.whoAmI;

            float shake = MAX_SHAKE * chargeProgress * chargeProgress;
            Vector2 shakeOffset = Vector2.Zero;
            if (shake > 0.1f)
            {
                shakeOffset = new Vector2(Main.rand.NextFloat(-shake, shake), Main.rand.NextFloat(-shake, shake));
            }

            Projectile.Center = owner.RotatedRelativePoint(owner.MountedCenter)
                + direction * HANDLE_DISTANCE
                + shakeOffset;

            Projectile.velocity = Vector2.Zero;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            ManageBlade(owner, chargeProgress, shakeOffset);
            ManageSounds(chargeProgress);

            if (chargeProgress > 0.3f && Main.rand.NextBool(8))
            {
                Vector2 bladePos = GetBladeAttachPoint();
                Dust d = Dust.NewDustDirect(bladePos, 1, 1, DustID.Blood);
                d.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                d.scale = Main.rand.NextFloat(0.4f, 0.8f);
                d.noGravity = true;
            }
        }

        private void ManageBlade(Player owner, float chargeProgress, Vector2 shakeOffset)
        {
            bool bladeAlive = bladeIndex >= 0
                && bladeIndex < Main.maxProjectiles
                && Main.projectile[bladeIndex].active
                && Main.projectile[bladeIndex].type == ModContent.ProjectileType<Saw_blade>()
                && Main.projectile[bladeIndex].owner == Projectile.owner;

            if (!bladeAlive)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.type == ModContent.ProjectileType<Saw_blade>() && p.owner == Projectile.owner && (int)p.ai[0] == Projectile.whoAmI)
                    {
                        bladeIndex = i;
                        bladeAlive = true;
                        break;
                    }
                }

                if (!bladeAlive && Main.myPlayer == Projectile.owner)
                {
                    Vector2 bladePos = GetBladeAttachPoint();
                    bladeIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), bladePos, Vector2.Zero, ModContent.ProjectileType<Saw_blade>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.whoAmI);
                    bladeAlive = bladeIndex >= 0 && bladeIndex < Main.maxProjectiles;
                }
            }

            if (bladeAlive)
            {
                Projectile blade = Main.projectile[bladeIndex];
                blade.Center = GetBladeAttachPoint() + shakeOffset;
                blade.ai[1] = chargeProgress;
            }
        }

        private Vector2 GetBladeAttachPoint()
        {
            Vector2 direction = Projectile.rotation.ToRotationVector2();
            return Projectile.Center + direction * 48.5f;
        }

        private void ManageSounds(float chargeProgress)
        {
            if (Main.netMode == NetmodeID.Server) return;

            soundTimer++;
            if (Timer == 5)
            {
                SoundEngine.PlaySound(SoundID.Item22 with
                {
                    Volume = 0.4f, Pitch = -0.5f
                }, Projectile.Center);
            }

            int soundInterval = (int)MathHelper.Lerp(25, 6, chargeProgress);
            if (soundTimer >= soundInterval)
            {
                soundTimer = 0;
                SoundEngine.PlaySound(SoundID.Item23 with
                {
                    Volume = MathHelper.Lerp(0.2f, 0.5f, chargeProgress), Pitch = MathHelper.Lerp(-0.5f, 0.5f, chargeProgress)
                }, Projectile.Center);
            }

            if (Timer == MAX_CHARGE_TICKS)
            {
                SoundEngine.PlaySound(SoundID.Item38 with
                {
                    Volume = 0.5f, Pitch = 0.2f
                }, Projectile.Center);
            }
        }

        private void KillBlade()
        {
            if (bladeIndex >= 0
                && bladeIndex < Main.maxProjectiles
                && Main.projectile[bladeIndex].active
                && Main.projectile[bladeIndex].type == ModContent.ProjectileType<Saw_blade>())
            {
                Main.projectile[bladeIndex].Kill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            KillBlade();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            SpriteEffects effects = SpriteEffects.None;
            Player owner = Main.player[Projectile.owner];
            if (owner.direction < 0)
                effects = SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }

        public override bool ShouldUpdatePosition() => false;
        public override bool? CanCutTiles() => false;
        public override bool? CanHitNPC(NPC target) => false;
	}
}
