using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.GameplayChanges
{
	public class TeamRejoinPlayer : ModPlayer
	{
		public int lastTeam = 0;

		public override void SaveData(TagCompound tag)
		{
			if (Player.team != 0)
			{
				lastTeam = Player.team;
			}
			tag["lastTeam"] = lastTeam;
		}

		public override void LoadData(TagCompound tag)
		{
			if (tag.ContainsKey("lastTeam"))
			{
				lastTeam = tag.GetInt("lastTeam");
			}
		}

		public override void PostUpdate()
		{
			if (Player.team != 0)
			{
				lastTeam = Player.team;
			}
		}

		public override void OnEnterWorld()
		{
			if (Jarkos_config.Instance != null && !Jarkos_config.Instance.AutoRejoinTeam)
				return;

			if (lastTeam > 0 && lastTeam < 6)
			{
				Player.team = lastTeam;
				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					NetMessage.SendData(MessageID.PlayerTeam, -1, -1, null, Player.whoAmI);
				}
			}
		}
	}
}
