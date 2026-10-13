using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using jarkos.npc;
using Terraria.Localization;

namespace jarkos.UI
{
	public class DummyUIState : UIState
	{
		private bool _isDraggingSlider = false;
		private float _animProgress = 0f;
		private float _targetAnimProgress = 1f;
		private float _currentPanelHeight = 65f;
		private float _removeBtnAnim = 0f;

		private bool _isDetailedOpen = false;
		private float _detailedAnim = 0f;
		private int _draggingDetailedSlider = -1;

		public bool IsClosing => _targetAnimProgress <= 0f;

		public override void OnInitialize()
		{
			Width.Set(680f, 0f);
			Height.Set(400f, 0f);
			HAlign = 0.5f;
			Top.Set(35f, 0f);
		}

		public void ResetAnimation()
		{
			_animProgress = 0f;
			_targetAnimProgress = 1f;
			bool hasDummies = AnyDummyActive();
			_removeBtnAnim = hasDummies ? 1f : 0f;
			float baseHeight = DummyUISystem.IsArmored ? 120f : 65f;
			_currentPanelHeight = baseHeight + 45f * _removeBtnAnim;
			_isDraggingSlider = false;
			_isDetailedOpen = false;
			_detailedAnim = 0f;
			_draggingDetailedSlider = -1;
		}

		public void StartOpening()
		{
			_targetAnimProgress = 1f;
		}

		public void StartClosing()
		{
			_targetAnimProgress = 0f;
			_isDraggingSlider = false;
			_draggingDetailedSlider = -1;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);


			_animProgress = MathHelper.Lerp(_animProgress, _targetAnimProgress, 0.22f);

			if (IsClosing && _animProgress <= 0.01f)
			{
				_animProgress = 0f;
				DummyUISystem.Instance.CloseStateNow();
				return;
			}

			if (!Main.mouseLeft)
			{
				_isDraggingSlider = false;
				_draggingDetailedSlider = -1;
			}

			Player player = Main.LocalPlayer;
			if (!player.active || player.dead || player.HeldItem == null || player.HeldItem.type != ModContent.ItemType<items.usable.Spawn_dummy>())
			{
				if (!IsClosing)
				{
					StartClosing();
				}
			}

			bool isArmored = DummyUISystem.IsArmored;
			bool hasDummies = AnyDummyActive();

			_removeBtnAnim = MathHelper.Lerp(_removeBtnAnim, hasDummies ? 1f : 0f, 0.2f);
			if (MathF.Abs(_removeBtnAnim - (hasDummies ? 1f : 0f)) < 0.005f)
			{
				_removeBtnAnim = hasDummies ? 1f : 0f;
			}

			_detailedAnim = MathHelper.Lerp(_detailedAnim, _isDetailedOpen ? 1f : 0f, 0.22f);
			if (MathF.Abs(_detailedAnim - (_isDetailedOpen ? 1f : 0f)) < 0.005f)
			{
				_detailedAnim = _isDetailedOpen ? 1f : 0f;
			}

			float baseHeight = isArmored ? 120f : 65f;
			float targetHeight = baseHeight + 45f * _removeBtnAnim;
			_currentPanelHeight = MathHelper.Lerp(_currentPanelHeight, targetHeight, 0.2f);

			float panelWidth = 360f;
			float panelLeft = (Main.screenWidth - panelWidth) * 0.5f;
			float targetTop = isArmored ? 35f : (60f - 10f * _removeBtnAnim);
			float animTop = MathHelper.Lerp(-180f, targetTop, EaseOut(_animProgress));

			Rectangle panelBounds = new Rectangle((int)panelLeft, (int)animTop, (int)panelWidth, (int)_currentPanelHeight);
			Rectangle detailedBtnBounds = new Rectangle((int)(panelLeft - 160f), (int)(animTop + 14f), 150, 34);
			Rectangle detailedPanelBounds = new Rectangle((int)(panelLeft - 325f), (int)(animTop + 54f), 315, 375);

			Vector2 mouse = new Vector2(Main.mouseX, Main.mouseY);

			if (panelBounds.Contains(mouse.ToPoint()) ||
				detailedBtnBounds.Contains(mouse.ToPoint()) ||
				(_detailedAnim > 0.1f && detailedPanelBounds.Contains(mouse.ToPoint())) ||
				_isDraggingSlider ||
				_draggingDetailedSlider != -1)
			{
				Main.LocalPlayer.mouseInterface = true;
			}

			if (_isDraggingSlider && isArmored)
			{
				UpdateSliderValue();
			}
		}

		private float EaseOut(float t)
		{
			t = MathHelper.Clamp(t, 0f, 1f);
			return 1f - (float)Math.Pow(1f - t, 3);
		}

		private void UpdateSliderValue()
		{
			float trackWidth = 260f;
			float panelLeft = (Main.screenWidth - 360f) * 0.5f;
			float trackLeft = panelLeft + (360f - trackWidth) * 0.5f;

			float ratio = MathHelper.Clamp((Main.mouseX - trackLeft) / trackWidth, 0f, 1f);
			int newDefense = (int)(ratio * 3000f);
			newDefense = (int)(Math.Round(newDefense / 10.0) * 10);

			if (DummyUISystem.ArmoredDefense != newDefense)
			{
				DummyUISystem.ArmoredDefense = newDefense;
				SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.4f });
			}
		}

		public static bool AnyDummyActive()
		{
			int testdummyType = ModContent.NPCType<Testdummy>();
			int testdummyDefensedType = ModContent.NPCType<Testdummy_defensed>();
			int testdummyRespawnableType = ModContent.NPCType<Testdummy_respawnable>();

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && (npc.type == testdummyType || npc.type == testdummyDefensedType || npc.type == testdummyRespawnableType || npc.type == NPCID.TargetDummy))
				{
					return true;
				}
			}
			return false;
		}

		public static void RemoveAllDummies()
		{
			int testdummyType = ModContent.NPCType<Testdummy>();
			int testdummyDefensedType = ModContent.NPCType<Testdummy_defensed>();
			int testdummyRespawnableType = ModContent.NPCType<Testdummy_respawnable>();

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && (npc.type == testdummyType 
				|| npc.type == testdummyDefensedType 
				|| npc.type == testdummyRespawnableType 
				|| npc.type == NPCID.TargetDummy))
				{
					for (int d = 0; d < 12; d++)
					{
						Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.WoodFurniture, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
						dust.scale = Main.rand.NextFloat(1f, 1.4f);
					}
					for (int s = 0; s < 6; s++)
					{
						Dust smoke = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Smoke, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f));
						smoke.scale = 1.2f;
					}

					npc.active = false;
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
					}
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			if (_animProgress <= 0.005f) return;

			Texture2D pixel = TextureAssets.MagicPixel.Value;

			bool isArmored = DummyUISystem.IsArmored;

			float panelWidth = 360f;
			float panelLeft = (Main.screenWidth - panelWidth) * 0.5f;
			float targetTop = isArmored ? 35f : (60f - 10f * _removeBtnAnim);
			float animTop = MathHelper.Lerp(-180f, targetTop, EaseOut(_animProgress));
			float alpha = MathHelper.Clamp(_animProgress, 0f, 1f);

			Rectangle panelRect = new Rectangle((int)panelLeft, (int)animTop, (int)panelWidth, (int)_currentPanelHeight);


			Vector2 mouse = new Vector2(Main.mouseX, Main.mouseY);
			if (panelRect.Contains(mouse.ToPoint()) || _isDraggingSlider || _draggingDetailedSlider != -1)
			{
				Main.LocalPlayer.mouseInterface = true;
			}


			Terraria.Utils.DrawInvBG(spriteBatch, panelRect, new Color(20, 25, 38) * 0.95f * alpha);


			float btnDetX = panelLeft - 160f;
			float btnDetY = animTop + 14f;
			float btnDetWidth = 150f;
			float btnDetHeight = 34f;

			Rectangle btnDetRect = new Rectangle((int)btnDetX, (int)btnDetY, (int)btnDetWidth, (int)btnDetHeight);
			string detText = (string)DummyUILanguageSystem.Dummy_details;

			if (DrawInvButton(spriteBatch, mouse, btnDetRect, detText, _isDetailedOpen, alpha, 0.82f))
			{
				_isDetailedOpen = !_isDetailedOpen;
				SoundEngine.PlaySound(SoundID.MenuTick);
			}


			if (_detailedAnim > 0.01f)
			{
				float dAlpha = alpha * MathHelper.Clamp(_detailedAnim, 0f, 1f);
				float dLeft = panelLeft - 325f;
				float dTop = animTop + 54f;
				float dWidth = 315f;
				float dHeight = 375f;

				Rectangle dRect = new Rectangle((int)dLeft, (int)dTop, (int)dWidth, (int)dHeight);

				if (dRect.Contains(mouse.ToPoint()))
				{
					Main.LocalPlayer.mouseInterface = true;
				}

				Terraria.Utils.DrawInvBG(spriteBatch, dRect, new Color(18, 22, 34) * 0.96f * dAlpha);

				string title = (string)DummyUILanguageSystem.Dummy_detailed_settings;
				Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(title);
				Vector2 titlePos = new Vector2(dLeft + (dWidth - titleSize.X) * 0.5f, dTop + 8f);

				Terraria.Utils.DrawBorderString(spriteBatch, title, titlePos, Color.Gold * dAlpha, 0.85f, 0f, 0f);

				float curY = dTop + 32f;


				string scaleText = (string)DummyUILanguageSystem.Dummy_scale + " " + $"{DummyUISystem.DummyScale:F1}x";
				Terraria.Utils.DrawBorderString(spriteBatch, scaleText, new Vector2(dLeft + 14f, curY), Color.White * dAlpha, 0.78f);
				curY += 18f;
				DrawInvSlider(spriteBatch, mouse, dLeft + 14f, curY, 287f, 10f, (DummyUISystem.DummyScale - 0.5f) / 9.5f, dAlpha, (ratio) => UpdateDetailedSliderValue(0, ratio), _draggingDetailedSlider == 0, (d) => _draggingDetailedSlider = d ? 0 : -1);
				curY += 20f;

				
				string drText = (string)DummyUILanguageSystem.Dummy_DR + " " + $"{(int)(DummyUISystem.DummyDR * 100f)}%";
				Terraria.Utils.DrawBorderString(spriteBatch, drText, new Vector2(dLeft + 14f, curY), Color.White * dAlpha, 0.78f);
				curY += 18f;

				DrawInvSlider(spriteBatch, mouse, dLeft + 14f, curY, 287f, 10f, DummyUISystem.DummyDR, dAlpha, (ratio) => UpdateDetailedSliderValue(1, ratio), _draggingDetailedSlider == 1, (d) => _draggingDetailedSlider = d ? 1 : -1);
				curY += 20f;


				string hpText = (string)DummyUILanguageSystem.Dummy_health + " " + $"{DummyUISystem.DummyHealthPercent:F4}%";
				Terraria.Utils.DrawBorderString(spriteBatch, hpText, new Vector2(dLeft + 14f, curY), Color.White * dAlpha, 0.78f);
				curY += 18f;

				DrawInvSlider(spriteBatch, mouse, dLeft + 14f, curY, 287f, 10f, (DummyUISystem.DummyHealthPercent - 0.0001f) / 99.9999f, dAlpha, (ratio) => UpdateDetailedSliderValue(2, ratio), _draggingDetailedSlider == 2, (d) => _draggingDetailedSlider = d ? 2 : -1);
				curY += 20f;


				string dmgText = (string)DummyUILanguageSystem.Dummy_dealt + " " + $"{DummyUISystem.DummyDamage}";
				Terraria.Utils.DrawBorderString(spriteBatch, dmgText, new Vector2(dLeft + 14f, curY), Color.White * dAlpha, 0.78f);
				curY += 18f;

				DrawInvSlider(spriteBatch, mouse, dLeft + 14f, curY, 287f, 10f, DummyUISystem.DummyDamage / 2500f, dAlpha, (ratio) => UpdateDetailedSliderValue(3, ratio), _draggingDetailedSlider == 3, (d) => _draggingDetailedSlider = d ? 3 : -1);
				curY += 20f;


				string debuffText = (string)DummyUILanguageSystem.Dummy_vulnerability + " " + ":";
				Terraria.Utils.DrawBorderString(spriteBatch, debuffText, new Vector2(dLeft + 14f, curY), Color.White * dAlpha, 0.78f);
				curY += 18f;

				string debuffYes = (string)DummyUILanguageSystem.Dummy_vulnerability_yes;
				string debuffNo = (string)DummyUILanguageSystem.Dummy_vulnerability_no;

				DrawToggleButtons(spriteBatch, pixel, mouse, dLeft + 14f, curY, 287f, 24f, debuffYes, debuffNo, DummyUISystem.DummyDebuffVulnerable, dAlpha, () => DummyUISystem.DummyDebuffVulnerable = true, () => DummyUISystem.DummyDebuffVulnerable = false);
				curY += 30f;


				string kbText = (string)DummyUILanguageSystem.Dummy_knockback_resist + " " + $"{(int)DummyUISystem.DummyKnockbackResist}%";
				Terraria.Utils.DrawBorderString(spriteBatch, kbText, new Vector2(dLeft + 14f, curY), Color.White * dAlpha, 0.78f);
				curY += 18f;

				DrawInvSlider(spriteBatch, mouse, dLeft + 14f, curY, 287f, 10f, DummyUISystem.DummyKnockbackResist / 100f, dAlpha, (ratio) => UpdateDetailedSliderValue(4, ratio), _draggingDetailedSlider == 4, (d) => _draggingDetailedSlider = d ? 4 : -1);
				curY += 20f;


				string gravText = (string)DummyUILanguageSystem.Dummy_gravity;
				Terraria.Utils.DrawBorderString(spriteBatch, gravText, new Vector2(dLeft + 14f, curY), Color.White * dAlpha, 0.78f);
				curY += 18f;

				string gravNormal = (string)DummyUILanguageSystem.Dummy_gravity_yes;
				string gravHover = (string)DummyUILanguageSystem.Dummy_gravity_no
				;
				DrawToggleButtons(spriteBatch, pixel, mouse, dLeft + 14f, curY, 287f, 24f, gravNormal, gravHover, !DummyUISystem.DummyNoGravity, dAlpha, () => DummyUISystem.DummyNoGravity = false, () => DummyUISystem.DummyNoGravity = true);
				curY += 30f;


				Rectangle resetRect = new Rectangle((int)(dLeft + 14f), (int)curY, 287, 24);
				string resetText = (string)DummyUILanguageSystem.Dummy_reset;

				if (DrawInvButton(spriteBatch, mouse, resetRect, resetText, false, dAlpha, 0.75f))
				{
					DummyUISystem.ResetDetailedSettings();
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
			}


			float armLerp = MathHelper.Clamp((_currentPanelHeight - (65f + 45f * _removeBtnAnim)) / 55f, 0f, 1f);
			if (isArmored || armLerp > 0.01f)
			{
				float sliderAlpha = armLerp * alpha;

				if (sliderAlpha > 0.01f)
				{
					string defText = (string)DummyUILanguageSystem.DummyAR_defense + " " + $"{DummyUISystem.ArmoredDefense}";

					Vector2 textSize = FontAssets.MouseText.Value.MeasureString(defText);
					Vector2 textPos = new Vector2(panelLeft + (panelWidth - textSize.X) * 0.5f, animTop + 10f);
					Terraria.Utils.DrawBorderString(spriteBatch, defText, textPos, Color.Gold * sliderAlpha, 0.85f, 0f, 0f);

					float trackWidth = 260f;
					float trackHeight = 12f;
					float trackLeft = panelLeft + (panelWidth - trackWidth) * 0.5f;
					float trackTop = animTop + 34f;

					float fillRatio = MathHelper.Clamp(DummyUISystem.ArmoredDefense / 3000f, 0f, 1f);

					DrawInvSlider(spriteBatch, mouse, trackLeft, trackTop, trackWidth, trackHeight, fillRatio, sliderAlpha, (ratio) => 
					{
						int newDefense = (int)(ratio * 3000f);
						newDefense = (int)(Math.Round(newDefense / 10.0) * 10);

						if (DummyUISystem.ArmoredDefense != newDefense)
						{
							DummyUISystem.ArmoredDefense = newDefense;

							SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.4f });
						}
					}, _isDraggingSlider, (d) => _isDraggingSlider = d);
				}
			}

			float buttonY = MathHelper.Lerp(animTop + 14f, animTop + 65f, armLerp);
			float buttonWidth = 150f;
			float buttonHeight = 34f;
			float buttonGap = 16f;
			float startX = panelLeft + (panelWidth - (buttonWidth * 2f + buttonGap)) * 0.5f;

			Rectangle btn1Rect = new Rectangle((int)startX, (int)buttonY, (int)buttonWidth, (int)buttonHeight);
			string btn1Text = (string)DummyUILanguageSystem.CommonDummy;

			if (DrawInvButton(spriteBatch, mouse, btn1Rect, btn1Text, !isArmored, alpha, 0.85f))
			{
				DummyUISystem.IsArmored = false;
				SoundEngine.PlaySound(SoundID.MenuTick);
			}

			Rectangle btn2Rect = new Rectangle((int)(startX + buttonWidth + buttonGap), (int)buttonY, (int)buttonWidth, (int)buttonHeight);
			string btn2Text = (string)DummyUILanguageSystem.ArmoredDummy;

			if (DrawInvButton(spriteBatch, mouse, btn2Rect, btn2Text, isArmored, alpha, 0.85f))
			{
				DummyUISystem.IsArmored = true;
				SoundEngine.PlaySound(SoundID.MenuTick);
			}

			if (_removeBtnAnim > 0.01f)
			{
				float btn3Alpha = alpha * _removeBtnAnim;
				float btn3Y = buttonY + 42f;
				float btn3Width = 220f;
				float btn3Height = 34f;

				float btn3X = panelLeft + (panelWidth - btn3Width) * 0.5f;

				Rectangle btn3Rect = new Rectangle((int)btn3X, (int)btn3Y, (int)btn3Width, (int)btn3Height);
				string btn3Text = (string)DummyUILanguageSystem.DummyRemove;

				if (DrawInvButton(spriteBatch, mouse, btn3Rect, btn3Text, false, btn3Alpha, 0.85f, isDanger: true))
				{
					RemoveAllDummies();
					SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, Pitch = 0.1f });
				}
			}
		}

		private bool DrawInvButton(SpriteBatch sb, Vector2 mouse, Rectangle rect, string text, bool selected, float alpha, float textScale = 0.8f, bool isDanger = false)
		{
			bool hover = rect.Contains(mouse.ToPoint()) && alpha > 0.5f;

			Color colorTint;
			if (isDanger)
			{
				colorTint = hover ? new Color(220, 60, 70) : new Color(140, 40, 48);
			}
			else if (selected)
			{
				colorTint = hover ? new Color(0, 180, 240) : new Color(20, 140, 200);
			}
			else
			{
				colorTint = hover ? new Color(75, 88, 115) : new Color(40, 48, 65);
			}

			Terraria.Utils.DrawInvBG(sb, rect, colorTint * alpha);

			Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
			Vector2 textPos = rect.Center.ToVector2() - textSize * (textScale * 0.42f);
			Color textColor = selected || hover ? Color.White : Color.LightGray;

			Terraria.Utils.DrawBorderString(sb, text, textPos, textColor * alpha, textScale, 0f, 0f);

			return !IsClosing && hover && Main.mouseLeft && Main.mouseLeftRelease;
		}

		private void DrawInvSlider(SpriteBatch sb, Vector2 mouse, float x, float y, float width, float height, float valueRatio, float alpha, Action<float> onRatioChange, bool isDragging, Action<bool> setDragging)
		{
			valueRatio = MathHelper.Clamp(valueRatio, 0f, 1f);

			Texture2D knobFill = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle_Fill").Value;
			Texture2D knobOutline = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle").Value;

			
			Rectangle trackRect = new Rectangle((int)x, (int)y, (int)width, (int)height);
			Terraria.Utils.DrawInvBG(sb, trackRect, new Color(30, 35, 50, 220) * alpha);

			
			float fillWidth = width * valueRatio;
			if (fillWidth >= 6f)
			{
				Rectangle fillRect = new Rectangle((int)x, (int)y, (int)fillWidth, (int)height);
				Terraria.Utils.DrawInvBG(sb, fillRect, new Color(0, 200, 240, 255) * alpha);
			}
			else if (fillWidth > 0f)
			{
				Rectangle fillRect = new Rectangle((int)x, (int)y, (int)Math.Max(3f, fillWidth), (int)height);
				sb.Draw(TextureAssets.MagicPixel.Value, fillRect, new Color(0, 200, 240, 255) * alpha);
			}

			
			Vector2 knobCenter = new Vector2(x + fillWidth, y + height * 0.5f + 4.5f);
			float knobScale = 0.55f;
			float knobRadius = knobFill.Width * knobScale * 0.5f;
			Rectangle knobBounds = new Rectangle((int)(knobCenter.X - knobRadius - 3f), (int)(knobCenter.Y - knobRadius - 3f), (int)(knobRadius * 2f + 6f), (int)(knobRadius * 2f + 6f));

			bool hover = (trackRect.Contains(mouse.ToPoint()) || knobBounds.Contains(mouse.ToPoint())) && alpha > 0.5f;

			
			Vector2 fillOrigin = knobFill.Size() * 0.5f;
			Color fillColor = (hover || isDragging) ? new Color(0, 240, 255) : new Color(0, 160, 220);
			sb.Draw(knobFill, knobCenter, null, fillColor * alpha, 0f, fillOrigin, knobScale, SpriteEffects.None, 0f);

			
			Vector2 outlineOrigin = knobOutline.Size() * 0.5f;
			sb.Draw(knobOutline, knobCenter, null, Color.White * alpha, 0f, outlineOrigin, knobScale, SpriteEffects.None, 0f);

			
			if (!IsClosing && alpha > 0.5f && Main.mouseLeft && (trackRect.Contains(mouse.ToPoint()) || knobBounds.Contains(mouse.ToPoint()) || isDragging))
			{
				setDragging?.Invoke(true);
				float newRatio = MathHelper.Clamp((Main.mouseX - x) / width, 0f, 1f);
				onRatioChange?.Invoke(newRatio);
			}
		}

		private void UpdateDetailedSliderValue(int sliderId, float ratio)
		{
			switch (sliderId)
			{
				case 0: 
					float newScale = (float)Math.Round(0.5f + ratio * 9.5f, 1);
					if (DummyUISystem.DummyScale != newScale)
					{
						DummyUISystem.DummyScale = newScale;
						SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.3f });
					}
					break;
				case 1: 
					float newDR = (float)Math.Round(ratio, 2);
					if (DummyUISystem.DummyDR != newDR)
					{
						DummyUISystem.DummyDR = newDR;
						SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.3f });
					}
					break;
				case 2: 
					float newHP = (float)Math.Round(0.0001f + ratio * 99.9999f, 4);
					if (DummyUISystem.DummyHealthPercent != newHP)
					{
						DummyUISystem.DummyHealthPercent = newHP;
						SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.3f });
					}
					break;
				case 3: 
					int newDmg = (int)(Math.Round(ratio * 2500.0 / 10.0) * 10);
					if (DummyUISystem.DummyDamage != newDmg)
					{
						DummyUISystem.DummyDamage = newDmg;
						SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.3f });
					}
					break;
				case 4: 
					float newKB = (float)Math.Round(ratio * 100f);
					if (DummyUISystem.DummyKnockbackResist != newKB)
					{
						DummyUISystem.DummyKnockbackResist = newKB;
						SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.3f });
					}
					break;
			}
		}

		private void DrawToggleButtons(SpriteBatch sb, Texture2D pixel, Vector2 mouse, float x, float y, float totalWidth, float height, string text1, string text2, bool option1Selected, float alpha, Action onSelect1, Action onSelect2)
		{
			float btnWidth = (totalWidth - 8f) * 0.5f;

			Rectangle rect1 = new Rectangle((int)x, (int)y, (int)btnWidth, (int)height);
			if (DrawInvButton(sb, mouse, rect1, text1, option1Selected, alpha, 0.72f))
			{
				onSelect1?.Invoke();
				SoundEngine.PlaySound(SoundID.MenuTick);
			}

			Rectangle rect2 = new Rectangle((int)(x + btnWidth + 8f), (int)y, (int)btnWidth, (int)height);
			if (DrawInvButton(sb, mouse, rect2, text2, !option1Selected, alpha, 0.72f))
			{
				onSelect2?.Invoke();
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		private void DrawBorder(SpriteBatch sb, Texture2D pixel, Rectangle r, Color color, int thickness)
		{
			sb.Draw(pixel, new Rectangle(r.X, r.Y, r.Width, thickness), color);
			sb.Draw(pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), color);
			sb.Draw(pixel, new Rectangle(r.X, r.Y, thickness, r.Height), color);
			sb.Draw(pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), color);
		}
	}

	public class DummyUILanguageSystem : ModSystem
	{
		public static LocalizedText Dummy_details { get; private set; }
		public static LocalizedText Dummy_detailed_settings { get; private set; }


		public static LocalizedText Dummy_scale { get; private set; }
		public static LocalizedText Dummy_DR { get; private set; }
		public static LocalizedText Dummy_health { get; private set; }
		public static LocalizedText Dummy_dealt { get; private set; }
		public static LocalizedText Dummy_vulnerability { get; private set; }
		public static LocalizedText Dummy_vulnerability_yes { get; private set; }
		public static LocalizedText Dummy_vulnerability_no { get; private set; }
		public static LocalizedText Dummy_knockback_resist { get; private set; }
		public static LocalizedText Dummy_gravity { get; private set; }
		public static LocalizedText Dummy_gravity_yes { get; private set; }
		public static LocalizedText Dummy_gravity_no { get; private set; }
		public static LocalizedText Dummy_reset { get; private set; }
		public static LocalizedText DummyAR_defense { get; private set; }
		public static LocalizedText CommonDummy { get; private set; }
		public static LocalizedText ArmoredDummy { get; private set; }
		public static LocalizedText DummyRemove { get; private set; }
        public override void Load()
        {
            string category = "UI";
			Dummy_details ??= Mod.GetLocalization($"{category}.Dummy_details");
			Dummy_detailed_settings ??= Mod.GetLocalization($"{category}.Dummy_detailed_settings");

			Dummy_scale ??= Mod.GetLocalization($"{category}.Dummy_scale");
			Dummy_DR ??= Mod.GetLocalization($"{category}.Dummy_DR");
			Dummy_health ??= Mod.GetLocalization($"{category}.Dummy_health");
			Dummy_dealt ??= Mod.GetLocalization($"{category}.Dummy_dealt");
			Dummy_vulnerability ??= Mod.GetLocalization($"{category}.Dummy_vulnerability");
			Dummy_vulnerability_yes ??= Mod.GetLocalization($"{category}.Dummy_vulnerability_yes");
			Dummy_vulnerability_no ??= Mod.GetLocalization($"{category}.Dummy_vulnerability_no");
			Dummy_knockback_resist ??= Mod.GetLocalization($"{category}.Dummy_knockback_resist");
			Dummy_gravity ??= Mod.GetLocalization($"{category}.Dummy_gravity");
			Dummy_gravity_yes ??= Mod.GetLocalization($"{category}.Dummy_gravity_yes");
			Dummy_gravity_no ??= Mod.GetLocalization($"{category}.Dummy_gravity_no");
			Dummy_reset ??= Mod.GetLocalization($"{category}.Dummy_reset");
			DummyAR_defense ??= Mod.GetLocalization($"{category}.DummyAR_defense");
			CommonDummy ??= Mod.GetLocalization($"{category}.CommonDummy");
			ArmoredDummy ??= Mod.GetLocalization($"{category}.ArmoredDummy");
			DummyRemove ??= Mod.GetLocalization($"{category}.DummyRemove");
        }
	}
}


