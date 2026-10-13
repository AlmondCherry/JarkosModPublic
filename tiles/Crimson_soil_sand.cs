using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using jarkos.systems;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using Terraria.Audio;
using System.Runtime.CompilerServices;
namespace jarkos.tiles
{
    public class Crimson_soil_sand : ModTile
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
            AddMapEntry(new Color(255, 51, 51));
			HitSound = SoundID.Tink;
            DustType = DustID.RedTorch;
            MineResist = 2f;
            MinPick = 95;
        }
    }
}
