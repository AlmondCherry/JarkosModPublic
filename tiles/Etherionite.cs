using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
namespace jarkos.tiles
{
    public class Etherionite : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 975;
            Main.tileBlockLight[Type] = false;
			LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(192, 81, 255));
			HitSound = SoundID.Tink;
            DustType = DustID.BlueFairy;
            MineResist = 2f;
            MinPick = 95;
        }
		public override bool CanExplode(int i, int j)
        {
            return false;
        }
    }
    public class EtherioniteSystem : ModSystem
	{
		public static LocalizedText ExampleOrePassMessage { get; private set; }
		public override void SetStaticDefaults() {
			ExampleOrePassMessage = Mod.GetLocalization($"WorldGen.{nameof(ExampleOrePassMessage)}");
		}
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
			int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
			if (ShiniesIndex != -1) {
				tasks.Insert(ShiniesIndex + 1, new EtherionitePass("Etherionite", 237.4298f));
			}
		}
	}
	public class EtherionitePass : GenPass
	{
		public EtherionitePass(string name, float loadWeight) : base(name, loadWeight) {
		}
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			progress.Message = EtherioniteSystem.ExampleOrePassMessage.Value;
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++) {
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);
				int y = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, Main.maxTilesY);
			}
		}
	}
}
