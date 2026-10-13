using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using System;
using System.Linq;
using Terraria.GameContent;
using jarkos.tiles.CradleTiles;
using jarkos.UI.Common;
using jarkos.systems;
using System.Collections.Generic;
using jarkos.core.PlayerInteract;
using Terraria.UI.Chat;
using jarkos.core.Data;

namespace jarkos.UI
{
	public class RitualBookUIState : UIState
	{
		private UIBookImage mainPanel;
		private UIScaledImageFramed bgPanel;
		private UIText titleTextLeft;
		private UIText titleTextRight;
		private UIText pageContentTextLeft;
		private UIText pageContentTextRight;
		private UIText pageNumberTextLeft;
		private UIText pageNumberTextRight;
		private UIAnimatedArrowButton prevButton;
		private UIAnimatedArrowButton nextButton;
		private UITextPanel<LocalizedText> closeButton;
		private UIAnimatedIcon rbIcon;
		private UIImage arrowDownImage;
		private UIFloatingImage exampleIconImage;
		private UIImage guideImage;
		private UIAnimatedIcon rbIconRight;
		private UIImage arrowDownImageRight;
		private UIFloatingImage exampleIconImageRight;
		private UIImage guideImageRight;
		private UITextPanel<LocalizedText> tab1;
		private UITextPanel<LocalizedText> tab2;
		private UITextPanel<LocalizedText> tab3;
		private UITextPanel<LocalizedText> tab4;
		private int currentSpread = 0;
		private bool isEssenceCatalog = false;
		private EssenceCatalogPage essenceCatalogPage;
		private bool isResearchCatalog = false;
		private ResearchCatalogPage researchCatalogPage;
		private bool isAnimating = false;
		private int animTimer = 0;
		private int animFrame = 0;
		private const int TicksPerFrame = 3;
		private float contentAnimProgress = 1f;
		private LocalizedText[] pages;
		private UIElement recipePageLeft;
		private UIElement recipePageRight;
		private Common.Tween _scaleTween;
		private bool _isClosing = false;
		public bool IsClosing => _isClosing;
		private UIPanel searchPanel;
		private UISearchBar searchBar;
		private string currentSearchText = "";
		private List<PetrifiedAltarRecipe> filteredRecipes = new List<PetrifiedAltarRecipe>();
		private bool _needsPageUpdate = false;
		
		private bool isGridMode = false;
		private UITextPanel<string> gridFilterBtn;
		private UITextPanel<string> lineFilterBtn;
		private UIPanel recipePopupPanel;
		private RitualRecipePage popupRecipePage;

		public void Open()
		{
			_isClosing = false;
			if (_scaleTween == null)
				_scaleTween = new Tween(0.35f, MathSystem.EaseOutCubic);
			_scaleTween.Set(0f);
			_scaleTween.To(1f);
		}

		public void Close()
		{
			if (_isClosing) return;
			_isClosing = true;
			if (_scaleTween == null)
				_scaleTween = new Tween(0.35f, MathSystem.EaseOutCubic);
			_scaleTween.To(0f);
		}

		private int GetMaxSpread()
		{
			int loreSpreads = 3;
			int recipesCount = filteredRecipes?.Count ?? 0;
			if (recipesCount == 0) return loreSpreads - 1;
			int itemsPerSpread = isGridMode ? 24 : 2;
			int recipeSpreads = (int)Math.Ceiling((double)recipesCount / itemsPerSpread);
			return loreSpreads + recipeSpreads - 1;
		}
		public override void OnInitialize()
		{
			pages = new LocalizedText[6]
			{
				Language.GetText("Mods.jarkos.UI.RitualBookUI.Page0"), Language.GetText("Mods.jarkos.UI.RitualBookUI.Page1"), Language.GetText("Mods.jarkos.UI.RitualBookUI.Page2"), Language.GetText("Mods.jarkos.UI.RitualBookUI.Page3"), Language.GetText("Mods.jarkos.UI.RitualBookUI.Page4"), Language.GetText("Mods.jarkos.UI.RitualBookUI.Page5")
			};

			if (PetrifiedAltarTileEntity.Recipes == null)
				PetrifiedAltarTileEntity.LoadRecipes();

			filteredRecipes = PetrifiedAltarTileEntity.Recipes.ToList();
			Asset<Texture2D> bookTexture = ModContent.Request<Texture2D>("jarkos/UI/BookGuideUI");
			Asset<Texture2D> bookBgTexture = ModContent.Request<Texture2D>("jarkos/UI/BookGuideUIBG");

			bgPanel = new UIScaledImageFramed(bookBgTexture, new Rectangle(0, 0, 850, 550));
			bgPanel.ImageScale = 1.25f;
			bgPanel.Width.Set(850f * 1.25f, 0f);
			bgPanel.Height.Set(550f * 1.25f, 0f);
			bgPanel.HAlign = 0.5f;
			bgPanel.VAlign = 0.5f;
			bgPanel.Left.Set(-55f, 0f);
			bgPanel.Top.Set(-40f, 0f);
			Append(bgPanel);

			mainPanel = new UIBookImage(bookTexture, new Rectangle(0, 0, 850, 550));
			mainPanel.Width.Set(850f, 0f);
			mainPanel.Height.Set(550f, 0f);
			mainPanel.HAlign = 0.5f;
			mainPanel.VAlign = 0.5f;
			Append(mainPanel);

			titleTextLeft = new UIText(Language.GetText("Mods.jarkos.UI.RitualBookUI.Title"), 1.2f, true);
			titleTextLeft.HAlign = 0.25f;
			titleTextLeft.Top.Set(30, 0f);
			titleTextLeft.TextColor = Color.White;
			titleTextRight = new UIText("", 1.2f, true);
			titleTextRight.HAlign = 0.75f;
			titleTextRight.Top.Set(30, 0f);
			titleTextRight.TextColor = Color.White;

			pageContentTextLeft = new UIText(pages[0], 1.1f, false);
			pageContentTextLeft.HAlign = 0.05f;
			pageContentTextLeft.Top.Set(90, 0f);
			pageContentTextLeft.Width.Set(380f, 0f);
			pageContentTextLeft.IsWrapped = true;
			pageContentTextLeft.TextColor = Color.White;

			pageContentTextRight = new UIText(pages[1], 1.1f, false);
			pageContentTextRight.HAlign = 0.95f;
			pageContentTextRight.Top.Set(90, 0f);
			pageContentTextRight.Width.Set(380f, 0f);
			pageContentTextRight.IsWrapped = true;
			pageContentTextRight.TextColor = Color.White;

			pageNumberTextLeft = new UIText("1", 1f, true);
			pageNumberTextLeft.HAlign = 0.25f;
			pageNumberTextLeft.Top.Set(-50, 1f);
			pageNumberTextLeft.TextColor = Color.White;

			pageNumberTextRight = new UIText("2", 1f, true);
			pageNumberTextRight.HAlign = 0.75f;
			pageNumberTextRight.Top.Set(-50, 1f);
			pageNumberTextRight.TextColor = Color.White;

			closeButton = new UITextPanel<LocalizedText>(Language.GetText("Mods.jarkos.UI.RitualBookUI.CloseButton"));
			closeButton.Width.Set(40, 0f);
			closeButton.Height.Set(40, 0f);
			closeButton.HAlign = 0.98f;
			closeButton.Top.Set(20, 0f);
			closeButton.BackgroundColor = new Color(180, 60, 60);
			closeButton.BorderColor = new Color(80, 20, 20);
			closeButton.OnLeftClick += CloseButtonClicked;
			closeButton.OnMouseOver += (evt, el) => closeButton.BackgroundColor = new Color(220, 80, 80);
			closeButton.OnMouseOut += (evt, el) => closeButton.BackgroundColor = new Color(180, 60, 60);

			tab1 = new UITextPanel<LocalizedText>(LangSystem.TabBasics);
			tab1.Width.Set(100, 0f);
			tab1.Height.Set(40, 0f);
			tab1.HAlign = 1f;
			tab1.Left.Set(90, 0f);
			tab1.Top.Set(80, 0f);
			tab1.BackgroundColor = new Color(73, 94, 171);
			tab1.OnLeftClick += (evt, el) => { if (!isAnimating && (currentSpread != 0 || isEssenceCatalog)) { isEssenceCatalog = false; currentSpread = 0; SoundEngine.PlaySound(SoundID.MenuTick); StartAnimation(); } };
			tab1.OnMouseOver += (evt, el) => tab1.BackgroundColor = new Color(100, 120, 200);
			tab1.OnLeftClick += (evt, el) => { if (!isAnimating && (currentSpread != 0 || isEssenceCatalog || isResearchCatalog)) { isEssenceCatalog = false; isResearchCatalog = false; currentSpread = 0; SoundEngine.PlaySound(SoundID.MenuTick); StartAnimation(); } };
			tab1.OnMouseOver += (evt, el) => tab1.BackgroundColor = new Color(100, 120, 200);
			tab1.OnMouseOut += (evt, el) => tab1.BackgroundColor = new Color(73, 94, 171);

			tab2 = new UITextPanel<LocalizedText>(LangSystem.TabRituals);
			tab2.Width.Set(100, 0f);
			tab2.Height.Set(40, 0f);
			tab2.HAlign = 1f;
			tab2.Left.Set(90, 0f);
			tab2.Top.Set(130, 0f);
			tab2.BackgroundColor = new Color(73, 94, 171);
			tab2.OnLeftClick += (evt, el) => { if (!isAnimating && (currentSpread != 3 || isEssenceCatalog || isResearchCatalog) && GetMaxSpread() >= 3) { isEssenceCatalog = false; isResearchCatalog = false; currentSpread = 3; SoundEngine.PlaySound(SoundID.MenuTick); StartAnimation(); } };
			tab2.OnMouseOver += (evt, el) => tab2.BackgroundColor = new Color(100, 120, 200);
			tab2.OnMouseOut += (evt, el) => tab2.BackgroundColor = new Color(73, 94, 171);

			tab3 = new UITextPanel<LocalizedText>(LangSystem.TabEssences);
			tab3.Width.Set(100, 0f);
			tab3.Height.Set(40, 0f);
			tab3.HAlign = 1f;
			tab3.Left.Set(90, 0f);
			tab3.Top.Set(180, 0f);
			tab3.BackgroundColor = new Color(73, 94, 171);
			tab3.OnLeftClick += (evt, el) => { if (!isAnimating && (!isEssenceCatalog || isResearchCatalog)) { isEssenceCatalog = true; isResearchCatalog = false; SoundEngine.PlaySound(SoundID.MenuTick); StartAnimation(); } };
			tab3.OnMouseOver += (evt, el) => tab3.BackgroundColor = new Color(100, 120, 200);
			tab3.OnMouseOut += (evt, el) => tab3.BackgroundColor = new Color(73, 94, 171);

			tab4 = new UITextPanel<LocalizedText>(LangSystem.TabResearch);
			tab4.Width.Set(100, 0f);
			tab4.Height.Set(40, 0f);
			tab4.HAlign = 1f;
			tab4.Left.Set(90, 0f);
			tab4.Top.Set(230, 0f);
			tab4.BackgroundColor = new Color(73, 94, 171);
			tab4.OnLeftClick += (evt, el) => { if (!isAnimating && !isResearchCatalog) { isEssenceCatalog = false; isResearchCatalog = true; SoundEngine.PlaySound(SoundID.MenuTick); StartAnimation(); } };
			tab4.OnMouseOver += (evt, el) => tab4.BackgroundColor = new Color(100, 120, 200);
			tab4.OnMouseOut += (evt, el) => tab4.BackgroundColor = new Color(73, 94, 171);

			prevButton = new UIAnimatedArrowButton(ModContent.Request<Texture2D>("jarkos/UI/arrow_left", AssetRequestMode.ImmediateLoad));
			prevButton.VAlign = 0f;
			prevButton.HAlign = 0.05f;
			prevButton.Top.Set(-70, 1f);
			prevButton.Left.Set(0, 0f);
			prevButton.OnLeftClick += PrevButtonClicked;

			nextButton = new UIAnimatedArrowButton(ModContent.Request<Texture2D>("jarkos/UI/arrow_right", AssetRequestMode.ImmediateLoad));
			nextButton.VAlign = 0f;
			nextButton.HAlign = 0.95f;
			nextButton.Top.Set(-70, 1f);
			nextButton.Left.Set(0, 0f);
			nextButton.OnLeftClick += NextButtonClicked;

			Asset<Texture2D> guidePicTexture = ModContent.Request<Texture2D>("jarkos/UI/BookGuideUIPic");
			guideImage = new UIImage(guidePicTexture);
			guideImage.HAlign = 0.14f;
			guideImage.Top.Set(380f, 0f);

			Asset<Texture2D> rbIconTexture = ModContent.Request<Texture2D>("jarkos/UI/RB_Icon", AssetRequestMode.ImmediateLoad);
			rbIcon = new UIAnimatedIcon(rbIconTexture, 26, 40, 2, 30);
			rbIcon.HAlign = 0.285f;
			rbIcon.Top.Set(350f, 0f);

			Asset<Texture2D> arrowDownTex = ModContent.Request<Texture2D>("jarkos/UI/arrow_down", AssetRequestMode.ImmediateLoad);
			arrowDownImage = new UIImage(arrowDownTex);
			arrowDownImage.HAlign = 0.285f;
			arrowDownImage.Top.Set(305f, 0f);

			Asset<Texture2D> exampleIconTex = ModContent.Request<Texture2D>("jarkos/UI/ExampleIcon", AssetRequestMode.ImmediateLoad);
			exampleIconImage = new UIFloatingImage(exampleIconTex);
			exampleIconImage.HAlign = 0.2867f;
			exampleIconImage.Top.Set(330f, 0f);

			guideImageRight = new UIImage(guidePicTexture);
			guideImageRight.HAlign = 0.79f;
			guideImageRight.Top.Set(380f, 0f);

			rbIconRight = new UIAnimatedIcon(rbIconTexture, 26, 40, 2, 30);
			rbIconRight.HAlign = 0.745f;
			rbIconRight.Top.Set(350f, 0f);

			arrowDownImageRight = new UIImage(arrowDownTex);
			arrowDownImageRight.HAlign = 0.745f;
			arrowDownImageRight.Top.Set(305f, 0f);

			exampleIconImageRight = new UIFloatingImage(exampleIconTex);
			exampleIconImageRight.HAlign = 0.8167f;
			exampleIconImageRight.VAlign = 0.1367f;
			exampleIconImageRight.Top.Set(480f, 0f);

			essenceCatalogPage = new EssenceCatalogPage();
			essenceCatalogPage.Width.Set(850f, 0f);
			essenceCatalogPage.Height.Set(550f, 0f);
			essenceCatalogPage.HAlign = 0.5f;
			essenceCatalogPage.VAlign = 0.5f;
			
			researchCatalogPage = new ResearchCatalogPage();
			researchCatalogPage.Width.Set(850f, 0f);
			researchCatalogPage.Height.Set(550f, 0f);
			researchCatalogPage.HAlign = 0.5f;
			researchCatalogPage.VAlign = 0.5f;

			searchPanel = new UIPanel();
			searchPanel.Width.Set(320f, 0f);
			searchPanel.Height.Set(50f, 0f);
			searchPanel.HAlign = 0.5f;
			searchPanel.VAlign = 0f;
			searchPanel.Top.Set(485f, 0f);
			searchPanel.BackgroundColor = new Color(73, 94, 171);
			searchPanel.BorderColor = new Color(40, 50, 100);

			searchBar = new UISearchBar(Language.GetText("UI.Search"), 0.8f);
			searchBar.Width.Set(300f, 0f);
			searchBar.Height.Set(40f, 0f);
			searchBar.HAlign = 0.5f;
			searchBar.VAlign = 0.5f;
			searchBar.Top.Set(-5f, 0f);
			searchBar.OnLeftClick += (evt, el) => searchBar.ToggleTakingText();
			searchBar.OnContentsChanged += (s) =>
			{
				currentSearchText = s;
				var allRecipes = PetrifiedAltarTileEntity.Recipes;
				if (string.IsNullOrWhiteSpace(currentSearchText))
				{
					filteredRecipes = allRecipes.ToList();
				}
				else
				{
					filteredRecipes = allRecipes.Where(r => Lang.GetItemNameValue(r.ResultItem).ToLower().Contains(currentSearchText.ToLower())).ToList();
				}
				if (currentSpread > GetMaxSpread()) currentSpread = Math.Max(3, GetMaxSpread());
				_needsPageUpdate = true;
			};
			searchPanel.OnLeftClick += (evt, el) => searchBar.ToggleTakingText();
			searchPanel.Append(searchBar);

			gridFilterBtn = new UITextPanel<string>("Grid filter");
			gridFilterBtn.Width.Set(100f, 0f);
			gridFilterBtn.Height.Set(30f, 0f);
			gridFilterBtn.Top.Set(30f, 0f);
			gridFilterBtn.Left.Set(40f, 0f);
			gridFilterBtn.BackgroundColor = new Color(73, 94, 171);
			gridFilterBtn.OnLeftClick += (evt, el) =>
			{
				if (!isGridMode)
				{
					SoundEngine.PlaySound(SoundID.MenuTick);
					isGridMode = true;
					if (currentSpread > GetMaxSpread()) currentSpread = Math.Max(3, GetMaxSpread());
					_needsPageUpdate = true;
				}
			};

			lineFilterBtn = new UITextPanel<string>("Line filter");
			lineFilterBtn.Width.Set(100f, 0f);
			lineFilterBtn.Height.Set(30f, 0f);
			lineFilterBtn.Top.Set(30f, 0f);
			lineFilterBtn.Left.Set(150f, 0f);
			lineFilterBtn.BackgroundColor = new Color(180, 160, 40);
			lineFilterBtn.OnLeftClick += (evt, el) =>
			{
				if (isGridMode)
				{
					SoundEngine.PlaySound(SoundID.MenuTick);
					isGridMode = false;
					if (currentSpread > GetMaxSpread()) currentSpread = Math.Max(3, GetMaxSpread());
					_needsPageUpdate = true;
				}
			};
			
			recipePopupPanel = new UIPanel();
			recipePopupPanel.Width.Set(420f, 0f);
			recipePopupPanel.Height.Set(440f, 0f);
			recipePopupPanel.HAlign = 0.5f;
			recipePopupPanel.VAlign = 0.5f;
			recipePopupPanel.BackgroundColor = new Color(40, 50, 100, 240);
			recipePopupPanel.BorderColor = new Color(73, 94, 171);
			
			UITextPanel<LocalizedText> closePopupBtn = new UITextPanel<LocalizedText>(Language.GetText("Mods.jarkos.UI.RitualBookUI.CloseButton"));
			closePopupBtn.Width.Set(40, 0f);
			closePopupBtn.Height.Set(40, 0f);
			closePopupBtn.HAlign = 1f;
			closePopupBtn.Top.Set(-10, 0f);
			closePopupBtn.BackgroundColor = new Color(180, 60, 60);
			closePopupBtn.OnLeftClick += (evt, el) =>
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				if (HasChild(recipePopupPanel)) RemoveChild(recipePopupPanel);
			};
			recipePopupPanel.Append(closePopupBtn);

			ShowElements();
		}
		private void HideElements()
		{
			mainPanel.RemoveAllChildren();
		}
		private void ShowElements()
		{
			contentAnimProgress = 0f;
			if (isResearchCatalog)
			{
				mainPanel.Append(researchCatalogPage);
				researchCatalogPage.OnShow();
				mainPanel.Append(closeButton);
				mainPanel.Append(tab1);
				mainPanel.Append(tab2);
				mainPanel.Append(tab3);
				mainPanel.Append(tab4);
				return;
			}
			if (isEssenceCatalog)
			{
				mainPanel.Append(essenceCatalogPage);
				essenceCatalogPage.OnShow();
				mainPanel.Append(closeButton);
				mainPanel.Append(tab1);
				mainPanel.Append(tab2);
				mainPanel.Append(tab3);
				mainPanel.Append(tab4);
				return;
			}
			
			mainPanel.Append(titleTextLeft);
			mainPanel.Append(titleTextRight);
			mainPanel.Append(pageContentTextLeft);
			mainPanel.Append(pageContentTextRight);
			mainPanel.Append(pageNumberTextLeft);
			mainPanel.Append(pageNumberTextRight);
			mainPanel.Append(closeButton);
			mainPanel.Append(tab1);
			mainPanel.Append(tab2);
			mainPanel.Append(tab3);
			mainPanel.Append(tab4);

			if (currentSpread > 0)
				mainPanel.Append(prevButton);
			if (currentSpread < GetMaxSpread())
				mainPanel.Append(nextButton);
			if (currentSpread >= 3 && !isEssenceCatalog && !isResearchCatalog)
			{
				mainPanel.Append(searchPanel);
				mainPanel.Append(gridFilterBtn);
				mainPanel.Append(lineFilterBtn);
			}
			if (currentSpread == 1)
			{
				mainPanel.Append(guideImage);
				mainPanel.Append(rbIcon);
				mainPanel.Append(arrowDownImage);
				mainPanel.Append(exampleIconImage);
				mainPanel.Append(guideImageRight);
				mainPanel.Append(rbIconRight);
				mainPanel.Append(arrowDownImageRight);
				mainPanel.Append(exampleIconImageRight);
			}
			UpdatePageData();
		}
		private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			SoundEngine.PlaySound(SoundID.MenuClose);
			ModContent.GetInstance<RitualBookUISystem>().HideBook();
		}
		private void PrevButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			if (isAnimating) return;
			if (currentSpread > 0)
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				currentSpread--;
				StartAnimation();
			}
		}
		private void NextButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			if (isAnimating) return;
			if (currentSpread < GetMaxSpread())
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				currentSpread++;
				StartAnimation();
			}
		}
		private void StartAnimation()
		{
			isAnimating = true;
			animTimer = 0;
			animFrame = 1;
			mainPanel.RemoveAllChildren();
			Rectangle newFrame = new Rectangle(0, animFrame * 550, 850, 550);
			mainPanel.SetFrame(newFrame);
		}
		private void UpdatePageData()
		{
			int leftPageIndex = currentSpread * 2;
			int rightPageIndex = leftPageIndex + 1;
			if (recipePageLeft != null) { mainPanel.RemoveChild(recipePageLeft); recipePageLeft = null; }
			if (recipePageRight != null) { mainPanel.RemoveChild(recipePageRight); recipePageRight = null; }
			if (currentSpread < 3)
			{
				pageContentTextLeft.SetText(leftPageIndex < pages.Length ? pages[leftPageIndex].Value : "");
				pageContentTextRight.SetText(rightPageIndex < pages.Length ? pages[rightPageIndex].Value : "");
			}
			else
			{
				//pageContentTextLeft.SetText("");
				//pageContentTextRight.SetText("");

				if (filteredRecipes != null)
				{
					int startIndexLeft = isGridMode ? (currentSpread - 3) * 24 : leftPageIndex - 6;
					int startIndexRight = isGridMode ? startIndexLeft + 12 : rightPageIndex - 6;

					if (!isGridMode)
					{
						if (startIndexLeft >= 0 && startIndexLeft < filteredRecipes.Count)
						{
							recipePageLeft = new RitualRecipePage(filteredRecipes[startIndexLeft], startIndexLeft);
							recipePageLeft.Width.Set(380, 0);
							recipePageLeft.Height.Set(400, 0);
							recipePageLeft.HAlign = 0.05f;
							recipePageLeft.Top.Set(90, 0);
							mainPanel.Append(recipePageLeft);
						}
						if (startIndexRight >= 0 && startIndexRight < filteredRecipes.Count)
						{
							recipePageRight = new RitualRecipePage(filteredRecipes[startIndexRight], startIndexRight);
							recipePageRight.Width.Set(380, 0);
							recipePageRight.Height.Set(400, 0);
							recipePageRight.HAlign = 0.95f;
							recipePageRight.Top.Set(90, 0);
							mainPanel.Append(recipePageRight);
						}
					}
					else
					{
						if (startIndexLeft >= 0 && startIndexLeft < filteredRecipes.Count)
						{
							var gridLeft = new RitualRecipeGridPage(filteredRecipes, startIndexLeft, Math.Min(12, filteredRecipes.Count - startIndexLeft), this);

							gridLeft.Width.Set(380, 0);
							gridLeft.Height.Set(400, 0);
							gridLeft.HAlign = 0.05f;
							gridLeft.Top.Set(90, 0);

							recipePageLeft = gridLeft;
							mainPanel.Append(recipePageLeft);
						}
						if (startIndexRight >= 0 && startIndexRight < filteredRecipes.Count)
						{
							var gridRight = new RitualRecipeGridPage(filteredRecipes, startIndexRight, Math.Min(12, filteredRecipes.Count - startIndexRight), this);

							gridRight.Width.Set(380, 0);
							gridRight.Height.Set(400, 0);
							gridRight.HAlign = 0.95f;
							gridRight.Top.Set(90, 0);

							recipePageRight = gridRight;
							mainPanel.Append(recipePageRight);
						}
					}
				}
			}
			
			bool isSearching = !string.IsNullOrWhiteSpace(currentSearchText);
			if (isSearching && currentSpread >= 3)
			{
				pageNumberTextLeft.SetText("");
				pageNumberTextRight.SetText("");

				if (mainPanel.HasChild(prevButton)) mainPanel.RemoveChild(prevButton);
				if (mainPanel.HasChild(nextButton)) mainPanel.RemoveChild(nextButton);
			}
			else
			{
				int maxSpreads = GetMaxSpread();

				int logicalLeftPage = isGridMode ? 1 + (currentSpread - 3) * 24 : leftPageIndex + 1;
				int logicalRightPage = isGridMode ? logicalLeftPage + 12 : rightPageIndex + 1;
				
				pageNumberTextLeft.SetText(logicalLeftPage.ToString());
				pageNumberTextRight.SetText(logicalRightPage.ToString());
				
				if (currentSpread > 0 && !mainPanel.HasChild(prevButton)) 
					mainPanel.Append(prevButton);
				else if (currentSpread == 0 && mainPanel.HasChild(prevButton)) 
					mainPanel.RemoveChild(prevButton);
					
				if (currentSpread < maxSpreads && !mainPanel.HasChild(nextButton)) 
					mainPanel.Append(nextButton);
				else if (currentSpread >= maxSpreads && mainPanel.HasChild(nextButton)) 
					mainPanel.RemoveChild(nextButton);
			}
			
			if (currentSpread == 0)
			{
				titleTextLeft.SetText(Language.GetText("Mods.jarkos.UI.RitualBookUI.Title"));
			}
			else
			{
				titleTextLeft.SetText("");
			}
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_needsPageUpdate)
			{
				_needsPageUpdate = false;
				UpdatePageData();
			}

			if (_scaleTween != null)
			{
				_scaleTween.Update(0.016f);
				float t = _scaleTween.Value;
				float easedT = _scaleTween.Eased;

				float yOffset = (1f - easedT) * -150f;
				mainPanel.Top.Set(yOffset, 0f);
				bgPanel.Top.Set(-40f + yOffset, 0f);

				bgPanel.Color = Color.White * t;
				mainPanel.Color = Color.White * t;

				Color textColor = Color.White * t;
				titleTextLeft.TextColor = textColor;
				titleTextRight.TextColor = textColor;
				pageContentTextLeft.TextColor = textColor;
				pageContentTextRight.TextColor = textColor;
				pageNumberTextLeft.TextColor = textColor;
				pageNumberTextRight.TextColor = textColor;

				closeButton.TextColor = Color.White * t;
				closeButton.BackgroundColor = (closeButton.IsMouseHovering ? new Color(220, 80, 80) : new Color(180, 60, 60)) * t;
				closeButton.BorderColor = new Color(80, 20, 20) * t;

				tab1.TextColor = Color.White * t;
				tab1.BackgroundColor = (tab1.IsMouseHovering ? new Color(100, 120, 200) : new Color(73, 94, 171)) * t;
				tab2.TextColor = Color.White * t;
				tab2.BackgroundColor = (tab2.IsMouseHovering ? new Color(100, 120, 200) : new Color(73, 94, 171)) * t;
				tab3.TextColor = Color.White * t;
				tab3.BackgroundColor = (tab3.IsMouseHovering ? new Color(100, 120, 200) : new Color(73, 94, 171)) * t;
				tab4.TextColor = Color.White * t;
				tab4.BackgroundColor = (tab4.IsMouseHovering ? new Color(100, 120, 200) : new Color(73, 94, 171)) * t;

				if (searchPanel != null)
				{
					searchPanel.BackgroundColor = new Color(73, 94, 171) * t;
					searchPanel.BorderColor = new Color(40, 50, 100) * t;
				}
				
				if (gridFilterBtn != null)
				{
					gridFilterBtn.TextColor = Color.White * t;
					if (isGridMode)
						gridFilterBtn.BackgroundColor = new Color(180, 160, 40) * t;
					else
						gridFilterBtn.BackgroundColor = (gridFilterBtn.IsMouseHovering ? new Color(100, 120, 200) : new Color(73, 94, 171)) * t;
				}
				
				if (lineFilterBtn != null)
				{
					lineFilterBtn.TextColor = Color.White * t;
					if (!isGridMode)
						lineFilterBtn.BackgroundColor = new Color(180, 160, 40) * t;
					else
						lineFilterBtn.BackgroundColor = (lineFilterBtn.IsMouseHovering ? new Color(100, 120, 200) : new Color(73, 94, 171)) * t;
				}

				if (prevButton != null) prevButton.Opacity = t;
				if (nextButton != null) nextButton.Opacity = t;

				if (guideImage != null) guideImage.Color = Color.White * t;
				if (guideImageRight != null) guideImageRight.Color = Color.White * t;

				if (rbIcon != null) rbIcon.Opacity = t;
				if (rbIconRight != null) rbIconRight.Opacity = t;
				if (arrowDownImage != null) arrowDownImage.Color = Color.White * t;
				if (arrowDownImageRight != null) arrowDownImageRight.Color = Color.White * t;

				if (exampleIconImage != null) exampleIconImage.Opacity = t;
				if (exampleIconImageRight != null) exampleIconImageRight.Opacity = t;

				if (recipePageLeft is RitualRecipePage r1) r1.Opacity = t;
				else if (recipePageLeft is RitualRecipeGridPage g1) g1.Opacity = t;

				if (recipePageRight is RitualRecipePage r2) r2.Opacity = t;
				else if (recipePageRight is RitualRecipeGridPage g2) g2.Opacity = t;

				if (essenceCatalogPage != null) essenceCatalogPage.Opacity = t;
				if (researchCatalogPage != null) researchCatalogPage.Opacity = t;

				Recalculate();

				if (_isClosing && _scaleTween.Target == 0f && !_scaleTween.Active)
				{
					_isClosing = false;
					ModContent.GetInstance<RitualBookUISystem>().ForceHideBook();
				}
			}

			if (HasChild(recipePopupPanel))
			{
				if (Main.mouseLeft && Main.mouseLeftRelease && !recipePopupPanel.ContainsPoint(Main.MouseScreen))
				{
					RemoveChild(recipePopupPanel);
				}
				else
				{
					Main.LocalPlayer.mouseInterface = true;
				}
			}

			if (mainPanel.ContainsPoint(Main.MouseScreen) || (searchPanel != null && searchPanel.ContainsPoint(Main.MouseScreen)))
			{
				Main.LocalPlayer.mouseInterface = true;
			}
			if (isAnimating)
			{
				animTimer++;
				if (animTimer >= TicksPerFrame)
				{
					animTimer = 0;
					animFrame++;
					if (animFrame > 4)
					{
						animFrame = 0;
						isAnimating = false;
						ShowElements();
					}
					Rectangle newFrame = new Rectangle(0, animFrame * 550, 850, 550);
					mainPanel.SetFrame(newFrame);
				}
			}
			if ((Main.LocalPlayer.dead || Main.playerInventory) && !_isClosing)
			{
				ModContent.GetInstance<RitualBookUISystem>().HideBook();
			}
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			if (!isEssenceCatalog)
			{
				if (recipePageLeft is RitualRecipePage r1) r1.DrawTooltip(spriteBatch);
				else if (recipePageLeft is RitualRecipeGridPage g1) g1.DrawTooltip(spriteBatch);
				
				if (recipePageRight is RitualRecipePage r2) r2.DrawTooltip(spriteBatch);
				else if (recipePageRight is RitualRecipeGridPage g2) g2.DrawTooltip(spriteBatch);
			}
		}
		
		public void OpenRecipePopup(PetrifiedAltarRecipe recipe, int index)
		{
			SoundEngine.PlaySound(SoundID.MenuOpen);
			if (popupRecipePage != null && recipePopupPanel.HasChild(popupRecipePage))
			{
				recipePopupPanel.RemoveChild(popupRecipePage);
			}
			popupRecipePage = new RitualRecipePage(recipe, index);
			popupRecipePage.Width.Set(380, 0);
			popupRecipePage.Height.Set(400, 0);
			popupRecipePage.HAlign = 0.5f;
			popupRecipePage.Top.Set(40, 0);
			recipePopupPanel.Append(popupRecipePage);
			
			if (!HasChild(recipePopupPanel))
			{
				Append(recipePopupPanel);
			}
		}
	}
	public class UIBookImage : UIImageFramed
	{
		private Asset<Texture2D> _textureAsset;
		private Rectangle _frameRect;
		public Color Color = Color.White;

		public UIBookImage(Asset<Texture2D> texture, Rectangle frame) : base(texture, frame)
		{
			_textureAsset = texture;
			_frameRect = frame;
		}

		public new void SetFrame(Rectangle frame)
		{
			_frameRect = frame;
			base.SetFrame(frame);
		}

		public override bool ContainsPoint(Vector2 point)
		{
			foreach (UIElement element in Elements)
			{
				if (element.ContainsPoint(point)) return true;
			}
			return base.ContainsPoint(point);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			Vector2 origin = new Vector2(_frameRect.Width, _frameRect.Height) / 2f;
			Vector2 center = dimensions.Position() + origin;

			spriteBatch.Draw(_textureAsset.Value, center, _frameRect, Color, 0f, origin, 1f, SpriteEffects.None, 0f);
		}
	}
	public class UIAnimatedArrowButton : UIElement
	{
		private Asset<Texture2D> _texture;
		private float _currentScale = 1f;
		public float Opacity = 1f;
		public UIAnimatedArrowButton(Asset<Texture2D> texture)
		{
			_texture = texture;
			Width.Set(_texture.Value.Width, 0f);
			Height.Set(_texture.Value.Height, 0f);
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			float targetScale = IsMouseHovering ? 1.2f : 1f;
			_currentScale = MathHelper.Lerp(_currentScale, targetScale, 0.2f);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			spriteBatch.Draw(_texture.Value, dimensions.Center(), null, Color.White * (IsMouseHovering ? 1f : 0.7f) * Opacity, 0f, _texture.Value.Size() / 2f, _currentScale, SpriteEffects.None, 0f);
		}
	}
	public class UIAnimatedIcon : UIElement
	{
		private Asset<Texture2D> _texture;
		private int _frames;
		private int _ticksPerFrame;
		private int _currentFrame;
		private int _frameTimer;
		public float Opacity = 1f;
		public UIAnimatedIcon(Asset<Texture2D> texture, int width, int height, int frames, int ticksPerFrame)
		{
			_texture = texture;
			_frames = frames;
			_ticksPerFrame = ticksPerFrame;
			Width.Set(width, 0f);
			Height.Set(height, 0f);
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			_frameTimer++;
			if (_frameTimer >= _ticksPerFrame)
			{
				_frameTimer = 0;
				_currentFrame = (_currentFrame + 1) % _frames;
			}
		}
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			int frameHeight = _texture.Value.Height / _frames;
			Rectangle sourceRect = new Rectangle(0, _currentFrame * frameHeight, _texture.Value.Width, frameHeight);
			spriteBatch.Draw(_texture.Value, dimensions.Center(), sourceRect, Color.White * Opacity, 0f, new Vector2(_texture.Value.Width / 2f, frameHeight / 2f), 1f, SpriteEffects.None, 0f);
		}
	}
	public class UIFloatingImage : UIElement
	{
		private Asset<Texture2D> _texture;
		public float Opacity = 1f;
		public UIFloatingImage(Asset<Texture2D> texture)
		{
			_texture = texture;
			Width.Set(_texture.Value.Width, 0f);
			Height.Set(_texture.Value.Height, 0f);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();

			float floatOffset = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 3f;
			Vector2 pos = dimensions.Center() + new Vector2(0, floatOffset);
			
			spriteBatch.Draw(_texture.Value, pos, null, Color.White * Opacity, 0f, _texture.Value.Size() / 2f, 1f, SpriteEffects.None, 0f);
		}
	}
	public class RitualRecipePage : UIElement
	{
		private class RecipeStarParticle
		{
			public Vector2 Position;
			public float Life;
			public float MaxLife;
			public float Scale;
			public float Amplitude;
			public float Frequency;
			public float BaseX;
		}
		private PetrifiedAltarRecipe _recipe;
		public float Opacity = 1f;
		private int _recipeIndex;
		private float _tooltipAlpha = 0f;
		private Vector2 _lastResPos;
		private List<RecipeStarParticle> _nameParticles = new List<RecipeStarParticle>();
		public RitualRecipePage(PetrifiedAltarRecipe recipe, int recipeIndex)
		{
			_recipe = recipe;
			_recipeIndex = recipeIndex;
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (Opacity <= 0) return;
			if (_recipe == null) return;

			string itemName = Lang.GetItemNameValue(_recipe.ResultItem);
			Vector2 textSize = FontAssets.MouseText.Value.MeasureString(itemName);

			if (Main.rand.NextBool(4))
			{
				_nameParticles.Add(new RecipeStarParticle
				{
					Life = 0f, MaxLife = Main.rand.NextFloat(40f, 90f), Scale = Main.rand.NextFloat(0.02f, 0.08f), Amplitude = Main.rand.NextFloat(3f, 8f), Frequency = Main.rand.NextFloat(0.05f, 0.15f), BaseX = Main.rand.NextFloat(-textSize.X / 2f, textSize.X / 2f), Position = new Vector2(0, Main.rand.NextFloat(0, 10f))
				});
			}
			for (int i = _nameParticles.Count - 1; i >= 0; i--)
			{
				var p = _nameParticles[i];
				p.Life++;
				p.Position.Y -= Main.rand.NextFloat(0.3f, 0.8f);

				if (p.Life >= p.MaxLife || p.Position.Y < -30f)
				{
					_nameParticles.RemoveAt(i);
				}
			}
		}
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (_recipe == null) return;

			CalculatedStyle dims = GetDimensions();
			Vector2 center = dims.Center() + new Vector2(0, 30f);

			float scale = 1.3f;
			bool isResearched = true;
			string reqNode = PetrifiedAltarTileEntity.GetRequiredNode(_recipe.ResultItem);

			if (reqNode != null)
			{
				isResearched = false;
				try 
				{
					var plr = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
					if (plr != null) isResearched = plr.IsResearched(reqNode);
				} 
				catch
				{
					
				}
			}

			Texture2D texGrid = null;
			Texture2D texAltar = null;
			Texture2D texMinor = null;

			try 
			{
				texGrid = ModContent.Request<Texture2D>("jarkos/UI/GuideRecipeGrid").Value;
				texAltar = ModContent.Request<Texture2D>("jarkos/UI/PetrifiedAltarItem_Guide").Value;
				texMinor = ModContent.Request<Texture2D>("jarkos/UI/PetrifiedMinorAltarTile_Guide").Value;
			} 
			catch 
			{
				try 
				{
					texGrid = ModContent.Request<Texture2D>("jarkos/UI/GuideRecipeGrid").Value;
					texAltar = ModContent.Request<Texture2D>("jarkos/UI/PetrifiedAltarItem_Guide").Value;
					texMinor = ModContent.Request<Texture2D>("jarkos/UI/PetrifiedMinorAltarTile_Guide").Value;
				} 
				catch 
				{
					return;
				}
			}
			
			if (!isResearched)
			{
				if (texGrid != null)
					spriteBatch.Draw(texGrid, center, null, Color.White * Opacity, 0f, texGrid.Size() / 2f, scale, SpriteEffects.None, 0f);
				try 
				{
					var fogEffectRef = ModContent.Request<Effect>("jarkos/Shaders/RecipeFog", AssetRequestMode.ImmediateLoad);

					if (fogEffectRef != null && fogEffectRef.Value != null)
					{
						Effect fogEffect = fogEffectRef.Value;
						fogEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);

						try 
						{
							Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/textures/Noises/Noise01").Value;
							Main.graphics.GraphicsDevice.Textures[1] = noiseTex;
							Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
						} 
						catch
						{
							
						}
						spriteBatch.End();
						spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fogEffect, Main.UIScaleMatrix);
						Rectangle fogArea = new Rectangle((int)center.X - 160, (int)center.Y - 140, 320, 200);

						spriteBatch.Draw(TextureAssets.MagicPixel.Value, fogArea, Color.White * Opacity);
						spriteBatch.End();
						spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

						if (fogArea.Contains(Main.MouseScreen.ToPoint()))
						{
							Main.hoverItemName = RitualBookLanguageSystem.RBstill_not_rsched?.Value;
						}
					}
				} catch {}
				return;
			}
			if (texAltar != null)
				spriteBatch.Draw(texAltar, center, null, Color.White * Opacity, 0f, texAltar.Size() / 2f, scale, SpriteEffects.None, 0f);

			List<Action> postDrawText = new List<Action>();

			void DrawMinor(int dx, int dy, int itemType, int amount = 1)
			{
				Vector2 pos = center + new Vector2((dx - 0.5f) * 16f * scale, (dy + 1f) * 16f * scale);

				if (texMinor != null)
					spriteBatch.Draw(texMinor, pos, null, Color.White * Opacity, 0f, texMinor.Size() / 2f, scale, SpriteEffects.None, 0f);

				Main.instance.LoadItem(itemType);
				var itemTex = TextureAssets.Item[itemType].Value;

				Rectangle rect = Main.itemAnimations[itemType] != null ? Main.itemAnimations[itemType].GetFrame(itemTex) : itemTex.Frame(1, 1, 0, 0);

				float maxDim = Math.Max(rect.Width, rect.Height);
				float itemScale = scale;

				if (maxDim > 22f)
				{
					itemScale = scale * (22f / maxDim);
				}

				float floatOffset = (float)Math.Sin(Main.GameUpdateCount * 0.05f + dx * 2 + dy) * 4f;

				Vector2 itemPos = pos + new Vector2(0, -22f * scale + floatOffset);

				spriteBatch.Draw(itemTex, itemPos, rect, Color.White * Opacity, 0f, rect.Size() / 2f, itemScale, SpriteEffects.None, 0f);

				if (amount > 1)
				{
					postDrawText.Add(() => {
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, amount.ToString(), itemPos + new Vector2(8, 8) * scale, Color.White * Opacity, 0f, Vector2.Zero, new Vector2(0.8f * scale));
					});
				}

				if (Terraria.Utils.CenteredRectangle(itemPos, rect.Size() * itemScale).Contains(Main.MouseScreen.ToPoint()))
				{
					Main.hoverItemName = Lang.GetItemNameValue(itemType);
				}
			}
			foreach (var strict in _recipe.StrictIngredients)
			{
				DrawMinor(strict.Dx, strict.Dy, strict.ItemType, strict.Amount);
			}
			int leftDx = -3;
			foreach (var itemType in _recipe.LeftIngredients)
			{
				DrawMinor(leftDx, -1, itemType);
				leftDx -= 1;
			}
			int rightDx = 4;
			foreach (var itemType in _recipe.RightIngredients)
			{
				DrawMinor(rightDx, -1, itemType);
				rightDx += 1;
			}
			if (texGrid != null)
				spriteBatch.Draw(texGrid, center, null, Color.White * Opacity, 0f, texGrid.Size() / 2f, scale, SpriteEffects.None, 0f);
			foreach (var action in postDrawText)
			{
				action();
			}
			Main.instance.LoadItem(_recipe.ResultItem);

			var resTex = TextureAssets.Item[_recipe.ResultItem].Value;
			Rectangle resRect = Main.itemAnimations[_recipe.ResultItem] != null ? Main.itemAnimations[_recipe.ResultItem].GetFrame(resTex) : resTex.Frame(1, 1, 0, 0);
			Vector2 resPos = center + new Vector2(0, -80f * scale + (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 5f);

			_lastResPos = resPos;
			float resMaxDim = Math.Max(resRect.Width, resRect.Height);
			float resScale = scale * 1.1f;

			if (resMaxDim > 36f)
			{
				resScale = scale * 1.1f * (36f / resMaxDim);
			}

			Common.Draw.RadialGlow(spriteBatch, resPos, 60f * scale, new Color(60, 40, 20, (int)(140 * Opacity)));

			for (int i = 0; i < 8; i++)
			{
				int t = (int)(Main.GameUpdateCount + i * 15) % 120;

				float life = t / 120f;
				float alpha = 1f - life;
				float pScale = alpha * 5f * scale;
				float startX = (float)Math.Sin(i * 12345) * 20f * scale;

				float pX = startX + (float)Math.Sin(t * 0.05f + i) * 10f * scale;
				float pY = -life * 50f * scale;

				Vector2 pPos = resPos + new Vector2(pX, pY);
				Color pColor = new Color(255, 220, 50, (int)(255 * alpha * Opacity));

				spriteBatch.Draw(TextureAssets.MagicPixel.Value, pPos, new Rectangle(0, 0, 1, 1), pColor, 0f, new Vector2(0.5f, 0.5f), pScale, SpriteEffects.None, 0f);
			}

			spriteBatch.Draw(resTex, resPos, resRect, Color.White * Opacity, 0f, resRect.Size() / 2f, resScale, SpriteEffects.None, 0f);

			if (_recipe.ResultStack > 1)
			{
				Terraria.Utils.DrawBorderString(spriteBatch, _recipe.ResultStack.ToString(), resPos + new Vector2(10, 10), Color.White * Opacity, 1f);
			}

			string itemName = Lang.GetItemNameValue(_recipe.ResultItem);
			var font = FontAssets.MouseText.Value;

			Vector2 textSize = font.MeasureString(itemName);
			Vector2 textBasePos = resPos + new Vector2(0, -resMaxDim/2f * resScale - 25f * scale);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
			Texture2D glowTex = null;

			try { glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow").Value; } 
			catch {}

			if (glowTex != null)
			{
				Color glowColor = new Color(255, 200, 100) * Opacity * 0.4f;
				spriteBatch.Draw(glowTex, textBasePos, null, glowColor, 0f, glowTex.Size() / 2f, new Vector2(textSize.X / glowTex.Width * 1.5f, 0.4f) * scale, SpriteEffects.None, 0f);
			}

			Texture2D starTex = null;

			try { starTex = ModContent.Request<Texture2D>("jarkos/textures/StarTexture").Value; } 
			catch {}

			if (starTex != null)
			{
				foreach (var p in _nameParticles)
				{
					float alpha = 1f;

					if (p.Life < 15f) alpha = p.Life / 15f;
					else if (p.Life > p.MaxLife - 15f) alpha = (p.MaxLife - p.Life) / 15f;

					float waveX = (float)Math.Sin(p.Life * p.Frequency) * p.Amplitude * scale;
					Vector2 pDrawPos = textBasePos + new Vector2(p.BaseX * scale + waveX, p.Position.Y * scale);

					spriteBatch.Draw(starTex, pDrawPos, null, Color.White * Opacity * alpha, 0f, starTex.Size() / 2f, p.Scale * scale, SpriteEffects.None, 0f);
				}
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

			for (int i = 0; i < itemName.Length; i++)
			{
				string c = itemName[i].ToString();
				float charOffsetX = (i == 0) ? 0 : font.MeasureString(itemName.Substring(0, i)).X * scale;
				float waveY = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + i * 0.4f) * 2.5f * scale;

				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, c, new Vector2(textBasePos.X - textSize.X / 2f * scale + charOffsetX, textBasePos.Y - textSize.Y / 2f * scale + waveY), Color.White * Opacity, 0f, Vector2.Zero, new Vector2(scale));
			}
			if (Terraria.Utils.CenteredRectangle(resPos, resRect.Size() * resScale).Contains(Main.MouseScreen.ToPoint()))
			{
				_tooltipAlpha = MathHelper.Clamp(_tooltipAlpha + 0.1f, 0f, 1f);
			}
			else
			{
				_tooltipAlpha = MathHelper.Clamp(_tooltipAlpha - 0.1f, 0f, 1f);
			}
		}
		public void DrawTooltip(SpriteBatch spriteBatch)
		{
			if (_tooltipAlpha <= 0f) return;
			string itemName = Lang.GetItemNameValue(_recipe.ResultItem);
			string itemDesc = "";
			var tt = Lang.GetTooltip(_recipe.ResultItem);
			if (tt != null)
			{
				for (int j = 0; j < tt.Lines; j++)
				{
					itemDesc += tt.GetLine(j) + (j < tt.Lines - 1 ? "\n" : "");
				}
			}
			Vector2 nameSize = FontAssets.MouseText.Value.MeasureString(itemName);
			Vector2 descSize = string.IsNullOrEmpty(itemDesc) ? Vector2.Zero : FontAssets.MouseText.Value.MeasureString(itemDesc);

			float width = Math.Max(nameSize.X, descSize.X) + 30f;
			float height = nameSize.Y + (string.IsNullOrEmpty(itemDesc) ? 0 : descSize.Y + 10f) + 20f;
			float easeT = 1f - (float)Math.Pow(1f - _tooltipAlpha, 3);

			Vector2 startPos = _lastResPos;
			Vector2 targetPos = Main.MouseScreen + new Vector2(24, 24);

			if (targetPos.X + width > Main.screenWidth) targetPos.X = Main.screenWidth - width;
			if (targetPos.Y + height > Main.screenHeight) targetPos.Y = Main.screenHeight - height;
			if (targetPos.X < 0) targetPos.X = 0;
			if (targetPos.Y < 0) targetPos.Y = 0;
			Vector2 tooltipPos = Vector2.Lerp(startPos, targetPos, easeT);
			Rectangle bgRect = new Rectangle((int)tooltipPos.X, (int)tooltipPos.Y, (int)width, (int)height);
			Terraria.Utils.DrawInvBG(spriteBatch, bgRect, new Color(40, 30, 50, (int)(220 * _tooltipAlpha * Opacity)));
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, itemName, tooltipPos + new Vector2(15, 10), Color.White * _tooltipAlpha * Opacity, 0f, Vector2.Zero, Vector2.One);
			if (!string.IsNullOrEmpty(itemDesc))
			{
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, itemDesc, tooltipPos + new Vector2(15, 10 + nameSize.Y + 10), new Color(180, 180, 180) * _tooltipAlpha * Opacity, 0f, Vector2.Zero, Vector2.One);
			}
		}
	}
	public class EssenceCatalogPage : UIElement
	{
		public float Opacity = 1f;
		private EssenceType hoveredEssence = (EssenceType)(-1);
		private EssenceType popupEssence = (EssenceType)(-1);
		private float popupAnim = 0f;
		private bool isPopupOpening = false;
		private bool isPopupOpen = false;
		private float[] essenceScales;
		private float[] essenceRotations;
		private Asset<Texture2D> circularGlowTex;
		private Asset<Texture2D> starTex;
		private Asset<Texture2D> ringSpinningTex;
		private class CatalogStar
		{
			public Vector2 Pos;
			public Vector2 Vel;
			public float Life;
			public float MaxLife;
			public float Scale;
			public float Opacity;
		}
		private List<CatalogStar> popupStars = new List<CatalogStar>();
		private (EssenceType e1, EssenceType e2)? GetRecipeFor(EssenceType target)
		{
			if (target == EssenceType.Pyros) return (jarkos.core.Data.EssenceType.Aer, jarkos.core.Data.EssenceType.Lux);
			if (target == EssenceType.Umbra) return (jarkos.core.Data.EssenceType.Aer, jarkos.core.Data.EssenceType.Terra);
			if (target == EssenceType.Cruor) return (jarkos.core.Data.EssenceType.Aer, jarkos.core.Data.EssenceType.Terra);
			if (target == EssenceType.Venenum) return (jarkos.core.Data.EssenceType.Aer, jarkos.core.Data.EssenceType.Vita);
			if (target == EssenceType.Arena) return (jarkos.core.Data.EssenceType.Anima, jarkos.core.Data.EssenceType.Terra);
			if (target == EssenceType.Glacies) return (jarkos.core.Data.EssenceType.Aqua, jarkos.core.Data.EssenceType.Aqua);
			if (target == EssenceType.Fungus) return (jarkos.core.Data.EssenceType.Terra, jarkos.core.Data.EssenceType.Venenum);
			if (target == EssenceType.Anima) return (jarkos.core.Data.EssenceType.Vita, jarkos.core.Data.EssenceType.Cruor);
			if (target == EssenceType.Regalis) return (jarkos.core.Data.EssenceType.Arena, jarkos.core.Data.EssenceType.Pyros);
			if (target == EssenceType.Acus) return (jarkos.core.Data.EssenceType.Venenum, jarkos.core.Data.EssenceType.Aqua);
			if (target == EssenceType.Caro) return (jarkos.core.Data.EssenceType.Pyros, jarkos.core.Data.EssenceType.Anima);
			if (target == EssenceType.Mechanus) return (jarkos.core.Data.EssenceType.Pyros, jarkos.core.Data.EssenceType.Anima);
			if (target == EssenceType.Lux) return (jarkos.core.Data.EssenceType.Pyros, jarkos.core.Data.EssenceType.Aer);
			if (target == EssenceType.Fulgur) return (jarkos.core.Data.EssenceType.Aer, jarkos.core.Data.EssenceType.Arena);
			if (target == EssenceType.Silva) return (EssenceType.Venenum, jarkos.core.Data.EssenceType.Vita);
			if (target == EssenceType.Chitinus) return (EssenceType.Acus, jarkos.core.Data.EssenceType.Aqua);
			if (target == EssenceType.Pelagus) return (EssenceType.Glacies, jarkos.core.Data.EssenceType.Aer);
			if (target == EssenceType.Imperium) return (jarkos.core.Data.EssenceType.Regalis, jarkos.core.Data.EssenceType.Aer);
			return null;
		}
		public EssenceCatalogPage()
		{
			var values = Enum.GetValues(typeof(EssenceType));
			essenceScales = new float[values.Length];
			essenceRotations = new float[values.Length];
			for(int i = 0; i < essenceScales.Length; i++) essenceScales[i] = 1f;
			if (!Main.dedServ)
			{
				circularGlowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad);
				try {
					ringSpinningTex = ModContent.Request<Texture2D>("jarkos/textures/RingSpinning", AssetRequestMode.ImmediateLoad);
				} catch {}
				try {
					starTex = ModContent.Request<Texture2D>("jarkos/particles/StarParticle", AssetRequestMode.ImmediateLoad);
				} catch {}
			}
		}
		public void OnShow()
		{
			ClosePopup();
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (isPopupOpening)
			{
				popupAnim += 0.08f;
				if (popupAnim >= 1f)
				{
					popupAnim = 1f;
					isPopupOpening = false;
					isPopupOpen = true;
				}
			}
			else if (!isPopupOpen && popupAnim > 0f)
			{
				popupAnim -= 0.1f;
				if (popupAnim < 0f) popupAnim = 0f;
			}
			var values = (EssenceType[])Enum.GetValues(typeof(EssenceType));
			for (int i = 0; i < values.Length; i++)
			{
				bool isHovered = hoveredEssence == values[i] && !isPopupOpening && !isPopupOpen;
				float targetScale = isHovered ? 0.85f : 1f;
				essenceScales[i] = MathHelper.Lerp(essenceScales[i], targetScale, 0.15f);
				if (isHovered)
				{
					essenceRotations[i] = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.1f;
				}
				else
				{
					essenceRotations[i] = MathHelper.Lerp(essenceRotations[i], 0f, 0.1f);
				}
			}
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			CalculatedStyle dims = GetDimensions();
			if (isPopupOpen || isPopupOpening)
			{
				if (Main.rand.NextBool(3))
				{
					popupStars.Add(new CatalogStar() {
						Pos = new Vector2(Main.rand.NextFloat(-60, 60), Main.rand.NextFloat(-10, 20)), Vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-1.5f, -0.5f)), Life = 0f, MaxLife = Main.rand.NextFloat(20, 50), Scale = Main.rand.NextFloat(0.01f, 0.08f), Opacity = 0f
					});
				}
				for (int i = popupStars.Count - 1; i >= 0; i--)
				{
					popupStars[i].Pos += popupStars[i].Vel;
					popupStars[i].Life++;
					float lifeRatio = popupStars[i].Life / popupStars[i].MaxLife;
					if (lifeRatio < 0.2f) popupStars[i].Opacity = lifeRatio / 0.2f;
					else if (lifeRatio > 0.8f) popupStars[i].Opacity = (1f - lifeRatio) / 0.2f;
					else popupStars[i].Opacity = 1f;
					if (popupStars[i].Life >= popupStars[i].MaxLife)
						popupStars.RemoveAt(i);
				}
			}
			else
			{
				popupStars.Clear();
			}
			var values = (EssenceType[])Enum.GetValues(typeof(EssenceType));
			int cols = 9;
			int spacingX = 64;
			int spacingY = 64;
			float startX = dims.X + dims.Width / 2f - cols * spacingX / 2f + spacingX / 2f;
			float startY = dims.Y + 140f;
			hoveredEssence = (EssenceType)(-1);
			ResearchPlayer rPlayer = null;

			try { rPlayer = Main.LocalPlayer.GetModPlayer<ResearchPlayer>(); } catch { }
			if (rPlayer == null) return;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

			for (int i = 0; i < values.Length; i++)
			{
				EssenceType essence = values[i];
				if (!rPlayer.DiscoveredEssences.Contains((int)essence)) continue;
				int col = i % cols;
				int row = i / cols;
				Vector2 pos = new Vector2(startX + col * spacingX, startY + row * spacingY);
				float scale = essenceScales[i];
				if (scale < 0.99f && circularGlowTex != null)
				{
					Color glowColor = EssencePalette.ColorOf(essence) * Opacity * (1f - scale) * 3f;
					spriteBatch.Draw(circularGlowTex.Value, pos, null, glowColor, 0f, circularGlowTex.Value.Size() / 2f, 0.4f, SpriteEffects.None, 0f);
				}
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
			for (int i = 0; i < values.Length; i++)
			{
				EssenceType essence = values[i];
				bool isResearched = rPlayer.DiscoveredEssences.Contains((int)essence);
				int col = i % cols;
				int row = i / cols;
				Vector2 pos = new Vector2(startX + col * spacingX, startY + row * spacingY);
				Texture2D tex = EssencePalette.GetTexture(essence);
				if (tex != null)
				{
					Rectangle hitBox = new Rectangle((int)pos.X - 24, (int)pos.Y - 24, 48, 48);
					bool hovering = hitBox.Contains(Main.MouseScreen.ToPoint()) && popupAnim <= 0f;
					if (hovering) hoveredEssence = essence;
					Color drawColor = isResearched ? Color.White * Opacity : Color.Black * Opacity;
					spriteBatch.Draw(tex, pos, null, drawColor, essenceRotations[i], tex.Size() / 2f, essenceScales[i], SpriteEffects.None, 0f);
					if (hovering)
					{
						Main.hoverItemName = isResearched ? EssencePalette.DisplayName(essence) : "?";
						if (isResearched && Main.mouseLeft && Main.mouseLeftRelease)
						{
							SoundEngine.PlaySound(SoundID.MenuOpen);
							popupEssence = essence;
							isPopupOpening = true;
							isPopupOpen = false;
						}
					}
				}
			}
			string title = LangSystem.StudiedEssences.Value;
			Vector2 tSize = FontAssets.DeathText.Value.MeasureString(title) * 0.7f;
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, title, new Vector2(dims.X + dims.Width / 2f - tSize.X / 2f, dims.Y + 60f), new Color(200, 200, 200) * Opacity, 0f, Vector2.Zero, new Vector2(0.7f));
			if (popupAnim > 0f)
			{
				DrawPopup(spriteBatch, dims);
			}
		}
		private void DrawPopup(SpriteBatch spriteBatch, CalculatedStyle dims)
		{
			Vector2 center = new Vector2(dims.X + dims.Width / 2f, dims.Y + dims.Height / 2f);
			float scale = popupAnim;
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, dims.ToRectangle(), Color.Black * 0.4f * popupAnim);
			Rectangle popupRect = new Rectangle((int)(center.X - 150 * scale), (int)(center.Y - 100 * scale), (int)(300 * scale), (int)(200 * scale));
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, popupRect, new Color(40, 30, 30) * 0.95f * popupAnim);
			spriteBatch.End();
			Rectangle oldScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
			Rectangle scissorRect = new Rectangle((int)(popupRect.X * Main.UIScale), (int)(popupRect.Y * Main.UIScale), (int)(popupRect.Width * Main.UIScale), (int)(popupRect.Height * Main.UIScale));
			scissorRect = Rectangle.Intersect(scissorRect, spriteBatch.GraphicsDevice.ScissorRectangle);
			RasterizerState scissorState = new RasterizerState { ScissorTestEnable = true };
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, scissorState, null, Main.UIScaleMatrix);
			spriteBatch.GraphicsDevice.ScissorRectangle = scissorRect;
			if (ringSpinningTex != null)
			{
				float vortexRot = Main.GlobalTimeWrappedHourly * 2.5f;
				Vector2 vortexCenter = new Vector2(popupRect.Center.X, popupRect.Center.Y);
				Color vortexColor = new Color(255, 180, 80) * 0.4f * popupAnim;
				float texWidth = ringSpinningTex.Value.Width;
				float scaleBg = (popupRect.Width * 1.5f) / texWidth * popupAnim;
				spriteBatch.Draw(ringSpinningTex.Value, vortexCenter, null, vortexColor, vortexRot, ringSpinningTex.Value.Size() / 2f, scaleBg, SpriteEffects.None, 0f);
				spriteBatch.Draw(ringSpinningTex.Value, vortexCenter, null, vortexColor * 1.2f, -vortexRot * 1.5f, ringSpinningTex.Value.Size() / 2f, scaleBg * 0.6f, SpriteEffects.None, 0f);
			}
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = oldScissor;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(popupRect.X, popupRect.Y, popupRect.Width, 2), Color.Gold * popupAnim);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(popupRect.X, popupRect.Bottom - 2, popupRect.Width, 2), Color.Gold * popupAnim);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(popupRect.X, popupRect.Y, 2, popupRect.Height), Color.Gold * popupAnim);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(popupRect.Right - 2, popupRect.Y, 2, popupRect.Height), Color.Gold * popupAnim);
			if (popupAnim >= 0.95f)
			{
				if (Main.mouseLeft && Main.mouseLeftRelease && !popupRect.Contains(Main.MouseScreen.ToPoint()))
				{
					ClosePopup();
				}
				string titleText = EssencePalette.DisplayName(popupEssence);
				Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(titleText);
				Vector2 titlePos = new Vector2(center.X - titleSize.X / 2f, popupRect.Y + 15);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
				if (circularGlowTex != null)
				{
					spriteBatch.Draw(circularGlowTex.Value, titlePos + titleSize / 2f, null, EssencePalette.ColorOf(popupEssence) * popupAnim * 0.5f, 0f, circularGlowTex.Value.Size() / 2f, 0.35f * popupAnim, SpriteEffects.None, 0f);
				}
				if (starTex != null)
				{
					foreach (var star in popupStars)
					{
						spriteBatch.Draw(starTex.Value, titlePos + titleSize / 2f + star.Pos, null, Color.White * star.Opacity * popupAnim, 0f, starTex.Value.Size() / 2f, star.Scale, SpriteEffects.None, 0f);
					}
				}
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, titleText, titlePos, jarkos.core.Data.EssencePalette.ColorOf(popupEssence), 0f, Vector2.Zero, Vector2.One);
				var recipe = GetRecipeFor(popupEssence);
				if (recipe != null)
				{
					string rText = LangSystem.CombinerRecipe.Value;
					Vector2 rSize = FontAssets.MouseText.Value.MeasureString(rText);
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, rText, new Vector2(center.X - rSize.X / 2f, popupRect.Y + 50), Color.LightGray, 0f, Vector2.Zero, new Vector2(0.9f));
					DrawPopupEssence(spriteBatch, recipe.Value.e1, new Vector2(center.X - 70, center.Y + 30));
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "+", new Vector2(center.X - 10, center.Y + 18), Color.White, 0f, Vector2.Zero, Vector2.One);
					DrawPopupEssence(spriteBatch, recipe.Value.e2, new Vector2(center.X, center.Y + 30));
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "=", new Vector2(center.X + 35, center.Y + 18), Color.White, 0f, Vector2.Zero, Vector2.One);
					DrawPopupEssence(spriteBatch, popupEssence, new Vector2(center.X + 70, center.Y + 30));
				}
				else
				{
					string rText = LangSystem.BaseEssence.Value;
					Vector2 rSize = FontAssets.MouseText.Value.MeasureString(rText);
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, rText, new Vector2(center.X - rSize.X / 2f, center.Y), Color.LightGray, 0f, Vector2.Zero, Vector2.One);
				}
				string cText = LangSystem.ClickToClose.Value;
				Vector2 cSize = FontAssets.MouseText.Value.MeasureString(cText);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, cText, new Vector2(center.X - cSize.X / 2f * 0.7f, popupRect.Bottom - 25), Color.Gray, 0f, Vector2.Zero, new Vector2(0.7f));
			}
		}
		private void DrawPopupEssence(SpriteBatch spriteBatch, EssenceType type, Vector2 pos)
		{
			Texture2D tex = EssencePalette.GetTexture(type);
			if (tex != null)
			{
				spriteBatch.Draw(tex, pos, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
				Rectangle hitBox = new Rectangle((int)pos.X - 16, (int)pos.Y - 16, 32, 32);
				if (hitBox.Contains(Main.MouseScreen.ToPoint()))
				{
					Main.hoverItemName = EssencePalette.DisplayName(type);
				}
			}
		}
		private void ClosePopup()
		{
			if (isPopupOpen || isPopupOpening)
			{
				isPopupOpen = false;
				isPopupOpening = false;
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
		}
	}

	public class ResearchCatalogPage : UIElement
	{
		private class NodeSlot
		{
			public ConstellationEntry Entry;
			public bool IsSubNode;
			public int BranchIndex;
			public int NodeIndex;
		}

		public float Opacity = 1f;
		private NodeSlot hoveredSlot = null;
		private NodeSlot popupSlot = null;
		private float popupAnim = 0f;
		private bool isPopupOpening = false;
		private bool isPopupOpen = false;
		private Dictionary<string, float> nodeScales = new();
		private Dictionary<string, float> nodeRotations = new();
		private Asset<Texture2D> circularGlowTex;
		private Asset<Texture2D> starTex;
		private Asset<Texture2D> ringSpinningTex;
		private List<NodeSlot> allNodeSlots;

		private class CatalogStar
		{
			public Vector2 Pos;
			public Vector2 Vel;
			public float Life;
			public float MaxLife;
			public float Scale;
			public float Opacity;
		}
		private List<CatalogStar> popupStars = new List<CatalogStar>();

		public ResearchCatalogPage()
		{
			if (!Main.dedServ)
			{
				circularGlowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad);
				try { ringSpinningTex = ModContent.Request<Texture2D>("jarkos/textures/RingSpinning", AssetRequestMode.ImmediateLoad); } catch {}
				try { starTex = ModContent.Request<Texture2D>("jarkos/particles/StarParticle", AssetRequestMode.ImmediateLoad); } catch {}
			}
		}

		public void OnShow()
		{
			ClosePopup();
			LoadNodeSlots();
		}

		private void LoadNodeSlots()
		{
			allNodeSlots = new List<NodeSlot>();
			var registry = ConstellationRegistry.All.Values;

			EssenceType[] branches = new EssenceType[] {
				EssenceType.Vita,
				EssenceType.Pyros,
				EssenceType.Venenum,
				EssenceType.Umbra,
				EssenceType.Aqua
			};

			for (int b = 0; b < branches.Length; b++)
			{
				var branchType = branches[b];

				for (int i = 1; i <= 8; i++)
				{
					string targetName = branchType.ToString() + "Node" + i;
					var entry = registry.FirstOrDefault(e => e.GetType().Name == targetName);
					if (entry != null)
					{
						allNodeSlots.Add(new NodeSlot {
							Entry = entry,
							IsSubNode = false,
							BranchIndex = b,
							NodeIndex = i - 1
						});
					}
				}

				for (int i = 1; i <= 4; i++)
				{
					string targetName = branchType.ToString() + "Sub1Node" + i;
					var entry = registry.FirstOrDefault(e => e.GetType().Name == targetName);
					if (entry != null)
					{
						allNodeSlots.Add(new NodeSlot {
							Entry = entry,
							IsSubNode = true,
							BranchIndex = b,
							NodeIndex = i - 1
						});
					}
				}
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (allNodeSlots == null) LoadNodeSlots();

			if (isPopupOpening)
			{
				popupAnim += 0.08f;
				if (popupAnim >= 1f)
				{
					popupAnim = 1f;
					isPopupOpening = false;
					isPopupOpen = true;
				}
			}
			else if (!isPopupOpen && popupAnim > 0f)
			{
				popupAnim -= 0.1f;
				if (popupAnim < 0f) popupAnim = 0f;
			}

			if (allNodeSlots != null)
			{
				foreach (var slot in allNodeSlots)
				{
					string key = slot.Entry.ID;
					if (!nodeScales.ContainsKey(key)) nodeScales[key] = 1f;
					if (!nodeRotations.ContainsKey(key)) nodeRotations[key] = 0f;

					bool isHovered = hoveredSlot == slot && !isPopupOpening && !isPopupOpen;
					float targetScale = isHovered ? 1.15f : 1f;
					nodeScales[key] = MathHelper.Lerp(nodeScales[key], targetScale, 0.15f);
					nodeRotations[key] = isHovered ? (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.1f : MathHelper.Lerp(nodeRotations[key], 0f, 0.1f);
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			CalculatedStyle dims = GetDimensions();
			
			if (allNodeSlots == null) LoadNodeSlots();

			if (isPopupOpen || isPopupOpening)
			{
				if (Main.rand.NextBool(3))
				{
					popupStars.Add(new CatalogStar() {
						Pos = new Vector2(Main.rand.NextFloat(-80, 80), Main.rand.NextFloat(-20, 30)), Vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-1.5f, -0.5f)), Life = 0f, MaxLife = Main.rand.NextFloat(20, 50), Scale = Main.rand.NextFloat(0.01f, 0.08f), Opacity = 0f
					});
				}
				for (int i = popupStars.Count - 1; i >= 0; i--)
				{
					popupStars[i].Pos += popupStars[i].Vel;
					popupStars[i].Life++;
					float lifeRatio = popupStars[i].Life / popupStars[i].MaxLife;
					if (lifeRatio < 0.2f) popupStars[i].Opacity = lifeRatio / 0.2f;
					else if (lifeRatio > 0.8f) popupStars[i].Opacity = (1f - lifeRatio) / 0.2f;
					else popupStars[i].Opacity = 1f;
					if (popupStars[i].Life >= popupStars[i].MaxLife) popupStars.RemoveAt(i);
				}
			}
			else
			{
				popupStars.Clear();
			}

			int spacingX = 40;
			int spacingY = 56;
			float startX = dims.X + 160f;
			float startY = dims.Y + 115f;

			hoveredSlot = null;
			ResearchPlayer rPlayer = null;
			try { rPlayer = Main.LocalPlayer.GetModPlayer<ResearchPlayer>(); } catch { }
			if (rPlayer == null) return;

			string[] branchLabels = new string[] { "Vita", "Pyros", "Venenum", "Umbra", "Aqua" };
			for (int r = 0; r < branchLabels.Length; r++)
			{
				Vector2 labelPos = new Vector2(startX - 95f, startY + r * spacingY - 8f);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, branchLabels[r], labelPos, Color.Gold * Opacity, 0f, Vector2.Zero, new Vector2(0.85f));

				float subSeparatorX = startX + 8 * spacingX + 8f;
				Vector2 subLabelPos = new Vector2(subSeparatorX - 5f, startY + r * spacingY - 14f);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "|", subLabelPos, Color.Gray * Opacity * 0.5f, 0f, Vector2.Zero, new Vector2(0.7f, 1.2f));
			}

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

			foreach (var slot in allNodeSlots)
			{
				bool isResearched = rPlayer.IsResearched(slot.Entry.ID);
				if (!isResearched) continue;

				float posX = slot.IsSubNode ? (startX + 8 * spacingX + 24f + slot.NodeIndex * spacingX) : (startX + slot.NodeIndex * spacingX);
				float posY = startY + slot.BranchIndex * spacingY;
				Vector2 pos = new Vector2(posX, posY);

				if (circularGlowTex != null)
				{
					Color glowColor = EssencePalette.ColorOf(slot.Entry.Tier) * Opacity * (slot.IsSubNode ? 0.9f : 0.7f);
					spriteBatch.Draw(circularGlowTex.Value, pos, null, glowColor, 0f, circularGlowTex.Value.Size() / 2f, slot.IsSubNode ? 0.28f : 0.35f, SpriteEffects.None, 0f);
				}
			}

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

			foreach (var slot in allNodeSlots)
			{
				bool isResearched = rPlayer.IsResearched(slot.Entry.ID);

				float posX = slot.IsSubNode ? (startX + 8 * spacingX + 24f + slot.NodeIndex * spacingX) : (startX + slot.NodeIndex * spacingX);
				float posY = startY + slot.BranchIndex * spacingY;
				Vector2 pos = new Vector2(posX, posY);

				Texture2D tex = EssencePalette.GetTexture(slot.Entry.Tier);
				if (tex != null)
				{
					Rectangle hitBox = new Rectangle((int)pos.X - 16, (int)pos.Y - 16, 32, 32);
					bool hovering = hitBox.Contains(Main.MouseScreen.ToPoint()) && popupAnim <= 0f;

					if (hovering)
					{
						hoveredSlot = slot;
						Main.hoverItemName = isResearched ? slot.Entry.DisplayName : RitualBookLanguageSystem.RB_not_rsched?.Value;

						if (isResearched && Main.mouseLeft && Main.mouseLeftRelease)
						{
							SoundEngine.PlaySound(SoundID.MenuOpen);
							popupSlot = slot;
							isPopupOpening = true;
							isPopupOpen = false;
						}
					}

					string key = slot.Entry.ID;
					float scaleVal = nodeScales.TryGetValue(key, out float sc) ? sc : 1f;
					float rotVal = nodeRotations.TryGetValue(key, out float rt) ? rt : 0f;

					Color drawColor = isResearched ? Color.White * Opacity : Color.Black * 0.45f * Opacity;
					float iconScale = (slot.IsSubNode ? 0.7f : 0.85f) * scaleVal;
					spriteBatch.Draw(tex, pos, null, drawColor, rotVal, tex.Size() / 2f, iconScale, SpriteEffects.None, 0f);
				}
			}

			string title = RitualBookLanguageSystem.RB_researched_Nodes?.Value;
			Vector2 tSize = FontAssets.DeathText.Value.MeasureString(title) * 0.65f;
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, title, new Vector2(dims.X + dims.Width / 2f - tSize.X / 2f, dims.Y + 48f), new Color(220, 200, 140) * Opacity, 0f, Vector2.Zero, new Vector2(0.65f));

			if (popupAnim > 0f && popupSlot != null)
			{
				DrawPopup(spriteBatch, dims);
			}
		}

		private void DrawPopup(SpriteBatch spriteBatch, CalculatedStyle dims)
		{
			if (popupSlot == null || popupSlot.Entry == null) return;
			
			Vector2 center = new Vector2(dims.X + dims.Width / 2f, dims.Y + dims.Height / 2f);
			float scale = popupAnim;

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, dims.ToRectangle(), Color.Black * 0.45f * popupAnim);

			Rectangle popupRect = new Rectangle((int)(center.X - 190 * scale), (int)(center.Y - 110 * scale), (int)(380 * scale), (int)(220 * scale));
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, popupRect, new Color(30, 25, 40) * 0.96f * popupAnim);

			spriteBatch.End();

			Rectangle oldScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
			Rectangle scissorRect = new Rectangle((int)(popupRect.X * Main.UIScale), (int)(popupRect.Y * Main.UIScale), (int)(popupRect.Width * Main.UIScale), (int)(popupRect.Height * Main.UIScale));

			scissorRect = Rectangle.Intersect(scissorRect, spriteBatch.GraphicsDevice.ScissorRectangle);
			RasterizerState scissorState = new RasterizerState { ScissorTestEnable = true };

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, scissorState, null, Main.UIScaleMatrix);
			spriteBatch.GraphicsDevice.ScissorRectangle = scissorRect;

			if (ringSpinningTex != null)
			{
				float vortexRot = Main.GlobalTimeWrappedHourly * 2.5f;
				Vector2 vortexCenter = new Vector2(popupRect.Center.X, popupRect.Center.Y);

				Color vortexColor = EssencePalette.ColorOf(popupSlot.Entry.Tier) * 0.45f * popupAnim;
				float texWidth = ringSpinningTex.Value.Width;
				float scaleBg = popupRect.Width * 1.5f / texWidth * popupAnim;

				spriteBatch.Draw(ringSpinningTex.Value, vortexCenter, null, vortexColor, vortexRot, ringSpinningTex.Value.Size() / 2f, scaleBg, SpriteEffects.None, 0f);
			}

			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = oldScissor;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

			Color borderCol = EssencePalette.ColorOf(popupSlot.Entry.Tier) * popupAnim;

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(popupRect.X, popupRect.Y, popupRect.Width, 2), borderCol);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(popupRect.X, popupRect.Bottom - 2, popupRect.Width, 2), borderCol);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(popupRect.X, popupRect.Y, 2, popupRect.Height), borderCol);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(popupRect.Right - 2, popupRect.Y, 2, popupRect.Height), borderCol);

			if (popupAnim >= 0.95f)
			{
				if (Main.mouseLeft && Main.mouseLeftRelease && !popupRect.Contains(Main.MouseScreen.ToPoint()))
				{
					ClosePopup();
				}

				string titleText = popupSlot.Entry.DisplayName;
				Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(titleText);
				Vector2 titlePos = new Vector2(center.X - titleSize.X / 2f, popupRect.Y + 18);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
				if (circularGlowTex != null)
				{
					spriteBatch.Draw(circularGlowTex.Value, titlePos + titleSize / 2f, null, EssencePalette.ColorOf(popupSlot.Entry.Tier) * popupAnim * 0.6f, 0f, circularGlowTex.Value.Size() / 2f, 0.4f * popupAnim, SpriteEffects.None, 0f);
				}
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, titleText, titlePos, Color.Gold, 0f, Vector2.Zero, Vector2.One);

				string nodeTypeTag = popupSlot.IsSubNode ? "Sub Node" : "Main Node";

				string categoryText = $"[ {nodeTypeTag} — {popupSlot.Entry.Tier} ]";
				Vector2 catSize = FontAssets.MouseText.Value.MeasureString(categoryText) * 0.8f;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, categoryText, new Vector2(center.X - catSize.X / 2f, popupRect.Y + 45), jarkos.core.Data.EssencePalette.ColorOf(popupSlot.Entry.Tier), 0f, Vector2.Zero, new Vector2(0.8f));

				string descText = popupSlot.Entry.Description;
				Vector2 descPos = new Vector2(popupRect.X + 20, popupRect.Y + 75);
				float maxDescWidth = popupRect.Width - 40;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, descText, descPos, Color.White, 0f, Vector2.Zero, new Vector2(0.9f), maxDescWidth);

				string cText = LangSystem.ClickToClose.Value;
				Vector2 cSize = FontAssets.MouseText.Value.MeasureString(cText) * 0.75f;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, cText, new Vector2(center.X - cSize.X / 2f, popupRect.Bottom - 24), Color.Gray, 0f, Vector2.Zero, new Vector2(0.75f));
			}
		}

		private void ClosePopup()
		{
			if (isPopupOpen || isPopupOpening)
			{
				isPopupOpen = false;
				isPopupOpening = false;
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
		}
	}

	public class RitualRecipeGridPage : UIElement
	{
		private List<PetrifiedAltarRecipe> _recipes;
		private int _startIndex;
		private int _count;
		private RitualBookUIState _uiState;
		public float Opacity = 1f;

		private int _hoveredIndex = -1;
		private float _tooltipAlpha = 0f;

		public RitualRecipeGridPage(List<PetrifiedAltarRecipe> recipes, int startIndex, int count, RitualBookUIState uiState)
		{
			_recipes = recipes;
			_startIndex = startIndex;
			_count = count;
			_uiState = uiState;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (_hoveredIndex != -1)
			{
				_tooltipAlpha = MathHelper.Lerp(_tooltipAlpha, 1f, 0.2f);
			}
			else
			{
				_tooltipAlpha = MathHelper.Lerp(_tooltipAlpha, 0f, 0.2f);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (_recipes == null || _count <= 0) return;

			CalculatedStyle dims = GetDimensions();
			_hoveredIndex = -1;

			int cols = 4;
			int rows = 3;
			float cellWidth = dims.Width / cols;
			float cellHeight = (dims.Height - 40f) / rows;

			Vector2 startPos = new Vector2(dims.X, dims.Y + 20f);

			for (int i = 0; i < _count; i++)
			{
				int recipeIdx = _startIndex + i;
				if (recipeIdx >= _recipes.Count) break;

				var recipe = _recipes[recipeIdx];
				int row = i / cols;
				int col = i % cols;

				Vector2 cellCenter = startPos + new Vector2(col * cellWidth + cellWidth / 2f, row * cellHeight + cellHeight / 2f);
				Rectangle cellRect = new Rectangle((int)(cellCenter.X - cellWidth / 2f), (int)(cellCenter.Y - cellHeight / 2f), (int)cellWidth, (int)cellHeight);

				bool isResearched = true;
				string reqNode = PetrifiedAltarTileEntity.GetRequiredNode(recipe.ResultItem);
				if (reqNode != null)
				{
					isResearched = false;
					try
					{
						var plr = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
						if (plr != null) isResearched = plr.IsResearched(reqNode);
					}
					catch { }
				}

				bool isHovering = cellRect.Contains(Main.MouseScreen.ToPoint());
				if (isHovering)
				{
					_hoveredIndex = i;
					if (Main.mouseLeft && Main.mouseLeftRelease)
					{
						_uiState.OpenRecipePopup(recipe, recipeIdx);
					}
				}

				Color boxColor = isHovering ? new Color(100, 120, 200, 180) : new Color(50, 60, 110, 140);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, cellRect, boxColor * Opacity);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, 1), Color.White * 0.3f * Opacity);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(cellRect.X, cellRect.Bottom - 1, cellRect.Width, 1), Color.White * 0.3f * Opacity);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(cellRect.X, cellRect.Y, 1, cellRect.Height), Color.White * 0.3f * Opacity);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(cellRect.Right - 1, cellRect.Y, 1, cellRect.Height), Color.White * 0.3f * Opacity);

				try
				{
					Main.instance.LoadItem(recipe.ResultItem);
					Texture2D itemTex = TextureAssets.Item[recipe.ResultItem].Value;
					Rectangle? frame = null;

					if (Main.itemAnimations[recipe.ResultItem] != null)
					{
						frame = Main.itemAnimations[recipe.ResultItem].GetFrame(itemTex);
					}

					Vector2 itemSize = frame?.Size() ?? itemTex.Size();
					float itemScale = Math.Min(32f / itemSize.X, 32f / itemSize.Y);

					if (!isResearched)
					{
						spriteBatch.Draw(itemTex, cellCenter, frame, Color.Black * Opacity, 0f, itemSize / 2f, itemScale, SpriteEffects.None, 0f);

						string qText = "?";
						Vector2 qSize = FontAssets.MouseText.Value.MeasureString(qText);
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, qText, cellCenter - qSize / 2f, Color.Gold * Opacity, 0f, Vector2.Zero, Vector2.One);
					}
					else
					{
						spriteBatch.Draw(itemTex, cellCenter, frame, Color.White * Opacity, 0f, itemSize / 2f, itemScale, SpriteEffects.None, 0f);
					}
				}
				catch { }
			}
		}

		public void DrawTooltip(SpriteBatch spriteBatch)
		{
			if (_hoveredIndex == -1 || _tooltipAlpha <= 0.05f) return;
			int recipeIdx = _startIndex + _hoveredIndex;
			if (recipeIdx >= _recipes.Count) return;

			var recipe = _recipes[recipeIdx];
			string itemName = Lang.GetItemNameValue(recipe.ResultItem);
			string description = "";
			try
			{
				var tooltip = Lang.GetTooltip(recipe.ResultItem);
				if (tooltip != null && tooltip.Lines > 0)
				{
					description = tooltip.GetLine(0);
				}
			}
			catch { }

			Vector2 mousePos = Main.MouseScreen + new Vector2(16, 16);
			Vector2 nameSize = FontAssets.MouseText.Value.MeasureString(itemName);
			Vector2 descSize = string.IsNullOrWhiteSpace(description) ? Vector2.Zero : FontAssets.MouseText.Value.MeasureString(description) * 0.85f;

			float tooltipWidth = Math.Max(nameSize.X, descSize.X) + 20f;
			float tooltipHeight = nameSize.Y + (descSize.Y > 0 ? descSize.Y + 8f : 0f) + 16f;

			Rectangle bgRect = new Rectangle((int)mousePos.X, (int)mousePos.Y, (int)tooltipWidth, (int)tooltipHeight);

			float a = _tooltipAlpha * Opacity;

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, bgRect, new Color(20, 25, 50, 230) * a);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(bgRect.X, bgRect.Y, bgRect.Width, 2), Color.Gold * a);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(bgRect.X, bgRect.Bottom - 2, bgRect.Width, 2), Color.Gold * a);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(bgRect.X, bgRect.Y, 2, bgRect.Height), Color.Gold * a);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(bgRect.Right - 2, bgRect.Y, 2, bgRect.Height), Color.Gold * a);

			Vector2 textPos = new Vector2(bgRect.X + 10, bgRect.Y + 8);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, itemName, textPos, Color.White * a, 0f, Vector2.Zero, Vector2.One);

			if (!string.IsNullOrWhiteSpace(description))
			{
				Vector2 descPos = textPos + new Vector2(0, nameSize.Y + 4f);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, description, descPos, Color.LightGray * a, 0f, Vector2.Zero, new Vector2(0.85f));
			}
		}
	}

	public class RitualBookLanguageSystem : ModSystem
	{
		public static LocalizedText RBstill_not_rsched { get; private set; }
		public static LocalizedText RB_not_rsched { get; private set; }
        public static LocalizedText RB_researched_Nodes { get; private set; }

        public override void Load()
        {
            string category = "RitualBookUI";

			RBstill_not_rsched ??= Mod.GetLocalization($"{category}.RBstill_not_rsched");
			RB_not_rsched ??= Mod.GetLocalization($"{category}.RB_not_rsched");
			RB_researched_Nodes ??= Mod.GetLocalization($"{category}.RB_researched_Nodes");
        }
	}
}
