using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.UI;

namespace jarkos.tiles
{
    public class UVlampTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<UVlampTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(160, 80, 220), Terraria.Localization.Language.GetText("UV Lamp"));
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.55f;
            g = 0.15f;
            b = 0.85f;
        }

        public override bool RightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int left = i - (tile.TileFrameX / 18);
            int top = j - (tile.TileFrameY / 18);

            if (!TileEntity.ByPosition.ContainsKey(new Point16(left, top)))
            {
                ModContent.GetInstance<UVlampTileEntity>().Place(left, top);
            }

            UVlampUISystem.OpenUI(left, top);
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<items.placeable.UVlampItem>();
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {

                Lighting.AddLight(new Vector2(i * 16 + 24, j * 16 + 24), new Vector3(0.55f, 0.15f, 0.85f));


                if (!Main.gamePaused && Main.instance.IsActive && Main.rand.NextBool(5))
                {
                    Vector2 spawnPos = new Vector2(i * 16 + Main.rand.NextFloat(4, 44), j * 16 + Main.rand.NextFloat(4, 44));
                    Vector2 speed = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, 0.5f));
                    Dust dust = Dust.NewDustPerfect(spawnPos, DustID.PurpleCrystalShard, speed, 100, default, Main.rand.NextFloat(0.5f, 0.8f));
                    dust.noGravity = true;
                }
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<UVlampTileEntity>().Kill(i, j);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail || effectOnly) return;
            ModContent.GetInstance<UVlampTileEntity>().Kill(i, j);
        }
    }
}
