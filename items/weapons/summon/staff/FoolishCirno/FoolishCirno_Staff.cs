using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.FoolishCirno
{
	public class FoolishCirno_Staff : ModItem
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/FoolishCirno/FoolishSpectre";

		public override void SetStaticDefaults()
		{
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 104;
			Item.knockBack = 2.0f;
			Item.mana = 10;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = null;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<LilCirno_buff>();
			Item.shoot = ModContent.ProjectileType<LilCirno_minion>();
			Item.shootSpeed = 10f;

			Item.rare = ItemRarityID.Purple;
			Item.value = Item.sellPrice(gold: 3);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 3600);

			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/baka_cirno") with { Pitch = 0.0f }, player.position);

			Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			proj.originalDamage = Item.damage;

			return false;
		}
	}
}
