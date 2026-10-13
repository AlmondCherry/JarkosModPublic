using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.npc;
using jarkos.UI;
using System;

namespace jarkos.items.usable
{
	public class Spawn_dummy : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 44;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.useAnimation = 10;
			Item.useTime = 10;
			Item.useTurn = true;
			Item.UseSound = null;
			Item.maxStack = 1;
			Item.consumable = false;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				return true;
			}
			if (Main.LocalPlayer.mouseInterface)
			{
				return false;
			}
			return true;
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				if (player.whoAmI == Main.myPlayer)
				{
					DummyUISystem.Instance.ToggleUI();
				}
				return true;
			}

			if (player.whoAmI == Main.myPlayer)
			{
				if (Main.LocalPlayer.mouseInterface)
				{
					return true;
				}

				if (DummyUISystem.AnyBossActive())
				{
					return true;
				}

				int npcType = DummyUISystem.IsArmored ? ModContent.NPCType<Testdummy_defensed>() : ModContent.NPCType<Testdummy>();
				int npcIndex = NPC.NewNPC(player.GetSource_ItemUse(Item), (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, npcType);

				if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
				{
					NPC npc = Main.npc[npcIndex];

					if (DummyUISystem.IsArmored)
					{
						npc.defense = DummyUISystem.ArmoredDefense;
					}

					if (npc.GetGlobalNPC<DummyGlobalNPC>() is DummyGlobalNPC dummyGlobal)
					{
						dummyGlobal.DamageReduction = DummyUISystem.DummyDR;
						dummyGlobal.DebuffVulnerable = DummyUISystem.DummyDebuffVulnerable;
					}

					float scale = DummyUISystem.DummyScale;
					npc.scale = scale;
					npc.width = Math.Max(4, (int)(32 * scale));
					npc.height = Math.Max(4, (int)(48 * scale));
					npc.position = Main.MouseWorld - new Vector2(npc.width * 0.5f, npc.height * 0.5f);

					long maxHp = (long)(int.MaxValue * (DummyUISystem.DummyHealthPercent / 100.0));
					maxHp = Math.Clamp(maxHp, 1L, int.MaxValue);
					npc.lifeMax = (int)maxHp;
					npc.life = npc.lifeMax;

					npc.damage = DummyUISystem.DummyDamage;
					npc.knockBackResist = 1f - MathHelper.Clamp(DummyUISystem.DummyKnockbackResist / 100f, 0f, 1f);
					npc.noGravity = DummyUISystem.DummyNoGravity;

					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
					}
				}
			}

			return true;
		}
	}
}

