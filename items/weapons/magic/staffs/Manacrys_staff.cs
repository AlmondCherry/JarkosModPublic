using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using jarkos.projectiles.magic.staffs;
using ReLogic.Content;

namespace jarkos.items.weapons.magic.staffs
{
	public class Manacrys_staff : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.staff[Item.type] = true;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 6));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.DamageType = DamageClass.Magic;
            Item.width = 44;
            Item.height = 62;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.damage = 18;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
            Item.mana = 2;
			Item.UseSound = SoundID.Item9 with { Volume = 0.5f, Pitch = 0.2f };
            Item.shoot = ModContent.ProjectileType<Manacrys_proj>();
			Item.shootSpeed = 18f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 80, 0);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int count = Main.rand.Next(1, 3);
			for (int i = 0; i < count; i++)
			{
				Vector2 spreadVel = velocity.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.85f, 1.15f);
				float randomSize = Main.rand.NextFloat(0.5f, 1.4f);
				int projType = Main.rand.NextBool() ? ModContent.ProjectileType<Manacrys_proj>() : ModContent.ProjectileType<Manacrys_proj2>();
				Projectile.NewProjectile(source, position, spreadVel, projType, damage, knockback, player.whoAmI, randomSize);
			}
			return false;
        }
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
        	Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/glowmasks/manacrys_glowmask", AssetRequestMode.ImmediateLoad).Value;
			Rectangle frame = Main.itemAnimations[Item.type] != null ? Main.itemAnimations[Item.type].GetFrame(texture) : texture.Frame(1, 1);
			Vector2 origin = frame.Size() * 0.5f;
			Vector2 drawPos = new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - frame.Height * 0.5f + 2f);
 			spriteBatch.Draw(texture, drawPos, frame, Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
        }
	}
}
