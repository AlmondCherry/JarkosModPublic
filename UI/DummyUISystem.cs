using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace jarkos.UI
{
	public class DummyUISystem : ModSystem
	{
		public static DummyUISystem Instance => ModContent.GetInstance<DummyUISystem>();

		public static bool IsArmored = false;
		public static int ArmoredDefense = 1000;

		public static float DummyScale = 1.0f;
		public static float DummyDR = 0.0f;
		public static float DummyHealthPercent = 100.0f;
		public static int DummyDamage = 0;
		public static bool DummyDebuffVulnerable = true;
		public static float DummyKnockbackResist = 100.0f;
		public static bool DummyNoGravity = false;

		public static void ResetDetailedSettings()
		{
			DummyScale = 1.0f;
			DummyDR = 0.0f;
			DummyHealthPercent = 100.0f;
			DummyDamage = 0;
			DummyDebuffVulnerable = true;
			DummyKnockbackResist = 100.0f;
			DummyNoGravity = false;
		}

		private UserInterface _userInterface;
		private DummyUIState _uiState;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				_uiState = new DummyUIState();
				_uiState.Activate();
				_userInterface = new UserInterface();
			}
		}

		public static bool AnyBossActive()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && (npc.boss || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail))
				{
					return true;
				}
			}
			return false;
		}

		public override void PostUpdateNPCs()
		{
			if (AnyBossActive())
			{
				DummyUIState.RemoveAllDummies();
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
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Jarkos: Dummy Selector UI", delegate
				{
					if (_userInterface?.CurrentState != null)
					{
						_userInterface.Draw(Main.spriteBatch, new GameTime());
					}
					return true;
				}, InterfaceScaleType.UI));
			}
		}

		public void ToggleUI()
		{
			if (_userInterface?.CurrentState != null && !_uiState.IsClosing)
			{
				ForceClose();
			}
			else
			{
				OpenUI();
			}
		}

		public void OpenUI()
		{
			if (_userInterface?.CurrentState == null)
			{
				_uiState.ResetAnimation();
				_userInterface?.SetState(_uiState);
				SoundEngine.PlaySound(SoundID.MenuOpen);
			}
			else
			{
				_uiState.StartOpening();
			}
		}

		public void ForceClose()
		{
			if (_userInterface?.CurrentState != null && !_uiState.IsClosing)
			{
				_uiState.StartClosing();
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
		}

		public void CloseStateNow()
		{
			_userInterface?.SetState(null);
		}
	}
}

