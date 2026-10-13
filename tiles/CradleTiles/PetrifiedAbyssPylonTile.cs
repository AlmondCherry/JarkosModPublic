using jarkos.cradle_biome;
using jarkos.cradle_biome.system;
using jarkos.items.usetiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;
namespace jarkos.tiles.CradleTiles
{
	public class PetrifiedAbyssPylonTile : ModPylon
	{
		private const int CrystalVerticalFrameCount = 8;
		private Asset<Texture2D> crystalTexture;
		private Asset<Texture2D> crystalHighlightTexture;
		private Asset<Texture2D> mapIcon;
		public override void Load() {
			crystalTexture = ModContent.Request<Texture2D>("Jarkos/tiles/CradleTiles/PetrifiedAbyssPylonTile_Crystal");
			crystalHighlightTexture = ModContent.Request<Texture2D>("Jarkos/tiles/CradleTiles/PetrifiedAbyssPylonTile_CrystalHighlight");
			mapIcon = ModContent.Request<Texture2D>("Jarkos/tiles/CradleTiles/PetrifiedAbyssPylonTile_MapIcon");
		}
		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			VanillaFallbackOnModDeletion = TileID.TeleportationPylon;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TEModdedPylon moddedPylon = ModContent.GetInstance<PetrifiedAbyssPylonTileEntity>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.PreventsSandfall[Type] = true;
			TileID.Sets.AvoidedByMeteorLanding[Type] = true;
			AddToArray(ref TileID.Sets.CountsAsPylon);
			AddMapEntry(new Color(178, 166, 160), CreateMapEntryName());
		}
		public override void MouseOver(int i, int j) {
			Main.LocalPlayer.cursorItemIconEnabled = true;
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<PetrifiedAbyssPylonItem>();
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			ModContent.GetInstance<PetrifiedAbyssPylonTileEntity>().Kill(i, j);
		}
		public override NPCShop.Entry GetNPCShopEntry() {
			NPCShop.Entry shopEntry = base.GetNPCShopEntry();
			shopEntry.AddCondition(PetrifiedAbyssConditions.PetrifiedAbyssGenerated);
			shopEntry.AddCondition(PetrifiedAbyssConditions.InPetrifiedAbyss);
			shopEntry.AddCondition(PetrifiedAbyssConditions.NeutralizerActive);
			return shopEntry;
		}
		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			return true;
		}
		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			return ModContent.GetInstance<PetrifiedAbyssTileCount>().petrifiedDepthstoneCount >= 75;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.45f;
			g = 0.38f;
			b = 0.34f;
		}
		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.SlateGray * 0.25f, new Color(178, 166, 160), 2, CrystalVerticalFrameCount);
		}
		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
			bool mouseOver = DefaultDrawMapIcon(ref context, mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
			DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<PetrifiedAbyssPylonItem>().DisplayName.Key, ref mouseOverText);
		}
	}
}
