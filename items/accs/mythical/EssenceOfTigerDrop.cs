using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.Audio;

namespace jarkos.items.accs.mythical
{
	public class EssenceOfTigerDrop : ModItem
	{
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                 Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Main.UIScaleMatrix);
                GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(3528), Item, null);
                Terraria.Utils.DrawBorderString(Main.spriteBatch, line.Text, new Vector2(line.X, line.Y), Color.Green, 1);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                return false;
            }
            return true;
        }
		public override void SetDefaults()
		{
			Item.damage = 100;
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<Tigerdropdash>().tigerdropEuqipped = true;
			if(player.GetModPlayer<Tigerdropdash>().DashDelay > 0){
				player.endurance = 100;
			}
			NPC.HitInfo hitInfo = new();
            hitInfo.Damage = Item.damage;
		}
			public override bool CanUseItem(Player player)
		{
			if (NPC.downedSlimeKing)
			{
				Item.damage = 150;
			}
			if (NPC.downedBoss1)
			{
				Item.damage = 200;
			}
			if (NPC.downedBoss2)
			{
				Item.damage = 250;
			}
			if (NPC.downedQueenBee)
			{
				Item.damage = 300;
			}
			if (NPC.downedBoss3)
			{
				Item.damage = 350;
			}
			if (Main.hardMode)
			{
				Item.damage = 500;
			}
			if (NPC.downedMechBoss2)
			{
				Item.damage = 550;
			}
			if (NPC.downedMechBoss2)
			{
				Item.damage = 600;
			}
			if (NPC.downedMechBoss3)
			{
				Item.damage = 650;
			}
			if (NPC.downedPlantBoss)
			{
				Item.damage = 1000;
			}
			if (NPC.downedFishron)
			{
				Item.damage = 1500;
			}
			if (NPC.downedGolemBoss)
			{
				Item.damage = 3000;
			}
			if (NPC.downedAncientCultist)
			{
				Item.damage = 5000;
			}
			if (NPC.downedMoonlord)
			{
				Item.damage = 10000;
			}
			return true;
		}
			public class Tigerdropdash : ModPlayer
	{
		public const int DashDown = 0;
		public const int DashUp = 1;
		public const int DashRight = 2;
		public const int DashLeft = 3;
		public const int DashCooldown = 50;
		public const int DashDuration = 150;
		public const float DashVelocity = 20f;
		public int DashDir = -1;
		public bool tigerdropEuqipped;
		public int DashDelay = 0;
		public int DashTimer = 0;
		public override void ResetEffects()
		{
			tigerdropEuqipped = false;
			if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15)
			{
				DashDir = DashDown;
			}
			else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15)
			{
				DashDir = DashUp;
			}
			else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15)
			{
				DashDir = DashRight;
			}
			else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15)
			{
				DashDir = DashLeft;
			}
			else {
				DashDir = -1;
			}
		}
		public override void PreUpdateMovement()
		{
			if (CanUseDash() && DashDir != -1 && DashDelay == 0)
			{
				Vector2 newVelocity = Player.velocity;
				switch (DashDir)
				{
					case DashUp when Player.velocity.Y > -DashVelocity:
					case DashDown when Player.velocity.Y < DashVelocity:
					{
						float dashDirection = DashDir == DashDown ? 1 : -1.3f;
						newVelocity.Y = dashDirection * DashVelocity;
						break;
					}
					case DashLeft when Player.velocity.X > -DashVelocity:
					case DashRight when Player.velocity.X < DashVelocity:
					{
						float dashDirection = DashDir == DashRight ? 1 : -1;
						newVelocity.X = dashDirection * DashVelocity;
						break;
					}
					default:
						return;
				}
				DashDelay = DashCooldown;
				DashTimer = DashDuration;
				Player.velocity = newVelocity;
			}
			if (DashDelay > 0)
				DashDelay--;
			if (DashTimer > 0)
			{
				Player.eocDash = DashTimer;
				Player.armorEffectDrawShadowEOCShield = true;
				DashTimer--;
			}
		}
		private bool CanUseDash()
		{
			return tigerdropEuqipped
				&& Player.dashType == 0
				&& !Player.setSolar
				&& !Player.mount.Active;
		}
		    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            NPC.HitInfo hitInfo = new();
			if (NPC.downedSlimeKing)
			{
				hitInfo.Damage = 150;
			}
			if (NPC.downedBoss1)
			{
				hitInfo.Damage = 200;
			}
			if (NPC.downedBoss2)
			{
				hitInfo.Damage = 250;
			}
			if (NPC.downedQueenBee)
			{
				hitInfo.Damage = 300;
			}
			if (NPC.downedBoss3)
			{
				hitInfo.Damage = 350;
			}
			if (Main.hardMode)
			{
				hitInfo.Damage = 500;
			}
			if (NPC.downedMechBoss2)
			{
				hitInfo.Damage = 550;
			}
			if (NPC.downedMechBoss2)
			{
				hitInfo.Damage = 600;
			}
			if (NPC.downedMechBoss3)
			{
				hitInfo.Damage = 650;
			}
			if (NPC.downedPlantBoss)
			{
				hitInfo.Damage = 1000;
			}
			if (NPC.downedFishron)
			{
				hitInfo.Damage = 1500;
			}
			if (NPC.downedGolemBoss)
			{
				hitInfo.Damage = 3000;
			}
			if (NPC.downedAncientCultist)
			{
				hitInfo.Damage = 5000;
			}
			if (NPC.downedMoonlord)
			{
				hitInfo.Damage = 10000;
			}
            if (DashDelay > 0 && tigerdropEuqipped)
			{
				SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Daaamnsound"), Player.position);
                CombatText.NewText(npc.Hitbox, Colors.RarityRed, "Ten years in the joint made you a fucking pussy!", true);
				
                npc.StrikeNPC(hitInfo, false, true);
            }
        }
	}
	}
}
