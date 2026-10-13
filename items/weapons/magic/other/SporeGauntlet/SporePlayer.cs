using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.weapons.magic.other.SporeGauntlet
{
    public class SporePlayerWeapon : ModPlayer
    {
        public bool Detonating;
        private int detonationTimer;
        private const int TicksBetweenBlasts = 11;
        public override void PostUpdate()
        {
            if (!Detonating)
                return;
            detonationTimer--;
            if (detonationTimer > 0)
                return;
            int best = -1;
            int bestTime = int.MaxValue;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.ModProjectile is SporeProj && p.timeLeft < bestTime)
                {
                    bestTime = p.timeLeft;
                    best = i;
                }
            }
            if (best < 0)
            {
                Detonating = false;
                SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.6f, Pitch = -0.3f }, Player.Center);
                return;
            }
            ((SporeProj)Main.projectile[best].ModProjectile).Explode(forced: true);
            detonationTimer = TicksBetweenBlasts;
        }
        public void StartDetonation()
        {
            Detonating = true;
            detonationTimer = 1;
        }
    }
}
