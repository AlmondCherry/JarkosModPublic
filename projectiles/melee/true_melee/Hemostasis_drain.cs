using System;
using System.Collections.Generic;
using jarkos.items.weapons.melee.true_melee;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class SecondaryThread {
        public int NpcIndex;
        public Vector2 CurrentEndPoint;
        public List<Vector2> BeamPoints = new List<Vector2>();
        public SecondaryThread(int npcIndex, Vector2 startPoint) {
            this.NpcIndex = npcIndex;
            this.CurrentEndPoint = startPoint;
        }
    }

	public class Hemostasis_drain : ModProjectile {
		private List<Vector2> beamPoints = new List<Vector2>();
        private List<SecondaryThread> secondaryThreads = new List<SecondaryThread>();
        private int secondaryScanTimer = 0;
        
        private float timer = 0f;
        private float maxTime = 300f;
        private bool exploded = false;
        private float wavePhase = 0f;
        private bool hasTarget => lockedTargetIndex >= 0;
        private int lockedTargetIndex
        {
            get => (int)Projectile.ai[0] - 1;
            set => Projectile.ai[0] = value + 1;
        }
        private int oldLockedTargetIndex = -1;
        private Vector2 currentEndPoint;
        private Vector2 targetEndPoint;
        private float transitionSpeed = 0.12f;
        private float[] noiseOffsets;
        private const int NOISE_LAYERS = 4;
        private float enemyLockTimer = 0f;
        
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Magic;
            noiseOffsets = new float[NOISE_LAYERS];
            Random rand = new Random();
            for (int i = 0; i < NOISE_LAYERS; i++)
            {
                noiseOffsets[i] = (float)(rand.NextDouble() * 1000);
            }
        }
        
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.channel || player.dead || !player.active)
            {
                if (Main.netMode != NetmodeID.Server)
                    SpawnDissipateEffect(player.Center, currentEndPoint);
                Projectile.Kill();
                return;
            }
            Projectile.Center = player.Center;
            Projectile.timeLeft = 60;
            timer++;
            wavePhase += 0.15f;
            
            Vector2 mouseWorld = player.Center;
            if (Projectile.owner == Main.myPlayer)
            {
                mouseWorld = Main.MouseWorld;
                Vector2 diff = mouseWorld - player.Center;
                if ((Projectile.velocity - diff).Length() > 10f || timer % 15 == 0)
                {
                    Projectile.velocity = diff;
                    Projectile.netUpdate = true;
                }
                
                NPC foundTarget = FindClosestNPCToCursor(player, mouseWorld);
                int newTargetIndex = foundTarget != null ? foundTarget.whoAmI : -1;
                
                if (lockedTargetIndex != newTargetIndex)
                {
                    lockedTargetIndex = newTargetIndex;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                mouseWorld = player.Center + Projectile.velocity;
            }

            if (lockedTargetIndex != oldLockedTargetIndex)
            {
                if (lockedTargetIndex >= 0)
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        SpawnLockOnEffect(Main.npc[lockedTargetIndex]);
                        SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.4f, Pitch = 0.8f }, Main.npc[lockedTargetIndex].Center);
                    }
                }
                else
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        SpawnUnlockEffect(currentEndPoint);
                        SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.3f, Pitch = 0.5f }, currentEndPoint);
                    }
                    secondaryThreads.Clear();
                }
                oldLockedTargetIndex = lockedTargetIndex;
            }
            
            if (hasTarget)
            {
                if (lockedTargetIndex < 0 || lockedTargetIndex >= Main.maxNPCs || !Main.npc[lockedTargetIndex].active || Main.npc[lockedTargetIndex].life <= 0)
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        lockedTargetIndex = -1;
                        Projectile.netUpdate = true;
                    }
                }
                else
                {
                    targetEndPoint = Main.npc[lockedTargetIndex].Center;
                }
            }
            else
            {
                targetEndPoint = mouseWorld;
                enemyLockTimer = Math.Max(0, enemyLockTimer - 2f);
            }
            
            if (timer <= 1)
            {
                currentEndPoint = targetEndPoint;
            }
            else
            {
                float lerpSpeed = hasTarget ? transitionSpeed : 0.2f;
                currentEndPoint = Vector2.Lerp(currentEndPoint, targetEndPoint, lerpSpeed);
            }
            
            player.ChangeDir(currentEndPoint.X > player.Center.X ? 1 : -1);
            player.itemAnimation = 10;
            player.itemTime = 10;
            player.itemRotation = (currentEndPoint - player.Center).ToRotation();
            if (player.direction < 0)
                player.itemRotation += MathHelper.Pi;
                
            if (hasTarget)
            {
                enemyLockTimer++;
            }
            
            float currentLockProgress = hasTarget ? (enemyLockTimer / maxTime) : (timer / (maxTime * 3f));
            if (Main.netMode != NetmodeID.Server)
            {
                GenerateBeamPoints(player.Center, currentEndPoint, beamPoints, currentLockProgress);
                SpawnBeamDust(beamPoints, currentLockProgress);
                SpawnPlayerDust(player);
            }
            
            if (hasTarget && lockedTargetIndex >= 0 && lockedTargetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[lockedTargetIndex];
                
                secondaryScanTimer++;
                if (secondaryScanTimer > 10 && secondaryThreads.Count < 8)
                {
                    secondaryScanTimer = 0;
                    NPC newSecondary = FindClosestSecondaryTarget(target.Center);
                    if (newSecondary != null)
                    {
                        secondaryThreads.Add(new SecondaryThread(newSecondary.whoAmI, target.Center));
                        if (Main.netMode != NetmodeID.Server)
                            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.2f, Pitch = 1.2f }, target.Center);
                    }
                }
                
                maxTime = 75f + secondaryThreads.Count * 10f;
                float lockProgress = enemyLockTimer / maxTime;
                
                if (Main.netMode != NetmodeID.Server)
                    SpawnTargetDust(target, lockProgress);
                
                if (enemyLockTimer % 15 == 0 && enemyLockTimer > 0)
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        target.life -= 2;
                        if (Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, lockedTargetIndex, 2f);
                    }
                    if (Main.netMode != NetmodeID.Server)
                        target.HitEffect(0, 2);
                }
                
                for (int i = secondaryThreads.Count - 1; i >= 0; i--)
                {
                    var st = secondaryThreads[i];
                    if (st.NpcIndex < 0 || st.NpcIndex >= Main.maxNPCs || !Main.npc[st.NpcIndex].active || Main.npc[st.NpcIndex].life <= 0)
                    {
                        secondaryThreads.RemoveAt(i);
                        continue;
                    }
                    NPC sTarget = Main.npc[st.NpcIndex];
                    st.CurrentEndPoint = Vector2.Lerp(st.CurrentEndPoint, sTarget.Center, transitionSpeed);
                    
                    if (Main.netMode != NetmodeID.Server)
                    {
                        GenerateBeamPoints(target.Center, st.CurrentEndPoint, st.BeamPoints, lockProgress, true);
                        SpawnBeamDust(st.BeamPoints, lockProgress, true);
                        SpawnTargetDust(sTarget, lockProgress);
                    }
                    
                    if (enemyLockTimer % 15 == 0 && enemyLockTimer > 0)
                    {
                        if (Projectile.owner == Main.myPlayer)
                        {
                            sTarget.life -= 2;
                            if (Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, st.NpcIndex, 2f);
                        }
                        if (Main.netMode != NetmodeID.Server)
                            sTarget.HitEffect(0, 2);
                    }
                }

                if (enemyLockTimer >= maxTime && !exploded)
                {
                    exploded = true;
                    int totalHeal = Main.rand.Next(1, 3);
                    BloodExplosion(target);
                    
                    foreach (var st in secondaryThreads)
                    {
                        if (Main.npc[st.NpcIndex].active)
                        {
                            BloodExplosion(Main.npc[st.NpcIndex]);
                            totalHeal += Main.rand.Next(1, 3);
                        }
                    }
                    
                    if (Projectile.owner == Main.myPlayer)
                    {
                        player.statLife += totalHeal;
                        if (player.statLife > player.statLifeMax2) player.statLife = player.statLifeMax2;
                        player.HealEffect(totalHeal);

                        int manaCost = Main.rand.Next(20, 41);
                        player.statMana = Math.Max(0, player.statMana - manaCost);
                        player.manaRegenDelay = 180;
                    }
                    
                    Projectile.Kill();
                    return;
                }
            }
            if (!hasTarget && Main.netMode != NetmodeID.Server)
            {
                SpawnFreeBeamEndDust();
            }
        }
        
        private NPC FindClosestNPCToCursor(Player player, Vector2 mouseWorld)
        {
            float searchRadius = 250f;
            float closestDist = searchRadius;
            NPC closest = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float distToCursor = Vector2.Distance(mouseWorld, npc.Center);
                    float distToPlayer = Vector2.Distance(player.Center, npc.Center);
                    if (distToCursor < closestDist && distToPlayer < 1500f)
                    {
                        closestDist = distToCursor;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        private NPC FindClosestSecondaryTarget(Vector2 origin)
        {
            float searchRadius = 400f;
            float closestDist = searchRadius;
            NPC closest = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && npc.whoAmI != lockedTargetIndex)
                {
                    
                    bool alreadyTargeted = false;
                    foreach (var st in secondaryThreads) {
                        if (st.NpcIndex == npc.whoAmI) {
                            alreadyTargeted = true;
                            break;
                        }
                    }
                    if (alreadyTargeted) continue;

                    float dist = Vector2.Distance(origin, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        private void GenerateBeamPoints(Vector2 start, Vector2 end, List<Vector2> pointsList, float lockProgress, bool isSecondary = false)
        {
            pointsList.Clear();
            Vector2 direction = end - start;
            float length = direction.Length();
            if (length < 5f)
            {
                pointsList.Add(start);
                pointsList.Add(end);
                return;
            }
            Vector2 normalized = direction / length;
            Vector2 perpendicular = new Vector2(-normalized.Y, normalized.X);
            int segments = Math.Max(isSecondary ? 15 : 30, (int)(length / 8f));
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(start, end, t);
                float wave = 0f;
                wave += (float)Math.Sin(t * 12f + wavePhase) * 14f;
                wave += (float)Math.Sin(t * 20f - wavePhase * 1.3f) * 8f;
                wave += (float)Math.Sin(t * 35f + wavePhase * 2.5f) * 4f;
                wave += PerlinNoise1D(t * 5f + wavePhase * 0.5f + noiseOffsets[0]) * 10f;
                if (isSecondary) wave *= 0.6f;
                float edgeFade = (float)Math.Sin(t * MathHelper.Pi);
                edgeFade = (float)Math.Pow(edgeFade, 0.5f);
                wave *= edgeFade;
                float intensity = MathHelper.Lerp(0.5f, 1.5f, Math.Min(lockProgress, 1f));
                wave *= intensity;
                float pulse = 1f + (float)Math.Sin(wavePhase * 3f) * 0.15f;
                wave *= pulse;
                if (!hasTarget && !isSecondary)
                {
                    wave *= 0.6f;
                }
                Vector2 point = basePos + perpendicular * wave;
                pointsList.Add(point);
            }
        }
        
        private float PerlinNoise1D(float x)
        {
            int xi = (int)Math.Floor(x);
            float xf = x - xi;
            float u = xf * xf * (3f - 2f * xf);
            float a = SimpleHash(xi);
            float b = SimpleHash(xi + 1);
            return MathHelper.Lerp(a, b, u);
        }
        
        private float SimpleHash(int x)
        {
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = (x >> 16) ^ x;
            return (x & 0xFFFF) / 32768f - 1f;
        }
        
        private void SpawnBeamDust(List<Vector2> pointsList, float progress, bool isSecondary = false)
        {
            if (pointsList.Count < 2) return;
            int particleCount = (int)MathHelper.Lerp(2, isSecondary ? 6 : 12, progress);
            for (int i = 0; i < particleCount; i++)
            {
                int index = Main.rand.Next(pointsList.Count);
                Vector2 pos = pointsList[index];
                Dust dust = Dust.NewDustPerfect(pos, DustID.Blood, Main.rand.NextVector2Circular(1.5f, 1.5f) * (0.5f + progress), 0, default, Main.rand.NextFloat(0.8f, 1.5f) * (isSecondary ? 0.7f : 1f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
                if (Main.rand.NextBool(3))
                {
                    Dust glow = Dust.NewDustPerfect(pos, DustID.RedTorch, Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, Main.rand.NextFloat(1f, 2f) * (isSecondary ? 0.7f : 1f));
                    glow.noGravity = true;
                    glow.noLight = false;
                }
            }
            if (hasTarget && progress > 0.3f && Main.rand.NextBool(isSecondary ? 4 : 2))
            {
                int idx = Main.rand.Next(pointsList.Count);
                Vector2 pos = pointsList[idx];
                Dust magic = Dust.NewDustPerfect(pos, DustID.GemRuby, Main.rand.NextVector2Circular(2f, 2f), 100, default, Main.rand.NextFloat(0.6f, 1.2f));
                magic.noGravity = true;
            }
            if (hasTarget && progress > 0.6f)
            {
                for (int i = 0; i < (int)(progress * (isSecondary ? 3 : 5)); i++)
                {
                    int idx = Main.rand.Next(pointsList.Count);
                    Vector2 pos = pointsList[idx];
                    Dust dark = Dust.NewDustPerfect(pos, DustID.Shadowflame, Main.rand.NextVector2Circular(1f, 1f), 0, default, Main.rand.NextFloat(0.5f, 1f));
                    dark.noGravity = true;
                }
            }
        }
        
        private void SpawnFreeBeamEndDust()
        {
            Vector2 endPos = currentEndPoint;
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(endPos + Main.rand.NextVector2Circular(15f, 15f), DustID.Blood, Main.rand.NextVector2Circular(1f, 1f), 0, default, Main.rand.NextFloat(0.5f, 1f));
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 10f + (float)Math.Sin(wavePhase * 2f) * 5f;
                Vector2 orbPos = endPos + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Dust orb = Dust.NewDustPerfect(orbPos, DustID.RedTorch, Vector2.Zero, 0, default, 0.8f);
                orb.noGravity = true;
            }
            if (timer % 10 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 searchDir = Main.rand.NextVector2Unit() * Main.rand.NextFloat(20f, 50f);
                    Vector2 searchPos = endPos + searchDir;
                    Dust search = Dust.NewDustPerfect(searchPos, DustID.Blood, -searchDir * 0.02f, 0, default, 0.6f);
                    search.noGravity = true;
                    search.fadeIn = 0.3f;
                }
            }
            Lighting.AddLight(endPos, new Vector3(0.6f, 0.05f, 0.05f));
        }
        
        private void SpawnLockOnEffect(NPC target)
        {
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi / 25f * i;
                float radius = 40f;
                Vector2 pos = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 vel = (target.Center - pos).SafeNormalize(Vector2.Zero) * 3f;
                Dust dust = Dust.NewDustPerfect(pos, DustID.Blood, vel, 0, default, 1.5f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 15; i++)
            {
                Dust glow = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.RedTorch, Main.rand.NextVector2Circular(2f, 2f), 0, default, 2f);
                glow.noGravity = true;
            }
        }
        
        private void SpawnUnlockEffect(Vector2 lastPos)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(lastPos + Main.rand.NextVector2Circular(15f, 15f), DustID.Blood, Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(0.6f, 1.2f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }
        
        private void SpawnTargetDust(NPC target, float progress)
        {
            progress = Math.Min(progress, 1f);
            int count = (int)MathHelper.Lerp(1, 8, progress);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(target.width, target.height);
                Vector2 pos = target.Center + offset;
                Dust dust = Dust.NewDustPerfect(pos, DustID.Blood, -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f) * progress, 0, default, Main.rand.NextFloat(1f, 2f) * progress);
                dust.noGravity = true;
            }
            if (progress > 0.4f)
            {
                float ringRadius = MathHelper.Lerp(60f, 20f, progress) + (float)Math.Sin(wavePhase * 4f) * 10f;
                int ringParticles = (int)(8 * progress);
                for (int i = 0; i < ringParticles; i++)
                {
                    float angle = MathHelper.TwoPi / ringParticles * i + wavePhase;
                    Vector2 ringPos = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringRadius;
                    Dust ring = Dust.NewDustPerfect(ringPos, DustID.RedTorch, Vector2.Zero, 0, default, 1.5f * progress);
                    ring.noGravity = true;
                    ring.noLight = false;
                }
            }
            if (progress > 0.8f)
            {
                float shake = (progress - 0.8f) * 10f;
                if (Main.rand.NextBool(3))
                {
                    Main.player[Projectile.owner].velocity += Main.rand.NextVector2Circular(shake, shake) * 0.1f;
                }
            }
            Lighting.AddLight(target.Center, new Vector3(1f, 0.1f, 0.1f) * progress * 2f);
        }
        
        private void SpawnPlayerDust(Player player)
        {
            float progress = hasTarget ? Math.Min(enemyLockTimer / maxTime, 1f) : 0.2f;
            Vector2 handPos = player.Center + new Vector2(player.direction * 15f, -5f);
            if (Main.rand.NextBool(3))
            {
                Dust hand = Dust.NewDustPerfect(handPos + Main.rand.NextVector2Circular(10f, 10f), DustID.Blood, Main.rand.NextVector2Circular(1f, 1f), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                hand.noGravity = true;
            }
            Lighting.AddLight(player.Center, new Vector3(0.8f, 0.05f, 0.05f) * (0.5f + progress * 0.5f));
            if (progress > 0.5f)
            {
                for (int i = 0; i < 2; i++)
                {
                    float spiralAngle = wavePhase * 3f + i * MathHelper.Pi;
                    float spiralRadius = 25f + (float)Math.Sin(wavePhase * 2f) * 5f;
                    Vector2 spiralPos = player.Center + new Vector2((float)Math.Cos(spiralAngle) * spiralRadius, (float)Math.Sin(spiralAngle) * spiralRadius);
                    Dust spiral = Dust.NewDustPerfect(spiralPos, DustID.GemRuby, Vector2.Zero, 100, default, 0.8f);
                    spiral.noGravity = true;
                }
            }
        }
        
        private void SpawnDissipateEffect(Vector2 start, Vector2 end)
        {
            if (beamPoints.Count == 0) return;
            for (int i = 0; i < beamPoints.Count; i += 2)
            {
                Vector2 pos = beamPoints[i];
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(pos, DustID.Blood, vel, 0, default, Main.rand.NextFloat(0.5f, 1.2f));
                dust.noGravity = false;
                dust.fadeIn = 0.3f;
            }
            foreach(var st in secondaryThreads) {
                for (int i = 0; i < st.BeamPoints.Count; i += 2) {
                    Vector2 pos = st.BeamPoints[i];
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust dust = Dust.NewDustPerfect(pos, DustID.Blood, vel, 0, default, Main.rand.NextFloat(0.5f, 1.2f));
                    dust.noGravity = false;
                    dust.fadeIn = 0.3f;
                }
            }
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.5f, Pitch = 0.5f }, start);
        }
        
        private void BloodExplosion(NPC target)
        {
            Vector2 center = target.Center;
            Player player = Main.player[Projectile.owner];

            int explosionDamage = Projectile.damage;
            
            if (Projectile.owner == Main.myPlayer)
            {
                target.SimpleStrikeNPC(explosionDamage, player.direction, true, 0f, DamageClass.Magic, false, player.luck);
                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
            }
            
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 1.2f, Pitch = -0.3f }, center);
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = -0.5f }, center);
                SoundEngine.PlaySound(SoundID.NPCDeath11 with { Volume = 1f }, center);
                for (int i = 0; i < 80; i++)
                {
                    float angle = MathHelper.TwoPi / 80f * i;
                    float speed = Main.rand.NextFloat(5f, 18f);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    Dust blood = Dust.NewDustPerfect(center, DustID.Blood, vel, 0, default, Main.rand.NextFloat(1.5f, 3f));
                    blood.noGravity = Main.rand.NextBool(2);
                    blood.fadeIn = 1f;
                }
                for (int i = 0; i < 50; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(15f, 15f);
                    Dust glow = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(20f, 20f), DustID.RedTorch, vel, 0, default, Main.rand.NextFloat(2f, 4f));
                    glow.noGravity = true;
                    glow.noLight = false;
                }
                for (int i = 0; i < 30; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                    Dust shadow = Dust.NewDustPerfect(center, DustID.Shadowflame, vel, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                    shadow.noGravity = true;
                }
                for (int i = 0; i < 40; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(12f, 16f);
                    vel.Y -= Main.rand.NextFloat(3f, 8f);
                    int dustIdx = Dust.NewDust(center - new Vector2(5, 5), 10, 10, DustID.Blood, vel.X, vel.Y, 0, default, Main.rand.NextFloat(1f, 2f));
                    Main.dust[dustIdx].noGravity = false;
                }
                for (int ring = 0; ring < 3; ring++)
                {
                    float radius = 30f + ring * 40f;
                    int ringCount = 20 + ring * 10;
                    for (int i = 0; i < ringCount; i++)
                    {
                        float angle = MathHelper.TwoPi / ringCount * i;
                        Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                        Vector2 vel = (pos - center).SafeNormalize(Vector2.Zero) * (3f + ring * 2f);
                        Dust ringDust = Dust.NewDustPerfect(pos, DustID.Blood, vel, 0, default, Main.rand.NextFloat(1f, 2f));
                        ringDust.noGravity = true;
                    }
                }
                for (int i = 0; i < 25; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust gem = Dust.NewDustPerfect(center, DustID.GemRuby, vel, 100, default, Main.rand.NextFloat(1f, 2f));
                    gem.noGravity = true;
                }
                for (int i = 0; i < 10; i++)
                {
                    int goreType = Main.rand.Next(3) switch
                    {
                        0 => GoreID.Smoke1, 1 => GoreID.Smoke2, _ => GoreID.Smoke3
                    };
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    Gore gore = Gore.NewGorePerfect(Projectile.GetSource_Death(), center, vel, goreType);
                    if (gore != null) gore.timeLeft = 30;
                }
                Lighting.AddLight(center, new Vector3(2f, 0.2f, 0.2f));
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (p.active && !p.dead)
                    {
                        float dist = Vector2.Distance(p.Center, center);
                        if (dist < 800f)
                        {
                            float shakePower = MathHelper.Lerp(8f, 0f, dist / 800f);
                            p.velocity += Main.rand.NextVector2Circular(shakePower, shakePower);
                        }
                    }
                }
            }
            float aoERadius = 200f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && npc.whoAmI != target.whoAmI)
                {
                    float dist = Vector2.Distance(npc.Center, center);
                    if (dist < aoERadius)
                    {
                        float damageMult = 1f - (dist / aoERadius);
                        int aoeDmg = (int)(explosionDamage * 0.5f * damageMult);
                        if (Projectile.owner == Main.myPlayer)
                        {
                            npc.SimpleStrikeNPC(aoeDmg, player.direction, true, 5f, DamageClass.Magic, false, player.luck);
                            if (Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                        }
                        if (Main.netMode != NetmodeID.Server)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                Vector2 trailPos = Vector2.Lerp(center, npc.Center, j / 10f);
                                Dust trail = Dust.NewDustPerfect(trailPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Blood, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.5f);
                                trail.noGravity = true;
                            }
                        }
                    }
                }
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float progress = hasTarget ? Math.Min(enemyLockTimer / maxTime, 1f) : 0.15f;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float colorIntensity = hasTarget ? 1f : 0.6f;
            Color outerColor = new Color(50, 0, 0, 100) * (0.3f + progress * 0.2f) * colorIntensity;
            Color midColor = new Color(120, 0, 10, 150) * (0.5f + progress * 0.3f) * colorIntensity;
            Color mainColor = new Color(200, 20, 20, 200) * (0.7f + progress * 0.3f) * colorIntensity;
            Color brightColor = new Color(255, 100, 100, 230) * colorIntensity;
            Color coreColor = new Color(255, 200, 200, 255) * colorIntensity;
            if (!hasTarget)
            {
                outerColor = new Color(40, 0, 10, 80) * 0.4f;
                midColor = new Color(100, 10, 20, 120) * 0.5f;
                mainColor = new Color(180, 30, 40, 160) * 0.6f;
                brightColor = new Color(230, 80, 90, 200);
                coreColor = new Color(255, 180, 180, 220);
            }
            
            if (beamPoints.Count >= 2) {
                DrawBeamLayer(sb, pixel, beamPoints, 12f + progress * 6f, outerColor);
                DrawBeamLayer(sb, pixel, beamPoints, 8f + progress * 4f, midColor);
                DrawBeamLayer(sb, pixel, beamPoints, 4f + progress * 2f, mainColor);
                DrawBeamLayer(sb, pixel, beamPoints, 2f + progress * 1f, brightColor);
                DrawBeamLayer(sb, pixel, beamPoints, 1f, coreColor);
            }

            foreach (var st in secondaryThreads) {
                if (st.BeamPoints.Count >= 2) {
                    DrawBeamLayer(sb, pixel, st.BeamPoints, (12f + progress * 6f) * 0.6f, outerColor);
                    DrawBeamLayer(sb, pixel, st.BeamPoints, (8f + progress * 4f) * 0.6f, midColor);
                    DrawBeamLayer(sb, pixel, st.BeamPoints, (4f + progress * 2f) * 0.6f, mainColor);
                    DrawBeamLayer(sb, pixel, st.BeamPoints, (2f + progress * 1f) * 0.6f, brightColor);
                    DrawBeamLayer(sb, pixel, st.BeamPoints, 1f, coreColor);
                }
            }

            if (hasTarget)
            {
                DrawLockIndicator(sb, pixel, progress);
            }
            else
            {
                DrawSearchIndicator(sb, pixel);
            }
            return false;
        }
        
        private void DrawBeamLayer(SpriteBatch sb, Texture2D pixel, List<Vector2> points, float width, Color color)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 start = points[i] - Main.screenPosition;
                Vector2 end = points[i + 1] - Main.screenPosition;
                Vector2 diff = end - start;
                float length = diff.Length();
                float rotation = diff.ToRotation();
                sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
            }
        }
        
        private void DrawLockIndicator(SpriteBatch sb, Texture2D pixel, float progress)
        {
            Vector2 pos = currentEndPoint - Main.screenPosition;
            float crossSize = MathHelper.Lerp(20f, 8f, progress);
            float rotation = wavePhase * 0.5f;
            Color crossColor = new Color(255, 30, 30, 200) * (0.6f + progress * 0.4f);
            for (int i = 0; i < 4; i++)
            {
                float angle = rotation + MathHelper.PiOver2 * i;
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 lineStart = pos + dir * (crossSize * 0.4f);
                Vector2 lineEnd = pos + dir * crossSize;
                Vector2 diff = lineEnd - lineStart;
                float len = diff.Length();
                sb.Draw(pixel, lineStart, new Rectangle(0, 0, 1, 1), crossColor, diff.ToRotation(), new Vector2(0, 0.5f), new Vector2(len, 2f), SpriteEffects.None, 0f);
            }
            if (progress > 0f)
            {
                int segments = 32;
                int filledSegments = (int)(segments * progress);
                float ringRadius = crossSize + 5f;
                for (int i = 0; i < filledSegments; i++)
                {
                    float angle1 = MathHelper.TwoPi / segments * i + rotation;
                    float angle2 = MathHelper.TwoPi / segments * (i + 1) + rotation;
                    Vector2 p1 = pos + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * ringRadius;
                    Vector2 p2 = pos + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * ringRadius;
                    Vector2 diff = p2 - p1;
                    float len = diff.Length();
                    Color segColor = Color.Lerp(new Color(200, 50, 50, 180), new Color(255, 0, 0, 255), progress);
                    sb.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), segColor, diff.ToRotation(), new Vector2(0, 0.5f), new Vector2(len, 2.5f), SpriteEffects.None, 0f);
                }
            }
        }
        
        private void DrawSearchIndicator(SpriteBatch sb, Texture2D pixel)
        {
            Vector2 pos = currentEndPoint - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(wavePhase * 2f) * 0.3f;
            float radius = 12f * pulse;
            Color searchColor = new Color(180, 40, 40, 80);
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathHelper.TwoPi / segments * i;
                float angle2 = MathHelper.TwoPi / segments * (i + 1);
                Vector2 p1 = pos + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
                Vector2 p2 = pos + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;
                Vector2 diff = p2 - p1;
                float len = diff.Length();
                sb.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), searchColor, diff.ToRotation(), new Vector2(0, 0.5f), new Vector2(len, 1.5f), SpriteEffects.None, 0f);
            }
            sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(255, 100, 100, 150), 0f, new Vector2(0.5f, 0.5f), 4f * pulse, SpriteEffects.None, 0f);
        }
        
        public override bool? CanDamage()
        {
            return false;
        }
	}
}
