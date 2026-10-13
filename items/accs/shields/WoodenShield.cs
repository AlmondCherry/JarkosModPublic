using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace jarkos.items.accs.shields
{
	public class WoodenShield : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Generic) += 0.04f;
            player.GetAttackSpeed(DamageClass.Generic) += 0.02f;
			player.statLifeMax2 += 15;
			player.GetModPlayer<WoodenDashPlayer>().DashAccessoryEquipped = true;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 16)
			.AddRecipeGroup(nameof(ItemID.IronBar), 7)
			.AddIngredient(ItemID.Chain, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	public class WoodenDashPlayer : ModPlayer
	{
		public const int DashDown = 0;
		public const int DashUp = 1;
		public const int DashRight = 2;
		public const int DashLeft = 3;
		public const int DashCooldown = 50;
		public const int DashDuration = 70;
		public const float DashVelocity = 6f;
		public int DashDir = -1;
		public bool DashAccessoryEquipped;
		public int DashDelay = 0;
		public int DashTimer = 0;
		public override void ResetEffects()
		{
			DashAccessoryEquipped = false;
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
			return DashAccessoryEquipped
				&& Player.dashType == 0
				&& !Player.setSolar
				&& !Player.mount.Active;
		}
	}
	}
}
