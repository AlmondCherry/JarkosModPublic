using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using jarkos.tiles;
using Terraria.Audio;

namespace jarkos.UI
{
    public class UVlampUISystem : ModSystem
    {
        public static int OpenUIX = -1;
        public static int OpenUIY = -1;
        public static float AnimationTimer = 0f;
        public static int IgnoreInputTimer = 0;

        public static void OpenUI(int i, int j)
        {
            OpenUIX = i;
            OpenUIY = j;
            AnimationTimer = 0f;
            IgnoreInputTimer = 12;
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        public static void CloseUI()
        {
            if (OpenUIX != -1)
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
            OpenUIX = -1;
            OpenUIY = -1;
            IgnoreInputTimer = 0;
        }

        public static UVlampTileEntity GetTileEntity(int i, int j)
        {
            if (i < 0 || j < 0) return null;
            Tile tile = Main.tile[i, j];
            if (!tile.HasTile) return null;

            int left = i - (tile.TileFrameX / 18);
            int top = j - (tile.TileFrameY / 18);

            if (TileEntity.ByPosition.TryGetValue(new Point16(left, top), out TileEntity te) && te is UVlampTileEntity lamp)
            {
                return lamp;
            }
            return null;
        }

        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server) return;

            if (OpenUIX != -1 && OpenUIY != -1)
            {
                AnimationTimer = Math.Min(1f, AnimationTimer + 0.15f);
                if (IgnoreInputTimer > 0) IgnoreInputTimer--;

                Player player = Main.LocalPlayer;
                Vector2 lampWorldCenter = new Vector2(OpenUIX * 16 + 24, OpenUIY * 16 + 24);

                if (!player.active || player.dead || Vector2.Distance(player.Center, lampWorldCenter) > 250f || PlayerInput.Triggers.JustPressed.Inventory)
                {
                    CloseUI();
                }
            }
            else
            {
                AnimationTimer = 0f;
                IgnoreInputTimer = 0;
            }
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.netMode == NetmodeID.Server || OpenUIX == -1 || OpenUIY == -1) return;

            UVlampTileEntity lamp = GetTileEntity(OpenUIX, OpenUIY);
            if (lamp != null)
            {
                DrawRadialMenu(spriteBatch, lamp);
            }
            else
            {

                int id = ModContent.GetInstance<UVlampTileEntity>().Place(OpenUIX, OpenUIY);
                if (TileEntity.ByID.TryGetValue(id, out TileEntity newTe) && newTe is UVlampTileEntity newLamp)
                {
                    DrawRadialMenu(spriteBatch, newLamp);
                }
                else
                {
                    CloseUI();
                }
            }
        }

        private void DrawRadialMenu(SpriteBatch spriteBatch, UVlampTileEntity lamp)
        {
            Vector2 worldPos = new Vector2(OpenUIX * 16 + 24, OpenUIY * 16 + 24);
            Vector2 blockCenter = Vector2.Transform(worldPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);

            Texture2D btnOutline = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle").Value;
            Texture2D btnFill = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle_Fill").Value;

            float radius = 55f * AnimationTimer;
            Vector2 btnPos = blockCenter + new Vector2(0f, -radius);

            bool hover = Vector2.Distance(Main.MouseScreen, btnPos) < (btnOutline.Width / 2f);
            if (hover) Main.LocalPlayer.mouseInterface = true;

            if (IgnoreInputTimer <= 0)
            {

                bool clickedOutside = (Main.mouseLeft && Main.mouseLeftRelease) || (Main.mouseRight && Main.mouseRightRelease);
                if (clickedOutside && !hover)
                {
                    CloseUI();
                    return;
                }


                if (Main.mouseLeft && Main.mouseLeftRelease && hover)
                {
                    lamp.ShowRadius = !lamp.ShowRadius;
                    lamp.SyncData();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }

            float drawScale = hover ? 1.15f : 1.0f;
            Color fillColor = lamp.ShowRadius ? new Color(140, 50, 200) * 0.9f : Color.DarkGray;
            Color outlineColor = lamp.ShowRadius ? Color.MediumPurple : Color.White;

            spriteBatch.Draw(btnFill, btnPos, null, fillColor, 0f, btnFill.Size() / 2f, drawScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(btnOutline, btnPos, null, outlineColor, 0f, btnOutline.Size() / 2f, drawScale, SpriteEffects.None, 0f);

            string btnText = lamp.ShowRadius ? "radius range enabled" : "radius range disabled";
            Color textColor = lamp.ShowRadius ? Color.MediumPurple : Color.LightGray;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(btnText) * (0.65f * drawScale);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, btnText, btnPos - textSize / 2f, textColor, 0f, Vector2.Zero, new Vector2(0.65f * drawScale));

            //if (hover)
            //{
            //}
        }

        public override void PostDrawTiles()
        {
            if (Main.netMode == NetmodeID.Server) return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            if (pixel == null) return;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var kvp in TileEntity.ByID)
            {
                if (kvp.Value is UVlampTileEntity lamp && lamp.ShowRadius)
                {
                    Vector2 lampCenter = new Vector2(lamp.Position.X * 16 + 24, lamp.Position.Y * 16 + 24);
                    DrawRadiusCircle(Main.spriteBatch, pixel, lampCenter, 300f, Color.MediumPurple * 0.75f);
                }
            }

            Main.spriteBatch.End();
        }
        private static void DrawRadiusCircle(SpriteBatch sb, Texture2D pixel, Vector2 center, float radius, Color color)
        {
            int segments = 120;
            float step = MathHelper.TwoPi / segments;

            for (int i = 0; i < segments; i++)
            {
                float a1 = i * step;
                float a2 = (i + 1) * step;

                Vector2 p1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius - Main.screenPosition;
                Vector2 p2 = center + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius - Main.screenPosition;

                DrawLine(sb, pixel, p1, p2, color, 3f);
            }
        }
        private static void DrawLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 edge = end - start;
            float angle = MathF.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, angle, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
        }
    }
}
