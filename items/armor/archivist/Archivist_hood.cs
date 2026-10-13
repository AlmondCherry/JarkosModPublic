using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.GameInput;
using Microsoft.Xna.Framework;
using jarkos.projectiles.bullet;
using Terraria.Audio;

namespace jarkos.items.armor.archivist
{
	[AutoloadEquip(EquipType.Head)]
	public class Archivist_hood : ModItem
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
            Item.value = Item.sellPrice(gold: 7);
            Item.rare = ItemRarityID.Cyan;
            Item.defense = 8;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Magic) += 8f;
            player.maxMinions += 1;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return 
            head.type == ModContent.ItemType<Archivist_hood>() 
            && body.type == ModContent.ItemType<Archivist_breastplace>() 
            && legs.type == ModContent.ItemType<Archivist_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = SetBonusText.Value;
            player.GetModPlayer<ArchivistPlayer>().hasArchivistSet = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpectreBar, 12)
                .AddIngredient(ItemID.SpellTome, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class ArchivistPlayer : ModPlayer
    {
        public bool hasArchivistSet = false;
        private int hemisphereSpawnTimer = 0;
        public override void ResetEffects()
        {
            hasArchivistSet = false;
        }
        public override void PostUpdate()
        {
            if (!hasArchivistSet) return;
            bool nearHemisphere = false;
            float buffRange = 200f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == ModContent.ProjectileType<ArchivistHemisphere>() && p.owner == Player.whoAmI)
                {
                    if (Player.Distance(p.Center) <= buffRange)
                    {
                        nearHemisphere = true;
                        break;
                    }
                }
            }
            if (nearHemisphere)
            {
                Player.statDefense += 5;
                Player.moveSpeed += 0.10f;
            }
            float speed = Player.velocity.Length();
            if (speed > 0.1f)
            {
                hemisphereSpawnTimer++;
                if (hemisphereSpawnTimer >= 30)
                {
                    hemisphereSpawnTimer = 0;
                    int count = 0;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<ArchivistHemisphere>() && Main.projectile[i].owner == Player.whoAmI)
                            count++;
                    }
                    if (count < 15)
                    {
                        Vector2 spawnPos = Player.Center + new Vector2(Player.direction * 20f, -10f);
                        Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<ArchivistHemisphere>(), 150, 0f, Player.whoAmI);
                        for (int d = 0; d < 3; d++)
                        {
                            Dust.NewDust(spawnPos, 0, 0, DustID.PurpleTorch, 0f, -1f, 100, default, 0.8f);
                        }
                    }
                }
            }
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (ArchivistSystem.GatherKey.JustPressed && hasArchivistSet)
            {
                ArchivistSystem.GatherTarget = Main.MouseWorld;
                ArchivistSystem.GatherTimer = 60;
                
                SoundEngine.PlaySound(SoundID.Item4, Player.Center);
            }
        }
    }
    public class ArchivistSystem : ModSystem
    {
        public static ModKeybind GatherKey { get; private set; }
        public static bool IsGathering { get; set; } = false;
        public static Vector2 GatherTarget { get; set; } = Vector2.Zero;
        public static int GatherTimer { get; set; } = 0;
        public override void Load()
        {
            GatherKey = KeybindLoader.RegisterKeybind(Mod, "Archive Gather", "G");
        }
        public override void Unload()
        {
            GatherKey = null;
        }
        public override void PostUpdateInput()
        {
            if (GatherTimer > 0)
            {
                GatherTimer--;
                IsGathering = true;
            }
            else
            {
                IsGathering = false;
            }
        }
    }
}
