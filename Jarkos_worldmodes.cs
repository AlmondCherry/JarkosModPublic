using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
namespace jarkos
{
	public class Jarkos_worldmodes : ModSystem
	{
        public static bool brokenpotion_buff = false;
        public static bool furiousfire_buff = false;
        public static bool fragility_buff = false;
        public static bool monitor_world = false;
        public static bool glass_world = false;
        public static bool cursedcoin_world = false;
        public static bool brokenwing_world = false;
        public override void SaveWorldData(TagCompound tag)
        {
            List<string> stringList = new List<string>();
            if (furiousfire_buff)
            stringList.Add("furiousfire_buff");
            if (brokenpotion_buff)
            stringList.Add("brokenpotion_buff");
            if (brokenpotion_buff)
            stringList.Add("fragility_buff");
            tag = new TagCompound()
            {
            {
            "downed", (object) stringList
            }
            };
        }
        public override void LoadWorldData(TagCompound tag)
        {
            IList<string> list = tag.GetList<string>("downed");
            furiousfire_buff = list.Contains("furiousfire_buff");
            brokenpotion_buff = list.Contains("brokenpotion_buff");
            fragility_buff = list.Contains("fragility_buff");
        }
    }
}
