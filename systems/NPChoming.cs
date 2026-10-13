using Microsoft.Xna.Framework;
using Terraria;

namespace jarkos.systems 
{
public static partial class NPChoming
{
		public static void HomingProj(Projectile proj, float distance, float mult, int delay_to_aim){
            Vector2 tonpc = proj.Center;
            float maxDistance = distance;
            bool locatedTarget = false;
            float npcDistCompare = 25000f;
            int index = -1;
            foreach (NPC n in Main.ActiveNPCs)
            {
                float extraDistance = (n.width / 2) + (n.height / 2);
                if (!n.CanBeChasedBy(proj, false) || !proj.WithinRange(n.Center, maxDistance + extraDistance))
                    continue;
                float currentNPCDist = Vector2.Distance(n.Center, proj.Center);
                if ((currentNPCDist < npcDistCompare) && Collision.CanHit(proj.Center, 1, 1, n.Center, 1, 1))
                {
                    npcDistCompare = currentNPCDist;
                    index = n.whoAmI;
                }
            }
            if (index != -1)
            {
                tonpc = Main.npc[index].Center;
                locatedTarget = true;
            }
            if (locatedTarget)
            {
                if(proj.timeLeft < delay_to_aim)
                {
                    Vector2 homeDirection = (tonpc - proj.Center).SafeNormalize(Vector2.Zero);
                    proj.velocity = proj.velocity + homeDirection * mult;
                    proj.velocity *= 0.94f;
                }
            }
        }
	}
}
