using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Audio;
using Terraria.ID;
using jarkos.npc.CradleMobs;

namespace jarkos.ui
{
    public class UIParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Scale;
        public int TimeLeft;
        public int MaxTime;
        public float Rotation;
        public float RotSpeed;
    }
    public class ScavengerUISystem : ModSystem
    {
        public static ScavengerUISystem Instance => ModContent.GetInstance<ScavengerUISystem>();
        public ScavengerShopUIState ScavengerUI;
        private UserInterface scavengerInterface;
        public static List<UIParticle> Particles = new List<UIParticle>();
        public override void Load()
        {
        }
        public override void Unload()
        {
            ScavengerUI = null;
            scavengerInterface = null;
            Particles.Clear();
        }
        public void OpenUI()
        {
            if (!Main.dedServ)
            {
                ScavengerUI = new ScavengerShopUIState();
                ScavengerUI.Activate();
                if (scavengerInterface == null)
                    scavengerInterface = new UserInterface();
            }
            scavengerInterface?.SetState(ScavengerUI);
            Main.playerInventory = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }
        public void CloseUI()
        {
            if (scavengerInterface?.CurrentState != null)
            {
                scavengerInterface.SetState(null);
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }
        public static void TrySpawnPurchaseEffect(Vector2 position)
        {
            int numParticles = 30;
            for (int i = 0; i < numParticles; i++)
            {
                UIParticle p = new UIParticle();
                p.Position = position;
                p.Velocity = Main.rand.NextVector2Circular(7f, 7f);
                p.Color = Main.rand.NextBool() ? Color.White : new Color(200, 50, 255);
                p.Scale = Main.rand.NextFloat(0.5f, 1.2f);
                p.MaxTime = Main.rand.Next(25, 45);
                p.TimeLeft = p.MaxTime;
                p.Rotation = 0f;
                p.RotSpeed = 0f;
                Particles.Add(p);
            }
            if (Particles.Count > 100)
            {
                Particles.RemoveRange(0, Particles.Count - 100);
            }
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (scavengerInterface?.CurrentState != null)
            {
                if (!Main.playerInventory || Main.LocalPlayer.talkNPC == -1 || Main.npc[Main.LocalPlayer.talkNPC].type != ModContent.NPCType<ScavengerNPC>())
                {
                    CloseUI();
                    return;
                }
                float scale = 1f;
                int originalMouseX = Main.mouseX;
                int originalMouseY = Main.mouseY;

                if (scale != 1f && scale > 0.05f)
                {
                    Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
                    Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
                    Vector2 unscaledMouse = screenCenter + (mousePos - screenCenter) / scale;

                    Main.mouseX = (int)unscaledMouse.X;
                    Main.mouseY = (int)unscaledMouse.Y;
                }

                scavengerInterface.Update(gameTime);

                Main.mouseX = originalMouseX;
                Main.mouseY = originalMouseY;
            }
            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                var p = Particles[i];
                p.Position += p.Velocity;
                p.Velocity *= 0.90f;
                p.Rotation += p.RotSpeed;
                p.TimeLeft--;
                if (p.TimeLeft <= 0)
                {
                    Particles.RemoveAt(i);
                }
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Jarkos: Scavenger Shop UI", delegate
                {
                        if (scavengerInterface?.CurrentState != null)
                        {
                            float scale = 1f;
                            if (scale != 1f && scale > 0.05f)
                            {
                                float uiScale = Main.UIScale;
                                Vector2 uiCenter = new Vector2(Main.screenWidth / (2f * uiScale), Main.screenHeight / (2f * uiScale));
                                Matrix scaleMatrix = Matrix.CreateTranslation(-uiCenter.X, -uiCenter.Y, 0) * Matrix.CreateScale(scale, scale, 1) * Matrix.CreateTranslation(uiCenter.X, uiCenter.Y, 0) * Main.UIScaleMatrix;

                                Main.spriteBatch.End();
                                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, scaleMatrix);

                                int originalMouseX = Main.mouseX;
                                int originalMouseY = Main.mouseY;

                                Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
                                Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
                                Vector2 unscaledMouse = screenCenter + (mousePos - screenCenter) / scale;

                                Main.mouseX = (int)unscaledMouse.X;
                                Main.mouseY = (int)unscaledMouse.Y;

                                scavengerInterface.Draw(Main.spriteBatch, new GameTime());
                                DrawParticles();

                                Main.mouseX = originalMouseX;
                                Main.mouseY = originalMouseY;

                                Main.spriteBatch.End();
                                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
                            }
                            else
                            {
                                scavengerInterface.Draw(Main.spriteBatch, new GameTime());
                                DrawParticles();
                            }
                        }
                        else
                        {
                            DrawParticles();
                        }
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
        private void DrawParticles()
        {
            if (Particles.Count == 0) return;
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.MagicPixel.Value;
            
            foreach (var p in Particles)
            {
                float fade = (float)p.TimeLeft / p.MaxTime;
                Color drawColor = p.Color * fade;
                sb.Draw(tex, p.Position, new Rectangle(0, 0, 4, 4), drawColor, p.Rotation, new Vector2(2f, 2f), p.Scale, SpriteEffects.None, 0f);
            }
        }
    }
}
