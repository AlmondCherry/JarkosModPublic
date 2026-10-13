using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace jarkos.npc.Punisher.Cutscene
{
    public class PortalEmergence : GlobalNPC
    {
        public const int EMERGE_TIME = 140;
        public const int GRACE_TIME = 180;
        private const int SPRITE_FLIP = -1;
        public override bool InstancePerEntity => true;
        public bool Emerging;
        public int EmergeTimer;
        public int EmergeDir = 1;
        public static void Begin(NPC npc, int direction)
        {
            var ge = npc.GetGlobalNPC<PortalEmergence>();
            ge.Emerging = true;
            ge.EmergeTimer = 0;
            ge.EmergeDir = direction;
            npc.alpha = 255;
            npc.direction = direction;
            npc.spriteDirection = direction * SPRITE_FLIP;
            npc.netUpdate = true;
        }
        public override bool PreAI(NPC npc)
        {
            if (!Emerging)
                return true;
            EmergeTimer++;
            npc.timeLeft = 750;
            npc.direction = EmergeDir;
            npc.spriteDirection = EmergeDir * SPRITE_FLIP;
            if (EmergeTimer <= EMERGE_TIME)
            {
                npc.dontTakeDamage = true;
                npc.damage = 0;
                npc.velocity.X = EmergeDir * 0.55f;
                npc.alpha = (int)MathHelper.Clamp(MathHelper.Lerp(255f, 0f, EmergeTimer / (float)EMERGE_TIME), 0f, 255f);
                if (EmergeTimer == EMERGE_TIME)
                {
                    npc.alpha = 0;
                    npc.dontTakeDamage = false;
                    npc.netUpdate = true;
                }
            }
            else
            {
                npc.damage = 0;
                npc.velocity.X *= 0.8f;
                npc.TargetClosest(false);
                Player target = Main.player[npc.target];
                if (target.active && !target.dead)
                {
                    npc.direction = target.Center.X > npc.Center.X ? 1 : -1;
                    npc.spriteDirection = npc.direction * SPRITE_FLIP;
                }
                if (EmergeTimer >= EMERGE_TIME + GRACE_TIME)
                {
                    Emerging = false;
                    npc.velocity = Vector2.Zero;
                    npc.damage = npc.defDamage;
                    npc.netUpdate = true;
                }
            }
            return false;
        }
    }
}
