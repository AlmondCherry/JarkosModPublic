using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
namespace jarkos.projectiles.melee.unique_melee
{
    public class Dante_sword_charge : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/Dante_sword";
        private const int MAX_CHARGE = 40;
        private int timer;
        private float aimAngle;
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 600;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(aimAngle);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            aimAngle = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Player player = Main.player[Projectile.owner];
                aimAngle = (Main.MouseWorld - player.MountedCenter).ToRotation();
                Projectile.rotation = aimAngle;
                Projectile.netUpdate = true;
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.CCed || player.noItems)
            {
                Projectile.Kill();
                return;
            }
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 toMouse = Main.MouseWorld - player.MountedCenter;
                float oldAim = aimAngle;
                aimAngle = toMouse.ToRotation();
                player.ChangeDir(Math.Cos(aimAngle) >= 0 ? 1 : -1);
                if (Math.Abs(MathHelper.WrapAngle(aimAngle - oldAim)) > 0.01f || timer % 10 == 0)
                {
                    Projectile.netUpdate = true;
                }
            }
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            timer++;
            float chargeProgress = MathHelper.Clamp(timer / (float)MAX_CHARGE, 0f, 1f);
            float baseRot = -MathHelper.PiOver2;
            float armRot = baseRot + player.direction * MathHelper.Lerp(0.5f, -0.4f, chargeProgress);
            Vector2 armOffset = armRot.ToRotationVector2() * MathHelper.Lerp(5f, 15f, chargeProgress);
            Projectile.Center = player.MountedCenter + armOffset;
            Projectile.rotation = armRot + (player.direction == 1 ? MathHelper.PiOver4 : MathHelper.PiOver4 * 3f);
            if (timer >= MAX_CHARGE)
            {
                float shake = (float)Math.Sin(timer * 2.5f) * 0.08f;
                Projectile.rotation += shake;
                if (timer == MAX_CHARGE && Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.5f, Pitch = 0.5f }, player.Center);
                }
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot - MathHelper.PiOver2);
            if (!player.controlUseTile && Main.myPlayer == Projectile.owner)
            {
                ThrowSword(player, chargeProgress);
                Projectile.Kill();
            }
        }
        private void ThrowSword(Player player, float chargeProgress)
        {
            Vector2 throwDir = aimAngle.ToRotationVector2();
            float speed = MathHelper.Lerp(15f, 45f, chargeProgress);
            float distance = MathHelper.Lerp(200f, 750f, chargeProgress);
            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f, Pitch = -0.2f }, player.Center);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, throwDir * speed, ModContent.ProjectileType<Dante_sword_thrown>(), Projectile.damage, Projectile.knockBack, player.whoAmI, distance, 0f);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(10f, tex.Height - 10f);
            SpriteEffects effects = playerDirection() == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float drawRot = Projectile.rotation;
            if (playerDirection() == -1)
            {
                origin = new Vector2(tex.Width - 10f, tex.Height - 10f);
            }
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, drawRot, origin, Projectile.scale, effects, 0f);
            return false;
        }
        private int playerDirection()
        {
            return Main.player[Projectile.owner].direction;
        }
    }
}
