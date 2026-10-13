using System;
using System.Collections.Generic;
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

namespace jarkos.items.weapons.summon.staff.AntiqueFollower
{
	#region Buff
	public class Antique_totem_buff : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Antique_totem_proj>()] > 0)
                player.buffTime[buffIndex] = 18000;
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
	}
	#endregion
	#region Item
	public class Antique_totem : ModItem
	{
		public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 38;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 14;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2f;

            Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();

            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<Antique_totem_proj>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Antique_totem_buff>();

            Item.value = Item.sellPrice(0, 40, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<AntiqueBar>(), 8);
            recipe.AddIngredient(ModContent.ItemType<LunarCrystal>(), 4);
            recipe.AddIngredient(ModContent.ItemType<ElementalFluid>(), 4);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
	}
	#endregion
	#region Projectile
	public class Antique_totem_proj : ModProjectile
	{
        private enum TotemState
        {
            Idle, Attacking, Supporting
        }
        private TotemState state = TotemState.Idle;
        private int targetNPC = -1;
        private int supportTargetTotem = -1;
        private int supportCount = 0;
        private int myIndex = 0;
        private int totalTotems = 1;
        private float orbitAngle = 0f;
        private float bobPhase = 0f;
        private float currentSpeed = 0f;
        private int myOrbitIndex = 0;
        private Color totemColor;
        private Color totemBright;
        private Color totemDark;
        private bool colorChosen = false;
        private float pulsePhase = 0f;
        private float pulseIntensity = 0f;
        private float glowScale = 1f;
        private float symbolRotation = 0f;
        private float timer = 0f;
        private float runeChargeProgress = 0f;
        private const float RuneChargeTime = 80f;
        private const float RuneCooldown = 50f;
        private float runeCooldownTimer = 0f;
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
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
            InitColor();
            timer++;
            UpdatePulse();
            totalTotems = CountAllTotems(owner);
            AssignRole(owner);
            switch (state)
            {
                case TotemState.Idle:
                    OrbitAroundOwner(owner);
                    runeChargeProgress = 0f;
                    break;
                case TotemState.Attacking:
                    if (ValidTarget(targetNPC))
                    {
                        OrbitAroundEnemy(Main.npc[targetNPC]);
                        UpdateRuneAttack(owner);
                    }
                    else
                    {
                        state = TotemState.Idle;
                        targetNPC = -1;
                    }
                    break;
                case TotemState.Supporting:
                    if (supportTargetTotem >= 0 && supportTargetTotem < Main.maxProjectiles
                        && Main.projectile[supportTargetTotem].active
                        && Main.projectile[supportTargetTotem].type == Projectile.type
                        && Main.projectile[supportTargetTotem].owner == Projectile.owner)
                    {
                        OrbitAroundTotem(Main.projectile[supportTargetTotem]);
                        runeChargeProgress = 0f;
                    }
                    else
                    {
                        state = TotemState.Idle;
                        supportTargetTotem = -1;
                    }
                    break;
            }
            float bob = (float)Math.Sin(timer * 0.04f + bobPhase) * 4f;
            Projectile.position.Y += bob * 0.06f;
            symbolRotation += 0.015f + pulseIntensity * 0.01f;
            float lightBonus = state == TotemState.Attacking ? 0.15f :
                               state == TotemState.Supporting ? 0.08f : 0f;
            float lightMult = 0.3f + pulseIntensity * 0.2f + lightBonus;
            Lighting.AddLight(Projectile.Center, totemColor.R / 255f * lightMult, totemColor.G / 255f * lightMult, totemColor.B / 255f * lightMult);
            SpawnTotemDust();
        }
        private void AssignRole(Player owner)
        {
            List<Projectile> allTotems = GetAllTotems(owner);
            List<NPC> enemies = GetNearbyEnemies(owner, 850f);
            if (enemies.Count == 0)
            {
                state = TotemState.Idle;
                targetNPC = -1;
                supportTargetTotem = -1;
                return;
            }
            int manualTarget = -1;
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC mt = Main.npc[owner.MinionAttackTargetNPC];
                if (mt.active && !mt.friendly && !mt.dontTakeDamage
                    && Vector2.Distance(owner.Center, mt.Center) < 950f)
                    manualTarget = owner.MinionAttackTargetNPC;
            }
            Dictionary<int, int> enemyAttackers = new Dictionary<int, int>();
            HashSet<int> takenEnemies = new HashSet<int>();
            foreach (var totem in allTotems)
            {
                if (totem.whoAmI == Projectile.whoAmI) continue;
                var otherMinion = totem.ModProjectile as Antique_totem_proj;
                if (otherMinion == null) continue;
                if (otherMinion.state == TotemState.Attacking && ValidTarget(otherMinion.targetNPC))
                {
                    if (!enemyAttackers.ContainsKey(otherMinion.targetNPC))
                        enemyAttackers[otherMinion.targetNPC] = totem.whoAmI;
                    takenEnemies.Add(otherMinion.targetNPC);
                }
            }
            if (state == TotemState.Attacking && ValidTarget(targetNPC))
            {
                if (!enemyAttackers.ContainsKey(targetNPC))
                    enemyAttackers[targetNPC] = Projectile.whoAmI;
                takenEnemies.Add(targetNPC);
            }
            int myOrder = 0;
            foreach (var totem in allTotems)
            {
                if (totem.whoAmI == Projectile.whoAmI) break;
                myOrder++;
            }
            if (manualTarget >= 0)
            {
                if (!enemyAttackers.ContainsKey(manualTarget))
                {
                    if (myOrder == 0 || state == TotemState.Idle)
                    {
                        state = TotemState.Attacking;
                        targetNPC = manualTarget;
                        supportTargetTotem = -1;
                        return;
                    }
                }
            }
            List<int> freeEnemies = new List<int>();
            foreach (var enemy in enemies)
            {
                if (!takenEnemies.Contains(enemy.whoAmI))
                    freeEnemies.Add(enemy.whoAmI);
            }
            if (state != TotemState.Attacking && freeEnemies.Count > 0)
            {
                float bestDist = float.MaxValue;
                int bestEnemy = -1;
                foreach (int eid in freeEnemies)
                {
                    float d = Vector2.Distance(Projectile.Center, Main.npc[eid].Center);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestEnemy = eid;
                    }
                }
                if (bestEnemy >= 0)
                {
                    state = TotemState.Attacking;
                    targetNPC = bestEnemy;
                    supportTargetTotem = -1;
                    return;
                }
            }
            if (state == TotemState.Attacking && ValidTarget(targetNPC))
                return;
            if (enemyAttackers.Count > 0)
            {
                float bestDist = float.MaxValue;
                int bestTotem = -1;
                foreach (var kvp in enemyAttackers)
                {
                    int totemID = kvp.Value;
                    if (totemID == Projectile.whoAmI) continue;
                    if (!Main.projectile[totemID].active) continue;
                    float d = Vector2.Distance(Projectile.Center, Main.projectile[totemID].Center);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestTotem = totemID;
                    }
                }
                if (bestTotem >= 0)
                {
                    state = TotemState.Supporting;
                    supportTargetTotem = bestTotem;
                    targetNPC = -1;
                    return;
                }
            }
            state = TotemState.Idle;
            targetNPC = -1;
            supportTargetTotem = -1;
        }
        private void OrbitAroundOwner(Player owner)
        {
            float radius = 60f + totalTotems * 8f;
            orbitAngle += 0.02f;
            int idleIndex = 0, idleCount = 0;
            foreach (var t in GetAllTotems(owner))
            {
                var m = t.ModProjectile as Antique_totem_proj;
                if (m != null && m.state == TotemState.Idle)
                {
                    if (t.whoAmI == Projectile.whoAmI) idleIndex = idleCount;
                    idleCount++;
                }
            }
            idleCount = Math.Max(idleCount, 1);
            float angleOffset = MathHelper.TwoPi / idleCount * idleIndex;
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(orbitAngle + angleOffset) * radius, (float)Math.Sin(orbitAngle + angleOffset) * radius * 0.4f - 45f);
            SmoothMove(targetPos, owner.Center);
        }
        private void OrbitAroundEnemy(NPC enemy)
        {
            float radius = 75f + enemy.width * 0.3f;
            orbitAngle += 0.035f;
            Player owner = Main.player[Projectile.owner];
            int aroundCount = 0, aroundIndex = 0;
            foreach (var t in GetAllTotems(owner))
            {
                var m = t.ModProjectile as Antique_totem_proj;
                if (m != null && m.state == TotemState.Attacking && m.targetNPC == targetNPC)
                {
                    if (t.whoAmI == Projectile.whoAmI) aroundIndex = aroundCount;
                    aroundCount++;
                }
            }
            aroundCount = Math.Max(aroundCount, 1);
            float angleOffset = MathHelper.TwoPi / aroundCount * aroundIndex;
            Vector2 targetPos = enemy.Center + new Vector2((float)Math.Cos(orbitAngle + angleOffset) * radius, (float)Math.Sin(orbitAngle + angleOffset) * radius * 0.5f - 20f);
            SmoothMove(targetPos, owner.Center);
        }
        private void OrbitAroundTotem(Projectile leader)
        {
            Player owner = Main.player[Projectile.owner];
            float radius = 40f;
            orbitAngle += 0.05f;
            int supCount = 0, supIndex = 0;
            foreach (var t in GetAllTotems(owner))
            {
                var m = t.ModProjectile as Antique_totem_proj;
                if (m != null && m.state == TotemState.Supporting && m.supportTargetTotem == leader.whoAmI)
                {
                    if (t.whoAmI == Projectile.whoAmI) supIndex = supCount;
                    supCount++;
                }
            }
            supCount = Math.Max(supCount, 1);
            float angleOffset = MathHelper.TwoPi / supCount * supIndex;
            Vector2 targetPos = leader.Center + new Vector2((float)Math.Cos(orbitAngle + angleOffset) * radius, (float)Math.Sin(orbitAngle + angleOffset) * radius * 0.5f);
            SmoothMove(targetPos, owner.Center);
        }
        private void SmoothMove(Vector2 targetPos, Vector2 ownerCenter)
        {
            Vector2 toTarget = targetPos - Projectile.Center;
            float dist = toTarget.Length();
            float distToOwner = Vector2.Distance(Projectile.Center, ownerCenter);
            float baseSpeed, lerpFactor;
            if (dist > 500f) { baseSpeed = 30f; lerpFactor = 0.14f; }
            else if (dist > 250f) { baseSpeed = 22f; lerpFactor = 0.11f; }
            else if (dist > 100f) { baseSpeed = 14f; lerpFactor = 0.09f; }
            else if (dist > 40f) { baseSpeed = 8f; lerpFactor = 0.07f; }
            else { baseSpeed = 4f; lerpFactor = 0.05f; }
            if (distToOwner > 450f)
            {
                float bonus = MathHelper.Clamp((distToOwner - 450f) / 500f, 0f, 1f);
                baseSpeed += bonus * 22f;
                lerpFactor += bonus * 0.1f;
            }
            currentSpeed = MathHelper.Lerp(currentSpeed, MathHelper.Clamp(baseSpeed, 1f, 42f), 0.07f);
            if (dist > 1f)
            {
                Vector2 desired = toTarget / dist * currentSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, lerpFactor);
            }
            else
                Projectile.velocity *= 0.85f;
            float maxSpd = currentSpeed * 1.15f;
            if (Projectile.velocity.Length() > maxSpd)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpd;
        }
        private void UpdateRuneAttack(Player owner)
        {
            if (!ValidTarget(targetNPC))
            {
                runeChargeProgress = 0f;
                runeCooldownTimer = 0f;
                return;
            }
            if (runeCooldownTimer > 0)
            {
                runeCooldownTimer--;
                return;
            }
            supportCount = 0;
            foreach (var t in GetAllTotems(owner))
            {
                var m = t.ModProjectile as Antique_totem_proj;
                if (m != null && m.state == TotemState.Supporting && m.supportTargetTotem == Projectile.whoAmI)
                    supportCount++;
            }
            float chargeSpeed = 1f + supportCount * 0.4f;
            runeChargeProgress += chargeSpeed;
            if (runeChargeProgress >= RuneChargeTime && Projectile.owner == Main.myPlayer)
            {
                NPC target = Main.npc[targetNPC];
                float dmgMult = 1.8f + supportCount * 0.5f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Bottom, Vector2.Zero, ModContent.ProjectileType<TotemPillarProjectile>(), (int)(Projectile.damage * dmgMult), 6f, Projectile.owner, 1 + supportCount);
                SoundEngine.PlaySound(SoundID.Item117 with
                {
                    Pitch = -0.3f + supportCount * 0.1f, Volume = 0.6f + supportCount * 0.08f
                }, target.Center);
                runeChargeProgress = 0f;
                runeCooldownTimer = Math.Max(RuneCooldown - supportCount * 5, 20);
            }
        }
        private List<Projectile> GetAllTotems(Player owner)
        {
            List<Projectile> totems = new List<Projectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type)
                    totems.Add(p);
            }
            return totems;
        }
        private int CountAllTotems(Player owner)
        {
            int c = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type)
                    c++;
            }
            return Math.Max(c, 1);
        }
        private List<NPC> GetNearbyEnemies(Player owner, float range)
        {
            List<NPC> list = new List<NPC>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                    continue;
                if (Vector2.Distance(owner.Center, npc.Center) < range)
                    list.Add(npc);
            }
            list.Sort((a, b) =>
                Vector2.Distance(owner.Center, a.Center)
                .CompareTo(Vector2.Distance(owner.Center, b.Center)));
            return list;
        }
        private bool ValidTarget(int npcID)
        {
            return npcID >= 0 && npcID < Main.maxNPCs
                && Main.npc[npcID].active
                && !Main.npc[npcID].friendly
                && !Main.npc[npcID].dontTakeDamage
                && Vector2.Distance(Main.player[Projectile.owner].Center, Main.npc[npcID].Center) < 950f;
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Antique_totem_buff>());
                Projectile.Kill();
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<Antique_totem_buff>()))
                Projectile.timeLeft = 2;
            else { Projectile.Kill(); return false; }
            return true;
        }
        private void InitColor()
        {
            if (colorChosen) return;
            int v = Projectile.whoAmI % 4;
            switch (v)
            {
                case 0:
                    totemColor = new Color(200, 160, 60);
                    totemBright = new Color(255, 220, 120);
                    totemDark = new Color(100, 70, 20);
                    break;
                case 1:
                    totemColor = new Color(180, 80, 40);
                    totemBright = new Color(255, 150, 80);
                    totemDark = new Color(90, 30, 15);
                    break;
                case 2:
                    totemColor = new Color(60, 180, 160);
                    totemBright = new Color(120, 240, 220);
                    totemDark = new Color(20, 80, 70);
                    break;
                default:
                    totemColor = new Color(160, 100, 200);
                    totemBright = new Color(220, 170, 255);
                    totemDark = new Color(70, 40, 100);
                    break;
            }
            colorChosen = true;
            bobPhase = Main.rand.NextFloat(MathHelper.TwoPi);
            pulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        private void UpdatePulse()
        {
            pulsePhase += 0.055f;
            float b1 = (float)Math.Sin(pulsePhase) * 0.5f + 0.5f;
            float b2 = (float)Math.Sin(pulsePhase * 3.2f) * 0.5f + 0.5f;
            float b3 = (float)Math.Sin(pulsePhase * 0.35f) * 0.5f + 0.5f;
            pulseIntensity = b1 * 0.45f + b2 * 0.25f + b3 * 0.3f;
            if (state == TotemState.Attacking)
                pulseIntensity += 0.15f;
            if (state == TotemState.Supporting)
                pulseIntensity += 0.1f;
            glowScale = MathHelper.Lerp(glowScale, 1f + pulseIntensity * 0.2f, 0.12f);
            if (Main.rand.NextBool(150))
                pulseIntensity = MathHelper.Clamp(pulseIntensity + 0.4f, 0f, 1.5f);
        }
        private void SpawnTotemDust()
        {
            float dustMult = state == TotemState.Attacking ? 1.5f : state == TotemState.Supporting ? 1.2f : 1f;
            if (Main.rand.NextFloat() < 0.2f * dustMult)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), DustID.TintableDustLighted, new Vector2(0, -Main.rand.NextFloat(0.3f, 0.8f)), 0, totemColor * 0.6f, Main.rand.NextFloat(0.3f, 0.7f));
                d.noGravity = true;
            }
            if (state == TotemState.Attacking && runeChargeProgress > 0 && runeCooldownTimer <= 0)
            {
                float charge = runeChargeProgress / RuneChargeTime;
                if (Main.rand.NextFloat() < charge * 0.5f)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-8, 8), 12), DustID.TintableDustLighted, new Vector2(0, -Main.rand.NextFloat(1f, 2.5f) * charge), 0, totemBright * charge, Main.rand.NextFloat(0.4f, 0.8f) * charge);
                    d.noGravity = true;
                }
            }
            if (state == TotemState.Supporting && supportTargetTotem >= 0
                && Main.projectile[supportTargetTotem].active && Main.rand.NextBool(6))
            {
                Vector2 toLeader = Main.projectile[supportTargetTotem].Center - Projectile.Center;
                if (toLeader.Length() > 5f)
                {
                    toLeader.Normalize();
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), DustID.TintableDustLighted, toLeader * Main.rand.NextFloat(1f, 3f), 0, totemBright * 0.6f, Main.rand.NextFloat(0.3f, 0.6f));
                    d.noGravity = true;
                }
            }
            if (Projectile.velocity.Length() > 0.5f && Main.rand.NextBool(6))
            {
                Vector2 trailingPos = Projectile.Center - Vector2.Normalize(Projectile.velocity) * 8f;
                Dust d = Dust.NewDustPerfect(trailingPos + Main.rand.NextVector2Circular(6, 6), DustID.CopperCoin, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f), 100, default, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
                d.velocity *= 0.6f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawTotemBody(sb, pixel);
            if (state == TotemState.Attacking && ValidTarget(targetNPC))
                DrawRuneCircle(sb, pixel, Main.npc[targetNPC]);
            if (state == TotemState.Supporting && supportTargetTotem >= 0
                && Main.projectile[supportTargetTotem].active)
                DrawSupportBeam(sb, pixel);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawTotemBody(SpriteBatch sb, Texture2D pixel)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float scale = Projectile.scale * glowScale;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Color glowColor = new Color(255, 130, 30) * 0.45f * scale;
            float glowScaleFactor = (Projectile.width * 2.2f) / (float)glowTex.Width;
            sb.Draw(glowTex, pos, null, glowColor, 0f, glowOrigin, glowScaleFactor * scale, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D totemTex = ModContent.Request<Texture2D>("jarkos/items/weapons/summon/staff/AntiqueFollower/Antique_totem", AssetRequestMode.ImmediateLoad).Value;
            Vector2 totemOrigin = totemTex.Size() / 2f;
            float rotation = Projectile.velocity.X * 0.04f;
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.12f);
            sb.Draw(totemTex, pos, null, new Color(255, 180, 100) * 0.4f, rotation, totemOrigin, scale * pulse * 1.1f, SpriteEffects.None, 0f);
            sb.Draw(totemTex, pos, null, Color.White, rotation, totemOrigin, scale * pulse, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawRuneCircle(SpriteBatch sb, Texture2D pixel, NPC target)
        {
            Vector2 runePos = target.Bottom - Main.screenPosition + new Vector2(0, 5);
            float time = timer;
            float charge = (runeCooldownTimer <= 0 && runeChargeProgress > 0)
                ? runeChargeProgress / RuneChargeTime : 0f;
            float circleRadius = 32f + target.width * 0.25f;
            float runeRot = time * 0.03f;
            DrawRuneRing(sb, pixel, runePos, circleRadius, runeRot, 28, 3, charge, totemColor);
            DrawRuneRing(sb, pixel, runePos, circleRadius * 0.6f, -runeRot * 1.5f, 18, 2, charge, totemBright);
            for (int i = 0; i < 4; i++)
            {
                float crossAngle = runeRot * 0.5f + i * MathHelper.PiOver2;
                float crossLen = circleRadius * 0.55f;
                float crossAlpha = 0.07f + charge * 0.18f;
                Vector2 p1 = runePos + new Vector2((float)Math.Cos(crossAngle) * crossLen, (float)Math.Sin(crossAngle) * crossLen * 0.3f);
                Vector2 p2 = runePos - new Vector2((float)Math.Cos(crossAngle) * crossLen, (float)Math.Sin(crossAngle) * crossLen * 0.3f);
                Vector2 dir = p2 - p1;
                float len = dir.Length();
                if (len < 0.5f) continue;
                sb.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), totemBright * crossAlpha, dir.ToRotation(), new Vector2(0f, 0.5f), new Vector2(len, 1f + charge * 0.5f), SpriteEffects.None, 0f);
            }
            float centerSize = 3f + charge * 7f + (float)Math.Sin(time * 0.2f) * 1.5f;
            sb.Draw(pixel, runePos, new Rectangle(0, 0, 1, 1), Color.White * (0.15f + charge * 0.45f), 0f, new Vector2(0.5f, 0.5f), new Vector2(centerSize, centerSize * 0.3f), SpriteEffects.None, 0f);
            if (supportCount > 0)
            {
                float bonusGlow = 20f + supportCount * 8f;
                float bonusPulse = (float)Math.Sin(time * 0.15f) * 0.1f + 0.15f;
                sb.Draw(pixel, runePos, new Rectangle(0, 0, 1, 1), totemBright * bonusPulse * (supportCount * 0.15f), time * 0.02f, new Vector2(0.5f, 0.5f), new Vector2(bonusGlow, bonusGlow * 0.3f), SpriteEffects.None, 0f);
            }
        }
        private void DrawRuneRing(SpriteBatch sb, Texture2D pixel, Vector2 center, float radius, float rotation, int segments, int skipEvery, float charge, Color color)
        {
            for (int i = 0; i < segments; i++)
            {
                if (i % skipEvery == 0) continue;
                float a1 = rotation + (float)i / segments * MathHelper.TwoPi;
                float a2 = rotation + (float)(i + 1) / segments * MathHelper.TwoPi;
                Vector2 p1 = center + new Vector2((float)Math.Cos(a1) * radius, (float)Math.Sin(a1) * radius * 0.3f);
                Vector2 p2 = center + new Vector2((float)Math.Cos(a2) * radius, (float)Math.Sin(a2) * radius * 0.3f);
                Vector2 dir = p2 - p1;
                float len = dir.Length();
                if (len < 0.3f) continue;
                float segPulse = (float)Math.Sin(timer * 0.15f + (float)i / segments * 8f) * 0.15f + 0.85f;
                float alpha = (0.12f + charge * 0.25f) * segPulse;
                sb.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), color * alpha, dir.ToRotation(), new Vector2(0f, 0.5f), new Vector2(len, 1.5f + charge), SpriteEffects.None, 0f);
            }
        }
        private void DrawSupportBeam(SpriteBatch sb, Texture2D pixel)
        {
            Projectile leader = Main.projectile[supportTargetTotem];
            Vector2 from = Projectile.Center;
            Vector2 to = leader.Center;
            Vector2 direction = to - from;
            float totalLen = direction.Length();
            if (totalLen < 5f) return;
            direction.Normalize();
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            int segments = Math.Clamp((int)(totalLen / 5f), 3, 60);
            float time = timer;
            Vector2[] points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = from + direction * totalLen * t;
                float wave1 = (float)Math.Sin(time * 0.15f + t * 10f) * 4f;
                float wave2 = (float)Math.Sin(time * 0.25f + t * 7f + 1.5f) * 2f;
                float wave3 = (float)Math.Sin(time * 0.38f + t * 16f + 3f) * 1f;
                float edgeFade = (float)Math.Sin(t * MathHelper.Pi);
                points[i] = basePos + perp * (wave1 + wave2 + wave3) * edgeFade * 0.15f;
            }
            Color[] colors = { totemDark, totemColor, totemBright, Color.Lerp(totemBright, Color.White, 0.4f) };
            float[] widths = { 4.5f, 2.7f, 1.5f, 0.5f };
            float[] alphas = { 0.06f, 0.14f, 0.25f, 0.45f };
            for (int layer = 0; layer < colors.Length; layer++)
            {
                for (int i = 0; i < segments; i++)
                {
                    Vector2 start = points[i] - Main.screenPosition;
                    Vector2 end = points[i + 1] - Main.screenPosition;
                    Vector2 dir = end - start;
                    float len = dir.Length();
                    if (len < 0.5f) continue;
                    float t = (float)i / segments;
                    float edgeFade = (float)Math.Pow(Math.Sin(t * MathHelper.Pi), 0.6f);
                    float shimmer = (float)Math.Sin(time * 0.2f + t * 5f + layer * 0.6f) * 0.12f + 0.88f;
                    float w = widths[layer] * edgeFade * shimmer;
                    float a = alphas[layer] * edgeFade * shimmer;
                    float run1 = (float)Math.Sin(time * 0.45f - t * 12f);
                    if (run1 > 0.4f)
                    {
                        float boost = (run1 - 0.4f) / 0.6f;
                        a += boost * 0.15f;
                        w += boost * 2f;
                    }
                    float run2 = (float)Math.Sin(time * 0.35f + t * 10f + 2f);
                    if (run2 > 0.5f)
                    {
                        float boost = (run2 - 0.5f) * 2f;
                        a += boost * 0.08f;
                        w += boost * 1f;
                    }
                    if (a < 0.003f || w < 0.2f) continue;
                    sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), colors[layer] * a, dir.ToRotation(), new Vector2(0f, 0.5f), new Vector2(len, w), SpriteEffects.None, 0f);
                }
            }
            for (int i = 0; i < segments; i += 3)
            {
                float t = (float)i / segments;
                float edgeFade = (float)Math.Sin(t * MathHelper.Pi);
                float flicker = (float)Math.Sin(time * 0.45f + i * 2.1f);
                if (flicker < 0.35f) continue;
                float sparkSize = (1.5f + flicker * 1.5f) * edgeFade;
                float sparkAlpha = flicker * 0.3f * edgeFade;
                sb.Draw(pixel, points[i] - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * sparkAlpha, 0f, new Vector2(0.5f, 0.5f), new Vector2(sparkSize), SpriteEffects.None, 0f);
            }
            for (int e = 0; e < 2; e++)
            {
                Vector2 endPos = (e == 0 ? from : to) - Main.screenPosition;
                float endPulse = (float)Math.Sin(time * 0.2f + e * 2f) * 0.1f + 0.2f;
                sb.Draw(pixel, endPos, new Rectangle(0, 0, 1, 1), totemBright * endPulse, time * 0.03f, new Vector2(0.5f, 0.5f), new Vector2(12f, 12f), SpriteEffects.None, 0f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 18; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, Main.rand.NextVector2Circular(4, 4), 0, totemColor, Main.rand.NextFloat(0.7f, 1.5f));
                d.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, totemColor.R / 255f * 0.6f, totemColor.G / 255f * 0.6f, totemColor.B / 255f * 0.6f);
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
		#endregion
	}
	#region Pillar Totem
	public class TotemPillarProjectile : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/Empty_proj";
        private float maxLife = 40f;
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] == 1)
            {
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = -0.2f }, Projectile.Center);
                int totemCount = Math.Max((int)Projectile.ai[0], 1);
                int particleCount = 15 + totemCount * 5;
                for (int i = 0; i < particleCount; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(4f, 12f));
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, vel, 0, new Color(200, 160, 60), Main.rand.NextFloat(1.2f, 2.2f));
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
                for (int i = 0; i < 10; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2, 2), -Main.rand.NextFloat(6, 14));
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, vel, 0, new Color(255, 220, 120), Main.rand.NextFloat(0.8f, 1.5f));
                    d.noGravity = true;
                }
            }
            float progress = Projectile.localAI[0] / maxLife;
            float lightFade = Math.Max(1f - progress, 0);
            Lighting.AddLight(Projectile.Center - new Vector2(0, 50), 0.6f * lightFade, 0.45f * lightFade, 0.15f * lightFade);
            Lighting.AddLight(Projectile.Center - new Vector2(0, 100), 0.4f * lightFade, 0.3f * lightFade, 0.1f * lightFade);
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-15, 15), 0), DustID.TintableDustLighted, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(3, 7)), 0, new Color(200, 160, 60) * lightFade, Main.rand.NextFloat(0.6f, 1.3f));
                d.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 180);
            target.AddBuff(BuffID.OnFire3, 120);
            target.AddBuff(BuffID.Slow, 120);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float progress = MathHelper.Clamp(Projectile.localAI[0] / maxLife, 0, 1);
            float rise = (float)Math.Sin(MathHelper.Clamp(progress * 2.5f, 0, 1) * MathHelper.PiOver2);
            float fade = progress > 0.4f ? (1f - (progress - 0.4f) / 0.6f) : 1f;
            fade = Math.Max(fade, 0);
            float pillarHeight = 160f * rise;
            int totemCount = Math.Max((int)Projectile.ai[0], 1);
            float powerScale = 1f + (totemCount - 1) * 0.15f;
            Vector2 basePos = Projectile.Center - Main.screenPosition;
            float time = Projectile.localAI[0];
            Color[] colors = {
                new Color(80, 50, 15), new Color(180, 130, 40), new Color(240, 190, 70), new Color(255, 230, 150), new Color(255, 250, 230), };
            float[] layerWidths = { 28f, 20f, 13f, 7f, 3f };
            float[] layerAlphas = { 0.08f, 0.15f, 0.28f, 0.45f, 0.7f };
            for (int layer = 0; layer < colors.Length; layer++)
            {
                int segments = 14;
                for (int s = 0; s < segments; s++)
                {
                    float segT = (float)s / segments;
                    float segNextT = (float)(s + 1) / segments;
                    float y1 = -pillarHeight * segT;
                    float y2 = -pillarHeight * segNextT;
                    float widthFade1 = (float)Math.Pow(1f - segT, 0.7f);
                    float widthFade2 = (float)Math.Pow(1f - segNextT, 0.7f);
                    float waveX1 = (float)Math.Sin(time * 0.12f + segT * 5f + layer * 0.5f) * 3f * widthFade1;
                    float waveX2 = (float)Math.Sin(time * 0.12f + segNextT * 5f + layer * 0.5f) * 3f * widthFade2;
                    Vector2 segStart = basePos + new Vector2(waveX1, y1);
                    Vector2 segEnd = basePos + new Vector2(waveX2, y2);
                    Vector2 dir = segEnd - segStart;
                    float len = dir.Length();
                    if (len < 0.3f) continue;
                    float w = layerWidths[layer] * widthFade1 * fade * powerScale;
                    float a = layerAlphas[layer] * fade;
                    float shimmer = (float)Math.Sin(time * 0.2f + s * 0.7f + layer) * 0.12f + 0.88f;
                    a *= shimmer;
                    sb.Draw(pixel, segStart, new Rectangle(0, 0, 1, 1), colors[layer] * a, dir.ToRotation(), new Vector2(0f, 0.5f), new Vector2(len, w), SpriteEffects.None, 0f);
                }
            }
            float baseGlow = 35f * fade * powerScale;
            sb.Draw(pixel, basePos, new Rectangle(0, 0, 1, 1), new Color(240, 190, 70) * fade * 0.2f, 0f, new Vector2(0.5f, 0.5f), new Vector2(baseGlow, baseGlow * 0.25f), SpriteEffects.None, 0f);
            float runeSize = 30f * fade * powerScale;
            int runeSegs = 24;
            float runeRot = time * 0.05f;
            for (int i = 0; i < runeSegs; i++)
            {
                if (i % 3 == 0) continue;
                float a1 = runeRot + (float)i / runeSegs * MathHelper.TwoPi;
                float a2 = runeRot + (float)(i + 1) / runeSegs * MathHelper.TwoPi;
                Vector2 p1 = basePos + new Vector2((float)Math.Cos(a1) * runeSize, (float)Math.Sin(a1) * runeSize * 0.25f);
                Vector2 p2 = basePos + new Vector2((float)Math.Cos(a2) * runeSize, (float)Math.Sin(a2) * runeSize * 0.25f);
                Vector2 dir = p2 - p1;
                float len = dir.Length();
                if (len < 0.3f) continue;
                sb.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), new Color(255, 220, 120) * fade * 0.25f, dir.ToRotation(), new Vector2(0f, 0.5f), new Vector2(len, 2f), SpriteEffects.None, 0f);
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
	#endregion
}
