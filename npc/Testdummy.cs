using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc
{
	public class Testdummy : ModNPC
	{
		private const float HitAnimDuration = 12f;

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
			NPC.lifeMax = int.MaxValue;
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

		public override void HitEffect(NPC.HitInfo hit)
		{
			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, hit.HitDirection * 2f, -1f);
				dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
			}
		}
	}
}
