using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using jarkos.buffs.buff;

namespace jarkos.items.accs.summon
{
	public class NatureSingularity : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 60;
			Item.height = 70;
			Item.accessory = true;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.aggro -= 25;
			player.endurance += 0.01f * player.numMinions;
			player.GetDamage(DamageClass.Summon) += 0.05f;
			player.maxMinions += 2;
			player.GetModPlayer<NatureSingularityPlayer>().hasNatureSingularity = true;
		}
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			Player player = Main.LocalPlayer;
			float endurance_by_acc = 1f * player.numMinions;

            tooltips.Add(new TooltipLine(Mod, "singularmasktooltip", (string)MinionUsage_Tooltip.minionusage_text + " " + endurance_by_acc + "%"));
        }
	}

	public class NatureSingularityPlayer : ModPlayer
	{
		public bool hasNatureSingularity;

		public override void ResetEffects()
		{
			hasNatureSingularity = false;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hasNatureSingularity && target.active && !target.friendly)
			{
				target.GetGlobalNPC<NatureSingularityGlobalNPC>().AddHitStack();
			}
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hasNatureSingularity && target.active && !target.friendly)
			{
				target.GetGlobalNPC<NatureSingularityGlobalNPC>().AddHitStack();
			}
		}
	}

	public class NatureSingularityGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public int Stacks;
		public int NoHitTimer;
		public int DecayTimer;

		public void AddHitStack()
		{
			if (Stacks < 20)
			{
				Stacks++;
			}
			NoHitTimer = 300;
			DecayTimer = 0;
		}

		public override void PostAI(NPC npc)
		{
			if (Stacks <= 0) return;

			Player localPlayer = Main.LocalPlayer;
			if (localPlayer.active && !localPlayer.dead && localPlayer.GetModPlayer<NatureSingularityPlayer>().hasNatureSingularity)
			{
				localPlayer.AddBuff(ModContent.BuffType<Nature_Gods_Bless>(), 10);
			}

			if (NoHitTimer > 0)
			{
				NoHitTimer--;
			}
			else
			{
				DecayTimer++;
				if (DecayTimer >= 3)
				{
					DecayTimer = 0;
					Stacks--;
				}
			}
		}

		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (Stacks > 0 && modifiers.DamageType.CountsAsClass(DamageClass.Summon))
			{
				modifiers.FinalDamage *= 1f + (Stacks * 0.01f);
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			if (Stacks > 0 && Main.rand.NextBool(4))
			{
				Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.JungleSpore, 0f, 0f, 100, default, 0.9f + Stacks * 0.02f);
				d.noGravity = true;
				d.velocity *= 0.3f;
			}
		}
	}

	public class MinionUsage_Tooltip : ModSystem
	{
		public static LocalizedText minionusage_text { get; private set; }
		public override void Load()
		{
			string category = "minionusagetextTooltip";
			minionusage_text ??= Mod.GetLocalization($"{category}.minionusage_text");
		}
	}
}
