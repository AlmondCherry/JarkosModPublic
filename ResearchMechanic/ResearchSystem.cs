using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.core.PlayerInteract
{
    public class ResearchSystem : ModSystem
    {
        public static HashSet<string> GlobalCompleted = new();
        public static string GlobalActiveResearchID = null;
        public static Dictionary<int, int> GlobalActivePuzzleProgress = new();

        public override void OnWorldLoad()
        {
            GlobalCompleted.Clear();
            GlobalActiveResearchID = null;
            GlobalActivePuzzleProgress.Clear();
        }

        public override void OnWorldUnload()
        {
            GlobalCompleted.Clear();
            GlobalActiveResearchID = null;
            GlobalActivePuzzleProgress.Clear();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["completed"] = GlobalCompleted.ToList();
            if (GlobalActiveResearchID != null)
                tag["active"] = GlobalActiveResearchID;
            
            if (GlobalActivePuzzleProgress.Count > 0)
            {
                tag["puzzleKeys"] = GlobalActivePuzzleProgress.Keys.ToList();
                tag["puzzleVals"] = GlobalActivePuzzleProgress.Values.ToList();
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            GlobalCompleted.Clear();
            if (tag.ContainsKey("completed"))
            {
                foreach (var s in tag.GetList<string>("completed"))
                    GlobalCompleted.Add(s);
            }

            GlobalActiveResearchID = tag.ContainsKey("active") ? tag.GetString("active") : null;

            GlobalActivePuzzleProgress.Clear();
            if (tag.ContainsKey("puzzleKeys"))
            {
                var pKeys = tag.GetList<int>("puzzleKeys");
                var pVals = tag.GetList<int>("puzzleVals");
                for (int i = 0; i < pKeys.Count && i < pVals.Count; i++)
                {
                    GlobalActivePuzzleProgress[pKeys[i]] = pVals[i];
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(GlobalCompleted.Count);
            foreach (var s in GlobalCompleted)
                writer.Write(s);

            writer.Write(GlobalActiveResearchID != null);
            if (GlobalActiveResearchID != null)
                writer.Write(GlobalActiveResearchID);

            writer.Write(GlobalActivePuzzleProgress.Count);
            foreach (var kvp in GlobalActivePuzzleProgress)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            GlobalCompleted.Clear();
            int compCount = reader.ReadInt32();
            for (int i = 0; i < compCount; i++)
                GlobalCompleted.Add(reader.ReadString());

            bool hasActive = reader.ReadBoolean();
            if (hasActive)
                GlobalActiveResearchID = reader.ReadString();
            else
                GlobalActiveResearchID = null;

            GlobalActivePuzzleProgress.Clear();
            int puzzCount = reader.ReadInt32();
            for (int i = 0; i < puzzCount; i++)
            {
                int k = reader.ReadInt32();
                int v = reader.ReadInt32();
                GlobalActivePuzzleProgress[k] = v;
            }
        }
    }
}
