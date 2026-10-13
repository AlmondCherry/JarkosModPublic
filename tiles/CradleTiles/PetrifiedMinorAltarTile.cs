using System;
using jarkos.items.usetiles;
using Jarkos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace jarkos.tiles.CradleTiles
{
	public class PetrifiedMinorAltarTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileSolidTop[Type] = false;
			Main.tileSolid[Type] = false;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
			ModTileEntity tileEntity = ModContent.GetInstance<PetrifiedMinorAltarTileEntity>();
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			RegisterItemDrop(ModContent.ItemType<PetrifiedMinorAltarItem>());
			AddMapEntry(new Color(155, 138, 128), CreateMapEntryName());
			DustType = DustID.Stone;
			HitSound = SoundID.Tink;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			int originX = i;
			int originY = j - frameY % 36 / 18;
			if (TileEntity.ByPosition.TryGetValue(new Point16(originX, originY), out TileEntity rawEntity) && rawEntity is PetrifiedMinorAltarTileEntity entity) {
				if (entity.HasItem) {
					Item taken = entity.TakeItem();
					int newIdx = Item.NewItem(new EntitySource_TileBreak(i, j), originX * 16, originY * 16, 32, 32, taken.type, taken.stack);
					if (newIdx >= 0 && newIdx < Main.maxItems) {
						Main.item[newIdx].prefix = taken.prefix;
					}
				}
			}
			ModContent.GetInstance<PetrifiedMinorAltarTileEntity>().Kill(originX, originY);
		}
		private static Point16 GetOrigin(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			int originX = i;
			int originY = j - tile.TileFrameY % 36 / 18;
			return new Point16(originX, originY);
		}
		public static PetrifiedMinorAltarTileEntity GetOrCreate(int i, int j) {
			Point16 origin = GetOrigin(i, j);
			if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity) && rawEntity is PetrifiedMinorAltarTileEntity existing) {
				return existing;
			}
			Tile origin_tile = Framing.GetTileSafely(origin.X, origin.Y);
			int minorType = ModContent.TileType<PetrifiedMinorAltarTile>();
			if (!origin_tile.HasTile || origin_tile.TileType != minorType) {
				return null;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return null;
			}
			int id = ModContent.GetInstance<PetrifiedMinorAltarTileEntity>().Place(origin.X, origin.Y);
			if (id < 0) {
				return null;
			}
			if (TileEntity.ByID.TryGetValue(id, out TileEntity placedRaw) && placedRaw is PetrifiedMinorAltarTileEntity placed) {
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.TileEntitySharing, number: placed.ID, number2: placed.Position.X, number3: placed.Position.Y);
				}
				return placed;
			}
			return null;
		}
		public override bool RightClick(int i, int j) {
			Point16 origin = GetOrigin(i, j);
			PetrifiedMinorAltarTileEntity entity = GetOrCreate(i, j);
			if (entity is null) {
				return false;
			}
			Player player = Main.LocalPlayer;
			Item heldByPlayer = player.HeldItem;
			if (entity.HasItem) {
				if (heldByPlayer != null && !heldByPlayer.IsAir) {
					return false;
				}
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
					packet.Write((byte)JarkosPacketType.MessageType_TakeMinorAltar);
					packet.Write((int)origin.X);
					packet.Write((int)origin.Y);
					packet.Send();
					SoundEngine.PlaySound(SoundID.Item37, new Vector2(origin.X * 16 + 8, origin.Y * 16));
					return true;
				}
				Item taken = entity.TakeItem();
				int newIdx = Item.NewItem(new EntitySource_TileEntity(entity), origin.X * 16, origin.Y * 16 - 8, 32, 16, taken.type, taken.stack);
				if (newIdx >= 0 && newIdx < Main.maxItems) {
					Main.item[newIdx].prefix = taken.prefix;
					Main.item[newIdx].velocity = new Vector2(0f, -2f);
					Main.item[newIdx].noGrabDelay = 0;
				}
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.TileEntitySharing, number: entity.ID, number2: entity.Position.X, number3: entity.Position.Y);
				}
				SoundEngine.PlaySound(SoundID.Item3, new Vector2(origin.X * 16 + 8, origin.Y * 16));
				return true;
			}
			if (heldByPlayer is null || heldByPlayer.IsAir) {
				return false;
			}
			if (heldByPlayer.stack > 1) {
				if (Main.netMode != NetmodeID.Server) {
					Jarkos.UI.AltarUISystem.OpenAmountSelect(entity, heldByPlayer);
				}
				return true;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				Item depositedLocal = heldByPlayer.Clone();
				depositedLocal.stack = 1;
				ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
				packet.Write((byte)JarkosPacketType.MessageType_DepositMinorAltar);
				packet.Write((int)origin.X);
				packet.Write((int)origin.Y);
				Terraria.ModLoader.IO.ItemIO.Send(depositedLocal, packet, writeStack: true);
				packet.Send();
				heldByPlayer.stack--;
				if (heldByPlayer.stack <= 0) {
					heldByPlayer.TurnToAir();
				}
				SoundEngine.PlaySound(SoundID.Item37, new Vector2(origin.X * 16 + 8, origin.Y * 16));
				return true;
			}
			Item deposited = heldByPlayer.Clone();
			deposited.stack = 1;
			entity.SetItem(deposited);
			heldByPlayer.stack--;
			if (heldByPlayer.stack <= 0) {
				heldByPlayer.TurnToAir();
			}
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.TileEntitySharing, number: entity.ID, number2: entity.Position.X, number3: entity.Position.Y);
			}
			SoundEngine.PlaySound(SoundID.Item37, new Vector2(origin.X * 16 + 8, origin.Y * 16));
			return true;
		}
		public override void MouseOver(int i, int j) {
			Point16 origin = GetOrigin(i, j);
			Player player = Main.LocalPlayer;
			if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity) && rawEntity is PetrifiedMinorAltarTileEntity entity && entity.HasItem) {
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = entity.HeldItem.type;
			}
			else {
				if (player.HeldItem != null && !player.HeldItem.IsAir) {
					Jarkos.UI.AltarUISystem.DrawWarningIconThisFrame = true;
				} else {
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = ModContent.ItemType<PetrifiedMinorAltarItem>();
				}
			}
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Point16 origin = GetOrigin(i, j);
			if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity rawEntity) && rawEntity is PetrifiedMinorAltarTileEntity entity && entity.HasItem) {
				r = 0.30f;
				g = 0.25f;
				b = 0.20f;
			}
			else {
				r = 0.10f;
				g = 0.08f;
				b = 0.06f;
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameY != 0) {
				return;
			}
			int originX = i;
			int originY = j;
			if (!TileEntity.ByPosition.TryGetValue(new Point16(originX, originY), out TileEntity rawEntity) || rawEntity is not PetrifiedMinorAltarTileEntity entity || !entity.HasItem) {
				return;
			}
			Item heldItem = entity.HeldItem;
			Main.instance.LoadItem(heldItem.type);
			Asset<Texture2D> itemAsset = TextureAssets.Item[heldItem.type];
			if (itemAsset?.Value is null) {
				return;
			}
			Texture2D texture = itemAsset.Value;
			Rectangle frame = Main.itemAnimations[heldItem.type] is { } anim ? anim.GetFrame(texture) : texture.Frame();
			float bobble = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 2f;
			Vector2 worldPos = new(originX * 16 + 8, originY * 16 - 4 + bobble);
			Vector2 screenPos = worldPos - Main.screenPosition + new Vector2(Main.offScreenRange, Main.offScreenRange);
			float maxSide = Math.Max(frame.Width, frame.Height);
			float scale = maxSide > 16f ? 16f / maxSide : 1f;
			Color drawColor = Color.White * 0.95f;
			spriteBatch.Draw(texture, screenPos, frame, drawColor, 0f, frame.Size() * 0.5f, scale, SpriteEffects.None, 0f);
		}
	}
}
