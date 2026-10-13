using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
namespace jarkos.tiles
{
    public class SporeFlowerTileEntity : ModTileEntity
    {
        public int SporeTimer = 0;
        public bool IsAnimating = false;
        public int AnimTimer = 0;
        public int HeadFrame = 0;
        public float ScaleY = 1f;
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SporeFlowerTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            int topLeftX = i - 1;
            int topLeftY = j - 3;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 3, 4);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeftX, topLeftY, Type);
                return -1;
            }
            return Place(topLeftX, topLeftY);
        }
        public override void Update()
        {
            if (!IsAnimating)
            {
                SporeTimer--;
                if (SporeTimer <= 0)
                {
                    IsAnimating = true;
                    AnimTimer = 0;
                    HeadFrame = 1;
                    SporeTimer = Main.rand.Next(900, 1500 + 1);
                }
            }
            else
            {
                AnimTimer++;
                int squashTime = 20;
                int sprayTime = 30;
                int unsquashTime = 20;
                if (AnimTimer <= squashTime)
                {
                    float t = AnimTimer / (float)squashTime;
                    ScaleY = MathHelper.Lerp(1f, 0.5f, EaseOutElastic(t));
                    if (AnimTimer > squashTime / 2) HeadFrame = 2;
                }
                else if (AnimTimer <= squashTime + sprayTime)
                {
                    ScaleY = 0.5f;
                    int sprayTimer = AnimTimer - squashTime;
                    if (sprayTimer == sprayTime / 4) HeadFrame = 3;
                    if (sprayTimer == sprayTime * 3 / 4) HeadFrame = 4;
                    if (Main.netMode != NetmodeID.Server && sprayTimer % 5 == 0)
                    {
                        Vector2 topCenter = new Vector2(Position.X * 16 + 24, Position.Y * 16 + 12 + (46 * 0.5f));
                        float angle = MathHelper.Pi + Main.rand.NextFloat(0, MathHelper.Pi);
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                        int dust = Dust.NewDust(topCenter, 0, 0, DustID.CursedTorch, velocity.X, velocity.Y, 100, default, 1.5f);
                        Main.dust[dust].noGravity = true;
                        if (Main.rand.NextBool(3))
                        {
                            Vector2 cloudVel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f);
                            int gore = Gore.NewGore(new EntitySource_TileEntity(this), topCenter, cloudVel, Main.rand.Next(11, 14));
                        }
                    }
                }
                else if (AnimTimer <= squashTime + sprayTime + unsquashTime)
                {
                    int unsquashTimer = AnimTimer - (squashTime + sprayTime);
                    float t = unsquashTimer / (float)unsquashTime;
                    ScaleY = MathHelper.Lerp(0.5f, 1f, EaseOutElastic(t));
                    if (unsquashTimer > unsquashTime / 2) HeadFrame = 0;
                }
                else
                {
                    IsAnimating = false;
                    ScaleY = 1f;
                    HeadFrame = 0;
                    AnimTimer = 0;
                }
            }
        }
        private float EaseOutElastic(float t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            return (float)(Math.Pow(2, -10 * t) * Math.Sin((t * 10f - 0.75f) * ((2f * Math.PI) / 3f)) + 1);
        }
    }
}
