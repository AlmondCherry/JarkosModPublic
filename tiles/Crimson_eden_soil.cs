using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace jarkos.tiles
{
    public class Crimson_eden_soil : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileBlockLight[Type] = false;
			LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(255, 0, 0));
			HitSound = SoundID.Tink;
            DustType = DustID.RedTorch;
            MineResist = 2f;
            MinPick = 95;
        }
    }
    public class Crimson_eden_soil_System : ModSystem
	{
	}
	 }
