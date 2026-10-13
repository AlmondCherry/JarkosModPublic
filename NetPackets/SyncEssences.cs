using Terraria;
using Terraria.ID;
using System.Linq;
using Terraria.ModLoader;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;
using Jarkos;

namespace jarkos.NetPackets
{
	public static class SyncEssences
	{
		public static void SyncEssencess(this ResearchPlayer modPlayer)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            var valid = modPlayer.Essences.Where(kv => System.Enum.IsDefined(typeof(EssenceType), kv.Key) && kv.Value > 0).ToList();
            ModPacket packet = modPlayer.Mod.GetPacket();
            packet.Write((byte)JarkosPacketType.SyncPlayerEssences);
            packet.Write((byte)modPlayer.Player.whoAmI);
            packet.Write((byte)valid.Count);

            foreach (var kvp in valid)
            {
                packet.Write((byte)kvp.Key);
                packet.Write(kvp.Value);
            }
            if (Main.netMode == NetmodeID.Server)
            {
                packet.Send(-1, modPlayer.Player.whoAmI);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient && modPlayer.Player.whoAmI == Main.myPlayer)
            {
                packet.Send();
            }
        }

		public static void CompleteResearchSync(this ResearchPlayer modPlayer, string id)
        {
            ModPacket packet = modPlayer.Mod.GetPacket();
            packet.Write((byte)JarkosPacketType.CompleteResearchGlobal);
            packet.Write(id);
            packet.Send();
        }

		public static void SetPuzzleProgressSync(this ResearchPlayer modPlayer, int nodeId, int type)
        {
            ModPacket packet = modPlayer.Mod.GetPacket();
            packet.Write((byte)JarkosPacketType.UpdatePuzzleGlobal);
            packet.Write(nodeId);
            packet.Write(type);
            packet.Send();
        }

		public static void StartResearchSync(this ResearchPlayer modPlayer, string id)
        {
            ModPacket packet = modPlayer.Mod.GetPacket();
            packet.Write((byte)JarkosPacketType.StartResearchGlobal);
            packet.Write(id);
            packet.Send();
        }
	}
}