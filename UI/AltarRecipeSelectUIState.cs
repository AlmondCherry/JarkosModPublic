using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using jarkos.tiles.CradleTiles;
using Terraria.UI.Chat;
using Terraria.GameContent;
using System;

namespace Jarkos.UI
{
	public class AltarRecipeSelectUIState : UIState
	{
		public bool Visible { get; private set; }
		private UIPanel panel;
		private UIList recipeList;
		private PetrifiedAltarTileEntity currentAltar;
		private List<PetrifiedAltarRecipe> currentRecipes;
		public override void OnInitialize() {
			panel = new UIPanel();
			panel.SetPadding(0);
			panel.HAlign = 0.5f;
			panel.VAlign = 0.5f;
			panel.Width.Set(300f, 0f);
			panel.Height.Set(250f, 0f);
			panel.BackgroundColor = new Color(73, 94, 171, 150);
			Append(panel);
			UIText title = new UIText("Select Ritual Output", 0.9f, true);
			HAlign = 0.5f;
			title.Top.Set(10, 0);
			panel.Append(title);
			UITextPanel<string> closeButton = new UITextPanel<string>("Close");
			HAlign = 0.5f;
			closeButton.Top.Set(-40, 1f);
			closeButton.Width.Set(100, 0);
			closeButton.Height.Set(30, 0);
			closeButton.OnLeftClick += (evt, element) => Close();
			panel.Append(closeButton);
			recipeList = new UIList();
			recipeList.Width.Set(-20, 1f);
			recipeList.Height.Set(-90, 1f);
			recipeList.Top.Set(40, 0);
			recipeList.Left.Set(10, 0);
			recipeList.ListPadding = 5f;
			panel.Append(recipeList);
			UIScrollbar scrollbar = new UIScrollbar();
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(-90, 1f);
			scrollbar.Top.Set(40, 0);
			scrollbar.Left.Set(-20, 1f);
			panel.Append(scrollbar);
			recipeList.SetScrollbar(scrollbar);
		}
		public void Open(PetrifiedAltarTileEntity altar, List<PetrifiedAltarRecipe> recipes) {
			currentAltar = altar;
			currentRecipes = recipes;
			Visible = true;
			Main.playerInventory = true;
			recipeList.Clear();

			for (int i = 0; i < recipes.Count; i++) 
			{
				int index = i;
				PetrifiedAltarRecipe recipe = recipes[i];
				UIPanel btn = new UIPanel();

				btn.Width.Set(0, 1f);
				btn.Height.Set(50, 0f);

				btn.OnLeftClick += (evt, element) => SelectRecipe(index);
				btn.OnMouseOver += (evt, element) => 
				{
					SoundEngine.PlaySound(SoundID.MenuTick);
					btn.BackgroundColor = new Color(73, 94, 171);
				};

				btn.OnMouseOut += (evt, element) => btn.BackgroundColor = new Color(63, 82, 151) * 0.7f;

				Item dummyItem = new Item();
				dummyItem.SetDefaults(recipe.ResultItem);
				dummyItem.stack = recipe.ResultStack;

				UIItemSlot itemSlot = new UIItemSlot(dummyItem);
				itemSlot.Left.Set(10, 0);
				itemSlot.VAlign = 0.5f;
				btn.Append(itemSlot);

				UIText itemName = new UIText(dummyItem.Name);
				itemName.Left.Set(60, 0);
				itemName.VAlign = 0.5f;
				btn.Append(itemName);
				recipeList.Add(btn);
			}
		}
		private void SelectRecipe(int localIndex) {
			Close();
			PetrifiedAltarRecipe recipe = currentRecipes[localIndex];
			int globalIndex = PetrifiedAltarTileEntity.Recipes.IndexOf(recipe);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
				packet.Write((byte)JarkosPacketType.MessageType_StartSpecificAltarRitual);
				packet.Write((int)currentAltar.Position.X);
				packet.Write((int)currentAltar.Position.Y);
				packet.Write(globalIndex);
				packet.Send();
				Vector2 exactCenter = new Vector2(currentAltar.Position.X * 16 + 32, currentAltar.Position.Y * 16 + 16);
				SoundEngine.PlaySound(SoundID.Item37, exactCenter);
			} 
			else
			{
				currentAltar.StartRitualWithRecipeIndex(globalIndex, Main.LocalPlayer);
			}
		}
		public void Close() {
			Visible = false;
		}
	}
	public class UIItemSlot : UIElement
	{
		private Item item;
		public UIItemSlot(Item item)
		{
			this.item = item;
			Width.Set(32, 0);
			Height.Set(32, 0);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			Vector2 position = new Vector2(dimensions.X, dimensions.Y);
			Main.instance.LoadItem(item.type);
			Texture2D texture = TextureAssets.Item[item.type].Value;
			Rectangle frame = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(texture) : texture.Frame();
			float scale = 1f;
			if (frame.Width > 32 || frame.Height > 32)
			{
				scale = 32f / Math.Max(frame.Width, frame.Height);
			}
			Vector2 drawPos = position + new Vector2(16, 16);
			spriteBatch.Draw(texture, drawPos, frame, Color.White, 0f, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
			if (item.stack > 1)
			{
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, item.stack.ToString(), position + new Vector2(10, 16), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
			}
			if (IsMouseHovering)
			{
				Main.HoverItem = item.Clone();
				Main.hoverItemName = item.Name;
			}
		}
	}
}
