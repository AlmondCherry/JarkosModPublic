using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using jarkos.particles;
using Terraria.DataStructures;
namespace jarkos.npc.Punisher.Cutscene
{
    public class PunisherSummonCutscene : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }
        private Player Owner => Main.player[Projectile.owner];
        private ref float Timer => ref Projectile.ai[0];
        public override void AI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Owner.velocity.X *= 0.8f;
            Timer++;
            if (Timer == 30)
            {
                SoundEngine.PlaySound(SoundID.NPCHit1, Owner.Center);
                for (int i = 0; i < 40; i++)
                {
                    new Spark(Owner.Center, Main.rand.NextVector2Circular(12f, 12f), Color.DarkRed, 40, Main.rand.NextFloat(1f, 2.5f), 1f).Spawn();
                    new Stretching_particle_out(Owner.Center, Main.rand.NextVector2Circular(20f, 20f), Color.IndianRed, 30, Main.rand.NextFloat(20f, 50f), Main.rand.NextFloat(2f, 6f), 0f).Spawn();
                }
                PunisherCinematicSystem.ScreenShakeAmount = 25f;
                PunisherCinematicSystem.PainOpacity = 1f;
            }
            if (Timer > 30 && Timer < 220)
            {
                PunisherCinematicSystem.ScreenShakeAmount = 5f + (Timer / 40f);
                Vector2 portalCenter = Owner.Center + new Vector2(150f * Owner.direction, -30f);
                if (Main.rand.NextBool(2))
                {
                    Vector2 spawnPos = portalCenter + Main.rand.NextVector2CircularEdge(200f, 250f);
                    Vector2 velocity = (portalCenter - spawnPos) * 0.05f;
                    new Spark(spawnPos, velocity, Main.rand.NextBool() ? Color.DarkRed : Color.Orange, 40, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
                }
            }
            if (Timer == 220)
            {
                Vector2 portalCenter = Owner.Center + new Vector2(150f * Owner.direction, -30f);
                SoundEngine.PlaySound(SoundID.Roar, portalCenter);
                PunisherCinematicSystem.ScreenShakeAmount = 40f;
                PunisherCinematicSystem.WhiteFlashOpacity = 1f;
                for (int i = 0; i < 60; i++)
                {
                    new Spark(portalCenter, Main.rand.NextVector2Circular(30f, 30f), Color.OrangeRed, 60, Main.rand.NextFloat(2f, 4f), 1f).Spawn();
                    new Stretching_particle_out(portalCenter, Main.rand.NextVector2Circular(25f, 25f), Color.Yellow, 40, Main.rand.NextFloat(30f, 60f), Main.rand.NextFloat(3f, 8f), 0f).Spawn();
                }
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.NewNPC(Projectile.GetSource_FromThis(), (int)portalCenter.X, (int)portalCenter.Y, ModContent.NPCType<Punisher>());
                }
            }
            if (Timer >= 300)
            {
                Owner.GetModPlayer<PunisherPlayer>().inCutscene = false;
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
