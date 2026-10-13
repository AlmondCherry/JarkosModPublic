using System;
using jarkos.items.usetiles;
using Jarkos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace jarkos.tiles.CradleTiles
{
	public class PetrifiedAltarTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 4, 0);
			TileObjectData.newTile.LavaDeath = false;
			ModTileEntity tileEntity = ModContent.GetInstance<PetrifiedAltarTileEntity>();
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			RegisterItemDrop(ModContent.ItemType<PetrifiedAltarItem>());
			AddMapEntry(new Color(129, 116, 112), CreateMapEntryName());
			DustType = DustID.Stone;
			HitSound = SoundID.Tink;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void KillMultiTile(int i, int j, int frameX, int frameY) 
		{
			int originX = i - frameX % 72 / 18;
			int originY = j - frameY % 36 / 18;
			ModContent.GetInstance<PetrifiedAltarTileEntity>().Kill(originX, originY);
		}
		private static Point16 GetOrigin(int i, int j) 
		{
			Tile tile = Framing.GetTileSafely(i, j);
			int originX = i - tile.TileFrameX % 72 / 18;
			int originY = j - tile.TileFrameY % 36 / 18;

			return new Point16(originX, originY);
		}
		public static PetrifiedAltarTileEntity GetOrCreate(int i, int j) 
		{
			Point16 origin = GetOrigin(i, j);
			if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity) && rawEntity is PetrifiedAltarTileEntity existing) 
			{
				return existing;
			}
			Tile origin_tile = Framing.GetTileSafely(origin.X, origin.Y);
			int altarType = ModContent.TileType<PetrifiedAltarTile>();
			if (!origin_tile.HasTile || origin_tile.TileType != altarType) 
			{
				return null;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) 
			{
				foreach (var pair in TileEntity.ByID) 
				{
					if (pair.Value is PetrifiedAltarTileEntity altarTE && altarTE.Position == origin) 
					{
						return altarTE;
					}
				}
				return null;
			}
			int id = ModContent.GetInstance<PetrifiedAltarTileEntity>().Place(origin.X, origin.Y);
			if (id < 0) {
				return null;
			}
			if (TileEntity.ByID.TryGetValue(id, out TileEntity placedRaw) && placedRaw is PetrifiedAltarTileEntity placed) 
			{
				if (Main.netMode == NetmodeID.Server) 
				{
					NetMessage.SendData(MessageID.TileEntitySharing, number: placed.ID, number2: placed.Position.X, number3: placed.Position.Y);
				}
				return placed;
			}
			return null;
		}
		public override bool RightClick(int i, int j) 
		{
			Point16 origin = GetOrigin(i, j);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				TileEntity.ByPosition.TryGetValue(origin, out TileEntity raw);
				if (raw is PetrifiedAltarTileEntity clientEntity && clientEntity.IsAnimating) 
				{
					return false;
				}
				ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
				packet.Write((byte)JarkosPacketType.MessageType_StartAltarRitual);
				packet.Write((int)origin.X);
				packet.Write((int)origin.Y);
				packet.Send();
				return true;
			}

			PetrifiedAltarTileEntity entity = GetOrCreate(i, j);
			if (entity is null || entity.IsAnimating) 
			{
				return false;
			}
			bool started = entity.TryStartRitual(Main.LocalPlayer);
			if (!started) 
			{
				ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("Something is missing."), Color.SlateGray, Main.myPlayer);
				SoundEngine.PlaySound(SoundID.Tink, new Vector2((origin.X + 1) * 16, (origin.Y + 1) * 16));
				return false;
			}
			return true;
		}
		public override void MouseOver(int i, int j) 
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<PetrifiedAltarItem>();
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) 
		{
			Point16 origin = GetOrigin(i, j);
			if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity) && rawEntity is PetrifiedAltarTileEntity entity && entity.IsAnimating) {
				float t = 1f - entity.RitualTimer / (float)PetrifiedAltarTileEntity.RitualDurationTicks;
				float pulse = 0.55f + 0.45f * (float)Math.Sin(Main.GameUpdateCount * 0.18f);
				r = 0.45f + t * 0.5f * pulse;
				g = 0.30f + t * 0.4f * pulse;
				b = 0.10f + t * 0.2f * pulse;
				return;
			}
			r = 0.13f;
			g = 0.10f;
			b = 0.08f;
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) 
		{
			Tile tile = Main.tile[i, j];
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle sourceRect = new Rectangle(tile.TileFrameX + 2, tile.TileFrameY + 2, 16, 16);
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 drawPos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
			Color drawColor = Lighting.GetColor(i, j);
			spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			return false;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) 
		{
			Point16 origin = GetOrigin(i, j);
			if (i != origin.X || j != origin.Y) 
			{
				return;
			}
			if (!TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity) || rawEntity is not PetrifiedAltarTileEntity entity || !entity.IsAnimating) 
			{
				return;
			}
			int centerXPx = origin.X * 16 + 32;
			int topYPx = origin.Y * 16;
			if (Main.GameUpdateCount % 2 == 0) 
			{
				int dust = Dust.NewDust(new Vector2(centerXPx - 14, topYPx - 6), 28, 6, DustID.YellowTorch, 0f, -2.5f, 80, default, 1.2f);
				if (dust >= 0 && dust < Main.maxDust) 
				{
					Main.dust[dust].noGravity = true;
				}
			}
		}
	}
}
