using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using jarkos.tiles;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Audio;

namespace jarkos.UI
{
	public class ReplicatorUISystem : ModSystem
	{
		public static ReplicatorUISystem Instance => ModContent.GetInstance<ReplicatorUISystem>();
		private UserInterface _userInterface;
		private ReplicatorUIState _uiState;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				_uiState = new ReplicatorUIState();
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
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Jarkos: Replicator UI", delegate
				{
					if (_userInterface?.CurrentState != null)
					{
						_userInterface.Draw(Main.spriteBatch, new GameTime());
					}
					return true;
				}, InterfaceScaleType.UI));
			}
		}

		public void OpenUI(ReplicatorTileEntity entity)
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
