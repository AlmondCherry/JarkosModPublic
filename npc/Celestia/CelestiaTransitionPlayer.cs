using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaTransitionPlayer : ModPlayer
    {
        private uint lastKnockbackApplied;

        public override void PostUpdate()
        {
            if (!CelestiaTransitionSystem.TransitionActive)
                return;

            if (CelestiaTransitionSystem.KnockbackTick != 0 && lastKnockbackApplied != CelestiaTransitionSystem.KnockbackTick && Main.GameUpdateCount - CelestiaTransitionSystem.KnockbackTick < 10)
            {
                lastKnockbackApplied = CelestiaTransitionSystem.KnockbackTick;

                Vector2 away = (Player.Center - CelestiaTransitionSystem.KnockbackFrom).SafeNormalize(-Vector2.UnitY);

                Player.velocity = away * 13f + new Vector2(0f, -3f);
                Player.fullRotation = 0f;
            }

            if (CelestiaTransitionSystem.Whiteout > 0.3f || CelestiaTransitionSystem.ShockProgress >= 0f)
            {
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, 2);
            }

            if (CelestiaTransitionSystem.SuctionPhase && !Player.dead)
            {
                float zoom = Main.GameViewMatrix.Zoom.X;
                float halfW = Main.screenWidth * 0.5f / zoom + 24f;
                float halfH = Main.screenHeight * 0.5f / zoom + 24f;

                Vector2 d = Player.Center - CelestiaTransitionSystem.FocusPoint;
                if (Math.Abs(d.X) > halfW || Math.Abs(d.Y) > halfH)
                {
                    Player.immune = false;
                    Player.immuneTime = 0;
                    Player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{Player.name} was.")), 9999, 0);
                }
            }
        }
        public override bool CanUseItem(Item item)
        {
            if (CelestiaTransitionSystem.TransitionActive &&
                (item.type == ItemID.MagicMirror
                || item.type == ItemID.IceMirror
                || item.type == ItemID.CellPhone
                || item.type == ItemID.RecallPotion
                || item.type == ItemID.TeleportationPotion
                || item.type == ItemID.Shellphone))
                return false;
            return true;
        }
    }
}
