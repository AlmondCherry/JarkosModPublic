using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Localization;
using jarkos.items.usetiles;
using jarkos.core.Data;
using System.Linq;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.UI.Chat;
using jarkos.UI;
using System;

namespace jarkos.tiles
{
    public class EssenceSpreaderTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<EssenceSpreaderTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
            HitSound = SoundID.Shatter;
            DustType = DustID.Glass;
            AddMapEntry(new Color(100, 255, 200), Language.GetText("Essence Spreader"));
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.25f;
            g = 0.65f;
            b = 0.95f;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Lighting.AddLight(new Vector2(i * 16 + 8, j * 16 + 8), 0.25f, 0.65f, 0.95f);

            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is EssenceSpreaderTileEntity spreader)
            {
                spreader.UpdateVisualHitDistance();
                Tile tile = Main.tile[i, j];
                Texture2D tex = TextureAssets.Tile[Type].Value;
                Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                if (Main.drawToScreen) zero = Vector2.Zero;
                Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;
                spriteBatch.Draw(tex, pos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                Vector2 center = pos + new Vector2(8, 8);
                Color tint = Color.White * 0.3f;
                if (spreader.TotalEssences > 0)
                {
                    foreach(var kvp in spreader.StoredEssences) {
                        if (kvp.Value > 0) {
                            tint = EssencePalette.ColorOf(kvp.Key);
                            break;
                        }
                    }
                }
                else if ((int)spreader.FilterType != -1)
                {
                    tint = EssencePalette.ColorOf(spreader.FilterType) * 0.3f;
                }
                


                Texture2D pipeTex = ModContent.Request<Texture2D>("jarkos/tiles/EssenceSpreaderTile_Pipe", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                if (pipeTex != null)
                {
                    Vector2 dir = spreader.TargetRotation.ToRotationVector2();
                    Vector2 pipePos = center + dir * 12f;
                    Vector2 pipeOrigin = pipeTex.Size() * 0.5f;

                    spriteBatch.Draw(pipeTex, pipePos, null, Color.White, spreader.TargetRotation + MathHelper.PiOver2, pipeOrigin, 1f, SpriteEffects.None, 0f);
                }

                if (TextureAssets.MagicPixel.IsLoaded && spreader.VisualHitDistance > 30f)
                {
                    Texture2D pixel = TextureAssets.MagicPixel.Value;
                    float time = Main.GlobalTimeWrappedHourly;
                    float startDist = 30f;
                    float remainingDist = spreader.VisualHitDistance - startDist;
                    int segments = (int)(remainingDist / 2f);
                    if (segments < 1) segments = 1;
                    float segmentLength = remainingDist / segments;
                    Vector2 dir = spreader.TargetRotation.ToRotationVector2();
                    Vector2 perp = new Vector2(-dir.Y, dir.X);
                    for (int idx = 0; idx < segments; idx++)
                    {
                        float progress = (float)idx / segments;
                        float realDistance = startDist + progress * remainingDist;
                        Vector2 segPos = center + dir * realDistance;
                        float waveOffset = (float)Math.Sin(realDistance * 0.05f - time * 15f) * 3f;
                        segPos += perp * waveOffset;
                        float derivative = 0.05f * (float)Math.Cos(realDistance * 0.05f - time * 15f) * 3f;
                        float segRotation = spreader.TargetRotation + (float)Math.Atan(derivative);
                        float thickness = 2f + (float)Math.Sin(time * 10f) * 0.5f;
                        for (int b = 0; b < spreader.ActiveBumps.Count; b++)
                        {
                            float bumpPos = spreader.ActiveBumps[b].X;
                            float bumpRealPos = bumpPos * spreader.VisualHitDistance;
                            float distToBump = Math.Abs(realDistance - bumpRealPos);
                            float bumpRadius = 20f;
                            if (distToBump < bumpRadius)
                            {
                                float falloff = 1f - (distToBump / bumpRadius);
                                falloff = (float)Math.Sin(falloff * Math.PI / 2f);
                                falloff *= falloff;
                                thickness += 8f * falloff;
                            }
                        }
                        if (thickness < 1f) thickness = 1f;
                        spriteBatch.Draw(pixel, segPos, new Rectangle(0, 0, 1, 1), tint * 0.4f, segRotation, new Vector2(0f, 0.5f), new Vector2(segmentLength + 1f, thickness + 4f), SpriteEffects.None, 0f);
                        spriteBatch.Draw(pixel, segPos, new Rectangle(0, 0, 1, 1), Color.Lerp(tint, Color.White, 0.7f), segRotation, new Vector2(0f, 0.5f), new Vector2(segmentLength + 1f, thickness), SpriteEffects.None, 0f);
                    }
                }
                if (spreader.IsTargetFull)
                {
                    Texture2D warnTex = ModContent.Request<Texture2D>("jarkos/UI/WarningIcon").Value;
                    if (warnTex != null)
                    {
                        Vector2 warnPos = center - new Vector2(warnTex.Width / 2f, 16 + warnTex.Height);
                        warnPos.Y += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 2f;
                        spriteBatch.Draw(warnTex, warnPos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }
                if (new Rectangle(i * 16, j * 16, 16, 16).Contains(Main.MouseWorld.ToPoint()) && spreader.TotalEssences > 0)
                {
                    var available = spreader.StoredEssences.Where(kv => kv.Value > 0).ToList();
                    int totalIcons = available.Count;
                    float iconSpacing = 28f;
                    float startX = center.X - totalIcons * iconSpacing / 2f + iconSpacing / 2f;
                    float drawY = center.Y - 40f;
                    for (int idx = 0; idx < totalIcons; idx++)
                    {
                        var kvp = available[idx];
                        var essenceTex = EssencePalette.GetTexture(kvp.Key);
                        if (essenceTex != null)
                        {
                            Vector2 iconPos = new Vector2(startX + idx * iconSpacing, drawY);
                            spriteBatch.Draw(essenceTex, iconPos, null, Color.White, 0f, essenceTex.Size() / 2f, 1f, SpriteEffects.None, 0f);
                            string numText = kvp.Value.ToString();
                            Vector2 numPos = iconPos + new Vector2(4f, 4f);
                            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, numText, numPos, Color.White, 0f, Vector2.Zero, new Vector2(0.7f));
                        }
                    }
                }
                return false;
            }
            return true;
        }
        public override bool RightClick(int i, int j)
        {
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is EssenceSpreaderTileEntity spreader)
            {
                EssenceSpreaderUISystem.OpenUI(i, j);
            }
            return true;
        }
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<EssenceSpreaderItem>();
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<EssenceSpreaderTileEntity>().Kill(i, j);
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail || effectOnly) return;
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is EssenceSpreaderTileEntity spreader)
            {
                ModContent.GetInstance<EssenceSpreaderTileEntity>().Kill(i, j);
            }
        }
    }
}
