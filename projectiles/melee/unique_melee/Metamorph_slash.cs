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
using jarkos.particles;
using jarkos.items.weapons.melee.true_melee;
using System.Collections.Generic;

namespace jarkos.projectiles.melee.unique_melee
{
    public class Metamorph_slash : ModProjectile
    {
        public float SWINGRANGE = (float)Math.PI;
		private const float FIRSTHALFSWING = 0.45f;
		private const float WINDUP = 0.001f;
		private const float UNWIND = 0.001f;
		private enum AttackStage
		{
			Prepare, Impact, Unwind
		}
		private AttackStage CurrentStage
		{
			get => (AttackStage)Projectile.localAI[0];
			set
			{
				Projectile.localAI[0] = (float)value;
				Timer = 0;
			}
		}
		private int alpha_runtime = 255;
		private ref float StartAngle => ref Projectile.ai[1];
		private ref float Timer => ref Projectile.ai[2];
		private ref float Progress => ref Projectile.localAI[1];
		private ref float Size => ref Projectile.localAI[2];
		private const int DissapearTimer = 20;
		private float DissapearProgress;
		private bool hitted = false;
		private bool hit_cap_condition = false;
		private float hit_delay_back;
		private float slowing = 1f;
		private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float impactTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float hideTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private Player Owner => Main.player[Projectile.owner];
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}
		public override void SetDefaults()
		{
			Projectile.width = 104;
			Projectile.height = 104;
			Projectile.friendly = true;
			Projectile.timeLeft = 10000;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
		}
		public override void OnSpawn(IEntitySource source)
		{
			Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
			float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
			StartAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection;
		}
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((sbyte)Projectile.spriteDirection);
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.spriteDirection = reader.ReadSByte();
		}
		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, 1f, 0.75f, 0.5f);
			Owner.itemAnimation = 2;
			Owner.itemTime = 2;
			alpha_runtime -= 5;
			slowing = 1f;
			if (alpha_runtime < 0)
			{
				alpha_runtime = 0;
			}
			if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
				{
					Projectile.Kill();
					return;
				}
			switch (CurrentStage)
			{
				case AttackStage.Prepare:
					PrepareStrike();
					break;
				case AttackStage.Impact:
					ImpactStrike();
					break;
				default:
					UnwindStrike();
					break;
			}
			SetSwordPosition();
			if(hit_cap_condition)
			{
				hit_delay_back++;
				if(hit_delay_back > 4f)
				{
					hitted = false;
				}
			}
			if(!hitted)
			{
				Timer++;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D glow = ModContent.Request<Texture2D>("jarkos/textures/Circle_slashglow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin;
			float rotationOffset;
			float rotationOffsetGlow;
			SpriteEffects effects;
			if (Projectile.spriteDirection > 0)
			{
				origin = new Vector2(0, Projectile.height);
				rotationOffset = MathHelper.ToRadians(45f);
				rotationOffsetGlow = -MathHelper.ToRadians(90f);
				effects = SpriteEffects.None;
			}
			else
			{
				origin = new Vector2(Projectile.width, Projectile.height);
				rotationOffset = MathHelper.ToRadians(135f);
				rotationOffsetGlow = -MathHelper.ToRadians(90f);
				effects = SpriteEffects.FlipHorizontally;
			}
			float bladeLength = 1f * Projectile.scale * 0.85f;
			Vector2 tipPosition = Projectile.Center + Projectile.rotation.ToRotationVector2() * bladeLength;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Color glowColor = new Color(39, 245, 197);
			float opacity = (255 - Projectile.alpha) / 255f;
			Color drawColor = glowColor * opacity;
			float glowScale = Projectile.scale * 0.24f;
			Vector2 glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
			Main.spriteBatch.Draw(glow, tipPosition - Main.screenPosition, null, drawColor * Projectile.Opacity, Projectile.rotation + rotationOffsetGlow, glowOrigin, glowScale, SpriteEffects.None, 0);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale * 0.5f, effects, 0);
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 0.5f);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
		}
		public override void CutTiles()
		{
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
			Terraria.Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool? CanDamage()
		{
			if (CurrentStage == AttackStage.Prepare)
				return false;
			return base.CanDamage();
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;
		}
		public void SetSwordPosition()
		{
			Projectile.rotation = StartAngle + Projectile.spriteDirection * Progress;
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2);
			armPosition.Y += Owner.gfxOffY;
			Projectile.Center = armPosition;
			Projectile.scale = Size * 1.89f * Owner.GetAdjustedItemScale(Owner.HeldItem);
			Owner.heldProj = Projectile.whoAmI;
		}
		private void PrepareStrike()
		{
			Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime);
			Size = MathHelper.SmoothStep(0.99f, 1, Timer / prepTime);
			if (Timer >= prepTime / 2)
			{
				SoundEngine.PlaySound(SoundID.Item21 with { Pitch = 0.5f }, Projectile.Center);
				CurrentStage = AttackStage.Impact;
			}
		}
		private void ImpactStrike()
		{
			Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / impactTime * slowing);
			DissapearProgress++;
			float ProgressDis = DissapearProgress / DissapearTimer;
			Projectile.alpha = (int)(255 * ProgressDis);
			if (Timer >= impactTime)
			{
				CurrentStage = AttackStage.Unwind;
			}
		}
		private void UnwindStrike()
		{
			Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
			Size = 1f - MathHelper.SmoothStep(0.85f, 1, Timer / hideTime);
			if (Timer >= hideTime)
			{
				Projectile.Kill();
			}
		}
		const float max_delay_attack = 1f;
		float delay_attack;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
			if(!hit_cap_condition)
			{
				hitted = true;
			}
			hit_cap_condition = true;
			owner.GetModPlayer<Screenshake_system>().stronger_shake++;
			for (int i = 0; i < 5; i++)
			{
				Vector2 new_vel = Main.MouseWorld - target.Center;
				new_vel.Normalize();
				float size_at_pitch = Main.rand.NextFloat(1.45f, 2.25f) / 2;
				float speed = Main.rand.NextFloat(12f, 18f) / 2;
				new Spark_gravit(target.Center, new_vel.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-75f, 75f))) * speed, Color.Orange, 300, size_at_pitch, 1f, 0f).Spawn();
			}
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.6f }, target.Center);
        }
    }
	#region EvolutionSlash
    public class EvolutionSlash : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/EvolutionSlash";
        private List<Vector2> trailPositions = new List<Vector2>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            Color color = EvolutionBladePlayer.StageColors[mp.evolutionStage];
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 20)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch);
                d.noGravity = true;
                d.scale = 1f + mp.evolutionStage * 0.15f;
                d.color = color;
                d.velocity *= 0.3f;
            }
            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);
            Projectile.alpha += 3;
            Projectile.scale -= 0.02f;
            if (Projectile.scale <= 0.1f)
                Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            if (target.life <= 0)
            {
                mp.AddKill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            Color color = EvolutionBladePlayer.StageColors[mp.evolutionStage];
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float progress = (float)i / trailPositions.Count;
                float alpha = 1f - progress;
                float scale = Projectile.scale * (1f - progress * 0.5f);
                Color trailColor = color * alpha * 0.6f;
                Vector2 pos = trailPositions[i] - Main.screenPosition;
                sb.Draw(texture, pos, null, trailColor, Projectile.rotation, texture.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
            return true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            return EvolutionBladePlayer.StageColors[mp.evolutionStage] * (1f - Projectile.alpha / 255f);
        }
    }
    #endregion
    #region StoneShatter
    public class StoneShatter : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/StoneShatter";
        private List<Vector2> trailPositions = new List<Vector2>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Projectile.rotation += 0.3f * Math.Sign(Projectile.velocity.X);
            Projectile.velocity.Y += 0.3f;
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 25)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
                d.noGravity = true;
                d.scale = 0.8f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Stone, vel.X, vel.Y);
                d.scale = 1.5f;
            }
            for (int i = 0; i < 3; i++)
            {
                int goreType = Main.rand.Next(61, 64);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), goreType);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            if (target.life <= 0)
                mp.AddKill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color bronzeColor = Color.Gray;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = 1f - progress;
                float width = (1f - progress) * 8f;
                DrawLine(sb, pixel, start, end, bronzeColor * alpha, width);
            }
            return true;
        }
        private void DrawLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0f);
        }
    }
    #endregion
    #region BronzeSpiral
    public class BronzeSpiral : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/BronzeSpiral";
        private float spiralAngle = 0f;
        private List<Vector2> trailPositions = new List<Vector2>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            spiralAngle += 0.15f;
            Vector2 spiralOffset = new Vector2(0, 30f).RotatedBy(spiralAngle);
            Projectile.position += spiralOffset * 0.1f;
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 25)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Copper);
                d.noGravity = true;
                d.scale = 1.2f;
                d.velocity = Projectile.velocity * 0.2f;
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.5f, 0.2f) * 0.5f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color bronzeColor = new Color(205, 127, 50);
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = 1f - progress;
                float width = (1f - progress) * 8f;
                DrawLine(sb, pixel, start, end, bronzeColor * alpha, width);
            }
            return true;
        }
        private void DrawLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(8f, 8f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<BronzeShard>(), Projectile.damage / 3, Projectile.knockBack / 2, Projectile.owner);
            }
            if (target.life <= 0)
                mp.AddKill();
        }
    }
    public class BronzeShard : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/BronzeShard";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Projectile.rotation += 0.4f;
            Projectile.velocity.Y += 0.2f;
            Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Copper);
            d.noGravity = true;
            d.scale = 0.6f;
            d.velocity = Vector2.Zero;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            if (target.life <= 0)
                mp.AddKill();
        }
    }
    #endregion
    #region IronMagnet
    public class IronMagnet : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/IronMagnet";
        private List<Vector2> trailPositions = new List<Vector2>();
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 15)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation += 0.2f;
            Projectile.velocity *= 0.97f;
            float magnetRadius = 200f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.boss && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = npc.Distance(Projectile.Center);
                    if (dist < magnetRadius && dist > 10f)
                    {
                        Vector2 direction = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                        float pullStrength = (1f - dist / magnetRadius) * 1.1f;
                        npc.velocity += direction * pullStrength;
                        if (Main.rand.NextBool(5))
                        {
                            Dust d = Dust.NewDustDirect(Vector2.Lerp(npc.Center, Projectile.Center, Main.rand.NextFloat()), 0, 0, DustID.Electric);
                            d.noGravity = true;
                            d.scale = 0.8f;
                            d.velocity = direction * 2f;
                        }
                    }
                }
            }
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Iron);
                d.noGravity = true;
                d.scale = 1f;
            }
            if (Main.rand.NextBool(10))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), 0, 0, DustID.Electric);
                d.noGravity = true;
                d.scale = 1.5f;
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.5f, 0.6f) * 0.6f);
            if (Projectile.velocity.Length() < 1f)
                Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color ironColor = new Color(160, 160, 170);
            float magnetRadius = 200f;
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f + Main.GameUpdateCount * 0.02f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * magnetRadius - Main.screenPosition;
                sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), ironColor * 0.3f, 0f, new Vector2(0.5f), 4f, SpriteEffects.None, 0f);
            }
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            target.AddBuff(BuffID.Slow, 120);
            target.AddBuff(BuffID.Confused, 60);
            if (target.life <= 0)
                mp.AddKill();
        }
    }
    #endregion
    #region SteelRicochet
    public class SteelRicochet : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/SteelRicochet";
        private int bounceCount = 0;
        private List<Vector2> trailPositions = new List<Vector2>();
        private List<int> hitNPCs = new List<int>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 10;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 40)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            if (bounceCount > 0)
            {
                NPC target = FindNearestEnemy(300f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, 0.1f) * Projectile.velocity.Length();
                }
            }
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Silver);
                d.noGravity = true;
                d.scale = 1.2f;
                d.velocity = -Projectile.velocity * 0.2f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch);
                d.noGravity = true;
                d.scale = 0.8f;
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.6f, 0.7f) * 0.5f);
        }
        private NPC FindNearestEnemy(float maxDistance)
        {
            NPC nearest = null;
            float nearestDist = maxDistance;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !hitNPCs.Contains(npc.whoAmI))
                {
                    float dist = npc.Distance(Projectile.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }
            return nearest;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            hitNPCs.Add(target.whoAmI);
            bounceCount++;
            Projectile.damage = (int)(Projectile.damage * 1.1f);
            NPC nextTarget = FindNearestEnemy(400f);
            if (nextTarget != null)
            {
                Vector2 toTarget = (nextTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = toTarget * (Projectile.velocity.Length() * 1.05f);
                for (int i = 0; i < 10; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch);
                    d.noGravity = true;
                    d.velocity = Main.rand.NextVector2Circular(5f, 5f);
                    d.scale = 1.5f;
                }
                SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            }
            if (target.life <= 0)
                mp.AddKill();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounceCount++;
            if (bounceCount >= 5)
                return true;
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
            SoundEngine.PlaySound(SoundID.Tink, Projectile.Center);
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color steelColor = new Color(180, 180, 200);
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * 0.8f;
                float width = (1f - progress) * 6f;
                Color trailColor = Color.Lerp(steelColor, Color.Orange, progress * 0.5f) * alpha;
                Vector2 edge = end - start;
                float angle = (float)Math.Atan2(edge.Y, edge.X);
                float length = edge.Length();
                sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0f);
            }
            return true;
        }
    }
    #endregion
    #region MythrilWave
    public class MythrilWave : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/MythrilWave";
        private float waveWidth = 0f;
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.scale = 1.5f;
        }
        public override void AI()
        {
            waveWidth = Math.Min(waveWidth + 10f, 150f);
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.width = (int)waveWidth;
            Projectile.height = (int)(waveWidth * 2);
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(waveWidth / 2, waveWidth);
                Vector2 pos = Projectile.Center + offset.RotatedBy(Projectile.rotation);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Mythril);
                d.noGravity = true;
                d.scale = 1.5f;
                d.velocity = Projectile.velocity * 0.3f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(waveWidth / 2, waveWidth / 2), 0, 0, DustID.GreenTorch);
                d.noGravity = true;
                d.scale = 2f;
                d.velocity = Vector2.Zero;
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.8f, 0.5f) * 0.8f);
            Projectile.alpha += 4;
            if (Projectile.alpha >= 255)
                Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color mythrilColor = new Color(100, 200, 150);
            float halfWidth = waveWidth / 2;
            for (int i = 0; i < 20; i++)
            {
                float t = i / 20f;
                float waveSin = (float)Math.Sin(t * MathHelper.Pi + Main.GameUpdateCount * 0.1f) * halfWidth;
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 lineStart = Projectile.Center + perpendicular * waveSin - Main.screenPosition;
                Vector2 lineEnd = Projectile.Center - perpendicular * waveSin - Main.screenPosition;
                float alpha = 1f - Projectile.alpha / 255f;
                Color lineColor = mythrilColor * alpha * (1f - t * 0.5f);
                Vector2 edge = lineEnd - lineStart;
                float angle = (float)Math.Atan2(edge.Y, edge.X);
                float length = edge.Length();
                sb.Draw(pixel, lineStart, new Rectangle(0, 0, 1, 1), lineColor, angle, Vector2.Zero, new Vector2(length, 4f), SpriteEffects.None, 0f);
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            target.AddBuff(BuffID.CursedInferno, 180);
            if (target.life <= 0)
                mp.AddKill();
        }
    }
    #endregion
    #region AdamantiteVampire
    public class AdamantiteVampire : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/AdamantiteVampire";
        private List<Vector2> trailPositions = new List<Vector2>();
        private float orbitalAngle = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 30)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            orbitalAngle += 0.2f;
            Projectile.rotation += 0.15f;
            NPC target = FindNearestEnemy(400f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, 0.03f) * Projectile.velocity.Length();
            }
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Adamantite);
                d.noGravity = true;
                d.scale = 1.3f;
                d.velocity = -Projectile.velocity * 0.1f;
            }
            for (int i = 0; i < 3; i++)
            {
                float angle = orbitalAngle + MathHelper.TwoPi * i / 3f;
                Vector2 orbitalPos = Projectile.Center + angle.ToRotationVector2() * 25f;
                Dust d = Dust.NewDustDirect(orbitalPos, 0, 0, DustID.Shadowflame);
                d.noGravity = true;
                d.scale = 0.8f;
                d.velocity = Vector2.Zero;
            }
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.3f, 0.3f) * 0.6f);
        }
        private NPC FindNearestEnemy(float maxDistance)
        {
            NPC nearest = null;
            float nearestDist = maxDistance;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = npc.Distance(Projectile.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }
            return nearest;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            int healAmount = Math.Max(1, damageDone / 8);
            owner.Heal(healAmount);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<HealingOrb>(), 0, 0, Projectile.owner);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = (owner.Center - target.Center).SafeNormalize(Vector2.Zero) * 5f + Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.LifeDrain, vel.X, vel.Y);
                d.noGravity = true;
                d.scale = 1.5f;
            }
            target.AddBuff(BuffID.ShadowFlame, 180);
            if (target.life <= 0)
                mp.AddKill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * 0.7f;
                float width = (1f - progress) * 10f;
                Color trailColor = Color.Lerp(new Color(255, 100, 100), new Color(100, 0, 50), progress) * alpha;
                Vector2 edge = end - start;
                float angle = (float)Math.Atan2(edge.Y, edge.X);
                float length = edge.Length();
                sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0f);
            }
            return true;
        }
    }
    public class HealingOrb : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/HealingOrb";
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 toOwner = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner * 15f, 0.1f);
            Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.LifeDrain);
            d.noGravity = true;
            d.scale = 1f;
            d.velocity = Vector2.Zero;
            Lighting.AddLight(Projectile.Center, new Vector3(0f, 1f, 0.3f) * 0.4f);
            if (Projectile.Distance(owner.Center) < 30f)
            {
                owner.Heal(5);
                Projectile.Kill();
                for (int i = 0; i < 15; i++)
                {
                    Dust d2 = Dust.NewDustDirect(owner.Center, 0, 0, DustID.LifeDrain);
                    d2.noGravity = true;
                    d2.velocity = Main.rand.NextVector2Circular(5f, 5f);
                    d2.scale = 1.5f;
                }
            }
        }
    }
    #endregion
    #region LuminiteLightning
    public class LuminiteLightning : ModProjectile
    {
        public override string Texture => "jarkos/textures/evo_textures/LuminiteLightning";
        private List<List<Vector2>> lightningBolts = new List<List<Vector2>>();
        private List<Vector2> mainTrail = new List<Vector2>();
        private int updateCounter = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            updateCounter++;
            mainTrail.Insert(0, Projectile.Center);
            if (mainTrail.Count > 40)
                mainTrail.RemoveAt(mainTrail.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            NPC target = FindNearestEnemy(600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, 0.08f) * Projectile.velocity.Length();
            }
            if (updateCounter % 10 == 0)
            {
                lightningBolts.Clear();
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy() && npc.Distance(Projectile.Center) < 300f)
                    {
                        List<Vector2> bolt = GenerateLightningBolt(Projectile.Center, npc.Center, 8);
                        lightningBolts.Add(bolt);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            npc.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 0f, DamageClass.Melee);
                        }
                    }
                }
            }
            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex);
                d.noGravity = true;
                d.scale = 0.3f;
                d.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
            }
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), 0, 0, DustID.RainbowTorch);
                d.noGravity = true;
                d.scale = 0.2f;
                d.color = Main.hslToRgb(Main.GameUpdateCount * 0.02f % 1f, 1f, 0.6f);
                d.velocity = Vector2.Zero;
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0f, 1f, 0.8f) * 0.9f);
        }
        private List<Vector2> GenerateLightningBolt(Vector2 start, Vector2 end, int segments)
        {
            List<Vector2> points = new List<Vector2>();
            points.Add(start);
            Vector2 direction = (end - start) / segments;
            float perpAmount = 30f;
            for (int i = 1; i < segments; i++)
            {
                Vector2 basePos = start + direction * i;
                Vector2 perp = new Vector2(-direction.Y, direction.X).SafeNormalize(Vector2.Zero);
                float offset = Main.rand.NextFloat(-perpAmount, perpAmount);
                points.Add(basePos + perp * offset);
            }
            points.Add(end);
            return points;
        }
        private NPC FindNearestEnemy(float maxDistance)
        {
            NPC nearest = null;
            float nearestDist = maxDistance;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = npc.Distance(Projectile.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }
            return nearest;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<EvolutionBladePlayer>();
            target.AddBuff(BuffID.Electrified, 180);
            target.AddBuff(BuffID.OnFire, 180);
            target.AddBuff(BuffID.CursedInferno, 180);
            target.AddBuff(BuffID.ShadowFlame, 180);
            int healAmount = Math.Max(1, damageDone / 10);
            owner.Heal(healAmount);
            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Vortex, vel.X, vel.Y);
                d.noGravity = true;
                d.scale = 0.2f;
            }
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && npc.Distance(target.Center) < 200f)
                {
                    List<Vector2> bolt = GenerateLightningBolt(target.Center, npc.Center, 5);
                    for (int i = 0; i < bolt.Count - 1; i++)
                    {
                        Vector2 a = bolt[i];
                        Vector2 b = bolt[i + 1];
                        for (int j = 0; j < 5; j++)
                        {
                            Vector2 pos = Vector2.Lerp(a, b, Main.rand.NextFloat());
                            Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Electric);
                            d.noGravity = true;
                            d.scale = 0.2f;
                            d.velocity = Vector2.Zero;
                        }
                    }
                    if (Main.myPlayer == Projectile.owner)
                    {
                        npc.SimpleStrikeNPC(damageDone / 2, hit.HitDirection, false, 0f, DamageClass.Melee);
                    }
                }
            }
            if (target.life <= 0)
                mp.AddKill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < mainTrail.Count - 1; i++)
            {
                Vector2 start = mainTrail[i] - Main.screenPosition;
                Vector2 end = mainTrail[i + 1] - Main.screenPosition;
                float progress = (float)i / mainTrail.Count;
                float alpha = (1f - progress) * 0.9f;
                float width = (1f - progress) * 12f;
                Color trailColor = Main.hslToRgb((Main.GameUpdateCount * 0.01f + progress * 0.3f) % 1f, 1f, 0.6f) * alpha;
                DrawLine(sb, pixel, start, end, trailColor, width);
                DrawLine(sb, pixel, start, end, Color.White * alpha * 0.5f, width * 0.5f);
            }
            foreach (var bolt in lightningBolts)
            {
                for (int i = 0; i < bolt.Count - 1; i++)
                {
                    Vector2 start = bolt[i] - Main.screenPosition;
                    Vector2 end = bolt[i + 1] - Main.screenPosition;
                    float alpha = 0.8f;
                    Color boltColor = Color.Cyan * alpha;
                    DrawLine(sb, pixel, start, end, boltColor, 3f);
                    DrawLine(sb, pixel, start, end, Color.White * alpha, 1.5f);
                }
            }
            return true;
        }
        private void DrawLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            if (length > 0)
            {
                sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, angle, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                Vector2 vel = angle.ToRotationVector2() * 10f;
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Vortex, vel.X, vel.Y);
                d.noGravity = true;
                d.scale = 2.5f;
            }
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 vel = angle.ToRotationVector2() * 8f;
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowTorch, vel.X, vel.Y);
                d.noGravity = true;
                d.scale = 2f;
                d.color = Main.hslToRgb((float)i / 36f, 1f, 0.6f);
            }
        }
    }
    #endregion
}
