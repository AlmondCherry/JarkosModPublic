using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.projectiles.magic.staffs;

namespace jarkos.projectiles.magic.staffs
{
    public class Rainfire_staff_swing : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.scale = 1f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter, true);


            bool stillInUse = player.channel;
            if (!stillInUse)
            {
                Projectile.Kill();
                return;
            }

            Projectile.ai[0]++;


            if (Projectile.ai[0] % 30 == 1)
            {
                if (!player.CheckMana(5, true, false))
                {
                    Projectile.Kill();
                    return;
                }
                player.manaRegenDelay = 60;
            }


            Vector2 aimDir = (Main.MouseWorld - rrp).SafeNormalize(Vector2.UnitX);
            Projectile.Center = rrp + aimDir * 24f;
            Projectile.velocity = aimDir;
            Projectile.rotation = aimDir.ToRotation() + MathHelper.PiOver4;

            player.ChangeDir(aimDir.X >= 0 ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = MathHelper.WrapAngle(aimDir.ToRotation() - (player.direction == -1 ? MathHelper.Pi : 0));


            if (Main.rand.NextBool(2))
            {
                Vector2 tipPos = Projectile.Center + aimDir * 28f;
                Dust d = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(6f, 6f), DustID.Torch, aimDir * 2f, 100, default, 1.3f);
                d.noGravity = true;
            }


            if (Projectile.owner == Main.myPlayer && Projectile.ai[0] % 6 == 0)
            {
                Vector2 targetMouse = Main.MouseWorld;


                float offsetX = Main.rand.NextFloat(-140f, 140f);
                float offsetY = Main.rand.NextFloat(-520f, -400f);
                Vector2 spawnPos = targetMouse + new Vector2(offsetX, offsetY);


                float manaRatio = (player.statManaMax2 > 0) ? (float)player.statMana / player.statManaMax2 : 1f;
                float manaSpeedMult = MathHelper.Lerp(2f, 1f, MathHelper.Clamp(manaRatio, 0f, 1f));


                Vector2 dropVel = (targetMouse + Main.rand.NextVector2Circular(30f, 30f) - spawnPos).SafeNormalize(Vector2.UnitY);
                dropVel *= Main.rand.NextFloat(14f, 19f) * manaSpeedMult;

                int damage = player.GetWeaponDamage(player.HeldItem);
                float knockback = player.GetWeaponKnockback(player.HeldItem);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    dropVel,
                    ModContent.ProjectileType<Rainfire_bolt>(),
                    damage,
                    knockback,
                    player.whoAmI
                );

                SoundEngine.PlaySound(SoundID.Item45 with { Volume = 0.4f, Pitch = 0.2f }, spawnPos);
            }

            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
