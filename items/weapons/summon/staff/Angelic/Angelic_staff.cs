using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.items.materials;
using Terraria.Audio;
using System.IO;

namespace jarkos.items.weapons.summon.staff.Angelic
{
    #region Buff
    public class Angelic_buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Angelic_proj>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    #endregion
    #region Item
    public class Angelic_staff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 65;
            Item.knockBack = 5f;
            Item.mana = 15;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<Angelic_buff>();
            Item.shoot = ModContent.ProjectileType<Angelic_proj>();
            Item.shootSpeed = 0f;
            Item.autoReuse = false;

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile proj = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, ModContent.ProjectileType<Angelic_proj>(), damage, knockback, player.whoAmI);
            proj.originalDamage = Item.damage;
            return false;
        }
        public override void AddRecipes()
		{
			CreateRecipe()
			.AddIngredient(ItemID.SpectreBar, 8)
            .AddIngredient(ItemID.PixieDust, 20)
			.AddIngredient(ModContent.ItemType<ElementalPowder>(), 2)
			.AddIngredient(ItemID.SoulofLight, 4)
			.AddTile(TileID.MythrilAnvil)
        	.Register();
		}
    }
    #endregion
    #region MinionProjectile

    public class Angelic_proj : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/summon/staff/Angelic/Angelic_proj_main";

        private enum AttackState
        {
            OrbitTarget = 0, DashWindup = 1, Dash = 2, Explosion = 3, LaserReposition = 4, LaserChannel = 5
        }
        private AttackState currentAttackState = AttackState.OrbitTarget;
        private int combatTimer = 0;
        private int idleBeamCooldown = 1200;
        private bool isIdleBeaming = false;
        private int idleBeamTimer = 0;

        private Vector2 dashDirection = Vector2.Zero;
        private Vector2 laserDirection = Vector2.Zero;
        private bool initialized = false;

        private int[] frameHistory = new int[6];
        private int[] trailFrameHistory = new int[10];
        private ref float AuraCooldown => ref Projectile.ai[0];
        public const int AuraCooldownMax = 900;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 32;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 168;
            Projectile.height = 68;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.timeLeft = 18000;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)currentAttackState);
            writer.Write((short)combatTimer);
            writer.Write((short)idleBeamCooldown);
            writer.Write(isIdleBeaming);
            writer.Write((short)idleBeamTimer);
            writer.Write(dashDirection.X);
            writer.Write(dashDirection.Y);
            writer.Write(laserDirection.X);
            writer.Write(laserDirection.Y);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            currentAttackState = (AttackState)reader.ReadByte();
            combatTimer = reader.ReadInt16();
            idleBeamCooldown = reader.ReadInt16();
            isIdleBeaming = reader.ReadBoolean();
            idleBeamTimer = reader.ReadInt16();
            dashDirection.X = reader.ReadSingle();
            dashDirection.Y = reader.ReadSingle();
            laserDirection.X = reader.ReadSingle();
            laserDirection.Y = reader.ReadSingle();
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            bool isAngelPlayer = player.name != null && player.name.ToLowerInvariant().Contains("angel");
            int targetW = isAngelPlayer ? 184 : 168;
            int targetH = isAngelPlayer ? 82 : 68;
            if (Projectile.width != targetW || Projectile.height != targetH)
            {
                Projectile.width = targetW;
                Projectile.height = targetH;
            }

            if (!initialized)
            {
                initialized = true;
                idleBeamCooldown = Main.rand.Next(900, 1800);
                for (int i = 0; i < frameHistory.Length; i++)
                {
                    frameHistory[i] = Projectile.frame;
                }
                for (int i = 0; i < trailFrameHistory.Length; i++)
                {
                    trailFrameHistory[i] = Projectile.frame;
                }
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 32;
            }
            for (int i = frameHistory.Length - 1; i > 0; i--)
            {
                frameHistory[i] = frameHistory[i - 1];
            }
            frameHistory[0] = Projectile.frame;
            for (int i = trailFrameHistory.Length - 1; i > 0; i--)
            {
                trailFrameHistory[i] = trailFrameHistory[i - 1];
            }
            trailFrameHistory[0] = Projectile.frame;
            if (player.HasBuff(ModContent.BuffType<Angelic_buff>()))
            {
                Projectile.timeLeft = 2;
            }
            if (AuraCooldown > 0f)
            {
                AuraCooldown--;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                bool rightClickPressed = Main.mouseRight && Main.mouseRightRelease;
                if (rightClickPressed && AuraCooldown <= 0f)
                {
                    SpawnAuraFromAllMinions(player);
                    ResetCooldownForAllMinions(player);
                }
            }
            if (Projectile.owner == Main.myPlayer)
            {
                int targetWhoAmI = -1;
                float maxDist = 900f;
                if (player.HasMinionAttackTargetNPC)
                {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    if (npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < maxDist)
                        {
                            targetWhoAmI = npc.whoAmI;
                            maxDist = dist;
                        }
                    }
                }
                if (targetWhoAmI == -1)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.CanBeChasedBy())
                        {
                            float dist = Vector2.Distance(Projectile.Center, npc.Center);
                            if (dist < maxDist)
                            {
                                targetWhoAmI = npc.whoAmI;
                                maxDist = dist;
                            }
                        }
                    }
                }
                float targetAi = targetWhoAmI + 1;
                if (Projectile.ai[1] != targetAi)
                {
                    Projectile.ai[1] = targetAi;
                    Projectile.netUpdate = true;
                }
            }

            NPC target = null;
            if (Projectile.ai[1] > 0)
            {
                target = Main.npc[(int)Projectile.ai[1] - 1];
                if (!target.active || !target.CanBeChasedBy())
                {
                    target = null;
                }
            }
            if (target != null)
            {
                if (isIdleBeaming)
                {
                    CleansePlayerDebuff(player);
                    isIdleBeaming = false;
                    idleBeamTimer = 0;
                    idleBeamCooldown = Main.rand.Next(900, 1800);
                }
                ExecuteCombatAI(target);
            }
            else
            {
                currentAttackState = AttackState.OrbitTarget;
                combatTimer = 0;
                ExecuteIdleAI(player);
            }
        }
        private void CleansePlayerDebuff(Player player)
        {
            List<int> debuffIndices = new List<int>();
            for (int b = 0; b < Player.MaxBuffs; b++)
            {
                if (player.buffType[b] > 0 && Main.debuff[player.buffType[b]])
                    debuffIndices.Add(b);
            }
            if (debuffIndices.Count > 0)
            {
                int randomDebuffIdx = debuffIndices[Main.rand.Next(debuffIndices.Count)];
                player.DelBuff(randomDebuffIdx);
                SoundEngine.PlaySound(SoundID.Item29, player.Center);
                for (int d = 0; d < 12; d++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust.NewDust(player.position, player.width, player.height, DustID.Enchanted_Gold, dustVel.X, dustVel.Y - 1f, 150, Color.White, 1.3f);
                }
                for (int i = 0; i < 4; i++)
                {
                    Vector2 start = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    new Stretching_particle(start, player.Center + new Vector2(0f, -40f), Color.Gold * 0.8f, Main.rand.Next(20, 30), 30f, Main.rand.NextFloat(4f, 8f), 0.02f).Spawn();
                }
            }
        }
        private void ExecuteIdleAI(Player player)
        {
            if (!isIdleBeaming)
            {
                idleBeamCooldown--;
                if (idleBeamCooldown <= 0)
                {
                    isIdleBeaming = true;
                    idleBeamTimer = 0;
                    SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                }
            }
            if (isIdleBeaming)
            {
                idleBeamTimer++;
                int minionCount = player.ownedProjectileCounts[Projectile.type];
                if (minionCount <= 0) minionCount = 1;
                float angleOffset = (Projectile.identity * (MathHelper.TwoPi / minionCount));
                Vector2 targetHoverPos = player.Center + new Vector2(0f, -70f).RotatedBy(angleOffset * 0.5f);
                Vector2 toHover = targetHoverPos - Projectile.Center;
                float dist = toHover.Length();
                if (dist > 300f)
                {
                    Projectile.Center = player.Center;
                    Projectile.velocity = Vector2.Zero;
                }
                else
                {
                    Projectile.velocity = (Projectile.velocity * 10f + toHover * 0.4f) / 11f;
                }
                Vector2 toPlayer = player.Center - Projectile.Center;
                Projectile.rotation = MathHelper.Clamp(toPlayer.X * 0.005f, -0.2f, 0.2f);
                Projectile.spriteDirection = toPlayer.X > 0 ? 1 : -1;
                if (Main.rand.NextBool(4))
                {
                    new Stretching_particle(Projectile.Center, player.Center, Color.Gold * 0.7f, Main.rand.Next(15, 25), toPlayer.Length(), Main.rand.NextFloat(5f, 10f), 0.02f).Spawn();
                }
                if (idleBeamTimer >= 30)
                {
                    CleansePlayerDebuff(player);
                    isIdleBeaming = false;
                    idleBeamTimer = 0;
                    idleBeamCooldown = Main.rand.Next(900, 1800);
                }
            }
            else
            {
                int minionCount = player.ownedProjectileCounts[Projectile.type];
                if (minionCount <= 0) minionCount = 1;
                float orbitSpeed = 0.02f;
                float baseAngle = (float)(Main.GameUpdateCount * orbitSpeed);
                float orbitAngle = baseAngle + (Projectile.identity * (MathHelper.TwoPi / minionCount));
                float radiusH = 75f + Projectile.minionPos * 25f;
                float radiusV = 35f + Projectile.minionPos * 10f;
                Vector2 targetPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * radiusH, (float)Math.Sin(orbitAngle) * radiusV);
                Vector2 dir = targetPos - Projectile.Center;
                float dist = dir.Length();
                if (dist > 300f)
                {
                    Projectile.Center = player.Center;
                    Projectile.velocity = Vector2.Zero;
                }
                else
                {
                    Projectile.velocity = (Projectile.velocity * 12f + dir * 0.6f) / 13f;
                }
                Projectile.rotation = MathHelper.Clamp(Projectile.velocity.X * 0.015f, -0.2f, 0.2f);
                Projectile.spriteDirection = Projectile.velocity.X > 0.5f ? 1 : (Projectile.velocity.X < -0.5f ? -1 : Projectile.spriteDirection);
                Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.6f);
            }
        }
        private void ExecuteCombatAI(NPC target)
        {
            combatTimer++;
            int baseDamage = Projectile.originalDamage;
            switch (currentAttackState)
            {
                case AttackState.OrbitTarget:
                    Projectile.damage = baseDamage;
                    float orbitRadiusH = 140f;
                    float orbitRadiusV = 60f;
                    float orbitSpeed = 0.025f;
                    float orbitAngle = (float)(Main.GameUpdateCount * orbitSpeed) + (Projectile.identity * (MathHelper.TwoPi / 3f));
                    Vector2 targetOrbitPos = target.Center + new Vector2((float)Math.Cos(orbitAngle) * orbitRadiusH, (float)Math.Sin(orbitAngle) * orbitRadiusV);
                    Vector2 dirToOrbit = targetOrbitPos - Projectile.Center;
                    float distToOrbit = dirToOrbit.Length();
                    float flySpeed = 15f;
                    if (distToOrbit > 8f)
                    {
                        dirToOrbit.Normalize();
                        Projectile.velocity = (Projectile.velocity * 14f + dirToOrbit * flySpeed) / 15f;
                    }
                    else
                    {
                        Vector2 toTarget = Projectile.Center - target.Center;
                        toTarget.Normalize();
                        Vector2 tangent = new Vector2(-toTarget.Y * (orbitRadiusH / orbitRadiusV), toTarget.X * (orbitRadiusV / orbitRadiusH));
                        if (tangent.LengthSquared() > 0.001f)
                            tangent.Normalize();
                        tangent *= flySpeed;
                        Projectile.velocity = (Projectile.velocity * 8f + tangent) / 9f;
                    }
                    Projectile.rotation = MathHelper.Clamp(Projectile.velocity.X * 0.015f, -0.25f, 0.25f);
                    Projectile.spriteDirection = Projectile.velocity.X > 0.5f ? 1 : (Projectile.velocity.X < -0.5f ? -1 : Projectile.spriteDirection);
                    Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 1.0f);
                    if (Main.rand.NextBool(18))
                    {
                        Vector2 startPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 10f);
                        Vector2 toTargetDir = target.Center - startPos;
                        float d = toTargetDir.Length();
                        if (d > 10f && d < 900f)
                        {
                            new Stretching_particle(startPos, target.Center, Color.Gold * 0.8f, Main.rand.Next(25, 35), d, Main.rand.NextFloat(8f, 14f), 0.02f).Spawn();
                        }
                    }
                    if (combatTimer >= 60)
                    {
                        currentAttackState = AttackState.DashWindup;
                        combatTimer = 0;
                        Projectile.netUpdate = true;
                    }
                    break;
                case AttackState.DashWindup:
                    Projectile.damage = baseDamage;
                    Projectile.velocity *= 0.8f;
                    Vector2 toTargetWindup = target.Center - Projectile.Center;
                    Projectile.rotation = MathHelper.Clamp(toTargetWindup.X * 0.015f, -0.3f, 0.3f);
                    Projectile.spriteDirection = toTargetWindup.X > 0 ? 1 : -1;
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                        new Stretching_particle(Projectile.Center + offset, Projectile.Center, Color.Gold * 0.9f, Main.rand.Next(10, 20), offset.Length(), Main.rand.NextFloat(3f, 6f), 0.02f).Spawn();
                    }
                    if (combatTimer >= 15)
                    {
                        currentAttackState = AttackState.Dash;
                        combatTimer = 0;
                        Vector2 dashDir = target.Center - Projectile.Center;
                        if (dashDir == Vector2.Zero) dashDir = new Vector2(0f, -1f);
                        dashDir.Normalize();
                        dashDirection = dashDir;
                        Projectile.velocity = dashDir * 24f;
                        SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);
                        Projectile.netUpdate = true;
                    }
                    break;
                case AttackState.Dash:
                    Projectile.damage = (int)(baseDamage * 1.5f);
                    Projectile.velocity = dashDirection * 24f;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                    if (Projectile.spriteDirection == -1)
                        Projectile.rotation += MathHelper.Pi;
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 trailEnd = Projectile.Center - Projectile.velocity * 1.5f;
                        new Stretching_particle(Projectile.Center, trailEnd, Color.Gold * 0.8f, Main.rand.Next(12, 18), Projectile.velocity.Length() * 1.5f, Main.rand.NextFloat(6f, 10f), 0.02f).Spawn();
                    }
                    float distToTarget = Vector2.Distance(Projectile.Center, target.Center);
                    if (distToTarget < 50f || combatTimer >= 25)
                    {
                        currentAttackState = AttackState.Explosion;
                        combatTimer = 0;
                        Projectile.velocity *= 0.2f;
                        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
                        ExplosionDamageNPCs((int)(baseDamage * 2.0f));
                        int sparkCount = Main.rand.Next(15, 22);
                        for (int i = 0; i < sparkCount; i++)
                        {
                            Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f) + dashDirection * Main.rand.NextFloat(2f, 5f);
                            new Spark(Projectile.Center, sparkVel, Color.Gold, Main.rand.Next(40, 70), Main.rand.NextFloat(0.6f, 1.4f), 1.5f, 0f).Spawn();
                        }
                        Projectile.netUpdate = true;
                    }
                    break;
                case AttackState.Explosion:
                    Projectile.damage = baseDamage;
                    Projectile.velocity *= 0.7f;
                    Projectile.rotation = Projectile.rotation * 0.8f;
                    float currentExplosionRadius = (combatTimer / 25f) * 150f;
                    if (combatTimer <= 15 && Main.rand.NextBool(3))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 edge = Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * currentExplosionRadius;
                        new Stretching_particle(edge, Projectile.Center, Color.Gold * 0.6f, Main.rand.Next(15, 25), currentExplosionRadius, Main.rand.NextFloat(4f, 8f), 0.02f).Spawn();
                    }
                    if (combatTimer >= 25)
                    {
                        currentAttackState = AttackState.LaserReposition;
                        combatTimer = 0;
                        Projectile.netUpdate = true;
                    }
                    break;
                case AttackState.LaserReposition:
                    Projectile.damage = baseDamage;
                    Vector2 toEnemy = target.Center - Projectile.Center;
                    Vector2 targetRepositionPos = target.Center - Vector2.Normalize(toEnemy) * 280f;
                    Vector2 repositionDir = targetRepositionPos - Projectile.Center;
                    float repositionDist = repositionDir.Length();
                    if (repositionDist > 10f)
                    {
                        repositionDir.Normalize();
                        Projectile.velocity = (Projectile.velocity * 10f + repositionDir * 18f) / 11f;
                    }
                    else
                    {
                        Projectile.velocity *= 0.8f;
                    }
                    Projectile.rotation = MathHelper.Clamp(Projectile.velocity.X * 0.015f, -0.2f, 0.2f);
                    Projectile.spriteDirection = toEnemy.X > 0 ? 1 : -1;
                    if (combatTimer >= 20)
                    {
                        currentAttackState = AttackState.LaserChannel;
                        combatTimer = 0;
                        Projectile.velocity = Vector2.Zero;
                        Vector2 laserDir = target.Center - Projectile.Center;
                        if (laserDir == Vector2.Zero) laserDir = new Vector2(1f, 0f);
                        laserDir.Normalize();
                        laserDirection = laserDir;
                        SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
                        Projectile.netUpdate = true;
                    }
                    break;
                case AttackState.LaserChannel:
                    Projectile.damage = baseDamage;
                    Projectile.velocity = Vector2.Zero;
                    Vector2 toTargetLaser = target.Center - Projectile.Center;
                    if (toTargetLaser != Vector2.Zero)
                    {
                        toTargetLaser.Normalize();
                        laserDirection = (laserDirection * 15f + toTargetLaser) / 16f;
                    }
                    Projectile.rotation = 0f;
                    Projectile.spriteDirection = toTargetLaser.X > 0 ? 1 : -1;
                    if (combatTimer % 9 == 0)
                    {
                        LaserDamageNPCs(laserDirection, baseDamage);
                    }
                    if (Main.rand.NextBool(3))
                    {
                        float beamLength = 500f;
                        float randomDist = Main.rand.NextFloat(50f, beamLength);
                        Vector2 startPoint = Projectile.Center + laserDirection * randomDist;
                        new Stretching_particle(startPoint, Projectile.Center, Color.Gold * 0.7f, Main.rand.Next(12, 22), randomDist, Main.rand.NextFloat(5f, 9f), 0.02f).Spawn();
                    }
                    if (combatTimer >= 45)
                    {
                        currentAttackState = AttackState.OrbitTarget;
                        combatTimer = 0;
                        Projectile.netUpdate = true;
                    }
                    break;
            }
        }
        private void ExplosionDamageNPCs(int damage)
        {
            float radius = 150f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= radius)
                {
                    npc.SimpleStrikeNPC(damage, 0, false, 0, null, false, 0, true);
                    for (int d = 0; d < 3; d++)
                    {
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Enchanted_Gold, 0f, 0f, 100, Color.White, 1.2f);
                    }
                }
            }
        }
        private void LaserDamageNPCs(Vector2 direction, int damage)
        {
            float beamLength = 500f;
            float beamWidth = 20f;
            Vector2 beamEnd = Projectile.Center + direction * beamLength;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;
                float collisionPoint = 0f;
                if (Collision.CheckAABBvLineCollision(npc.Hitbox.TopLeft(), npc.Hitbox.Size(), Projectile.Center, beamEnd, beamWidth, ref collisionPoint))
                {
                    npc.SimpleStrikeNPC(damage, 0, false, 0, null, false, 0, true);
                    for (int d = 0; d < 2; d++)
                    {
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Enchanted_Gold, 0f, 0f, 100, Color.White, 1.0f);
                    }
                }
            }
        }
        private void SpawnAuraFromAllMinions(Player player)
        {
            EntitySource_Parent source = new EntitySource_Parent(Projectile);
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI)
                    continue;
                if (p.type != ModContent.ProjectileType<Angelic_proj>())
                    continue;
                int auraDamage = Projectile.damage / 2;
                Projectile.NewProjectile(source, p.Center, Vector2.Zero, ModContent.ProjectileType<Angelic_proj_aura>(), auraDamage, 0f, player.whoAmI);
            }
        }
        private void ResetCooldownForAllMinions(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI)
                    continue;
                if (p.type != ModContent.ProjectileType<Angelic_proj>())
                    continue;
                p.ai[0] = AuraCooldownMax;
                p.netUpdate = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) => false;
        public override bool PreDraw(ref Color lightColor)
        {
            if (isIdleBeaming)
            {
                DrawHealingBeam();
            }
            if (currentAttackState == AttackState.Explosion && combatTimer > 0)
            {
                DrawCombatExplosionRing();
            }
            if (currentAttackState == AttackState.LaserChannel)
            {
                DrawHolyLaser();
            }

            Player player = Main.player[Projectile.owner];
            bool isAngelPlayer = player.name != null && player.name.ToLowerInvariant().Contains("angel");

            Texture2D texture;
            if (isAngelPlayer)
            {
                texture = ModContent.Request<Texture2D>("jarkos/items/weapons/summon/staff/Angelic/Angelic_proj_main", AssetRequestMode.ImmediateLoad).Value;
            }
            else
            {
                texture = TextureAssets.Projectile[Projectile.type].Value;
            }

            int frameHeight = texture.Height / 32;
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);

            if (Projectile.oldPos != null)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                for (int k = Projectile.oldPos.Length - 1; k >= 0; k--)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero) continue;
                    Vector2 oldDrawPos = Projectile.oldPos[k] + Projectile.Size * 0.5f - Main.screenPosition;
                    float oldRot = Projectile.oldRot[k];
                    Rectangle trailSourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
                    float progress = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                    Color ghostColor = Color.Lerp(drawColor, new Color(255, 215, 0), 0.85f) * 0.10f * progress;
                    Main.EntitySpriteDraw(texture, oldDrawPos, trailSourceRect, ghostColor, oldRot, origin, Projectile.scale * (0.6f + progress * 0.4f), effects, 0);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            int outlineFrame = frameHistory[frameHistory.Length - 1];
            Rectangle outlineSourceRect = new Rectangle(0, frameHeight * outlineFrame, texture.Width, frameHeight);
            Color outlineColor = new Color(255, 215, 0) * 0.10f;
            Vector2[] offsets = new Vector2[]
            {
                new Vector2(-1.5f, 0f), new Vector2(1.5f, 0f), new Vector2(0f, -1.5f), new Vector2(0f, 1.5f), new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f)
            };
            foreach (var offset in offsets)
            {
                Main.EntitySpriteDraw(texture, drawPos + offset, outlineSourceRect, outlineColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            }
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            if (Projectile.owner == Main.myPlayer && !Main.dedServ)
            {
                DrawCooldownBar();
            }
            return false;
        }
        private void DrawAngelicAura()
        {
            if (AngelicShaderSystem.AuraShader == null) return;
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/textures/HealNoise", AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = noise.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Effect effect = AngelicShaderSystem.AuraShader;
            effect.Parameters["uColor"]?.SetValue(new Color(255, 215, 0).ToVector3());
            effect.Parameters["uOpacity"]?.SetValue(0.4f);
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.CurrentTechnique.Passes[0].Apply();
            float drawScale = 1.8f * (184f / noise.Width) * Projectile.scale;
            Main.spriteBatch.Draw(noise, drawPos, null, Color.White, Projectile.rotation * 0.1f, origin, drawScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawHealingBeam()
        {
            Player player = Main.player[Projectile.owner];
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 start = Projectile.Center;
            Vector2 end = player.Center;
            Vector2 diff = end - start;
            float len = diff.Length();
            if (len <= 0f) return;
            float rot = diff.ToRotation();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float progress = idleBeamTimer / 30f;
            float pulseWidth = MathHelper.Lerp(12f, 4f, progress);
            Color beamColor = Color.Gold * 0.7f * (1f - progress);
            Main.EntitySpriteDraw(tex, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), beamColor, rot, new Vector2(0f, 0.5f), new Vector2(len, pulseWidth), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * 0.9f * (1f - progress), rot, new Vector2(0f, 0.5f), new Vector2(len, pulseWidth * 0.4f), SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawHolyLaser()
        {
            if (AngelicShaderSystem.LaserShader == null) return;
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/textures/HealNoise", AssetRequestMode.ImmediateLoad).Value;

            Vector2 start = Projectile.Center;
            float beamLength = 500f;
            float rot = laserDirection.ToRotation();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float progress = combatTimer / 45f;
            float baseWidth = (36f + MathF.Sin(combatTimer * 0.6f) * 6f) * (1f - progress * 0.3f);
            float opacity = 1f - progress;

            Effect effect = AngelicShaderSystem.LaserShader;
            effect.Parameters["uColor"]?.SetValue(new Color(255, 205, 30).ToVector3());
            effect.Parameters["uSecondaryColor"]?.SetValue(new Color(255, 255, 220).ToVector3());
            effect.Parameters["uOpacity"]?.SetValue(opacity);
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(noise, start - Main.screenPosition, null, Color.White, rot, new Vector2(0f, noise.Height * 0.5f), new Vector2(beamLength / noise.Width, baseWidth / noise.Height), SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawCombatExplosionRing()
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            float progress = (float)combatTimer / 25f;
            float maxExplosionRadius = 150f;
            float currentExplosionRadius = progress * maxExplosionRadius;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 origin = new Vector2(0.5f, 0.5f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Gold * 0.25f * (1f - progress), 0f, origin, new Vector2(currentExplosionRadius * 2f, currentExplosionRadius * 2f), SpriteEffects.None, 0);

            int segments = 36;
            float ringThickness = MathHelper.Lerp(8f, 2f, progress);
            Color ringColor = Color.Gold * 0.7f * (1f - progress);

            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(angle) * currentExplosionRadius, MathF.Sin(angle) * currentExplosionRadius);
                Vector2 nextPos = Projectile.Center + new Vector2(MathF.Cos(nextAngle) * currentExplosionRadius, MathF.Sin(nextAngle) * currentExplosionRadius);
                Vector2 diff = nextPos - pos;
                float segLen = diff.Length();
                float segRot = diff.ToRotation();
                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, new Rectangle(0, 0, 1, 1), ringColor, segRot, origin, new Vector2(segLen + 1f, ringThickness), SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawCooldownBar()
        {
            if (AuraCooldown <= 0f)
                return;

            Texture2D tex = TextureAssets.MagicPixel.Value;
            float progress = 1f - (AuraCooldown / AuraCooldownMax);

            Vector2 barPos = Projectile.Center - Main.screenPosition + new Vector2(-15f, -25f);
            float barWidth = 30f;
            float barHeight = 4f;

            Main.EntitySpriteDraw(tex, barPos, new Rectangle(0, 0, 1, 1), Color.Black * 0.7f, 0f, Vector2.Zero, new Vector2(barWidth, barHeight), SpriteEffects.None, 0);
            if (progress > 0f)
            {
                Main.EntitySpriteDraw(tex, barPos, new Rectangle(0, 0, 1, 1), Color.Gold * 0.9f, 0f, Vector2.Zero, new Vector2(barWidth * progress, barHeight), SpriteEffects.None, 0);
            }
        }
    }
    #endregion
    #region AuraProjectile
    public class Angelic_proj_aura : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[1];
        private bool[] cleansedPlayers = new bool[256];
        private float currentVisualRadius = 0f;
        private float maxVisualRadius = 2450f;
        private const int EXPAND_DURATION = 50;
        private const int FADE_DURATION = 40;
        private const int TOTAL_DURATION = EXPAND_DURATION + FADE_DURATION;

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TOTAL_DURATION;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
            Projectile.hide = false;
        }
        public override void AI()
        {
            Timer++;
            float expandProgress = MathHelper.Clamp(Timer / (float)EXPAND_DURATION, 0f, 1f);
            float easedExpand = 1f - MathF.Pow(1f - expandProgress, 3f);
            currentVisualRadius = maxVisualRadius * easedExpand;

            for (int p = 0; p < Main.maxPlayers; p++)
            {
                Player plr = Main.player[p];
                if (!plr.active || plr.dead || cleansedPlayers[p])
                    continue;
                float dist = Vector2.Distance(Projectile.Center, plr.Center);
                if (dist > currentVisualRadius)
                    continue;
                List<int> debuffIndices = new List<int>();
                for (int b = 0; b < Player.MaxBuffs; b++)
                {
                    if (plr.buffType[b] > 0 && Main.debuff[plr.buffType[b]])
                        debuffIndices.Add(b);
                }
                if (debuffIndices.Count > 0)
                {
                    int randomDebuffIdx = debuffIndices[Main.rand.Next(debuffIndices.Count)];
                    plr.DelBuff(randomDebuffIdx);
                    for (int d = 0; d < 8; d++)
                    {
                        Dust.NewDust(plr.Center, 0, 0, DustID.Enchanted_Gold, 0f, -2f, 150, Color.White, 1.5f);
                    }
                }
                cleansedPlayers[p] = true;
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= currentVisualRadius && dist >= currentVisualRadius - 80f)
                {
                    if (Projectile.localNPCImmunity[i] <= 0)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0, null, false, 0, true);
                        Projectile.localNPCImmunity[i] = 30;
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (AngelicShaderSystem.AuraShader == null) return false;

            float progress = (float)Timer / TOTAL_DURATION;
            if (progress >= 1f) return false;

            float opacity = 1f - progress;
            opacity *= opacity;

            float expandProgress = MathHelper.Clamp(Timer / EXPAND_DURATION, 0f, 1f);
            float easedExpand = 1f - MathF.Pow(1f - expandProgress, 3f);
            float currentRadius = maxVisualRadius * easedExpand;

            if (currentRadius <= 1f) return false;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float diameter = currentRadius * 2.05f;
            Rectangle targetRect = new Rectangle((int)(drawPos.X - diameter * 0.5f), (int)(drawPos.Y - diameter * 0.5f), (int)diameter, (int)diameter);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect effect = AngelicShaderSystem.AuraShader;
            effect.Parameters["uColor"]?.SetValue(new Color(255, 215, 40).ToVector3());
            effect.Parameters["uOpacity"]?.SetValue(opacity);
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
            effect.Parameters["uProgress"]?.SetValue(1.0f);
            effect.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(pixel, targetRect, Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
    #endregion
    #region AutoAuraProjectile
    public class Angelic_proj_auto_aura : ModProjectile
    {
        private float CurrentRadius => Projectile.ai[0];
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.hide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }
        public override void AI()
        {
            float maxRadius = 2450f;
            float progress = (float)(60 - Projectile.timeLeft) / 60f;
            float currentRadius = MathHelper.Lerp(0f, maxRadius, (float)Math.Sin(progress * MathHelper.PiOver2));
            Projectile.ai[0] = currentRadius;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= currentRadius && dist >= currentRadius - 60f)
                {
                    if (Projectile.localNPCImmunity[i] <= 0)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0, null, false, 0, true);
                        Projectile.localNPCImmunity[i] = 40;
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (CurrentRadius <= 1f || AngelicShaderSystem.AuraShader == null)
                return false;

            float progress = (60 - Projectile.timeLeft) / 60f;
            float opacity = 1f - progress;
            opacity *= opacity;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float diameter = CurrentRadius * 2.05f;
            Rectangle targetRect = new Rectangle((int)(drawPos.X - diameter * 0.5f), (int)(drawPos.Y - diameter * 0.5f), (int)diameter, (int)diameter);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect effect = AngelicShaderSystem.AuraShader;
            effect.Parameters["uColor"]?.SetValue(new Color(255, 215, 40).ToVector3());
            effect.Parameters["uOpacity"]?.SetValue(opacity);
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
            effect.Parameters["uProgress"]?.SetValue(1.0f);
            effect.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(pixel, targetRect, Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
    #endregion
    #region ModPlayer
    public class AngelStaffPlayer : ModPlayer
    {
        public override void ResetEffects() { }
    }
    #endregion
    #region ShadersModSystem
    public class AngelicShaderSystem : ModSystem
    {
        public static Effect AuraShader { get; private set; }
        public static Effect LaserShader { get; private set; }
        public override void Load()
        {
            if (Main.dedServ)
                return;
            AuraShader = ModContent.Request<Effect>("jarkos/Shaders/AngelicAura", AssetRequestMode.ImmediateLoad).Value;
            LaserShader = ModContent.Request<Effect>("jarkos/Shaders/AngelicLaser", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void Unload()
        {
            AuraShader = null;
            LaserShader = null;
        }
    }
    #endregion
}
