using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace jarkos.items.accs.species
{
	public class MechanoidPendantPlayer : ModPlayer
	{
		public bool hasMechanoidPendant;
		public int jumpsUsed;

		public override void ResetEffects()
		{
			hasMechanoidPendant = false;
		}

		public override void PostUpdateEquips()
		{
			if (hasMechanoidPendant)
			{
				if (Player.velocity.Y == 0f || Player.sliding || Player.grappling[0] >= 0)
				{
					jumpsUsed = 0;
				}
				Player.GetJumpState<MechanoidJump>().Enable();
			}
		}
	}

	public class MechanoidPendant : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 50;
			Item.accessory = true;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<MechanoidPendantPlayer>().hasMechanoidPendant = true;
		}
	}

	public class MechanoidJump : ExtraJump
	{
		public override Position GetDefaultPosition() => new After(CloudInABottle);

		public override float GetDurationMultiplier(Player player) => 0.4f;

		public override void OnStarted(Player player, ref bool playSound)
		{
			playSound = true;
			var modPlayer = player.GetModPlayer<MechanoidPendantPlayer>();
			modPlayer.jumpsUsed++;

			if (modPlayer.jumpsUsed < 3)
			{
				player.GetJumpState<MechanoidJump>().Available = true;
			}

			float boost = 4.5f;
			player.velocity.X += player.direction * boost;
			
			player.velocity.X = MathHelper.Clamp(player.velocity.X, -12f, 12f);

			for (int i = 0; i < 8; i++)
			{
				Dust d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Electric, -player.velocity.X * 0.2f, 2f, 100, default, 0.8f);
				d.noGravity = true;
			}
		}
	}
}
