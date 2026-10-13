using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace jarkos.npc.tenderote
{
    public class TenderoteBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            if (bossHeadIndex == -1)
            {
                bossHeadIndex = ModContent.GetModBossHeadSlot("jarkos/npc/tenderote/tenderote_head_Head_Boss");
            }
            if (bossHeadIndex != -1)
            {
                return TextureAssets.NpcHeadBoss[bossHeadIndex];
            }
            return null;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;

            float widthFill = 424f;
            float fillHeight = 16f;

            float currentWidth = widthFill * (drawParams.Life / drawParams.LifeMax);
            Effect healthShader = null;

            try
            {
                healthShader = ModContent.Request<Effect>("jarkos/Shaders/TenderoteHealthBar", AssetRequestMode.ImmediateLoad).Value;
            } 
            catch
            {
            
            }

            if (healthShader != null)
            {
                spriteBatch.End();
                healthShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, healthShader, Main.UIScaleMatrix);
                Vector2 fillPos = new Vector2(drawParams.BarCenter.X - widthFill / 2f, drawParams.BarCenter.Y - fillHeight / 2f);

                spriteBatch.Draw(tex, fillPos, null, Color.White, 0f, Vector2.Zero, new Vector2(currentWidth / tex.Width, fillHeight / tex.Height), SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            }
            Vector2 pos50 = drawParams.BarCenter + new Vector2(0f, 0f);
            Vector2 pos25 = drawParams.BarCenter + new Vector2(-widthFill / 4f, 0f);

            Vector2 scale = new Vector2(2f, 20f);
            Vector2 origin = new Vector2(0.5f, 0.5f);

            spriteBatch.Draw(tex, pos50, new Rectangle(0, 0, 1, 1), Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, pos25, new Rectangle(0, 0, 1, 1), Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
