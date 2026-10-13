using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using jarkos.items.materials;
using Terraria.GameInput;
using jarkos.projectiles.bullet;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using jarkos.rare;

namespace jarkos.items.armor.anker
{
	[AutoloadEquip(EquipType.Head)]
	public class Anker_hood : ModItem
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
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<CosmicRarity>();
            Item.defense = 40;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Ranged) += 5f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return 
            head.type == ModContent.ItemType<Anker_hood>() 
            && body.type == ModContent.ItemType<Anker_breastplace>() 
            && legs.type == ModContent.ItemType<Anker_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = SetBonusText.Value;
            var modPlayer = player.GetModPlayer<AnchorArmorPlayer>();
            
            modPlayer.hasAnchorSet = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.VortexHelmet, 1)
                .AddIngredient(ModContent.ItemType<HeavenlyPowder>(), 2)
                .AddIngredient(ModContent.ItemType<CelestiaStar>(), 2)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class AnchorArmorPlayer : ModPlayer
    {
        public bool hasAnchorSet = false;
        public int auraCooldown = 0;
        public override void ResetEffects()
        {
            hasAnchorSet = false;
        }
        public override void UpdateEquips()
        {
            if (hasAnchorSet)
            {
                float speed = Player.velocity.Length();
                Player.GetDamage(DamageClass.Ranged) += speed * 0.05f;
            }
            if (auraCooldown > 0)
            {
                auraCooldown--;
            }
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (AnchorArmorSystem.AuraKey.JustPressed && hasAnchorSet && auraCooldown <= 0)
            {
                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<AnchorAuraProjectile>(), 0, 0f, Player.whoAmI);
                auraCooldown = 90 * 60;
                SoundEngine.PlaySound(SoundID.Item84, Player.Center);
            }
        }
    }
    public class AnchorArmorSystem : ModSystem
    {
        public static ModKeybind AuraKey { get; private set; }
        public override void Load()
        {
            AuraKey = KeybindLoader.RegisterKeybind(Mod, "Aura Anchor", "V");
        }
        public override void Unload()
        {
            AuraKey = null;
        }
    }
}
