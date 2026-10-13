using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using jarkos.particles;

namespace jarkos.npc.Celestia
{
    public class CelestiaOrbitalArena : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_89";

        public const float ArenaRadius = 1450f;
        private bool _orbsSpawned = false;
        private float _arenaAlpha = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 3000;
            Projectile.height = 3000;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1080;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 60)
            {
                _arenaAlpha = MathHelper.Lerp(_arenaAlpha, 1f, 0.05f);
            }
            else
            {
                _arenaAlpha = MathHelper.Lerp(_arenaAlpha, 0f, 0.08f);
            }

            if (!_orbsSpawned && Main.netMode != NetmodeID.MultiplayerClient)
            {
                _orbsSpawned = true;
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.2f, Volume = 1.1f }, Projectile.Center);

                for (float r = 200f; r <= 1400f; r += 200f)
                {
                    for (int arm = 0; arm < 4; arm++)
                    {
                        float armAngle = arm * MathHelper.PiOver2;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CelestiaOrbitalOrb>(), 45, 1f, Main.myPlayer, r, armAngle, Projectile.whoAmI);
                    }
                }
            }

            Projectile.localAI[0]++;
            if (Projectile.localAI[0] >= 40f && Projectile.timeLeft > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.localAI[0] = 0f;

                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                if (target != null && target.active && !target.dead)
                {
                    float spawnAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 outerSpawnPos = Projectile.Center + spawnAngle.ToRotationVector2() * (ArenaRadius + 450f);

                    Vector2 aimDir = (target.Center + target.velocity * 10f - outerSpawnPos).SafeNormalize(Vector2.UnitY);
                    float slantAngle = aimDir.ToRotation() - MathHelper.PiOver2;

                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), outerSpawnPos, Vector2.Zero, ModContent.ProjectileType<CelestiaSpearTelegraph>(), 0, 0f, Main.myPlayer, slantAngle);
                }
            }

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead)
                {
                    float distance = Vector2.Distance(player.Center, Projectile.Center);
                    if (distance > ArenaRadius)
                    {
                        Vector2 toCenter = (Projectile.Center - player.Center).SafeNormalize(Vector2.Zero);
                        player.Center = Projectile.Center - toCenter * ArenaRadius;
                        if (Vector2.Dot(player.velocity, toCenter) < 0)
                        {
                            player.velocity = Vector2.Zero;
                        }

                        if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                        {
                            for (int k = 0; k < 6; k++)
                            {
                                new Spark(player.Center + Main.rand.NextVector2Circular(20f, 20f), toCenter * Main.rand.NextFloat(4f, 10f), Color.Cyan, 20, 0.8f, 1f, 0f).Spawn();
                            }
                        }
                    }
                }
            }

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
            {
                float randAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                
                Vector2 rimPos = Projectile.Center + new Vector2((float)Math.Cos(randAngle), (float)Math.Sin(randAngle)) * ArenaRadius;
                Color pColor = Main.rand.NextBool() ? Color.Cyan : Color.Magenta;

                new StarParticle(rimPos, Main.rand.NextVector2Circular(2f, 2f), pColor * _arenaAlpha, 25, MathHelper.Clamp(0.35f * _arenaAlpha, 0.05f, 0.45f), 1f, 0f).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
