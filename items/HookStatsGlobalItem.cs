using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items
{
	public class HookStatsGlobalItem : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (item == null || item.IsAir) return;

			if (item.shoot <= ProjectileID.None || item.shoot >= Main.projHook.Length || !Main.projHook[item.shoot])
				return;

			Projectile dummyProj = new Projectile();
			dummyProj.SetDefaults(item.shoot);
			Player player = Main.LocalPlayer ?? new Player();

			float pullSpeed = 11f;
			ProjectileLoader.GrapplePullSpeed(dummyProj, player, ref pullSpeed);

			float retreatSpeed = 12f;
			ProjectileLoader.GrappleRetreatSpeed(dummyProj, player, ref retreatSpeed);
		}
	}
}
