using System;
using System.Collections.Generic;
using jarkos.buffs.summonbuff;
using jarkos.items.accs.general;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
    public class LevitatingBlade_proj : ModProjectile
    {
        private enum BladeState : byte
        {
            Idle, Approach, WindUp, Attack, Recover
        }
        private enum AttackKind : byte
        {
            Lunge, Slash, Spin
        }
        private const float SearchRange = 720f;
        private const float TargetLockRange = 1700f;
        private const float WindUpStandoff = 140f;
        private const float LungeOvershoot = 60f;
        private const float OrbitRadius = 36f;
        private const float OrbitHeight = 64f;
        private const float IdleSpinSpeed = 0.045f;
        private const float ApproachAccel = 0.85f;
        private const float ApproachMaxSpeed = 18f;
        private const float ApproachSnap = 24f;
        private const int   WindUpTime = 14;
        private const float WindUpRetreat = 36f;
        private const int   LungeTime = 10;
        private const float LungeDistanceMin = 220f;
        private const int   SlashTime = 12;
        private const float SlashSweep = MathHelper.Pi;
        private const int   SpinTime = 16;
        private const float SpinSweep = MathHelper.TwoPi * 2f;
        private const float SpinRadius = 70f;
        private const int   RecoverTime = 14;
        private const int   MaxTrailSamples = 28;
        private const float TrailDecayPerFrame = 1f / 9f;
        private BladeState State
        {
            get => (BladeState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        private float StateTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private int TargetWhoAmI
        {
            get => (int)Projectile.localAI[0] - 1;
            set => Projectile.localAI[0] = value + 1;
        }
        private AttackKind CurrentAttack
        {
            get => (AttackKind)(int)Projectile.localAI[1];
            set => Projectile.localAI[1] = (float)value;
        }
        private int AttackCounter
        {
            get => (int)Projectile.localAI[2];
            set => Projectile.localAI[2] = value;
        }
        private Vector2 _lungeStart;
        private Vector2 _lungeEnd;
        private float _lungeDir;
        private Vector2 _orbitCenter;
        private float _orbitStartAngle;
        private float _orbitEndAngle;
        private float _orbitRadius;
        private float _orbitDir;
        private Vector2 _targetCenterAtSetup;
        private float _idleAngle;
        private float _spinSpeed;
        private readonly List<TrailSample> _trail = new(MaxTrailSamples);
        private Vector2 _lastTipOffset;
        private bool _hasLastTip;
        private struct TrailSample
        {
            public Vector2 Tip;
            public float Time;
            public float Speed;
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            ProjectileID.Sets.MinionSacrificable[Type] = false;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override bool MinionContactDamage() => false;
        public override bool? CanCutTiles() => false;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player == null || !player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            var mp = player.GetModPlayer<LevitatingBladePlayer>();
            if (mp.hasLevitatingBlade)
                Projectile.timeLeft = 2;

            Lighting.AddLight(Projectile.Center, 0.10f, 0.70f, 0.25f);
            ValidateTarget(player);

            switch (State)
            {
                case BladeState.Idle:
                    UpdateIdle(player); 
                    break;
                case BladeState.Approach:
                    UpdateApproach(player);
                    break;
                case BladeState.WindUp:
                    UpdateWindUp(player);  
                    break;
                case BladeState.Attack:
                    UpdateAttack(player);
                    break;
                case BladeState.Recover:
                    UpdateRecover(player);
                    break;
            }
            if (State == BladeState.Idle || State == BladeState.Approach)
            {
                _spinSpeed = MathHelper.Lerp(_spinSpeed, IdleSpinSpeed, 0.10f);
                Projectile.rotation += _spinSpeed;
            }
            UpdateTrail();
        }
        private void UpdateIdle(Player player)
        {
            _idleAngle += 0.028f;

            float bob = MathF.Sin(Main.GameUpdateCount * 0.05f) * 6f;
            Vector2 anchor = player.Center + new Vector2(0f, -OrbitHeight + bob);
            Vector2 orbit  = new Vector2(MathF.Cos(_idleAngle) * OrbitRadius, MathF.Sin(_idleAngle * 1.3f) * (OrbitRadius * 0.45f));
            Vector2 desired = anchor + orbit;

            Projectile.Center = Vector2.Lerp(Projectile.Center, desired, 0.18f);
            Projectile.velocity = Vector2.Zero;
            NPC found = FindNearestTarget(player.Center, SearchRange);

            if (found != null)
                EnterApproach(found);
        }
        private void UpdateApproach(Player player)
        {
            NPC target = GetTargetNPC();
            if (target == null)
            {
                EnterIdle();
                return;
            }

            Vector2 pose = ComputeWindUpPose(target.Center, player.Center);
            Vector2 delta = pose - Projectile.Center;
            float dist = delta.Length();
            if (dist < ApproachSnap)
            {
                EnterWindUp(player, target);
                return;
            }

            Vector2 dir = delta / MathF.Max(dist, 0.001f);
            Vector2 want = dir * ApproachMaxSpeed;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, want, ApproachAccel * 0.1f);
            Projectile.Center += Projectile.velocity;
            float lookAngle = Projectile.velocity.LengthSquared() > 4f ? Projectile.velocity.ToRotation() : (target.Center - Projectile.Center).ToRotation();

            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, lookAngle + MathHelper.PiOver4, 0.18f);
        }
        private void UpdateWindUp(Player player)
        {
            NPC target = GetTargetNPC();
            if (target == null)
            {
                EnterIdle();
                return;
            }

            StateTimer += 1f;
            float t = MathHelper.Clamp(StateTimer / WindUpTime, 0f, 1f);

            Vector2 toTarget = target.Center - Projectile.Center;
            Vector2 backDir = -Vector2.Normalize(toTarget);
            Vector2 nudge = backDir * (WindUpRetreat / WindUpTime);

            if (t < 0.55f)
                Projectile.Center += nudge;
            else
                Projectile.Center += nudge * 0.15f;
            float spinTarget = MathHelper.Lerp(IdleSpinSpeed, 0.40f, t);
            _spinSpeed = MathHelper.Lerp(_spinSpeed, spinTarget, 0.20f);
            Projectile.rotation += _spinSpeed;

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f), DustID.GreenTorch, default, 120, default, Main.rand.NextFloat(0.7f, 1.1f));
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(1.2f, 1.2f);
            }
            if (StateTimer >= WindUpTime)
            {
                EnterAttack(player, target);
            }
        }
        private void UpdateAttack(Player player)
        {
            switch (CurrentAttack)
            {
                case AttackKind.Lunge: UpdateLungeAttack(player); break;
                case AttackKind.Slash: UpdateSlashAttack(player); break;
                case AttackKind.Spin:  UpdateSpinAttack(player);  break;
            }
        }
        private void UpdateLungeAttack(Player player)
        {
            StateTimer += 1f;

            float t = MathHelper.Clamp(StateTimer / LungeTime, 0f, 1f);
            float k = 1f - MathF.Pow(1f - t, 4f);
            Vector2 pos = Vector2.Lerp(_lungeStart, _lungeEnd, k);

            Projectile.velocity = pos - Projectile.Center;
            Projectile.Center = pos;
            Projectile.rotation = _lungeDir + MathHelper.PiOver4 + MathHelper.PiOver2;

            if (Main.rand.NextBool(1))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center - _lungeDir.ToRotationVector2() * 14f
                    + Main.rand.NextVector2Circular(3f, 3f), DustID.GreenTorch, Main.rand.NextVector2Circular(0.5f, 0.5f), 100, default, Main.rand.NextFloat(0.9f, 1.4f));
                d.noGravity = true;
            }
            if (StateTimer >= LungeTime)
                EnterRecover();
        }
        private void UpdateSlashAttack(Player player)
        {
            StateTimer += 1f;

            float t = MathHelper.Clamp(StateTimer / SlashTime, 0f, 1f);
            float k = 1f - MathF.Pow(1f - t, 4f);
            float angle = MathHelper.Lerp(_orbitStartAngle, _orbitEndAngle, k);
            Vector2 effectiveCenter = _orbitCenter;
            NPC target = GetTargetNPC();
            if (target != null)
                effectiveCenter += target.Center - _targetCenterAtSetup;

            Vector2 dir = angle.ToRotationVector2();
            Projectile.Center = effectiveCenter + dir * _orbitRadius;
            float tangent = angle + MathHelper.PiOver2 * _orbitDir;

            _lungeDir = tangent;
            Projectile.rotation = tangent + MathHelper.PiOver4 + MathHelper.PiOver2;
            Projectile.velocity = tangent.ToRotationVector2() * 8f;
            if (StateTimer >= SlashTime)
                EnterRecover();
        }
        private void UpdateSpinAttack(Player player)
        {
            StateTimer += 1f;

            float t = MathHelper.Clamp(StateTimer / SpinTime, 0f, 1f);
            float k = 1f - MathF.Pow(1f - t, 4f);
            float angle = MathHelper.Lerp(_orbitStartAngle, _orbitEndAngle, k);
            float r = _orbitRadius;

            Vector2 effectiveCenter = _orbitCenter;
            NPC target = GetTargetNPC();
            if (target != null)
                effectiveCenter += target.Center - _targetCenterAtSetup;

            Vector2 dir = angle.ToRotationVector2();
            Projectile.Center = effectiveCenter + dir * r;
            _lungeDir = angle;

            Projectile.rotation = angle + MathHelper.PiOver4 + MathHelper.PiOver2;
            Projectile.velocity = (angle + MathHelper.PiOver2 * _orbitDir).ToRotationVector2() * 10f;
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + dir * 12f, DustID.GreenTorch, Main.rand.NextVector2Circular(2f, 2f), 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                d.noGravity = true;
            }
            if (StateTimer >= SpinTime)
                EnterRecover();
        }
        private void UpdateRecover(Player player)
        {
            StateTimer += 1f;

            Projectile.velocity *= 0.78f;
            Projectile.Center += Projectile.velocity * 0.6f;
            _spinSpeed = MathHelper.Lerp(_spinSpeed, IdleSpinSpeed * 1.6f, 0.10f);
            Projectile.rotation += _spinSpeed;

            if (StateTimer >= RecoverTime)
            {
                Projectile.localNPCHitCooldown = 30;
                NPC target = GetTargetNPC();
                if (target != null)
                {
                    EnterApproach(target);
                }
                else
                {
                    NPC next = FindNearestTarget(player.Center, SearchRange);
                    if (next != null)
                        EnterApproach(next);
                    else
                        EnterIdle();
                }
            }
        }
        private void EnterIdle()
        {
            State = BladeState.Idle;
            StateTimer = 0f;
            TargetWhoAmI = -1;
            Projectile.velocity = Vector2.Zero;
        }
        private void EnterApproach(NPC target)
        {
            State = BladeState.Approach;
            StateTimer = 0f;
            TargetWhoAmI = target.whoAmI;
        }
        private void EnterWindUp(Player player, NPC target)
        {
            State = BladeState.WindUp;
            StateTimer = 0f;
            Projectile.velocity *= 0.25f;
            SoundEngine.PlaySound(SoundID.Item9 with { Pitch = 0.4f, Volume = 0.45f }, Projectile.Center);
        }
        private void EnterAttack(Player player, NPC target)
        {
            State = BladeState.Attack;
            StateTimer = 0f;
            CurrentAttack = (AttackKind)(((AttackCounter % 3) + 3) % 3);
            AttackCounter++;
            switch (CurrentAttack)
            {
                case AttackKind.Lunge:
                    SetupLungeAttack(target);
                    break;
                case AttackKind.Slash:
                    SetupSlashAttack(target);
                    break;
                case AttackKind.Spin:
                    SetupSpinAttack(target);
                    break;
            }
            Projectile.ResetLocalNPCHitImmunity();
        }
        private void SetupLungeAttack(NPC target)
        {
            _lungeStart = Projectile.Center;
            Vector2 toTarget = target.Center - Projectile.Center;
            float dist = toTarget.Length();

            if (dist < 1f) { toTarget = Vector2.UnitX; dist = 1f; }

            _lungeDir = toTarget.ToRotation();
            float totalDist = MathF.Max(LungeDistanceMin, dist + LungeOvershoot);
            _lungeEnd = Projectile.Center + _lungeDir.ToRotationVector2() * totalDist;

            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.6f, Volume = 0.55f }, Projectile.Center);
        }
        private void SetupSlashAttack(NPC target)
        {
            Vector2 T = target.Center;
            Vector2 P = Projectile.Center;
            Vector2 PT = T - P;
            float d = PT.Length();

            if (d < 12f)
            {
                _orbitCenter = T;
                _orbitRadius = MathF.Max(d, 24f);
                _orbitStartAngle = (P - T).LengthSquared() > 0.1f ? (P - T).ToRotation() : 0f;
                _orbitDir = (AttackCounter % 2 == 0) ? 1f : -1f;
                _orbitEndAngle = _orbitStartAngle + SlashSweep * _orbitDir;
            }
            else
            {
                Vector2 dir = PT / d;
                Vector2 perp = new Vector2(-dir.Y, dir.X);

                float side = (AttackCounter % 2 == 0) ? 1f : -1f;
                float bulge = d * 0.55f;
                Vector2 midPT = (P + T) * 0.5f;

                _orbitCenter = midPT - perp * (bulge * side);
                _orbitRadius = Vector2.Distance(P, _orbitCenter);

                float startAng = (P - _orbitCenter).ToRotation();
                float tAng = (T - _orbitCenter).ToRotation();
                float halfSweep = MathHelper.WrapAngle(tAng - startAng);

                if (MathF.Abs(halfSweep) < 1e-3f)
                    halfSweep = MathHelper.PiOver2 * side;

                _orbitStartAngle = startAng;
                _orbitDir = MathF.Sign(halfSweep);
                _orbitEndAngle = startAng + halfSweep * 2f;
            }
            _targetCenterAtSetup = T;
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.85f, Volume = 0.45f }, Projectile.Center);
        }
        private void SetupSpinAttack(NPC target)
        {
            Vector2 T = target.Center;
            Vector2 P = Projectile.Center;
            Vector2 PT = T - P;
            float d = PT.Length();

            if (d < 16f)
            {
                _orbitCenter = T;
                _orbitRadius = MathF.Max(d, 24f);
                _orbitStartAngle = (P - T).LengthSquared() > 0.1f ? (P - T).ToRotation() : 0f;
            }
            else
            {
                Vector2 dir = PT / d;
                Vector2 perp = new Vector2(-dir.Y, dir.X);

                float side = (AttackCounter % 2 == 0) ? 1f : -1f;
                float R = MathF.Max(SpinRadius, d * 0.55f);
                float half = d * 0.5f;
                float perpDist = MathF.Sqrt(MathF.Max(0f, R * R - half * half));
                
                Vector2 midPT = (P + T) * 0.5f;
                _orbitCenter = midPT + perp * (perpDist * side);
                _orbitRadius = R;
                _orbitStartAngle = (P - _orbitCenter).ToRotation();
            }

            _orbitDir = (AttackCounter % 2 == 0) ? 1f : -1f;
            _orbitEndAngle = _orbitStartAngle + SpinSweep * _orbitDir;
            _targetCenterAtSetup = T;

            Projectile.localNPCHitCooldown = 6;
            Projectile.ResetLocalNPCHitImmunity();
            SoundEngine.PlaySound(SoundID.Item22 with { Pitch = 0.3f, Volume = 0.55f }, Projectile.Center);
        }
        private void EnterRecover()
        {
            State = BladeState.Recover;
            StateTimer = 0f;
        }
        private NPC GetTargetNPC()
        {
            int idx = TargetWhoAmI;
            if (idx < 0 || idx >= Main.maxNPCs)
                return null;
            NPC npc = Main.npc[idx];
            if (!npc.active || npc.life <= 0)
                return null;
            return npc;
        }
        private void ValidateTarget(Player player)
        {
            if (TargetWhoAmI < 0)
                return;
            NPC npc = GetTargetNPC();
            if (npc == null)
            {
                TargetWhoAmI = -1;
                return;
            }
            float distToPlayer = Vector2.Distance(npc.Center, player.Center);
            if (distToPlayer > TargetLockRange)
            {
                TargetWhoAmI = -1;
            }
        }
        private NPC FindNearestTarget(Vector2 origin, float maxRange)
        {
            NPC best = null;
            float bestDistSq = maxRange * maxRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.life <= 0)
                    continue;
                if (!npc.CanBeChasedBy(this))
                    continue;
                float d = Vector2.DistanceSquared(origin, npc.Center);
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = npc;
                }
            }
            return best;
        }
        private Vector2 ComputeWindUpPose(Vector2 targetCenter, Vector2 playerCenter)
        {
            Vector2 backToPlayer = playerCenter - targetCenter;
            float baseAngle = backToPlayer.LengthSquared() > 1f ? backToPlayer.ToRotation() : 0f;
            float side = (Main.GameUpdateCount / 60 % 2 == 0) ? 1f : -1f;
            float offsetAngle = baseAngle + side * 0.45f;
            return targetCenter + offsetAngle.ToRotationVector2() * WindUpStandoff;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool earlyRecover = State == BladeState.Recover && StateTimer <= 4f;
            if (State != BladeState.Attack && !earlyRecover)
                return false;

            Vector2 dir = _lungeDir.ToRotationVector2();
            float halfLen = CurrentAttack == AttackKind.Spin ? 16f : 22f;
            float width = CurrentAttack == AttackKind.Slash ? 22f : 18f;
            Vector2 start = Projectile.Center - dir * halfLen;
            Vector2 end = Projectile.Center + dir * halfLen;
            float cp = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref cp) ? true : null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            player.AddBuff(ModContent.BuffType<LevitatingBlade_buff>(), 300);
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.5f);
                d.noGravity = true;
                d.velocity *= 2f;
            }
            SoundEngine.PlaySound(SoundID.NPCHit53 with { Pitch = 0.3f, Volume = 0.55f }, target.Center);
        }
        private void UpdateTrail()
        {
            for (int i = _trail.Count - 1; i >= 0; i--)
            {
                TrailSample s = _trail[i];
                s.Time -= TrailDecayPerFrame;
                if (s.Time <= 0f)
                    _trail.RemoveAt(i);
                else
                    _trail[i] = s;
            }
            bool trailActive = State == BladeState.Attack || (State == BladeState.Recover && StateTimer <= RecoverTime * 0.6f);
            if (!trailActive)
            {
                _hasLastTip = false;
                return;
            }
            Player player = Main.player[Projectile.owner];
            Vector2 tipWorld = Projectile.Center + _lungeDir.ToRotationVector2() * 16f;
            Vector2 tipOffset = tipWorld - player.MountedCenter;

            float speed = _hasLastTip ? Vector2.Distance(_lastTipOffset, tipOffset) : 0f;
            _trail.Insert(0, new TrailSample
            {
                Tip = tipOffset, Time = 1f, Speed = speed
            });
            if (_trail.Count > MaxTrailSamples)
                _trail.RemoveAt(_trail.Count - 1);
            _lastTipOffset = tipOffset;
            _hasLastTip = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTipRibbon();
            DrawBlade(lightColor);
            return false;
        }
        private void DrawTipRibbon()
        {
            if (_trail.Count < 2)
                return;
            try
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Texture2D pixel = TextureAssets.MagicPixel.Value;

                Rectangle pixelRect = new Rectangle(0, 0, 1, 1);
                Vector2 pixelOrigin = new Vector2(0f, 0.5f);
                Color inner = new Color(230, 255, 240);
                Color outer = new Color(0, 220, 100);

                const float baseWidth = 1.5f;
                const float maxWidth  = 9f;

                Vector2 anchorWorld = Main.player[Projectile.owner].MountedCenter;
                List<TrailSample> smoothTrail = new List<TrailSample>();
                int subdivisions = 4;

                for (int i = 0; i < _trail.Count - 1; i++)
                {
                    TrailSample p1 = _trail[i];
                    TrailSample p2 = _trail[i + 1];
                    TrailSample p0 = i > 0 ? _trail[i - 1] : new TrailSample { Tip = p1.Tip + (p1.Tip - p2.Tip), Time = p1.Time, Speed = p1.Speed };
                    TrailSample p3 = i < _trail.Count - 2 ? _trail[i + 2] : new TrailSample { Tip = p2.Tip + (p2.Tip - p1.Tip), Time = p2.Time, Speed = p2.Speed };
                    for (int j = 0; j < subdivisions; j++)
                    {
                        float t = (float)j / subdivisions;
                        Vector2 smoothTip = Vector2.CatmullRom(p0.Tip, p1.Tip, p2.Tip, p3.Tip, t);

                        float smoothTime = MathHelper.Lerp(p1.Time, p2.Time, t);
                        float smoothSpeed = MathHelper.Lerp(p1.Speed, p2.Speed, t);

                        smoothTrail.Add(new TrailSample { Tip = smoothTip, Time = smoothTime, Speed = smoothSpeed });
                    }
                }
                smoothTrail.Add(_trail[_trail.Count - 1]);
                for (int i = 0; i < smoothTrail.Count - 1; i++)
                {
                    TrailSample a = smoothTrail[i];
                    TrailSample b = smoothTrail[i + 1];

                    Vector2 from = anchorWorld + a.Tip;
                    Vector2 to   = anchorWorld + b.Tip;
                    Vector2 delta = to - from;

                    float length = delta.Length();
                    if (length < 0.01f)
                        continue;

                    float life = MathHelper.Clamp(a.Time, 0f, 1f);
                    float speedFactor = MathHelper.Clamp(a.Speed / 22f, 0f, 1f);
                    float thickness = MathHelper.Lerp(baseWidth, maxWidth, speedFactor) * life;
                    if (thickness < 0.6f)
                        continue;

                    Color color = Color.Lerp(outer, inner, life) * life;
                    float drawLen = length + 1f;
                    float rot = delta.ToRotation();

                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color * 0.30f, rot, pixelOrigin, new Vector2(drawLen, thickness * 2.6f), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color, rot, pixelOrigin, new Vector2(drawLen, thickness), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, new Color(255, 255, 255) * (life * 0.85f), rot, pixelOrigin, new Vector2(drawLen, thickness * 0.38f), SpriteEffects.None, 0f);
                }
            }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawBlade(Color lightColor)
        {
            Asset<Texture2D> asset = TextureAssets.Projectile[Type];
            if (!asset.IsLoaded)
                return;

            Texture2D tex = asset.Value;
            Vector2 origin = tex.Size() * 0.5f;

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;
            
            float glowScale = 0.8f * Projectile.scale;
            float opacityMult = 0.45f;
            
            if (State == BladeState.Attack || State == BladeState.WindUp)
            {
                glowScale *= 1.2f;
                opacityMult = 0.65f;
            }

            Color glowColor = new Color(0, 255, 120, 0) * opacityMult * Projectile.Opacity;
            Main.EntitySpriteDraw(glowTex, pos, null, glowColor, 0f, glowOrigin, glowScale, SpriteEffects.None, 0);
            
            if (State == BladeState.Attack || State == BladeState.WindUp)
            {
                try
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    float pulse = 0.5f + 0.5f * MathF.Sin(Main.GameUpdateCount * 0.25f);
                    Color glow = new Color(0, 255, 120, 0) * (0.55f + 0.25f * pulse);
                    Main.EntitySpriteDraw(tex, pos, null, glow, Projectile.rotation, origin, Projectile.scale * (1.15f + 0.08f * pulse), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(tex, pos, null, glow * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.30f, SpriteEffects.None, 0);
                }
                finally
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            Color drawColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(tex, pos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}
