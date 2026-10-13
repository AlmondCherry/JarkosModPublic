using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.core.Data;
using jarkos.UI.Common;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.Localization;

namespace jarkos.UI
{
	public class ReplicatorUIState : UIState
	{
		private class ReplicatorBarElement : UIElement
		{
			public ReplicatorTileEntity Entity;
			public float Opacity = 1f;

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				if (Entity == null) return;

				CalculatedStyle dims = GetDimensions();

				Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle((int)dims.X - 2, (int)dims.Y - 2, (int)dims.Width + 4, (int)dims.Height + 4), new Color(40, 40, 40, (int)(200 * Opacity)));
				int total = Entity.GetTotalEssences();

				float currentY = dims.Y + dims.Height;
				EssenceType? hoveredEssence = null;

				int hoveredAmount = 0;
				float time = Main.GlobalTimeWrappedHourly;

				if (total > 0)
				{
					foreach (var kvp in Entity.StoredEssences)
					{
						if (kvp.Value <= 0) continue;

						float segmentHeight = dims.Height * ((float)kvp.Value / ReplicatorTileEntity.MaxCapacity);

						float topY = currentY - segmentHeight;
						float bottomY = currentY;

						Color baseColor = EssencePalette.ColorOf(kvp.Key) * Opacity;

						if (Jarkos_config.StaticEssenceSections)
						{
							int sliceTop = (int)topY;
							int sliceBottom = (int)bottomY;

							if (sliceTop < dims.Y) sliceTop = (int)dims.Y;
							if (sliceBottom > dims.Y + dims.Height) sliceBottom = (int)(dims.Y + dims.Height);

							int sliceHeight = sliceBottom - sliceTop;

							if (sliceHeight > 0)
							{
								Rectangle fillRect = new Rectangle((int)dims.X, sliceTop, (int)dims.Width, sliceHeight);

								spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, baseColor);
								spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dims.X, sliceTop, (int)dims.Width, Math.Min(2, sliceHeight)), Color.White * 0.25f * Opacity);
							}
						}
						else
						{
							for (int x = (int)dims.X; x < (int)dims.X + (int)dims.Width; x += 2)
							{
								int sliceW = Math.Min(2, (int)(dims.X + dims.Width) - x);

								float topWave = (float)(Math.Sin(x * 0.12f + topY * 0.05f + time * 3f) * 2.5f + Math.Cos(x * 0.07f - time * 1.5f) * 1.5f);
								float bottomWave = (float)(Math.Sin(x * 0.12f + bottomY * 0.05f + time * 3f) * 2.5f + Math.Cos(x * 0.07f - time * 1.5f) * 1.5f);

								if (bottomY >= dims.Y + dims.Height - 0.5f)
									bottomWave = 0;

								int sliceTop = (int)(topY + topWave);
								int sliceBottom = (int)(bottomY + bottomWave);

								if (sliceTop < dims.Y) sliceTop = (int)dims.Y;
								if (sliceBottom > dims.Y + dims.Height) sliceBottom = (int)(dims.Y + dims.Height);

								int sliceHeight = sliceBottom - sliceTop;
								
								if (sliceHeight > 0)
								{
									Rectangle slice = new Rectangle(x, sliceTop, sliceW, sliceHeight);
									spriteBatch.Draw(TextureAssets.MagicPixel.Value, slice, baseColor);
									spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x, sliceTop, sliceW, Math.Min(2, sliceHeight)), Color.White * 0.25f * Opacity);
								}
							}
						}

						Rectangle hoverRect = new Rectangle((int)dims.X, (int)topY, (int)dims.Width, (int)segmentHeight);
						if (hoverRect.Contains(Main.mouseX, Main.mouseY))
						{
							hoveredEssence = kvp.Key;
							hoveredAmount = kvp.Value;
						}
						currentY = topY;
					}
				}

				if (hoveredEssence.HasValue)
				{
					string text = $"{EssencePalette.DisplayName(hoveredEssence.Value)}: {hoveredAmount}";
					Main.hoverItemName = text;
					var tex = EssencePalette.GetTexture(hoveredEssence.Value);
					if (tex != null)
					{
						spriteBatch.Draw(tex, new Vector2(Main.mouseX + 20, Main.mouseY + 20), Color.White * Opacity);
					}
				}
			}
		}

		private class ReplicatorItemSlot : UIElement
		{
			public ReplicatorTileEntity Entity;
			public int SlotIndex; 
			public float Opacity = 1f;

			public ReplicatorItemSlot(int slotIndex)
			{
				SlotIndex = slotIndex;
			}

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				CalculatedStyle dims = GetDimensions();

				Color bgColor = SlotIndex == 0 ? new Color(140, 140, 180, (int)(160 * Opacity)) : new Color(160, 130, 170, (int)(160 * Opacity));
				Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height), bgColor);

				if (Entity == null) return;
				Item item = SlotIndex == 0 ? Entity.InputSlot : Entity.OutputSlots[SlotIndex - 1];

				if (item != null && !item.IsAir)
				{
					Main.instance.LoadItem(item.type);
					Texture2D tex = TextureAssets.Item[item.type].Value;

					Rectangle rect = tex.Frame(1, 1, 0, 0);
					float scale = 1f;

					if (rect.Width > dims.Width - 10 || rect.Height > dims.Height - 10)
					{
						scale = Math.Min((dims.Width - 10) / rect.Width, (dims.Height - 10) / rect.Height);
					}

					spriteBatch.Draw(tex, dims.Center(), rect, Color.White * Opacity, 0f, rect.Size() / 2f, scale, SpriteEffects.None, 0f);

					if (item.stack > 1)
					{
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, item.stack.ToString(), dims.Position() + new Vector2(8, dims.Height - 18), Color.White * Opacity, 0f, Vector2.Zero, new Vector2(0.8f));
					}

					if (IsMouseHovering)
					{
						Main.hoverItemName = item.Name;
					}
				}
				else if (IsMouseHovering)
				{
					Main.hoverItemName = SlotIndex == 0 ? (string)ReplicatorLanguageSystem.R_inputslot : (string)ReplicatorLanguageSystem.R_outputslot;
				}
			}
		}

		private class ReplicatorPanel : UIElement
		{
			public float Opacity = 1f;
			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				var tex1 = ModContent.Request<Texture2D>("jarkos/UI/Textures/DistillatorUI_1");
				var tex2 = ModContent.Request<Texture2D>("jarkos/UI/Textures/DistillatorUI_2");
				var tex3 = ModContent.Request<Texture2D>("jarkos/UI/Textures/DistillatorUI_3");

				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				if (tex1.IsLoaded && tex2.IsLoaded && tex3.IsLoaded)
				{
					spriteBatch.Draw(tex2.Value, rect, new Color(40, 40, 40, (int)(240 * Opacity)));
					spriteBatch.End();

					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
					spriteBatch.Draw(tex3.Value, rect, new Color(150, 150, 150, (int)(200 * Opacity)));
					spriteBatch.End();

					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
					spriteBatch.Draw(tex1.Value, rect, Color.White * Opacity);
				}
			}
		}

		private ReplicatorPanel _mainPanel;
		private ReplicatorBarElement _barContainer;
		private ReplicatorItemSlot _inputSlot;
		private ReplicatorItemSlot _outputSlot0;
		private ReplicatorItemSlot _outputSlot1;
		private UIPanel _closeButton;
		private UIText _closeText;
		private UIPanel _cloneButton;
		private UIText _cloneText;

		private Point16 _tePosition = new Point16(-1, -1);
		private ReplicatorTileEntity _entity => (_tePosition.X != -1 && TileEntity.ByPosition.TryGetValue(_tePosition, out TileEntity te) && te is ReplicatorTileEntity rte) ? rte : null;
		private Tween _scaleTween;

		public void SetEntity(ReplicatorTileEntity entity)
		{
			_tePosition = entity != null ? entity.Position : new Point16(-1, -1);
			var ent = _entity;

			_barContainer.Entity = ent;

			if (_inputSlot != null) _inputSlot.Entity = ent;
			if (_outputSlot0 != null) _outputSlot0.Entity = ent;
			if (_outputSlot1 != null) _outputSlot1.Entity = ent;

			_scaleTween.Set(0f);
			_scaleTween.To(1f);
		}

		public override void OnInitialize()
		{
			_scaleTween = new Tween(0.3f, Easing.EaseOutBack);
			_mainPanel = new ReplicatorPanel();
			_mainPanel.SetPadding(0);
			_mainPanel.Width.Set(420, 0f);
			_mainPanel.Height.Set(260, 0f);
			_mainPanel.HAlign = 0.5f;
			_mainPanel.VAlign = 0.5f;
			Append(_mainPanel);

			_closeButton = new UIPanel();
			_closeButton.Width.Set(30, 0f);
			_closeButton.Height.Set(30, 0f);
			_closeButton.HAlign = 0.95f;
			_closeButton.VAlign = 0.05f;
			_closeButton.BackgroundColor = new Color(200, 40, 40, 180);
			_closeButton.OnLeftClick += (evt, el) => Close();
			_mainPanel.Append(_closeButton);

			_closeText = new UIText("X", 0.9f, true);
			_closeText.HAlign = 0.5f;
			_closeText.VAlign = 0.5f;
			_closeButton.Append(_closeText);

			_inputSlot = new ReplicatorItemSlot(0);
			_inputSlot.Width.Set(56, 0f);
			_inputSlot.Height.Set(56, 0f);
			_inputSlot.HAlign = 0.12f;
			_inputSlot.VAlign = 0.40f;
			_inputSlot.OnLeftClick += (evt, el) => HandleSlotClick(0);
			_mainPanel.Append(_inputSlot);

			_barContainer = new ReplicatorBarElement();
			_barContainer.Width.Set(46, 0f);
			_barContainer.Height.Set(140, 0f);
			_barContainer.HAlign = 0.5f;
			_barContainer.VAlign = 0.35f;
			_mainPanel.Append(_barContainer);

			_outputSlot0 = new ReplicatorItemSlot(1);
			_outputSlot0.Width.Set(52, 0f);
			_outputSlot0.Height.Set(52, 0f);
			_outputSlot0.HAlign = 0.88f;
			_outputSlot0.VAlign = 0.22f;
			_outputSlot0.OnLeftClick += (evt, el) => HandleSlotClick(1);
			_mainPanel.Append(_outputSlot0);

			_outputSlot1 = new ReplicatorItemSlot(2);
			_outputSlot1.Width.Set(52, 0f);
			_outputSlot1.Height.Set(52, 0f);
			_outputSlot1.HAlign = 0.88f;
			_outputSlot1.VAlign = 0.56f;
			_outputSlot1.OnLeftClick += (evt, el) => HandleSlotClick(2);
			_mainPanel.Append(_outputSlot1);

			_cloneButton = new UIPanel();
			_cloneButton.Width.Set(150, 0f);
			_cloneButton.Height.Set(32, 0f);
			_cloneButton.HAlign = 0.5f;
			_cloneButton.VAlign = 0.88f;
			_cloneButton.BackgroundColor = new Color(140, 80, 180, 180);
			_cloneButton.OnLeftClick += (evt, el) => TryStartCloning();
			_mainPanel.Append(_cloneButton);

			_cloneText = new UIText("Clone", 0.9f);
			HAlign = 0.5f;
			VAlign = 0.5f;
			_cloneButton.Append(_cloneText);
		}

		private void HandleSlotClick(int slotIndex)
		{
			if (_entity == null) return;

			if (slotIndex == 0)
			{
				if (_entity.IsCloning) return;

				Item slotItem = _entity.InputSlot;
				Item mouseItem = Main.mouseItem;

				if (mouseItem != null && !mouseItem.IsAir)
				{
					if (slotItem == null || slotItem.IsAir)
					{
						_entity.InputSlot = mouseItem.Clone();
						Main.mouseItem.TurnToAir();

						SoundEngine.PlaySound(SoundID.Grab);
					}
					else if (slotItem.type == mouseItem.type && slotItem.stack < slotItem.maxStack)
					{
						int space = slotItem.maxStack - slotItem.stack;
						int toAdd = Math.Min(space, mouseItem.stack);

						slotItem.stack += toAdd;
						mouseItem.stack -= toAdd;

						if (mouseItem.stack <= 0) Main.mouseItem.TurnToAir();
						SoundEngine.PlaySound(SoundID.Grab);
					}
					else
					{
						_entity.InputSlot = mouseItem.Clone();
						Main.mouseItem = slotItem.Clone();

						SoundEngine.PlaySound(SoundID.Grab);
					}
				}
				else if (slotItem != null && !slotItem.IsAir)
				{
					Main.mouseItem = slotItem.Clone();
					_entity.InputSlot.TurnToAir();

					SoundEngine.PlaySound(SoundID.Grab);
				}
			}
			else
			{
				int outIndex = slotIndex - 1;

				Item slotItem = _entity.OutputSlots[outIndex];
				Item mouseItem = Main.mouseItem;

				if (mouseItem != null && !mouseItem.IsAir)
				{
					if (slotItem != null && !slotItem.IsAir && slotItem.type == mouseItem.type && mouseItem.stack < mouseItem.maxStack)
					{
						int space = mouseItem.maxStack - mouseItem.stack;
						int toAdd = Math.Min(space, slotItem.stack);

						mouseItem.stack += toAdd;
						slotItem.stack -= toAdd;

						if (slotItem.stack <= 0) slotItem.TurnToAir();

						SoundEngine.PlaySound(SoundID.Grab);
					}
				}
				else if (slotItem != null && !slotItem.IsAir)
				{
					Main.mouseItem = slotItem.Clone();
					_entity.OutputSlots[outIndex].TurnToAir();

					SoundEngine.PlaySound(SoundID.Grab);
				}
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				_entity.SyncData();
			}
		}

		private void TryStartCloning()
		{
			if (_entity == null) return;
			if (_entity.IsCloning) return;

			if (_entity.CanStartCloning())
			{
				_entity.StartCloning();
				SoundEngine.PlaySound(SoundID.Item4);
			}
			else
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (_entity == null) return;
			base.Draw(spriteBatch);
		}

		public override void Update(GameTime gameTime)
		{
			var ent = _entity;
			if (ent == null)
			{
				Close();
				return;
			}
			_barContainer.Entity = ent;

			if (_inputSlot != null) _inputSlot.Entity = ent;

			if (_outputSlot0 != null) _outputSlot0.Entity = ent;
			if (_outputSlot1 != null) _outputSlot1.Entity = ent;

			base.Update(gameTime);

			if (_mainPanel != null && _mainPanel.ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
			}

			if (ent != null && _cloneButton != null && _cloneText != null)
			{
				if (ent.IsCloning)
				{
					float progress = (float)ent.CloneTimer / ReplicatorTileEntity.CloneReqTime;

					if (progress > 1f) progress = 1f;
					int percent = (int)(progress * 100f);

					_cloneText.SetText((string)ReplicatorLanguageSystem.R_cloning + $" {percent}%");
					_cloneButton.BackgroundColor = new Color(100, 100, 100, (int)(180 * _scaleTween.Value));
				}
				else if (!ent.AreOutputSlotsEmpty())
				{
					_cloneText.SetText((string)ReplicatorLanguageSystem.R_takeitems);
					_cloneButton.BackgroundColor = new Color(100, 100, 100, (int)(180 * _scaleTween.Value));
				}
				else
				{
					_cloneText.SetText((string)ReplicatorLanguageSystem.R_clonebychitinus);
					bool canClone = ent.CanStartCloning();
					_cloneButton.BackgroundColor = canClone ? new Color(140, 80, 180, (int)(180 * _scaleTween.Value)) : new Color(80, 60, 100, (int)(180 * _scaleTween.Value));
				}
			}

			if (_entity != null && _scaleTween != null && _scaleTween.Target != 0f)
			{
				Player player = Main.LocalPlayer;
				float distance = Vector2.Distance(player.Center, new Vector2(_entity.Position.X * 16 + 24, _entity.Position.Y * 16 + 24));
				if (distance > 180f)
				{
					Close();
				}
			}

			if (_scaleTween != null)
			{
				_scaleTween.Update(0.016f);

				float t = _scaleTween.Value;
				float easedT = _scaleTween.Eased;

				_mainPanel.Top.Set((1f - easedT) * 100f, 0f);
				_mainPanel.Recalculate();

				if (_mainPanel != null) _mainPanel.Opacity = t;
				if (_barContainer != null) _barContainer.Opacity = t;

				if (_inputSlot != null) _inputSlot.Opacity = t;
				if (_outputSlot0 != null) _outputSlot0.Opacity = t;
				if (_outputSlot1 != null) _outputSlot1.Opacity = t;

				if (_closeButton != null)
				{
					_closeButton.BackgroundColor = new Color(200, 40, 40, (int)(180 * t));
					_closeButton.BorderColor = Color.Black * t;
				}
				if (_closeText != null) _closeText.TextColor = Color.White * t;
				if (_cloneButton != null)
				{
					_cloneButton.BorderColor = Color.Black * t;
				}
				if (_cloneText != null)
				{
					_cloneText.TextColor = Color.White * t;
				}
				if (_scaleTween.Target == 0f && !_scaleTween.Active)
				{
					ReplicatorUISystem.Instance.ForceClose();
				}
			}
		}

		public void Close()
		{
			_scaleTween.To(0f);
		}
	}

	public class ReplicatorLanguageSystem : ModSystem
	{
		public static LocalizedText R_inputslot { get; private set; }
		public static LocalizedText R_outputslot { get; private set; }
        public static LocalizedText R_clonebtn { get; private set; }
        public static LocalizedText R_cloning { get; private set; }
        public static LocalizedText R_takeitems { get; private set; }
		public static LocalizedText R_clonebychitinus { get; private set; }

        public override void Load()
        {
            string category = "GoblinRegorgeRework";
			R_inputslot ??= Mod.GetLocalization($"{category}.R_inputslot");
			R_outputslot ??= Mod.GetLocalization($"{category}.R_outputslot");
			R_clonebtn ??= Mod.GetLocalization($"{category}.R_clonebtn");
			R_cloning ??= Mod.GetLocalization($"{category}.R_cloning");
			R_takeitems ??= Mod.GetLocalization($"{category}.R_takeitems");
			R_clonebychitinus ??= Mod.GetLocalization($"{category}.R_clonebychitinus");
        }
	}
}
