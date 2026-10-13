using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.particles.storm;

namespace jarkos.projectiles.melee.true_melee
{
    public class AegisExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        private const int Lifetime = 45;
        private float Progress => 1f - Projectile.timeLeft / (float)Lifetime;
        public override void SetDefaults() {
            Projectile.width = Projectile.height = 260;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI() {
            if (Projectile.timeLeft == Lifetime) {
                SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.4f, Volume = 1.2f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.1f }, Projectile.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center,
                    Main.rand.NextVector2Unit(), 18f, 10f, 30, 1600f));


                ModContent.GetInstance<ShaderSystem>()?.StartExplosion(Projectile.Center, 1.4f);
                ShaderEchoSystem.SpawnPulse(Projectile.Center, 1.6f);
                CelestiaEnergyBurstSystem.Burst(Projectile.Center, 1.4f);


                foreach (NPC npc in Main.ActiveNPCs) {
                    if (npc.active && Vector2.Distance(npc.Center, Projectile.Center) < 350f) {
                        if (npc.TryGetGlobalNPC<MarkedNPC>(out var marked)) {
                            marked.RemoveMark();
                        }
                    }
                }


                for (int i = 0; i < 40; i++) {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.Torch : DustID.CopperCoin,
                        Main.rand.NextVector2Circular(14f, 14f), 0, default, Main.rand.NextFloat(1.8f, 3f));
                    d.noGravity = true;
                }


                for (int i = 0; i < 50; i++) {
                    Vector2 smokeVel = Main.rand.NextVector2Circular(10f, 10f) - Vector2.UnitY * Main.rand.NextFloat(1f, 4f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, smokeVel, 80, Color.DarkGray, Main.rand.NextFloat(1.8f, 3.2f));
                    d.noGravity = Main.rand.NextBool();
                }


                if (Main.netMode != NetmodeID.Server) {

                    for (int i = 0; i < 35; i++) {
                        Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 16f);
                        Color grayColor = Color.Lerp(new Color(50, 50, 55), new Color(160, 160, 170), Main.rand.NextFloat());
                        new Fire_particle(
                            Projectile.Center,
                            vel,
                            grayColor,
                            lifetime: Main.rand.Next(20, 40),
                            scale: Main.rand.NextFloat(1.0f, 2.0f),
                            opacity: 0.95f,
                            rotationSpeed: Main.rand.NextFloat(-0.12f, 0.12f)
                        ).Spawn();
                    }


                    for (int i = 0; i < 20; i++) {
                        Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 18f);
                        Color orangeRedColor = Color.Lerp(new Color(255, 60, 20), new Color(255, 140, 30), Main.rand.NextFloat());
                        new Fire_particle(
                            Projectile.Center,
                            vel,
                            orangeRedColor,
                            lifetime: Main.rand.Next(12, 25),
                            scale: Main.rand.NextFloat(0.8f, 1.5f),
                            opacity: 0.9f,
                            rotationSpeed: Main.rand.NextFloat(-0.1f, 0.1f)
                        ).Spawn();
                    }
                }
            }
            Lighting.AddLight(Projectile.Center, 3.5f * (1f - Progress), 1.8f * (1f - Progress), 0.5f);
        }
        public override bool PreDraw(ref Color lightColor) {
            return false;
        }
    }
}
