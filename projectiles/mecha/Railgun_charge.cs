using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;
using jarkos.particles;
using Jarkos.projectiles.mecha;
using jarkos;
using Jarkos.items.weapons.magic.toms;
using System;
namespace Jarkos.projectiles.mecha
{
    public class Railgun_charge : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<Railgun>())
            {
                Projectile.Kill();
                return;
            }
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            Vector2 aim;
            if (Main.myPlayer == Projectile.owner)
            {
                aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                if (Vector2.DistanceSquared(Projectile.velocity, aim) > 0.001f || (int)Projectile.ai[0] % 10 == 0)
                {
                    Projectile.velocity = aim;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                aim = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            }

            player.ChangeDir(aim.X > 0 ? 1 : -1);
            player.itemRotation = (float)Math.Atan2(aim.Y * player.direction, aim.X * player.direction);
            Projectile.rotation = aim.ToRotation() + (player.direction == 1 ? 0 : MathHelper.Pi);
            Vector2 muzzlePos = player.MountedCenter + aim * 115f;
            Projectile.Center = muzzlePos;

            if (Projectile.ai[0] == 0 && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.5f, Volume = 0.5f }, muzzlePos);
            }
            Projectile.ai[0]++;
            float maxCharge = 180f;
            if (player.channel)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2CircularEdge(80f, 80f);
                        Vector2 spawnPos = muzzlePos + offset;
                        new Stretching_particle(spawnPos, muzzlePos, Color.Cyan, 12, Main.rand.NextFloat(15f, 30f), Main.rand.NextFloat(2f, 4f), 0.05f).Spawn();
                    }
                }
                if (Projectile.ai[0] >= maxCharge)
                {
                    if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
                    {
                        Dust.NewDustPerfect(muzzlePos + Main.rand.NextVector2Circular(10f, 10f), DustID.Electric, Vector2.Zero, 0, default, 1.2f).noGravity = true;
                    }
                    if (Projectile.ai[0] % 5 == 0 && Main.myPlayer == Projectile.owner)
                    {
                        player.GetModPlayer<Screenshake_system>().regular_shake++;
                    }
                }
            }
            else
            {
                if (Projectile.ai[0] >= maxCharge)
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        SoundEngine.PlaySound(SoundID.Item157, muzzlePos);
                        SoundEngine.PlaySound(SoundID.Thunder with { Volume = 0.8f, Pitch = 0.2f }, muzzlePos);
                    }
                    if (Projectile.owner == Main.myPlayer)
                    {
                        player.velocity -= aim * 6f;
                        player.GetModPlayer<Screenshake_system>().regular_shake += 15;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, aim * 3f, ModContent.ProjectileType<Railgun_proj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                }
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
