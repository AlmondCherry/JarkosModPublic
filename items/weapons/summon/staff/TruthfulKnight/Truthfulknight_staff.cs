using System;
using System.Collections.Generic;
using jarkos.items.materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.TruthfulKnight
{
    #region Buff
    public class Truthfulknight_buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Truthfulknight_proj>()] > 0)
                player.buffTime[buffIndex] = 18000;
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    #endregion
    #region ModPlayer
    public class GuardianPlayer : ModPlayer
    {
        public bool HarmonyActive   = false;
        public int  NoManaGraceTimer = 0;
        public const int NoManaGrace = 60;
        public override void ResetEffects()
        {
            HarmonyActive = false;
        }
        public override void PostUpdateEquips()
        {
            int knightCount = Player.ownedProjectileCounts[
                ModContent.ProjectileType<Truthfulknight_proj>()];

            if (knightCount <= 0) return;

            bool wantHarmony = Main.mouseRight && Main.myPlayer == Player.whoAmI;
            int manaCostPerSec = Math.Min(knightCount, 5) * 20;
            float manaCostPerTick = manaCostPerSec / 60f;

            if (wantHarmony)
            {
                if (Player.statMana >= manaCostPerTick)
                {
                    NoManaGraceTimer = NoManaGrace;
                    HarmonyActive = true;
                    Player.statMana = Math.Max(0, (int)(Player.statMana - manaCostPerTick));
                    Player.manaRegenDelay = (int)Player.maxRegenDelay;
                }
                else
                {
                    if (NoManaGraceTimer > 0)
                    {
                        NoManaGraceTimer--;
                        HarmonyActive = true;
                    }
                    else
                    {
                        HarmonyActive = false;
                    }
                }
            }
            else
            {
                NoManaGraceTimer = 0;
                HarmonyActive = false;
            }
            if (HarmonyActive)
            {
                int active = Math.Min(knightCount, 5);
                Player.lifeRegen += active;
                Player.statDefense += active * 2;
                Player.endurance += Math.Min(0.9f, Player.endurance + active * 0.0075f);
            }
        }
    }
    #endregion
    #region Item
    public class Truthfulknight_staff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 43;
            Item.knockBack = 4f;
            Item.mana = 10;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<Truthfulknight_buff>();
            Item.shoot = ModContent.ProjectileType<Truthfulknight_proj>();
            Item.shootSpeed = 0f;
            Item.autoReuse = false;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 50, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile proj = Projectile.NewProjectileDirect(source, player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -20f), Vector2.Zero, ModContent.ProjectileType<Truthfulknight_proj>(), damage, knockback, player.whoAmI);
            proj.originalDamage = Item.damage;

            return false;
        }
        
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HallowedBar, 12);
            recipe.AddIngredient(ItemID.SoulofMight, 8);
            recipe.AddIngredient(ModContent.ItemType<SemiorganicStone>(), 2);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
    #endregion
    #region Guardian Projectile
    public class Truthfulknight_proj : ModProjectile
    {
        private const int FRAME_IDLE_START = 0;
        private const int FRAME_IDLE_COUNT = 3;
        private const int FRAME_TRANSITION = 3;
        private const int FRAME_ATTACK_START = 4;
        private const int FRAME_ATTACK_COUNT = 5;
        private const int FRAME_HARMONY_START = 9;
        private const int FRAME_HARMONY_COUNT = 4;
        private const int FRAME_SPEED = 8;
        private ref float AttackTimer => ref Projectile.ai[0];
        private ref float OrbitAngle => ref Projectile.ai[1];
        private ref float AnimFrame => ref Projectile.localAI[0];
        private ref float AnimTimer => ref Projectile.localAI[1];
        private const float AttackCooldown = 54f;
        private const float HitRange = 55f;
        private enum KnightState { Idle, Charging, Slashing, Circling }
        private KnightState state = KnightState.Idle;
        private int targetNPCIndex = -1;
        private float circleAngle = 0f;
        private float circleRadius = 110f;
        private Vector2 circleCenter = Vector2.Zero;
        private float circleTotalArc = 0f;
        private float circleDir = 1f;
        private const float CircleArc  = MathHelper.Pi * 1.4f;
        private int transitionTimer = 0;
        private int attackAnimFrame = 0;
        private int attackAnimTimer = 0;
        private int slashesCount = 0;
        private int slashTimer  = 0;
        private bool hasDealtDamageThisCharge = false;
        private readonly List<Vector2> trail = new List<Vector2>();
        private const int TrailLength = 22;
        private float harmonyBlend = 0f;
        private bool idleOffsetSet = false;
        private float idleAngleOffset = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 13;
        }
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 48;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.timeLeft = 18000;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active) { Projectile.Kill(); return; }
            if (player.HasBuff(ModContent.BuffType<Truthfulknight_buff>()))
                Projectile.timeLeft = 2;

            if (!idleOffsetSet)
            {
                idleOffsetSet = true;
                idleAngleOffset = GetMyIndex() * (MathHelper.TwoPi / Math.Max(1, GetTotalKnights()));
                circleAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            bool harmonyActive = player.GetModPlayer<GuardianPlayer>().HarmonyActive;
            harmonyBlend = MathHelper.Lerp(harmonyBlend, harmonyActive ? 1f : 0f, 0.08f);
            UpdateTrail(harmonyActive);

            if (harmonyActive)
            {
                state = KnightState.Idle;
                targetNPCIndex = -1;
                DoHarmonyMode(player);
            }
            else
            {
                DoCombatAI(player);
            }

            float targetRotation = Projectile.velocity.X * 0.04f;
            targetRotation = MathHelper.Clamp(targetRotation, -0.45f, 0.45f);
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.15f);

            UpdateAnimation(harmonyActive);
            Vector3 lightCol = harmonyActive ? new Vector3(0.2f, 0.6f, 1.0f) : new Vector3(0.5f, 0.4f, 0.1f);

            lightCol += new Vector3(0.4f, 0.4f, 0.4f);
            Lighting.AddLight(Projectile.Center, lightCol * 0.7f);
        }
        private void DoCombatAI(Player player)
        {
            if (AttackTimer > 0f) AttackTimer--;
            NPC target = GetLockedTarget();
            switch (state)
            {
                case KnightState.Idle:
                {
                    FollowPlayer(player);
                    if (AttackTimer <= 0f)
                    {
                        NPC found = FindTarget(player);
                        if (found != null)
                        {
                            targetNPCIndex = found.whoAmI;
                            hasDealtDamageThisCharge = false;
                            state = KnightState.Charging;
                            TriggerAttackAnim();
                        }
                    }
                    break;
                }
                case KnightState.Charging:
                {
                    if (target == null || !target.active || !target.CanBeChasedBy())
                    {
                        state = KnightState.Idle;
                        targetNPCIndex = -1;
                        break;
                    }
                    ChargeAtTarget(target);
                    float dist = Vector2.Distance(Projectile.Center, target.Center);
                    if (dist <= HitRange)
                    {
                        state = KnightState.Slashing;
                        slashesCount = 0;
                        slashTimer = 0;
                        break;
                    }
                    break;
                }
                case KnightState.Slashing:
                {
                    if (target == null || !target.active || !target.CanBeChasedBy())
                    {
                        state = KnightState.Idle;
                        targetNPCIndex = -1;
                        break;
                    }
                    Vector2 dir = target.Center - Projectile.Center;
                    float dist = dir.Length();
                    if (dist > 1f)
                    {
                        dir.Normalize();
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 12f, 0.35f);
                    }
                    slashTimer--;
                    if (slashTimer <= 0)
                    {
                        slashTimer = 14;
                        slashesCount++;
                        bool lastSlash = slashesCount >= 4;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            DealDamageToNPC(target, lastSlash);
                        }
                        if (lastSlash)
                        {
                            StartCircling(target);
                        }
                    }
                    break;
                }
                case KnightState.Circling:
                {
                    DoCircle();
                    if (circleTotalArc >= CircleArc)
                    {
                        circleTotalArc = 0f;
                        NPC nextTarget = FindTarget(player);
                        if (nextTarget != null)
                        {
                            targetNPCIndex = nextTarget.whoAmI;
                            hasDealtDamageThisCharge = false;
                            state = KnightState.Charging;
                            TriggerAttackAnim();
                        }
                        else
                        {
                            state = KnightState.Idle;
                            targetNPCIndex = -1;
                            AttackTimer = AttackCooldown;
                        }
                    }
                    break;
                }
            }

            if (Math.Abs(Projectile.velocity.X) > 0.2f)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
        }
        private void DealDamageToNPC(NPC target, bool lastSlash = false)
        {
            Player player = Main.player[Projectile.owner];

            int baseDamage = (int)player.GetDamage(DamageClass.Summon).ApplyTo(Projectile.originalDamage);
            int damageVariance = Main.rand.Next(-10, 21);
            int damage = Math.Max(1, baseDamage + damageVariance);
            float kb = Projectile.knockBack;
            int hitDir = Projectile.Center.X < target.Center.X ? 1 : -1;

            NPC.HitInfo hitInfo = target.CalculateHitInfo(damage, hitDir, false, kb, DamageClass.Summon);
            int damageDone = target.StrikeNPC(hitInfo);
            player.addDPS(damageDone);
            target.immune[player.whoAmI] = 0;

            SpawnHitEffects(target);
        }
        private void SpawnHitEffects(NPC target)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 4f), 0, default, Main.rand.NextFloat(1f, 1.8f));
                d.velocity += Projectile.velocity * 0.15f;
            }
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(target.Center, 8, 8, DustID.GoldFlame, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, default, 1.3f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
        }
        private void ChargeAtTarget(NPC target)
        {
            Vector2 dir = target.Center - Projectile.Center;

            float dist = dir.Length();
            if (dist < 1f) return;
            dir.Normalize();
            float speed = 15f;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speed, 0.22f);

            if (Projectile.velocity.Length() > speed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= speed;
            }
        }
        private void StartCircling(NPC target)
        {
            state = KnightState.Circling;
            circleCenter = target.Center;
            circleRadius = 100f + Main.rand.NextFloat(40f);
            circleTotalArc = 0f;
            circleDir = Main.rand.NextBool() ? 1f : -1f;

            Vector2 fromTarget = Projectile.Center - target.Center;
            circleAngle = fromTarget.ToRotation();
        }
        private void DoCircle()
        {
            float speed = 0.09f * circleDir;
            circleAngle += speed;
            circleTotalArc += Math.Abs(speed);

            Vector2 targetPos = circleCenter + new Vector2((float)Math.Cos(circleAngle), (float)Math.Sin(circleAngle)) * circleRadius;
            Vector2 dir = targetPos - Projectile.Center;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 0.55f, 0.22f);
        }
        private void FollowPlayer(Player player)
        {
            int total = GetTotalKnights();
            int myIndex = GetMyIndex();
            float spread  = 50f;
            float offset = total > 1 ? MathHelper.Lerp(-spread, spread, (float)myIndex / (total - 1)) : 0f;

            Vector2 targetPos = player.Center + new Vector2(offset, 10f);
            Vector2 dir = targetPos - Projectile.Center;
            float dist = dir.Length();

            if (dist > 1200f)
            {
                Projectile.Center = targetPos;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
                return;
            }

            float speedMult = MathHelper.Clamp(dist * 0.13f, 4f, 18f);
            Vector2 targetVel = dir;

            if (dist > 1f) targetVel.Normalize();
            targetVel *= speedMult;

            Projectile.velocity = dist > 6f ? Vector2.Lerp(Projectile.velocity, targetVel, 0.12f) : Projectile.velocity * 0.8f;
        }
        private void DoHarmonyMode(Player player)
        {
            int total = GetTotalKnights();
            int myIndex = GetMyIndex();
            float baseAngle = MathHelper.TwoPi / Math.Max(1, total) * myIndex;
            OrbitAngle += 0.04f;

            float orbitRadius = 65f + total * 8f;
            Vector2 orbitCenter = player.Center + new Vector2(0f, -90f);
            Vector2 targetPos = orbitCenter + new Vector2((float)Math.Cos(OrbitAngle + baseAngle) * orbitRadius, (float)Math.Sin(OrbitAngle + baseAngle) * orbitRadius * 0.38f);
            Vector2 toTarget = targetPos - Projectile.Center;

            float dist = toTarget.Length();
            if (dist > 400f)
            {
                Projectile.Center = targetPos;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }
            else
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 0.18f, 0.15f);
            }

            if (Math.Abs(Projectile.velocity.X) > 0.1f)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center + new Vector2(Main.rand.NextFloat(-16f, 16f), Main.rand.NextFloat(-20f, 20f)), 0, 0, DustID.BlueCrystalShard, Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f - 0.5f, 150, Color.CornflowerBlue, 0.8f);
                dust.noGravity = true;
            }
        }
        private NPC GetLockedTarget()
        {
            if (targetNPCIndex < 0 || targetNPCIndex >= Main.maxNPCs) return null;
            NPC npc = Main.npc[targetNPCIndex];

            return npc.active && npc.CanBeChasedBy() ? npc : null;
        }
        private NPC FindTarget(Player player)
        {
            NPC best = null;
            float closest = 850f;

            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy())
                {
                    float d = Vector2.Distance(Projectile.Center, npc.Center);
                    if (d < closest) { best = npc; closest = d; }
                }
                if (best != null) return best;
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < closest) { best = npc; closest = d; }
            }
            return best;
        }
        private int GetMyIndex()
        {
            int idx = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.type != Projectile.type || p.owner != Projectile.owner)
                    continue;
                if (p.whoAmI == Projectile.whoAmI) return idx;
                idx++;
            }
            return 0;
        }
        private int GetTotalKnights()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Projectile.type && p.owner == Projectile.owner)
                    count++;
            }
            return Math.Max(1, count);
        }
        private void TriggerAttackAnim()
        {
            transitionTimer = FRAME_SPEED;
            attackAnimFrame = 0;
            attackAnimTimer = 0;
        }
        private void UpdateAnimation(bool harmonyActive)
        {
            if (harmonyActive)
            {
                transitionTimer = 0;
                AnimTimer++;
                if (AnimTimer >= FRAME_SPEED)
                {
                    AnimTimer = 0;
                    AnimFrame++;
                    if (AnimFrame >= FRAME_HARMONY_COUNT) AnimFrame = 0;
                }
                Projectile.frame = FRAME_HARMONY_START + (int)AnimFrame;
                return;
            }
            if (state == KnightState.Charging || state == KnightState.Circling || state == KnightState.Slashing)
            {
                if (transitionTimer > 0)
                {
                    transitionTimer--;
                    Projectile.frame = FRAME_TRANSITION;
                    return;
                }
                int combatFrameSpeed = FRAME_SPEED / 2;
                if (combatFrameSpeed < 1) combatFrameSpeed = 1;
                attackAnimTimer++;
                if (attackAnimTimer >= combatFrameSpeed)
                {
                    attackAnimTimer = 0;
                    attackAnimFrame++;
                    if (attackAnimFrame >= FRAME_ATTACK_COUNT) attackAnimFrame = 0;
                }
                Projectile.frame = FRAME_ATTACK_START + attackAnimFrame;
                return;
            }
            transitionTimer = 0;
            AnimTimer++;
            if (AnimTimer >= FRAME_SPEED)
            {
                AnimTimer = 0;
                AnimFrame++;
                if (AnimFrame >= FRAME_IDLE_COUNT) AnimFrame = 0;
            }
            Projectile.frame = FRAME_IDLE_START + (int)AnimFrame;
        }
        private void UpdateTrail(bool harmonyActive)
        {
            bool showTrail = harmonyActive || state == KnightState.Circling
                             || state == KnightState.Charging || state == KnightState.Slashing;
            if (showTrail)
            {
                trail.Insert(0, Projectile.Center);
                if (trail.Count > TrailLength) trail.RemoveAt(trail.Count - 1);
            }
            else
            {
                if (trail.Count > 0) trail.RemoveAt(trail.Count - 1);
            }
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override bool OnTileCollide(Vector2 oldVelocity) => false;
        public override bool PreDraw(ref Color lightColor)
        {
            if (trail.Count > 1) DrawTrail();
            DrawKnight(lightColor);
            return false;
        }
        private void DrawTrail()
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            bool isHarmonyMode = harmonyBlend > 0.3f;

            for (int i = 1; i < trail.Count; i++)
            {
                float progress = 1f - (float)i / trail.Count;
                float alpha = progress * (isHarmonyMode ? harmonyBlend : 0.65f) * 0.78f;
                float width = MathHelper.Lerp(1f, isHarmonyMode ? 9f : 6f, progress);
                Color col = isHarmonyMode ? Color.Lerp(Color.DeepSkyBlue, Color.White, progress * 0.5f) : Color.Lerp(Color.White, Color.LightGray * 0.5f, progress);
                Vector2 seg = trail[i - 1] - trail[i];
                float rot = seg.ToRotation();

                Main.EntitySpriteDraw(tex, trail[i] - Main.screenPosition, new Rectangle(0, 0, 1, 1), col * alpha, rot, Vector2.Zero, new Vector2(seg.Length() + 1f, width), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(tex, trail[i] - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * alpha * 0.4f, rot, Vector2.Zero, new Vector2(seg.Length() + 1f, width * 0.35f), SpriteEffects.None, 0);
            }
        }
        private void DrawKnight(Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameH = tex.Height / Main.projFrames[Projectile.type];
            
            Rectangle rect = new Rectangle(0, Projectile.frame * frameH, tex.Width, frameH);
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color col = Color.Lerp(lightColor, new Color(100, 180, 255), harmonyBlend * 0.4f);

            if (state == KnightState.Charging && harmonyBlend < 0.1f)
                col = Color.Lerp(lightColor, new Color(255, 180, 60), 0.35f);

            int r = Math.Max(col.R, (byte)150);
            int g = Math.Max(col.G, (byte)150);
            int b = Math.Max(col.B, (byte)150);
            col = new Color((byte)r, (byte)g, (byte)b, col.A);
            Color outlineCol = Color.White * 0.28f;

            Vector2[] offsets = new Vector2[] { new Vector2(-1.5f, 0), new Vector2(1.5f, 0), new Vector2(0, -1.5f), new Vector2(0, 1.5f) };

            foreach (var offset in offsets)
            {
                Main.EntitySpriteDraw(tex, Projectile.Center + offset - Main.screenPosition, rect, outlineCol, Projectile.rotation, new Vector2(rect.Width / 2f, rect.Height / 2f), Projectile.scale, fx, 0);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, rect, col, Projectile.rotation, new Vector2(rect.Width / 2f, rect.Height / 2f), Projectile.scale, fx, 0);
        }
    }
    #endregion
}
