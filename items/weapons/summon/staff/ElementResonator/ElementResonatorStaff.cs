using System;
using System.Collections.Generic;
using jarkos.buffs.summonbuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using Jarkos.items.materials;
using jarkos.items.materials;
using Terraria.GameContent;
using Terraria.Audio;

namespace jarkos.items.weapons.summon.staff.ElementResonator
{
	#region Buff
	public class ElementResonator_buff : ModBuff
	{
		public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/ElementResonator_buff";
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			var modPlayer = player.GetModPlayer<ElementalResonatorPlayer>();
			if (modPlayer.elementalGroupActive)
			{
				player.buffTime[buffIndex] = 2;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
		public override bool RightClick(int buffIndex)
		{
			Player player = Main.LocalPlayer;
			var modPlayer = player.GetModPlayer<ElementalResonatorPlayer>();
			modPlayer.KillAllSpirits();
			return true;
		}
	}
	#endregion
	#region Item
	public class ElementResonatorStaff : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 22;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<FireSpirit>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 90, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<ElementalResonatorPlayer>();
            if (modPlayer.elementalGroupActive)
            {
                modPlayer.KillAllSpirits();
            }
            if (player.slotsMinions + 2 > player.maxMinions)
            {
                Main.NewText("Not enough minion slots! You need 2 slots", Color.OrangeRed);
                return false;
            }
            SpawnAllSpirits(player, source, position, damage);
            return false;
        }
        private void SpawnAllSpirits(Player player, IEntitySource source, Vector2 position, int damage)
        {
            var modPlayer = player.GetModPlayer<ElementalResonatorPlayer>();
            Vector2[] spawnOffsets = {
                new Vector2(-60, -20), new Vector2(-30, -40), new Vector2(0,   -50), new Vector2(30,  -40), new Vector2(60,  -20)
            };
            int[] types = {
                ModContent.ProjectileType<FireSpirit>(), ModContent.ProjectileType<WaterSpirit>(), ModContent.ProjectileType<EarthSpirit>(), ModContent.ProjectileType<IceSpirit>(), ModContent.ProjectileType<LightningSpirit>()
            };
            int[] indices = new int[5];
            for (int i = 0; i < 5; i++)
            {
                indices[i] = Projectile.NewProjectile(source, position + spawnOffsets[i], Vector2.Zero, types[i], damage, 3f, player.whoAmI, ai1: i);
            }
            modPlayer.SetSpiritIndices(indices);
            modPlayer.elementalGroupActive = true;
            player.AddBuff(ModContent.BuffType<ElementResonator_buff>(), 2);
            SoundEngine.PlaySound(SoundID.Item44, position);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<DiffusalBar>(), 10);
            recipe.AddIngredient(ModContent.ItemType<ElementalPowder>(), 4);
            recipe.AddIngredient(ModContent.ItemType<SemiorganicStone>(), 2);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
	public class ElementalResonatorPlayer : ModPlayer
    {
        public bool elementalGroupActive = false;
        public int[] spiritIndices = new int[5] { -1, -1, -1, -1, -1 };
        public int sharedTargetIndex = -1;
        public float whipMarkBonus = 0f;
        private int whipMarkBonusTimer = 0;
        public override void ResetEffects()
        {
        }
        public void SetSpiritIndices(int[] indices)
        {
            for (int i = 0; i < 5; i++)
                spiritIndices[i] = indices[i];
        }
        public void KillAllSpirits()
        {
            for (int i = 0; i < 5; i++)
            {
                if (spiritIndices[i] >= 0 && spiritIndices[i] < Main.maxProjectiles)
                {
                    Projectile proj = Main.projectile[spiritIndices[i]];
                    if (proj.active) proj.Kill();
                }
                spiritIndices[i] = -1;
            }
            elementalGroupActive = false;
            sharedTargetIndex = -1;
            Player.ClearBuff(ModContent.BuffType<ElementResonator_buff>());
        }
        public override void PostUpdate()
        {
            if (elementalGroupActive)
            {
                bool anyAlive = false;
                for (int i = 0; i < 5; i++)
                {
                    if (spiritIndices[i] >= 0 && spiritIndices[i] < Main.maxProjectiles)
                    {
                        if (Main.projectile[spiritIndices[i]].active)
                        {
                            anyAlive = true;
                            break;
                        }
                    }
                }
                if (!anyAlive)
                {
                    elementalGroupActive = false;
                    sharedTargetIndex = -1;
                }
            }
            if (whipMarkBonusTimer > 0)
            {
                whipMarkBonusTimer--;
                if (whipMarkBonusTimer <= 0)
                    whipMarkBonus = 0f;
            }
            UpdateSharedTarget();
        }
        private void UpdateSharedTarget()
        {
            if (!elementalGroupActive) return;
            float closestDist = 800f;
            int newTarget = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.lifeMax <= 5) continue;
                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    newTarget = i;
                }
            }
            sharedTargetIndex = newTarget;
        }
        public void OnWhipHit(NPC target)
        {
            if (!elementalGroupActive) return;
            if (Main.rand.NextFloat() < 0.30f)
            {
                whipMarkBonus = Main.rand.NextFloat(0.05f, 0.10f);
                whipMarkBonusTimer = 5 * 60;
            }
        }
        public int GetSpiritDamage(int baseDamage)
        {
            float fullBonus = Player.GetDamage(DamageClass.Summon).Additive - 1f;
            float reducedBonus = 1f + fullBonus * 0.25f;
            float multBonus = Player.GetDamage(DamageClass.Summon).Multiplicative;
            float reducedMult = 1f + (multBonus - 1f) * 0.25f;
            return Math.Max(1, (int)(baseDamage * reducedBonus * reducedMult));
        }
    }
	#endregion
	#region Projectile
	public abstract class ElementSpiritBase : ModProjectile
    {
        protected const float HoverRadius = 80f;
        protected const float DetectionRange = 600f;
        protected ref float AttackTimer => ref Projectile.ai[0];
        protected ref float SpiritIndexSlot => ref Projectile.ai[1];
        protected float hoverAngle;
        protected float hoverAngleSpeed = 0.025f;
        protected virtual float BaseMarkChance => 0.05f;
        protected abstract int MarkBuffType { get; }
        protected abstract int MarkDuration { get; }
        protected abstract Color SpiritGlowColor { get; }
        protected virtual bool IsGroundBased => true;
        protected bool isForceFlying = false;
        protected bool isInAir = false;
        protected bool isAttacking = false;
        protected float AirAttackPenalty => (IsGroundBased && isInAir) ? 2.0f : 1.0f;
        private const float ForceFlightThreshold = 350f;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 0.5f;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.netImportant = true;
            Projectile.minion = true;
            Projectile.minionSlots = 0;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            float distToOwner = Vector2.Distance(Projectile.Center, owner.Center);
            if (distToOwner > 1200f)
            {
                Projectile.Center = owner.Center + new Vector2(Main.rand.NextFloat(-40f, 40f), -20f);
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
                isForceFlying = false;
                isAttacking = false;
                ResetSpiritAttackState();
            }
            var modPlayer = owner.GetModPlayer<ElementalResonatorPlayer>();
            Projectile.damage = modPlayer.GetSpiritDamage(GetBaseDamage());
            if (IsGroundBased)
            {
                UpdateGroundLogic(owner);
            }
            NPC target = GetSharedTarget(modPlayer);
            UpdateSpiritAI(owner, modPlayer, target);
            if (target == null)
            {
                if (IsGroundBased && !isForceFlying)
                    WalkTowardPlayer(owner);
                else
                    HoverAroundPlayer(owner);
            }
            SpawnSpiritParticles();
            owner.MinionNPCTargetAim(false);
        }
        protected void UpdateGroundLogic(Player owner)
        {
            float distToOwner = Vector2.Distance(Projectile.Center, owner.Center);
            bool ownerAirborne = IsOwnerAirborne(owner);
            bool tooFar = distToOwner > ForceFlightThreshold;
            if ((ownerAirborne || tooFar) && !isAttacking)
            {
                if (!isForceFlying)
                {
                    isForceFlying = true;
                    Projectile.tileCollide = false;
                }
                Vector2 flyTarget = owner.Center + new Vector2(SpiritIndexSlot * 20f - 40f, -30f);
                MoveToward(flyTarget, 10f);
                isInAir = true;
            }
            else
            {
                if (isForceFlying && !isAttacking)
                {
                    isForceFlying = false;
                    Projectile.tileCollide = true;
                }
                if (!isAttacking && !isForceFlying)
                {
                    Projectile.velocity.Y += 0.5f;
                    Projectile.velocity.Y = Math.Min(Projectile.velocity.Y, 14f);
                    Projectile.tileCollide = true;
                }
                isInAir = !HasGroundBelow(8);
            }
        }
        protected virtual void ResetSpiritAttackState()
        {
            AttackTimer = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int numFrames = Main.projFrames[Type];
            int frameHeight = texture.Height / numFrames;
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color glowColor = SpiritGlowColor;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.5f, 0f).RotatedBy(i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.04f);
                Main.EntitySpriteDraw(texture, drawPos + offset, sourceRect, glowColor * 0.25f, Projectile.rotation, origin, Projectile.scale, effects, 0);
            }
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, lightColor * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
        protected bool IsOwnerAirborne(Player owner)
        {
            int tileX = (int)(owner.Center.X / 16f);
            int tileY = (int)((owner.Bottom.Y + 8f) / 16f);
            bool hasGround = false;
            for (int y = tileY; y < tileY + 4; y++)
            {
                if (y >= 0 && y < Main.maxTilesY && tileX >= 0 && tileX < Main.maxTilesX)
                {
                    Tile tile = Main.tile[tileX, y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        hasGround = true;
                        break;
                    }
                }
            }
            return !hasGround;
        }
        protected bool HasGroundBelow(int blocksDown)
        {
            int tileX = (int)(Projectile.Center.X / 16f);
            int tileY = (int)((Projectile.Bottom.Y + 4f) / 16f);
            for (int y = tileY; y < tileY + blocksDown; y++)
            {
                if (y >= 0 && y < Main.maxTilesY && tileX >= 0 && tileX < Main.maxTilesX)
                {
                    Tile tile = Main.tile[tileX, y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                        return true;
                }
            }
            return false;
        }
        protected void WalkTowardPlayer(Player owner)
        {
            float dx = owner.Center.X - Projectile.Center.X;
            float walkSpeed = 4f;
            Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, Math.Sign(dx) * walkSpeed, 0.12f);
            if (Projectile.velocity.X != 0)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            TryJumpOverObstacle();
        }
        protected void WalkTowardTarget(Vector2 targetPos, float speed)
        {
            float dx = targetPos.X - Projectile.Center.X;
            Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, Math.Sign(dx) * speed, 0.15f);
            if (Projectile.velocity.X != 0)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            TryJumpOverObstacle();
        }
        protected void TryJumpOverObstacle()
        {
            if (Math.Abs(Projectile.velocity.X) < 1f) return;
            int dir = Projectile.velocity.X > 0 ? 1 : -1;
            int tileX = (int)((Projectile.Center.X + dir * (Projectile.width / 2f + 8f)) / 16f);
            int tileY = (int)((Projectile.Bottom.Y - 4f) / 16f);
            bool wallAhead = false;
            for (int y = tileY; y >= tileY - 2; y--)
            {
                if (y >= 0 && y < Main.maxTilesY && tileX >= 0 && tileX < Main.maxTilesX)
                {
                    Tile tile = Main.tile[tileX, y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        wallAhead = true;
                        break;
                    }
                }
            }
            if (wallAhead && Projectile.velocity.Y == 0f)
            {
                Projectile.velocity.Y = -8f;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (IsGroundBased)
            {
                if (oldVelocity.Y > 0)
                    Projectile.velocity.Y = 0;
                if (Math.Abs(oldVelocity.X) > 0 && Projectile.velocity.X == 0)
                    Projectile.velocity.X = 0;
                return false;
            }
            return false;
        }
        protected NPC GetSharedTarget(ElementalResonatorPlayer modPlayer)
        {
            int idx = modPlayer.sharedTargetIndex;
            if (idx >= 0 && idx < Main.maxNPCs && Main.npc[idx].active && !Main.npc[idx].friendly)
                return Main.npc[idx];
            return null;
        }
        protected virtual int GetBaseDamage() => Projectile.originalDamage;
        protected abstract void UpdateSpiritAI(Player owner, ElementalResonatorPlayer modPlayer, NPC target);
        protected void HoverAroundPlayer(Player owner)
        {
            hoverAngle += hoverAngleSpeed;
            float spiritOffset = SpiritIndexSlot * (MathHelper.TwoPi / 5f);
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(hoverAngle + spiritOffset) * HoverRadius, (float)Math.Sin(hoverAngle + spiritOffset) * HoverRadius - 30f);
            MoveToward(targetPos, 5f);
        }
        protected void MoveToward(Vector2 target, float speed)
        {
            Vector2 dir = target - Projectile.Center;
            float dist = dir.Length();
            if (dist > 5f)
            {
                dir.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * Math.Min(dist * 0.12f, speed), 0.18f);
            }
            else
            {
                Projectile.velocity *= 0.85f;
            }
            if (Projectile.velocity.X != 0)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
        }
        protected bool TryApplyMark(NPC target, ElementalResonatorPlayer modPlayer)
        {
            float chance = BaseMarkChance + modPlayer.whipMarkBonus;
            if (Main.rand.NextFloat() < chance)
            {
                target.AddBuff(MarkBuffType, MarkDuration);
                SpawnMarkApplyEffect(target);
                return true;
            }
            return false;
        }
        protected virtual void SpawnMarkApplyEffect(NPC target)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(target.Center - new Vector2(8f), 16, 16, DustID.MagicMirror, vel.X, vel.Y, 150, SpiritGlowColor, 1.2f);
                d.noGravity = true;
            }
        }
        protected virtual void SpawnSpiritParticles()
        {
            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 150, SpiritGlowColor, 0.9f);
                d.noGravity = true;
            }
        }
        public override bool? CanDamage() => false;
    }
	#endregion
	public class FireSpirit : ElementSpiritBase
    {
        protected override float BaseMarkChance => 0.05f;
        protected override int MarkBuffType => ModContent.BuffType<FireMarkDebuff>();
        protected override int MarkDuration => 5 * 60;
        protected override Color SpiritGlowColor => new Color(255, 100, 30);
        private const int AttackCooldown = 75;
        private bool isCharging = false;
        private Vector2 chargeTarget;
        private int chargeTimer = 0;
        private const int ChargeDuration = 20;
        public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/FireElementProj";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 26;
            Projectile.height = 34;
            Projectile.minionSlots = 2;
        }
        protected override void ResetSpiritAttackState()
        {
            base.ResetSpiritAttackState();
            isCharging = false;
            chargeTimer = 0;
        }
        protected override void UpdateSpiritAI(Player owner, ElementalResonatorPlayer modPlayer, NPC target)
        {
            if (target == null)
            {
                isCharging = false;
                chargeTimer = 0;
                isAttacking = false;
                return;
            }
            AttackTimer++;
            if (isCharging)
            {
                isAttacking = true;
                Projectile.tileCollide = false;
                chargeTimer++;
                Projectile.velocity = Vector2.Normalize(chargeTarget - Projectile.Center) * 18f;
                if (Main.rand.NextBool(2))
                {
                    Vector2 particleVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2f, -0.5f));
                    float scale = Main.rand.NextFloat(0.4f, 0.7f);
                    new FireTrailParticle(Projectile.Center, particleVel, new Color(255, 120, 20), 60, scale, 1f, Main.rand.NextFloat(-0.04f, 0.04f)).Spawn();
                }
                for (int i = 0; i < 3; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, -Projectile.velocity.X * 0.3f + Main.rand.NextFloat(-1f, 1f), -Projectile.velocity.Y * 0.3f + Main.rand.NextFloat(-1f, 1f), 150, SpiritGlowColor, 1.5f);
                    d.noGravity = true;
                }
                float dist = Vector2.Distance(Projectile.Center, chargeTarget);
                if (dist < 30f || chargeTimer >= ChargeDuration)
                {
                    if (dist < 60f && Main.myPlayer == Projectile.owner)
                    {
                        DealDamageToTarget(target, modPlayer, owner);
                    }
                    isCharging = false;
                    chargeTimer = 0;
                    AttackTimer = 0;
                    isAttacking = false;
                    Projectile.tileCollide = true;
                }
            }
            else
            {
                isAttacking = false;
                if (!isForceFlying)
                {
                    WalkTowardTarget(target.Center, 5f);
                }
                else
                {
                    Vector2 orbitPos = target.Center + new Vector2((float)Math.Cos(hoverAngle) * 70f, (float)Math.Sin(hoverAngle) * 40f - 20f);
                    hoverAngle += 0.04f;
                    MoveToward(orbitPos, 6f);
                }
                if (AttackTimer >= (int)(AttackCooldown * AirAttackPenalty))
                {
                    isCharging = true;
                    chargeTimer = 0;
                    chargeTarget = target.Center;
                    SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, Projectile.position);
                }
            }
        }
        private void DealDamageToTarget(NPC target, ElementalResonatorPlayer modPlayer, Player owner)
        {
            int damage = modPlayer.GetSpiritDamage(Projectile.originalDamage);
            target.SimpleStrikeNPC(damage, Projectile.direction, false, 3f, DamageClass.Summon);
            TryApplyMark(target, modPlayer);
            SpawnFireExplosion(target.Center);
            ElementalMarkGlobalNPC.CheckResonance(target, owner);
        }
        private void SpawnFireExplosion(Vector2 center)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustDirect(center - new Vector2(10f), 20, 20, DustID.Torch, vel.X, vel.Y, 150, SpiritGlowColor, 1.8f);
                d.noGravity = true;
            }
        }
    }
	public class WaterSpirit : ElementSpiritBase
    {
        protected override float BaseMarkChance => 0.02f;
        protected override int MarkBuffType => ModContent.BuffType<WaterMarkDebuff>();
        protected override int MarkDuration => 6 * 60;
        protected override Color SpiritGlowColor => new Color(30, 120, 255);
        private const int AttackCooldown = 90;
        private const float ShootRange = 200f;
        public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/WaterElementProj";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 25;
            Projectile.height = 25;
        }
        protected override void ResetSpiritAttackState()
        {
            base.ResetSpiritAttackState();
        }
        protected override void UpdateSpiritAI(Player owner, ElementalResonatorPlayer modPlayer, NPC target)
        {
            if (target == null) return;
            AttackTimer++;
            if (!isForceFlying)
            {
                float dx = target.Center.X - Projectile.Center.X;
                float idealDist = ShootRange * 0.7f;
                if (Math.Abs(dx) > ShootRange)
                {
                    WalkTowardTarget(target.Center, 5f);
                }
                else if (Math.Abs(dx) < idealDist * 0.5f)
                {
                    Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, Math.Sign(Projectile.Center.X - target.Center.X) * 3f, 0.1f);
                }
            }
            else
            {
                hoverAngle += 0.03f;
                Vector2 idealPos = target.Center + new Vector2((float)Math.Cos(hoverAngle) * ShootRange, (float)Math.Sin(hoverAngle) * ShootRange * 0.5f);
                MoveToward(idealPos, 5f);
            }
            if (AttackTimer >= (int)(AttackCooldown * AirAttackPenalty) && Main.myPlayer == Projectile.owner)
            {
                AttackTimer = 0;
                ShootWaterBolt(target, modPlayer, owner);
            }
        }
        private void ShootWaterBolt(NPC target, ElementalResonatorPlayer modPlayer, Player owner)
        {
            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            dir = dir.RotatedByRandom(MathHelper.ToRadians(8f));
            int damage = modPlayer.GetSpiritDamage(Projectile.originalDamage);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir * 10f, ModContent.ProjectileType<WaterBoltProjectile>(), damage, 2f, Projectile.owner, ai0: target.whoAmI);
            SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.position);
        }
    }
	public class EarthSpirit : ElementSpiritBase
    {
        protected override float BaseMarkChance => 0.05f;
        protected override int MarkBuffType => ModContent.BuffType<EarthMarkDebuff>();
        protected override int MarkDuration => 5 * 60;
        protected override Color SpiritGlowColor => new Color(139, 90, 43);
        private enum EarthState { Walking, JumpingUp, Petrifying, Falling }
        private EarthState state = EarthState.Walking;
        private const int JumpCooldown = 40;
        private Vector2 jumpTarget;
        private int stateTimer = 0;
        private const int PetrifyDuration = 2;
        public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/EarthElementProj";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 30;
            Projectile.height = 35;
            Projectile.tileCollide = true;
        }
        protected override void ResetSpiritAttackState()
        {
            base.ResetSpiritAttackState();
            state = EarthState.Walking;
            stateTimer = 0;
        }
        protected override void UpdateSpiritAI(Player owner, ElementalResonatorPlayer modPlayer, NPC target)
        {
            switch (state)
            {
                case EarthState.Walking:
                    UpdateWalking(owner, target);
                    break;
                case EarthState.JumpingUp:
                    UpdateJumping();
                    break;
                case EarthState.Petrifying:
                    UpdatePetrifying();
                    break;
                case EarthState.Falling:
                    UpdateFalling(target, modPlayer, owner);
                    break;
            }
        }
        private void UpdateWalking(Player owner, NPC target)
        {
            isAttacking = false;
            Projectile.tileCollide = true;
            AttackTimer++;
            if (target == null)
            {
                return;
            }
            WalkTowardTarget(target.Center, 4f);
            if (Main.rand.NextBool(8))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0, 0, 150, SpiritGlowColor);
            }
            float dx = target.Center.X - Projectile.Center.X;
            if (AttackTimer >= (int)(JumpCooldown * AirAttackPenalty) && Math.Abs(dx) < 200f)
            {
                state = EarthState.JumpingUp;
                stateTimer = 0;
                jumpTarget = target.Center;
                isAttacking = true;
                Projectile.tileCollide = false;
                Projectile.velocity = new Vector2(dx * 0.02f, -16f);
                AttackTimer = 0;
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.position);
            }
        }
        private void UpdateJumping()
        {
            isAttacking = true;
            Projectile.tileCollide = false;
            stateTimer++;
            Projectile.velocity.Y += 0.3f;
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 0f), 150, SpiritGlowColor, 1.1f);
                d.noGravity = false;
            }
            if (Projectile.velocity.Y >= 0 || stateTimer > 40)
            {
                state = EarthState.Petrifying;
                stateTimer = 0;
                Projectile.velocity = Vector2.Zero;
            }
        }
        private void UpdatePetrifying()
        {
            isAttacking = true;
            Projectile.tileCollide = false;
            stateTimer++;
            Projectile.velocity = Vector2.Zero;
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 0f), 150, Color.Gray, 1.2f);
                d.noGravity = true;
            }
            if (stateTimer >= PetrifyDuration)
            {
                state = EarthState.Falling;
                stateTimer = 0;
                Projectile.velocity = new Vector2((jumpTarget.X - Projectile.Center.X) * 0.05f, 2f);
            }
        }
        private void UpdateFalling(NPC target, ElementalResonatorPlayer modPlayer, Player owner)
        {
            isAttacking = true;
            Projectile.tileCollide = false;
            stateTimer++;
            Projectile.velocity.Y += 1.2f;
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y, 20f);
            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, Projectile.velocity.X * -0.3f + Main.rand.NextFloat(-2f, 2f), -2f, 150, SpiritGlowColor, 1.4f);
                d.noGravity = false;
            }
            bool hitGround = HasGroundBelow(1) && Projectile.velocity.Y > 0;
            if (hitGround || stateTimer > 180)
            {
                LandingImpact(modPlayer, owner);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (state == EarthState.Falling)
            {
                Player owner = Main.player[Projectile.owner];
                var modPlayer = owner.GetModPlayer<ElementalResonatorPlayer>();
                LandingImpact(modPlayer, owner);
                return false;
            }
            if (state == EarthState.Walking)
            {
                if (oldVelocity.Y > 0)
                    Projectile.velocity.Y = 0;
                return false;
            }
            return false;
        }
        private void LandingImpact(ElementalResonatorPlayer modPlayer, Player owner)
        {
            state = EarthState.Walking;
            isAttacking = false;
            Projectile.tileCollide = true;
            Projectile.velocity.Y = 0;
            stateTimer = 0;
            if (Main.myPlayer != Projectile.owner) return;
            int damage = (int)(modPlayer.GetSpiritDamage(Projectile.originalDamage) * 1.8f);
            float aoeRadius = 100f;
            SpawnEarthExplosion(Projectile.Center);
            bool hitAnyone = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.lifeMax <= 5) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= aoeRadius)
                {
                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage = damage, Knockback = 6f, HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1, Crit = true, DamageType = DamageClass.Summon
                    };
                    npc.StrikeNPC(hitInfo);
                    TryApplyMark(npc, modPlayer);
                    ElementalMarkGlobalNPC.CheckResonance(npc, owner);
                    hitAnyone = true;
                }
            }
            if (hitAnyone)
            {
               SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.position);
            }
        }
        private void SpawnEarthExplosion(Vector2 center)
        {
            for (int i = 0; i < 18; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f);
                float scale = Main.rand.NextFloat(0.4f, 1.2f);
                Color brownColor = new Color(139 + Main.rand.Next(-20, 20), 90 + Main.rand.Next(-15, 15), 43 + Main.rand.Next(-10, 10));
                new Spark(center, vel, brownColor, 45, scale, 1f).Spawn();
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 4f);
                vel.Y = Math.Abs(vel.Y) * -1f;
                Dust d = Dust.NewDustDirect(center - new Vector2(15f), 30, 30, DustID.Dirt, vel.X, vel.Y, 150, SpiritGlowColor, 1.5f);
                d.noGravity = false;
                Dust d2 = Dust.NewDustDirect(center - new Vector2(10f), 20, 20, DustID.Stone, vel.X * 0.5f, vel.Y * 0.5f, 150, Color.Gray, 1.2f);
            }
        }
        protected override void SpawnSpiritParticles()
        {
            if (Main.rand.NextBool(8))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0, -0.5f, 150, SpiritGlowColor, 0.8f);
            }
        }
    }
	public class IceSpirit : ElementSpiritBase
    {
        protected override float BaseMarkChance => 0.03f;
        protected override int MarkBuffType => ModContent.BuffType<IceMarkDebuff>();
        protected override int MarkDuration => 5 * 60;
        protected override Color SpiritGlowColor => new Color(150, 230, 255);
        private const int AttackCooldown = 120;
        private const float ShootRange = 320f;
        public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/IceElementProj";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 31;
            Projectile.height = 30;
        }
        protected override void ResetSpiritAttackState()
        {
            base.ResetSpiritAttackState();
        }
        protected override void UpdateSpiritAI(Player owner, ElementalResonatorPlayer modPlayer, NPC target)
        {
            if (target == null) return;
            float distToTarget = Vector2.Distance(Projectile.Center, target.Center);
            if (!isForceFlying)
            {
                float dx = target.Center.X - Projectile.Center.X;
                float idealDist = ShootRange * 0.7f;
                if (Math.Abs(dx) > ShootRange)
                {
                    WalkTowardTarget(target.Center, 4f);
                }
                else if (Math.Abs(dx) < 160f)
                {
                    Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, Math.Sign(Projectile.Center.X - target.Center.X) * 3f, 0.1f);
                }
            }
            else
            {
                hoverAngle += 0.03f;
                Vector2 idealPos = target.Center + new Vector2((float)Math.Cos(hoverAngle) * ShootRange, (float)Math.Sin(hoverAngle) * ShootRange * 0.5f);
                MoveToward(idealPos, 5f);
            }
            if (distToTarget >= 160f && distToTarget <= 600f)
            {
                AttackTimer++;
                if (AttackTimer >= (int)(AttackCooldown * AirAttackPenalty) && Main.myPlayer == Projectile.owner)
                {
                    AttackTimer = 0;
                    ShootIcicle(target, modPlayer, owner);
                }
            }
            else
            {
                AttackTimer = 0;
            }
        }
        private void ShootIcicle(NPC target, ElementalResonatorPlayer modPlayer, Player owner)
        {
            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            dir = dir.RotatedByRandom(MathHelper.ToRadians(6f));
            int damage = modPlayer.GetSpiritDamage(Projectile.originalDamage);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir * 12f, ModContent.ProjectileType<IceIcicleProjectile>(), damage, 2f, Projectile.owner);
            SoundEngine.PlaySound(SoundID.Item28, Projectile.position);
        }
    }
	public class LightningSpirit : ElementSpiritBase
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 28;
            Projectile.height = 32;
        }
        protected override void ResetSpiritAttackState()
        {
            base.ResetSpiritAttackState();
            isPreparingAttack = false;
            prepTimer = 0;
        }
        protected override bool IsGroundBased => false;
        protected override float BaseMarkChance => 0.05f;
        protected override int MarkBuffType => ModContent.BuffType<LightningMarkDebuff>();
        protected override int MarkDuration => 4 * 60;
        protected override Color SpiritGlowColor => new Color(255, 255, 80);
        private const int AttackCooldown = 150;
        private const int MaxLightningPoints = 4;
        private bool isPreparingAttack = false;
        private int prepTimer = 0;
        private const int PrepDuration = 60;
        private List<Vector2> lightningPoints = new List<Vector2>();
        private NPC cachedTarget;
        public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/EnergyElementProj";
        protected override void UpdateSpiritAI(Player owner, ElementalResonatorPlayer modPlayer, NPC target)
        {
            if (target == null)
            {
                isPreparingAttack = false;
                prepTimer = 0;
                lightningPoints.Clear();
                return;
            }
            AttackTimer++;
            if (isPreparingAttack)
            {
                UpdatePreparingAttack(target, modPlayer, owner);
            }
            else
            {
                Vector2 behindPos = GetBehindPosition(target);
                MoveToward(behindPos, 6f);
                if (AttackTimer >= AttackCooldown)
                {
                    StartAttack(target, modPlayer);
                }
            }
        }
        private Vector2 GetBehindPosition(NPC target)
        {
            float behindX = target.Center.X - Math.Sign(target.velocity.X != 0 ? target.velocity.X : 1f) * (target.width / 2f + 60f);
            return new Vector2(behindX, target.Center.Y - 40f);
        }
        private void StartAttack(NPC target, ElementalResonatorPlayer modPlayer)
        {
            isPreparingAttack = true;
            prepTimer = 0;
            cachedTarget = target;
            lightningPoints.Clear();
            AttackTimer = 0;
            if (Main.myPlayer != Projectile.owner) return;
            int pointCount = Main.rand.Next(2, MaxLightningPoints + 1);
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi / pointCount * i + Main.rand.NextFloat(-0.3f, 0.3f);
                float radius = target.width * 0.7f + Main.rand.NextFloat(20f, 50f);
                Vector2 point = target.Center + new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius * 0.6f);
                lightningPoints.Add(point);
            }
        }
        private void UpdatePreparingAttack(NPC target, ElementalResonatorPlayer modPlayer, Player owner)
        {
            prepTimer++;
            if (cachedTarget != null && cachedTarget.active && lightningPoints.Count > 0)
            {
                for (int i = 0; i < lightningPoints.Count; i++)
                {
                    lightningPoints[i] += cachedTarget.velocity * 0.5f;
                }
                foreach (var point in lightningPoints)
                {
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustDirect(point - new Vector2(4f), 8, 8, DustID.YellowTorch, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 150, SpiritGlowColor, 0.5f);
                        d.noGravity = true;
                    }
                }
            }
            if (prepTimer >= PrepDuration)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    FireLightning(modPlayer, owner);
                }
                isPreparingAttack = false;
                prepTimer = 0;
                lightningPoints.Clear();
            }
        }
        private void FireLightning(ElementalResonatorPlayer modPlayer, Player owner)
        {
            if (lightningPoints.Count == 0 || cachedTarget == null || !cachedTarget.active) return;
            int damage = modPlayer.GetSpiritDamage((int)(Projectile.originalDamage * 1.5f));
            for (int i = 0; i < lightningPoints.Count; i++)
            {
                Vector2 startPoint = i == 0 ? Projectile.Center : lightningPoints[i - 1];
                Vector2 endPoint = lightningPoints[i];
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), startPoint, (endPoint - startPoint).SafeNormalize(Vector2.UnitX) * 16f, ModContent.ProjectileType<LightningArcProjectile>(), damage, 3f, Projectile.owner, ai0: (endPoint - startPoint).Length());
            }
            Vector2 lastPoint = lightningPoints[lightningPoints.Count - 1];
            Vector2 finalDir = (cachedTarget.Center - lastPoint).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), lastPoint, finalDir * 18f, ModContent.ProjectileType<LightningArcProjectile>(), (int)(damage * 1.3f), 5f, Projectile.owner, ai0: (cachedTarget.Center - lastPoint).Length());
            TryApplyMark(cachedTarget, modPlayer);
            ElementalMarkGlobalNPC.CheckResonance(cachedTarget, owner);
            SoundEngine.PlaySound(SoundID.Thunder, Projectile.position);
            SpawnLightningExplosion(cachedTarget.Center);
        }
        private void SpawnLightningExplosion(Vector2 center)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustDirect(center - new Vector2(15f), 30, 30, DustID.YellowTorch, vel.X, vel.Y, 150, SpiritGlowColor, 0.5f);
                d.noGravity = true;
            }
        }
    }
	public class WaterBoltProjectile : ModProjectile
    {
        public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/ElementResonator_proj";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            int targetIdx = (int)Projectile.ai[0];
            if (targetIdx >= 0 && targetIdx < Main.maxNPCs && Main.npc[targetIdx].active)
            {
                NPC target = Main.npc[targetIdx];
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 12f, 0.06f);
            }
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, -Projectile.velocity.X * 0.2f + Main.rand.NextFloat(-0.5f, 0.5f), -Projectile.velocity.Y * 0.2f + Main.rand.NextFloat(-0.5f, 0.5f), 150, new Color(30, 120, 255), 1.2f);
                d.noGravity = true;
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var modPlayer = owner.GetModPlayer<ElementalResonatorPlayer>();
            float chance = 0.02f + modPlayer.whipMarkBonus;
            if (Main.rand.NextFloat() < chance)
            {
                target.AddBuff(ModContent.BuffType<WaterMarkDebuff>(), 6 * 60);
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust d = Dust.NewDustDirect(target.Center - new Vector2(8f), 16, 16, DustID.MagicMirror, vel.X, vel.Y, 150, new Color(30, 120, 255), 1.2f);
                    d.noGravity = true;
                }
            }
            ElementalMarkGlobalNPC.CheckResonance(target, owner);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 150, new Color(30, 120, 255), 1f);
            }
        }
    }
	public class IceSpikeProjectile : ModProjectile
    {
        public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/ElementResonator_proj";
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 45;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.92f;
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f), 150, new Color(150, 230, 255), 1.3f);
                d.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust d2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0f, -0.5f, 200, Color.White, 0.8f);
                d2.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 3f);
                vel.Y = Math.Abs(vel.Y) * -1f;
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, vel.X, vel.Y, 150, new Color(150, 230, 255), 1.1f);
            }
        }
    }
	public class LightningArcProjectile : ModProjectile
    {
        public override string Texture => "Jarkos/items/weapons/summon/staff/ElementResonator/ElementResonator_proj";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        private Vector2[] lightningPoints;
        public override void AI()
        {
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float totalDist = Projectile.ai[0];
            if (lightningPoints == null)
            {
                int steps = Math.Max(2, (int)(totalDist / 15f));
                lightningPoints = new Vector2[steps + 1];
                lightningPoints[0] = Projectile.Center;
                for (int i = 1; i < steps; i++)
                {
                    float t = i / (float)steps;
                    Vector2 pos = Projectile.Center + dir * totalDist * t;
                    pos += dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-15f, 15f);
                    lightningPoints[i] = pos;
                }
                lightningPoints[steps] = Projectile.Center + dir * totalDist;
                for (int i = 0; i < lightningPoints.Length; i++)
                {
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                        new Spark(lightningPoints[i], vel, new Color(255, 255, 80), 30, Main.rand.NextFloat(0.3f, 0.6f), 1f).Spawn();
                    }
                }
            }
            if (Main.rand.NextBool(3))
            {
                int pointIdx = Main.rand.Next(lightningPoints.Length);
                Dust d = Dust.NewDustDirect(lightningPoints[pointIdx] - new Vector2(4f), 8, 8, DustID.YellowTorch, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 150, new Color(255, 255, 80), 0.6f);
                d.noGravity = true;
            }
            Projectile.velocity = dir * Math.Min(totalDist / 15f, 20f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (lightningPoints == null || lightningPoints.Length < 2) return false;
            Texture2D tex = TextureAssets.MagicPixel.Value;
            float opacity = Projectile.timeLeft / 15f;
            Color color = new Color(255, 255, 150, 0) * opacity;
            for (int i = 0; i < lightningPoints.Length - 1; i++)
            {
                Vector2 start = lightningPoints[i];
                Vector2 end = lightningPoints[i + 1];
                float rot = (end - start).ToRotation();
                float dist = Vector2.Distance(start, end);
                Main.EntitySpriteDraw(tex, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rot, new Vector2(0, 0.5f), new Vector2(dist, 4f), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(tex, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * opacity, rot, new Vector2(0, 0.5f), new Vector2(dist, 2f), SpriteEffects.None, 0);
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                float scale = Main.rand.NextFloat(0.3f, 0.7f);
                new Spark(target.Center, vel, new Color(255, 255, 80), 35, scale, 1.0f).Spawn();
            }
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(target.Center - new Vector2(6f), 12, 12, DustID.YellowTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 150, new Color(255, 255, 80), 0.5f);
                d.noGravity = true;
            }
        }
    }
	public class ElementResonatorWhipHook : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.aiStyle != ProjAIStyleID.Whip) return;
            Player owner = Main.player[projectile.owner];
            var modPlayer = owner.GetModPlayer<ElementalResonatorPlayer>();
            modPlayer.OnWhipHit(target);
        }
    }
	public class ElementalMarkGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => false;
        private static int FireMark => ModContent.BuffType<FireMarkDebuff>();
        private static int WaterMark => ModContent.BuffType<WaterMarkDebuff>();
        private static int EarthMark => ModContent.BuffType<EarthMarkDebuff>();
        private static int IceMark => ModContent.BuffType<IceMarkDebuff>();
        private static int LightningMark => ModContent.BuffType<LightningMarkDebuff>();
        private static Dictionary<int, int> resonanceCooldowns = new Dictionary<int, int>();
        public override void PostAI(NPC npc)
        {
            if (resonanceCooldowns.TryGetValue(npc.whoAmI, out int cd))
            {
                if (cd > 0)
                    resonanceCooldowns[npc.whoAmI] = cd - 1;
            }
        }
        public static void CheckResonance(NPC npc, Player owner)
        {
            if (!npc.active) return;
            if (resonanceCooldowns.TryGetValue(npc.whoAmI, out int cd) && cd > 0) return;
            bool hasFire = npc.HasBuff(FireMark);
            bool hasWater = npc.HasBuff(WaterMark);
            bool hasEarth = npc.HasBuff(EarthMark);
            bool hasIce = npc.HasBuff(IceMark);
            bool hasLightning = npc.HasBuff(LightningMark);
            int markCount = (hasFire ? 1 : 0) + (hasWater ? 1 : 0) + (hasEarth ? 1 : 0) + (hasIce ? 1 : 0) + (hasLightning ? 1 : 0);
            if (markCount < 2) return;
            resonanceCooldowns[npc.whoAmI] = 180;
            TriggerResonances(npc, owner, hasFire, hasWater, hasEarth, hasIce, hasLightning);
        }
        private static void TriggerResonances(NPC npc, Player owner, bool fire, bool water, bool earth, bool ice, bool lightning)
        {
            var modPlayer = owner.GetModPlayer<ElementalResonatorPlayer>();
            int baseDamage = modPlayer.GetSpiritDamage(30);
            if (fire && water)
            {
                SteamExplosion(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(FireMark));
                npc.DelBuff(npc.FindBuffIndex(WaterMark));
            }
            if (fire && ice)
            {
                ThermalShock(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(FireMark));
                npc.DelBuff(npc.FindBuffIndex(IceMark));
            }
            if (fire && earth)
            {
                LavaSpray(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(FireMark));
                npc.DelBuff(npc.FindBuffIndex(EarthMark));
            }
            if (fire && lightning)
            {
                PlasmaExplosion(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(FireMark));
                npc.DelBuff(npc.FindBuffIndex(LightningMark));
            }
            if (water && ice)
            {
                IceStorm(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(WaterMark));
                npc.DelBuff(npc.FindBuffIndex(IceMark));
            }
            if (water && earth)
            {
                MudFlow(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(WaterMark));
                npc.DelBuff(npc.FindBuffIndex(EarthMark));
            }
            if (water && lightning)
            {
                ElectricShock(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(WaterMark));
                npc.DelBuff(npc.FindBuffIndex(LightningMark));
            }
            if (earth && ice)
            {
                IcySpikes(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(EarthMark));
                npc.DelBuff(npc.FindBuffIndex(IceMark));
            }
            if (earth && lightning)
            {
                MagneticStrike(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(EarthMark));
                npc.DelBuff(npc.FindBuffIndex(LightningMark));
            }
            if (ice && lightning)
            {
                CryoDischarge(npc, baseDamage, owner);
                npc.DelBuff(npc.FindBuffIndex(IceMark));
                npc.DelBuff(npc.FindBuffIndex(LightningMark));
            }
        }
        private static void SteamExplosion(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 1.6f);
            DealAoEDamage(npc, damage, 120f, owner);
            npc.AddBuff(BuffID.OnFire, 5 * 60);
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustDirect(npc.Center - new Vector2(10f), 20, 20, DustID.Smoke, vel.X, vel.Y, 150, new Color(200, 200, 220), 1.5f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.DD2_FlameburstTowerShot, npc.position);
        }
        private static void ThermalShock(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 2.2f);
            npc.SimpleStrikeNPC(damage, 0, false, 0f, DamageClass.Summon);
            npc.AddBuff(BuffID.Frozen, 3 * 60);
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Dust d = Dust.NewDustDirect(npc.Center - new Vector2(8f), 16, 16, DustID.IceTorch, vel.X, vel.Y, 150, new Color(255, 150, 80), 1.8f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item27, npc.position);
        }
        private static void LavaSpray(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 1.4f);
            DealAoEDamage(npc, damage, 100f, owner);
            npc.AddBuff(BuffID.OnFire, 8 * 60);
            npc.AddBuff(BuffID.Slow, 4 * 60);
            for (int i = 0; i < 18; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                vel.Y = Math.Abs(vel.Y) * -1f;
                Dust.NewDustDirect(npc.Center - new Vector2(8f), 16, 16, DustID.Lava, vel.X, vel.Y, 150, Color.OrangeRed, 1.6f);
            }
        }
        private static void PlasmaExplosion(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 2.5f);
            DealAoEDamage(npc, damage, 150f, owner);
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustDirect(npc.Center - new Vector2(12f), 24, 24, DustID.YellowTorch, vel.X, vel.Y, 150, new Color(255, 200, 80), 0.2f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Thunder, npc.position);
        }
        private static void IceStorm(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 1.3f);
            DealAoEDamage(npc, damage, 130f, owner);
            npc.AddBuff(BuffID.Chilled, 6 * 60);
            npc.AddBuff(BuffID.Wet, 5 * 60);
            for (int i = 0; i < 16; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 6f);
                Dust d = Dust.NewDustDirect(npc.Center - new Vector2(8f), 16, 16, DustID.Ice, vel.X, vel.Y, 150, new Color(150, 220, 255), 1.3f);
                d.noGravity = true;
            }
        }
        private static void MudFlow(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 1.2f);
            DealAoEDamage(npc, damage, 90f, owner);
            npc.AddBuff(BuffID.Slow, 8 * 60);
            npc.AddBuff(BuffID.Wet, 4 * 60);
            for (int i = 0; i < 14; i++)
            {
                Dust.NewDustDirect(npc.Center - new Vector2(8f), 16, 16, DustID.Mud, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 0f), 150, new Color(100, 70, 40), 1.1f);
            }
        }
        private static void ElectricShock(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 2.0f);
            npc.SimpleStrikeNPC(damage, 0, false, 0f, DamageClass.Summon);
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.whoAmI == npc.whoAmI || !other.active || other.friendly) continue;
                if (Vector2.Distance(npc.Center, other.Center) < 150f)
                {
                    other.SimpleStrikeNPC((int)(damage * 0.6f), 0, false, 0f, DamageClass.Summon);
                    SpawnLightningBetween(npc.Center, other.Center);
                }
            }
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustDirect(npc.Center - new Vector2(6f), 12, 12, DustID.YellowTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 150, new Color(100, 150, 255), 0.8f);
                d.noGravity = true;
            }
        }
        private static void IcySpikes(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 1.7f);
            npc.SimpleStrikeNPC(damage, 0, true, 0f, DamageClass.Summon);
            npc.AddBuff(BuffID.Frozen, 2 * 60);
            npc.AddBuff(BuffID.Slow, 5 * 60);
            for (int i = 0; i < 14; i++)
            {
                Vector2 vel = new Vector2(0, -Main.rand.NextFloat(3f, 7f))
                    .RotatedByRandom(MathHelper.ToRadians(30f));
                Dust d = Dust.NewDustDirect(npc.Bottom - new Vector2(4f, 0f), 8, 4, DustID.Ice, vel.X, vel.Y, 150, new Color(180, 240, 255), 1.4f);
                d.noGravity = false;
            }
        }
        private static void MagneticStrike(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 1.9f);
            DealAoEDamage(npc, damage, 130f, owner);
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (!other.active || other.friendly) continue;
                float dist = Vector2.Distance(npc.Center, other.Center);
                if (dist < 150f && dist > 1f)
                {
                    Vector2 pull = Vector2.Normalize(npc.Center - other.Center) * 6f;
                    other.velocity += pull;
                }
            }
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                Dust d = Dust.NewDustDirect(npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 100f - new Vector2(4f), 8, 8, DustID.YellowTorch, -(float)Math.Cos(angle) * 5f, -(float)Math.Sin(angle) * 5f, 150, new Color(139, 90, 43), 0.3f);
                d.noGravity = true;
            }
        }
        private static void CryoDischarge(NPC npc, int baseDamage, Player owner)
        {
            int damage = (int)(baseDamage * 2.8f);
            DealAoEDamage(npc, damage, 100f, owner);
            npc.AddBuff(BuffID.Frozen, 4 * 60);
            for (int i = 0; i < 22; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Dust d = Dust.NewDustDirect(npc.Center - new Vector2(10f), 20, 20, Main.rand.NextBool() ? DustID.Clentaminator_Cyan : DustID.IceTorch, vel.X, vel.Y, 150, Main.rand.NextBool() ? new Color(255, 255, 80) : new Color(150, 230, 255), 1.8f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item27, npc.position);
            SoundEngine.PlaySound(SoundID.Thunder, npc.position);
        }
        private static void DealAoEDamage(NPC source, int damage, float radius, Player owner)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.lifeMax <= 5) continue;
                float dist = Vector2.Distance(source.Center, npc.Center);
                if (dist <= radius)
                {
                    float falloff = 1f - (dist / radius) * 0.5f;
                    int finalDamage = (int)(damage * falloff);
                    npc.SimpleStrikeNPC(finalDamage, npc.Center.X > source.Center.X ? 1 : -1, false, 4f, DamageClass.Summon);
                }
            }
        }
        private static void SpawnLightningBetween(Vector2 start, Vector2 end)
        {
            int steps = (int)(Vector2.Distance(start, end) / 16f);
            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)steps;
                Vector2 pos = Vector2.Lerp(start, end, t);
                pos += Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustDirect(pos - new Vector2(4f), 8, 8, DustID.YellowTorch, 0, 0, 150, new Color(100, 150, 255), 0.5f);
                d.noGravity = true;
            }
        }
    }
    public class IceIcicleProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.IceSpike;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Summon;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 150, Color.White, 0.9f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                float scale = Main.rand.NextFloat(0.4f, 0.8f);
                new Spark(Projectile.Center, vel, Color.White, 30, scale, 1f).Spawn();
            }
        }
    }
    public class FireTrailParticle : Solid_particle
    {
        private float waveOffset;
        private float initialX;
        private float driftX;
        public FireTrailParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
            : base(position, new Vector2(0f, velocity.Y), color, lifetime, scale, opacity, rotationSpeed)
        {
            initialX = position.X;
            driftX = velocity.X;
            waveOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);
        }
        public override void Update()
        {
            initialX += driftX;
            Position.X = initialX + (float)Math.Sin(Time * 0.15f + waveOffset) * 12f;
            Opacity = 1f - (float)Time / Lifetime;
            Scale *= 0.97f;
            Rotation += Spin;
            if (Time >= Lifetime || Opacity <= 0f)
            {
                Kill();
            }
        }
    }
}
