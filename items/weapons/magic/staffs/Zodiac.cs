using System;
using System.Collections.Generic;
using jarkos.items.materials;
using jarkos.projectiles.magic.staffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.magic.staffs
{
	public class Zodiac : ModItem
	{
		public const int StarCount = 5;
		public const int MaxActiveConstellations = 5;
		public const float ConstellationRadius = 200f;
		public const float MinStarSpacing = 90f;
		public const int ManaCost = 40;
		public override void SetDefaults()
		{
			Item.damage = 600;
			Item.DamageType = DamageClass.Magic;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 8f;
			Item.UseSound = SoundID.Item9;
			Item.autoReuse = false;
			Item.mana = ManaCost;
			Item.shoot = ModContent.ProjectileType<ZodiacStar>();
			Item.shootSpeed = 8f;
			Item.crit = 14;

			Item.rare = ModContent.RarityType<jarkos.rare.CosmicRarity>();
            Item.value = Item.sellPrice(5, 0, 0, 0);
		}
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(0f, 0f);
		}
		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				return CountActiveConstellations(player) > 0;
			}
			return CountActiveConstellations(player) < MaxActiveConstellations;
		}
		private static int CountActiveConstellations(Player player)
		{
			int projType = ModContent.ProjectileType<ZodiacStar>();
			var seen = new HashSet<int>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (!p.active || p.type != projType || p.owner != player.whoAmI) continue;
				seen.Add(Math.Abs((int)p.ai[1]));
			}
			return seen.Count;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				int projTypee = ModContent.ProjectileType<ZodiacStar>();
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.active && p.type == projTypee && p.owner == player.whoAmI)
					{
						if (p.ai[2] > -2000f)
						{
							p.ai[2] = -2000f - (p.ai[0] * 12f);
							p.netUpdate = true;
						}
					}
				}
				return false;
			}
			Vector2 cursor = Main.MouseWorld;
			Vector2[] points = GenerateConstellationPoints(cursor);
			float[] mult = new float[StarCount];
			bool[] critFlag = new bool[StarCount];
			ComputeStarStats(points, mult, critFlag);
			int constellationId = Main.rand.Next(1, 0x3FFFFFF);
			int projType = ModContent.ProjectileType<ZodiacStar>();
			for (int i = 0; i < StarCount; i++)
			{
				int starDamage = (int)Math.Round(damage * mult[i]);
				int packedId = critFlag[i] ? -constellationId : constellationId;
				Projectile.NewProjectile(source, points[i], Vector2.Zero, projType, starDamage, knockback, player.whoAmI, ai0: i + 1, ai1: packedId, ai2: -1f);
			}
			return false;
		}
		private static Vector2[] GenerateConstellationPoints(Vector2 cursor)
		{
			Vector2[] pts = new Vector2[StarCount];
			for (int i = 0; i < StarCount; i++)
			{
				Vector2 p = Vector2.Zero;
				for (int attempt = 0; attempt < 18; attempt++)
				{
					float angle = Main.rand.NextFloat(MathHelper.TwoPi);
					float radius = Main.rand.NextFloat(ConstellationRadius * 0.45f, ConstellationRadius);
					p = cursor + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
					bool tooClose = false;
					for (int j = 0; j < i; j++)
					{
						if (Vector2.Distance(p, pts[j]) < MinStarSpacing)
						{
							tooClose = true;
							break;
						}
					}
					if (!tooClose) break;
				}
				pts[i] = p;
			}
			return pts;
		}
		private static void ComputeStarStats(Vector2[] points, float[] mult, bool[] critFlag)
		{
			for (int i = 0; i < points.Length; i++)
			{
				mult[i] = 1f;
				critFlag[i] = false;
			}
			for (int i = 1; i < points.Length - 1; i++)
			{
				if (IsPointBetween(points[i - 1], points[i], points[i + 1]))
				{
					mult[i - 1] += 0.5f;
					mult[i + 1] += 0.5f;
					critFlag[i] = true;
				}
			}
		}
		private static bool IsPointBetween(Vector2 a, Vector2 b, Vector2 c)
		{
			Vector2 ac = c - a;
			float lenSq = ac.LengthSquared();
			if (lenSq < 1f) return false;
			Vector2 ab = b - a;
			float t = Vector2.Dot(ab, ac) / lenSq;
			if (t < 0.15f || t > 0.85f) return false;
			Vector2 closest = a + ac * t;
			float perpDist = Vector2.Distance(closest, b);
			float tolerance = MathHelper.Clamp((float)Math.Sqrt(lenSq) * 0.25f, 40f, 140f);
			return perpDist < tolerance;
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.LunarBar, 20)
				.AddIngredient(ModContent.ItemType<CelestiaStar>(), 4)
				.AddIngredient(ItemID.ManaCrystal, 5)
				.AddIngredient(ItemID.FragmentNebula, 20)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
