using System;
using System.Collections.Generic;
using jarkos.items.weapons.melee.other_melee;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.other_melee
{
	public class Antique_melee_axe_proj : ModProjectile
	{
        private enum AxeState
        {
            Flying, Stuck, Returning
        }
        private AxeState State
        {
            get => (AxeState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        private int StuckEnemyID
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private AxeState oldState = AxeState.Flying;
        private Vector2 stuckOffset;
        private int stuckTimer = 0;
        private int lifeTimer = 0;
        private const int MaxStuckTime = 1000;
        private int stuckDamageTimer = 0;
        private const int StuckDamageInterval = 100;
        private float returnSpeed = 0f;
        private HashSet<int> hitEnemiesOnReturn = new HashSet<int>();
        private List<Vector2> trailPositions = new List<Vector2>();
        private List<float> trailRotations = new List<float>();
        private const int TrailLength = 15;
        private float runeRotation = 0f;
        private float runeScale = 1f;
        private List<EnergyRing> energyRings = new List<EnergyRing>();
        private List<LightningBolt> lightningBolts = new List<LightningBolt>();
        private int lightningTimer = 0;
        private struct EnergyRing
        {
            public float Radius;
            public float MaxRadius;
            public float Alpha;
            public Color RingColor;
        }
        private struct LightningBolt
        {
            public Vector2 Start;
            public Vector2 End;
            public List<Vector2> Points;
            public float Alpha;
            public Color BoltColor;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1500;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 100;
            State = AxeState.Flying;
            StuckEnemyID = -1;
        }
        public override void AI()
        {
            if (State != oldState)
            {
                if (State == AxeState.Stuck)
                {
                    if (StuckEnemyID >= 0 && StuckEnemyID < Main.maxNPCs)
                    {
                        NPC target = Main.npc[StuckEnemyID];
                        if (target.active)
                        {
                            stuckOffset = Projectile.Center - target.Center;
                        }
                    }
                    stuckTimer = 0;
                    stuckDamageTimer = 0;
                }
                else if (State == AxeState.Returning)
                {
                    returnSpeed = 8f;
                    hitEnemiesOnReturn.Clear();
                }
                oldState = State;
            }

            Player owner = Main.player[Projectile.owner];
            lifeTimer++;
            if (Main.netMode != NetmodeID.Server)
            {
                UpdateTrail();
                UpdateRunes();
                UpdateEnergyRings();
                UpdateLightning();
            }
            switch (State)
            {
                case AxeState.Flying:
                    FlyingBehavior();
                    break;
                case AxeState.Stuck:
                    StuckBehavior(owner);
                    break;
                case AxeState.Returning:
                    ReturningBehavior(owner);
                    break;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                CreateDynamicLighting();
                SpawnAmbientParticles();
            }
        }
        private void FlyingBehavior()
        {
            Projectile.rotation += 0.35f * Math.Sign(Projectile.velocity.X);
            Projectile.velocity.Y += 0.12f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 dustPos = Projectile.Center - Projectile.velocity * 0.5f;
                    Dust fire = Dust.NewDustDirect(dustPos, 0, 0, DustID.Torch, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 2f);
                    fire.noGravity = true;
                    fire.fadeIn = 1.2f;
                }
                if (Main.rand.NextBool(3))
                {
                    Dust Spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0, 0, 150, default, 1.5f);
                    Spark.noGravity = true;
                    Spark.velocity = Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f);
                }
                if (Main.rand.NextBool(15))
                {
                    CreateMiniLightning(Projectile.Center, 30f);
                }
            }
        }
        private void StuckBehavior(Player owner)
        {
            stuckTimer++;
            float pulse = (float)Math.Sin(stuckTimer * 0.15f) * 0.03f;
            Projectile.scale = 1f + pulse;
            if (StuckEnemyID >= 0 && StuckEnemyID < Main.maxNPCs)
            {
                NPC target = Main.npc[StuckEnemyID];
                if (target.active && target.life > 0)
                {
                    Projectile.position = target.Center + stuckOffset - Projectile.Size / 2f;
                    stuckDamageTimer++;
                    if (stuckDamageTimer >= StuckDamageInterval)
                    {
                        stuckDamageTimer = 0;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            int pulseDamage = (int)(Projectile.damage * 0.45f);
                            int hitDirection = Projectile.Center.X < target.Center.X ? 1 : -1;
                            target.StrikeNPC(target.CalculateHitInfo(pulseDamage, hitDirection, false, Projectile.knockBack * 0.35f));
                            if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
                        }
                        if (Main.netMode != NetmodeID.Server)
                        {
                            ShaderEchoSystem.SpawnPulse(target.Center, 0.55f);
                            AddEnergyRing(target.Center, 90f, Color.OrangeRed);
                            CreateLightningToEnemy(target);
                            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/antique_axe_echo") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
                        }
                    }
                    if (Main.netMode != NetmodeID.Server)
                    {
                        if (Main.rand.NextBool(6))
                        {
                            Dust blood = Dust.NewDustDirect(Projectile.Center + new Vector2(0, 10), 0, 0, DustID.Blood, Main.rand.NextFloat(-1f, 1f), 2f, 0, default, 1.5f);
                        }
                        if (Main.rand.NextBool(4))
                        {
                            Vector2 dustPos = target.Center + Main.rand.NextVector2Circular(target.width / 2, target.height / 2);
                            Dust drain = Dust.NewDustDirect(dustPos, 0, 0, DustID.GoldFlame, 0, 0, 100, default, 1f);
                            drain.noGravity = true;
                            drain.velocity = (Projectile.Center - dustPos) * 0.1f;
                            drain.fadeIn = 1.5f;
                        }
                        if (stuckTimer % 30 == 0)
                        {
                            CreateLightningToEnemy(target);
                        }
                    }
                }
                else
                {
                    if (Projectile.owner == Main.myPlayer) StartReturning();
                }
            }
            Projectile.rotation += (float)Math.Sin(stuckTimer * 0.08f) * 0.015f;
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(10))
                {
                    SpawnRuneParticle();
                }
            }
            if (stuckTimer >= MaxStuckTime)
            {
                if (Projectile.owner == Main.myPlayer) StartReturning();
            }
        }
        private void ReturningBehavior(Player owner)
        {
            Projectile.tileCollide = false;
            returnSpeed += 1.2f;
            if (returnSpeed > 30f)
                returnSpeed = 30f;
            Vector2 direction = owner.Center - Projectile.Center;
            float distance = direction.Length();
            if (distance > 25f)
            {
                direction.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * returnSpeed, 0.15f);
                Projectile.rotation += 0.6f * Math.Sign(Projectile.velocity.X);
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 dustVel = -Projectile.velocity.RotatedByRandom(0.3f) * 0.3f;
                        Dust fire = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, dustVel.X, dustVel.Y, 100, default, 2.5f);
                        fire.noGravity = true;
                        fire.fadeIn = 1.5f;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        Dust gold = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.5f, 0, 0, DustID.GoldFlame, 0, 0, 100, default, 2f);
                        gold.noGravity = true;
                        gold.velocity = -Projectile.velocity * 0.1f;
                    }
                    if (lifeTimer % 8 == 0)
                    {
                        AddEnergyRing(Projectile.Center, 60f, Color.Orange);
                    }
                    if (Main.rand.NextBool(5))
                    {
                        CreateMiniLightning(Projectile.Center, 50f);
                    }
                }
            }
            else
            {
                CatchAxe(owner);
                return;
            }
            CheckReturnDamage();
        }
        private void CheckReturnDamage()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                if (hitEnemiesOnReturn.Contains(i))
                    continue;
                if (Projectile.Hitbox.Intersects(npc.Hitbox))
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        int returnDamage = (int)(Projectile.damage * 0.6f);
                        npc.StrikeNPC(npc.CalculateHitInfo(returnDamage, Projectile.velocity.X > 0 ? 1 : -1, false, Projectile.knockBack / 2));
                        if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                    }
                    hitEnemiesOnReturn.Add(i);
                    if (Main.netMode != NetmodeID.Server) SpawnReturnHitEffect(npc.Center);
                }
            }
        }
        public void StartReturning()
        {
            if (State == AxeState.Returning)
                return;
            State = AxeState.Returning;
            Projectile.netUpdate = true;
            returnSpeed = 8f;
            hitEnemiesOnReturn.Clear();
            if (StuckEnemyID >= 0 && StuckEnemyID < Main.maxNPCs)
            {
                NPC target = Main.npc[StuckEnemyID];
                if (target.active && target.life > 0)
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        int extractDamage = (int)(Projectile.damage * 1.75f);
                        bool crit = Main.rand.NextBool(4);
                        target.StrikeNPC(target.CalculateHitInfo(extractDamage, Projectile.velocity.X > 0 ? 1 : -1, crit, Projectile.knockBack * 1.5f));
                        if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
                    }
                    if (Main.netMode != NetmodeID.Server)
                    {
                        SpawnExtractionEffect(target, false);
                        ShaderEchoSystem.SpawnPulse(target.Center, 0.45f);
                    }
                }
            }
            if (Main.netMode != NetmodeID.Server)
            {
                SpawnReturnStartEffect();
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 1.2f }, Projectile.position);
            }
        }
        private void SpawnExtractionEffect(NPC target, bool crit)
        {
            if (Main.LocalPlayer.Distance(Projectile.Center) < 500f)
            {
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake++;
            }
            for (int i = 0; i < (crit ? 40 : 25); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(12f, 12f);
                Dust blood = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, velocity.X, velocity.Y, 0, default, Main.rand.NextFloat(1.5f, 3f));
                blood.noGravity = Main.rand.NextBool(3);
            }
            for (int i = 0; i < (crit ? 30 : 18); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(10f, 10f);
                Dust gold = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, velocity.X, velocity.Y, 100, default, Main.rand.NextFloat(2f, 3.5f));
                gold.noGravity = true;
                gold.fadeIn = 2f;
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust fire = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, 2.5f);
                fire.noGravity = true;
            }
            AddEnergyRing(Projectile.Center, crit ? 150f : 100f, crit ? Color.Red : Color.Orange);
            AddEnergyRing(Projectile.Center, crit ? 100f : 70f, Color.Gold);
            for (int i = 0; i < (crit ? 8 : 5); i++)
            {
                float angle = MathHelper.TwoPi * i / (crit ? 8 : 5);
                CreateDirectionalLightning(Projectile.Center, angle, crit ? 80f : 50f);
            }
            for (int i = 0; i < 5; i++)
            {
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), GoreID.Smoke1 + Main.rand.Next(3));
                gore.velocity *= 0.8f;
            }
            SoundEngine.PlaySound(SoundID.NPCHit1 with { Pitch = -0.5f, Volume = 1.5f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.7f }, Projectile.position);
        }
        private void SpawnReturnStartEffect()
        {
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                Dust ring = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, velocity.X, velocity.Y, 0, default, 2f);
                ring.noGravity = true;
                ring.fadeIn = 1.5f;
            }
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                Dust inner = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, 2.5f);
                inner.noGravity = true;
            }
            AddEnergyRing(Projectile.Center, 80f, Color.Orange);
        }
        private void SpawnReturnHitEffect(Vector2 position)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust Spark = Dust.NewDustDirect(position, 0, 0, DustID.GoldFlame, velocity.X, velocity.Y, 100, default, 1.8f);
                Spark.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust blood = Dust.NewDustDirect(position, 0, 0, DustID.Blood, velocity.X, velocity.Y, 0, default, 1.5f);
            }
            AddEnergyRing(position, 40f, Color.OrangeRed);
            SoundEngine.PlaySound(SoundID.NPCHit1, position);
        }
        private void CatchAxe(Player owner)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                    Dust catch_effect = Dust.NewDustDirect(owner.Center, 0, 0, DustID.GoldFlame, velocity.X, velocity.Y, 100, default, 1.8f);
                    catch_effect.noGravity = true;
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust flash = Dust.NewDustDirect(owner.Center, 0, 0, DustID.Torch, 0, 0, 100, default, 2f);
                    flash.noGravity = true;
                    flash.velocity = Main.rand.NextVector2Circular(3f, 3f);
                }
                SoundEngine.PlaySound(SoundID.Grab with { Pitch = 0.3f }, owner.position);
                SoundEngine.PlaySound(SoundID.Item35 with { Volume = 0.5f }, owner.position);
            }
            if (Projectile.owner == Main.myPlayer) Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (State == AxeState.Flying)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    State = AxeState.Stuck;
                    StuckEnemyID = target.whoAmI;
                    stuckOffset = Projectile.Center - target.Center;
                    Projectile.velocity = Vector2.Zero;
                    stuckTimer = 0;
                    stuckDamageTimer = 0;
                    Projectile.netUpdate = true;
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    SpawnImpactEffect(target, hit.Crit);
                    SoundEngine.PlaySound(SoundID.NPCHit1 with { Pitch = -0.2f }, Projectile.position);
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.position);
                }
            }
        }
        private void SpawnImpactEffect(NPC target, bool crit)
        {
            for (int i = 0; i < (crit ? 25 : 15); i++)
            {
                Vector2 velocity = -Projectile.oldVelocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.3f, 0.8f);
                velocity += Main.rand.NextVector2Circular(3f, 3f);
                Dust blood = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, velocity.X, velocity.Y, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
            }
            for (int i = 0; i < (crit ? 15 : 10); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, velocity.X, velocity.Y, 100, default, crit ? 2f : 1.5f);
                Spark.noGravity = true;
            }
            AddEnergyRing(Projectile.Center, crit ? 60f : 40f, crit ? Color.Gold : Color.Orange);
            if (crit)
            {
                for (int i = 0; i < 4; i++)
                {
                    CreateMiniLightning(Projectile.Center, 40f);
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (State == AxeState.Flying)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    State = AxeState.Stuck;
                    StuckEnemyID = -1;
                    Projectile.velocity = Vector2.Zero;
                    stuckTimer = 0;
                    stuckDamageTimer = 0;
                    Projectile.netUpdate = true;
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 velocity = -oldVelocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.2f, 0.5f);
                        Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, 1.5f);
                        Spark.noGravity = true;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        Dust stone = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, -oldVelocity.X * 0.2f, -oldVelocity.Y * 0.2f);
                    }
                    SoundEngine.PlaySound(SoundID.Dig with { Pitch = -0.3f }, Projectile.position);
                    SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.position);
                }
            }
            return false;
        }
        #region Visual Systems
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            trailRotations.Insert(0, Projectile.rotation);
            while (trailPositions.Count > TrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
                trailRotations.RemoveAt(trailRotations.Count - 1);
            }
        }
        private void UpdateRunes()
        {
            runeRotation += 0.03f;
            float targetScale = State == AxeState.Returning ? 1.5f : 1f;
            runeScale = MathHelper.Lerp(runeScale, targetScale, 0.1f);
        }
        private void UpdateEnergyRings()
        {
            for (int i = energyRings.Count - 1; i >= 0; i--)
            {
                EnergyRing ring = energyRings[i];
                ring.Radius += 4f;
                ring.Alpha -= 0.05f;
                if (ring.Alpha <= 0 || ring.Radius >= ring.MaxRadius)
                {
                    energyRings.RemoveAt(i);
                }
                else
                {
                    energyRings[i] = ring;
                }
            }
        }
        private void AddEnergyRing(Vector2 position, float maxRadius, Color color)
        {
            energyRings.Add(new EnergyRing            {
                Radius = 5f, MaxRadius = maxRadius, Alpha = 1f, RingColor = color
            });
        }
        private void UpdateLightning()
        {
            lightningTimer++;
            for (int i = lightningBolts.Count - 1; i >= 0; i--)
            {
                LightningBolt bolt = lightningBolts[i];
                bolt.Alpha -= 0.15f;
                if (bolt.Alpha <= 0)
                {
                    lightningBolts.RemoveAt(i);
                }
                else
                {
                    lightningBolts[i] = bolt;
                }
            }
        }
        private void CreateMiniLightning(Vector2 center, float maxLength)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float length = Main.rand.NextFloat(maxLength * 0.5f, maxLength);
            Vector2 end = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length;
            CreateLightningBolt(center, end, Color.Gold);
        }
        private void CreateDirectionalLightning(Vector2 center, float angle, float length)
        {
            Vector2 end = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length;
            CreateLightningBolt(center, end, Color.Orange);
        }
        private void CreateLightningToEnemy(NPC target)
        {
            Vector2 start = Projectile.Center;
            Vector2 end = target.Center + Main.rand.NextVector2Circular(target.width / 3, target.height / 3);
            CreateLightningBolt(start, end, Color.Red);
        }
        private void CreateLightningBolt(Vector2 start, Vector2 end, Color color)
        {
            List<Vector2> points = new List<Vector2>();
            points.Add(start);
            Vector2 direction = end - start;
            float distance = direction.Length();
            int segments = (int)(distance / 8f);
            segments = Math.Max(segments, 3);
            for (int i = 1; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 basePoint = Vector2.Lerp(start, end, progress);
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                perpendicular.Normalize();
                float offset = Main.rand.NextFloat(-15f, 15f) * (1f - Math.Abs(progress - 0.5f) * 2f);
                basePoint += perpendicular * offset;
                points.Add(basePoint);
            }
            points.Add(end);
            lightningBolts.Add(new LightningBolt            {
                Start = start, End = end, Points = points, Alpha = 1f, BoltColor = color
            });
        }
        private void SpawnRuneParticle()
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 25f;
            Dust rune = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.GoldFlame, 0, 0, 100, default, 1.2f);
            rune.noGravity = true;
            rune.velocity = (Projectile.Center - rune.position) * 0.05f;
            rune.fadeIn = 1.5f;
        }
        private void SpawnAmbientParticles()
        {
            if (State == AxeState.Flying && Main.rand.NextBool(4))
            {
                Dust ambient = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0, 0, 150, default, 0.8f);
                ambient.noGravity = true;
                ambient.velocity *= 0.3f;
            }
            else if (State == AxeState.Returning && Main.rand.NextBool(2))
            {
                Dust intense = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, 0, 0, 100, default, 1.5f);
                intense.noGravity = true;
                intense.velocity = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
            }
        }
        private void CreateDynamicLighting()
        {
            float intensity = State == AxeState.Returning ? 1.2f : 0.8f;
            float pulse = (float)Math.Sin(lifeTimer * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.6f, 0.2f) * intensity * pulse);
        }
        #endregion
        #region Drawing
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            DrawEnergyRings(spriteBatch);
            DrawLightning(spriteBatch);
            if (State == AxeState.Flying || State == AxeState.Returning)
            {
                DrawAdvancedTrail(spriteBatch, texture, origin);
            }
            DrawRunes(spriteBatch);
            DrawGlow(spriteBatch, texture, origin, lightColor);
            DrawMainSprite(spriteBatch, texture, origin, lightColor);
            return false;
        }
        private void DrawAdvancedTrail(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin)
        {
            for (int i = 0; i < trailPositions.Count; i++)
            {
                float progress = 1f - (float)i / trailPositions.Count;
                Color trailColor;
                if (State == AxeState.Returning)
                {
                    trailColor = Color.Lerp(Color.Red, Color.Orange, progress) * (progress * 0.6f);
                }
                else
                {
                    trailColor = Color.Lerp(Color.DarkOrange, Color.Gold, progress) * (progress * 0.4f);
                }
                float scale = progress * 0.8f;
                float rotation = i < trailRotations.Count ? trailRotations[i] : Projectile.rotation;
                spriteBatch.Draw(texture, trailPositions[i] - Main.screenPosition, null, trailColor, rotation, origin, Projectile.scale * scale, SpriteEffects.None, 0);
            }
        }
        private void DrawRunes(SpriteBatch spriteBatch)
        {
            int runeCount = State == AxeState.Returning ? 6 : 4;
            float runeDistance = 35f * runeScale;
            for (int i = 0; i < runeCount; i++)
            {
                float angle = runeRotation + MathHelper.TwoPi * i / runeCount;
                Vector2 runePos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * runeDistance;
                float pulse = (float)Math.Sin(lifeTimer * 0.15f + i) * 0.3f + 0.7f;
                Color runeColor = Color.Gold * pulse * (State == AxeState.Returning ? 0.8f : 0.5f);
                float runeSize = 4f * runeScale;
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                Rectangle runeRect = new Rectangle(0, 0, 1, 1);
                spriteBatch.Draw(pixel, runePos - Main.screenPosition, runeRect, runeColor, angle, new Vector2(0.5f, 0.5f), runeSize, SpriteEffects.None, 0);
                spriteBatch.Draw(pixel, runePos - Main.screenPosition, runeRect, runeColor * 0.3f, angle, new Vector2(0.5f, 0.5f), runeSize * 2f, SpriteEffects.None, 0);
            }
        }
        private void DrawEnergyRings(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            foreach (var ring in energyRings)
            {
                int segments = 40;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = MathHelper.TwoPi * i / segments;
                    float angle2 = MathHelper.TwoPi * (i + 1) / segments;
                    Vector2 pos1 = Projectile.Center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * ring.Radius;
                    Vector2 pos2 = Projectile.Center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * ring.Radius;
                    Vector2 direction = pos2 - pos1;
                    float lineLength = direction.Length();
                    float lineRotation = direction.ToRotation();
                    Color ringColor = ring.RingColor * ring.Alpha;
                    spriteBatch.Draw(pixel, pos1 - Main.screenPosition, new Rectangle(0, 0, 1, 1), ringColor, lineRotation, Vector2.Zero, new Vector2(lineLength, 2f), SpriteEffects.None, 0);
                }
            }
        }
        private void DrawLightning(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            foreach (var bolt in lightningBolts)
            {
                for (int i = 0; i < bolt.Points.Count - 1; i++)
                {
                    Vector2 start = bolt.Points[i];
                    Vector2 end = bolt.Points[i + 1];
                    Vector2 direction = end - start;
                    float length = direction.Length();
                    float rotation = direction.ToRotation();
                    Color boltColor = bolt.BoltColor * bolt.Alpha;
                    spriteBatch.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), boltColor, rotation, Vector2.Zero, new Vector2(length, 2f), SpriteEffects.None, 0);
                    spriteBatch.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), boltColor * 0.3f, rotation, Vector2.Zero, new Vector2(length, 6f), SpriteEffects.None, 0);
                }
            }
        }
        private void DrawGlow(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin, Color lightColor)
        {
            float pulse = (float)Math.Sin(lifeTimer * 0.12f) * 0.3f + 0.7f;
            float glowIntensity = State == AxeState.Returning ? 0.6f : 0.3f;
            Color glowColor = Color.Orange * glowIntensity * pulse;
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + lifeTimer * 0.02f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0);
            }
            if (State == AxeState.Returning)
            {
                Color returnGlow = Color.Red * 0.4f * pulse;
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f - lifeTimer * 0.03f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                    spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, returnGlow, Projectile.rotation, origin, Projectile.scale * 1.25f, SpriteEffects.None, 0);
                }
            }
        }
        private void DrawMainSprite(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin, Color lightColor)
        {
            Color drawColor = Color.Lerp(lightColor, Color.White, 0.4f);
            float brightPulse = (float)Math.Sin(lifeTimer * 0.1f) * 0.1f + 0.9f;
            drawColor *= brightPulse;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }
        #endregion
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }
    }
    public class AxePlayer : ModPlayer
    {
        private bool wasUsingAxe = false;
        private int axeItemType = -1;
        public override void PreUpdate()
        {
            axeItemType = ModContent.ItemType<Antique_melee_axe>();
        }
        public override void PostUpdate()
        {
            bool isUsingAxe = Player.HeldItem.type == axeItemType && Player.controlUseItem;
            if (wasUsingAxe && !isUsingAxe)
            {
                RecallAllAxes();
            }
            wasUsingAxe = isUsingAxe;
        }
        private void RecallAllAxes()
        {
            int axeProjType = ModContent.ProjectileType<Antique_melee_axe_proj>();
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Player.whoAmI && proj.type == axeProjType)
                {
                    if (proj.ModProjectile is Antique_melee_axe_proj axeProj)
                    {
                        if (proj.owner == Main.myPlayer) axeProj.StartReturning();
                        count++;
                    }
                }
            }
            if (count > 0)
            {
                float volume = 0.7f + count * 0.1f;
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = volume, Pitch = 0.2f }, Player.position);
                for (int i = 0; i < 15; i++)
                {
                    Dust recall = Dust.NewDustDirect(Player.Center, 0, 0, DustID.GoldFlame, 0, 0, 100, default, 1.5f);
                    recall.noGravity = true;
                    recall.velocity = Main.rand.NextVector2Circular(4f, 4f);
                }
            }
        }
    }
}
