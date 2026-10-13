using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.mounts
{
    public class Rhinobeetle_mount : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.buff = ModContent.BuffType<buffs.Rhinobeetle_mount_buff>();
            MountData.heightBoost = 20;
            MountData.fallDamage = 0f;
            MountData.runSpeed = 16f;
            MountData.dashSpeed = 16f;
            MountData.flightTimeMax = 0;
            MountData.fatigueMax = 0;
            MountData.jumpHeight = 25;
            MountData.acceleration = 0.2f;
            MountData.jumpSpeed = 10f;
            MountData.blockExtraJumps = false;
            MountData.totalFrames = 1;
            int[] array = new int[MountData.totalFrames];
            for (int l = 0; l < array.Length; l++)
            {
                array[l] = 20;
            }
            MountData.playerYOffsets = array;
            MountData.xOffset = 10;
            MountData.yOffset = -5;
            MountData.playerHeadOffset = 22;
            MountData.bodyFrame = 3;
            MountData.standingFrameCount = 1;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            MountData.runningFrameCount = 1;
            MountData.runningFrameDelay = 12;
            MountData.runningFrameStart = 0;
            MountData.flyingFrameCount = 0;
            MountData.flyingFrameDelay = 0;
            MountData.flyingFrameStart = 0;
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 12;
            MountData.inAirFrameStart = 0;
            MountData.idleFrameCount = 1;
            MountData.idleFrameDelay = 12;
            MountData.idleFrameStart = 0;
            MountData.idleFrameLoop = true;
            MountData.swimSpeed = 5f;
            if (!Main.dedServ)
            {
                MountData.backTexture = ModContent.Request<Texture2D>("jarkos/mounts/Rhinobeetle_mount");
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }
        }
    }
    public class RhinobeetleMountPlayer : ModPlayer
    {
        public bool BounceModeActive { get; private set; }
        public override void PreUpdate()
        {
            if (Player.mount.Active && Player.mount.Type == ModContent.MountType<Rhinobeetle_mount>())
            {
                if (Player.controlUseTile && !Player.mouseInterface && !Main.playerInventory)
                {
                    BounceModeActive = true;
                }
                else
                {
                    BounceModeActive = false;
                }
                if (!BounceModeActive && Math.Abs(Player.velocity.X) > 10f)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke);
                    }
                    Rectangle playerRect = Player.getRect();
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(playerRect))
                        {
                            if (npc.immune[Player.whoAmI] == 0)
                            {
                                int damage = (int)(80 * Player.GetDamage(DamageClass.Generic).Multiplicative);
                                npc.SimpleStrikeNPC(damage, Player.direction, false, 8f);
                                npc.immune[Player.whoAmI] = 20;
                                SoundEngine.PlaySound(SoundID.Item14, npc.position);
                            }
                        }
                    }
                }
                if (BounceModeActive)
                {
                    Player.maxFallSpeed = 25f;
                    if (Player.velocity.Y > 0)
                    {
                        Player.velocity.Y += 0.5f;
                    }
                }
            }
            else
            {
                BounceModeActive = false;
            }
        }
        public override void PostUpdate()
        {
            if (Player.mount.Active && Player.mount.Type == ModContent.MountType<Rhinobeetle_mount>())
            {
                if (BounceModeActive)
                {
                    Player.fullRotation = MathHelper.Pi;
                    Player.fullRotationOrigin = new Vector2(Player.width / 2f, Player.height / 2f);
                    if (Math.Abs(Player.velocity.Y) < 0.1f && Player.oldVelocity.Y > 0.5f)
                    {
                        Player.velocity.Y = -Player.oldVelocity.Y * 1.5f;
                        if (Player.velocity.Y > -10f) Player.velocity.Y = -10f;
                        if (Player.velocity.Y < -25f) Player.velocity.Y = -25f;
                        SoundEngine.PlaySound(SoundID.Item14, Player.position);
                        Terraria.Graphics.CameraModifiers.PunchCameraModifier modifier = new Terraria.Graphics.CameraModifiers.PunchCameraModifier(Player.Center, new Vector2(0, 1), 6f, 4f, 15, 1000f, "RhinobeetleMount");
                        Main.instance.CameraModifiers.Add(modifier);
                    }
                    if (Math.Abs(Player.velocity.Y) < 0.1f && Player.oldVelocity.Y < -0.5f)
                    {
                        Player.velocity.Y = -Player.oldVelocity.Y * 1.2f;
                        SoundEngine.PlaySound(SoundID.Item14, Player.position);
                    }
                    if (Math.Abs(Player.velocity.X) < 0.1f && Math.Abs(Player.oldVelocity.X) > 3f)
                    {
                        Player.velocity.X = -Player.oldVelocity.X * 1.2f;
                        SoundEngine.PlaySound(SoundID.Item14, Player.position);
                    }
                }
                else
                {
                    if (Player.fullRotation == MathHelper.Pi)
                    {
                        Player.fullRotation = 0f;
                    }
                }
            }
            else
            {
                if (Player.fullRotation == MathHelper.Pi)
                {
                    Player.fullRotation = 0f;
                }
            }
        }
    }
}
