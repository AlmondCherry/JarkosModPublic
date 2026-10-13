using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
namespace jarkos.projectiles.magic.other
{
    public class StellarPlasma : Laserbeam, ILocalizedModType
    {
        public string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "jarkos/textures/beam_start";
        public override float Lifetime => 36000f;
        public override float MaxScale => 1f;
        public override float MaxLaserLength => 2400f;
        public override bool Glowing => false;
        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("jarkos/textures/beam_start", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("jarkos/textures/beam_mid", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("jarkos/textures/beam_end", AssetRequestMode.ImmediateLoad).Value;
        public override bool UseFlowShader => false;
        public override bool UseGlowShader => false;
        private const float MAX_CHARGE = 90f;
        private const float STAR_CONSUME_TIME = 90f;
        private const float BURST_TIME = 12f;
        private float ChargeTimer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float BurstTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private float ConsumeTimer
        {
            get => Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        private float BeamTicks
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override float DetermineLaserLength()
        {
            float maxDist = DetermineLaserLength_CollideWithTiles(36);
            float minDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                bool isTargetDummy = npc.type == NPCID.TargetDummy || (npc.ModNPC != null && npc.ModNPC.GetType().Name.ToLower().Contains("dummy"));
                if (npc.active && (!npc.friendly || isTargetDummy) && !npc.dontTakeDamage)
                {
                    float collisionPoint = 0f;
                    if (Collision.CheckAABBvLineCollision(npc.Hitbox.TopLeft(), npc.Hitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * maxDist, 14f * Projectile.scale, ref collisionPoint))
                    {
                        if (collisionPoint < minDist)
                        {
                            minDist = collisionPoint + 8f;
                        }
                    }
                }
            }
            return minDist;
        }
        public override void Behavior()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active || player.noItems || player.CCed || !player.channel)
            {
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 2;
            player.itemTime = 2;
            player.itemAnimation = 2;
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 dir = Main.MouseWorld - player.MountedCenter;
                dir.Normalize();
                if (dir != Projectile.velocity)
                {
                    Projectile.velocity = dir;
                    Projectile.netUpdate = true;
                }
            }
            Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false);
            Vector2 shake = Vector2.Zero;
            if (ChargeTimer >= MAX_CHARGE)
            {
                shake = Main.rand.NextVector2Circular(2f, 2f);
                player.itemLocation += shake;

                if (Projectile.owner == Main.myPlayer)
                {
                    if (player.statMana < 10)
                    {
                        Projectile.Kill();
                        return;
                    }

                    BeamTicks++;
                    if (BeamTicks % 25f == 0f)
                    {
                        int secondsActive = (int)(BeamTicks / 60f);
                        int currentManaCost = 10 + (secondsActive * 5);

                        if (!player.CheckMana(currentManaCost, pay: true))
                        {
                            Projectile.Kill();
                            return;
                        }
                    }

                    if (BeamTicks % 30f == 0f)
                    {
                        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/StellarRifleWorking") with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.5f, 0.5f) }, Projectile.Center);
                    }
                }
            }
            Vector2 offset = new Vector2(100f, 0f).RotatedBy(Projectile.velocity.ToRotation());
            Projectile.Center = rrp + offset + shake;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * player.direction, Projectile.velocity.X * player.direction);
            if (ChargeTimer < MAX_CHARGE)
            {
                ChargeTimer++;
                Projectile.friendly = false;
                LaserLength = 0f;
                Projectile.scale = 0f;
                if (Main.netMode != NetmodeID.Server)
                {
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 particleOffset = Main.rand.NextVector2CircularEdge(40f, 40f);
                        float progress = ChargeTimer / MAX_CHARGE;
                        particleOffset *= (1f - progress * 0.8f);
                        new Solid_particle(Projectile.Center + particleOffset, -particleOffset * 0.08f, Color.Yellow, 30, 0.8f, 0.8f, Main.rand.NextFloat(-0.05f, 0.05f)).Spawn();
                    }
                    if (ChargeTimer % 10 == 0)
                    {
                        new Circular_glow(Projectile.Center, Vector2.Zero, Color.Yellow * 0.6f, 20, 0.2f, 0.6f, 0.05f).Spawn();
                        new Circleedge_reducing(Projectile.Center, Vector2.Zero, Color.Yellow * 0.5f, 15, 1.2f, 0.7f, 0.02f).Spawn();
                    }
                }
                if (ChargeTimer == MAX_CHARGE)
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/StellarRifleStarting") with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);
                }
            }
            else
            {
                Projectile.friendly = true;
                LaserLength = DetermineLaserLength();
                if (Main.netMode != NetmodeID.Server)
                {
                    if (Main.rand.NextBool(3))
                    {
                        float randDist = Main.rand.NextFloat(LaserLength);
                        Vector2 pos = Projectile.Center + Projectile.velocity * randDist;
                        new Spark(pos, Projectile.velocity * Main.rand.NextFloat(2f, 6f) + Main.rand.NextVector2Circular(1f, 1f), Color.Yellow, 25, Main.rand.NextFloat(0.4f, 0.8f), 0.9f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
                    }
                    if (Main.rand.NextBool(4))
                    {
                        float randDist = Main.rand.NextFloat(LaserLength);
                        Vector2 pos = Projectile.Center + Projectile.velocity * randDist;
                        new Solid_particle(pos, Projectile.velocity * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(0.5f, 0.5f), Color.Gold, 20, Main.rand.NextFloat(0.5f, 1.0f), 0.8f, Main.rand.NextFloat(-0.08f, 0.08f)).Spawn();
                    }
                    if (LaserLength > 0f)
                    {
                        Vector2 endPoint = Projectile.Center + Projectile.velocity * LaserLength;
                        if (Main.rand.NextBool(2))
                        {
                            new Spark(endPoint, (-Projectile.velocity).RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 8f), Color.Yellow, 30, Main.rand.NextFloat(0.6f, 1.2f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
                        }
                        if (Main.rand.NextBool(3))
                        {
                            new Solid_particle(endPoint, (-Projectile.velocity).RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 6f), Color.Gold, 25, Main.rand.NextFloat(0.8f, 1.4f), 0.9f, Main.rand.NextFloat(-0.08f, 0.08f)).Spawn();
                        }
                        if (Main.rand.NextBool(8))
                        {
                            new Circular_glow(endPoint, Vector2.Zero, Color.Yellow * 0.8f, 15, 0.4f, 0.8f, 0.05f).Spawn();
                        }
                        if (Main.rand.NextBool(12))
                        {
                            new Circleedge(endPoint, Vector2.Zero, Color.Yellow * 0.7f, 12, 0.3f, 0.8f, 0.06f).Spawn();
                        }
                    }
                }
                if (BurstTimer > 0)
                {
                    BurstTimer--;
                    Projectile.scale = 1.6f + (float)Math.Sin(BurstTimer) * 0.2f;
                    if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                    {
                        float randDist = Main.rand.NextFloat(LaserLength);
                        Vector2 pos = Projectile.Center + Projectile.velocity * randDist;
                        Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X) * Main.rand.NextFloat(-4f, 4f);
                        new Spark(pos, Projectile.velocity * 5f + perp, Color.Cyan, 30, Main.rand.NextFloat(0.8f, 1.4f), 1f, Main.rand.NextFloat(-0.05f, 0.05f)).Spawn();
                    }
                }
                else
                {
                    ConsumeTimer++;
                    Projectile.scale = 0.8f;
                    if (ConsumeTimer >= STAR_CONSUME_TIME)
                    {
                        ConsumeTimer = 0;
                        if (ConsumeFallenStar(player))
                        {
                            BurstTimer = BURST_TIME;
                            SoundEngine.PlaySound(SoundID.Item43 with { Pitch = 0.2f }, Projectile.Center);
                            if (Main.netMode != NetmodeID.Server)
                            {
                                for (int i = 0; i < 15; i++)
                                {
                                    new Spark(Projectile.Center, Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 8f), Color.Cyan, 25, Main.rand.NextFloat(0.6f, 1.2f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
                                }
                            }
                        }
                    }
                }
                DelegateMethods.v3_1 = new Vector3(1f, 0.9f, 0.2f);
                Terraria.Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, 14, DelegateMethods.CastLight);
            }
        }
        private bool ConsumeFallenStar(Player player)
        {
            for (int i = 0; i < 58; i++)
            {
                if (player.inventory[i].type == ItemID.FallenStar && player.inventory[i].stack > 0)
                {
                    player.inventory[i].stack--;
                    if (player.inventory[i].stack <= 0)
                        player.inventory[i].TurnToAir();
                    return true;
                }
            }
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (ChargeTimer < MAX_CHARGE) return false;
            float collisionPoint = 0f;
            float width = BurstTimer > 0 ? 30f : 8f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, width, ref collisionPoint);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (BurstTimer > 0)
            {
                modifiers.FinalDamage *= 2.5f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (ChargeTimer < MAX_CHARGE) return false;
            DrawBeamWithColor(Color.Yellow * 1.1f, Projectile.scale * 1f);
            DrawBeamWithColor(Color.White * 1.1f, Projectile.scale * 0.5f);
            return false;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
