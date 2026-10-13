using jarkos.items.usetiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using System.IO;
using jarkos.items.materials;
using Terraria.Enums;
using Jarkos.UI;
using Jarkos.items.materials;
using Jarkos;
using Terraria.GameContent.ObjectInteractions;
namespace Jarkos.tiles
{
	public class Uranium_charger_tile : ModTile
	{
		 public const int Width = 4;
        public const int Height = 3;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, Width, 0);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<EnergyConverterTE>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(200, 50, 50), name);
            DustType = DustID.Blood;
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
        public static Point16 GetTopLeft(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX / 18;
            int frameY = tile.TileFrameY / 18;
            return new Point16(i - frameX, j - frameY);
        }
        public override bool RightClick(int i, int j)
        {
            Point16 topLeft = GetTopLeft(i, j);
            EnergyConverterTE converterTE = null;
            if (TileEntity.ByPosition.TryGetValue(topLeft, out TileEntity te))
            {
                converterTE = te as EnergyConverterTE;
            }
            if (converterTE == null)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeft.X, topLeft.Y, ModContent.GetInstance<EnergyConverterTE>().Type);
                    return true;
                }
                int id = ModContent.GetInstance<EnergyConverterTE>().Place(topLeft.X, topLeft.Y);
                if (id != -1 && TileEntity.ByID.TryGetValue(id, out TileEntity newTE))
                {
                    converterTE = newTE as EnergyConverterTE;
                }
            }
            if (converterTE != null)
            {
                ModContent.GetInstance<EnergyConverterUISystem>().ToggleUI(converterTE);
            }
            return true;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int topLeftX = i - frameX / 18;
            int topLeftY = j - frameY / 18;
            ModContent.GetInstance<EnergyConverterUISystem>()?.CloseUI();
            Point16 tePos = new Point16(topLeftX, topLeftY);
            if (TileEntity.ByPosition.TryGetValue(tePos, out TileEntity te) && te is EnergyConverterTE converterTE)
            {
                converterTE.DropItems(topLeftX, topLeftY);
            }
            ModContent.GetInstance<EnergyConverterTE>().Kill(topLeftX, topLeftY);
        }
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<ChargerItem>();
        }
	}
	#region TileEntity
	public class EnergyConverterTE : ModTileEntity
    {
        public Item InputSlot;
        public Item OutputSlot;
        public int ConversionProgress = 0;
        public const int MaxProgress = 600;
        public EnergyConverterTE()
        {
            InputSlot = new Item();
            InputSlot.TurnToAir();
            OutputSlot = new Item();
            OutputSlot.TurnToAir();
        }
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<Uranium_charger_tile>();
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            TileObjectData data = TileObjectData.GetTileData(type, style);
            int width = data?.Width ?? Uranium_charger_tile.Width;
            int height = data?.Height ?? Uranium_charger_tile.Height;
            Point16 originOffset = data?.Origin ?? new Point16(1, 2);
            int left = i - originOffset.X;
            int top = j - originOffset.Y;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, left, top, width, height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, left, top, Type);
                return -1;
            }
            int id = Place(left, top);
            return id;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }
        private int syncTimer = 0;
        private const int SyncInterval = 10;
        public override void Update()
        {
            if (InputSlot == null)
            {
                InputSlot = new Item();
                InputSlot.TurnToAir();
            }
            if (OutputSlot == null)
            {
                OutputSlot = new Item();
                OutputSlot.TurnToAir();
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                bool hasValidInput = !InputSlot.IsAir && InputSlot.type == ModContent.ItemType<Battery>();
                if (hasValidInput && CanOutput())
                {
                    ConversionProgress++;
                    if (ConversionProgress >= MaxProgress)
                    {
                        CompleteConversion();
                    }
                }
                else
                {
                    if (ConversionProgress > 0)
                    {
                        ConversionProgress = 0;
                    }
                }
            }
            if (Main.netMode == NetmodeID.Server)
            {
                syncTimer++;
                if (syncTimer >= SyncInterval)
                {
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID);
                    syncTimer = 0;
                }
            }
        }
        private bool CanOutput()
        {
            if (OutputSlot == null || OutputSlot.IsAir)
                return true;
            if (OutputSlot.type == ModContent.ItemType<SyntheticBlood>() && OutputSlot.stack < OutputSlot.maxStack)
                return true;
            return false;
        }
        private void CompleteConversion()
        {
            InputSlot.stack--;
            if (InputSlot.stack <= 0)
            {
                InputSlot.TurnToAir();
            }
            if (OutputSlot == null || OutputSlot.IsAir)
            {
                OutputSlot = new Item();
                OutputSlot.SetDefaults(ModContent.ItemType<SyntheticBlood>());
                OutputSlot.stack = 1;
            }
            else
            {
                OutputSlot.stack++;
            }
            ConversionProgress = 0;
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }
        public override void SaveData(TagCompound tag)
        {
            if (InputSlot != null && !InputSlot.IsAir)
                tag["inputSlot"] = ItemIO.Save(InputSlot);
            if (OutputSlot != null && !OutputSlot.IsAir)
                tag["outputSlot"] = ItemIO.Save(OutputSlot);
            tag["progress"] = ConversionProgress;
        }
        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("inputSlot"))
                InputSlot = ItemIO.Load(tag.GetCompound("inputSlot"));
            else
            {
                InputSlot = new Item();
                InputSlot.TurnToAir();
            }
            if (tag.ContainsKey("outputSlot"))
                OutputSlot = ItemIO.Load(tag.GetCompound("outputSlot"));
            else
            {
                OutputSlot = new Item();
                OutputSlot.TurnToAir();
            }
            ConversionProgress = tag.GetInt("progress");
        }
        public override void NetSend(BinaryWriter writer)
        {
            bool hasInput = InputSlot != null && !InputSlot.IsAir;
            writer.Write(hasInput);
            if (hasInput)
            {
                ItemIO.Send(InputSlot, writer, true);
            }
            bool hasOutput = OutputSlot != null && !OutputSlot.IsAir;
            writer.Write(hasOutput);
            if (hasOutput)
            {
                ItemIO.Send(OutputSlot, writer, true);
            }
            writer.Write(ConversionProgress);
        }
        public override void NetReceive(BinaryReader reader)
        {
            bool hasInput = reader.ReadBoolean();
            if (hasInput)
            {
                InputSlot = ItemIO.Receive(reader, true);
            }
            else
            {
                InputSlot = new Item();
                InputSlot.TurnToAir();
            }
            bool hasOutput = reader.ReadBoolean();
            if (hasOutput)
            {
                OutputSlot = ItemIO.Receive(reader, true);
            }
            else
            {
                OutputSlot = new Item();
                OutputSlot.TurnToAir();
            }
            ConversionProgress = reader.ReadInt32();
        }
        public void SendSlotToServer(int slotIndex)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Item item = slotIndex == 0 ? InputSlot : OutputSlot;
                if (item == null)
                {
                    item = new Item();
                    item.TurnToAir();
                }
                ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                packet.Write((byte)JarkosPacketType.ConverterSlotSync);
                packet.Write(ID);
                packet.Write((byte)slotIndex);
                bool hasItem = !item.IsAir;
                packet.Write(hasItem);
                if (hasItem)
                    ItemIO.Send(item, packet, true);
                packet.Send();
            }
        }
        public void DropItems(int x, int y)
        {
            if (InputSlot != null && !InputSlot.IsAir)
            {
                Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 48, 64, InputSlot.type, InputSlot.stack);
                InputSlot.TurnToAir();
            }
            if (OutputSlot != null && !OutputSlot.IsAir)
            {
                Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 48, 64, OutputSlot.type, OutputSlot.stack);
                OutputSlot.TurnToAir();
            }
        }
    }
	#endregion
}
