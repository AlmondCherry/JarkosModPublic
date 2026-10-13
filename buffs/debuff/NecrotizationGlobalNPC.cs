using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class NecrotizationGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool necrotizationDebuff = false;
        public int necrotizationTimer = 0;

        public override void ResetEffects(NPC npc)
        {
            necrotizationDebuff = false;
        }

        public override void PostAI(NPC npc)
        {
            if (necrotizationDebuff)
            {
                necrotizationTimer++;

                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.CursedTorch, 0f, -1f, 100, default, 1.1f);
                    d.noGravity = true;
                }

                if (necrotizationTimer >= 60)
                {
                    necrotizationTimer = 0;

                    int loss = 25;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.SimpleStrikeNPC(loss, 0, false, 0f, null, false, 0, false);
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                        Dust d = Dust.NewDustPerfect(npc.Center, DustID.CursedTorch, vel, 100, default, 1.4f);
                        d.noGravity = true;
                    }

                    for (int p = 0; p < Main.maxPlayers; p++)
                    {
                        Player player = Main.player[p];
                        if (player.active && !player.dead && Vector2.Distance(npc.Center, player.Center) <= 1200f)
                        {
                            player.statLife += loss;
                            player.HealEffect(loss);
                            if (player.statLife > player.statLifeMax2)
                            {
                                player.statLife = player.statLifeMax2;
                            }

                            for (int i = 0; i < 8; i++)
                            {
                                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                                Dust d = Dust.NewDustPerfect(player.Center, DustID.GreenTorch, vel, 100, default, 1.3f);
                                d.noGravity = true;
                            }
                        }
                    }
                }
            }
            else
            {
                necrotizationTimer = 0;
            }
        }
    }
}
