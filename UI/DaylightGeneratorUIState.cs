using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.core.PlayerInteract;
using jarkos.core.Data;
using jarkos.UI.Common;
using Terraria.ID;
using Terraria.GameContent;
using jarkos.systems;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;

namespace jarkos.UI
{
	public class DaylightGeneratorUIState : UIState
	{
		private class DaylightGeneratorIgnoreLaserButton : UIElement
        {
            public DaylightGeneratorTileEntity Entity;
            public float Opacity = 1f;
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (Entity == null) return;
                CalculatedStyle dim = GetDimensions();

                Color bgColor = Entity.IgnoreLaser ? new Color(100, 150, 100, (int)(200 * Opacity)) : new Color(150, 100, 100, (int)(200 * Opacity));

                if (IsMouseHovering) bgColor = Entity.IgnoreLaser ? new Color(130, 180, 130, (int)(200 * Opacity)) : new Color(180, 130, 130, (int)(200 * Opacity));
                Terraria.Utils.DrawInvBG(spriteBatch, dim.ToRectangle(), bgColor);

                string text = Entity.IgnoreLaser ? "Ignore: ON" : "Ignore: OFF";
                
                Terraria.Utils.DrawBorderString(spriteBatch, text, new Vector2(dim.X + dim.Width / 2f, dim.Y + dim.Height / 2f), Color.White * Opacity, 0.8f, 0.5f, 0.5f);
                if (IsMouseHovering)
                {
                    Main.hoverItemName = LangSystem.DistillerIgnoreTooltip.Value;
                }
            }
        }
		private class DaylightGeneratorBarElement : UIElement
		{
			public DaylightGeneratorTileEntity Entity;
			public float Opacity = 1f;
			public float CollectProgress = 1f;

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				if (Entity == null) return;
				CalculatedStyle dims = GetDimensions();
				Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle((int)dims.X - 2, (int)dims.Y - 2, (int)dims.Width + 4, (int)dims.Height + 4), new Color(40, 40, 40, (int)(200 * Opacity)));
				int total = Entity.GetTotalEssences();
				if (total == 0) return;

				float currentY = dims.Y + dims.Height;
				EssenceType? hoveredEssence = null;
				int hoveredAmount = 0;
				float time = Main.GlobalTimeWrappedHourly;

				foreach (var kvp in Entity.StoredEssences)
				{
					if (kvp.Value <= 0) continue;

					float segmentHeight = dims.Height * ((float)kvp.Value / DaylightGeneratorTileEntity.MaxCapacity) * CollectProgress;
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

		private class DaylightGeneratorPanel : UIElement
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

		private class DaylightGeneratorProgressBar : UIElement
		{
			public DaylightGeneratorTileEntity Entity;
			public float Opacity = 1f;

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				if (Entity == null) return;

				CalculatedStyle dims = GetDimensions();
				Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

				Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4), new Color(40, 40, 40, (int)(200 * Opacity)));

				float progress = Entity.ProcessTimer / DaylightGeneratorTileEntity.ReqTime;

				if (progress < 0f) progress = 0f;
				if (progress > 1f) progress = 1f;

				int maxFillHeight = rect.Height - 4;
				int totalFill = (int)(maxFillHeight * progress);

				if (totalFill > 0)
				{
					Color fillColor = new Color(255, 200, 50, (int)(220 * Opacity));

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

		private class DaylightGeneratorStatusIndicator : UIElement
		{
			public DaylightGeneratorTileEntity Entity;
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
						if (!Main.dayTime && !alwaysWorks)
							Main.hoverItemName = (string)DGLanguageSystem.DGnot_work_day;
						else if (Entity.GetTotalEssences() >= DaylightGeneratorTileEntity.MaxCapacity)
							Main.hoverItemName = (string)DGLanguageSystem.DGnot_work_fulled;
						else if (Entity.Position.Y >= Main.rockLayer)
							Main.hoverItemName = (string)DGLanguageSystem.DGnot_work_to_deep;
						else
							Main.hoverItemName = (string)DGLanguageSystem.DGnot_work;
					}
				}
			}
		}

		private DaylightGeneratorPanel _mainPanel;
		private DaylightGeneratorStatusIndicator _statusIndicator;
		private DaylightGeneratorBarElement _barContainer;
		private DaylightGeneratorProgressBar _progressBar;
		private DaylightGeneratorIgnoreLaserButton _ignoreLaserButton;
		private UIPanel _closeButton;
		private UIText _closeText;
		private UIPanel _collectButton;
		private UIText _collectText;
		private Point16 _tePosition = new Point16(-1, -1);
		private DaylightGeneratorTileEntity _entity => (_tePosition.X != -1 && TileEntity.ByPosition.TryGetValue(_tePosition, out TileEntity te) && te is DaylightGeneratorTileEntity gte) ? gte : null;
		private Tween _scaleTween;
		private bool _isCollecting = false;
		private float _collectTimer = 0f;

		public void SetEntity(DaylightGeneratorTileEntity entity)
		{
			_tePosition = entity != null ? entity.Position : new Point16(-1, -1);
			var ent = _entity;
			_barContainer.Entity = ent;
			if (_progressBar != null) _progressBar.Entity = ent;
			if (_statusIndicator != null) _statusIndicator.Entity = ent;
			if (_ignoreLaserButton != null) _ignoreLaserButton.Entity = ent;
			_scaleTween.Set(0f);
			_scaleTween.To(1f);
		}

		public override void OnInitialize()
		{
			_scaleTween = new Tween(0.3f, Easing.EaseOutBack);
			_mainPanel = new DaylightGeneratorPanel();
			_mainPanel.SetPadding(0);
			_mainPanel.Width.Set(360, 0f);
			_mainPanel.Height.Set(240, 0f);
			_mainPanel.HAlign = 0.5f;
			_mainPanel.VAlign = 0.5f;
			Append(_mainPanel);

			_statusIndicator = new DaylightGeneratorStatusIndicator();
			_statusIndicator.Width.Set(24, 0f);
			_statusIndicator.Height.Set(24, 0f);
			_statusIndicator.Left.Set(20, 0f);
			_statusIndicator.Top.Set(52, 0f);
			_mainPanel.Append(_statusIndicator);

			_ignoreLaserButton = new DaylightGeneratorIgnoreLaserButton();
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

			_barContainer = new DaylightGeneratorBarElement();
			_barContainer.Width.Set(44, 0f);
			_barContainer.Height.Set(140, 0f);
			_barContainer.HAlign = 0.42f;
			_barContainer.VAlign = 0.35f;
			_mainPanel.Append(_barContainer);

			_progressBar = new DaylightGeneratorProgressBar();
			_progressBar.Width.Set(18, 0f);
			_progressBar.Height.Set(140, 0f);
			_progressBar.HAlign = 0.58f;
			_progressBar.VAlign = 0.35f;
			_mainPanel.Append(_progressBar);

			_collectButton = new UIPanel();
			_collectButton.Width.Set(130, 0f);
			_collectButton.Height.Set(32, 0f);
			_collectButton.HAlign = 0.5f;
			_collectButton.VAlign = 0.88f;
			_collectButton.BackgroundColor = new Color(200, 40, 40, 180);
			_collectButton.OnLeftClick += (evt, el) => CollectEssences();
			_mainPanel.Append(_collectButton);

			_collectText = new UIText(LangSystem.CollectButtonText, 0.9f);
			_collectText.HAlign = 0.5f;
			_collectText.VAlign = 0.5f;
			_collectButton.Append(_collectText);
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
				float distance = Vector2.Distance(player.Center, new Vector2(_entity.Position.X * 16 + 16, _entity.Position.Y * 16 + 16));
				if (distance > 160f)
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
				if (_ignoreLaserButton != null) _ignoreLaserButton.Opacity = t;
				if (_closeButton != null)
				{
					_closeButton.BackgroundColor = new Color(200, 40, 40, (int)(180 * t));
					_closeButton.BorderColor = Color.Black * t;
				}
				if (_closeText != null) _closeText.TextColor = Color.White * t;
				if (_collectButton != null)
				{
					_collectButton.BackgroundColor = _isCollecting ? new Color(100, 100, 100, (int)(180 * t)) : new Color(200, 40, 40, (int)(180 * t));
					_collectButton.BorderColor = Color.Black * t;
				}
				if (_collectText != null)
				{
					_collectText.TextColor = Color.White * t;
					_collectText.SetText(_isCollecting ? (string)DGLanguageSystem.DGcollecting : LangSystem.CollectButtonText.Value);
				}
				if (_scaleTween.Target == 0f && !_scaleTween.Active)
				{
					DaylightGeneratorUISystem.Instance.ForceClose();
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

	public class DGLanguageSystem : ModSystem
	{
		public static LocalizedText DGWorking_always { get; private set; }
		public static LocalizedText DGWorking_on_day { get; private set; }


		public static LocalizedText DGnot_work_fulled { get; private set; }
		public static LocalizedText DGnot_work_day { get; private set; }
		public static LocalizedText DGnot_work_to_deep { get; private set; }
		public static LocalizedText DGnot_work { get; private set; }

		public static LocalizedText DGempty { get; private set; }
		public static LocalizedText DGcollecting { get; private set; }
		public static LocalizedText DGspectral_fuel_extra { get; private set; }
		public static LocalizedText DGcooling_fuel_extra { get; private set; }
		public static LocalizedText DGfuel_slot { get; private set; }
		public static LocalizedText DGprogress { get; private set; }
		public static LocalizedText DGignore_off { get; private set; }
		public static LocalizedText DGignore_on { get; private set; }


		public static LocalizedText DSinfo_title { get; private set; }
		public static LocalizedText DSinfo_intro { get; private set; }
		public static LocalizedText DSinfo_section_title { get; private set; }
		public static LocalizedText DSinfo_one { get; private set; }
		public static LocalizedText DSinfo_two { get; private set; }
		public static LocalizedText DSinfo_three { get; private set; }
		public static LocalizedText DSinfo_four { get; private set; }
		public static LocalizedText DSinfo_five { get; private set; }
        public override void Load()
        {
            string category = "UI";
			DGWorking_always ??= Mod.GetLocalization($"{category}.DGWorking_always");
			DGWorking_on_day ??= Mod.GetLocalization($"{category}.DGWorking_on_day");

			DGnot_work_fulled ??= Mod.GetLocalization($"{category}.DGnot_work_fulled");
			DGnot_work_day ??= Mod.GetLocalization($"{category}.DGnot_work_day");
			DGnot_work_to_deep ??= Mod.GetLocalization($"{category}.DGnot_work_to_deep");
			DGnot_work ??= Mod.GetLocalization($"{category}.DGnot_work");

			DGempty ??= Mod.GetLocalization($"{category}.DGempty");
			DGcollecting ??= Mod.GetLocalization($"{category}.DGcollecting");
			DGspectral_fuel_extra ??= Mod.GetLocalization($"{category}.DGspectral_fuel_extra");
			DGcooling_fuel_extra ??= Mod.GetLocalization($"{category}.DGcooling_fuel_extra");
			DGfuel_slot ??= Mod.GetLocalization($"{category}.DGfuel_slot");
			DGprogress ??= Mod.GetLocalization($"{category}.DGprogress");
			DGignore_off ??= Mod.GetLocalization($"{category}.DGignore_off");
			DGignore_on ??= Mod.GetLocalization($"{category}.DGignore_on");

			DSinfo_title ??= Mod.GetLocalization($"{category}.DSinfo_title");
			DSinfo_intro ??= Mod.GetLocalization($"{category}.DSinfo_intro");
			DSinfo_section_title ??= Mod.GetLocalization($"{category}.DSinfo_section_title");
			DSinfo_one ??= Mod.GetLocalization($"{category}.DSinfo_one");
			DSinfo_two ??= Mod.GetLocalization($"{category}.DSinfo_two");
			DSinfo_three ??= Mod.GetLocalization($"{category}.DSinfo_three");
			DSinfo_four ??= Mod.GetLocalization($"{category}.DSinfo_four");
			DSinfo_five ??= Mod.GetLocalization($"{category}.DSinfo_five");
        }
	}
}
