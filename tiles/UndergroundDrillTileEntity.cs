using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.tiles
{
    public class UndergroundDrillTileEntity : ModTileEntity
    {
        public Item[] OutputSlots = new Item[5];
        public Item[] FuelSlots = new Item[3];

        public bool IsActive = true;
        public bool IsFull = false;
        public float MineTimer = 0f;
        public const float BaseMineTime = 2100f;
        private int fuelConsumeTimer = 0;

        public UndergroundDrillTileEntity()
        {
            for (int i = 0; i < OutputSlots.Length; i++)
            {
                OutputSlots[i] = new Item();
            }
            for (int i = 0; i < FuelSlots.Length; i++)
            {
                FuelSlots[i] = new Item();
            }
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<UndergroundDrillTile>();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            int topLeftX = i - 2;
            int topLeftY = j - 4;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 5, 5);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeftX, topLeftY, Type);
                return -1;
            }
            return Place(topLeftX, topLeftY);
        }

        public float GetSpeedMultiplier()
        {
            float mult = 1.0f;
            for (int i = 0; i < FuelSlots.Length; i++)
            {
                Item item = FuelSlots[i];
                if (item != null && !item.IsAir && item.stack > 0)
                {
                    if (item.type == ModContent.ItemType<items.materials.CoolingFuel>())
                    {
                        mult += 0.20f;
                    }
                    else if (item.type == ModContent.ItemType<items.materials.SpectralFuel>())
                    {
                        mult += 0.70f;
                    }
                }
            }
            return mult;
        }

        public override void Update()
        {
            if (!IsActive)
            {
                return;
            }


            if (Main.netMode != NetmodeID.Server)
            {
                SpawnDrillSparks();
            }

            float speedMult = GetSpeedMultiplier();


            if (speedMult > 1.0f)
            {
                fuelConsumeTimer++;
                if (fuelConsumeTimer >= 1800)
                {
                    fuelConsumeTimer = 0;
                    bool consumedAny = false;
                    for (int i = 0; i < FuelSlots.Length; i++)
                    {
                        if (FuelSlots[i] != null && !FuelSlots[i].IsAir && FuelSlots[i].stack > 0)
                        {
                            FuelSlots[i].stack--;
                            if (FuelSlots[i].stack <= 0)
                            {
                                FuelSlots[i].TurnToAir();
                            }
                            consumedAny = true;
                        }
                    }
                    if (consumedAny)
                    {
                        SyncData();
                    }
                }
            }


            MineTimer += speedMult;
            if (MineTimer >= BaseMineTime)
            {
                MineTimer = 0f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    PerformMiningCycle();
                    SyncData();
                }
            }
        }

        private void PerformMiningCycle()
        {

            int bottomY = Position.Y + 5;
            List<int> oresToMine = new List<int>();

            for (int x = 0; x < 5; x++)
            {
                int checkX = Position.X + x;
                Tile tile = Main.tile[checkX, bottomY];
                if (tile.HasTile)
                {
                    int oreItemId = GetOreItemType(tile);
                    if (oreItemId > 0)
                    {
                        oresToMine.Add(oreItemId);
                    }
                }
            }

            if (oresToMine.Count == 0)
            {
                IsFull = false;
                return;
            }


            bool anyFailed = false;
            foreach (int oreId in oresToMine)
            {
                bool added = AddOreToOutputSlots(oreId);
                if (!added)
                {
                    anyFailed = true;
                }
            }

            IsFull = anyFailed;
        }

        private bool AddOreToOutputSlots(int oreItemId)
        {

            for (int i = 0; i < OutputSlots.Length; i++)
            {
                if (OutputSlots[i] != null && !OutputSlots[i].IsAir && OutputSlots[i].type == oreItemId)
                {
                    if (OutputSlots[i].stack < OutputSlots[i].maxStack)
                    {
                        OutputSlots[i].stack++;
                        return true;
                    }
                }
            }


            for (int i = 0; i < OutputSlots.Length; i++)
            {
                if (OutputSlots[i] == null || OutputSlots[i].IsAir)
                {
                    OutputSlots[i] = new Item(oreItemId, 1);
                    return true;
                }
            }

            return false;
        }

        public static int GetOreItemType(Tile tile)
        {
            ushort type = tile.TileType;

            if (type == TileID.Copper) return ItemID.CopperOre;
            if (type == TileID.Tin) return ItemID.TinOre;
            if (type == TileID.Iron) return ItemID.IronOre;
            if (type == TileID.Lead) return ItemID.LeadOre;
            if (type == TileID.Silver) return ItemID.SilverOre;
            if (type == TileID.Tungsten) return ItemID.TungstenOre;
            if (type == TileID.Gold) return ItemID.GoldOre;
            if (type == TileID.Platinum) return ItemID.PlatinumOre;
            if (type == TileID.Demonite) return ItemID.DemoniteOre;
            if (type == TileID.Crimtane) return ItemID.CrimtaneOre;
            if (type == TileID.Meteorite) return ItemID.Meteorite;
            if (type == TileID.Obsidian) return ItemID.Obsidian;
            if (type == TileID.Hellstone) return ItemID.Hellstone;
            if (type == TileID.Cobalt) return ItemID.CobaltOre;
            if (type == TileID.Palladium) return ItemID.PalladiumOre;
            if (type == TileID.Mythril) return ItemID.MythrilOre;
            if (type == TileID.Orichalcum) return ItemID.OrichalcumOre;
            if (type == TileID.Adamantite) return ItemID.AdamantiteOre;
            if (type == TileID.Titanium) return ItemID.TitaniumOre;
            if (type == TileID.Chlorophyte) return ItemID.ChlorophyteOre;
            if (type == TileID.LunarOre) return ItemID.LunarOre;

            if (TileID.Sets.Ore[type] || Main.tileSpelunker[type])
            {
                int drop = TileLoader.GetItemDropFromTypeAndStyle(type);
                if (drop > 0) return drop;
            }

            return 0;
        }

        private void SpawnDrillSparks()
        {
            if (Main.rand.NextBool(2))
            {

                Vector2 spawnPos = new Vector2((Position.X + 2.5f) * 16f + Main.rand.NextFloat(-12f, 12f), (Position.Y + 4.8f) * 16f);


                int sampleX = Position.X + Main.rand.Next(5);
                int sampleY = Position.Y + 5;
                Tile tile = Main.tile[sampleX, sampleY];

                int dustType = GetDustForTile(tile, sampleX, sampleY);


                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.7f, 0.7f), -Main.rand.NextFloat(3.2f, 6.2f));
                Dust d = Dust.NewDustPerfect(spawnPos, dustType, vel, 110, default, Main.rand.NextFloat(0.9f, 1.4f));
                d.noGravity = true;
                d.velocity = vel;
            }
        }

        public static int GetDustForTile(Tile tile, int x, int y)
        {
            if (!tile.HasTile) return DustID.Smoke;

            ushort type = tile.TileType;

            if (type == TileID.Copper) return DustID.Copper;
            if (type == TileID.Tin) return DustID.Tin;
            if (type == TileID.Iron) return DustID.Iron;
            if (type == TileID.Lead) return DustID.Lead;
            if (type == TileID.Silver) return DustID.Silver;
            if (type == TileID.Tungsten) return DustID.Tungsten;
            if (type == TileID.Gold) return DustID.Gold;
            if (type == TileID.Platinum) return DustID.Platinum;
            if (type == TileID.Demonite) return DustID.Demonite;
            if (type == TileID.Crimtane) return DustID.Crimstone;
            if (type == TileID.Meteorite) return DustID.Meteorite;
            if (type == TileID.Obsidian) return DustID.Obsidian;
            if (type == TileID.Hellstone) return DustID.Torch;
            if (type == TileID.Cobalt) return DustID.Cobalt;
            if (type == TileID.Palladium) return DustID.Palladium;
            if (type == TileID.Mythril) return DustID.Mythril;
            if (type == TileID.Orichalcum) return DustID.Orichalcum;
            if (type == TileID.Adamantite) return DustID.Adamantite;
            if (type == TileID.Titanium) return DustID.Titanium;
            if (type == TileID.Chlorophyte) return DustID.Chlorophyte;
            if (type == TileID.LunarOre) return DustID.Vortex;

            ModTile modTile = TileLoader.GetTile(type);
            if (modTile != null && modTile.DustType >= 0)
            {
                return modTile.DustType;
            }

            return DustID.Stone;
        }

        public bool TakeAllOresToPlayer(Player player)
        {
            bool movedAny = false;
            for (int i = 0; i < OutputSlots.Length; i++)
            {
                if (OutputSlots[i] != null && !OutputSlots[i].IsAir)
                {
                    Item item = OutputSlots[i].Clone();
                    OutputSlots[i].TurnToAir();
                    player.QuickSpawnItem(player.GetSource_FromThis(), item, item.stack);
                    movedAny = true;
                }
            }

            if (movedAny)
            {
                IsFull = false;
                SyncData();
            }
            return movedAny;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["isActive"] = IsActive;
            tag["mineTimer"] = MineTimer;
            tag["isFull"] = IsFull;

            for (int i = 0; i < OutputSlots.Length; i++)
            {
                tag[$"output{i}"] = ItemIO.Save(OutputSlots[i]);
            }
            for (int i = 0; i < FuelSlots.Length; i++)
            {
                tag[$"fuel{i}"] = ItemIO.Save(FuelSlots[i]);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("isActive")) IsActive = tag.GetBool("isActive");
            if (tag.ContainsKey("mineTimer")) MineTimer = tag.GetFloat("mineTimer");
            if (tag.ContainsKey("isFull")) IsFull = tag.GetBool("isFull");

            for (int i = 0; i < OutputSlots.Length; i++)
            {
                if (tag.ContainsKey($"output{i}")) OutputSlots[i] = ItemIO.Load(tag.GetCompound($"output{i}"));
            }
            for (int i = 0; i < FuelSlots.Length; i++)
            {
                if (tag.ContainsKey($"fuel{i}")) FuelSlots[i] = ItemIO.Load(tag.GetCompound($"fuel{i}"));
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(IsActive);
            writer.Write(IsFull);
            writer.Write(MineTimer);

            for (int i = 0; i < OutputSlots.Length; i++)
            {
                ItemIO.Send(OutputSlots[i], writer, true);
            }
            for (int i = 0; i < FuelSlots.Length; i++)
            {
                ItemIO.Send(FuelSlots[i], writer, true);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            IsActive = reader.ReadBoolean();
            IsFull = reader.ReadBoolean();
            MineTimer = reader.ReadSingle();

            for (int i = 0; i < OutputSlots.Length; i++)
            {
                OutputSlots[i] = ItemIO.Receive(reader, true);
            }
            for (int i = 0; i < FuelSlots.Length; i++)
            {
                FuelSlots[i] = ItemIO.Receive(reader, true);
            }
        }

        public void SyncData()
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }
    }
}
