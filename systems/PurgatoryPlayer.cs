using System;
using jarkos.buffs.buff;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
namespace jarkos.systems
{
    public class PurgatoryPlayer : ModPlayer
    {
        public const int ArrowRainHealCap = 25;
        public int SwingVariation;
        public int FusionSpearSwingVariation;
        public int ArrowRainHealedThisCast;
        public int AbilityCooldown;
        public int RollbackLMBTimer = 150;
        public int RollbackRMBTimer = 25;
        public int ScarletTravelerCooldown;
        public int ShakeTimer;
        public int ShakeMaxTimer;
        public float ShakeIntensity;
        public float AtomgunHeat;
        public int AtomgunOverheatTime;
        public int AtomgunTimeSinceLastShot;
        public override void ResetEffects()
        {
            if (AbilityCooldown > 0)
            {
                AbilityCooldown--;
            }
        }
        public override void UpdateBadLifeRegen()
        {
            if (AtomgunHeat >= 300f && AtomgunTimeSinceLastShot < 5)
            {
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 30;
                AtomgunOverheatTime++;
                if (AtomgunOverheatTime >= 900)
                {
                    ExplodeAtomgun();
                }
            }
        }
        private void ExplodeAtomgun()
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                Player.KillMe(PlayerDeathReason.ByCustomReason(Player.name + " overheated their atom gun."), 9999, 0);
            }

            AtomgunHeat = 0;
            AtomgunOverheatTime = 0;
        }
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (Player.HasBuff(ModContent.BuffType<PurgatorySanctuaryBuff>()))
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }
        public override void PostUpdate()
        {
            if (ShakeTimer > 0)
            {
                ShakeTimer--;
            }
            else
            {
                ShakeMaxTimer = 0;
                ShakeIntensity = 0f;
            }
            if (RollbackLMBTimer < 150)
            {
                RollbackLMBTimer++;
            }
            if (RollbackRMBTimer < 25)
            {
                RollbackRMBTimer++;
            }
            if (ScarletTravelerCooldown > 0)
            {
                ScarletTravelerCooldown--;
            }
            AtomgunTimeSinceLastShot++;
            if (AtomgunTimeSinceLastShot >= 60)
            {
                if (AtomgunHeat > 0)
                    AtomgunHeat -= 8f;
                if (AtomgunHeat < 0)
                    AtomgunHeat = 0;
                AtomgunOverheatTime = 0;
            }
        }

        public override void ModifyScreenPosition()
        {
            if (Player.whoAmI != Main.myPlayer || ShakeTimer <= 0 || ShakeMaxTimer <= 0)
            {
                return;
            }

            float progress = ShakeTimer / (float)ShakeMaxTimer;
            float falloff = progress * progress;

            Main.screenPosition += Main.rand.NextVector2Circular(ShakeIntensity, ShakeIntensity * 0.7f) * falloff;
        }

        public void AddShake(float intensity, int duration)
        {
            if (Player.whoAmI != Main.myPlayer)
            {
                return;
            }
            ShakeTimer = Math.Max(ShakeTimer, duration);
            ShakeMaxTimer = Math.Max(ShakeMaxTimer, duration);
            ShakeIntensity = Math.Max(ShakeIntensity, intensity);
        }
        public bool TryHealFromArrowRain()
        {
            if (ArrowRainHealedThisCast >= ArrowRainHealCap)
            {
                return false;
            }

            ArrowRainHealedThisCast++;
            Player.statLife = Math.Min(Player.statLife + 1, Player.statLifeMax2);
            Player.HealEffect(1, true);
            
            return true;
        }
    }
}
