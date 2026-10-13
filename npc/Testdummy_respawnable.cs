using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc
{
	public class Testdummy_respawnable : ModNPC
	{
		private const float HitAnimDuration = 12f;
		private ref float InvincibleTimer => ref NPC.ai[1];

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 16;
		}

		public override void SetDefaults()
		{
			NPC.width = 32;
			NPC.height = 48;
			NPC.scale = 1f;
			NPC.defense = 0;
			NPC.lifeMax = 100;
			NPC.value = 0f;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.noTileCollide = false;
			NPC.noGravity = false;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
		}

		public override void AI()
		{
			if (NPC.ai[0] > 0f)
			{
				NPC.ai[0]--;
			}

			if (InvincibleTimer > 0f)
			{
				InvincibleTimer--;
				NPC.dontTakeDamage = true;
				NPC.alpha = (int)(100 + 80 * MathF.Sin(Main.GlobalTimeWrappedHourly * 25f));
			}
			else
			{
				NPC.dontTakeDamage = false;
				NPC.alpha = 0;
			}
		}

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			TriggerHitAnimation(hit.HitDirection);
		}

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			TriggerHitAnimation(hit.HitDirection);
		}

		private void TriggerHitAnimation(int hitDir)
		{
			NPC.ai[0] = HitAnimDuration;
			if (hitDir != 0)
			{
				NPC.spriteDirection = -hitDir;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frame.Height = frameHeight;
			if (NPC.ai[0] > 0f)
			{
				float progress = 1f - (NPC.ai[0] / HitAnimDuration);
				int frameIndex = 1 + (int)(progress * 14.99f);
				frameIndex = Math.Clamp(frameIndex, 1, 15);
				NPC.frame.Y = frameIndex * frameHeight;
			}
			else
			{
				NPC.frame.Y = 0;
			}
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			modifiers.SetInstantKill();
		}

		public override bool CheckDead()
		{
			SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.8f }, NPC.Center);

			for (int i = 0; i < 25; i++)
			{
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f));
				dust.scale = 1.2f;
				dust.noGravity = true;
			}

			for (int i = 0; i < 12; i++)
			{
				Dust smoke = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
				smoke.scale = 1.3f;
			}

			NPC.life = NPC.lifeMax;
			InvincibleTimer = 60f;
			NPC.dontTakeDamage = true;

			return false;
		}
	}
}
