using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;

namespace jarkos.UI
{
	public class RitualBookUISystem : ModSystem
	{
		public static RitualBookUISystem Instance => ModContent.GetInstance<RitualBookUISystem>();

		private UserInterface _bookUserInterface;
		internal RitualBookUIState BookUI;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				BookUI = new RitualBookUIState();
				BookUI.Activate();
				_bookUserInterface = new UserInterface();
			}
		}

		public override void Unload()
		{
			BookUI = null;
			_bookUserInterface = null;
		}

		public void ShowBook()
		{
			if (BookUI == null)
			{
				BookUI = new RitualBookUIState();
				BookUI.Activate();
			}
			SoundEngine.PlaySound(SoundID.MenuOpen);
			BookUI.Open();
			_bookUserInterface?.SetState(BookUI);
		}

		public void HideBook()
		{
			if (BookUI != null && IsBookOpen())
			{
				if (!BookUI.IsClosing)
				{
					SoundEngine.PlaySound(SoundID.MenuClose);
					BookUI.Close();
				}
			}
		}

		public void ForceHideBook()
		{
			_bookUserInterface?.SetState(null);
		}
		public bool IsBookOpen()
		{
			return _bookUserInterface?.CurrentState == BookUI;
		}
		public override void UpdateUI(GameTime gameTime)
		{
			if (_bookUserInterface?.CurrentState != null)
			{
				_bookUserInterface.Update(gameTime);
			}
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("jarkos: Ritual Book UI", delegate
					{
						if (_bookUserInterface?.CurrentState != null)
						{
							_bookUserInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					}, InterfaceScaleType.UI));
			}
		}
	}
}
