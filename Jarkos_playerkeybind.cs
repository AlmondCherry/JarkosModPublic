using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using jarkos.systems;
using jarkos.items.accs.special;

namespace jarkos
{
	public class Jarkos_playerkeybind : ModPlayer
	{
		public int charger_health_timer_cooldown = 0;
		public float lotus_activity = 0f;
		public float lotus_cooldown = 0f;
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            Player player = Main.LocalPlayer;
            var modPl = player.GetModPlayer<Jarkos_player>();
			#region Charger crystal
			if (Keybindsystem.accessory_ability_bind.JustPressed)
			{
				player.GetModPlayer<ChargerHealthPlayer>().ActivateChargerHealth();
			}
			#endregion
			#region Lotos
			if (Keybindsystem.accessory_ability_bind.JustPressed && modPl.lotos_equiped && lotus_activity == 0 && lotus_cooldown == 0)
        	{
				modPl.lotos_active = true;
				CombatText.NewText(player.Hitbox, Colors.RarityCyan, "les", false, false);
			}
			else if(Keybindsystem.accessory_ability_bind.JustPressed && lotus_cooldown != 0)
            {
                CombatText.NewText(player.Hitbox, Colors.RarityCyan, "on cd", false, false);
            }
			#endregion
			#region Diffusal Set Bonus
			if (Keybindsystem.accessory_ability_bind.JustPressed && modPl.diffusalSet)
			{
				if (modPl.diffusalWheelCooldown == 0)
				{
					if (Main.myPlayer == player.whoAmI)
					{
						Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<projectiles.mecha.DiffusalWheelProj>(), 0, 0f, player.whoAmI);
					}
					modPl.diffusalWheelCooldown = 600;
				}
				else
				{
					CombatText.NewText(player.Hitbox, Colors.RarityRed, $"On cooldown ({modPl.diffusalWheelCooldown / 60f:F1}s)", false, false);
				}
			}
			#endregion
		}
    }
}
