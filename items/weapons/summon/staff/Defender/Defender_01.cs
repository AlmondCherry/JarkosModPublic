using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.Defender
{
	public class Defender01buff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			var modPlayer = player.GetModPlayer<Jarkos_player>();
			if (modPlayer.anticvarSet && player.ownedProjectileCounts[ModContent.ProjectileType<Defender_01>()] > 0) {
				player.buffTime[buffIndex] = 2;
			}
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	public class Defender_01 : ModProjectile
	{
		private int passThroughTimer = 0;
		private int loopDirection = 1;
		private const float MaxSpeed = 36f;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 4;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public sealed override void SetDefaults() {
			Projectile.width = 36;
			Projectile.height = 68;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!CheckActive(owner)) {
				return;
			}

			SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);

			if (foundTarget)
			{
				if (passThroughTimer > 0)
				{
					passThroughTimer--;
					
					Projectile.velocity = Projectile.velocity.RotatedBy(loopDirection * 0.10f);
					if (Projectile.velocity.Length() < MaxSpeed)
					{
						Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;
					}

					if (Main.rand.NextBool(2))
					{
						Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, -Projectile.velocity * 0.2f, 0, default, Main.rand.NextFloat(1.2f, 1.8f));
						d.noGravity = true;
					}
				}
				else
				{
					Vector2 toTarget = targetCenter - Projectile.Center;
					float dist = toTarget.Length();
					Vector2 desiredVel = toTarget.SafeNormalize(Vector2.Zero) * MaxSpeed;

					float inertia = 14f;
					Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desiredVel) / inertia;
					if (Projectile.velocity.Length() < MaxSpeed)
					{
						Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;
					}

					if (dist < 70f)
					{
						passThroughTimer = 18;
						loopDirection = Main.rand.NextBool() ? 1 : -1;
						SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.7f, Pitch = 0.4f }, Projectile.Center);
					}

					if (Main.rand.NextBool(2))
					{
						Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, -Projectile.velocity * 0.2f, 0, default, Main.rand.NextFloat(1.2f, 1.8f));
						d.noGravity = true;
					}
				}

				if (Projectile.velocity != Vector2.Zero)
				{
					Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				}
			}
			else
			{
				passThroughTimer = 0;
				Vector2 idlePosition = owner.Center + new Vector2(owner.direction * -45f, -60f);
				Vector2 toIdle = idlePosition - Projectile.Center;
				
				if (toIdle.Length() > 2000f && Main.myPlayer == owner.whoAmI)
				{
					Projectile.position = idlePosition;
					Projectile.velocity = Vector2.Zero;
					Projectile.netUpdate = true;
				}
				else
				{
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * 0.14f, 0.12f);
					if (Projectile.velocity.Length() > 1.5f)
					{
						Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
					}
					else
					{
						Projectile.rotation = 0f;
					}
				}
			}

			Visuals();
		}

		private bool CheckActive(Player owner) {
			if (owner.dead || !owner.active || !owner.GetModPlayer<Jarkos_player>().anticvarSet) {
				owner.ClearBuff(ModContent.BuffType<Defender01buff>());
				Projectile.Kill();
				return false;
			}

			if (owner.HasBuff(ModContent.BuffType<Defender01buff>())) {
				Projectile.timeLeft = 2;
			}
			else {
				Projectile.Kill();
				return false;
			}

			return true;
		}

		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
		{
			distanceFromTarget = 750f;
			targetCenter = Projectile.position;
			foundTarget = false;

			if (owner.HasMinionAttackTargetNPC) {
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, Projectile.Center);
				if (between < 1500f) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
					foundTarget = true;
				}
			}

			if (!foundTarget) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						bool closeThroughWall = between < 120f;
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

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.SourceDamage.Flat += Main.rand.NextFloat(-40f, 75f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[Projectile.owner];

			for (int i = 0; i < 18; i++)
			{
				Vector2 sparkVel = Main.rand.NextVector2Circular(9f, 9f);
				Color sparkColor = Main.rand.NextBool() ? Color.Gold : Color.Orange;
				new jarkos.particles.Spark(target.Center, sparkVel, sparkColor, 26, Main.rand.NextFloat(0.9f, 1.7f), 1f, 0f).Spawn();
			}

			for (int i = 0; i < 12; i++)
			{
				Dust d = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(7f, 7f), 0, default, Main.rand.NextFloat(1.5f, 2.5f));
				d.noGravity = true;
			}

			new jarkos.particles.Circleedge(target.Center, Vector2.Zero, Color.Orange, 16, 0.15f, 0.85f).Spawn();
			SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.9f, Pitch = 0.2f }, target.Center);

			if (Main.netMode != NetmodeID.Server)
			{
				Main.instance.CameraModifiers.Add(new PunchCameraModifier(target.Center, (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), 5f, 4f, 10, 800f, FullName));
			}
		}

		private void Visuals() {
			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}
			Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.7f, 0.2f) * 0.85f);
		}
	}
}
