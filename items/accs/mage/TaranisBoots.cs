using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.items.materials;

namespace jarkos.items.accs.mage
{
	public class TaranisBoots : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<Jarkos_player>();
			var Taranis = player.GetModPlayer<TaranisRingDash>();

			modPlayer.taranis_boots = true;
			Taranis.DashAccessoryEquipped = true;

			player.accRunSpeed = 2.25f;
      		player.moveSpeed += 2.25f;
			player.maxRunSpeed += 2.25f;
			player.iceSkate = true;
      		player.waterWalk = true;
      		player.fireWalk = true;
      		player.lavaMax += 400;
			player.noFallDmg = true;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.TerrasparkBoots, 1)
			.AddIngredient(ModContent.ItemType<LunarCrystal>(), 5)
			.AddIngredient(ModContent.ItemType<ElementalFluid>(), 5)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
		public class TaranisRingDash : ModPlayer
		{
		public const int DashDown = 0;
		public const int DashUp = 1;
		public const int DashRight = 2;
		public const int DashLeft = 3;
		public const int DashCooldown = 60;
		public const int DashDuration = 70;
		public const float DashVelocity = 25f;
		public int DashDir = -1;
		public bool DashAccessoryEquipped;
		public int DashDelay = 0;
		public int DashTimer = 0;
		public override void ResetEffects()
		{
			DashAccessoryEquipped = false;
			DashDir = -1;
			if (Player.whoAmI == Main.myPlayer)
			{
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
			}
		}
		public override void PreUpdateMovement()
		{
			if (Player.whoAmI == Main.myPlayer && CanUseDash() && DashDir != -1 && DashDelay == 0)
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
