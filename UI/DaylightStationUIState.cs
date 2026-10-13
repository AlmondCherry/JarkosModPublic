using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.core.PlayerInteract;
using jarkos.core.Data;
using jarkos.items.materials;
using jarkos.UI.Common;
using System.Linq;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.UI.Chat;
using jarkos.systems;
using Terraria.DataStructures;
using Terraria.Audio;

namespace jarkos.UI
{
	public class DaylightStationUIState : UIState
	{
		private class DaylightStationBarElement : UIElement
		{
			public DaylightStationTileEntity Entity;
			public float Opacity = 1f;
			public float CollectProgress = 1f;

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				if (Entity == null) return;
				CalculatedStyle dims = GetDimensions();

				Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle((int)dims.X - 2, (int)dims.Y - 2, (int)dims.Width + 4, (int)dims.Height + 4), new Color(40, 40, 40, (int)(200 * Opacity)));
				int total = Entity.GetTotalEssences();

				float currentY = dims.Y + dims.Height;
				float time = Main.GlobalTimeWrappedHourly;

				if (total > 0)
				{
					foreach (var kvp in Entity.StoredEssences)
					{
						if (kvp.Value <= 0) continue;

						float segmentHeight = dims.Height * ((float)kvp.Value / DaylightStationTileEntity.MaxCapacity) * CollectProgress;
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
						currentY = topY;
					}
				}
			}
		}

		private class DaylightStationFuelSlot : UIElement
		{
			public DaylightStationTileEntity Entity;
			public int SlotIndex;
			public float Opacity = 1f;

			public DaylightStationFuelSlot(int slotIndex)
			{
				SlotIndex = slotIndex;
			}

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				Color bgColor = IsMouseHovering ? new Color(120, 25, 40, (int)(240 * Opacity)) : new Color(65, 15, 25, (int)(220 * Opacity));
				Color borderColor = IsMouseHovering ? Color.Gold * Opacity : Color.Black * Opacity;

				var pixel = TextureAssets.MagicPixel.Value;
				spriteBatch.Draw(pixel, rect, bgColor);

				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), borderColor);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor);
				spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), borderColor);

				if (Entity == null) return;
				Item item = Entity.FuelSlots[SlotIndex];

				if (item != null && !item.IsAir)
				{
					Main.instance.LoadItem(item.type);

					Texture2D tex = TextureAssets.Item[item.type].Value;
					Rectangle itemFrame = tex.Frame(1, 1, 0, 0);

					float scale = 1f;

					if (itemFrame.Width > dims.Width - 10 || itemFrame.Height > dims.Height - 10)
					{
						scale = Math.Min((dims.Width - 10) / itemFrame.Width, (dims.Height - 10) / itemFrame.Height);
					}

					spriteBatch.Draw(tex, dims.Center(), itemFrame, Color.White * Opacity, 0f, itemFrame.Size() / 2f, scale, SpriteEffects.None, 0f);

					if (item.stack > 1)
					{
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, item.stack.ToString(), dims.Position() + new Vector2(8, dims.Height - 18), Color.White * Opacity, 0f, Vector2.Zero, new Vector2(0.8f));
					}

					if (IsMouseHovering)
					{
						string extra = item.type == ModContent.ItemType<CoolingFuel>() ? (string)DGLanguageSystem.DGcooling_fuel_extra : (item.type == ModContent.ItemType<SpectralFuel>() ? (string)DGLanguageSystem.DGspectral_fuel_extra : "");
						Main.hoverItemName = item.Name + extra;
					}
				}
				else if (IsMouseHovering)
				{
					Main.hoverItemName = (string)DGLanguageSystem.DGfuel_slot;
				}
			}
		}

		private class DaylightStationFuelBranchLines : UIElement
		{
			public DaylightStationTileEntity Entity;
			public DaylightStationFuelSlot Slot0;
			public DaylightStationFuelSlot Slot1;
			public float Opacity = 1f;

			public override bool ContainsPoint(Vector2 point)
			{
				return false;
			}

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				if (Entity == null || Slot0 == null || Slot1 == null) return;

				CalculatedStyle dims0 = Slot0.GetDimensions();
				CalculatedStyle dims1 = Slot1.GetDimensions();

				float slot0RightX = dims0.X + dims0.Width;
				float slot0CenterY = dims0.Y + dims0.Height / 2f;

				float slot1RightX = dims1.X + dims1.Width;
				float slot1CenterY = dims1.Y + dims1.Height / 2f;

				float xBranch = slot0RightX + 24f;
				float yMid = (slot0CenterY + slot1CenterY) / 2f;
				float xOut = xBranch + 20f;

				Color yellowColor = new Color(255, 225, 40);
				Color cyanColor = new Color(0, 215, 255);
				Color greyColor = new Color(110, 110, 110);

				Color GetSlotColor(Item item)
				{
					if (item != null && !item.IsAir)
					{
						if (item.type == ModContent.ItemType<CoolingFuel>())
							return yellowColor;
						if (item.type == ModContent.ItemType<SpectralFuel>())
							return cyanColor;
					}
					return greyColor;
				}

				Item item0 = Entity.FuelSlots[0];
				Item item1 = Entity.FuelSlots[1];

				Color color0 = GetSlotColor(item0);
				Color color1 = GetSlotColor(item1);

				Color colorMerged;
				bool has0 = item0 != null && !item0.IsAir;
				bool has1 = item1 != null && !item1.IsAir;

				if (has0 && has1)
				{
					if (color0 == color1) colorMerged = color0;
					else colorMerged = new Color(130, 235, 150);
				}

				else if (has0) colorMerged = color0;
				else if (has1) colorMerged = color1;
				else colorMerged = greyColor;

				Texture2D pixel = TextureAssets.MagicPixel.Value;

				Rectangle line0Rect = new Rectangle((int)slot0RightX, (int)(slot0CenterY - 1f), (int)(xBranch - slot0RightX), 2);
				spriteBatch.Draw(pixel, line0Rect, color0 * Opacity);

				Rectangle line1Rect = new Rectangle((int)slot1RightX, (int)(slot1CenterY - 1f), (int)(xBranch - slot1RightX), 2);
				spriteBatch.Draw(pixel, line1Rect, color1 * Opacity);

				Rectangle vertUpper = new Rectangle((int)(xBranch - 1f), (int)slot0CenterY, 2, (int)(yMid - slot0CenterY));
				spriteBatch.Draw(pixel, vertUpper, color0 * Opacity);

				Rectangle vertLower = new Rectangle((int)(xBranch - 1f), (int)yMid, 2, (int)(slot1CenterY - yMid));
				spriteBatch.Draw(pixel, vertLower, color1 * Opacity);

				Rectangle lineOutRect = new Rectangle((int)xBranch, (int)(yMid - 1f), (int)(xOut - xBranch), 2);
				spriteBatch.Draw(pixel, lineOutRect, colorMerged * Opacity);

				float speedMult = Entity.GetSpeedMultiplier();
				int bonusPercent = (int)((speedMult - 1f) * 100f + 0.5f);
				string speedText = bonusPercent > 0 ? $"+{bonusPercent}%" : "0%";

				Color textColor;
				if (bonusPercent > 0)
				{
					textColor = colorMerged != greyColor ? colorMerged : Color.Gold;
				}
				else
				{
					textColor = greyColor;
				}

				Vector2 textSize = FontAssets.MouseText.Value.MeasureString(speedText) * 0.85f;
				Vector2 textPos = new Vector2(xOut + 6f, yMid - textSize.Y / 2f);

				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, speedText, textPos, textColor * Opacity, 0f, Vector2.Zero, new Vector2(0.85f));
			}
		}
		private class DaylightStationInfoButton : UIElement
		{
			public float Opacity = 1f;

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				var fillTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle_Fill");
				var outlineTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle");
				var iconTex = ModContent.Request<Texture2D>("jarkos/UI/BookGuidePic");

				Color blueTint = IsMouseHovering ? new Color(0, 220, 255) * Opacity : new Color(0, 150, 220) * Opacity;

				if (fillTex.IsLoaded && outlineTex.IsLoaded)
				{
					spriteBatch.Draw(fillTex.Value, rect, blueTint);
					spriteBatch.Draw(outlineTex.Value, rect, Color.White * Opacity);
				}

				if (iconTex.IsLoaded)
				{
					Texture2D book = iconTex.Value;
					Rectangle bookFrame = book.Frame(1, 1, 0, 0);
					float scale = 1f;

					if (bookFrame.Width > dims.Width - 8 || bookFrame.Height > dims.Height - 8)
					{
						scale = Math.Min((dims.Width - 8) / bookFrame.Width, (dims.Height - 8) / bookFrame.Height);
					}

					spriteBatch.Draw(book, dims.Center(), bookFrame, Color.White * Opacity, 0f, bookFrame.Size() / 2f, scale, SpriteEffects.None, 0f);
				}
			}
		}

		private class DaylightStationPanel : UIElement
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

		private class DaylightStationStatusIndicator : UIElement
		{
			public DaylightStationTileEntity Entity;
			public float Opacity = 1f;

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				if (Entity == null) return;
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				bool operating = Entity.IsOperating;
				float pulse = operating ? (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.15f + 0.85f) : 1f;

				Color fillTint = operating ? new Color(50, 255, 60) : new Color(255, 45, 45);
				fillTint *= pulse * Opacity;

				var fillTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle_Fill");
				var outlineTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle");

				if (fillTex.IsLoaded && outlineTex.IsLoaded)
				{
					spriteBatch.Draw(fillTex.Value, rect, fillTint);
					spriteBatch.Draw(outlineTex.Value, rect, Color.White * Opacity);
				}

				if (IsMouseHovering)
				{
					bool alwaysWorks = Jarkos_config.Instance != null && Jarkos_config.Instance.DaylightAlwaysWorks;
					if (operating)
					{
						Main.hoverItemName = alwaysWorks ? (string)DGLanguageSystem.DGWorking_always : (string)DGLanguageSystem.DGWorking_on_day;
					}
					else
					{
						if (Main.dayTime && !alwaysWorks)
							Main.hoverItemName = (string)DGLanguageSystem.DGnot_work_day;
						else if (Entity.GetTotalEssences() >= DaylightStationTileEntity.MaxCapacity)
							Main.hoverItemName = (string)DGLanguageSystem.DGnot_work_fulled;
						else if (Entity.Position.Y >= Main.rockLayer)
							Main.hoverItemName = (string)DGLanguageSystem.DGnot_work_to_deep;
						else
							Main.hoverItemName = (string)DGLanguageSystem.DGnot_work;
					}
				}
			}
		}

		private class DaylightStationCloseButton : UIElement
		{
			public float Opacity = 1f;

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				var fillTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonSquare_Fill");
				var outlineTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonSquare");
				var crossTex = ModContent.Request<Texture2D>("jarkos/UI/CloseBtn_3");

				Color redTint = IsMouseHovering ? new Color(240, 50, 50) * Opacity : new Color(200, 40, 40) * Opacity;

				if (fillTex.IsLoaded && outlineTex.IsLoaded)
				{
					spriteBatch.Draw(fillTex.Value, rect, redTint);
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

		private class DaylightStationCollectButton : UIElement
		{
			public float Opacity = 1f;
			public string Text = "";

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				Color btnColor = IsMouseHovering ? new Color(60, 70, 110, (int)(230 * Opacity)) : new Color(30, 40, 75, (int)(210 * Opacity));
				Color borderColor = IsMouseHovering ? Color.Gold * Opacity : Color.Black * Opacity;

				var pixel = TextureAssets.MagicPixel.Value;
				spriteBatch.Draw(pixel, rect, btnColor);

				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), borderColor);
				spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor);
				spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), borderColor);

				if (!string.IsNullOrEmpty(Text))
				{
					float textScale = 0.9f;
					Vector2 textSize = FontAssets.MouseText.Value.MeasureString(Text) * textScale;
					Vector2 pos = dims.Position() + (new Vector2(dims.Width, dims.Height) - textSize) / 2f;
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch,FontAssets.MouseText.Value, Text, pos, Color.White * Opacity, 0f, Vector2.Zero, new Vector2(textScale));
				}
			}
		}

		private class DaylightStationIgnoreLaserButton : UIElement
		{
			public DaylightStationTileEntity Entity;
			public float Opacity = 1f;
			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				if (Entity == null) return;
				CalculatedStyle dim = GetDimensions();

				Color bgColor = Entity.IgnoreLaser ? new Color(100, 150, 100, (int)(200 * Opacity)) : new Color(150, 100, 100, (int)(200 * Opacity));

				if (IsMouseHovering) bgColor = Entity.IgnoreLaser ? new Color(130, 180, 130, (int)(200 * Opacity)) : new Color(180, 130, 130, (int)(200 * Opacity));
				Terraria.Utils.DrawInvBG(spriteBatch, dim.ToRectangle(), bgColor);

				string text = Entity.IgnoreLaser ? (string)DGLanguageSystem.DGignore_on : (string)DGLanguageSystem.DGignore_off;

				Terraria.Utils.DrawBorderString(spriteBatch, text, new Vector2(dim.X + dim.Width / 2f, dim.Y + dim.Height / 2f), Color.White * Opacity, 0.8f, 0.5f, 0.5f);
				if (IsMouseHovering)
				{
					Main.hoverItemName = LangSystem.DistillerIgnoreTooltip.Value;
				}
			}
		}

		private class DaylightStationProgressBar : UIElement
		{
			public DaylightStationTileEntity Entity;
			public float Opacity = 1f;

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				if (Entity == null) return;
				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4), new Color(40, 40, 40, (int)(200 * Opacity)));

				float progress = Entity.ProcessTimer / DaylightStationTileEntity.ReqTime;
				if (progress < 0f) progress = 0f;
				if (progress > 1f) progress = 1f;

				int maxFillHeight = rect.Height - 4;
				int totalFill = (int)(maxFillHeight * progress);

				if (totalFill > 0)
				{
					Color fillColor = new Color(0, 220, 255, (int)(220 * Opacity));
					int bottomY = rect.Y + rect.Height - 2;
					int bodyY = bottomY - totalFill;
					int bodyW = rect.Width - 4;

					spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X + 2, bodyY, bodyW, totalFill), fillColor);
					spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X + 2, bodyY, bodyW, 2), Color.White * 0.75f * Opacity);
				}

				if (IsMouseHovering)
				{
					int pct = (int)(progress * 100f);
					Main.hoverItemName = (string)DGLanguageSystem.DGprogress + " " + $"{pct}%";
				}
			}
		}

		private DaylightStationPanel _mainPanel;
		private DaylightStationStatusIndicator _statusIndicator;
		private DaylightStationInfoButton _infoButton;
		private DaylightStationBarElement _barContainer;
		private DaylightStationProgressBar _progressBar;
		private DaylightStationFuelSlot _fuelSlot0;
		private DaylightStationFuelSlot _fuelSlot1;
		private DaylightStationFuelBranchLines _fuelBranchLines;
		private DaylightStationCloseButton _closeButton;
		private DaylightStationCollectButton _collectButton;
		private DaylightStationIgnoreLaserButton _ignoreLaserButton;
		private Point16 _tePosition = new Point16(-1, -1);
		private DaylightStationTileEntity _entity => (_tePosition.X != -1 && TileEntity.ByPosition.TryGetValue(_tePosition, out TileEntity te) && te is DaylightStationTileEntity gte) ? gte : null;
		private Tween _scaleTween;
		private bool _isCollecting = false;
		private float _collectTimer = 0f;

		public void SetEntity(DaylightStationTileEntity entity)
		{
			_tePosition = entity != null ? entity.Position : new Point16(-1, -1);
			var ent = _entity;

			_barContainer.Entity = ent;

			if (_progressBar != null) _progressBar.Entity = ent;
			if (_statusIndicator != null) _statusIndicator.Entity = ent;

			if (_fuelSlot0 != null) _fuelSlot0.Entity = ent;
			if (_fuelSlot1 != null) _fuelSlot1.Entity = ent;

			if (_fuelBranchLines != null) _fuelBranchLines.Entity = ent;
			if (_ignoreLaserButton != null) _ignoreLaserButton.Entity = ent;

			_scaleTween.Set(0f);
			_scaleTween.To(1f);
		}

		public override void OnInitialize()
		{
			_scaleTween = new Tween(0.3f, Easing.EaseOutBack);

			_mainPanel = new DaylightStationPanel();
			_mainPanel.SetPadding(0);
			_mainPanel.Width.Set(430, 0f);
			_mainPanel.Height.Set(260, 0f);
			_mainPanel.HAlign = 0.5f;
			_mainPanel.VAlign = 0.5f;
			Append(_mainPanel);

			_statusIndicator = new DaylightStationStatusIndicator();
			_statusIndicator.Width.Set(24, 0f);
			_statusIndicator.Height.Set(24, 0f);
			_statusIndicator.Left.Set(20, 0f);
			_statusIndicator.Top.Set(52, 0f);
			_mainPanel.Append(_statusIndicator);

			_infoButton = new DaylightStationInfoButton();
			_infoButton.Width.Set(24, 0f);
			_infoButton.Height.Set(24, 0f);
			_infoButton.Left.Set(50, 0f);
			_infoButton.Top.Set(52, 0f);
			_mainPanel.Append(_infoButton);

			_ignoreLaserButton = new DaylightStationIgnoreLaserButton();
			_ignoreLaserButton.Width.Set(100, 0f);
			_ignoreLaserButton.Height.Set(26, 0f);
			_ignoreLaserButton.Left.Set(20, 0f);
			_ignoreLaserButton.Top.Set(20, 0f);
			_ignoreLaserButton.OnLeftClick += (evt, el) =>
			{
				if (_entity != null)
				{
					_entity.IgnoreLaser = !_entity.IgnoreLaser;
					_entity.SyncData();
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
			};
			_mainPanel.Append(_ignoreLaserButton);

			_closeButton = new DaylightStationCloseButton();
			_closeButton.Width.Set(36, 0f);
			_closeButton.Height.Set(36, 0f);
			_closeButton.HAlign = 0.96f;
			_closeButton.VAlign = 0.04f;
			_closeButton.OnLeftClick += (evt, el) => Close();
			_mainPanel.Append(_closeButton);

			_barContainer = new DaylightStationBarElement();
			_barContainer.Width.Set(48, 0f);
			_barContainer.Height.Set(150, 0f);
			_barContainer.HAlign = 0.33f;
			_barContainer.VAlign = 0.35f;
			_mainPanel.Append(_barContainer);

			_progressBar = new DaylightStationProgressBar();
			_progressBar.Width.Set(18, 0f);
			_progressBar.Height.Set(150, 0f);
			_progressBar.HAlign = 0.50f;
			_progressBar.VAlign = 0.35f;
			_mainPanel.Append(_progressBar);

			_fuelBranchLines = new DaylightStationFuelBranchLines();
			_fuelBranchLines.Width.Set(0, 1f);
			_fuelBranchLines.Height.Set(0, 1f);
			_mainPanel.Append(_fuelBranchLines);

			_fuelSlot0 = new DaylightStationFuelSlot(0);
			_fuelSlot0.Width.Set(50, 0f);
			_fuelSlot0.Height.Set(50, 0f);
			_fuelSlot0.HAlign = 0.68f;
			_fuelSlot0.VAlign = 0.20f;
			_fuelSlot0.OnLeftClick += (evt, el) => HandleFuelSlotClick(0);
			_mainPanel.Append(_fuelSlot0);

			_fuelSlot1 = new DaylightStationFuelSlot(1);
			_fuelSlot1.Width.Set(50, 0f);
			_fuelSlot1.Height.Set(50, 0f);
			_fuelSlot1.HAlign = 0.68f;
			_fuelSlot1.VAlign = 0.48f;
			_fuelSlot1.OnLeftClick += (evt, el) => HandleFuelSlotClick(1);
			_mainPanel.Append(_fuelSlot1);

			_fuelBranchLines.Slot0 = _fuelSlot0;
			_fuelBranchLines.Slot1 = _fuelSlot1;

			_collectButton = new DaylightStationCollectButton();
			_collectButton.Width.Set(150, 0f);
			_collectButton.Height.Set(36, 0f);
			_collectButton.HAlign = 0.45f;
			_collectButton.VAlign = 0.94f;
			_collectButton.OnLeftClick += (evt, el) => CollectEssences();
			_mainPanel.Append(_collectButton);
		}

		private void HandleFuelSlotClick(int slotIndex)
		{
			if (_entity == null) return;
			Item slotItem = _entity.FuelSlots[slotIndex];
			Item mouseItem = Main.mouseItem;

			if (mouseItem != null && !mouseItem.IsAir)
			{
				bool isFuel = mouseItem.type == ModContent.ItemType<CoolingFuel>() || mouseItem.type == ModContent.ItemType<SpectralFuel>();
				if (!isFuel) return;

				if (slotItem == null || slotItem.IsAir)
				{
					_entity.FuelSlots[slotIndex] = mouseItem.Clone();
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
					_entity.FuelSlots[slotIndex] = mouseItem.Clone();
					Main.mouseItem = slotItem.Clone();

					SoundEngine.PlaySound(SoundID.Grab);
				}
			}
			else if (slotItem != null && !slotItem.IsAir)
			{
				Main.mouseItem = slotItem.Clone();
				_entity.FuelSlots[slotIndex].TurnToAir();

				SoundEngine.PlaySound(SoundID.Grab);
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				_entity.SyncData();
			}
		}

		private void CollectEssences()
		{
			if (_entity == null || _entity.GetTotalEssences() == 0 || _isCollecting) return;
			_isCollecting = true;
			_collectTimer = 1.5f;
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (_entity == null) return;

			base.Draw(spriteBatch);
			DrawHoverTooltip(spriteBatch);
			DrawInfoTooltip(spriteBatch);
		}

		private void DrawInfoTooltip(SpriteBatch spriteBatch)
		{
			if (_infoButton == null || !_infoButton.IsMouseHovering) return;

			float opacity = _infoButton.Opacity;

			string titleText = (string)DGLanguageSystem.DSinfo_title;
			string introText = (string)DGLanguageSystem.DSinfo_intro;
			string sectionTitle = (string)DGLanguageSystem.DSinfo_section_title;
			string[] points = new string[]
			{
				(string)DGLanguageSystem.DSinfo_one,
				(string)DGLanguageSystem.DSinfo_two,
				(string)DGLanguageSystem.DSinfo_three,
				(string)DGLanguageSystem.DSinfo_four
			};

			float textScale = 0.78f;
			var font = FontAssets.MouseText.Value;
			int boxWidth = 420;
			float maxLineWidth = boxWidth - 24;

			List<ValueTuple<string, Color, bool>> renderedLines = new List<ValueTuple<string, Color, bool>>();

			renderedLines.Add((titleText, Color.Gold, true));

			List<string> introWrapped = WrapText(introText, textScale, maxLineWidth);
			foreach (var l in introWrapped) renderedLines.Add((l, Color.LightGray, false));
			renderedLines.Add(("", Color.White, false));

			renderedLines.Add((sectionTitle, Color.Gold, true));

			foreach (var pt in points)
			{
				List<string> ptWrapped = WrapText(pt, textScale, maxLineWidth);
				foreach (var l in ptWrapped) renderedLines.Add((l, Color.White, false));
				renderedLines.Add(("", Color.White, false));
			}

			int lineHeight = 18;
			int boxHeight = renderedLines.Count * lineHeight + 16;

			float boxX = Main.mouseX + 16;
			float boxY = Main.mouseY - boxHeight / 2f;

			if (boxX + boxWidth > Main.screenWidth - 10) boxX = Main.screenWidth - boxWidth - 10;
			if (boxX < 10) boxX = 10;
			if (boxY + boxHeight > Main.screenHeight - 10) boxY = Main.screenHeight - boxHeight - 10;
			if (boxY < 10) boxY = 10;

			Rectangle boxRect = new Rectangle((int)boxX, (int)boxY, boxWidth, boxHeight);
			Terraria.Utils.DrawInvBG(spriteBatch, boxRect, new Color(18, 20, 32, (int)(240 * opacity)));

			float currY = boxY + 10;
			foreach (var lineData in renderedLines)
			{
				if (string.IsNullOrEmpty(lineData.Item1))
				{
					currY += 6;
					continue;
				}

				Vector2 pos = new Vector2(boxX + 12, currY);
				float scale = lineData.Item3 ? 0.88f : textScale;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, lineData.Item1, pos, lineData.Item2 * opacity, 0f, Vector2.Zero, new Vector2(scale));

				currY += lineHeight;
			}
		}

		private static List<string> WrapText(string text, float scale, float maxLineWidth)
		{
			List<string> lines = new List<string>();
			var font = FontAssets.MouseText.Value;

			if (string.IsNullOrWhiteSpace(text)) return lines;

			string[] words = text.Split(' ');
			string currentLine = "";

			foreach (string word in words)
			{
				string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
				float lineW = font.MeasureString(testLine).X * scale;

				if (lineW > maxLineWidth && !string.IsNullOrEmpty(currentLine))
				{
					lines.Add(currentLine);
					currentLine = word;
				}
				else
				{
					currentLine = testLine;
				}
			}

			if (!string.IsNullOrEmpty(currentLine))
			{
				lines.Add(currentLine);
			}

			return lines;
		}

		private void DrawHoverTooltip(SpriteBatch spriteBatch)
		{
			if (_barContainer == null || _entity == null) return;
			CalculatedStyle dims = _barContainer.GetDimensions();
			Rectangle barRect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);
			if (barRect.Contains(Main.mouseX, Main.mouseY))
			{
				float opacity = _barContainer.Opacity;
				var activeEssences = _entity.StoredEssences.Where(kvp => kvp.Value > 0).ToList();

				if (activeEssences.Count > 0)
				{
					int rowHeight = 24;
					int padding = 8;
					int maxTextW = 0;

					foreach (var kvp in activeEssences)
					{
						string lineText = $"{EssencePalette.DisplayName(kvp.Key)} x{kvp.Value}";
						Vector2 sz = FontAssets.MouseText.Value.MeasureString(lineText) * 0.85f;
						
						if ((int)sz.X > maxTextW) maxTextW = (int)sz.X;
					}

					int boxWidth = maxTextW + 42;
					int boxHeight = activeEssences.Count * rowHeight + padding * 2;

					float boxX = Main.mouseX + 16;
					float boxY = Main.mouseY - boxHeight - 8;

					if (boxX + boxWidth > Main.screenWidth - 10) boxX = Main.screenWidth - boxWidth - 10;
					if (boxY < 10) boxY = Main.mouseY + 20;

					Rectangle boxRect = new Rectangle((int)boxX, (int)boxY, boxWidth, boxHeight);
					Terraria.Utils.DrawInvBG(spriteBatch, boxRect, new Color(20, 22, 30, (int)(230 * opacity)));

					float currRowY = boxY + padding;
					foreach (var kvp in activeEssences)
					{
						var tex = EssencePalette.GetTexture(kvp.Key);
						if (tex != null)
						{
							Rectangle iconDest = new Rectangle((int)boxX + 10, (int)currRowY + 2, 18, 18);
							spriteBatch.Draw(tex, iconDest, Color.White * opacity);
						}

						string text = $"{EssencePalette.DisplayName(kvp.Key)} x{kvp.Value}";
						Color col = EssencePalette.ColorOf(kvp.Key);
						Vector2 rowTextPos = new Vector2(boxX + 34, currRowY + 2);
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, rowTextPos, col * opacity, 0f, Vector2.Zero, new Vector2(0.85f));

						currRowY += rowHeight;
					}
				}
				else
				{
					Main.hoverItemName = (string)DGLanguageSystem.DGempty;
				}
			}
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

			if (_fuelSlot0 != null) _fuelSlot0.Entity = ent;
			if (_fuelSlot1 != null) _fuelSlot1.Entity = ent;
			if (_fuelBranchLines != null) _fuelBranchLines.Entity = ent;
			if (_ignoreLaserButton != null) _ignoreLaserButton.Entity = ent;

			base.Update(gameTime);

			if (_mainPanel != null && _mainPanel.ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
			}

			if (_isCollecting)
			{
				_collectTimer -= 0.016f;
				if (_collectTimer <= 0f)
				{
					_isCollecting = false;
					_collectTimer = 0f;
					if (_entity != null && _entity.GetTotalEssences() > 0)
					{
						var rp = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
						foreach (var kvp in _entity.StoredEssences)
						{
							rp.AddEssence(kvp.Key, kvp.Value);
						}
						_entity.ClearEssences();
						SoundEngine.PlaySound(SoundID.Item4);
					}
				}
			}

			if (_entity != null && _scaleTween != null && _scaleTween.Target != 0f)
			{
				Player player = Main.LocalPlayer;
				float distance = Vector2.Distance(player.Center, new Vector2(_entity.Position.X * 16 + 48, _entity.Position.Y * 16 + 32));

				if (distance > 200f)
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

				if (_barContainer != null)
				{
					_barContainer.Opacity = t;
					_barContainer.CollectProgress = _isCollecting ? (_collectTimer / 1.5f) : 1f;
				}

				if (_fuelSlot0 != null) _fuelSlot0.Opacity = t;
				if (_fuelSlot1 != null) _fuelSlot1.Opacity = t;

				if (_fuelBranchLines != null) _fuelBranchLines.Opacity = t;
				if (_infoButton != null) _infoButton.Opacity = t;

				if (_ignoreLaserButton != null) _ignoreLaserButton.Opacity = t;
				if (_closeButton != null) _closeButton.Opacity = t;

				if (_collectButton != null)
				{
					_collectButton.Opacity = t;
					_collectButton.Text = _isCollecting ? (string)DGLanguageSystem.DGcollecting : LangSystem.CollectButtonText.Value;
				}
				if (_scaleTween.Target == 0f && !_scaleTween.Active)
				{
					DaylightStationUISystem.Instance.ForceClose();
				}
			}
		}

		public void Close()
		{
			if (_isCollecting)
			{
				_isCollecting = false;
				if (_entity != null && _entity.GetTotalEssences() > 0)
				{
					var rp = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
					foreach (var kvp in _entity.StoredEssences)
					{
						rp.AddEssence(kvp.Key, kvp.Value);
					}
					_entity.ClearEssences();
				}
			}
			_scaleTween.To(0f);
		}
	}
}
