using System;
using jarkos.items.materials;
using jarkos.items.weapons.magic.staffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.StarCycle
{
	#region Buff
	public class StarCycle_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<StarCycle_proj>()] > 0)
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
	public class StarCycle_item : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults()
        {
            Item.damage = 24;
            Item.knockBack = 3f;
            Item.mana = 12;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<StarCycle_buff>();
            Item.buffTime = 2;
            Item.shoot = ModContent.ProjectileType<StarCycle_proj>();
            Item.shootSpeed = 10f;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            position = Main.MouseWorld;
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Manacrys_staff>(), 1);
            recipe.AddIngredient(ModContent.ItemType<ElementalPowder>(), 2);
            recipe.AddIngredient(ItemID.SoulofLight, 8);
            recipe.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 2);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
	#endregion
	#region Projectile
	public class StarCycle_proj : ModProjectile
	{
        private float PulseGlow
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private float BobTimer
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        private const float ORBIT_RADIUS = 90f;
        private const float ORBIT_SPEED = 0.035f;
        private const float DETECT_RANGE = 700f;
        private const float METEOR_COOLDOWN = 36f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
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
            Projectile.ai[0] += ORBIT_SPEED;
            if (Projectile.ai[0] > MathHelper.TwoPi)
                Projectile.ai[0] -= MathHelper.TwoPi;
            BobTimer += 0.04f;
            if (PulseGlow > 0f)
                PulseGlow = Math.Max(PulseGlow - 0.04f, 0f);
            int targetIndex = FindTarget(owner);
            bool hasTarget = targetIndex != -1;
            MoveInOrbit(owner, hasTarget);
            if (hasTarget)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] >= METEOR_COOLDOWN)
                {
                    Projectile.ai[1] = 0f;
                    if (Main.myPlayer == Projectile.owner)
                    {
                        SpawnMeteor(Main.npc[targetIndex]);
                    }
                    TriggerPulse();
                }
            }
            else
            {
                if (Projectile.ai[1] > 0f)
                    Projectile.ai[1] = METEOR_COOLDOWN - 15f;
            }
            SpawnAmbientDust(hasTarget);
            Projectile.rotation += 0.06f;
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<StarCycle_buff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<StarCycle_buff>()))
            {
                Projectile.timeLeft = 2;
            }
            return true;
        }
        private int FindTarget(Player owner)
        {
            int result = -1;
            float closest = DETECT_RANGE;
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC t = Main.npc[owner.MinionAttackTargetNPC];
                if (t.CanBeChasedBy() && Vector2.Distance(owner.Center, t.Center) < DETECT_RANGE * 1.5f)
                    return t.whoAmI;
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(owner.Center, npc.Center);
                if (dist < closest)
                {
                    closest = dist;
                    result = i;
                }
            }
            return result;
        }
        private void MoveInOrbit(Player owner, bool inCombat)
        {
            float radiusX = ORBIT_RADIUS;
            float radiusY = ORBIT_RADIUS * 0.45f;
            float bob = (float)Math.Sin(BobTimer) * 12f;
            if (inCombat)
            {
                radiusX *= 0.85f;
                radiusY *= 0.85f;
                Projectile.ai[0] += ORBIT_SPEED * 0.6f;
            }
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(Projectile.ai[0]) * radiusX, (float)Math.Sin(Projectile.ai[0]) * radiusY + bob - 40f);
            float dist = Vector2.Distance(Projectile.Center, targetPos);
            if (dist > 1500f)
            {
                Projectile.Center = targetPos;
                Projectile.velocity = Vector2.Zero;
                return;
            }
            Vector2 direction = targetPos - Projectile.Center;
            float moveSpeed;
            if (dist > 300f)
                moveSpeed = 16f;
            else if (dist > 50f)
                moveSpeed = 8f + dist * 0.02f;
            else
                moveSpeed = dist * 0.15f;
            if (dist > 1f)
            {
                direction.Normalize();
                Projectile.velocity = direction * moveSpeed;
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
            }
            Projectile.Center += Projectile.velocity * 0.5f;
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.12f);
        }
        private void SpawnMeteor(NPC target)
		{
			Vector2 spawnPos = Projectile.Center;
			Vector2 dir = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY);
			dir = dir.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));
			Vector2 velocity = dir * 10f;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocity, ModContent.ProjectileType<StarMeteor>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
			SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
		}
        private void TriggerPulse()
        {
            PulseGlow = 1f;
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch, vel.X, vel.Y, 0, default, 1.2f);
                    d.noGravity = true;
                }
            }
        }
        private void SpawnAmbientDust(bool inCombat)
        {
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 120, default, 0.7f);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.fadeIn = 1.2f;
            }
            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, 0f, 0f, 150, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.15f;
            }
            if (PulseGlow > 0.3f && Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch, vel.X, vel.Y, 0, default, PulseGlow * 1.2f);
                d.noGravity = true;
            }
            if (inCombat && Main.rand.NextBool(4))
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 0, 0, DustID.YellowStarDust, vel.X, vel.Y, 100, default, 0.9f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = (Texture2D)ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color gold = new Color(255, 210, 60);
            Color white = new Color(255, 255, 255);
            Color current = Color.Lerp(gold, white, PulseGlow);
            float outerScale = 2.2f + PulseGlow * 0.6f + (float)Math.Sin(BobTimer * 1.5f) * 0.15f;
            Color outerColor = current * (0.12f + PulseGlow * 0.2f);
            outerColor.A = 0;
            Main.EntitySpriteDraw(tex, pos, null, outerColor, Projectile.rotation * 0.3f, origin, outerScale, SpriteEffects.None, 0);
            float midScale = 1.6f + PulseGlow * 0.4f + (float)Math.Sin(BobTimer * 2f) * 0.1f;
            Color midColor = current * (0.25f + PulseGlow * 0.3f);
            midColor.A = 0;
            Main.EntitySpriteDraw(tex, pos, null, midColor, -Projectile.rotation * 0.5f, origin, midScale, SpriteEffects.None, 0);
            float mainScale = 1f + PulseGlow * 0.15f;
            Color mainColor = current;
            mainColor.A = 220;
            Main.EntitySpriteDraw(tex, pos, null, mainColor, Projectile.rotation, origin, mainScale, SpriteEffects.None, 0);
            float centerScale = 0.45f + PulseGlow * 0.2f;
            Color centerColor = Color.White * (0.6f + PulseGlow * 0.4f);
            centerColor.A = 0;
            Main.EntitySpriteDraw(tex, pos, null, centerColor, -Projectile.rotation * 0.7f, origin, centerScale, SpriteEffects.None, 0);
            float rayScale = 0.3f + (float)Math.Sin(BobTimer * 3f) * 0.1f + PulseGlow * 0.3f;
            Color rayColor = current * 0.3f;
            rayColor.A = 0;
            Main.EntitySpriteDraw(tex, pos, null, rayColor, MathHelper.PiOver4, origin, new Vector2(rayScale * 3f, rayScale), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, pos, null, rayColor, -MathHelper.PiOver4, origin, new Vector2(rayScale * 3f, rayScale), SpriteEffects.None, 0);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(new Color(255, 210, 60, 200), new Color(255, 255, 255, 230), PulseGlow);
        }
	}
	public class StarMeteor : ModProjectile
    {
		public override string Texture => "jarkos/items/weapons/summon/staff/StarCycle/StarProj";
        private bool exploded = false;
        private float spinTimer = 0f;
        private const float EXPLOSION_RADIUS = 40f;
        private const float HOMING_STRENGTH = 0.045f;
        private const float HOMING_RANGE = 600f;
        private const float MAX_SPEED = 18f;
        private const float INITIAL_SPEED = 10f;
        private const float ACCELERATION = 0.3f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.light = 0.6f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            if (exploded)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.scale = 0f;
                Projectile.Opacity = 0f;
                Projectile.ai[1] = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowStarDust, dustVel.X, dustVel.Y, 120, default, 0.8f);
                    d.noGravity = true;
                }
            }
            Projectile.ai[0]++;
            float age = Projectile.ai[0];
            if (age < 12f)
            {
                Projectile.scale = MathHelper.Lerp(0f, 1f, age / 12f);
                Projectile.Opacity = MathHelper.Lerp(0f, 1f, age / 12f);
            }
            else
            {
                Projectile.scale = 1f;
                Projectile.Opacity = 1f;
            }
            int targetIndex = FindHomingTarget();
            if (targetIndex != -1)
            {
                NPC target = Main.npc[targetIndex];
                ApplyHoming(target);
            }
            float currentSpeed = Projectile.velocity.Length();
            if (currentSpeed < MAX_SPEED)
            {
                Projectile.velocity *= 1f + (ACCELERATION / Math.Max(currentSpeed, 1f));
            }
            Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X);
            if (perp != Vector2.Zero)
            {
                perp.Normalize();
                float waveFrequency = 0.22f;
                float waveAmplitude = 2.8f;
                float waveOffset = (float)Math.Cos(age * waveFrequency + Projectile.ai[1]) * waveAmplitude;
                Projectile.Center += perp * waveOffset;
            }
            if (Projectile.velocity.Length() > MAX_SPEED)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= MAX_SPEED;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2 + spinTimer;
            spinTimer += 0.08f;
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, 0f, 0f, 150, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.1f;
            }
        }
        private int FindHomingTarget()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC t = Main.npc[owner.MinionAttackTargetNPC];
                if (t.CanBeChasedBy() && Vector2.Distance(Projectile.Center, t.Center) < HOMING_RANGE)
                    return t.whoAmI;
            }
            int result = -1;
            float closest = HOMING_RANGE;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closest)
                {
                    closest = dist;
                    result = i;
                }
            }
            return result;
        }
        private void ApplyHoming(NPC target)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            float distance = toTarget.Length();
            if (distance < 1f) return;
            toTarget.Normalize();
            float distanceFactor = 1f - MathHelper.Clamp(distance / HOMING_RANGE, 0f, 1f);
            float homingPower = HOMING_STRENGTH * (0.4f + distanceFactor * 0.6f);
            Vector2 currentDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            float angleDiff = MathHelper.WrapAngle(toTarget.ToRotation() - currentDir.ToRotation());
            if (Math.Abs(angleDiff) > MathHelper.ToRadians(120f))
                return;
            float currentAngle = Projectile.velocity.ToRotation();
            float targetAngle = toTarget.ToRotation();
            float newAngle = currentAngle + MathHelper.WrapAngle(targetAngle - currentAngle) * homingPower;
            float speed = Projectile.velocity.Length();
            Projectile.velocity = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * speed;
        }
        private void SpawnTrailDust()
        {
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!exploded)
                Explode();
        }
        private void Explode()
        {
            if (exploded) return;
            exploded = true;
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.35f, Pitch = 0.2f }, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                DealAreaDamage();
            }
            CreateExplosion();
            Projectile.Kill();
        }
        private void DealAreaDamage()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy())
                    continue;
                float dist = DistanceToNPC(npc);
                if (dist < EXPLOSION_RADIUS)
                {
                    float mult = 1f - (dist / EXPLOSION_RADIUS) * 0.5f;
                    int dmg = (int)(Projectile.damage * mult);
                    if (dmg < 1) dmg = 1;
                    float kb = Projectile.knockBack * mult;
                    int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;
                    npc.SimpleStrikeNPC(dmg, dir, false, kb, Projectile.DamageType);
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, i, dmg, kb, dir);
                    }
                }
            }
        }
        private float DistanceToNPC(NPC npc)
        {
            float cx = MathHelper.Clamp(Projectile.Center.X, npc.position.X, npc.position.X + npc.width);
            float cy = MathHelper.Clamp(Projectile.Center.Y, npc.position.Y, npc.position.Y + npc.height);
            return Vector2.Distance(Projectile.Center, new Vector2(cx, cy));
        }
        private void CreateExplosion()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, vel.X, vel.Y, 0, default, 1.2f);
                d.noGravity = true;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowStarDust, vel.X, vel.Y, 100, default, 0.8f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = (Texture2D)ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() * 0.5f;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;
            Color glowColor = new Color(255, 220, 50, 0) * 0.45f * Projectile.Opacity;
            float glowScale = 0.35f * Projectile.scale;
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, 0f, glowOrigin, glowScale, SpriteEffects.None, 0);
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * Projectile.Opacity;
                float scale = (1f - progress * 0.5f) * 0.8f * Projectile.scale;
                Color c = new Color(255, 230, 90, 0) * alpha * 0.4f;
                Main.EntitySpriteDraw(tex, trailPos, null, c, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color mainColor = new Color(255, 245, 180) * Projectile.Opacity;
            Main.EntitySpriteDraw(tex, pos, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 245, 180, 255);
        }
    }
	#endregion
}
