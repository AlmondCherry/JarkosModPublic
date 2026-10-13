using System;
using System.Collections.Generic;
using jarkos.items.materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.StaticCrystal
{
	#region Buff
	public class Static_crystal_buff : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Static_crystal_proj>()] > 0)
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
	public class Static_crystal_staff : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 8));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 12;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<Static_crystal_proj>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Static_crystal_buff>();

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 50, 0);
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
            recipe.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 6);
            recipe.AddIngredient(ModContent.ItemType<EssentialPowder>(), 2);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
	}
	#endregion
	#region Projectile
	public class Static_crystal_proj : ModProjectile
	{
		private int myIndex = 0;
        private int totalCrystals = 1;
        private float orbitAngle = 0f;
        private float bobOffset = 0f;
        private Color crystalColor;
        private Color crystalColorBright;
        private Color crystalColorDark;
        private bool colorChosen = false;
        private float beamTimer = 0f;
        private int targetNPC = -1;
        private float pulsePhase = 0f;
        private float pulseIntensity = 0f;
        private float pulseScale = 1f;
        private float currentSpeed = 0f;
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
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
            if (!CheckActive(owner))
                return;
            if (!colorChosen)
            {
                int variant = Projectile.whoAmI % 5;
                switch (variant)
                {
                    case 0:
                        crystalColor = new Color(80, 160, 255);
                        crystalColorBright = new Color(170, 220, 255);
                        crystalColorDark = new Color(30, 60, 140);
                        break;
                    case 1:
                        crystalColor = new Color(180, 100, 255);
                        crystalColorBright = new Color(230, 180, 255);
                        crystalColorDark = new Color(80, 30, 140);
                        break;
                    case 2:
                        crystalColor = new Color(100, 220, 255);
                        crystalColorBright = new Color(190, 245, 255);
                        crystalColorDark = new Color(30, 90, 130);
                        break;
                    case 3:
                        crystalColor = new Color(255, 140, 200);
                        crystalColorBright = new Color(255, 210, 235);
                        crystalColorDark = new Color(130, 50, 90);
                        break;
                    default:
                        crystalColor = new Color(140, 255, 200);
                        crystalColorBright = new Color(210, 255, 235);
                        crystalColorDark = new Color(50, 130, 90);
                        break;
                }
                colorChosen = true;
                bobOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                pulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            beamTimer++;
            CountCrystals(owner);
            FindTarget(owner);
            UpdatePulse();
            bool hasTarget = targetNPC >= 0 && targetNPC < Main.maxNPCs
                && Main.npc[targetNPC].active && !Main.npc[targetNPC].friendly
                && !Main.npc[targetNPC].dontTakeDamage
                && Vector2.Distance(owner.Center, Main.npc[targetNPC].Center) < 650f;
            if (hasTarget)
                MoveToOrbitTarget(Main.npc[targetNPC], owner);
            else
            {
                targetNPC = -1;
                MoveToOrbitOwner(owner);
            }
            float bob = (float)Math.Sin(beamTimer * 0.05f + bobOffset) * 3f;
            Projectile.position.Y += bob * 0.08f;
            Projectile.rotation += 0.03f + pulseIntensity * 0.02f;
            float lightMult = 0.35f + pulseIntensity * 0.3f;
            Lighting.AddLight(Projectile.Center, crystalColor.R / 255f * lightMult, crystalColor.G / 255f * lightMult, crystalColor.B / 255f * lightMult);
            SpawnCrystalDust();
            if (hasTarget && Projectile.owner == Main.myPlayer)
                ManageBeam();
        }
        private void UpdatePulse()
        {
            pulsePhase += 0.06f;
            float basePulse = (float)Math.Sin(pulsePhase) * 0.5f + 0.5f;
            float fastPulse = (float)Math.Sin(pulsePhase * 3.7f) * 0.5f + 0.5f;
            float breathPulse = (float)Math.Sin(pulsePhase * 0.4f) * 0.5f + 0.5f;
            pulseIntensity = basePulse * 0.5f + fastPulse * 0.25f + breathPulse * 0.25f;
            float targetScale = 1f + pulseIntensity * 0.25f;
            pulseScale = MathHelper.Lerp(pulseScale, targetScale, 0.15f);
            if (Main.rand.NextBool(120))
                pulseIntensity = MathHelper.Clamp(pulseIntensity + 0.5f, 0f, 1.5f);
        }
        private void MoveToOrbitOwner(Player owner)
        {
            float orbitRadius = 70f + totalCrystals * 8f;
            float orbitSpeed = 0.03f;
            float angleOffset = MathHelper.TwoPi / totalCrystals * myIndex;

            orbitAngle += orbitSpeed;
            float targetAngle = orbitAngle + angleOffset;
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(targetAngle) * orbitRadius, (float)Math.Sin(targetAngle) * orbitRadius * 0.5f - 30f);
            SmoothMoveToPosition(targetPos, owner.Center, 500f);
        }
        private void MoveToOrbitTarget(NPC target, Player owner)
        {
            float orbitRadius = 90f + totalCrystals * 5f;
            float orbitSpeed = 0.04f + totalCrystals * 0.005f;
            float angleOffset = MathHelper.TwoPi / totalCrystals * myIndex;

            orbitAngle += orbitSpeed;
            float targetAngle = orbitAngle + angleOffset;
            Vector2 targetPos = target.Center + new Vector2((float)Math.Cos(targetAngle) * orbitRadius, (float)Math.Sin(targetAngle) * orbitRadius * 0.6f);
            SmoothMoveToPosition(targetPos, owner.Center, 800f);
        }
        private void SmoothMoveToPosition(Vector2 targetPos, Vector2 ownerCenter, float maxTeleportDist)
        {
            Vector2 toTarget = targetPos - Projectile.Center;
            float dist = toTarget.Length();
            float distToOwner = Vector2.Distance(Projectile.Center, ownerCenter);
            float baseSpeed;
            float lerpFactor;
            if (dist > 600f)
            {
                baseSpeed = 30f;
                lerpFactor = 0.15f;
            }
            else if (dist > 300f)
            {
                baseSpeed = 22f;
                lerpFactor = 0.12f;
            }
            else if (dist > 150f)
            {
                baseSpeed = 16f;
                lerpFactor = 0.1f;
            }
            else if (dist > 50f)
            {
                baseSpeed = 10f;
                lerpFactor = 0.08f;
            }
            else
            {
                baseSpeed = 5f;
                lerpFactor = 0.06f;
            }
            if (distToOwner > 400f)
            {
                float ownerDistFactor = MathHelper.Clamp((distToOwner - 400f) / 600f, 0f, 1f);
                baseSpeed += ownerDistFactor * 20f;
                lerpFactor += ownerDistFactor * 0.1f;
            }
            float targetSpeed = MathHelper.Clamp(baseSpeed, 1f, 40f);
            currentSpeed = MathHelper.Lerp(currentSpeed, targetSpeed, 0.08f);
            if (dist > 1f)
            {
                Vector2 desired = toTarget / dist * currentSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, lerpFactor);
            }
            else
            {
                Projectile.velocity *= 0.85f;
            }
            float maxSpeed = currentSpeed * 1.2f;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
            }
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Static_crystal_buff>());
                Projectile.Kill();
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<Static_crystal_buff>()))
                Projectile.timeLeft = 2;
            else
            {
                Projectile.Kill();
                return false;
            }
            return true;
        }
        private void CountCrystals(Player owner)
        {
            int count = 0;
            int index = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type)
                {
                    if (i == Projectile.whoAmI)
                        myIndex = index;
                    count++;
                    index++;
                }
            }
            totalCrystals = Math.Max(count, 1);
        }
        private void FindTarget(Player owner)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC minionTarget = Main.npc[owner.MinionAttackTargetNPC];
                if (minionTarget.active && !minionTarget.friendly && !minionTarget.dontTakeDamage
                    && Vector2.Distance(owner.Center, minionTarget.Center) < 650f)
                {
                    targetNPC = owner.MinionAttackTargetNPC;
                    return;
                }
            }
            float searchRange = 550f;
            float closestDist = searchRange;
            int closest = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                    continue;
                float dist = Vector2.Distance(owner.Center, npc.Center);
                if (dist < closestDist && Collision.CanHitLine(owner.Center, 1, 1, npc.Center, 1, 1))
                {
                    closestDist = dist;
                    closest = i;
                }
            }
            targetNPC = closest;
        }
        private void ManageBeam()
        {
            bool hasBeam = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == ModContent.ProjectileType<CrystalBeamProjectile>()
                    && p.owner == Projectile.owner && (int)p.ai[0] == Projectile.whoAmI)
                {
                    hasBeam = true;
                    break;
                }
            }
            if (!hasBeam && targetNPC >= 0)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CrystalBeamProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI, targetNPC);
            }
        }
        private void SpawnCrystalDust()
        {
            float dustChance = 4f - pulseIntensity * 2f;
            dustChance = Math.Max(dustChance, 1.5f);
            if (Main.rand.NextFloat() < 1f / dustChance)
            {
                float dustScale = (0.4f + pulseIntensity * 0.5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), DustID.TintableDustLighted, Main.rand.NextVector2Circular(0.5f, 0.5f) + Projectile.velocity * 0.05f, 0, crystalColor * 0.8f, dustScale);
                d.noGravity = true;
            }
            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6), DustID.TintableDustLighted, new Vector2(0, -Main.rand.NextFloat(0.3f, 1f)), 0, Color.White * (0.3f + pulseIntensity * 0.3f), Main.rand.NextFloat(0.3f, 0.5f));
                d.noGravity = true;
            }
            if (pulseIntensity > 0.7f && Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(8f, 16f);
                Vector2 sparkPos = Projectile.Center + new Vector2((float)Math.Cos(angle) * dist, (float)Math.Sin(angle) * dist);
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.TintableDustLighted, new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 0.5f, 0, crystalColorBright * 0.8f, 0.6f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float scale = Projectile.scale * pulseScale;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Color glowColor = new Color(170, 80, 255) * 0.45f * scale;
            float glowScale = (Projectile.width * 1.8f) / (float)glowTex.Width;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            sb.Draw(glowTex, pos, null, glowColor, 0f, glowOrigin, glowScale * scale, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D crystalTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 crystalOrigin = crystalTex.Size() / 2f;
            sb.Draw(crystalTex, pos, null, new Color(200, 160, 255) * 0.5f, -beamTimer * 0.02f, crystalOrigin, scale * 1.15f, SpriteEffects.None, 0f);
            sb.Draw(crystalTex, pos, null, Color.White, Projectile.rotation, crystalOrigin, scale, SpriteEffects.None, 0f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5, 5);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, vel, 0, crystalColor, Main.rand.NextFloat(0.8f, 1.8f));
                d.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted, Main.rand.NextVector2Circular(3, 3), 0, Color.White, Main.rand.NextFloat(0.5f, 1.2f));
                d.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, crystalColor.R / 255f * 0.8f, crystalColor.G / 255f * 0.8f, crystalColor.B / 255f * 0.8f);
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
	}
	#region Beam projectile
	public class CrystalBeamProjectile : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/Empty_proj";
        private float beamTimer = 0f;
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override bool? CanCutTiles() => false;
        public override void AI()
        {
            int crystalID = (int)Projectile.ai[0];
            int targetID = (int)Projectile.ai[1];
            if (crystalID < 0 || crystalID >= Main.maxProjectiles
                || !Main.projectile[crystalID].active
                || Main.projectile[crystalID].type != ModContent.ProjectileType<Static_crystal_proj>()
                || Main.projectile[crystalID].owner != Projectile.owner)
            {
                Projectile.Kill();
                return;
            }
            if (targetID < 0 || targetID >= Main.maxNPCs
                || !Main.npc[targetID].active
                || Main.npc[targetID].friendly
                || Main.npc[targetID].dontTakeDamage
                || Vector2.Distance(Main.player[Projectile.owner].Center, Main.npc[targetID].Center) > 650f)
            {
                Projectile.Kill();
                return;
            }
            Projectile source = Main.projectile[crystalID];
            NPC target = Main.npc[targetID];
            Projectile.Center = source.Center;
            Projectile.timeLeft = 2;
            beamTimer++;
            int crystalCount = CountCrystals();
            float damageMultiplier = 1f + (crystalCount - 1) * 0.35f;
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.damage = (int)(Main.player[Projectile.owner].GetTotalDamage(DamageClass.Summon).ApplyTo(Main.projectile[crystalID].originalDamage) * damageMultiplier);
            }
            Vector2 beamDir = target.Center - source.Center;
            float beamLen = beamDir.Length();
            if (beamLen > 0)
            {
                beamDir.Normalize();
                float intensityMult = 0.2f + crystalCount * 0.08f;
                int lightPoints = Math.Max((int)(beamLen / 60f), 2);
                for (int i = 0; i < lightPoints; i++)
                {
                    float t = (float)i / lightPoints;
                    Vector2 lightPos = source.Center + beamDir * beamLen * t;
                    Lighting.AddLight(lightPos, 0.25f * intensityMult, 0.35f * intensityMult, 0.7f * intensityMult);
                }
            }
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width * 0.3f, target.height * 0.3f), DustID.TintableDustLighted, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, new Color(150, 180, 255) * 0.8f, Main.rand.NextFloat(0.5f, 1f));
                d.noGravity = true;
            }
        }
        private int CountCrystals()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner
                    && p.type == ModContent.ProjectileType<Static_crystal_proj>())
                    count++;
            }
            return Math.Max(count, 1);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int crystalID = (int)Projectile.ai[0];
            int targetID = (int)Projectile.ai[1];
            if (crystalID < 0 || crystalID >= Main.maxProjectiles || !Main.projectile[crystalID].active)
                return false;
            if (targetID < 0 || targetID >= Main.maxNPCs || !Main.npc[targetID].active)
                return false;
            Vector2 start = Main.projectile[crystalID].Center;
            Vector2 end = Main.npc[targetID].Center;
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 16f, ref point);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int crystalID = (int)Projectile.ai[0];
            int targetID = (int)Projectile.ai[1];
            if (crystalID < 0 || crystalID >= Main.maxProjectiles || !Main.projectile[crystalID].active)
                return false;
            if (targetID < 0 || targetID >= Main.maxNPCs || !Main.npc[targetID].active)
                return false;
            Projectile source = Main.projectile[crystalID];
            NPC target = Main.npc[targetID];
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            int crystalCount = CountCrystals();
            float powerFactor = 1f + (crystalCount - 1) * 0.2f;
            DrawWavyBeam(sb, pixel, source.Center, target.Center, powerFactor, false);
            if (crystalCount >= 2)
                DrawConnectionBeams(sb, pixel, source, crystalCount, powerFactor);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawWavyBeam(SpriteBatch sb, Texture2D pixel, Vector2 from, Vector2 to, float power, bool isConnection)
        {
            Vector2 direction = to - from;
            float totalLength = direction.Length();
            if (totalLength < 1f) return;
            direction.Normalize();
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            int segments = Math.Clamp((int)(totalLength / 5f), 4, 100);
            float timeMultiplier = isConnection ? 2.5f : 1f;
            float waveMultiplier = isConnection ? 0.05f : 0.15f;
            Vector2[] points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = from + direction * totalLength * t;
                float wave1 = (float)Math.Sin(beamTimer * 0.15f * timeMultiplier + t * 12f) * 6f * waveMultiplier;
                float wave2 = (float)Math.Sin(beamTimer * 0.23f * timeMultiplier + t * 8f + 1.5f) * 3f * waveMultiplier;
                float wave3 = (float)Math.Sin(beamTimer * 0.37f * timeMultiplier + t * 20f + 3f) * 1.5f * waveMultiplier;
                float edgeFade = (float)Math.Sin(t * MathHelper.Pi);
                float totalWave = (wave1 + wave2 + wave3) * edgeFade;
                points[i] = basePos + perp * totalWave;
            }
            Color[] colors;
            float[] widths;
            float[] alphas;
            if (isConnection)
            {
                colors = new Color[]                {
                    new Color(50, 70, 160), new Color(90, 130, 255), new Color(160, 195, 255), new Color(230, 240, 255), };
                widths = new float[] { 4f, 2.5f, 1.5f, 0.6f };
                alphas = new float[] { 0.1f, 0.2f, 0.35f, 0.55f };
            }
            else
            {
                colors = new Color[]                {
                    new Color(40, 50, 150), new Color(70, 100, 220), new Color(110, 160, 255), new Color(160, 200, 255), new Color(220, 235, 255), new Color(255, 255, 255), };
                widths = new float[] { 16f, 11f, 8f, 5f, 3f, 1.2f };
                alphas = new float[] { 0.06f, 0.12f, 0.22f, 0.35f, 0.5f, 0.75f };
            }
            float powerWidth = MathHelper.Clamp(power, 1f, 2.5f);
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
                    float pulseSpeed = isConnection ? 0.25f : 0.1f;
                    float pulse = (float)Math.Sin(beamTimer * pulseSpeed + t * 6f + layer * 0.5f);
                    float shimmer = pulse * 0.15f + 0.85f;
                    float w = widths[layer] * edgeFade * shimmer * powerWidth;
                    float a = alphas[layer] * edgeFade * shimmer;
                    float runSpeed = isConnection ? 0.6f : 0.3f;
                    float runFreq = isConnection ? 10f : 15f;
                    float runningWave = (float)Math.Sin(beamTimer * runSpeed - t * runFreq);
                    if (runningWave > 0.4f)
                    {
                        float boost = (runningWave - 0.4f) / 0.6f;
                        a += boost * (isConnection ? 0.1f : 0.2f);
                        w += boost * (isConnection ? 1.0f : 2.5f);
                    }
                    if (isConnection)
                    {
                        float runBack = (float)Math.Sin(beamTimer * 0.45f + t * 12f + 2f);
                        if (runBack > 0.5f)
                        {
                            float boost2 = (runBack - 0.5f) * 2f;
                            a += boost2 * 0.08f;
                            w += boost2 * 0.6f;
                        }
                    }
                    if (a < 0.003f || w < 0.2f) continue;
                    sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), colors[layer] * a, dir.ToRotation(), new Vector2(0f, 0.5f), new Vector2(len, w), SpriteEffects.None, 0f);
                }
            }
            float sparkInterval = isConnection ? 2f : 3f;
            for (int i = 0; i < segments; i += (int)sparkInterval)
            {
                float t = (float)i / segments;
                float edgeFade = (float)Math.Sin(t * MathHelper.Pi);
                float flickerSpeed = isConnection ? 0.55f : 0.35f;
                float flicker = (float)Math.Sin(beamTimer * flickerSpeed + i * 2.3f);
                if (flicker < 0.3f) continue;
                float sparkSize = (1.5f + flicker * 2f) * edgeFade * powerWidth;
                float sparkAlpha = flicker * (isConnection ? 0.5f : 0.4f) * edgeFade;
                Vector2 sparkPos = points[i] - Main.screenPosition;
                sb.Draw(pixel, sparkPos, new Rectangle(0, 0, 1, 1), Color.White * sparkAlpha, 0f, new Vector2(0.5f, 0.5f), new Vector2(sparkSize, sparkSize), SpriteEffects.None, 0f);
            }
        }
        private void DrawConnectionBeams(SpriteBatch sb, Texture2D pixel, Projectile source, int crystalCount, float power)
        {
            List<Projectile> crystals = new List<Projectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner
                    && p.type == ModContent.ProjectileType<Static_crystal_proj>())
                    crystals.Add(p);
            }
            if (crystals.Count < 2)
                return;
            crystals.Sort((a, b) => a.whoAmI.CompareTo(b.whoAmI));
            for (int i = 0; i < crystals.Count; i++)
            {
                if (crystals[i].whoAmI != source.whoAmI)
                    continue;
                int next = (i + 1) % crystals.Count;
                float dist = Vector2.Distance(crystals[i].Center, crystals[next].Center);
                if (dist < 400f && dist > 5f)
                {
                    DrawWavyBeam(sb, pixel, crystals[i].Center, crystals[next].Center, power * 0.6f, true);
                    Vector2 midPoint = (crystals[i].Center + crystals[next].Center) * 0.5f
                                       - Main.screenPosition;
                    float midPulse1 = (float)Math.Sin(beamTimer * 0.25f + i * 2f) * 0.5f + 0.5f;
                    float midPulse2 = (float)Math.Sin(beamTimer * 0.4f + i * 3f) * 0.5f + 0.5f;
                    float midPulse = midPulse1 * 0.6f + midPulse2 * 0.4f;
                    float midSize = 12f + midPulse * 10f;
                    float midAlpha = 0.1f + midPulse * 0.15f;
                    sb.Draw(pixel, midPoint, new Rectangle(0, 0, 1, 1), new Color(140, 180, 255) * midAlpha, beamTimer * 0.03f, new Vector2(0.5f, 0.5f), new Vector2(midSize, midSize), SpriteEffects.None, 0f);
                    float dotPulse = (float)Math.Sin(beamTimer * 0.5f + i);
                    if (dotPulse > 0.3f)
                    {
                        float dotSize = 3f * (dotPulse - 0.3f) / 0.7f;
                        sb.Draw(pixel, midPoint, new Rectangle(0, 0, 1, 1), Color.White * 0.5f * ((dotPulse - 0.3f) / 0.7f), 0f, new Vector2(0.5f, 0.5f), new Vector2(dotSize, dotSize), SpriteEffects.None, 0f);
                    }
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width * 0.3f, target.height * 0.3f), DustID.TintableDustLighted, Main.rand.NextVector2Circular(2.5f, 2.5f), 0, new Color(180, 210, 255), Main.rand.NextFloat(0.6f, 1.2f));
                d.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
#endregion
#endregion
