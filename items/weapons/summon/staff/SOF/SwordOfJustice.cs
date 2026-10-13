using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using Terraria.GameContent;

namespace jarkos.items.weapons.summon.staff.SOF
{
    public class SwordOfJustice_buff : ModBuff
    {
        public override void SetStaticDefaults() {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex) {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SwordOfJustice_proj>()] > 0) {
                player.buffTime[buffIndex] = 2;
            }
            else {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    public class SwordOfJustice_item : ModItem
    {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }
        public override void SetDefaults() {
            Item.damage = 165;
            Item.crit = 6;
            Item.knockBack = 3f;
            Item.mana = 12;
            Item.width = 38;
            Item.height = 82;
            Item.useTime = 21;
            Item.useAnimation = 21;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<SwordOfJustice_buff>();
            Item.shoot = ModContent.ProjectileType<SwordOfJustice_proj>();

            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            position = Main.MouseWorld;
        }
        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            int projType = ModContent.ProjectileType<SwordOfJustice_proj>();

            while (player.ownedProjectileCounts[projType] >= 2)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == player.whoAmI && p.type == projType)
                    {
                        p.Kill();
                        player.ownedProjectileCounts[projType]--;
                        break;
                    }
                }
            }

            player.AddBuff(ModContent.BuffType<SwordOfJustice_buff>(), 180);
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.EmpressBlade, 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class SwordOfJustice_proj : ModProjectile
    {
        private enum AttackState
        {
            Idle,
            HoverTarget,
            Telegraph,
            Slashing,
            Returning
        }

        private AttackState CurrentState
        {
            get => (AttackState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private ref float StateTimer => ref Projectile.ai[1];
        private ref float AttackCooldown => ref Projectile.localAI[0];

        private Vector2 slashStart;
        private Vector2 slashEnd;
        private float targetOrbitAngle;
        private float targetOrbitDistance;
        private bool initialized;

        private readonly List<Vector2> tipTrail = new List<Vector2>();

        public override void SetStaticDefaults() {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public sealed override void SetDefaults() {
            Projectile.width = 38;
            Projectile.height = 83;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 2;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.light = 0.85f;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => CurrentState == AttackState.Slashing;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (CurrentState == AttackState.Slashing)
            {
                float collisionPoint = 0f;
                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), slashStart, slashEnd, 45f, ref collisionPoint);
            }
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void AI() {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner)) return;

            if (!initialized)
            {
                initialized = true;
                targetOrbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                targetOrbitDistance = Main.rand.NextFloat(130f, 210f);
                AttackCooldown = Main.rand.Next(10, 35);
            }

            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);

            if (!foundTarget)
            {
                CurrentState = AttackState.Idle;
            }
            else if (CurrentState == AttackState.Idle)
            {
                CurrentState = AttackState.HoverTarget;
                targetOrbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                targetOrbitDistance = Main.rand.NextFloat(130f, 210f);
                AttackCooldown = Main.rand.Next(10, 35);
            }

            switch (CurrentState)
            {
                case AttackState.Idle:
                    UpdateIdle(owner);
                    break;

                case AttackState.HoverTarget:
                    UpdateHoverTarget(owner, targetCenter);
                    break;

                case AttackState.Telegraph:
                    UpdateTelegraph(owner, targetCenter);
                    break;

                case AttackState.Slashing:
                    UpdateSlashing(owner, targetCenter);
                    break;

                case AttackState.Returning:
                    UpdateReturning(owner, targetCenter);
                    break;
            }

            Visuals();

            Vector2 tipPos = Projectile.Center + new Vector2(0f, -Projectile.height / 2f * Projectile.scale - 20f).RotatedBy(Projectile.rotation);
            if (CurrentState == AttackState.Slashing || CurrentState == AttackState.Telegraph)
            {
                tipTrail.Add(tipPos);
                if (tipTrail.Count > 18) tipTrail.RemoveAt(0);
            }
            else
            {
                if (tipTrail.Count > 0) tipTrail.RemoveAt(0);
            }
        }

        private void UpdateIdle(Player owner)
        {
            Vector2 idlePosition = owner.Center;
            idlePosition.Y -= 48f;
            float offset = (20 + Projectile.minionPos * 45) * -owner.direction;
            idlePosition.X += offset;
            idlePosition.Y += (float)Math.Sin((Main.GameUpdateCount + Projectile.minionPos * 20) * 0.08f) * 12f;

            Vector2 dir = idlePosition - Projectile.Center;
            float dist = dir.Length();

            if (dist > 1800f)
            {
                Projectile.position = idlePosition;
                Projectile.velocity = Vector2.Zero;
            }
            else
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 0.15f, 0.1f);
            }

            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.velocity.X * 0.04f, 0.1f);
        }

        private void UpdateHoverTarget(Player owner, Vector2 targetCenter)
        {
            targetOrbitAngle += 0.02f;
            float hoverPulse = (float)Math.Sin((Main.GameUpdateCount + Projectile.minionPos * 30) * 0.06f) * 20f;
            Vector2 hoverPos = targetCenter + targetOrbitAngle.ToRotationVector2() * (targetOrbitDistance + hoverPulse);

            Vector2 toHover = hoverPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toHover * 0.2f, 0.18f);

            Vector2 toTarget = targetCenter - Projectile.Center;
            float desiredRot = toTarget.ToRotation() + MathHelper.PiOver2;
            float rotDiff = MathHelper.WrapAngle(desiredRot - Projectile.rotation);
            Projectile.rotation += rotDiff * 0.15f;

            AttackCooldown--;
            if (AttackCooldown <= 0)
            {
                CurrentState = AttackState.Telegraph;
                StateTimer = 12f;

                float slashAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                slashStart = targetCenter - slashAngle.ToRotationVector2() * 150f;
                slashEnd = targetCenter + slashAngle.ToRotationVector2() * 170f;

                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.5f, Pitch = 0.4f }, Projectile.Center);
            }
        }

        private void UpdateTelegraph(Player owner, Vector2 targetCenter)
        {
            StateTimer--;

            Vector2 pullBackPos = slashStart - Vector2.Normalize(slashEnd - slashStart) * 25f;
            Projectile.Center = Vector2.Lerp(Projectile.Center, pullBackPos, 0.35f);

            Vector2 slashDir = Vector2.Normalize(slashEnd - slashStart);
            Projectile.rotation = slashDir.ToRotation() + MathHelper.PiOver2;

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
            {
                Vector2 pVel = Main.rand.NextVector2Circular(3f, 3f);
                new Spark(Projectile.Center, pVel, Color.IndianRed, 20, Main.rand.NextFloat(0.5f, 0.9f), 0.9f).Spawn();
            }

            if (StateTimer <= 0)
            {
                CurrentState = AttackState.Slashing;
                StateTimer = 14f;

                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.1f, 0.2f) }, Projectile.Center);

                if (Main.netMode != NetmodeID.Server)
                {
                    new SpectralRingParticle(Projectile.Center, Color.IndianRed, 14, 0.1f, 0.5f, 0.9f).Spawn();
                }
            }
        }

        private void UpdateSlashing(Player owner, Vector2 targetCenter)
        {
            StateTimer--;

            float progress = 1f - (StateTimer / 14f);
            float easeOut = 1f - (float)Math.Pow(1f - progress, 3);

            Projectile.Center = Vector2.Lerp(slashStart, slashEnd, easeOut);

            Vector2 slashDir = Vector2.Normalize(slashEnd - slashStart);
            Projectile.rotation = slashDir.ToRotation() + MathHelper.PiOver2;

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
            {
                Vector2 pVel = -slashDir * Main.rand.NextFloat(4f, 8f) + Main.rand.NextVector2Circular(2f, 2f);
                new Spark(Projectile.Center, pVel, Color.Red, 25, Main.rand.NextFloat(0.6f, 1.1f), 1f).Spawn();
                new StarParticle(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), Color.LightCoral, 18, 0.3f, 0.9f).Spawn();
            }

            if (StateTimer <= 0)
            {
                CurrentState = AttackState.Returning;
                StateTimer = 18f;

                targetOrbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                targetOrbitDistance = Main.rand.NextFloat(130f, 210f);
            }
        }

        private void UpdateReturning(Player owner, Vector2 targetCenter)
        {
            StateTimer--;

            Vector2 targetHover = targetCenter + targetOrbitAngle.ToRotationVector2() * targetOrbitDistance;
            float progress = 1f - (StateTimer / 18f);
            float easeOut = 1f - (float)Math.Pow(1f - progress, 2);

            Projectile.Center = Vector2.Lerp(slashEnd, targetHover, easeOut);

            Vector2 toHover = targetHover - Projectile.Center;
            if (toHover != Vector2.Zero)
            {
                float desiredRot = toHover.ToRotation() + MathHelper.PiOver2;
                float rotDiff = MathHelper.WrapAngle(desiredRot - Projectile.rotation);
                Projectile.rotation += rotDiff * 0.15f;
            }

            if (StateTimer <= 0)
            {
                CurrentState = AttackState.HoverTarget;
                AttackCooldown = Main.rand.Next(20, 35);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                var modPlayer = Main.LocalPlayer.GetModPlayer<Screenshake_system>();
                if (modPlayer != null) modPlayer.weak_shake = 1.5f;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                    new Spark(target.Center, burstVel, Color.Crimson, 25, Main.rand.NextFloat(0.5f, 1.0f), 1f).Spawn();
                }
            }
        }

        private bool CheckActive(Player owner) {
            if (owner.dead || !owner.active) {
                owner.ClearBuff(ModContent.BuffType<SwordOfJustice_buff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<SwordOfJustice_buff>())) {
                Projectile.timeLeft = 2;
            }
            return true;
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter) {
            distanceFromTarget = 1200f;
            targetCenter = Projectile.position;
            foundTarget = false;

            if (owner.HasMinionAttackTargetNPC) 
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);

                if (npc.CanBeChasedBy() && between < 2000f) 
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }

            if (!foundTarget) {
                for (int i = 0; i < Main.maxNPCs; i++) 
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy()) {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        bool closeThroughWall = between < 200f;
                        if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
            Projectile.friendly = foundTarget;
        }

        private void Visuals() {
            Lighting.AddLight(Projectile.Center, Color.IndianRed.ToVector3() * 0.85f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTipTrail();

            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle rectangle = new(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;

            if (CurrentState == AttackState.Slashing || CurrentState == AttackState.Telegraph)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero) continue;
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + origin + new Vector2(0f, Projectile.gfxOffY);
                    float progress = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                    Color glowColor = Color.Crimson * progress * 0.65f;
                    float rot = Projectile.oldRot[k] != 0f ? Projectile.oldRot[k] : Projectile.rotation;

                    spriteBatch.Draw(texture, drawPos, rectangle, glowColor, rot, origin, Projectile.scale * (0.9f + 0.2f * progress), SpriteEffects.None, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Color color = Projectile.GetAlpha(lightColor);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        private void DrawTipTrail()
        {
            if (tipTrail.Count < 2) return;
            if (systems.HealShaders.Stream != null && systems.HealShaders.HealNoise != null)
            {
                Effect effect = systems.HealShaders.Stream;
                Main.spriteBatch.End();
                try
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    effect.Parameters["uOpacity"]?.SetValue(2.5f);
                    effect.Parameters["uColor"]?.SetValue(Color.IndianRed.ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(255, 180, 180).ToVector4());
                    systems.TrailRenderer.Draw(effect, tipTrail.ToArray(), t => MathHelper.Lerp(0f, 32f, t), t => Color.White * t, systems.HealShaders.HealNoise);
                }
                finally
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
        }
    }
}
