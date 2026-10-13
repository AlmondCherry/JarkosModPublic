using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameInput;
using Terraria.GameContent;
using Terraria.UI.Chat;
using jarkos.tiles.CradleTiles;

namespace Jarkos.UI
{
	public class MinorAltarAmountUIState : UIState
	{
		public bool Visible { get; private set; }
		private PopupPanelElement panel;
		private CloseButtonElement closeButton;
		private StyledButtonElement minusButton;
		private StyledButtonElement depositAllBtn;
		private AmountBoxElement amountBox;
		private StyledButtonElement plusButton;
		private StyledButtonElement confirmButton;
		private UIText amountText;

		private PetrifiedMinorAltarTileEntity currentAltar;
		private Item currentItem;
		private int selectedAmount = 1;

		private bool isTyping = false;
		private string typingBuffer = "";
		private float digitOffsetY = 0f;

		private float _animProgress = 0f;
		private bool _isClosing = false;
		private const float AnimDuration = 0.2f;
		public static Matrix CurrentAnimMatrix = Main.UIScaleMatrix;

		private class PopupPanelElement : UIElement
		{
			public float Opacity = 1f;
			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				var tex1 = ModContent.Request<Texture2D>("jarkos/UI/Textures/PopupPanel_1");
				var tex2 = ModContent.Request<Texture2D>("jarkos/UI/Textures/PopupPanel_2");
				var tex3 = ModContent.Request<Texture2D>("jarkos/UI/Textures/PopupPanel_3");
				CalculatedStyle dims = GetDimensions();

				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);
				if (tex1.IsLoaded && tex2.IsLoaded && tex3.IsLoaded)
				{

					spriteBatch.Draw(tex2.Value, rect, new Color(12, 22, 55, (int)(245 * Opacity)));
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, CurrentAnimMatrix);
					spriteBatch.Draw(tex3.Value, rect, new Color(30, 80, 180, (int)(180 * Opacity)));
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, CurrentAnimMatrix);

					spriteBatch.Draw(tex1.Value, rect, Color.Black * Opacity);
				}
			}
		}

		private class CloseButtonElement : UIElement
		{
			public float Opacity = 1f;
			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				var fillTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonSquare_Fill");
				var outlineTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonSquare");
				var crossTex = ModContent.Request<Texture2D>("jarkos/UI/CloseBtn_3");

				Color redTint = IsMouseHovering ? new Color(240, 50, 50) : new Color(200, 40, 40);

				if (fillTex.IsLoaded && outlineTex.IsLoaded)
				{
					spriteBatch.Draw(fillTex.Value, rect, redTint * Opacity);
					spriteBatch.Draw(outlineTex.Value, rect, Color.White * Opacity);
				}

				if (crossTex.IsLoaded)
				{
					int iconW = crossTex.Width();
					int iconH = crossTex.Height();
					Rectangle crossRect = new Rectangle((int)dims.X + ((int)dims.Width - iconW) / 2, (int)dims.Y + ((int)dims.Height - iconH) / 2, iconW, iconH);
					spriteBatch.Draw(crossTex.Value, crossRect, Color.White * Opacity);
				}
			}
		}

		private class StyledButtonElement : UIElement
		{
			public float Opacity = 1f;
			private string text;
			private Texture2D iconTexture;
			private float textScale;

			public string Tooltip { get; set; }

			public StyledButtonElement(string text, float textScale = 1f)
			{
				this.text = text;
				this.textScale = textScale;
			}

			public StyledButtonElement(Texture2D iconTexture)
			{
				this.iconTexture = iconTexture;
			}

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				Color btnColor = IsMouseHovering ? new Color(60, 70, 110, 230) : new Color(30, 40, 75, 210);
				Color borderColor = IsMouseHovering ? Color.Gold : Color.Black;

				var pixel = TextureAssets.MagicPixel.Value;
				spriteBatch.Draw(pixel, rect, btnColor * Opacity);


				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor * Opacity);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), borderColor * Opacity);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor * Opacity);
				spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), borderColor * Opacity);

				if (iconTexture != null)
				{
					Vector2 pos = dims.Position() + new Vector2(dims.Width, dims.Height) / 2f;
					Vector2 origin = iconTexture.Size() / 2f;
					spriteBatch.Draw(iconTexture, pos, null, Color.White * Opacity, 0f, origin, 1f, SpriteEffects.None, 0f);
				}
				else if (!string.IsNullOrEmpty(text))
				{
					Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text) * textScale;
					Vector2 pos = dims.Position() + (new Vector2(dims.Width, dims.Height) - textSize) / 2f;
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, pos, Color.White * Opacity, 0f, Vector2.Zero, new Vector2(textScale));
				}

				if (IsMouseHovering && !string.IsNullOrEmpty(Tooltip) && Opacity > 0.8f)
				{
					Main.instance.MouseText(Tooltip);
				}
			}
		}

		private class AmountBoxElement : UIElement
		{
			public float Opacity = 1f;
			public bool IsTyping { get; set; }

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				Color bgColor = new Color(10, 18, 42, 230);
				Color borderColor = (IsTyping || IsMouseHovering) ? Color.Gold : Color.Black;

				var pixel = TextureAssets.MagicPixel.Value;
				spriteBatch.Draw(pixel, rect, bgColor * Opacity);

				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor * Opacity);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), borderColor * Opacity);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor * Opacity);
				spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), borderColor * Opacity);
			}
		}

		public override void OnInitialize()
		{
			panel = new PopupPanelElement();
			panel.SetPadding(0);
			panel.HAlign = 0.5f;
			panel.VAlign = 0.5f;
			panel.Width.Set(460f, 0f);
			panel.Height.Set(280f, 0f);
			Append(panel);


			closeButton = new CloseButtonElement();
			closeButton.Width.Set(36f, 0f);
			closeButton.Height.Set(36f, 0f);
			closeButton.Left.Set(-50f, 1f);
			closeButton.Top.Set(18f, 0f);
			closeButton.OnLeftClick += (evt, element) => Close();
			panel.Append(closeButton);


			minusButton = new StyledButtonElement("-", 1.2f);
			minusButton.VAlign = 0.5f;
			minusButton.Left.Set(50f, 0f);
			minusButton.Width.Set(50f, 0f);
			minusButton.Height.Set(50f, 0f);
			minusButton.OnLeftClick += (evt, element) => { StopTyping(); ChangeAmount(-1); };
			minusButton.OnRightClick += (evt, element) => { StopTyping(); ChangeAmount(-10); };
			panel.Append(minusButton);


			var arrowTexAsset = ModContent.Request<Texture2D>("jarkos/UI/arrow_down");
			depositAllBtn = new StyledButtonElement(arrowTexAsset.Value);
			depositAllBtn.Tooltip = "Put everything";
			depositAllBtn.Left.Set(50f, 0f);
			depositAllBtn.Top.Set(178f, 0f);
			depositAllBtn.Width.Set(50f, 0f);
			depositAllBtn.Height.Set(50f, 0f);
			depositAllBtn.OnLeftClick += (evt, element) => {
				if (currentItem != null && !currentItem.IsAir)
				{
					selectedAmount = currentItem.stack;
				}
				Confirm();
			};
			panel.Append(depositAllBtn);


			amountBox = new AmountBoxElement();
			amountBox.VAlign = 0.5f;
			amountBox.HAlign = 0.5f;
			amountBox.Width.Set(120f, 0f);
			amountBox.Height.Set(54f, 0f);
			amountBox.OnLeftClick += (evt, element) => StartTyping();
			panel.Append(amountBox);

			amountText = new UIText("1", 1.2f, true);
			amountText.HAlign = 0.5f;
			amountText.VAlign = 0.5f;
			amountBox.Append(amountText);


			plusButton = new StyledButtonElement("+", 1.2f);
			plusButton.VAlign = 0.5f;
			plusButton.Left.Set(-100f, 1f);
			plusButton.Width.Set(50f, 0f);
			plusButton.Height.Set(50f, 0f);
			plusButton.OnLeftClick += (evt, element) => { StopTyping(); ChangeAmount(1); };
			plusButton.OnRightClick += (evt, element) => { StopTyping(); ChangeAmount(10); };
			panel.Append(plusButton);


			confirmButton = new StyledButtonElement("Apply", 0.9f);
			confirmButton.HAlign = 0.5f;
			confirmButton.Top.Set(-60f, 1f);
			confirmButton.Width.Set(160f, 0f);
			confirmButton.Height.Set(40f, 0f);
			confirmButton.OnLeftClick += (evt, element) => Confirm();
			panel.Append(confirmButton);
		}

		public void Open(PetrifiedMinorAltarTileEntity altar, Item item)
		{
			currentAltar = altar;
			currentItem = item;
			selectedAmount = 1;
			isTyping = false;
			typingBuffer = "";
			digitOffsetY = 0f;
			if (amountBox != null) amountBox.IsTyping = false;
			amountText.Top.Set(0f, 0f);
			Visible = true;
			_isClosing = false;
			_animProgress = 0f;
			Main.playerInventory = true;
			UpdateAmountText();
		}

		private void StartTyping()
		{
			if (_isClosing) return;
			isTyping = true;
			typingBuffer = selectedAmount.ToString();
			Main.clrInput();
			SoundEngine.PlaySound(SoundID.MenuTick);
			if (amountBox != null) amountBox.IsTyping = true;
			UpdateAmountText();
		}

		private void StopTyping()
		{
			if (!isTyping) return;
			isTyping = false;
			if (amountBox != null) amountBox.IsTyping = false;
			if (int.TryParse(typingBuffer, out int parsed))
			{
				int maxStack = currentItem != null ? currentItem.stack : 9999;
				selectedAmount = (int)MathHelper.Clamp(parsed, 1, maxStack);
			}
			else
			{
				selectedAmount = 1;
			}
			UpdateAmountText();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			float animSpeed = 1f / AnimDuration;

			if (!_isClosing)
			{
				if (_animProgress < 1f)
				{
					_animProgress = Math.Min(1f, _animProgress + dt * animSpeed);
				}
			}
			else
			{
				if (_animProgress > 0f)
				{
					_animProgress = Math.Max(0f, _animProgress - dt * animSpeed);
					if (_animProgress <= 0f)
					{
						Visible = false;
						_isClosing = false;
					}
				}
			}

			if (Visible && !_isClosing)
			{
				Main.LocalPlayer.mouseInterface = true;

				if (Math.Abs(digitOffsetY) > 0.01f)
				{
					digitOffsetY = MathHelper.Lerp(digitOffsetY, 0f, 0.22f);
					if (Math.Abs(digitOffsetY) < 0.05f) digitOffsetY = 0f;
					amountText.Top.Set(digitOffsetY, 0f);
					amountText.Recalculate();
				}

				if (isTyping)
				{
					PlayerInput.WritingText = true;

					for (int k = 0; k <= 9; k++)
					{
						Keys numKey = Keys.D0 + k;
						Keys numPadKey = Keys.NumPad0 + k;

						if ((Main.keyState.IsKeyDown(numKey) && Main.oldKeyState.IsKeyUp(numKey)) ||
							(Main.keyState.IsKeyDown(numPadKey) && Main.oldKeyState.IsKeyUp(numPadKey)))
						{
							if (typingBuffer == "0") typingBuffer = "";
							if (typingBuffer.Length < 6)
							{
								typingBuffer += k.ToString();
								if (int.TryParse(typingBuffer, out int parsed))
								{
									int maxStack = currentItem != null ? currentItem.stack : 9999;
									selectedAmount = (int)MathHelper.Clamp(parsed, 1, maxStack);
								}
								UpdateAmountText();
							}
						}
					}

					if (Main.keyState.IsKeyDown(Keys.Back) && Main.oldKeyState.IsKeyUp(Keys.Back))
					{
						if (typingBuffer.Length > 0)
						{
							typingBuffer = typingBuffer.Substring(0, typingBuffer.Length - 1);
							if (int.TryParse(typingBuffer, out int parsed) && parsed >= 1)
							{
								int maxStack = currentItem != null ? currentItem.stack : 9999;
								selectedAmount = (int)MathHelper.Clamp(parsed, 1, maxStack);
							}
							else
							{
								selectedAmount = 1;
							}
							UpdateAmountText();
						}
					}

					if (Main.keyState.IsKeyDown(Keys.Enter) && Main.oldKeyState.IsKeyUp(Keys.Enter))
					{
						StopTyping();
					}
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!Visible && _animProgress <= 0f) return;

			float easeP = 1f - MathF.Pow(1f - _animProgress, 3f);
			float scaleX = MathHelper.Lerp(0.05f, 1.0f, easeP);


			float verticalSquash = MathHelper.Lerp(0.15f, 1.0f, MathF.Sin(easeP * MathF.PI * 0.5f));
			float scaleY = scaleX * verticalSquash;
			float opacity = MathHelper.Clamp(easeP * 1.5f, 0f, 1f);


			if (panel != null) panel.Opacity = opacity;
			if (closeButton != null) closeButton.Opacity = opacity;
			if (minusButton != null) minusButton.Opacity = opacity;
			if (depositAllBtn != null) depositAllBtn.Opacity = opacity;
			if (amountBox != null) amountBox.Opacity = opacity;
			if (plusButton != null) plusButton.Opacity = opacity;
			if (confirmButton != null) confirmButton.Opacity = opacity;
			if (amountText != null) amountText.TextColor = Color.White * opacity;

			Vector2 center = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
			CurrentAnimMatrix = Matrix.CreateTranslation(-center.X, -center.Y, 0f) * Matrix.CreateScale(scaleX, scaleY, 1f) * Matrix.CreateTranslation(center.X, center.Y, 0f) * Main.UIScaleMatrix;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, CurrentAnimMatrix);

			base.Draw(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
		}

		private void ChangeAmount(int delta)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
			selectedAmount += delta;
			int maxStack = currentItem != null ? currentItem.stack : 9999;
			if (selectedAmount < 1) selectedAmount = 1;
			if (selectedAmount > maxStack) selectedAmount = maxStack;
			digitOffsetY = -10f;
			UpdateAmountText();
		}

		private void UpdateAmountText()
		{
			if (isTyping)
			{
				bool pulse = Main.GlobalTimeWrappedHourly * 4f % 1f > 0.5f;
				amountText.SetText((string.IsNullOrEmpty(typingBuffer) ? "0" : typingBuffer) + (pulse ? "|" : ""));
			}
			else
			{
				amountText.SetText(selectedAmount.ToString());
			}
		}

		private void Confirm()
		{
			if (currentItem == null || currentAltar == null) return;

			Item depositItem = currentItem.Clone();
			depositItem.stack = selectedAmount;
			currentItem.stack -= selectedAmount;
			if (currentItem.stack <= 0)
			{
				currentItem.TurnToAir();
			}
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
				packet.Write((byte)JarkosPacketType.MessageType_DepositMinorAltar);
				packet.Write((int)currentAltar.Position.X);
				packet.Write((int)currentAltar.Position.Y);
				Terraria.ModLoader.IO.ItemIO.Send(depositItem, packet, true);
				packet.Send();
			}
			else
			{
				currentAltar.SetItem(depositItem);
			}
			SoundEngine.PlaySound(SoundID.Grab);

			Close();
		}

		public void Close()
		{
			StopTyping();
			if (!_isClosing && Visible)
			{
				_isClosing = true;
			}
		}
	}
}
