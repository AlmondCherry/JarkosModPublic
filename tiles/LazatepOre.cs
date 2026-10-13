using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using jarkos.systems;
namespace jarkos.tiles
{
    public class LazatepOre : ModTile
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
            AddMapEntry(new Color(153, 204, 255));
			HitSound = SoundID.Tink;
            DustType = DustID.Clentaminator_Cyan;
            MineResist = 2f;
            MinPick = 30;
        }
		public override bool CanExplode(int i, int j)
        {
            return false;
        }
    }
    public class LazatepOreSystem : ModSystem
	{
		public static LocalizedText ExampleOrePassMessage { get; private set; }
		public override void SetStaticDefaults() {
			ExampleOrePassMessage = Mod.GetLocalization($"WorldGen.{nameof(ExampleOrePassMessage)}");
		}
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
			int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
			if (ShiniesIndex != -1) {
				tasks.Insert(ShiniesIndex + 1, new LazatepOrePass("lazatep Ore", 237.4298f));
			}
		}
	}
	public class LazatepOrePass : GenPass
	{
		public LazatepOrePass(string name, float loadWeight) : base(name, loadWeight) {
		}
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			progress.Message = LazatepOreSystem.ExampleOrePassMessage.Value;
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++) {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next(Main.maxTilesY - 500, Main.maxTilesY - 50);
                Tile tile = Framing.GetTileSafely(x, y);
			    if (tile.HasTile && tile.TileType == TileID.Stone && !DownedBossSystem.downedTenderod) {
                   for(int j = 0; j < 30; j++) {
                	WorldGen.TileRunner(x, y, WorldGen.genRand.Next(8, 12), WorldGen.genRand.Next(1, 2), ModContent.TileType<LazatepOre>());
                   }
                }
            }
		}
	}
}
