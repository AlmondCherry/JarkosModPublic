using System;
using jarkos.items.materials;
using jarkos.items.usetiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.ChaosDevouvrer
{
	#region Buff
	public class Chaos_Devouver_buff : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            int minionType = ModContent.ProjectileType<Chaos_Devouver_proj>();
            if (player.ownedProjectileCounts[minionType] > 0)
            {
                player.buffTime[buffIndex] = 18000;
                int supportCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == player.whoAmI && p.type == minionType)
                    {
                        var minion = p.ModProjectile as Chaos_Devouver_proj;
                        if (minion != null && minion.IsSupportMode)
                            supportCount++;
                    }
                }
                if (supportCount > 0)
                {
                    player.statDefense += supportCount * 1;
                    player.lifeRegen += (int)(supportCount * 1);
                    player.lifeRegenTime += (int)(supportCount * 60);
                }
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
	public class Chaos_Devouver_staff : ModItem
	{
		public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.UseSound = SoundID.Item82;
            Item.shoot = ModContent.ProjectileType<Chaos_Devouver_proj>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Chaos_Devouver_buff>();

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 60, 0);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.mana = 0;
                Item.shoot = ProjectileID.None;
                Item.buffType = 0;
                Item.UseSound = null;
                Item.noUseGraphic = true;
            }
            else
            {
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.mana = 15;
                Item.shoot = ModContent.ProjectileType<Chaos_Devouver_proj>();
                Item.buffType = ModContent.BuffType<Chaos_Devouver_buff>();
                Item.UseSound = SoundID.Item82;
                Item.noUseGraphic = false;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                ToggleMode(player);
                return false;
            }
            player.AddBuff(ModContent.BuffType<Chaos_Devouver_buff>(), 2);
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<Chaos_Devouver_proj>(), damage, knockback, player.whoAmI);
            return false;
        }
        private void ToggleMode(Player player)
        {
            int minionType = ModContent.ProjectileType<Chaos_Devouver_proj>();
            bool anyFound = false;
            bool newMode = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == minionType)
                {
                    var minion = p.ModProjectile as Chaos_Devouver_proj;
                    if (minion != null)
                    {
                        if (!anyFound)
                        {
                            newMode = !minion.IsSupportMode;
                            anyFound = true;
                        }
                    }
                }
            }
            if (!anyFound) return;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == minionType)
                {
                    var minion = p.ModProjectile as Chaos_Devouver_proj;
                    if (minion != null)
                        minion.SetSupportMode(newMode);
                }
            }
            string modeText = newMode ? "Support mode" : "Battle mode";
            Color textColor = newMode ? new Color(80, 200, 255) : new Color(220, 50, 80);
            CombatText.NewText(player.Hitbox, textColor, modeText, true);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8);
            recipe.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
	}
	#endregion
	#region Projectile
	public class Chaos_Devouver_proj : ModProjectile
	{
        public bool IsSupportMode { get; private set; } = false;
        public void SetSupportMode(bool support)
		{
			if (IsSupportMode == support) return;
			IsSupportMode = support;
			modeTransition = 0f;
			Color flashColor = support ? new Color(80, 200, 255) : new Color(220, 50, 80);
			for (int i = 0; i < 15; i++)
			{
				float angle = MathHelper.TwoPi / 15f * i;
				float speed = Main.rand.NextFloat(2f, 5f);
				Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, vel, 0, flashColor, Main.rand.NextFloat(0.8f, 1.5f));
				d.noGravity = true;
				d.fadeIn = 1.2f;
			}
			if (support)
			{
				attackState = AttackState.Idle;
				targetNPC = -1;
				devourEnergy = 0f;
			}
		}
        private enum AttackState
        {
            Idle, Seeking, Attached, Detonating
        }
        private AttackState attackState = AttackState.Idle;
        private float modeTransition = 1f;
        private float devourEnergy = 0f;
        private const float MaxEnergy = 100f;
        private float devourPulse = 0f;
        private float frenzyBonus = 0f;
        private int targetNPC = -1;
        private float attachAngle = 0f;
        private float attachDist = 0f;
        private float orbitAngle = 0f;
        private float currentSpeed = 0f;
        private float bobPhase = 0f;
        private float timer = 0f;
        private float pulsePhase = 0f;
        private float coreRotation = 0f;
        private float coreScale = 1f;
        private float chaosDistortion = 0f;
        private const int TentacleCount = 5;
        private float[] tentaclePhases = new float[TentacleCount];
        private float[] tentacleLengths = new float[TentacleCount];
        private const int TrailMax = 25;
        private Vector2[] trailPos = new Vector2[TrailMax];
        private bool trailInit = false;
        private float shieldPulse = 0f;
        private float shieldRotation = 0f;
        private static readonly Color chaosCore = new Color(180, 40, 220);
        private static readonly Color chaosGlow = new Color(120, 20, 180);
        private static readonly Color chaosBright = new Color(255, 100, 255);
        private static readonly Color chaosDark = new Color(40, 5, 60);
        private static readonly Color devourColor = new Color(220, 50, 80);
        private static readonly Color energyColor = new Color(255, 180, 50);
        private static readonly Color supportCore = new Color(40, 160, 220);
        private static readonly Color supportGlow = new Color(20, 100, 180);
        private static readonly Color supportBright = new Color(100, 220, 255);
        private static readonly Color supportDark = new Color(5, 30, 60);
        private static readonly Color healColor = new Color(80, 255, 160);
        private static readonly Color shieldColor = new Color(60, 180, 255);
        private Color currentCore, currentGlow, currentBright, currentDark;
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner)) return;
            InitTrail();

            for (int i = TrailMax - 1; i > 0; i--)
                trailPos[i] = trailPos[i - 1];

            trailPos[0] = Projectile.Center;
            timer++;
            pulsePhase += 0.06f;
            coreRotation += IsSupportMode ? 0.015f : 0.025f;
            devourPulse += IsSupportMode ? 0.04f : 0.08f;
            shieldPulse += 0.05f;
            shieldRotation += 0.02f;
            
            float targetTransition = 1f;
            modeTransition = MathHelper.Lerp(modeTransition, targetTransition, 0.04f);
            float modeLerp = IsSupportMode ? modeTransition : 1f - modeTransition;

            modeLerp = MathHelper.Clamp(modeLerp, 0f, 1f);
            float supportLerp = IsSupportMode ? MathHelper.Clamp(modeTransition, 0, 1) : MathHelper.Clamp(1f - modeTransition, 0, 1);
            supportLerp = IsSupportMode ? 1f : 0f;
            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], supportLerp, 0.05f);

            float blend = Projectile.localAI[1];
            currentCore = Color.Lerp(chaosCore, supportCore, blend);
            currentGlow = Color.Lerp(chaosGlow, supportGlow, blend);
            currentBright = Color.Lerp(chaosBright, supportBright, blend);
            currentDark = Color.Lerp(chaosDark, supportDark, blend);
            frenzyBonus = Math.Max(frenzyBonus - 0.003f, 0f);
            chaosDistortion = IsSupportMode
                ? (float)Math.Sin(timer * 0.04f) * 0.05f
                : (float)Math.Sin(timer * 0.07f) * 0.15f
                    + (float)Math.Sin(timer * 0.19f) * 0.1f
                    + devourEnergy / MaxEnergy * 0.3f;

            float energyPercent = devourEnergy / MaxEnergy;
            float targetScale = IsSupportMode
                ? 1f + (float)Math.Sin(shieldPulse) * 0.08f
                : 1f + energyPercent * 0.5f + chaosDistortion * 0.2f;
            coreScale = MathHelper.Lerp(coreScale, targetScale, 0.08f);

            if (IsSupportMode)
                UpdateSupportMode(owner);
            else
                UpdateAttackMode(owner);

            float bob = (float)Math.Sin(timer * 0.04f + bobPhase) * (IsSupportMode ? 3f : 4f);
            Projectile.position.Y += bob * 0.06f;
            float lightI = IsSupportMode ? 0.25f + (float)Math.Sin(shieldPulse) * 0.08f : 0.3f + energyPercent * 0.4f;
            Lighting.AddLight(Projectile.Center, currentCore.R / 255f * lightI, currentCore.G / 255f * lightI, currentCore.B / 255f * lightI);

            SpawnAmbientDust(energyPercent, blend);
        }
        private void UpdateSupportMode(Player owner)
        {
            int devCount = CountDevourers(owner);
            int supportCount = CountSupportDevourers(owner);
            int myIdx = GetSupportIndex(owner);
            float radius = 50f + supportCount * 6f;
            orbitAngle += 0.03f;
            float angleOff = MathHelper.TwoPi / Math.Max(supportCount, 1) * myIdx;
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(orbitAngle + angleOff) * radius, (float)Math.Sin(orbitAngle + angleOff) * radius * 0.35f - 15f);
            SmoothMove(targetPos, owner.Center);
            if (Main.rand.NextBool(25))
            {
                Vector2 toOwner = owner.Center - Projectile.Center;
                if (toOwner.Length() > 5f)
                {
                    toOwner.Normalize();
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.TintableDustLighted, toOwner * Main.rand.NextFloat(1.5f, 3f), 0, healColor * 0.7f, Main.rand.NextFloat(0.3f, 0.6f));
                    d.noGravity = true;
                }
            }
        }
        private int CountSupportDevourers(Player owner)
        {
            int c = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type)
                {
                    var m = p.ModProjectile as Chaos_Devouver_proj;
                    if (m != null && m.IsSupportMode) c++;
                }
            }
            return Math.Max(c, 1);
        }
        private int GetSupportIndex(Player owner)
        {
            int idx = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type)
                {
                    var m = p.ModProjectile as Chaos_Devouver_proj;
                    if (m != null && m.IsSupportMode)
                    {
                        if (i == Projectile.whoAmI) return idx;
                        idx++;
                    }
                }
            }
            return 0;
        }
        private void UpdateAttackMode(Player owner)
        {
            switch (attackState)
            {
                case AttackState.Idle:
                    FindTarget(owner);
                    if (targetNPC >= 0)
                        attackState = AttackState.Seeking;
                    else
                        OrbitAroundOwner(owner);
                    break;
                case AttackState.Seeking:
                    if (!ValidTarget(targetNPC))
                    { attackState = AttackState.Idle; targetNPC = -1; break; }
                    NPC seekTarget = Main.npc[targetNPC];
                    float dist = Vector2.Distance(Projectile.Center, seekTarget.Center);
                    if (dist < 50f)
                    {
                        attackState = AttackState.Attached;
                        attachAngle = (Projectile.Center - seekTarget.Center).ToRotation();
                        attachDist = Math.Max(seekTarget.width, seekTarget.height) * 0.4f + 15f;
                        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
                        for (int i = 0; i < 12; i++)
                        {
                            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, Main.rand.NextVector2Circular(3, 3), 0, chaosCore, Main.rand.NextFloat(0.6f, 1.2f));
                            d.noGravity = true;
                        }
                    }
                    else
                        ChaseTarget(seekTarget, owner);
                    break;
                case AttackState.Attached:
                    if (!ValidTarget(targetNPC))
                    { attackState = AttackState.Idle; targetNPC = -1; devourEnergy *= 0.5f; break; }
                    UpdateAttached(owner);
                    break;
                case AttackState.Detonating:
                    attackState = AttackState.Idle;
                    targetNPC = -1;
                    break;
            }
        }
        private void UpdateAttached(Player owner)
        {
            NPC target = Main.npc[targetNPC];
            int totalTargeting = CountAttackDevourersTargeting(target, out int myIdx);
            float baseAngle = timer * 0.03f;
            float angleSpacing = MathHelper.TwoPi / totalTargeting;
            float targetAngle = baseAngle + angleSpacing * myIdx;
            Vector2 attachPos = target.Center + new Vector2((float)Math.Cos(targetAngle) * attachDist, (float)Math.Sin(targetAngle) * attachDist * 0.8f);
            Vector2 toPos = attachPos - Projectile.Center;
            if (toPos.Length() > 2f)
            { toPos.Normalize(); Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPos * 7f, 0.15f); }
            else Projectile.velocity *= 0.8f;
            int devCount = CountDevourers(owner);
            float devourSpeed = 0.5f + devCount * 0.15f + frenzyBonus * 2f;
            devourEnergy += devourSpeed;
            if ((int)timer % 15 == 0 && Projectile.owner == Main.myPlayer)
            {
                float dmgMult = 0.8f + devourEnergy / MaxEnergy * 0.5f;
                int dmg = (int)(Projectile.damage * dmgMult);
                target.SimpleStrikeNPC(dmg, 0, false, 0f, DamageClass.Summon);
                float healPercent = 0.02f + devourEnergy / MaxEnergy * 0.03f;
                int heal = Math.Max((int)(dmg * healPercent), 1);

                if(Main.rand.NextBool(4))
                owner.Heal(heal);
            }
            if (devourEnergy >= MaxEnergy)
                Detonate(owner, target);
        }
        private void Detonate(Player owner, NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.4f, Volume = 0.8f }, Projectile.Center);
             for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type && p.whoAmI != Projectile.whoAmI)
                {
                    var other = p.ModProjectile as Chaos_Devouver_proj;
                    if (other != null && !other.IsSupportMode)
                        other.frenzyBonus = MathHelper.Clamp(other.frenzyBonus + 0.5f, 0f, 1f);
                }
            }
            devourEnergy = 0f;
            attackState = AttackState.Idle;
            targetNPC = -1;
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi / 30f * i;
                float speed = Main.rand.NextFloat(4f, 12f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, vel, 0, chaosCore, Main.rand.NextFloat(1.5f, 2.5f));
                d.noGravity = true; d.fadeIn = 1.5f;
            }
            Lighting.AddLight(Projectile.Center, 1.5f, 0.5f, 1.8f);
        }
        private void OrbitAroundOwner(Player owner)
        {
            int devCount = CountDevourers(owner);
            int myIdx = GetMyIndex(owner);
            float radius = 55f + devCount * 8f;
            orbitAngle += 0.025f;
            float angleOff = MathHelper.TwoPi / Math.Max(devCount, 1) * myIdx;
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(orbitAngle + angleOff) * radius, (float)Math.Sin(orbitAngle + angleOff) * radius * 0.4f - 35f);
            targetPos += new Vector2((float)Math.Sin(timer * 0.13f + bobPhase) * 5f, (float)Math.Cos(timer * 0.11f + bobPhase) * 3f);
            SmoothMove(targetPos, owner.Center);
        }
        private void ChaseTarget(NPC target, Player owner)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            float dist = toTarget.Length();
            float chaseSpeed = MathHelper.Clamp(8f + dist * 0.03f, 8f, 25f);
            if (dist > 1f)
            {
                toTarget.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * chaseSpeed, 0.12f);
            }
            Vector2 perp = new Vector2(-toTarget.Y, toTarget.X);
            if (perp.Length() > 0) perp.Normalize();
            Projectile.velocity += perp * (float)Math.Sin(timer * 0.25f) * 0.06f;
            if (Projectile.velocity.Length() > chaseSpeed * 1.2f)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * chaseSpeed * 1.2f;
        }
        private void SmoothMove(Vector2 targetPos, Vector2 ownerCenter)
        {
            Vector2 toTarget = targetPos - Projectile.Center;
            float dist = toTarget.Length();
            float distOwner = Vector2.Distance(Projectile.Center, ownerCenter);
            float baseSpeed, lerpFactor;
            if (dist > 500f) { baseSpeed = 30f; lerpFactor = 0.14f; }
            else if (dist > 250f) { baseSpeed = 20f; lerpFactor = 0.1f; }
            else if (dist > 100f) { baseSpeed = 12f; lerpFactor = 0.08f; }
            else if (dist > 40f) { baseSpeed = 7f; lerpFactor = 0.06f; }
            else { baseSpeed = 3f; lerpFactor = 0.04f; }
            if (distOwner > 450f)
            {
                float bonus = MathHelper.Clamp((distOwner - 450f) / 500f, 0f, 1f);
                baseSpeed += bonus * 20f; lerpFactor += bonus * 0.1f;
            }
            currentSpeed = MathHelper.Lerp(currentSpeed, MathHelper.Clamp(baseSpeed, 1f, 40f), 0.07f);
            if (dist > 1f)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget / dist * currentSpeed, lerpFactor);
            else Projectile.velocity *= 0.85f;
            if (Projectile.velocity.Length() > currentSpeed * 1.15f)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * currentSpeed * 1.15f;
        }
        private void FindTarget(Player owner)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC mt = Main.npc[owner.MinionAttackTargetNPC];
                if (mt.active && !mt.friendly && !mt.dontTakeDamage && Vector2.Distance(owner.Center, mt.Center) < 800f)
                { targetNPC = owner.MinionAttackTargetNPC; return; }
            }
            float range = 650f, best = range; int closest = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5) continue;
                float d = Vector2.Distance(owner.Center, npc.Center);
                if (d < best) { best = d; closest = i; }
            }
            targetNPC = closest;
        }
        private bool ValidTarget(int id) =>
            id >= 0 && id < Main.maxNPCs && Main.npc[id].active && !Main.npc[id].friendly && !Main.npc[id].dontTakeDamage;
        private int CountDevourers(Player owner)
        {
            int c = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type) c++;
            }
            return Math.Max(c, 1);
        }
        private int GetMyIndex(Player owner)
        {
            int idx = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type)
                { if (i == Projectile.whoAmI) return idx; idx++; }
            }
            return 0;
        }
        private int CountAttackDevourersTargeting(NPC target, out int myIndex)
        {
            myIndex = 0;
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Projectile.type && p.owner == Projectile.owner)
                {
                    var m = p.ModProjectile as Chaos_Devouver_proj;
                    if (m != null && !m.IsSupportMode && m.targetNPC == target.whoAmI && (m.attackState == AttackState.Seeking || m.attackState == AttackState.Attached))
                    {
                        if (p.whoAmI == Projectile.whoAmI)
                        {
                            myIndex = count;
                        }
                        count++;
                    }
                }
            }
            return Math.Max(count, 1);
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            { owner.ClearBuff(ModContent.BuffType<Chaos_Devouver_buff>()); Projectile.Kill(); return false; }
            if (owner.HasBuff(ModContent.BuffType<Chaos_Devouver_buff>()))
                Projectile.timeLeft = 2;
            else { Projectile.Kill(); return false; }
            return true;
        }
        private void InitTrail()
        {
            if (trailInit) return;
            for (int i = 0; i < TrailMax; i++) trailPos[i] = Projectile.Center;
            for (int i = 0; i < TentacleCount; i++)
            {
                tentaclePhases[i] = Main.rand.NextFloat(MathHelper.TwoPi);
                tentacleLengths[i] = Main.rand.NextFloat(25f, 45f);
            }
            bobPhase = Main.rand.NextFloat(MathHelper.TwoPi);
            trailInit = true;
        }
        private void SpawnAmbientDust(float energy, float blend)
        {
            Color dustColor = Color.Lerp(chaosCore, supportCore, blend);
            if (Main.rand.NextFloat() < 0.15f)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12 * coreScale, 12 * coreScale), DustID.TintableDustLighted, Main.rand.NextVector2Circular(0.8f, 0.8f), 0, dustColor * 0.6f, Main.rand.NextFloat(0.3f, 0.7f) * coreScale);
                d.noGravity = true;
            }
            if (!IsSupportMode && energy > 0.5f && Main.rand.NextFloat() < energy * 0.2f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(20f, 35f) * coreScale;
                Vector2 dustPos = Projectile.Center + new Vector2((float)Math.Cos(angle) * dist, (float)Math.Sin(angle) * dist);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.TintableDustLighted, (Projectile.Center - dustPos) * 0.06f, 0, energyColor * energy, Main.rand.NextFloat(0.3f, 0.6f));
                d.noGravity = true;
            }
            if (IsSupportMode && Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = 15f * coreScale;
                Vector2 sparkPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.TintableDustLighted, new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 0.3f, 0, shieldColor * 0.5f, 0.4f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float scale = Projectile.scale;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Color glowColor = new Color(255, 60, 180) * 0.45f * scale;
            float glowScale = (Projectile.width * 1.8f) / (float)glowTex.Width;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            sb.Draw(glowTex, pos, null, glowColor, 0f, glowOrigin, glowScale * scale, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D devourerTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 devourerOrigin = devourerTex.Size() / 2f;
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);
            float rotation;
            if (attackState == AttackState.Attached && ValidTarget(targetNPC))
            {
                rotation = (Main.npc[targetNPC].Center - Projectile.Center).ToRotation() + MathHelper.PiOver2;
            }
            else if (Projectile.velocity.Length() > 0.1f)
            {
                rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                rotation = timer * 0.02f;
            }
            sb.Draw(devourerTex, pos, null, new Color(255, 120, 200) * 0.4f, -rotation * 0.5f, devourerOrigin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            sb.Draw(devourerTex, pos, null, Color.White, rotation, devourerOrigin, scale * pulse, SpriteEffects.None, 0f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            Color deathColor = IsSupportMode ? supportCore : chaosCore;
            for (int i = 0; i < 15; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, Main.rand.NextVector2Circular(4, 4), 0, deathColor, Main.rand.NextFloat(0.7f, 1.5f));
                d.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
	}
	public class ChaoticDevourerPlayer : ModPlayer
    {
        public int supportMinionCount = 0;
        private int toggleCooldown = 0;
        private float regenAccumulator = 0f;
        public override void ResetEffects()
        {
            supportMinionCount = 0;
        }
        public override void PreUpdate()
        {
            if (toggleCooldown > 0)
                toggleCooldown--;
            if (Player.HeldItem != null
                && Player.HeldItem.type == ModContent.ItemType<Chaos_Devouver_staff>()
                && Player.controlUseTile
                && !Player.mouseInterface
                && toggleCooldown <= 0)
            {
                ToggleMode();
                toggleCooldown = 20;
            }
        }
        private void ToggleMode()
        {
            int minionType = ModContent.ProjectileType<Chaos_Devouver_proj>();
            bool anyFound = false;
            bool newMode = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == minionType)
                {
                    var minion = p.ModProjectile as Chaos_Devouver_proj;
                    if (minion != null && !anyFound)
                    {
                        newMode = !minion.IsSupportMode;
                        anyFound = true;
                        break;
                    }
                }
            }
            if (!anyFound) return;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == minionType)
                {
                    var minion = p.ModProjectile as Chaos_Devouver_proj;
                    if (minion != null)
                        minion.SetSupportMode(newMode);
                }
            }
            SoundEngine.PlaySound(newMode
                ? SoundID.Item29 with { Pitch = 0.4f, Volume = 0.7f }
                : SoundID.Item29 with { Pitch = -0.2f, Volume = 0.7f }, Player.Center);
            string modeText = newMode ? "mode: support" : "mode: attack";
            Color textColor = newMode ? new Color(80, 200, 255) : new Color(220, 50, 80);
            CombatText.NewText(Player.Hitbox, textColor, modeText, true);
        }
        public override void PostUpdateMiscEffects()
        {
            int minionType = ModContent.ProjectileType<Chaos_Devouver_proj>();
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == minionType)
                {
                    var minion = p.ModProjectile as Chaos_Devouver_proj;
                    if (minion != null && minion.IsSupportMode)
                        count++;
                }
            }
            supportMinionCount = count;
            if (count > 0)
            {
                Player.statDefense += count * 5;
            }
        }
        public override void PostUpdate()
        {
            if (supportMinionCount > 0 && Player.statLife < Player.statLifeMax2 && !Player.dead)
            {
                float regenPerTick = supportMinionCount * 0.1f / 60f;
                regenAccumulator += regenPerTick;
                if (regenAccumulator >= 1f)
                {
                    int healAmount = (int)regenAccumulator;
                    regenAccumulator -= healAmount;
                    Player.statLife = Math.Min(Player.statLife + healAmount, Player.statLifeMax2);
                    Player.HealEffect(healAmount, false);
                }
            }
            else
            {
                regenAccumulator = 0f;
            }
        }
    }
}
#endregion
