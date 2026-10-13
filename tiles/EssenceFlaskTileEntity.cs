using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using jarkos.core.Data;
using Jarkos.tiles;
using Terraria.ID;
using jarkos.items.usetiles;
using System.IO;

namespace jarkos.tiles
{
    public class EssenceFlaskTileEntity : ModTileEntity
    {
        public EssenceType storedType = (EssenceType)(-1);
        public int storedAmount = 0;
        public const int MaxCapacity = 1000;
        private int _timer = 0;
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<EssenceFlaskTile>();
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
                return -1;
            }
            int id = Place(i, j);
            if (id != -1 && ByID.TryGetValue(id, out var te) && te is EssenceFlaskTileEntity flask)
            {
                Item item = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
                if (item.ModItem is EssenceFlaskItem flaskItem)
                {
                    flask.storedType = flaskItem.StoredType;
                    flask.storedAmount = flaskItem.StoredAmount;
                }
            }
            return id;
        }
        public override void Update()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (storedAmount >= MaxCapacity) return;
            _timer++;
            if (_timer >= 12)
            {
                _timer = 0;
                bool changed = false;
                foreach (var kvp in ByID)
                {
                    if (kvp.Value is DistillerTileEntity dte)
                    {
                        float distSq = Vector2.DistanceSquared(new Vector2(Position.X, Position.Y), new Vector2(dte.Position.X, dte.Position.Y));
                        if (distSq <= 5f * 5f)
                        {
                            if ((int)storedType == -1)
                            {
                                var availableTypes = dte.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
                                if (availableTypes.Count > 0)
                                {
                                    storedType = availableTypes[Main.rand.Next(availableTypes.Count)];
                                    changed = true;
                                }
                            }
                            if ((int)storedType != -1)
                            {
                                if (dte.StoredEssences.TryGetValue(storedType, out int amountInDistiller) && amountInDistiller > 0)
                                {
                                    if (storedAmount < MaxCapacity)
                                    {
                                        dte.StoredEssences[storedType]--;
                                        storedAmount++;
                                        changed = true;
                                        if (Main.netMode == NetmodeID.Server) dte.SyncData();
                                    }
                                }
                            }
                        }
                    }
                    else if (kvp.Value is AlchemicalEnchantmentTE ate)
                    {
                        if (!ate.PullEssenceMode)
                        {
                            float distSq = Vector2.DistanceSquared(new Vector2(Position.X, Position.Y), new Vector2(ate.Position.X, ate.Position.Y));
                            if (distSq <= 5f * 5f)
                            {
                                if ((int)storedType == -1)
                                {
                                    var availableTypes = ate.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
                                    if (availableTypes.Count > 0)
                                    {
                                        storedType = availableTypes[Main.rand.Next(availableTypes.Count)];
                                        changed = true;
                                    }
                                }
                                if ((int)storedType != -1)
                                {
                                    if (ate.StoredEssences.TryGetValue(storedType, out int amountInTable) && amountInTable > 0)
                                    {
                                        if (storedAmount < MaxCapacity)
                                        {
                                            ate.StoredEssences[storedType]--;
                                            storedAmount++;
                                            changed = true;

                                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ate.ID, ate.Position.X, ate.Position.Y);

                                            for (int i = 0; i < 3; i++)
                                            {
                                                Vector2 startPos = new Vector2(ate.Position.X * 16 + 24, ate.Position.Y * 16 + 16);
                                                Vector2 targetPos = new Vector2(Position.X * 16 + 8, Position.Y * 16 + 8);
                                                int dust = Dust.NewDust(startPos, 16, 16, DustID.MagicMirror, 0f, 0f, 100, default, 1.2f);
                                                Main.dust[dust].noGravity = true;
                                                Vector2 dir = targetPos - startPos;
                                                dir.Normalize();
                                                Main.dust[dust].velocity = dir * Main.rand.NextFloat(2f, 4f);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (changed && Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                }
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag["storedType"] = (int)storedType;
            tag["storedAmount"] = storedAmount;
        }
        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("storedType"))
                storedType = (EssenceType)tag.GetInt("storedType");
            if (tag.ContainsKey("storedAmount"))
                storedAmount = tag.GetInt("storedAmount");
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((int)storedType);
            writer.Write(storedAmount);
        }
        public override void NetReceive(BinaryReader reader)
        {
            storedType = (EssenceType)reader.ReadInt32();
            storedAmount = reader.ReadInt32();
        }
    }
}
