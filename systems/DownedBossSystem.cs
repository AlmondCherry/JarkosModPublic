using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
namespace jarkos.systems
{
	public class DownedBossSystem : ModSystem
	{
		#region Variables
		public static bool downedTenderod = false;
		public static bool downedGoD = false;
		public static bool downedPunisher = false;
		public static bool downedCelestia = false;
		public static bool killedPunisher = false;
		public static bool generatedEtherionite = false;
		#endregion
		#region Reset vars
		public override void ClearWorld()
		{
			downedTenderod = false;
			downedGoD = false;
			downedPunisher = false;
			downedCelestia = false;
			killedPunisher = false;
			generatedEtherionite = false;
		}
		#endregion
		#region SaveWorldData
		public override void SaveWorldData(TagCompound tag)
		{
			if (downedTenderod)
			{
				tag["downedTenderod"] = true;
			}
			if (downedGoD)
			{
				tag["downedGoD"] = true;
			}
			if (downedPunisher)
			{
				tag["downedPunisher"] = true;
			}
			if (downedCelestia)
			{
				tag["downedCelestia"] = true;
			}
			if (killedPunisher)
			{
				tag["killedPunisher"] = true;
			}
			if (generatedEtherionite)
			{
				tag["generatedEtherionite"] = true;
			}
		}
		#endregion
		#region LoadWorldData
		public override void LoadWorldData(TagCompound tag)
		{
			downedTenderod = tag.ContainsKey("downedTenderod");
			downedGoD = tag.ContainsKey("downedGoD");
			downedPunisher = tag.ContainsKey("downedPunisher");
			downedCelestia = tag.ContainsKey("downedCelestia");
			killedPunisher = tag.ContainsKey("killedPunisher");
			generatedEtherionite = tag.ContainsKey("generatedEtherionite");
		}
		#endregion
		#region LoadWorldData
		public override void NetSend(BinaryWriter writer)
		{
			var flags = new BitsByte();
			flags[0] = downedTenderod;
			flags[1] = downedGoD;
			flags[2] = downedPunisher;
			flags[3] = downedCelestia;
			flags[4] = killedPunisher;
			flags[5] = generatedEtherionite;
			writer.Write(flags);
		}
		#endregion
		#region LoadWorldData
		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			downedTenderod = flags[0];
			downedGoD = flags[1];
			downedPunisher = flags[2];
			downedCelestia = flags[3];
			killedPunisher = flags[4];
			generatedEtherionite = flags[5];
		}
		#endregion
	}
}
