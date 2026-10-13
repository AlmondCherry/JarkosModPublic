using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System;
using Jarkos.projectiles.summon.staff;

namespace jarkos.items.armor.small_wooden
{
	[AutoloadEquip(EquipType.Head)]
	public class Weak_wooden_helmet : ModItem
	{
	    public static LocalizedText SetBonusText { get; private set; }
	    public override void SetStaticDefaults()
        {
		    SetBonusText = this.GetLocalization(nameof(SetBonusText));
	    }
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.buyPrice(0, 0, 0, 5);
            Item.rare = ItemRarityID.White;
            Item.defense = 3;
        }
        public override void UpdateEquip(Player player)
        {
            player.statDefense += 0;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<Weak_wooden_chest>() && legs.type == ModContent.ItemType<Weak_wooden_leggins>();
        }
        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.GetModPlayer<WoodArmorPlayer>();
            modPlayer.hasWoodSet = true;
            int entlingDamage = modPlayer.GetEntlingDamage(player);
            player.setBonus = SetBonusText.Value + "\n" + LangSystemWWH.Desc1 + entlingDamage + "\n" + LangSystemWWH.Desc2 + WoodArmorPlayer.EntlingBaseDamage + "\n" + LangSystemWWH.Desc3 + (entlingDamage - WoodArmorPlayer.EntlingBaseDamage);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 30);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
    public class WoodArmorPlayer : ModPlayer
    {
        public bool hasWoodSet = false;
        public const int EntlingBaseDamage = 8;
        private int regenTimer = 0;
        private const int RegenInterval = 60;
        private Vector2 lastPosition;
        private bool wasStationary = false;
        public int entlingProjectileIndex = -1;
        public override void ResetEffects()
        {
            hasWoodSet = false;
        }
        public int GetEntlingDamage(Player player)
        {
            float summonDamageMultiplier = player.GetDamage(DamageClass.Summon).Multiplicative;
            float summonDamageFlatBonus = player.GetDamage(DamageClass.Summon).Flat;
            float summonDamageAdditive = player.GetDamage(DamageClass.Summon).Additive;
            float totalDamage = (EntlingBaseDamage + summonDamageFlatBonus) * summonDamageAdditive * summonDamageMultiplier;
            return Math.Max(1, (int)totalDamage);
        }
        public override void PostUpdateEquips()
        {
            if (!hasWoodSet)
            {
                KillEntling();
                return;
            }
            SpawnEntlingIfNeeded();

            bool isStationary = (Player.position - lastPosition).Length() < 0.5f;
            if (isStationary)
            {
                regenTimer++;
                if (regenTimer >= RegenInterval)
                {
                    regenTimer = 0;
                    if (Player.statLife < Player.statLifeMax2)
                    {
                        Player.statLife = Math.Min(Player.statLife + 2, Player.statLifeMax2);
                        Player.HealEffect(2, false);
                    }
                }
                Player.lifeRegen += 3;
            }
            else
            {
                regenTimer = 0;
            }

            wasStationary = isStationary;
            lastPosition = Player.position;
        }
        private void SpawnEntlingIfNeeded()
        {
            if (entlingProjectileIndex >= 0 && entlingProjectileIndex < Main.maxProjectiles)
            {
                Projectile proj = Main.projectile[entlingProjectileIndex];
                
                if (proj.active && proj.type == ModContent.ProjectileType<EntlingProjectile>()
                    && proj.owner == Player.whoAmI)
                {
                    return;
                }
            }
            if (Main.myPlayer == Player.whoAmI)
            {
                int damage = GetEntlingDamage(Player);
                entlingProjectileIndex = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<EntlingProjectile>(), damage, 2f, Player.whoAmI);
            }
        }
        private void KillEntling()
        {
            if (entlingProjectileIndex >= 0 && entlingProjectileIndex < Main.maxProjectiles)
            {
                Projectile proj = Main.projectile[entlingProjectileIndex];
                if (proj.active && proj.type == ModContent.ProjectileType<EntlingProjectile>())
                {
                    proj.Kill();
                }
            }
            entlingProjectileIndex = -1;
        }
        public override void OnEnterWorld()
        {
            lastPosition = Player.position;
            entlingProjectileIndex = -1;
        }
    }
    public class LangSystemWWH : ModSystem
    {
        public static LocalizedText Desc1 { get; private set; }
        public static LocalizedText Desc2 { get; private set; }
        public static LocalizedText Desc3 { get; private set; }
        public override void Load()
		{
			string category_ = "MiscWWH";
			Desc1 ??= Mod.GetLocalization($"{category_}.Desc1");
            Desc2 ??= Mod.GetLocalization($"{category_}.Desc2");
            Desc3 ??= Mod.GetLocalization($"{category_}.Desc3");
        }
    }
}
