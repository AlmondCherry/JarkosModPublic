using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
namespace jarkos.npc.Punisher.Cutscene
{
    public class RitualKnifeLayer : PlayerDrawLayer
    {
        private static Asset<Texture2D> knifeTex;
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var cp = drawInfo.drawPlayer.GetModPlayer<HarakiriCutscenePlayer>();
            return cp.CutsceneActive && cp.Timer < HarakiriCutscenePlayer.PORTAL_TICK + 60;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var cp = player.GetModPlayer<HarakiriCutscenePlayer>();
            knifeTex ??= ModContent.Request<Texture2D>("Jarkos/npc/Punisher/Cutscene/RitualKnife");
            Texture2D tex = knifeTex.Value;
            int dir = player.direction;
            int timer = cp.Timer;
            Vector2 off;
            float rot;
            if (timer < HarakiriCutscenePlayer.RAISE_END)
            {
                float t = timer / (float)HarakiriCutscenePlayer.RAISE_END;
                float e = HarakiriCutscenePlayer.EaseOut(t);
                off = Vector2.Lerp(new Vector2(14f, 4f), new Vector2(8f, -38f), e);
                rot = MathHelper.Lerp(-0.4f, 1.7f, e);
            }
            else if (timer < HarakiriCutscenePlayer.STAB_TICK)
            {
                float t = (timer - HarakiriCutscenePlayer.RAISE_END) / (float)(HarakiriCutscenePlayer.STAB_TICK - HarakiriCutscenePlayer.RAISE_END);
                off = Vector2.Lerp(new Vector2(8f, -38f), new Vector2(4f, -2f), t * t);
                rot = MathHelper.Lerp(1.7f, 2.4f, t);
            }
            else
            {
                off = new Vector2(4f, -2f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                rot = 2.4f + (float)Math.Sin(timer * 0.35f) * 0.05f;
            }
            off.X *= dir;
            if (dir == -1)
                rot = -rot;
            Vector2 drawPos = (player.Center - Main.screenPosition + off).Floor();
            SpriteEffects fx = dir == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            drawInfo.DrawDataCache.Add(new DrawData(tex, drawPos, null, Color.White, rot, tex.Size() / 2f, 1f, fx));
        }
    }
}
