using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.GameContent;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;
using jarkos.buffs.debuff;
using jarkos.particles;
using jarkos.items.weapons.melee.true_melee;
namespace jarkos.projectiles.melee.unique_melee
{
	public class Blade_of_war_proj : ModProjectile
	{
		public enum SwordState
        {
            Orbiting, Attacking, Returning, Formation
        }
        private ref float State => ref Projectile.ai[0];
        private ref float OrbitAngle => ref Projectile.ai[1];
        private ref float SwordIndex => ref Projectile.localAI[0];
        private ref float AttackTimer => ref Projectile.localAI[1];
        private Vector2 targetPosition;
        private float targetRotation;
        private int targetNPCIndex = -1;
        private float glowIntensity = 0f;
        private float trailAlpha = 0f;
        private const int TRAIL_LENGTH = 20;
        private Vector2[] trailPositions = new Vector2[TRAIL_LENGTH];
        private float[] trailRotations = new float[TRAIL_LENGTH];
        private const float ORBIT_RADIUS = 80f;
        private const float ORBIT_SPEED = 0.03f;
        private const float ATTACK_SPEED = 25f;
        private const float RETURN_SPEED = 15f;
        private const float BLADE_LENGTH = 36f;
        private const float BLADE_WIDTH = 6f;
        private const float HANDLE_LENGTH = 10f;
        private const float GUARD_WIDTH = 12f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 110;
            Projectile.height = 112;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = int.MaxValue;
            Projectile.alpha = 80;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            var legionPlayer = owner.GetModPlayer<LegionBladePlayer>();
            if (!owner.active || owner.dead || !legionPlayer.HasLegionBlade)
            {
                Projectile.Kill();
                return;
            }
            UpdateTrail();
            switch ((SwordState)State)
            {
                case SwordState.Orbiting:
                    OrbitBehavior(owner, legionPlayer);
                    break;
                case SwordState.Attacking:
                    AttackBehavior(owner, legionPlayer);
                    break;
                case SwordState.Returning:
                    ReturnBehavior(owner);
                    break;
                case SwordState.Formation:
                    FormationBehavior(owner, legionPlayer);
                    break;
            }
            UpdateVisuals();
            SpawnAmbientParticles();
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.7f, 0.3f) * (0.4f + glowIntensity * 0.4f));
        }
        private void UpdateTrail()
        {
            for (int i = trailPositions.Length - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
                trailRotations[i] = trailRotations[i - 1];
            }
            trailPositions[0] = Projectile.Center;
            trailRotations[0] = Projectile.rotation;
        }
        private void SpawnAmbientParticles()
        {
            if ((SwordState)State == SwordState.Attacking)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(10, 10);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
                }
                if (Main.rand.NextBool(3))
                {
                    Vector2 tipPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * BLADE_LENGTH;
                    Dust Spark = Dust.NewDustPerfect(tipPos, DustID.Torch, Main.rand.NextVector2Circular(3f, 3f));
                    Spark.noGravity = true;
                    Spark.scale = 0.6f;
                }
            }
            else if (Main.rand.NextBool(20))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(8, 8);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldCoin, Main.rand.NextVector2Circular(0.5f, 0.5f));
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.fadeIn = 0.8f;
            }
        }
        private void OrbitBehavior(Player owner, LegionBladePlayer legionPlayer)
        {
            Projectile.velocity = Vector2.Zero;

            int totalSwords = Math.Max(legionPlayer.SwordCount, 1);
            float myIndex = SwordIndex;

            float baseAngle = Main.GameUpdateCount * ORBIT_SPEED;
            float offset = MathHelper.TwoPi * myIndex / totalSwords;

            OrbitAngle = baseAngle + offset;
            float dynamicRadius = ORBIT_RADIUS + MathF.Sin(Main.GameUpdateCount * 0.05f + myIndex) * 10f;
            float verticalOffset = MathF.Sin(OrbitAngle * 2) * 15f;

            targetPosition = owner.Center + new Vector2(MathF.Cos(OrbitAngle) * dynamicRadius, MathF.Sin(OrbitAngle) * dynamicRadius * 0.5f + verticalOffset);

            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, 0.15f);
            targetRotation = OrbitAngle + MathHelper.PiOver4;
            
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.2f);

            glowIntensity = MathHelper.Lerp(glowIntensity, 0.3f, 0.1f);
            trailAlpha = MathHelper.Lerp(trailAlpha, 0.3f, 0.1f);

            if (!legionPlayer.IsCommanding && Main.rand.NextBool(120))
            {
                TryAutoAttack(owner);
            }
        }
        private void FormationBehavior(Player owner, LegionBladePlayer legionPlayer)
        {
            Projectile.velocity = Vector2.Zero;
            int totalSwords = Math.Max(legionPlayer.SwordCount, 1);
            float myIndex = SwordIndex;

            switch (legionPlayer.CurrentFormation)
            {
                case Formation.Circle:
                    FormationCircle(owner, totalSwords, myIndex);
                    break;
                case Formation.Spiral:
                    FormationSpiral(owner, totalSwords, myIndex);
                    break;
                default:
                    State = (float)SwordState.Orbiting;
                    return;
            }

            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, 0.12f);
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.15f);
            glowIntensity = MathHelper.Lerp(glowIntensity, 0.5f, 0.1f);
            trailAlpha = MathHelper.Lerp(trailAlpha, 0.2f, 0.1f);
        }
        private void FormationCircle(Player owner, int total, float index)
        {
            float angle = MathHelper.TwoPi * index / total + Main.GameUpdateCount * 0.01f;
            float radius = 120f + MathF.Sin(Main.GameUpdateCount * 0.03f) * 10f;
            targetPosition = owner.Center + angle.ToRotationVector2() * radius;
            targetRotation = angle + MathHelper.PiOver2;
        }
        private void FormationSpiral(Player owner, int total, float index)
        {
            float time = Main.GameUpdateCount * 0.02f;
            float spiralAngle = index * 0.5f + time;
            float radius = 50f + index * 3f + MathF.Sin(time + index) * 20f;
            float verticalOffset = MathF.Sin(spiralAngle * 2 + time) * 30f;
            targetPosition = owner.Center + new Vector2(MathF.Cos(spiralAngle) * radius, MathF.Sin(spiralAngle) * radius * 0.6f + verticalOffset);
            targetRotation = spiralAngle * 2 + time;
        }
        private void AttackBehavior(Player owner, LegionBladePlayer legionPlayer)
        {
            AttackTimer += 0.5f;
            if (targetNPCIndex < 0 || targetNPCIndex >= Main.maxNPCs || !Main.npc[targetNPCIndex].active || !Main.npc[targetNPCIndex].CanBeChasedBy())
            {
                TransitionToReturning();
                return;
            }

            NPC target = Main.npc[targetNPCIndex];
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float speed = ATTACK_SPEED + AttackTimer * 0.1f;

            Projectile.velocity = direction * speed;
            Projectile.Center += Projectile.velocity * 0.5f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            glowIntensity = MathHelper.Lerp(glowIntensity, 1f, 0.2f);
            trailAlpha = MathHelper.Lerp(trailAlpha, 1f, 0.15f);

            if (AttackTimer > 180)
            {
                TransitionToReturning();
            }
        }
        private void ReturnBehavior(Player owner)
        {
            Vector2 direction = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float distance = Vector2.Distance(Projectile.Center, owner.Center);
            float speed = Math.Min(RETURN_SPEED + distance * 0.05f, 40f);

            Projectile.velocity = direction * speed;
            Projectile.Center += Projectile.velocity * 0.5f;
            Projectile.rotation += 0.3f;

            glowIntensity = MathHelper.Lerp(glowIntensity, 0.5f, 0.1f);
            trailAlpha = MathHelper.Lerp(trailAlpha, 0.5f, 0.1f);

            if (distance < 80f)
            {
                TransitionToOrbiting();
            }
        }
        private void TransitionToOrbiting()
        {
            State = (float)SwordState.Orbiting;
            AttackTimer = 0;
            targetNPCIndex = -1;
            Projectile.velocity = Vector2.Zero;
        }
        private void TransitionToReturning()
        {
            State = (float)SwordState.Returning;
            targetNPCIndex = -1;
        }
        private void TryAutoAttack(Player owner)
        {
            float range = 750f;
            NPC closest = null;
            float closestDist = range;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(npc.Center, owner.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            if (closest != null)
            {
                SetAttackTarget(closest.whoAmI);
            }
        }
        public void SetAttackTarget(int npcIndex)
        {
            targetNPCIndex = npcIndex;
            State = (float)SwordState.Attacking;
            AttackTimer = 0;
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
        }
        public void SetFormationMode(Formation formation)
        {
            if (formation == Formation.Orbit)
            {
                if ((SwordState)State != SwordState.Attacking && (SwordState)State != SwordState.Returning)
                {
                    State = (float)SwordState.Orbiting;
                }
            }
            else
            {
                if ((SwordState)State != SwordState.Attacking && (SwordState)State != SwordState.Returning)
                {
                    State = (float)SwordState.Formation;
                }
            }
        }
        private void UpdateVisuals()
        {
            int baseAlpha = 80;
            int pulseAlpha = (int)(MathF.Sin(Main.GameUpdateCount * 0.1f + SwordIndex) * 20);
            Projectile.alpha = baseAlpha - pulseAlpha - (int)(glowIntensity * 40);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var legionPlayer = owner.GetModPlayer<LegionBladePlayer>();
            CreateHitEffects(target.Center);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.5f, Volume = 0.4f }, target.Center);
            if ((SwordState)State == SwordState.Attacking)
            {
                TransitionToReturning();
            }
            legionPlayer.AddCombo();
        }
        private void CreateHitEffects(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(position, DustID.GoldFlame, Main.rand.NextVector2Circular(6f, 6f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.8f);
            }
            for (int i = 0; i < 8; i++)
            {
                Dust fire = Dust.NewDustPerfect(position, DustID.Torch, Main.rand.NextVector2Circular(4f, 4f));
                fire.noGravity = true;
                fire.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            SpriteBatch spriteBatch = Main.spriteBatch;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 origin = new Vector2(Projectile.height / 2, Projectile.height / 2);

            float rotationOffset = MathHelper.ToRadians(90f);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            Color glowColor = new Color(255, 140, 87);
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(6, 0).RotatedBy(MathHelper.TwoPi / 6 * i);
                spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, glowColor, Projectile.rotation + rotationOffset, origin, Projectile.scale, SpriteEffects.FlipHorizontally, 0);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f + 0.9f;

            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 140, 87) * pulse, Projectile.rotation + rotationOffset, origin, Projectile.scale, SpriteEffects.FlipHorizontally, 0);
            DrawTrail(spriteBatch, pixel);
            DrawOuterGlow(spriteBatch, pixel);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            DrawSwordBody(spriteBatch, pixel);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            return true;
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (trailAlpha < 0.05f) return;
            for (int i = TRAIL_LENGTH - 1; i >= 1; i--)
            {
                if (trailPositions[i] == Vector2.Zero || trailPositions[i - 1] == Vector2.Zero)
                    continue;

                float progress = 1f - (float)i / TRAIL_LENGTH;
                float alpha = progress * trailAlpha * 0.8f;
                float width = BLADE_WIDTH * 1.5f * progress;

                Vector2 start = trailPositions[i];
                Vector2 end = trailPositions[i - 1];
                Vector2 direction = end - start;

                float length = direction.Length();
                if (length < 1f) continue;

                float rotation = direction.ToRotation();

                Color outerColor = new Color(255, 120, 30) * alpha * 0.4f;
                DrawLine(spriteBatch, pixel, start, rotation, length, width * 2f, outerColor);

                Color midColor = new Color(255, 200, 80) * alpha * 0.6f;
                DrawLine(spriteBatch, pixel, start, rotation, length, width * 1.3f, midColor);

                Color innerColor = new Color(255, 240, 180) * alpha * 0.8f;
                DrawLine(spriteBatch, pixel, start, rotation, length, width * 0.6f, innerColor);
            }
        }
        private void DrawOuterGlow(SpriteBatch spriteBatch, Texture2D pixel)
        {
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.15f + SwordIndex) * 0.3f + 0.7f;
            float glowSize = 50f * (0.5f + glowIntensity * 0.5f) * pulse;
            for (int layer = 0; layer < 4; layer++)
            {
                float layerSize = glowSize * (1f - layer * 0.2f);
                float layerAlpha = 0.15f * (1f - layer * 0.2f) * (0.3f + glowIntensity * 0.7f);
                Color glowColor = Color.Lerp(new Color(255, 100, 20), new Color(255, 220, 100), layer / 3f) * layerAlpha;
                DrawGlowCircle(spriteBatch, pixel, Projectile.Center, layerSize, glowColor);
            }
        }
        private void DrawSwordBody(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 center = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;

            Vector2 bladeDir = rotation.ToRotationVector2();
            Vector2 perpDir = bladeDir.RotatedBy(MathHelper.PiOver2);

            float baseAlpha = 1f - Projectile.alpha / 255f;
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.1f + SwordIndex * 0.5f) * 0.1f + 0.9f;

            Vector2 handleStart = center - bladeDir * HANDLE_LENGTH;
            Vector2 handleEnd = center;
            Color handleColor = new Color(60, 40, 20) * baseAlpha;

            DrawLineBetweenPoints(spriteBatch, pixel, handleStart, handleEnd, 5f, handleColor);
            
            Color wrapColor = new Color(139, 90, 43) * baseAlpha;
            DrawLineBetweenPoints(spriteBatch, pixel, handleStart, handleEnd, 3f, wrapColor);

            Vector2 guardLeft = center + perpDir * (GUARD_WIDTH / 2);
            Vector2 guardRight = center - perpDir * (GUARD_WIDTH / 2);
            Color guardColor = new Color(180, 140, 60) * baseAlpha;
            DrawLineBetweenPoints(spriteBatch, pixel, guardLeft, guardRight, 4f, guardColor);

            Color guardHighlight = new Color(255, 220, 120) * baseAlpha * 0.7f;
            DrawLineBetweenPoints(spriteBatch, pixel, guardLeft, guardRight, 2f, guardHighlight);

            Vector2 bladeStart = center;
            Vector2 bladeTip = center + bladeDir * BLADE_LENGTH;
            Color shadowColor = new Color(80, 50, 20) * baseAlpha * 0.5f;

            DrawBlade(spriteBatch, pixel, bladeStart + perpDir * 2f, bladeTip + perpDir * 1f, BLADE_WIDTH, 1f, shadowColor);

            Color bladeBase = new Color(200, 160, 80) * baseAlpha * pulse;
            Color bladeEdge = new Color(255, 220, 140) * baseAlpha * pulse;

            DrawBlade(spriteBatch, pixel, bladeStart, bladeTip, BLADE_WIDTH, 2f, bladeBase);
            DrawBlade(spriteBatch, pixel, bladeStart, bladeTip, BLADE_WIDTH * 0.5f, 1f, bladeEdge);

            Color fullerColor = new Color(120, 90, 40) * baseAlpha * 0.8f;
            DrawLineBetweenPoints(spriteBatch, pixel, bladeStart + bladeDir * 4f, bladeTip - bladeDir * 8f, 1.5f, fullerColor);

            Vector2 pommelCenter = handleStart - bladeDir * 3f;
            Color pommelColor = new Color(200, 160, 60) * baseAlpha;

            DrawGlowCircle(spriteBatch, pixel, pommelCenter + Main.screenPosition, 4f, pommelColor);
        }
        private void DrawBlade(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 tip, float baseWidth, float tipWidth, Color color)
        {
            Vector2 direction = tip - start;
            float length = direction.Length();
            if (length < 1f) return;

            float rotation = direction.ToRotation();
            Vector2 perpDir = rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
            int segments = 8;

            for (int i = 0; i < segments; i++)
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + 1) / segments;

                Vector2 p1 = Vector2.Lerp(start, tip, t1);
                Vector2 p2 = Vector2.Lerp(start, tip, t2);

                float w1 = MathHelper.Lerp(baseWidth, tipWidth, t1);
                float w2 = MathHelper.Lerp(baseWidth, tipWidth, t2);

                float avgWidth = (w1 + w2) / 2f;
                DrawLineBetweenPoints(spriteBatch, pixel, p1, p2, avgWidth, color);
            }
        }
        private void DrawHighlights(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 center = Projectile.Center;
            float rotation = Projectile.rotation;
            Vector2 bladeDir = rotation.ToRotationVector2();

            float pulse = MathF.Sin(Main.GameUpdateCount * 0.2f + SwordIndex) * 0.5f + 0.5f;
            float highlightAlpha = (0.3f + glowIntensity * 0.4f) * pulse;

            Vector2 highlightStart = center + bladeDir * 5f;
            Vector2 highlightEnd = center + bladeDir * (BLADE_LENGTH - 5f);
            Color highlightColor = new Color(255, 255, 200) * highlightAlpha;

            DrawLineBetweenPoints(spriteBatch, pixel, highlightStart - Main.screenPosition, highlightEnd - Main.screenPosition, 1.5f, highlightColor);
            if ((SwordState)State == SwordState.Attacking)
            {
                for (int i = 0; i < 3; i++)
                {
                    float t = i / 3f + Main.GameUpdateCount * 0.05f % 1f;
                    if (t > 1f) t -= 1f;
                    Vector2 sparkPos = Vector2.Lerp(center, center + bladeDir * BLADE_LENGTH, t);
                    float sparkAlpha = MathF.Sin(t * MathHelper.Pi) * highlightAlpha;
                    DrawGlowCircle(spriteBatch, pixel, sparkPos, 4f, new Color(255, 255, 255) * sparkAlpha);
                }
            }
        }
        private void DrawBladeTipEffect(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 bladeDir = Projectile.rotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + bladeDir * BLADE_LENGTH;
            float intensity = 0.4f + glowIntensity * 0.6f;
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 0.7f;

            Color tipCore = new Color(255, 255, 220) * intensity * pulse;
            DrawGlowCircle(spriteBatch, pixel, tipPos, 6f, tipCore);

            Color tipGlow = new Color(255, 180, 80) * intensity * pulse * 0.5f;
            DrawGlowCircle(spriteBatch, pixel, tipPos, 15f, tipGlow);

            if ((SwordState)State == SwordState.Attacking)
            {
                Color flameColor = new Color(255, 100, 30) * intensity * 0.6f;
                for (int i = 0; i < 5; i++)
                {
                    float offset = i * 4f;
                    Vector2 flamePos = tipPos - bladeDir * offset;
                    float flameSize = 10f - i * 1.5f;
                    DrawGlowCircle(spriteBatch, pixel, flamePos, flameSize, flameColor * (1f - i * 0.15f));
                }
            }
        }
        private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, float rotation, float length, float width, Color color)
        {
            spriteBatch.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
        }
        private void DrawLineBetweenPoints(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            if (length < 0.1f) return;

            float rotation = direction.ToRotation();

            spriteBatch.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
        }
        private void DrawGlowCircle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 worldCenter, float radius, Color color)
        {
            Vector2 screenPos = worldCenter - Main.screenPosition;
            int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                Vector2 p1 = screenPos + angle.ToRotationVector2() * radius * 0.3f;
                Vector2 p2 = screenPos + nextAngle.ToRotationVector2() * radius * 0.3f;
                DrawLineBetweenPoints(spriteBatch, pixel, p1 + Main.screenPosition, p2 + Main.screenPosition, radius * 0.5f, color);
            }
            spriteBatch.Draw(pixel, screenPos, new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f, 0.5f), new Vector2(radius * 0.6f, radius * 0.6f), SpriteEffects.None, 0f);
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 25; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(6f, 6f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 2f);
            }

            for (int i = 0; i < 10; i++)
            {
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(4f, 4f));
                fire.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = -0.3f, Volume = 0.4f }, Projectile.Center);
        }
	}
}
