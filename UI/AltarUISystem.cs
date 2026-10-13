using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using jarkos.tiles.CradleTiles;
using Microsoft.Xna.Framework.Graphics;

namespace Jarkos.UI
{
	public class AltarUISystem : ModSystem
	{
		public static bool DrawWarningIconThisFrame = false;
		internal AltarRecipeSelectUIState recipeSelectUI;
		internal MinorAltarAmountUIState amountSelectUI;
		private UserInterface recipeUserInterface;
		private UserInterface amountUserInterface;
		public override void Load() {
			if (!Main.dedServ) {
				recipeSelectUI = new AltarRecipeSelectUIState();
				recipeSelectUI.Activate();
				recipeUserInterface = new UserInterface();
				recipeUserInterface.SetState(recipeSelectUI);
				amountSelectUI = new MinorAltarAmountUIState();
				amountSelectUI.Activate();
				amountUserInterface = new UserInterface();
				amountUserInterface.SetState(amountSelectUI);
			}
		}
		public override void Unload() {
			recipeSelectUI = null;
			amountSelectUI = null;
			recipeUserInterface = null;
			amountUserInterface = null;
		}
		public override void UpdateUI(GameTime gameTime) {
			if (recipeUserInterface?.CurrentState != null && recipeSelectUI.Visible)
			{
				recipeUserInterface.Update(gameTime);
			}
			if (amountUserInterface?.CurrentState != null && amountSelectUI.Visible)
			{
				amountUserInterface.Update(gameTime);
			}
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Jarkos: Altar Recipe Select UI", delegate
				{
						if (recipeSelectUI.Visible) {
							recipeUserInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					}, InterfaceScaleType.UI));
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Jarkos: Minor Altar Amount UI", delegate
				{
						if (amountSelectUI.Visible) {
							amountUserInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					}, InterfaceScaleType.UI));
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Jarkos: Minor Altar Warning Icon", delegate
				{
						if (DrawWarningIconThisFrame) {
							DrawWarningIconThisFrame = false;
							Texture2D tex = ModContent.Request<Texture2D>("jarkos/UI/WarningIcon").Value;
							Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY) + new Vector2(16, 16);
							Main.spriteBatch.Draw(tex, mousePos, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
						}
						return true;
				}, InterfaceScaleType.UI));
			}
		}
		public static void OpenRecipeSelect(PetrifiedAltarTileEntity altar, List<PetrifiedAltarRecipe> recipes)
		{
			ModContent.GetInstance<AltarUISystem>().recipeSelectUI.Open(altar, recipes);
		}
		public static void OpenAmountSelect(PetrifiedMinorAltarTileEntity altar, Item item)
		{
			ModContent.GetInstance<AltarUISystem>().amountSelectUI.Open(altar, item);
		}
	}
}
