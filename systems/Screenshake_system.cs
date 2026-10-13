using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
namespace jarkos
{
    public class Screenshake_system : ModPlayer
    {
        #region Screenshake Effects
        public float weak_shake = 0;
        public float regular_shake = 0;
        public float stronger_shake = 0;
        public override void ModifyScreenPosition()
        {
            #region Weak Shake
            if (weak_shake >= 5)
            {
                weak_shake = 0;
            }
            if (weak_shake > 0)
            {
                float ws_strength = 1.25f;
                Main.screenPosition = Main.screenPosition + new Vector2(Main.rand.NextFloat(-ws_strength, ws_strength), Main.rand.NextFloat(-ws_strength, ws_strength));
                weak_shake++;
                if (weak_shake > 5)
                {
                    weak_shake = 0;
                }
            }
            #endregion
            #region Regular Shake
            if (regular_shake >= 5)
            {
                regular_shake = 0;
            }
            if (regular_shake > 0)
            {
                float rs_strength = 2.5f;
                Main.screenPosition = Main.screenPosition + new Vector2(Main.rand.NextFloat(-rs_strength, rs_strength), Main.rand.NextFloat(-rs_strength, rs_strength));
                regular_shake++;
                if (regular_shake > 5)
                {
                    regular_shake = 0;
                }
            }
            #endregion
            #region Strong Shake
            if (stronger_shake >= 5)
            {
                stronger_shake = 0;
            }
            if (stronger_shake > 0)
            {
                float ss_strength = 4f;
                Main.screenPosition = Main.screenPosition + new Vector2(Main.rand.NextFloat(-ss_strength, ss_strength), Main.rand.NextFloat(-ss_strength, ss_strength));
                stronger_shake++;
                if (stronger_shake > 5)
                {
                    stronger_shake = 0;
                }
            }
            #endregion
        }
        #endregion
    }
}
