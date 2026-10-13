using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using jarkos.buffs.buff;
using Terraria.DataStructures;
using Terraria.Enums;
using jarkos.items.usetiles;
using Terraria.ModLoader.IO;
using System.IO;
using Jarkos.UI;
using Terraria.UI;
using System.Collections.Generic;
using Terraria.GameContent.ObjectInteractions;
namespace jarkos.tiles
{
	public class Arcane_forge : ModTile
	{
		public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileLighted[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 4, 0);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<SkyForgeTE>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(100, 150, 255), CreateMapEntryName());
            DustType = DustID.BlueTorch;
            MinPick = 200;
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.5f;
            b = 1.0f;
        }
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX / 18;
            int frameY = tile.TileFrameY / 18;
            int left = i - frameX;
            int top = j - frameY;
            foreach (var kvp in TileEntity.ByPosition)
            {
            }
            Point16 tePosition = new Point16(left, top);
            if (TileEntity.ByPosition.TryGetValue(tePosition, out TileEntity te))
            {
                if (te is SkyForgeTE skyForge)
                {
                    SkyForgeUISystem.Instance.ToggleUI(skyForge);
                }
            }
            else
            {
                if (TryPlaceMissingTE(left, top))
                {
                }
            }
            return true;
        }
        private bool TryPlaceMissingTE(int left, int top)
        {
            Tile tile = Main.tile[left, top];
            if (tile.HasTile && tile.TileType == Type)
            {
                int id = ModContent.GetInstance<SkyForgeTE>().Place(left, top);
                if (id != -1)
                {
                    if (TileEntity.ByPosition.TryGetValue(new Point16(left, top), out TileEntity te) && te is SkyForgeTE skyForge)
                    {
                        SkyForgeUISystem.Instance.ToggleUI(skyForge);
                        return true;
                    }
                }
            }
            return false;
        }
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<ArcaneForgeItem>();
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Point16 origin = new Point16(i, j);
            ModContent.GetInstance<SkyForgeTE>().Kill(origin.X, origin.Y);
        }
	}
	#region TileEntity
	public class SkyForgeTE : ModTileEntity
    {
        public Item MaterialSlot = new Item();
        public Item WeaponSlot = new Item();
        public Item OutputSlot = new Item();
        public float BonusDamage { get; set; }
        public float BonusSpeed { get; set; }
        public float BonusCrit { get; set; }
        public float BonusKnockback { get; set; }
        private bool isInitialized = false;
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            if (!tile.HasTile)
                return false;
            if (tile.TileType != ModContent.TileType<Arcane_forge>())
                return false;
            return tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            TileObjectData data = TileObjectData.GetTileData(type, style);
            int left = i - (data?.Origin.X ?? 1);
            int top = j - (data?.Origin.Y ?? 1);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, left, top, 3, 2);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, left, top, Type);
                return -1;
            }
            int id = Place(left, top);
            if (id != -1 && TileEntity.ByID.TryGetValue(id, out TileEntity te) && te is SkyForgeTE skyForge)
            {
                skyForge.RandomizeCalibration();
            }
            return id;
        }
        public void RandomizeCalibration()
        {
            BonusDamage = Main.rand.NextFloat(0f, 0.20f);
            BonusSpeed = Main.rand.NextFloat(0f, 0.20f);
            BonusCrit = Main.rand.NextFloat(0f, 0.20f);
            BonusKnockback = Main.rand.NextFloat(0f, 0.20f);
            isInitialized = true;
        }
        public bool NeedsInitialization()
        {
            return !isInitialized && BonusDamage == 0 && BonusSpeed == 0 && BonusCrit == 0 && BonusKnockback == 0;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag["materialSlot"] = ItemIO.Save(MaterialSlot ?? new Item());
            tag["weaponSlot"] = ItemIO.Save(WeaponSlot ?? new Item());
            tag["outputSlot"] = ItemIO.Save(OutputSlot ?? new Item());
            tag["bonusDamage"] = BonusDamage;
            tag["bonusSpeed"] = BonusSpeed;
            tag["bonusCrit"] = BonusCrit;
            tag["bonusKnockback"] = BonusKnockback;
            tag["isInitialized"] = isInitialized;
        }
        public override void LoadData(TagCompound tag)
        {
            MaterialSlot = tag.ContainsKey("materialSlot") ? ItemIO.Load(tag.GetCompound("materialSlot")) : new Item();
            WeaponSlot = tag.ContainsKey("weaponSlot") ? ItemIO.Load(tag.GetCompound("weaponSlot")) : new Item();
            OutputSlot = tag.ContainsKey("outputSlot") ? ItemIO.Load(tag.GetCompound("outputSlot")) : new Item();
            BonusDamage = tag.GetFloat("bonusDamage");
            BonusSpeed = tag.GetFloat("bonusSpeed");
            BonusCrit = tag.GetFloat("bonusCrit");
            BonusKnockback = tag.GetFloat("bonusKnockback");
            isInitialized = tag.GetBool("isInitialized");
        }
        public override void NetSend(BinaryWriter writer)
        {
            ItemIO.Send(MaterialSlot ?? new Item(), writer, true);
            ItemIO.Send(WeaponSlot ?? new Item(), writer, true);
            ItemIO.Send(OutputSlot ?? new Item(), writer, true);
            writer.Write(BonusDamage);
            writer.Write(BonusSpeed);
            writer.Write(BonusCrit);
            writer.Write(BonusKnockback);
            writer.Write(isInitialized);
        }
        public override void NetReceive(BinaryReader reader)
        {
            MaterialSlot = ItemIO.Receive(reader, true);
            WeaponSlot = ItemIO.Receive(reader, true);
            OutputSlot = ItemIO.Receive(reader, true);
            BonusDamage = reader.ReadSingle();
            BonusSpeed = reader.ReadSingle();
            BonusCrit = reader.ReadSingle();
            BonusKnockback = reader.ReadSingle();
            isInitialized = reader.ReadBoolean();
        }
    }
	#endregion
}
