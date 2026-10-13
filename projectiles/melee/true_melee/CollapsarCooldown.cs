using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class CollapsarCooldown : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.ChaosState;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (Main.rand.NextBool(20))
            {
                Dust d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Electric, 0f, 0f, 0, default, 0.7f);
                d.noGravity = true;
                d.velocity *= 0.4f;
            }

            if (player.buffTime[buffIndex] == 60 && player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.5f }, player.Center);
                for (int i = 0; i < 20; i++)
                {
                    Dust d = Dust.NewDustPerfect(player.Center, DustID.Electric, Main.rand.NextVector2Circular(5f, 5f), 0, Color.Cyan, 1.3f);
                    d.noGravity = true;
                }
            }
        }
    }
}
