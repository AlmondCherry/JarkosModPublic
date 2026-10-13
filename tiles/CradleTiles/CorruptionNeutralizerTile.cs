using System;
using jarkos.items.usetiles;
using Jarkos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace jarkos.tiles.CradleTiles
{
	public class CorruptionNeutralizerTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 0;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
			ModTileEntity tileEntity = ModContent.GetInstance<CorruptionNeutralizerTileEntity>();
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			RegisterItemDrop(ModContent.ItemType<CorruptionNeutralizerItem>());
			AddMapEntry(new Color(115, 90, 130), CreateMapEntryName());
			DustType = DustID.Stone;
			HitSound = SoundID.Tink;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Point16 origin = GetOrigin(i, j);
			ModContent.GetInstance<CorruptionNeutralizerTileEntity>().Kill(origin.X, origin.Y);
		}
		public static Point16 GetOrigin(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			int originX = i - tile.TileFrameX % 32 / 16;
			int originY = j - tile.TileFrameY % 48 / 16;
			return new Point16(originX, originY);
		}
		public static CorruptionNeutralizerTileEntity GetOrCreate(int i, int j) {
			Point16 origin = GetOrigin(i, j);
			if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity) && rawEntity is CorruptionNeutralizerTileEntity existing) {
				return existing;
			}
			Tile origin_tile = Framing.GetTileSafely(origin.X, origin.Y);
			int neutralizerType = ModContent.TileType<CorruptionNeutralizerTile>();
			if (!origin_tile.HasTile || origin_tile.TileType != neutralizerType) {
				return null;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return null;
			}
			int id = ModContent.GetInstance<CorruptionNeutralizerTileEntity>().Place(origin.X, origin.Y);
			if (id < 0) {
				return null;
			}
			if (TileEntity.ByID.TryGetValue(id, out TileEntity placedRaw) && placedRaw is CorruptionNeutralizerTileEntity placed) {
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.TileEntitySharing, number: placed.ID, number2: placed.Position.X, number3: placed.Position.Y);
				}
				return placed;
			}
			return null;
		}
		public override bool RightClick(int i, int j) {
			Point16 origin = GetOrigin(i, j);
			SpawnToggleVisuals(origin, predictedActive: !IsCurrentlyActive(origin));
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
				packet.Write((byte)JarkosPacketType.MessageType_ToggleNeutralizer);
				packet.Write((int)origin.X);
				packet.Write((int)origin.Y);
				packet.Send();
				return true;
			}
			CorruptionNeutralizerTileEntity entity = GetOrCreate(i, j);
			if (entity is null) {
				return false;
			}
			ToggleAndAnnounce(entity);
			return true;
		}
		public static void ToggleAndAnnounce(CorruptionNeutralizerTileEntity entity) {
			entity.IsActive = !entity.IsActive;
			if (Main.netMode == NetmodeID.SinglePlayer) {
				SpawnToggleVisuals(entity.Position, entity.IsActive);
			}
			else if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.TileEntitySharing, number: entity.ID, number2: entity.Position.X, number3: entity.Position.Y);
			}
		}
		private static bool IsCurrentlyActive(Point16 origin) {
			return TileEntity.ByPosition.TryGetValue(origin, out TileEntity raw)
				&& raw is CorruptionNeutralizerTileEntity entity
				&& entity.IsActive;
		}
		private static void SpawnToggleVisuals(Point16 origin, bool predictedActive) {
			Vector2 worldPos = new(origin.X * 16 + 16, origin.Y * 16 + 24);
			SoundEngine.PlaySound(predictedActive ? SoundID.Item29 : SoundID.Item68, worldPos);
			for (int k = 0; k < 16; k++) {
				int dustType = predictedActive ? DustID.PurpleTorch : DustID.Smoke;
				Dust dust = Dust.NewDustDirect(new Vector2(origin.X * 16, origin.Y * 16), 32, 48, dustType, 0f, -2f);
				dust.noGravity = true;
				dust.scale = 1.2f;
			}
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<CorruptionNeutralizerItem>();
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Point16 origin = GetOrigin(i, j);
			bool active = TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity)
				&& rawEntity is CorruptionNeutralizerTileEntity entity
				&& entity.IsActive;
			if (active) {
				float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.07f);
				r = 0.45f * pulse;
				g = 0.30f * pulse;
				b = 0.55f * pulse;
			}
			else {
				r = 0.10f;
				g = 0.08f;
				b = 0.12f;
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameX != 0 || tile.TileFrameY != 0) {
				return;
			}
			Point16 origin = new(i, j);
			if (!TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity) || rawEntity is not CorruptionNeutralizerTileEntity entity) {
				return;
			}
			if (!entity.IsActive) {
				return;
			}
		}
	}
}
