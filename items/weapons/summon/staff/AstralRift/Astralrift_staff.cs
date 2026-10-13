using System;
using System.Collections.Generic;
using System.Linq;
using jarkos.items.materials;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.AstralRift
{
    #region Buff
    public class Astralrift_buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Astralrift_proj>()] > 0)
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
    public class Astralrift_staff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 210;
            Item.knockBack = 3f;
            Item.mana = 10;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item8;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<Astralrift_buff>();
            Item.shoot = ModContent.ProjectileType<Astralrift_proj>();
            Item.shootSpeed = 0f;
            Item.autoReuse = false;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Vector2 spawnPos = player.Center + new Vector2(Main.rand.NextFloat(-60f, 60f), Main.rand.NextFloat(-80f, -20f));
            Projectile proj = Projectile.NewProjectileDirect(source, spawnPos, Vector2.Zero, ModContent.ProjectileType<Astralrift_proj>(), damage, knockback, player.whoAmI);
            proj.originalDamage = Item.damage;
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.StardustCellStaff, 1);
            recipe.AddIngredient(ModContent.ItemType<HeavenlyPowder>(), 2);
            recipe.AddIngredient(ItemID.LunarBar, 12);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
    #endregion
    #region RiftProjectile
    public class Astralrift_proj : ModProjectile
    {
        private ref float AttackTimer => ref Projectile.ai[0];
        private ref float RotationPhase => ref Projectile.ai[1];
        private ref float DriftAngle => ref Projectile.localAI[0];
        private ref float SpawnTimer => ref Projectile.localAI[1];
        private const float BaseAttackCooldown = 100f;
        private const float MinAttackCooldown = 40f;
        private const float PreferredDistance = 220f;
        private const float DetectionRange = 800f;
        private const float RepelStrength = 1.2f;
        private const float RepelRadius = 120f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
            Projectile.timeLeft = 18000;
            Projectile.hide = false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }
            if (player.HasBuff(ModContent.BuffType<Astralrift_buff>()))
            {
                Projectile.timeLeft = 2;
            }
            if (SpawnTimer < 60f)
            {
                SpawnTimer++;
                Projectile.scale = SpawnTimer / 60f;
            }
            else
            {
                Projectile.scale = 1f;
            }
            RotationPhase += 0.03f;
            NPC target = FindTarget(player);
            if (target != null)
            {
                MoveAroundTarget(target);
            }
            else
            {
                DriftNearPlayer(player);
            }
            ApplyRepulsion(player);
            float distToPlayer = Vector2.Distance(Projectile.Center, player.Center);
            float maxSpeed = target != null ? 12f : (distToPlayer > 250f ? 35f : 6f);
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }
            if (AttackTimer > 0f)
            {
                AttackTimer--;
            }
            if (target != null && AttackTimer <= 0f && SpawnTimer >= 60f)
            {
                float cooldown = CalculateAttackCooldown(player);
                AttackTimer = cooldown;
                FireBeam(player, target);
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.8f);
            SpawnParticles();
        }
        private NPC FindTarget(Player player)
        {
            NPC target = null;
            float closest = DetectionRange;
            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closest)
                    {
                        target = npc;
                        closest = dist;
                    }
                }
            }
            if (target != null) return target;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closest)
                {
                    target = npc;
                    closest = dist;
                }
            }
            return target;
        }
        private void MoveAroundTarget(NPC target)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            float dist = toTarget.Length();
            if (dist < 1f) return;
            Vector2 dirToTarget = toTarget / dist;
            if (dist > PreferredDistance + 30f)
            {
                Projectile.velocity += dirToTarget * 0.6f;
            }
            else if (dist < PreferredDistance - 30f)
            {
                Projectile.velocity -= dirToTarget * 0.8f;
            }
            else
            {
                Vector2 tangent = new Vector2(-dirToTarget.Y, dirToTarget.X);
                Projectile.velocity += tangent * 0.5f;
                Projectile.velocity *= 0.96f;
            }
        }
        private void DriftNearPlayer(Player player)
        {
            DriftAngle += 0.008f;
            float radius = 80f + 30f * (float)Math.Sin(DriftAngle * 1.3f);
            Vector2 targetPos = player.Center + new Vector2((float)Math.Cos(DriftAngle) * radius, -50f + (float)Math.Sin(DriftAngle * 0.7f) * 30f);
            Vector2 dir = targetPos - Projectile.Center;
            float dist = dir.Length();
            if (dist > 250f)
            {
                float speedMult = MathHelper.Clamp(dist / 120f, 2f, 18f);
                dir.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speedMult * 2f, 0.15f);
            }
            else
            {
                Projectile.velocity += dir * 0.04f;
                Projectile.velocity *= 0.94f;
            }
        }
        private void ApplyRepulsion(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active) continue;
                if (other.whoAmI == Projectile.whoAmI) continue;
                if (other.type != Projectile.type) continue;
                if (other.owner != player.whoAmI) continue;
                Vector2 diff = Projectile.Center - other.Center;
                float dist = diff.Length();
                if (dist < RepelRadius && dist > 0.1f)
                {
                    float force = RepelStrength * (1f - dist / RepelRadius);
                    Projectile.velocity += diff / dist * force;
                }
            }
        }
        private float CalculateAttackCooldown(Player player)
        {
            int maxMana = player.statManaMax2;
            float t = MathHelper.Clamp(maxMana / 200f, 0f, 1f);
            return MathHelper.Lerp(BaseAttackCooldown, MinAttackCooldown, t);
        }
        private void FireBeam(Player player, NPC target)
        {
            if (Projectile.owner != Main.myPlayer) return;
            EntitySource_Parent source = new EntitySource_Parent(Projectile);
            Vector2 predictedPos = target.Center + target.velocity * 8f;
            Vector2 direction = predictedPos - Projectile.Center;
            if (direction == Vector2.Zero) return;

            direction.Normalize();
            float beamSpeed = 18f;

            Projectile beam = Projectile.NewProjectileDirect(source, Projectile.Center, direction * beamSpeed, ModContent.ProjectileType<Astralrift_beam>(), Projectile.damage, 2f, player.whoAmI);
            beam.ai[0] = Projectile.whoAmI;

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12f * i;
                Vector2 dustVel = new Vector2(3f, 0f).RotatedBy(angle);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PurpleTorch, dustVel.X, dustVel.Y, 0, Color.Violet, 1.5f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item68, Projectile.Center);
        }
        private void SpawnParticles()
        {
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Projectile.width / 2f * Projectile.scale;
                Vector2 offset = new Vector2(radius, 0f).RotatedBy(angle);
                Dust dust = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.PurpleTorch, offset.Y * -0.05f, offset.X * 0.05f, 150, Color.MediumPurple, 1.1f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Enchanted_Gold, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 0, Color.Gold, 1.4f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawPortal();
            return false;
        }
        private void DrawPortal()
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float scale = Projectile.scale;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Color glowColor = new Color(180, 50, 255) * 0.5f * scale;
            float glowScale = Projectile.width * 1.6f / glowTex.Width;
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(glowTex, drawPos, null, glowColor, 0f, glowOrigin, glowScale * scale, SpriteEffects.None, 0);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D portalTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 portalOrigin = portalTex.Size() / 2f;
            Main.EntitySpriteDraw(portalTex, drawPos, null, Color.White * scale, -RotationPhase * 0.7f, portalOrigin, scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(portalTex, drawPos, null, Color.Violet * 0.7f * scale, RotationPhase * 1.2f, portalOrigin, scale * 0.85f, SpriteEffects.None, 0);
        }
        public override bool? CanHitNPC(NPC target)
        {
            return null;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= 2f;
            modifiers.Knockback *= 2f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
    #endregion
    #region BeamProjectile
    public class Astralrift_beam : ModProjectile
    {
        private readonly Queue<Vector2> trailPositions = new Queue<Vector2>();
        private const int TrailLength = 18;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.minion = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.light = 0.6f;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            trailPositions.Enqueue(Projectile.Center);
            if (trailPositions.Count > TrailLength)
                trailPositions.Dequeue();
            Lighting.AddLight(Projectile.Center, 0.3f, 0.0f, 0.6f);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, Color.Violet, 1.0f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i;
                Vector2 vel = new Vector2(2f, 0f).RotatedBy(angle);
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.PurpleTorch, vel.X, vel.Y, 0, Color.MediumPurple, 1.3f);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 150, Color.Violet, 1.2f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawBeamTrail();
            DrawBeamHead();
            return false;
        }
        private void DrawBeamTrail()
        {
            if (trailPositions.Count < 1) return;

            List<Vector2> pts = new List<Vector2>(trailPositions.Count + 1);
            pts.Add(Projectile.Center);
            foreach (Vector2 p in Enumerable.Reverse(trailPositions))
            {
                if (Vector2.DistanceSquared(pts[pts.Count - 1], p) > 1f)
                    pts.Add(p);
            }

            if (pts.Count < 2) return;

            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                try
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    effect.Parameters["uOpacity"]?.SetValue(1.8f);
                    effect.Parameters["uColor"]?.SetValue(new Color(180, 80, 255, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(245, 220, 255, 255).ToVector4());

                    TrailRenderer.Draw(effect, pts, t => MathHelper.Lerp(14f, 0f, t), t => Color.White * (1f - t * 0.8f), HealShaders.HealNoise);
                }
                finally
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            else
            {
                Texture2D tex = TextureAssets.MagicPixel.Value;
                for (int i = 1; i < pts.Count; i++)
                {
                    float progress = 1f - (float)i / (pts.Count - 1);
                    float alpha = progress * 0.9f;
                    float width = MathHelper.Lerp(0f, 14f, progress);
                    Vector2 segment = pts[i - 1] - pts[i];
                    float rotation = segment.ToRotation();
                    Main.EntitySpriteDraw(tex, pts[i] - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.MediumPurple * alpha, rotation, Vector2.Zero, new Vector2(segment.Length(), width), SpriteEffects.None, 0);
                }
            }
        }
        private void DrawBeamHead()
        {
            Texture2D tex = ModContent.Request<Texture2D>("jarkos/textures/MagicSphereProj", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.3f);
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(glowTex, drawPos, null, Color.Violet * 0.45f * pulse, 0f, glowOrigin, 30f * pulse / glowTex.Width, SpriteEffects.None, 0);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(tex, drawPos, null, Color.White * 0.9f, rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
    }
    #endregion
}
