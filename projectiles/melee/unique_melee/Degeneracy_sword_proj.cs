using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.buffs.debuff;
using jarkos.systems;

namespace jarkos.projectiles.melee.unique_melee
{
	public class Degeneracy_sword_proj : ModProjectile {
		public override string Texture => "jarkos/items/weapons/melee/unique_melee/Zweihander";

		public float SWING_TIME = 22f;
		public float FINISHER_TIME = 34f;
		private const int LINGER = 12;
		private const float BLADE_LEN = 120f;
		private const float ANTIC = 0.15f;


		public static readonly Color ColorBlade = new(240, 170, 20);
		public static readonly Color ColorCyan = new(255, 235, 40);
		public static readonly Color ColorGold = new(255, 255, 140);
		public static readonly Color ColorCore = new(255, 255, 255);

		private static BasicEffect basicFallback;

		private int ComboStep => (int)Projectile.ai[0];
		private bool IsFinisher => ComboStep >= 4;

		private float ChargeTimer = 0f;
		private const float MaxChargeTimer = 75f;
		private float ChargeScale = 1f;
		private bool IsCharging = false;
		private bool ReleasedCharge = false;

		private float StretchK
		{
			get
			{
				float t = MathHelper.Clamp(timer / (float)SwingTimeTotal, 0f, 1f);
				if (t <= ANTIC || timer > SwingTimeTotal)
					return 1f;
				float s = (t - ANTIC) / (1f - ANTIC);
				float bump = (float)Math.Sin(MathHelper.Pi * s);
				bump *= bump;
				return 1f + (IsFinisher ? 0.35f : 0.20f) * bump;
			}
		}

		private float SwingTimeTotal => IsFinisher ? FINISHER_TIME : SWING_TIME;
		private float SwingArc => IsFinisher ? MathHelper.TwoPi + 1.2f : 4.2f;
		private float VisualScale => (IsFinisher ? 1.25f : 1.05f) * ChargeScale;

		private int angDir = 1;
		private float aimAngle;
		private int timer;
		private int freezeFrames;
		private bool strikeSoundPlayed;
		private bool firedProjectiles;
		private float prevRot;
		private float lastAngSpeed;

		private const int MAX_TRAIL = 40;
		private const float TRAIL_MAX_AGE = 12f;
		private int trailCount;
		private readonly float[] trailRot = new float[MAX_TRAIL];
		private readonly float[] trailAge = new float[MAX_TRAIL];
		private readonly float[] trailLen = new float[MAX_TRAIL];

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 600;
			Projectile.ownerHitCheck = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(aimAngle);
			writer.Write(angDir);
			writer.Write(ChargeScale);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			aimAngle = reader.ReadSingle();
			angDir = reader.ReadInt32();
			ChargeScale = reader.ReadSingle();
		}

		public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[Projectile.owner];

			float baseSwingTime = IsFinisher ? 32f : 22f;
			float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
			if (meleeSpeedMultiplier < 0.01f) meleeSpeedMultiplier = 0.01f;

			SWING_TIME = 22f / meleeSpeedMultiplier;
			FINISHER_TIME = 32f / meleeSpeedMultiplier;

			if (Projectile.owner == Main.myPlayer)
			{
				aimAngle = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
				angDir = (int)Projectile.ai[1];
				if (Math.Cos(aimAngle) < 0)
					angDir = -angDir;
				Projectile.netUpdate = true;
			}

			prevRot = CurrentRotation();
			Projectile.rotation = prevRot;

			if (IsFinisher && player.channel)
			{
				IsCharging = true;
			}

			if (Main.netMode != NetmodeID.Server)
			{
				new Circular_glow(Main.player[Projectile.owner].MountedCenter, Vector2.Zero, ColorGold, 14, 0.16f, 0.6f).Spawn();
			}
		}

		private float Eased(float t)
		{
			if (t < ANTIC)
			{
				float a = t / ANTIC;
				return -0.085f * (float)Math.Sin(a * MathHelper.PiOver2);
			}
			float s = (t - ANTIC) / (1f - ANTIC);
			float power = IsFinisher ? 3.2f : 4.4f;
			float e = 1f - (float)Math.Pow(1f - s, power);
			return MathHelper.Lerp(-0.085f, 1f, e);
		}

		private float CurrentRotation()
		{
			float t = MathHelper.Clamp(timer / (float)SwingTimeTotal, 0f, 1f);
			return aimAngle + angDir * SwingArc * (Eased(t) - 0.5f);
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead)
			{
				Projectile.Kill();
				return;
			}
			Projectile.Center = player.MountedCenter;
			Projectile.timeLeft = 2 + LINGER;


			if (IsFinisher && IsCharging && !ReleasedCharge)
			{
				if (player.channel && Main.mouseLeft)
				{
					ChargeTimer++;
					float progress = MathHelper.Clamp(ChargeTimer / MaxChargeTimer, 0f, 0.5f);
					ChargeScale = 1f + progress;

					timer = 0;
					HoldPlayer(player, true);


					Vector2 vibrate = Main.rand.NextVector2Circular(2f * progress, 2f * progress);
					Projectile.Center += vibrate;

					if (Main.rand.NextBool(2))
					{
						Vector2 tip = Projectile.Center + Projectile.rotation.ToRotationVector2() * BLADE_LEN * VisualScale;
						Dust d = Dust.NewDustPerfect(tip + Main.rand.NextVector2Circular(12f, 12f), DustID.GoldFlame, Main.rand.NextVector2Circular(4f, 4f));
						d.noGravity = true;
						d.scale = 1.2f + progress * 1.0f;
					}

					if ((int)ChargeTimer % 12 == 0 && Main.netMode != NetmodeID.Server)
					{
						SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.4f + progress * 0.4f, Pitch = 0.2f + progress * 0.4f }, Projectile.Center);
						new Circular_glow(Projectile.Center, Vector2.Zero, ColorGold, 14, 0.25f * progress, 0.6f).Spawn();
					}

					return;
				}
				else
				{

					ReleasedCharge = true;
					IsCharging = false;
					if (Main.netMode != NetmodeID.Server)
					{
						SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Zweihander_laststageattacks") with { Volume = 1f, Pitch = 0.1f }, Projectile.Center);
					}
				}
			}

			if (freezeFrames > 0 && timer <= SwingTimeTotal)
			{
				freezeFrames--;
				HoldPlayer(player, true);
				return;
			}
			AgeTrail();
			if (timer > SwingTimeTotal + LINGER)
			{
				Projectile.Kill();
				return;
			}
			timer++;
			float rot = CurrentRotation();
			float angSpeed = MathHelper.WrapAngle(rot - prevRot);
			prevRot = rot;
			Projectile.rotation = rot;
			lastAngSpeed = angSpeed;
			bool swinging = timer <= SwingTimeTotal;
			HoldPlayer(player, swinging);
			if (!swinging)
				return;

			float t = timer / (float)SwingTimeTotal;


			if (!strikeSoundPlayed && t >= ANTIC)
			{
				strikeSoundPlayed = true;
				if (Main.netMode != NetmodeID.Server)
				{
					float pitch = ComboStep * 0.08f - 0.2f;
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Zweihander_swing") with { Pitch = pitch }, Projectile.Center);
				}
			}


			if (!firedProjectiles && t >= 0.20f)
			{
				firedProjectiles = true;
				int projCount = IsFinisher ? (int)(5 + (ChargeScale - 1f) * 3f) : (ComboStep + 1);
				if (Projectile.owner == Main.myPlayer)
				{
					Vector2 toCursor = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
					for (int i = 0; i < projCount; i++)
					{
						float spreadAngle = MathHelper.ToRadians(Main.rand.NextFloat(-35f, 35f));
						Vector2 vel = toCursor.RotatedBy(spreadAngle) * (14f + Main.rand.NextFloat(0f, 6f));
						int projDamage = (int)(Projectile.damage * (IsFinisher ? (0.35f * ChargeScale) : 0.4f));
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<Zweihander_proj>(), projDamage, 1f, Projectile.owner);
					}
				}
			}

			if (Math.Abs(angSpeed) > 0.035f)
				PushTrail(rot, BLADE_LEN * VisualScale * StretchK + 6f);

			Lighting.AddLight(Projectile.Center + rot.ToRotationVector2() * BLADE_LEN * 0.6f * VisualScale, ColorCyan.ToVector3() * 0.55f);
		}

		private void HoldPlayer(Player player, bool swinging)
		{
			if (!swinging)
				return;
			player.heldProj = Projectile.whoAmI;
			player.SetDummyItemTime(2);
			player.ChangeDir(Math.Cos(aimAngle) >= 0 ? 1 : -1);
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
		}

		private void PushTrail(float rot, float len)
		{
			int last = Math.Min(trailCount, MAX_TRAIL - 1);
			for (int i = last; i > 0; i--)
			{
				trailRot[i] = trailRot[i - 1];
				trailAge[i] = trailAge[i - 1];
				trailLen[i] = trailLen[i - 1];
			}
			trailRot[0] = rot;
			trailAge[0] = 0f;
			trailLen[0] = len;
			trailCount = Math.Min(trailCount + 1, MAX_TRAIL);
		}

		private void AgeTrail()
		{
			for (int i = 0; i < trailCount; i++)
				trailAge[i] += 1f;
			while (trailCount > 0 && trailAge[trailCount - 1] > TRAIL_MAX_AGE)
				trailCount--;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (timer > SwingTimeTotal)
				return false;
			float t = timer / (float)SwingTimeTotal;
			if (t < ANTIC || t > 0.95f)
				return false;
			float point = 0f;
			Vector2 dir = Projectile.rotation.ToRotationVector2();
			Vector2 start = Projectile.Center + dir * 18f;
			Vector2 end = Projectile.Center + dir * BLADE_LEN * VisualScale * StretchK;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 44f * VisualScale, ref point);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);
			if (IsFinisher)
			{
				modifiers.SourceDamage *= 1.3f * ChargeScale;
				modifiers.Knockback *= 1.5f * ChargeScale;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			freezeFrames = IsFinisher ? 6 : 3;
			target.AddBuff(ModContent.BuffType<Hellfire_debuff>(), 180);

			if (Main.netMode != NetmodeID.Server)
			{
				if (Main.LocalPlayer.whoAmI == Projectile.owner)
				{
					Main.instance.CameraModifiers.Add(new PunchCameraModifier(target.Center, (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), IsFinisher ? 10f : 5f, 7f, IsFinisher ? 16 : 10, 900f, FullName));
				}
				Vector2 hitPos = target.Center;
				Vector2 dirOut = (hitPos - Projectile.Center).SafeNormalize(Vector2.UnitX);
				new Circleedge(hitPos, Vector2.Zero, ColorCyan, 18, 0.16f, 0.85f).Spawn();
				new StarParticle(hitPos, Vector2.Zero, ColorCore, 26, 0.14f, 1f, 0.05f).Spawn();
				for (int i = 0; i < 8; i++)
				{
					Vector2 v = dirOut.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(4f, 9f);
					new Spark(hitPos, v, Main.rand.NextBool(3) ? ColorCore : ColorCyan, 24, Main.rand.NextFloat(0.4f, 0.7f), 0.9f).Spawn();
				}
				for (int i = 0; i < 3; i++)
				{
					new Line_glow(hitPos, Vector2.Zero, ColorBlade, 16, Main.rand.NextFloat(0.5f, 0.8f), 0.7f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
				}
				if (IsFinisher)
				{
					new Circleedge(hitPos, Vector2.Zero, ColorGold, 28, 0.35f, 0.95f, 0.1f).Spawn();
					new Circular_glow(hitPos, Vector2.Zero, ColorCyan, 24, 0.45f, 0.85f).Spawn();
					for (int i = 0; i < 12; i++)
					{
						float a = Main.rand.NextFloat(MathHelper.TwoPi);
						Vector2 from = hitPos + a.ToRotationVector2() * Main.rand.NextFloat(60f, 120f);
						new Stretching_particle_out(from, (hitPos - from) * 0.12f, ColorBlade, 22, Main.rand.NextFloat(40f, 80f), Main.rand.NextFloat(4f, 7f), 0.02f).Spawn();
					}
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Zweihander_impact") with { Pitch = 0.1f }, hitPos);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			DrawTrail(player);
			if (timer <= SwingTimeTotal || IsCharging)
				DrawBlade(player, lightColor);
			return false;
		}

		private static BasicEffect GetBasicFallback(GraphicsDevice gd)
		{
			if (basicFallback == null || basicFallback.IsDisposed)
				basicFallback = new BasicEffect(gd) { TextureEnabled = true, VertexColorEnabled = true };
			return basicFallback;
		}

		private void DrawTrail(Player player)
		{
			if (trailCount < 2)
				return;
			const int SUB = 3;
			int segs = (trailCount - 1) * SUB;
			var verts = new VertexPositionColorTexture[(segs + 1) * 2];
			Vector2 center = player.MountedCenter;
			float innerR = 20f;
			int v = 0;
			for (int i = 0; i <= segs; i++)
			{
				float fi = i / (float)SUB;
				int i0 = (int)fi;
				int i1 = Math.Min(i0 + 1, trailCount - 1);
				float fr = fi - i0;
				float rot = trailRot[i0] + MathHelper.WrapAngle(trailRot[i1] - trailRot[i0]) * fr;
				float age = MathHelper.Lerp(trailAge[i0], trailAge[i1], fr);
				float outerR = MathHelper.Lerp(trailLen[i0], trailLen[i1], fr);
				float p = MathHelper.Clamp(age / TRAIL_MAX_AGE, 0f, 1f);
				float fade = (1f - p) * (1f - p);
				Vector2 dir = rot.ToRotationVector2();
				Vector2 inner = center + dir * innerR;
				Vector2 outer = center + dir * outerR;
				Color cIn = ColorBlade * (0.45f * fade);
				Color cOut = Color.Lerp(ColorCyan, ColorGold, p * 0.8f) * (0.9f * fade);
				float u = i / (float)segs * 1.6f;
				verts[v] = new VertexPositionColorTexture(new Vector3(inner, 0f), cIn, new Vector2(u, 0f));
				verts[v + 1] = new VertexPositionColorTexture(new Vector3(outer, 0f), cOut, new Vector2(u, 1f));
				v += 2;
			}
			Main.spriteBatch.End();
			GraphicsDevice gd = Main.instance.GraphicsDevice;
			gd.BlendState = BlendState.Additive;
			gd.RasterizerState = RasterizerState.CullNone;
			gd.DepthStencilState = DepthStencilState.None;
			gd.SamplerStates[0] = SamplerState.LinearWrap;
			Matrix wvp = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
				* Main.GameViewMatrix.TransformationMatrix
				* Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);
			Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/RiftNoise").Value;
			BasicEffect be = GetBasicFallback(gd);
			be.World = Matrix.Identity;
			be.View = Matrix.Identity;
			be.Projection = wvp;
			be.Texture = noise;
			be.CurrentTechnique.Passes[0].Apply();
			gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, verts.Length - 2);
			RestartSpriteBatch();
		}

		private static void RestartSpriteBatch()
		{
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawBlade(Player player, Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 drawPos = player.MountedCenter - Main.screenPosition;
			bool flip = Math.Cos(aimAngle) < 0;
			SpriteEffects fxFlip = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 origin = flip
				? new Vector2(tex.Width - 6f, tex.Height - 6f)
				: new Vector2(6f, tex.Height - 6f);
			float rotOffset = flip ? MathHelper.Pi * 0.75f : MathHelper.PiOver4;
			float texDiag = (float)Math.Sqrt(tex.Width * tex.Width + tex.Height * tex.Height);
			float scale = BLADE_LEN * VisualScale / texDiag * 1.08f;
			float stretch = StretchK;
			float speedFactor = MathHelper.Clamp(Math.Abs(lastAngSpeed) / 0.3f, 0f, 1f);

			int ghosts = (int)(speedFactor * 5f);
			for (int i = Math.Min(ghosts, trailCount - 1); i >= 1; i--)
			{
				float a = (1f - i / 6f) * 0.3f * speedFactor;
				Color ghost = new Color(ColorCyan.R, ColorCyan.G, ColorCyan.B, 0) * a;
				Main.spriteBatch.Draw(tex, drawPos, null, ghost, trailRot[i] + rotOffset, origin, scale * MathHelper.Lerp(1f, stretch, 0.35f) * (1f - i * 0.015f), fxFlip, 0f);
			}

			if (speedFactor > 0.1f || IsCharging)
			{
				Color glow = new Color(ColorBlade.R, ColorBlade.G, ColorBlade.B, 0) * (0.45f * (IsCharging ? (ChargeScale / 2.5f) : speedFactor));
				Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation + rotOffset, origin, scale * MathHelper.Lerp(1f, stretch, 0.35f) * 1.07f, fxFlip, 0f);
			}

			float metalScale = scale * MathHelper.Lerp(1f, stretch, 0.35f);
			Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation + rotOffset, origin, metalScale, fxFlip, 0f);

			if (stretch > 1.02f || IsCharging)
			{
				float over = IsCharging ? (ChargeScale / 2.5f) : ((stretch - 1f) / 0.35f);
				for (int i = 1; i <= 4; i++)
				{
					float k = i / 4f;
					float ext = scale * MathHelper.Lerp(MathHelper.Lerp(1f, stretch, 0.35f), stretch, k);
					Color c = Color.Lerp(ColorCyan, ColorCore, k);
					Color energy = new Color(c.R, c.G, c.B, 0) * ((1f - k * 0.55f) * 0.5f * over);
					Main.spriteBatch.Draw(tex, drawPos, null, energy, Projectile.rotation + rotOffset, origin, ext, fxFlip, 0f);
				}
			}
		}
	}
}
