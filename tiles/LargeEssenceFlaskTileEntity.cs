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
    public class LargeEssenceFlaskTileEntity : ModTileEntity
    {
        public Dictionary<EssenceType, int> StoredEssences = new Dictionary<EssenceType, int>();
        public const int MaxCapacity = 10000;
        public const int MaxTypes = 5;
        private int _timer = 0;
        public int TotalEssences => StoredEssences.Values.Sum();
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<LargeEssenceFlaskTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            int topLeftX = i - 1;
            int topLeftY = j - 2;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 3, 3);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeftX, topLeftY, Type);
                return -1;
            }
            int id = Place(topLeftX, topLeftY);
            if (id != -1 && ByID.TryGetValue(id, out var te) && te is LargeEssenceFlaskTileEntity flask)
            {
                Item item = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
                if (item.ModItem is LargeEssenceFlaskItem flaskItem)
                {
                    foreach (var kvp in flaskItem.StoredEssences)
                    {
                        flask.StoredEssences[kvp.Key] = kvp.Value;
                    }
                }
            }
            return id;
        }
        public override void Update()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (TotalEssences >= MaxCapacity) return;
            _timer++;
            if (_timer >= 12)
            {
                _timer = 0;
                bool changed = false;
                Vector2 myPos = new Vector2(Position.X * 16 + 24, Position.Y * 16 + 24);

                foreach (var kvp in ByID)
                {
                    if (kvp.Value is DistillerTileEntity dte)
                    {
                        Vector2 distPos = new Vector2(dte.Position.X * 16 + 24, dte.Position.Y * 16 + 24);
                        float distSq = Vector2.DistanceSquared(myPos, distPos);

                        if (distSq <= 6f * 16f * 6f * 16f)
                        {
                            var availableTypes = dte.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
                            availableTypes.RemoveAll(t => !StoredEssences.ContainsKey(t) && StoredEssences.Count >= MaxTypes);
                            if (availableTypes.Count > 0)
                            {
                                EssenceType typeToSuck = availableTypes[Main.rand.Next(availableTypes.Count)];
                                if (!StoredEssences.ContainsKey(typeToSuck)) StoredEssences[typeToSuck] = 0;
                                if (TotalEssences < MaxCapacity)
                                {
                                    dte.StoredEssences[typeToSuck]--;
                                    StoredEssences[typeToSuck]++;
                                    changed = true;
                                    SpawnTransferParticles(distPos, myPos, typeToSuck);
                                    if (Main.netMode == NetmodeID.Server) dte.SyncData();
                                }
                            }
                        }
                    }
                    else if (kvp.Value is AlchemicalEnchantmentTE ate)
                    {
                        if (!ate.PullEssenceMode)
                        {
                            Vector2 atePos = new Vector2(ate.Position.X * 16 + 16, ate.Position.Y * 16 + 16);
                            float distSq = Vector2.DistanceSquared(myPos, atePos);
                            if (distSq <= 6f * 16f * 6f * 16f)
                            {
                                var availableTypes = ate.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
                                availableTypes.RemoveAll(t => !StoredEssences.ContainsKey(t) && StoredEssences.Count >= MaxTypes);
                                if (availableTypes.Count > 0)
                                {
                                    EssenceType typeToSuck = availableTypes[Main.rand.Next(availableTypes.Count)];
                                    if (!StoredEssences.ContainsKey(typeToSuck)) StoredEssences[typeToSuck] = 0;
                                    if (TotalEssences < MaxCapacity)
                                    {
                                        ate.StoredEssences[typeToSuck]--;
                                        StoredEssences[typeToSuck]++;
                                        changed = true;
                                        SpawnTransferParticles(atePos, myPos, typeToSuck);
                                        if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ate.ID, ate.Position.X, ate.Position.Y);
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
        private void SpawnTransferParticles(Vector2 startPos, Vector2 targetPos, EssenceType type)
        {
            if (Main.netMode == NetmodeID.Server) return;
            Color color = EssencePalette.ColorOf(type);
            for (int i = 0; i < 3; i++)
            {
                int dust = Dust.NewDust(startPos, 16, 16, DustID.MagicMirror, 0f, 0f, 100, color, 1.2f);
                Main.dust[dust].noGravity = true;
                Vector2 dir = targetPos - startPos;
                dir.Normalize();
                Main.dust[dust].velocity = dir * Main.rand.NextFloat(2f, 4f);
                Main.dust[dust].color = color;
            }
        }
        public override void SaveData(TagCompound tag)
        {
            var keys = StoredEssences.Keys.Select(k => (int)k).ToList();
            var vals = StoredEssences.Values.ToList();
            tag["essKeys"] = keys;
            tag["essVals"] = vals;
        }
        public override void LoadData(TagCompound tag)
        {
            StoredEssences.Clear();
            if (tag.ContainsKey("essKeys"))
            {
                var keys = tag.GetList<int>("essKeys");
                var vals = tag.GetList<int>("essVals");
                for (int i = 0; i < keys.Count && i < vals.Count; i++)
                    StoredEssences[(EssenceType)keys[i]] = vals[i];
            }
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(StoredEssences.Count);
            foreach (var kvp in StoredEssences)
            {
                writer.Write((int)kvp.Key);
                writer.Write(kvp.Value);
            }
        }
        public override void NetReceive(BinaryReader reader)
        {
            StoredEssences.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                EssenceType type = (EssenceType)reader.ReadInt32();
                int amount = reader.ReadInt32();
                StoredEssences[type] = amount;
            }
        }
    }
}
