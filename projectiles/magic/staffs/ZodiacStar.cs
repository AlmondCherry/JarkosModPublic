using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using jarkos.particles;
namespace jarkos.projectiles.magic.staffs
{
	public class ZodiacStar : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FallingStar;
		public const int StarCount = 5;
		public const int SpawnDelayPerStar = 5;
		public const int RevealDuration = 15;
		public const int AllStarsRevealedTick = (StarCount - 1) * SpawnDelayPerStar + RevealDuration;
		public const int LineDrawDuration = 15;
		public const int DriftStartTick = AllStarsRevealedTick + LineDrawDuration;
		public const int Lifetime = 60 * 14;
		public const float SlotFollowSpeedFactor = 0.32f;
		public const float SlotFollowMaxSpeed = 9f;
		public const float SlotFollowBlend = 0.38f;
		public const float MinRotationSpeed = 0.0035f;
		public const float MaxRotationSpeed = 0.0120f;
		public const float WobbleAmplitude = 2.6f;
		public const float WobbleFreqX = 1.7f;
		public const float WobbleFreqY = 2.1f;
		public const float CursorLeanRadius = 110f;
		public const float CursorLeanMaxPull = 7f;
		public const int NetSyncInterval = 30;
		public const float HuntMaxSpeed = 18f;
		public const float HuntBlend = 0.16f;
		public const float ArrivalRadius = 18f;
		public const string FlowShaderKey = "jarkos:ZodiacFlow";
		private Vector2 _nextSiblingPos;
		private bool _hasNextSibling;
		private int _huntFadeTimer;
		private Vector2 _idealOffset;
		private bool _captured;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 1;
			Projectile.timeLeft = Lifetime;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.alpha = 0;
		}
		public override void OnSpawn(IEntitySource source)
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.localAI[0] = float.NaN;
			Projectile.localAI[1] = 0f;
		}
		private Vector2 SpawnCentroid => new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
		private bool HasSpawnState => _captured;
		private void TryCaptureSpawnState()
		{
			if (_captured) return;
			int count = CountSiblings();
			if (count < StarCount) return;
			Vector2 centroid = ComputeGroupCenter();
			_idealOffset = Projectile.Center - centroid;
			Projectile.localAI[0] = centroid.X;
			Projectile.localAI[1] = centroid.Y;
			_captured = true;
		}
		private static float ComputeRotationSpeed(int constId)
		{
			unchecked
			{
				uint h = (uint)constId * 2654435761u;
				h ^= h >> 13;
				h *= 0x27d4eb2du;
				h ^= h >> 15;
				float t = (h & 0xFFFFu) / 65535f;
				float magnitude = MinRotationSpeed + t * (MaxRotationSpeed - MinRotationSpeed);
				bool reverse = ((h >> 16) & 1u) == 0u;
				return reverse ? -magnitude : magnitude;
			}
		}
		public override void AI()
		{
			if (Projectile.ai[2] <= -2000f)
			{
				float timer = -2000f - Projectile.ai[2];
				if (timer <= 0f)
				{
					ExplodeArea();
					Projectile.Kill();
				}
				else
				{
					Projectile.ai[2] += 1f;
					Projectile.velocity *= 0.85f;
					Projectile.rotation += 0.25f;
					if (Main.rand.NextBool(2))
					{
						int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
						Main.dust[d].noGravity = true;
					}
				}
				return;
			}
			int globalAge = Lifetime - Projectile.timeLeft;
			int starIdx = (int)Projectile.ai[0];
			int spawnDelay = (starIdx - 1) * SpawnDelayPerStar;
			int localAge = globalAge - spawnDelay;
			RefreshSiblingCache();
			TryCaptureSpawnState();
			if (localAge < 0)
			{
				Projectile.velocity = Vector2.Zero;
				return;
			}
			if (localAge == 0)
				PlayRevealSound(starIdx);
			int target = (int)Projectile.ai[2];
			if (target >= 0)
			{
				_huntFadeTimer++;
				DoHuntPhase(target);
				EmitStarLight(globalAge, hunting: true);
				return;
			}
			if (globalAge < AllStarsRevealedTick)
			{
				DoRevealPhase(localAge);
				EmitStarLight(globalAge, hunting: false);
				EmitIntroSparkles(localAge);
				return;
			}
			if (globalAge < DriftStartTick)
			{
				DoLinkHoldPhase();
				EmitStarLight(globalAge, hunting: false);
				return;
			}
			DoDriftPhase(globalAge, starIdx);
			EmitStarLight(globalAge, hunting: false);
		}
		private void PlayRevealSound(int starIdx)
		{
			SoundStyle s = SoundID.Item9 with
			{
				Volume = 0.28f, Pitch = -0.25f + (starIdx - 1) * 0.12f, PitchVariance = 0.05f, };
			SoundEngine.PlaySound(s, Projectile.Center);
		}
		private void DoRevealPhase(int localAge)
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.rotation += 0.05f;
		}
		private void DoLinkHoldPhase()
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.rotation += 0.04f;
		}
		private void DoDriftPhase(int globalAge, int starIdx)
		{
			if (!HasSpawnState)
			{
				Projectile.velocity *= 0.9f;
				return;
			}
			int constId = Math.Abs((int)Projectile.ai[1]);
			float rotSpeed = ComputeRotationSpeed(constId);
			float driftAge = globalAge - DriftStartTick;
			float angle = driftAge * rotSpeed;
			Vector2 rotatedOffset = _idealOffset.RotatedBy(angle);
			float tSeconds = driftAge / 60f;
			Vector2 wobble = new Vector2((float)Math.Sin(tSeconds * WobbleFreqX + starIdx * 0.4f), (float)Math.Cos(tSeconds * WobbleFreqY + starIdx * 0.6f)) * WobbleAmplitude;
			Vector2 slot = SpawnCentroid + rotatedOffset + wobble;
			if (Projectile.owner == Main.myPlayer)
			{
				Vector2 toCursor = Main.MouseWorld - slot;
				float distCursor = toCursor.Length();
				if (distCursor < CursorLeanRadius && distCursor > 0.01f)
				{
					float leanT = 1f - distCursor / CursorLeanRadius;
					slot += (toCursor / distCursor) * CursorLeanMaxPull * leanT;
				}
				if (driftAge > 0 && (int)driftAge % NetSyncInterval == 0)
					Projectile.netUpdate = true;
			}
			Vector2 toSlot = slot - Projectile.Center;
			float distToSlot = toSlot.Length();
			Vector2 desired = Vector2.Zero;
			if (distToSlot > 0.5f)
			{
				float followSpeed = MathHelper.Min(distToSlot * SlotFollowSpeedFactor, SlotFollowMaxSpeed);
				desired = (toSlot / distToSlot) * followSpeed;
			}
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, SlotFollowBlend);
			float speed = Projectile.velocity.Length();
			if (speed > SlotFollowMaxSpeed) Projectile.velocity *= SlotFollowMaxSpeed / speed;
			if (Projectile.velocity.LengthSquared() > 0.05f)
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		private void DoHuntPhase(int targetIdx)
		{
			Vector2 target = ResolveHuntTarget(targetIdx);
			Vector2 toTarget = target - Projectile.Center;
			float dist = toTarget.Length();
			if (dist > 1f)
			{
				Vector2 desired = (toTarget / dist) * HuntMaxSpeed;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HuntBlend);
			}
			float speed = Projectile.velocity.Length();
			if (speed > HuntMaxSpeed) Projectile.velocity *= HuntMaxSpeed / speed;
			if (Projectile.velocity.LengthSquared() > 0.01f)
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Main.rand.NextBool(2))
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, 0f, 0f, 100, default, 0.95f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= 0.3f;
			}
			if (Main.rand.NextBool(6))
			{
				Color cosmic = GetCosmicColor(Projectile.timeLeft);
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight, 0f, 0f, 100, cosmic, 1.1f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.6f, 0.6f);
			}
			if (dist < ArrivalRadius)
			{
				NPC npc = (targetIdx >= 0 && targetIdx < Main.maxNPCs) ? Main.npc[targetIdx] : null;
				bool alive = npc != null && npc.active && !npc.dontTakeDamage && npc.life > 0;
				if (!alive) Projectile.Kill();
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if ((int)Projectile.ai[1] < 0)
				modifiers.SetCrit();
		}
		private void EmitStarLight(int globalAge, bool hunting)
		{
			float pulse = 0.85f + 0.15f * (float)Math.Sin(globalAge * 0.20f);
			float r, g, b;
			if (hunting)
			{
				r = 0.55f * pulse;
				g = 0.48f * pulse;
				b = 0.32f * pulse;
			}
			else
			{
				r = 0.32f * pulse;
				g = 0.32f * pulse;
				b = 0.48f * pulse;
			}
			Lighting.AddLight(Projectile.Center, r, g, b);
		}
		private void EmitIntroSparkles(int localAge)
		{
			if (localAge <= RevealDuration && Main.rand.NextBool(2))
			{
				Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
				int d = Dust.NewDust(Projectile.Center + offset, 1, 1, DustID.YellowStarDust, 0f, 0f, 100, default, 1.4f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity = -offset * 0.06f;
			}
			else if (Main.rand.NextBool(10))
			{
				Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
				int d = Dust.NewDust(Projectile.Center + offset, 1, 1, DustID.YellowStarDust, 0f, 0f, 120, default, 1.0f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= 0.2f;
			}
		}
		private void RefreshSiblingCache()
		{
			int starIdx = (int)Projectile.ai[0];
			int nextIdx = (starIdx >= StarCount) ? 1 : starIdx + 1;
			int constId = Math.Abs((int)Projectile.ai[1]);
			_hasNextSibling = false;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (!p.active || p.type != Projectile.type) continue;
				if (p.owner != Projectile.owner) continue;
				if ((int)p.ai[0] != nextIdx) continue;
				if (Math.Abs((int)p.ai[1]) != constId) continue;
				_nextSiblingPos = p.Center;
				_hasNextSibling = true;
				break;
			}
		}
		private Vector2 ComputeGroupCenter()
		{
			int constId = Math.Abs((int)Projectile.ai[1]);
			Vector2 sum = Vector2.Zero;
			int count = 0;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (!p.active || p.type != Projectile.type) continue;
				if (p.owner != Projectile.owner) continue;
				if (Math.Abs((int)p.ai[1]) != constId) continue;
				sum += p.Center;
				count++;
			}
			return (count > 0) ? sum / count : Projectile.Center;
		}
		private int CountSiblings()
		{
			int constId = Math.Abs((int)Projectile.ai[1]);
			int count = 0;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (!p.active || p.type != Projectile.type) continue;
				if (p.owner != Projectile.owner) continue;
				if (Math.Abs((int)p.ai[1]) != constId) continue;
				count++;
			}
			return count;
		}
		private Vector2 ResolveHuntTarget(int targetIdx)
		{
			if (targetIdx >= 0 && targetIdx < Main.maxNPCs)
			{
				NPC npc = Main.npc[targetIdx];
				if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0)
				{
					Projectile.localAI[0] = npc.Center.X;
					Projectile.localAI[1] = npc.Center.Y;
					return npc.Center;
				}
			}
			float lx = Projectile.localAI[0];
			float ly = Projectile.localAI[1];
			if (float.IsNaN(lx) || float.IsNaN(ly)) return Projectile.Center;
			return new Vector2(lx, ly);
		}
		private void BroadcastTarget(int targetWho)
		{
			if (Projectile.owner != Main.myPlayer) return;
			int constId = Math.Abs((int)Projectile.ai[1]);
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (!p.active || p.type != Projectile.type) continue;
				if (p.owner != Projectile.owner) continue;
				if (Math.Abs((int)p.ai[1]) != constId) continue;
				if ((int)p.ai[2] >= 0) continue;
				p.ai[2] = targetWho;
				p.localAI[0] = Main.npc[targetWho].Center.X;
				p.localAI[1] = Main.npc[targetWho].Center.Y;
				p.netUpdate = true;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			BroadcastTarget(target.whoAmI);
			SoundStyle impactSound = SoundID.Item122 with
			{
				Volume = 0.5f, Pitch = 0.4f + Main.rand.NextFloat(-0.12f, 0.12f), };
			SoundEngine.PlaySound(impactSound, target.Center);
			Lighting.AddLight(target.Center, 1.4f, 1.3f, 0.9f);
			for (int i = 0; i < 14; i++)
			{
				int d = Dust.NewDust(target.Center - new Vector2(8f), 16, 16, DustID.YellowStarDust, 0f, 0f, 100, default, 1.3f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity = Main.rand.NextVector2Circular(4f, 4f);
			}
			for (int i = 0; i < 6; i++)
			{
				int d = Dust.NewDust(target.Center - new Vector2(8f), 16, 16, DustID.AncientLight, 0f, 0f, 100, GetCosmicColor(i), 1.2f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity = Main.rand.NextVector2Circular(3f, 3f);
			}
			if (hit.Crit)
			{
				for (int i = 0; i < 10; i++)
				{
					int d = Dust.NewDust(target.Center - new Vector2(8f), 16, 16, DustID.GoldFlame, 0f, 0f, 100, default, 1.7f);
					Main.dust[d].noGravity = true;
					Main.dust[d].velocity = Main.rand.NextVector2Circular(5f, 5f);
				}
			}
		}
		private void ExplodeArea()
		{
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			int damage = (int)(Projectile.damage * 0.25f);
			float explosionRadius = Main.rand.NextFloat(280f, 530f);
			if (Projectile.owner == Main.myPlayer)
			{
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Distance(Projectile.Center) <= explosionRadius)
					{
						NPC.HitInfo hitInfo = new NPC.HitInfo
						{
							Damage = damage, Knockback = Projectile.knockBack, HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1
						};
						npc.StrikeNPC(hitInfo);
					}
				}
			}
			if (!Main.dedServ)
			{
				new Circleedge(Projectile.Center, Vector2.Zero, Color.Yellow, 40, explosionRadius * 0.01f * 0.5f, 1f, 0f).Spawn();
				new Circleedge(Projectile.Center, Vector2.Zero, Color.White, 30, explosionRadius * 0.01f * 0.75f, 0.7f, 0f).Spawn();
				for (int i = 0; i < 5; i++)
				{
					Vector2 sparkVel = Main.rand.NextVector2Circular(10f, 10f);
					new Spark(Projectile.Center, sparkVel, Color.Yellow, 30, Main.rand.NextFloat(0.8f, 1.4f), 1f, 0f).Spawn();
				}
				for (int i = 0; i < 15; i++)
				{
					int d = Dust.NewDust(Projectile.Center - new Vector2(explosionRadius / 2), (int)explosionRadius, (int)explosionRadius, DustID.YellowStarDust, 0f, 0f, 100, default, 1.8f);
					Main.dust[d].noGravity = true;
					Main.dust[d].velocity = Main.rand.NextVector2Circular(8f, 8f);
				}
			}
		}
		public override void OnKill(int timeLeft)
		{
			SoundStyle popSound = SoundID.Item125 with { Volume = 0.32f, Pitch = 0.25f };
			SoundEngine.PlaySound(popSound, Projectile.Center);
			for (int i = 0; i < 22; i++)
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, 0f, 0f, 100, default, 1.3f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= Main.rand.NextFloat(1.1f, 2.4f);
			}
			for (int i = 0; i < 12; i++)
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight, 0f, 0f, 100, GetCosmicColor(i), 1.2f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= Main.rand.NextFloat(1.0f, 2.1f);
			}
			for (int i = 0; i < 6; i++)
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 100, default, 1.6f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= Main.rand.NextFloat(0.7f, 1.6f);
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			int globalAge = Lifetime - Projectile.timeLeft;
			int starIdx = (int)Projectile.ai[0];
			int spawnDelay = (starIdx - 1) * SpawnDelayPerStar;
			int localAge = globalAge - spawnDelay;
			if (localAge < 0) return false;
			SpriteBatch sb = Main.spriteBatch;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = tex.Size() * 0.5f;
			Vector2 drawCenter = Projectile.Center - Main.screenPosition;
			bool hunting = (int)Projectile.ai[2] >= 0;
			DrawConnectionLine(sb, globalAge, hunting);
			if (!hunting && globalAge < AllStarsRevealedTick)
				DrawIntroStar(sb, tex, origin, drawCenter, localAge);
			else
				DrawStableStar(sb, tex, origin, drawCenter, globalAge, hunting);
			return false;
		}
		private void DrawConnectionLine(SpriteBatch sb, int globalAge, bool hunting)
		{
			if (!_hasNextSibling) return;
			float lineProgress;
			float lineOpacity;
			float baseThickness;
			if (hunting)
			{
				float fadeT = MathHelper.Clamp(_huntFadeTimer / 12f, 0f, 1f);
				lineOpacity = 1f - fadeT;
				lineProgress = 1f;
				baseThickness = MathHelper.Lerp(3.0f, 1.4f, fadeT);
				if (lineOpacity <= 0.02f) return;
			}
			else if (globalAge < AllStarsRevealedTick)
			{
				return;
			}
			else if (globalAge < DriftStartTick)
			{
				int lineAge = globalAge - AllStarsRevealedTick;
				lineProgress = MathHelper.Clamp(lineAge / (float)LineDrawDuration, 0f, 1f);
				lineOpacity = MathHelper.Clamp(lineProgress * 1.6f, 0f, 1f);
				baseThickness = MathHelper.Lerp(4.5f, 3.0f, lineProgress);
			}
			else
			{
				lineProgress = 1f;
				lineOpacity = 1f;
				baseThickness = 3.0f;
			}
			Vector2 from = Projectile.Center - Main.screenPosition;
			Vector2 fullTo = _nextSiblingPos - Main.screenPosition;
			Vector2 to = Vector2.Lerp(from, fullTo, lineProgress);
			Vector2 delta = to - from;
			float length = delta.Length();
			if (length < 4f) return;
			float rotation = delta.ToRotation();
			Texture2D px = TextureAssets.MagicPixel.Value;
			Rectangle src = new Rectangle(0, 0, 1, 1);
			Vector2 origin = new Vector2(0f, 0.5f);
			sb.End();
			sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Color warm = new Color(255, 232, 168);
			Color cool = new Color(150, 180, 255);
			Color halo = new Color(190, 205, 255);
			sb.Draw(px, from, src, halo * (0.18f * lineOpacity), rotation, origin, new Vector2(length, baseThickness * 3.4f), SpriteEffects.None, 0f);
			sb.Draw(px, from, src, cool * (0.28f * lineOpacity), rotation, origin, new Vector2(length, baseThickness * 2.0f), SpriteEffects.None, 0f);
			sb.Draw(px, from, src, warm * (0.32f * lineOpacity), rotation, origin, new Vector2(length, baseThickness * 1.10f), SpriteEffects.None, 0f);
			sb.Draw(px, from, src, Color.White * (0.35f * lineOpacity), rotation, origin, new Vector2(length, MathHelper.Max(1.0f, baseThickness * 0.45f)), SpriteEffects.None, 0f);
			DrawSpeedStreaks(sb, px, src, from, delta, length, rotation, baseThickness, lineOpacity, globalAge);
			if (GameShaders.Misc.TryGetValue(FlowShaderKey, out MiscShaderData flowShader) && flowShader != null)
			{
				flowShader.UseColor(new Vector3(1.0f, 0.93f, 0.60f));
				flowShader.UseSecondaryColor(new Vector3(0.55f, 0.70f, 1.00f));
				flowShader.UseOpacity(lineOpacity);
				Effect fx = flowShader.Shader;
				if (fx != null)
				{
					fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 60f);
					fx.Parameters["uSegmentLength"]?.SetValue(length);
					fx.Parameters["uThickness"]?.SetValue(baseThickness);
					fx.Parameters["uProgress"]?.SetValue(lineProgress);
				}
				flowShader.Apply(null);
				sb.Draw(px, from, src, Color.White * (lineOpacity * 0.9f), rotation, origin, new Vector2(length, baseThickness * 1.6f), SpriteEffects.None, 0f);
			}
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}
		private static void DrawSpeedStreaks(SpriteBatch sb, Texture2D px, Rectangle src, Vector2 from, Vector2 delta, float length, float rotation, float thickness, float opacity, int globalAge)
		{
			if (length < 6f) return;
			Vector2 dir = delta / length;
			float tFloat = globalAge + Main.GlobalTimeWrappedHourly * 60f - (int)(Main.GlobalTimeWrappedHourly * 60f);
			DrawStreakLayer(sb, px, src, from, dir, length, rotation, thickness * 0.95f, opacity * 1.10f, tFloat, streakLen: 18f, gap: 9f, speed: 5.8f, phase: 0f);
			DrawStreakLayer(sb, px, src, from, dir, length, rotation, thickness * 0.55f, opacity * 1.20f, tFloat, streakLen: 10f, gap: 13f, speed: 8.4f, phase: 6.5f);
			DrawStreakLayer(sb, px, src, from, dir, length, rotation, thickness * 0.30f, opacity * 1.35f, tFloat, streakLen: 5f, gap: 22f, speed: 11.0f, phase: 13f);
		}
		private static void DrawStreakLayer(SpriteBatch sb, Texture2D px, Rectangle src, Vector2 from, Vector2 dir, float length, float rotation, float thickness, float opacity, float timeT, float streakLen, float gap, float speed, float phase)
		{
			float pitch = streakLen + gap;
			float raw = timeT * speed + phase;
			float scroll = raw - (float)Math.Floor(raw / pitch) * pitch;
			Vector2 origin = new Vector2(0f, 0.5f);
			for (float startX = -pitch + scroll; startX < length; startX += pitch)
			{
				float drawStart = MathHelper.Max(0f, startX);
				float drawEnd = MathHelper.Min(length, startX + streakLen);
				float drawLen = drawEnd - drawStart;
				if (drawLen < 1.0f) continue;
				float startFade = MathHelper.Clamp(drawStart / 6f, 0f, 1f);
				float endFade = MathHelper.Clamp((length - drawStart) / 10f, 0f, 1f);
				float coverage = MathHelper.Clamp(drawLen / streakLen, 0f, 1f);
				float a = MathHelper.Clamp(opacity * startFade * endFade * coverage, 0f, 1f);
				if (a < 0.015f) continue;
				Vector2 pos = from + dir * drawStart;
				sb.Draw(px, pos, src, Color.White * a, rotation, origin, new Vector2(drawLen, thickness), SpriteEffects.None, 0f);
			}
		}
		private void DrawIntroStar(SpriteBatch sb, Texture2D tex, Vector2 origin, Vector2 drawCenter, int age)
		{
			float revealT = MathHelper.Clamp(age / (float)RevealDuration, 0f, 1f);
			float scale = EaseOutBack(revealT);
			float alpha = revealT;
			float pulse = 0.65f + 0.35f * (float)Math.Sin(age * 0.32f);
			DrawHaloLayers(sb, tex, origin, drawCenter, scale, alpha * pulse, layers: 2, spread: 0.45f, basePulse: 1f);
			Color core = Color.Lerp(new Color(255, 240, 200), Color.White, revealT) * alpha;
			sb.Draw(tex, drawCenter, null, core, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
		}
		private void DrawStableStar(SpriteBatch sb, Texture2D tex, Vector2 origin, Vector2 drawCenter, int globalAge, bool hunting)
		{
			float pulse = 0.85f + 0.15f * (float)Math.Sin(globalAge * 0.40f);
			if (hunting && Projectile.oldPos != null)
			{
				for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
				{
					Vector2 op = Projectile.oldPos[i];
					if (op == Vector2.Zero) continue;
					float t = i / (float)Projectile.oldPos.Length;
					float fade = 1f - t;
					float trailScale = MathHelper.Lerp(1.05f, 0.4f, t);
					Color trailColor = Color.Lerp(Color.LightCyan, Color.MediumPurple, t) * fade * 0.40f;
					Vector2 trailPos = op + Projectile.Size * 0.5f - Main.screenPosition;
					sb.Draw(tex, trailPos, null, trailColor, Projectile.rotation, origin, trailScale, SpriteEffects.None, 0f);
				}
			}
			DrawHaloLayers(sb, tex, origin, drawCenter, scaleBase: 1f, alphaBase: 0.85f, layers: 2, spread: 0.40f, basePulse: pulse);
			sb.Draw(tex, drawCenter, null, Color.White, Projectile.rotation, origin, 1.0f, SpriteEffects.None, 0f);
		}
		private void DrawHaloLayers(SpriteBatch sb, Texture2D tex, Vector2 origin, Vector2 drawCenter, float scaleBase, float alphaBase, int layers, float spread, float basePulse)
		{
			for (int g = layers; g >= 0; g--)
			{
				float gScale = scaleBase * (1.0f + g * spread);
				float gAlpha = alphaBase * (0.32f - g * 0.07f) * basePulse;
				if (gAlpha <= 0f) continue;
				float denom = layers > 0 ? layers : 1f;
				Color gColor = Color.Lerp(Color.LightCyan, Color.MediumPurple, g / denom) * gAlpha;
				sb.Draw(tex, drawCenter, null, gColor, Projectile.rotation, origin, gScale, SpriteEffects.None, 0f);
			}
		}
		private static float EaseOutBack(float t)
		{
			const float c1 = 1.70158f;
			const float c3 = c1 + 1f;
			float u = t - 1f;
			return 1f + c3 * u * u * u + c1 * u * u;
		}
		private static Color GetCosmicColor(int seed)
		{
			switch (seed % 3)
			{
				case 0: return new Color(180, 220, 255);
				case 1: return new Color(200, 180, 255);
				default: return new Color(255, 240, 200);
			}
		}
	}
}
