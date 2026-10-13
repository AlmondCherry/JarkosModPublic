using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.GameplayChanges
{
    public class UniversalHerbGlobalTile : GlobalTile
    {
        
        private const ushort HerbSprout = 82;
        private const ushort HerbMature = 83;
        private const ushort HerbBlooming = 84;

        public override void RandomUpdate(int i, int j, int type)
        {
            if (Jarkos_config.Instance == null || !Jarkos_config.Instance.AlwaysGrowHerbs)
                return;

            if (type == HerbSprout || type == HerbMature)
            {
                Tile tile = Main.tile[i, j];
                if (tile.HasTile)
                {

                    tile.TileType = HerbBlooming;
                    WorldGen.SquareTileFrame(i, j);
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, i, j, 1);
                    }
                }
            }
        }

        public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
        {
            if (Jarkos_config.Instance == null || !Jarkos_config.Instance.AlwaysGrowHerbs)
                return true;

            if (type == HerbSprout || type == HerbMature || type == HerbBlooming)
            {
                Tile tileBelow = Framing.GetTileSafely(i, j + 1);
                if (tileBelow.HasTile && (Main.tileSolid[tileBelow.TileType] || Main.tileSolidTop[tileBelow.TileType]))
                {
                    noBreak = true;
                }


                Tile tile = Main.tile[i, j];
                if (tile.HasTile && tile.TileType == HerbMature)
                {
                    tile.TileType = HerbBlooming;
                }
            }
            return true;
        }
    }

    public class UniversalHerbGlobalItem : GlobalItem
    {
        private const ushort HerbSprout = 82;

        public override bool? UseItem(Item item, Player player)
        {
            if (Jarkos_config.Instance != null && Jarkos_config.Instance.AlwaysGrowHerbs && item.createTile == TileID.Plants)
            {
                int targetX = Player.tileTargetX;
                int targetY = Player.tileTargetY;
                Tile targetTile = Framing.GetTileSafely(targetX, targetY);
                Tile tileBelow = Framing.GetTileSafely(targetX, targetY + 1);

                if (!targetTile.HasTile && tileBelow.HasTile && (Main.tileSolid[tileBelow.TileType] || Main.tileSolidTop[tileBelow.TileType]))
                {

                    if (WorldGen.PlaceTile(targetX, targetY, HerbSprout, mute: false, forced: true, style: item.placeStyle))
                    {
                        SoundEngine.PlaySound(SoundID.Dig, new Vector2(targetX * 16, targetY * 16));
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendTileSquare(-1, targetX, targetY, 1);
                        }
                        return true;
                    }
                }
            }
            return null;
        }
    }

    public class UVlampHerbGrowthSystem : ModSystem
    {
        private const ushort HerbSprout = 82;
        private const ushort HerbMature = 83;
        private const ushort HerbBlooming = 84;

        public override void PostUpdateWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;


            foreach (var kvp in TileEntity.ByID)
            {
                if (kvp.Value is jarkos.tiles.UVlampTileEntity lamp)
                {
                    int lampCenterX = lamp.Position.X + 1;
                    int lampCenterY = lamp.Position.Y + 1;


                    for (int n = 0; n < 3; n++)
                    {
                        int rx = lampCenterX + Main.rand.Next(-19, 20);
                        int ry = lampCenterY + Main.rand.Next(-19, 20);

                        if (rx < 10 || rx >= Main.maxTilesX - 10 || ry < 10 || ry >= Main.maxTilesY - 10) continue;

                        Vector2 tileWorldPos = new Vector2(rx * 16 + 8, ry * 16 + 8);
                        Vector2 lampWorldCenter = new Vector2(lamp.Position.X * 16 + 24, lamp.Position.Y * 16 + 24);

                        if (Vector2.Distance(tileWorldPos, lampWorldCenter) <= 300f)
                        {
                            Tile tile = Main.tile[rx, ry];
                            if (tile.HasTile && (tile.TileType == HerbSprout || tile.TileType == HerbMature))
                            {
                                tile.TileType = HerbBlooming;
                                WorldGen.SquareTileFrame(rx, ry);
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    NetMessage.SendTileSquare(-1, rx, ry, 1);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
