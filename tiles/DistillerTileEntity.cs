using System.Collections.Generic;
using System.IO;
using System.Linq;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.tiles
{
    public class DistillerTileEntity : ModTileEntity
    {
        public Dictionary<EssenceType, int> StoredEssences = new Dictionary<EssenceType, int>();
        public const int MaxCapacity = 100;

        public Item[] InputSlots = new Item[2];
        public int[] ProcessTimers = new int[2];
        public bool IgnoreLaser = false;

        public DistillerTileEntity()
        {
            for (int i = 0; i < InputSlots.Length; i++) InputSlots[i] = new Item();
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<DistillerTile>();
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            int topLeftX = i;
            int topLeftY = j - 3;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 4);
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.PlaceDistiller);
                packet.Write((short)topLeftX);
                packet.Write((short)topLeftY);
                packet.Send();
                return -1;
            }
            return Place(topLeftX, topLeftY);
        }
        public int GetTotalEssences()
        {
            return StoredEssences.Values.Sum();
        }
        public void AddEssence(EssenceType type, int amount)
        {
            if (GetTotalEssences() >= MaxCapacity) return;

            int spaceLeft = MaxCapacity - GetTotalEssences();
            int toAdd = amount > spaceLeft ? spaceLeft : amount;

            if (StoredEssences.ContainsKey(type))
                StoredEssences[type] += toAdd;
            else
                StoredEssences[type] = toAdd;
        }
        public void ClearEssences()
        {
            StoredEssences.Clear();
        }
        public override void Update()
        {
            int reqTime = 300;
            bool secondSlotUnlocked = false;
            int closestPlayerIndex = Player.FindClosest(new Vector2(Position.X * 16, Position.Y * 16), 480, 480);

            if (closestPlayerIndex >= 0 && closestPlayerIndex < Main.maxPlayers)
            {
                Player closest = Main.player[closestPlayerIndex];
                if (closest != null && closest.active)
                {
                    var rp = closest.GetModPlayer<ResearchPlayer>();
                    if (rp.IsCompleted("jarkos:AquaNode6"))
                    {
                        reqTime = 195;
                    }
                    if (rp.IsCompleted("jarkos:AquaNode7"))
                    {
                        secondSlotUnlocked = true;
                    }
                }
            }
            
            bool changed = false;
            for (int i = 0; i < InputSlots.Length; i++)
            {
                if (i == 1 && !secondSlotUnlocked)
                {
                    ProcessTimers[i] = 0;
                    continue;
                }

                if (InputSlots[i] != null && !InputSlots[i].IsAir)
                {
                    if (GetTotalEssences() >= MaxCapacity) continue;
                    ProcessTimers[i]++;

                    if (ProcessTimers[i] >= reqTime)
                    {
                        ProcessTimers[i] = 0;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            List<EssenceType> types = GetEssencesForItem(InputSlots[i]);
                            InputSlots[i].stack--;

                            if (InputSlots[i].stack <= 0) InputSlots[i].TurnToAir();

                            foreach (var type in types)
                                AddEssence(type, 1);

                            changed = true;
                        }
                    }
                }
                else
                {
                    ProcessTimers[i] = 0;
                }
            }
            if (changed && Main.netMode == NetmodeID.Server)
            {
                SyncData();
            }
        }
        public override void SaveData(TagCompound tag)
        {
            var keys = StoredEssences.Keys.Select(k => (int)k).ToList();
            var vals = StoredEssences.Values.ToList();

            tag["keys"] = keys;
            tag["vals"] = vals;

            tag["slot0"] = ItemIO.Save(InputSlots[0]);
            tag["slot1"] = ItemIO.Save(InputSlots[1]);

            tag["ignoreLaser"] = IgnoreLaser;
        }
        public override void LoadData(TagCompound tag)
        {
            StoredEssences.Clear();

            if (tag.ContainsKey("keys"))
            {
                var keys = tag.GetList<int>("keys");
                var vals = tag.GetList<int>("vals");

                for (int i = 0; i < keys.Count && i < vals.Count; i++)
                {
                    StoredEssences[(EssenceType)keys[i]] = vals[i];
                }
            }

            InputSlots[0] = tag.ContainsKey("slot0") ? ItemIO.Load(tag.GetCompound("slot0")) : new Item();
            InputSlots[1] = tag.ContainsKey("slot1") ? ItemIO.Load(tag.GetCompound("slot1")) : new Item();

            if (tag.ContainsKey("ignoreLaser")) IgnoreLaser = tag.GetBool("ignoreLaser");
        }
        public override void NetSend(BinaryWriter writer)
        {
            InputSlots[0] ??= new Item();
            InputSlots[1] ??= new Item();

            ItemIO.Send(InputSlots[0], writer, true);
            ItemIO.Send(InputSlots[1], writer, true);

            writer.Write(IgnoreLaser);
            writer.Write((ushort)ProcessTimers[0]);
            writer.Write((ushort)ProcessTimers[1]);
            writer.Write((byte)StoredEssences.Count);

            foreach (var kvp in StoredEssences)
            {
                writer.Write((byte)kvp.Key);
                writer.Write((ushort)kvp.Value);
            }
        }
        public void SyncData()
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.DistillerStateSync);
                packet.Write(Position.X);
                packet.Write(Position.Y);
                packet.Write(IgnoreLaser);
                packet.Write((ushort)ProcessTimers[0]);
                packet.Write((ushort)ProcessTimers[1]);
                packet.Write((byte)StoredEssences.Count);

                foreach (var kvp in StoredEssences)
                {
                    packet.Write((byte)kvp.Key);
                    packet.Write((ushort)kvp.Value);
                }

                bool has0 = InputSlots[0] != null && !InputSlots[0].IsAir;
                packet.Write(has0);
                if (has0) ItemIO.Send(InputSlots[0], packet, true);
                bool has1 = InputSlots[1] != null && !InputSlots[1].IsAir;
                packet.Write(has1);
                if (has1) ItemIO.Send(InputSlots[1], packet, true);
                packet.Send();
            }
        }
        public override void NetReceive(BinaryReader reader)
        {
            InputSlots[0] = ItemIO.Receive(reader, true);
            InputSlots[1] = ItemIO.Receive(reader, true);

            IgnoreLaser = reader.ReadBoolean();
            ProcessTimers[0] = reader.ReadUInt16();
            ProcessTimers[1] = reader.ReadUInt16();

            StoredEssences.Clear();

            byte count = reader.ReadByte();
            for (int i = 0; i < count; i++)
            {
                EssenceType key = (EssenceType)reader.ReadByte();
                int val = reader.ReadUInt16();
                StoredEssences[key] = val;
            }
        }
        public static List<EssenceType> GetEssencesForItem(Item item)
        {
            List<EssenceType> result = new List<EssenceType>();

            float roll = Main.rand.NextFloat();

            if (roll < 0.25f)
            {
                result.Add(EssenceType.Aer);
            }
            else if (roll < 0.50f)
            {
                result.Add(EssenceType.Aqua);
            }
            else if (roll < 0.75f)
            {
                result.Add(EssenceType.Terra);
            }
            else
            {
                result.Add(EssenceType.Vita);
            }
            return result;
        }
    }
}
