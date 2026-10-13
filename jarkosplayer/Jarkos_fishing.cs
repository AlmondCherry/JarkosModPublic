using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
namespace jarkos
{
    public class Jarkos_fishing : ModPlayer
    {
        public bool better_potion_crate_buff = false;
        public override void ResetEffects()
        {
            better_potion_crate_buff = false;
        }
        public override void UpdateDead()
        {
            ResetEffects();
        }
        public override void ModifyFishingAttempt(ref FishingAttempt attempt)
        {
             if (better_potion_crate_buff && !attempt.crate)
            {
                if (Main.rand.Next(100) < 50)
                {
                    attempt.crate = true;
                }
            }
        }
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            bool inWater = !attempt.inLava && !attempt.inHoney;
			bool inBiomeJungle = Player.ZoneJungle;
				 if(itemDrop == ItemID.NeonTetra && Main.rand.NextBool())
                {
                    itemDrop = ItemID.NaturesGift;
					return;
                }
                if(itemDrop == ItemID.NeonTetra && Main.rand.NextBool())
                {
                    itemDrop = ItemID.NaturesGift;
					return;
                }
        }
    }
}
