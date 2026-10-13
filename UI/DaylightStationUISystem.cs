using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using jarkos.tiles;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;

namespace jarkos.UI
{
	public class DaylightStationUISystem : ModSystem
	{
		public static DaylightStationUISystem Instance => ModContent.GetInstance<DaylightStationUISystem>();
		private UserInterface _userInterface;
		private DaylightStationUIState _uiState;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				_uiState = new DaylightStationUIState();
				_uiState.Activate();
				_userInterface = new UserInterface();
			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			if (_userInterface?.CurrentState != null)
			{
				_userInterface.Update(gameTime);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Jarkos: Daylight Station UI", delegate
				{
					if (_userInterface?.CurrentState != null)
					{
						_userInterface.Draw(Main.spriteBatch, new GameTime());
					}
					return true;
				}, InterfaceScaleType.UI));
			}
		}

		public void OpenUI(DaylightStationTileEntity entity)
		{
			_uiState.SetEntity(entity);
			_userInterface.SetState(_uiState);
			SoundEngine.PlaySound(SoundID.MenuOpen);
		}

		public void ForceClose()
		{
			if (_userInterface?.CurrentState != null)
			{
				_userInterface.SetState(null);
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
		}
	}
}
