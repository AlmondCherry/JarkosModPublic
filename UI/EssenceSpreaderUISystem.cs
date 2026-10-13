using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using jarkos.tiles;
using Jarkos.tiles;
using jarkos.core.Data;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.UI.Chat;

namespace jarkos.UI
{
    public class EssenceSpreaderUISystem : ModSystem
    {
        public static int OpenUIX = -1;
        public static int OpenUIY = -1;
        public static float AnimationTimer = 0f;
        public static bool IsFilterMenuOpen = false;
        public static int AimingSpreaderX = -1;
        public static int AimingSpreaderY = -1;

        public static void OpenUI(int i, int j)
        {
            if (OpenUIX == i && OpenUIY == j)
            {
                return;
            }
            OpenUIX = i;
            OpenUIY = j;
            AnimationTimer = 0f;
            IsFilterMenuOpen = false;
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
            IsFilterMenuOpen = false;
        }
        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server) return;

            if (AimingSpreaderX != -1 && AimingSpreaderY != -1)
            {
                if (TileEntity.ByPosition.TryGetValue(new Point16(AimingSpreaderX, AimingSpreaderY), out TileEntity te) && te is EssenceSpreaderTileEntity spreader)
                {
                    Vector2 pos = new Vector2(spreader.Position.X * 16 + 8, spreader.Position.Y * 16 + 8);
                    spreader.TargetRotation = (Main.MouseWorld - pos).ToRotation();
                    spreader.AimingPlayer = Main.myPlayer;

                    bool justClicked = (Main.mouseLeft && Main.mouseLeftRelease) || (Main.mouseRight && Main.mouseRightRelease);
                    if (justClicked)
                    {
                        AimingSpreaderX = -1;
                        AimingSpreaderY = -1;
                        spreader.AimingPlayer = -1;
                        spreader.SyncData();
                        Main.NewText("Essence Spreader locked direction.", Color.Cyan);
                    }
                }
                else
                {
                    AimingSpreaderX = -1;
                    AimingSpreaderY = -1;
                }
            }
        }

        public override void PostDrawTiles()
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (!TextureAssets.MagicPixel.IsLoaded) return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float maxDist = EssenceSpreaderTileEntity.SuckRadius;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var kvp in TileEntity.ByID)
            {
                if (kvp.Value is EssenceSpreaderTileEntity spreader)
                {

                    if (spreader.BeamMode == 1)
                    {
                        Vector2 spreaderCenter = new Vector2(spreader.Position.X * 16 + 8, spreader.Position.Y * 16 + 8);

                        foreach (var innerKvp in TileEntity.ByID)
                        {
                            Vector2 targetCenter = Vector2.Zero;
                            bool isSourceBlock = false;

                            if (innerKvp.Value is EssenceFlaskTileEntity flask)
                            {
                                targetCenter = new Vector2(flask.Position.X * 16 + 8, flask.Position.Y * 16 + 8);
                                isSourceBlock = true;
                            }
                            else if (innerKvp.Value is LargeEssenceFlaskTileEntity largeFlask)
                            {
                                targetCenter = new Vector2(largeFlask.Position.X * 16 + 24, largeFlask.Position.Y * 16 + 24);
                                isSourceBlock = true;
                            }
                            else if (innerKvp.Value is AlchemicalEnchantmentTE ate)
                            {
                                targetCenter = new Vector2(ate.Position.X * 16 + 32, ate.Position.Y * 16 + 16);
                                isSourceBlock = true;
                            }
                            else if (innerKvp.Value is DistillerTileEntity distiller)
                            {
                                targetCenter = new Vector2(distiller.Position.X * 16 + 16, distiller.Position.Y * 16 + 16);
                                isSourceBlock = true;
                            }
                            else if (innerKvp.Value is DaylightGeneratorTileEntity dgen)
                            {
                                targetCenter = new Vector2(dgen.Position.X * 16 + 16, dgen.Position.Y * 16 + 16);
                                isSourceBlock = true;
                            }
                            else if (innerKvp.Value is DaylightStationTileEntity dstation)
                            {
                                targetCenter = new Vector2(dstation.Position.X * 16 + 16, dstation.Position.Y * 16 + 16);
                                isSourceBlock = true;
                            }

                            if (isSourceBlock)
                            {
                                Vector2 diff = targetCenter - spreaderCenter;
                                float dist = diff.Length();
                                if (dist > 1f && dist <= maxDist)
                                {
                                    float rot = diff.ToRotation();
                                    float thickness = 4f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f) * 1.2f;
                                    Vector2 drawPos = spreaderCenter - Main.screenPosition;

                                    Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.Red * 0.85f, rot, new Vector2(0f, 0.5f), new Vector2(dist, thickness + 5f), SpriteEffects.None, 0f);
                                    Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.Pink * 0.95f, rot, new Vector2(0f, 0.5f), new Vector2(dist, thickness), SpriteEffects.None, 0f);
                                }
                            }
                        }
                    }

                    else if (spreader.BeamMode == 2)
                    {
                        Vector2 spreaderCenter = new Vector2(spreader.Position.X * 16 + 8, spreader.Position.Y * 16 + 8);
                        float suckRadius = EssenceSpreaderTileEntity.SuckRadius;
                        int segments = 48;
                        for (int i = 0; i < segments; i++)
                        {
                            float a1 = MathHelper.TwoPi * i / segments;
                            float a2 = MathHelper.TwoPi * (i + 1) / segments;

                            Vector2 p1 = spreaderCenter + a1.ToRotationVector2() * suckRadius - Main.screenPosition;
                            Vector2 p2 = spreaderCenter + a2.ToRotationVector2() * suckRadius - Main.screenPosition;

                            Vector2 d = p2 - p1;
                            float len = d.Length();
                            float rot = MathF.Atan2(d.Y, d.X);

                            float pulse = 0.75f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + i * 0.1f);
                            Main.spriteBatch.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), Color.Cyan * 0.5f * pulse, rot, new Vector2(0f, 0.5f), new Vector2(len, 4f), SpriteEffects.None, 0f);
                            Main.spriteBatch.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), Color.LightCyan * 0.9f * pulse, rot, new Vector2(0f, 0.5f), new Vector2(len, 1.8f), SpriteEffects.None, 0f);
                        }
                    }
                }
            }

            Main.spriteBatch.End();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (OpenUIX == -1 || OpenUIY == -1) return;
            AnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * 6f;
            if (AnimationTimer > 1f) AnimationTimer = 1f;
            Player player = Main.LocalPlayer;
            if (player.dead || !player.active || Vector2.Distance(player.Center, new Vector2(OpenUIX * 16, OpenUIY * 16)) > 200f)
            {
                CloseUI();
                return;
            }
            if ((Main.mouseLeft && Main.mouseLeftRelease || Main.mouseRight && Main.mouseRightRelease) && !Main.LocalPlayer.mouseInterface)
            {
                Point tileCoords = Main.MouseWorld.ToTileCoordinates();
                if (tileCoords.X == OpenUIX && tileCoords.Y == OpenUIY)
                {
                }
                else if (AnimationTimer > 0.2f)
                {
                    CloseUI();
                }
            }
        }
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (OpenUIX == -1 || OpenUIY == -1) return;
            if (TileEntity.ByPosition.TryGetValue(new Point16(OpenUIX, OpenUIY), out TileEntity te) && te is EssenceSpreaderTileEntity spreader)
            {
                DrawRadialMenu(spriteBatch, spreader);
            }
            else
            {
                CloseUI();
            }
        }
        private void DrawRadialMenu(SpriteBatch spriteBatch, EssenceSpreaderTileEntity spreader)
        {
            Vector2 worldPos = new Vector2(OpenUIX * 16 + 8, OpenUIY * 16 + 8);
            Vector2 blockCenter = Vector2.Transform(worldPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
            Texture2D btnOutline = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle").Value;
            Texture2D btnFill = ModContent.Request<Texture2D>("jarkos/UI/ButtonCircle_Fill").Value;
            Texture2D arrowTex = ModContent.Request<Texture2D>("jarkos/UI/arrow_up").Value;

            float radius = 50f * AnimationTimer;
            float angle1 = -MathHelper.Pi * 0.78f;
            Vector2 pos1 = blockCenter + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
            float angle2 = -MathHelper.Pi * 0.50f;
            Vector2 pos2 = blockCenter + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;
            float angle3 = -MathHelper.Pi * 0.22f;
            Vector2 pos3 = blockCenter + new Vector2((float)Math.Cos(angle3), (float)Math.Sin(angle3)) * radius;

            bool hover1 = Vector2.Distance(Main.MouseScreen, pos1) < btnOutline.Width / 2f;
            bool hover2 = Vector2.Distance(Main.MouseScreen, pos2) < btnOutline.Width / 2f;
            bool hover3 = Vector2.Distance(Main.MouseScreen, pos3) < btnOutline.Width / 2f;

            if (hover1 || hover2 || hover3) Main.LocalPlayer.mouseInterface = true;

            if (Main.mouseLeft && Main.mouseLeftRelease && !IsFilterMenuOpen)
            {
                if (hover1)
                {
                    IsFilterMenuOpen = true;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else if (hover2)
                {
                    AimingSpreaderX = OpenUIX;
                    AimingSpreaderY = OpenUIY;
                    spreader.AimingPlayer = Main.myPlayer;
                    spreader.SyncData();
                    Main.NewText("Aiming Essence Spreader... Click anywhere to lock.", Color.Cyan);
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    CloseUI();
                }
                else if (hover3)
                {
                    spreader.BeamMode = (spreader.BeamMode + 1) % 3;
                    spreader.SyncData();
                    SoundEngine.PlaySound(SoundID.MenuTick);

                    string statusMsg = spreader.BeamMode switch
                    {
                        1 => "Source Beams: Enabled (BEAM)",
                        2 => "Source Beams: Suction Radius Only (RADIUS)",
                        _ => "Source Beams: Disabled (OFF)"
                    };
                    Color msgColor = spreader.BeamMode switch
                    {
                        1 => Color.Red,
                        2 => Color.Cyan,
                        _ => Color.Gray
                    };
                    Main.NewText(statusMsg, msgColor);
                }
            }

            float drawScale1 = hover1 ? 1.1f : 1f;
            float drawScale2 = hover2 ? 1.1f : 1f;
            float drawScale3 = hover3 ? 1.1f : 1f;


            spriteBatch.Draw(btnFill, pos1, null, Color.DarkGray, 0f, btnFill.Size() / 2f, drawScale1, SpriteEffects.None, 0f);
            spriteBatch.Draw(btnOutline, pos1, null, Color.White, 0f, btnOutline.Size() / 2f, drawScale1, SpriteEffects.None, 0f);
            if ((int)spreader.FilterType == -1)
            {
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "All", pos1 - new Vector2(10, 10), Color.White, 0f, Vector2.Zero, Vector2.One);
            }
            else
            {
                Texture2D essenceTex = EssencePalette.GetTexture(spreader.FilterType);
                if (essenceTex != null)
                {
                    float scale = 20f / Math.Max(essenceTex.Width, essenceTex.Height) * drawScale1;
                    spriteBatch.Draw(essenceTex, pos1, null, Color.White, 0f, essenceTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                }
            }
            if (hover1 && !IsFilterMenuOpen) ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "Select Filter", Main.MouseScreen + new Vector2(16, 16), Color.White, 0f, Vector2.Zero, Vector2.One);


            spriteBatch.Draw(btnFill, pos2, null, Color.DarkGray, 0f, btnFill.Size() / 2f, drawScale2, SpriteEffects.None, 0f);
            spriteBatch.Draw(btnOutline, pos2, null, Color.White, 0f, btnOutline.Size() / 2f, drawScale2, SpriteEffects.None, 0f);
            spriteBatch.Draw(arrowTex, pos2, null, Color.White, 0f, arrowTex.Size() / 2f, drawScale2, SpriteEffects.None, 0f);
            if (hover2) ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "Change Direction", Main.MouseScreen + new Vector2(16, 16), Color.White, 0f, Vector2.Zero, Vector2.One);


            Color btnColor3 = spreader.BeamMode switch
            {
                1 => Color.DarkRed * 0.9f,
                2 => Color.DarkCyan * 0.9f,
                _ => Color.DarkGray
            };
            Color outlineColor3 = spreader.BeamMode switch
            {
                1 => Color.Red,
                2 => Color.Cyan,
                _ => Color.White
            };
            spriteBatch.Draw(btnFill, pos3, null, btnColor3, 0f, btnFill.Size() / 2f, drawScale3, SpriteEffects.None, 0f);
            spriteBatch.Draw(btnOutline, pos3, null, outlineColor3, 0f, btnOutline.Size() / 2f, drawScale3, SpriteEffects.None, 0f);

            string beamLabel = spreader.BeamMode switch
            {
                1 => "BEAM",
                2 => "RAD",
                _ => "OFF"
            };
            Color labelColor = spreader.BeamMode switch
            {
                1 => Color.Red,
                2 => Color.Cyan,
                _ => Color.LightGray
            };
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, beamLabel, pos3 - new Vector2(14, 8) * drawScale3, labelColor, 0f, Vector2.Zero, new Vector2(0.7f * drawScale3));

            string hoverTooltip = spreader.BeamMode switch
            {
                1 => "Mode: Red Source Beams (BEAM)",
                2 => "Mode: Suction Radius Only (RADIUS)",
                _ => "Mode: Disabled (OFF)"
            };
            if (hover3) ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, hoverTooltip, Main.MouseScreen + new Vector2(16, 16), Color.White, 0f, Vector2.Zero, Vector2.One);
            if (IsFilterMenuOpen)
            {
                var values = (EssenceType[])Enum.GetValues(typeof(EssenceType));
                int totalItems = values.Length + 1;
                int columns = 4;
                int rows = (int)Math.Ceiling(totalItems / (float)columns);
                int itemSize = 32;
                int padding = 4;
                int panelWidth = columns * (itemSize + padding) + padding;
                int panelHeight = rows * (itemSize + padding) + padding;
                
                Vector2 panelPos = pos1 - new Vector2(panelWidth / 2, panelHeight + 20);
                Rectangle panelRect = new Rectangle((int)panelPos.X, (int)panelPos.Y, panelWidth, panelHeight);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, panelRect, Color.DarkBlue * 0.8f);
                if (panelRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                }
                for (int i = 0; i < totalItems; i++)
                {
                    int col = i % columns;
                    int row = i / columns;
                    Vector2 itemPos = panelPos + new Vector2(padding + col * (itemSize + padding), padding + row * (itemSize + padding));
                    Rectangle itemRect = new Rectangle((int)itemPos.X, (int)itemPos.Y, itemSize, itemSize);
                    bool itemHover = itemRect.Contains(Main.MouseScreen.ToPoint());
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, itemRect, itemHover ? Color.Gray : Color.Black * 0.5f);
                    if (i == 0)
                    {
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "All", itemPos + new Vector2(4, 8), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
                        if (itemHover && Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            spreader.FilterType = (EssenceType)(-1);
                            spreader.SyncData();
                            IsFilterMenuOpen = false;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                    }
                    else
                    {
                        EssenceType type = values[i - 1];
                        Texture2D essenceTex = EssencePalette.GetTexture(type);
                        if (essenceTex != null)
                        {
                            float scale = (float)itemSize / Math.Max(essenceTex.Width, essenceTex.Height) * 0.8f;
                            spriteBatch.Draw(essenceTex, itemPos + new Vector2(itemSize / 2f), null, Color.White, 0f, essenceTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                        }
                        if (itemHover && Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            spreader.FilterType = type;
                            spreader.SyncData();
                            IsFilterMenuOpen = false;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                        if (itemHover)
                        {
                            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, EssencePalette.DisplayName(type), Main.MouseScreen + new Vector2(16, 16), Color.White, 0f, Vector2.Zero, Vector2.One);
                        }
                    }
                }
                if (Main.mouseLeft && Main.mouseLeftRelease && !panelRect.Contains(Main.MouseScreen.ToPoint()) && !hover1)
                {
                    IsFilterMenuOpen = false;
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
            }
        }
    }
}
