using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.GameInput;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.buffs.buff;

namespace jarkos.items.armor.hellabyssal
{
	[AutoloadEquip(EquipType.Head)]
	public class Hellabyssal_hood : ModItem
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
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Red;
            Item.defense = 12;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.10f;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.15f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ModContent.ItemType<Hellabyssal_hood>() && body.type == ModContent.ItemType<Hellabyssal_breastplace>() && legs.type == ModContent.ItemType<Hellabyssal_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = SetBonusText.Value;
            player.GetModPlayer<AbyssalPlayer>().hasAbyssalSet = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ObsidianHelm, 1)
                .AddIngredient(ItemID.HellstoneBar, 15)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class AbyssalPlayer : ModPlayer
    {
        public bool hasAbyssalSet = false;
        public int abyssalCooldown = 0;
        private const int MaxBuffDuration = 600;
        public override void ResetEffects()
        {
            hasAbyssalSet = false;
        }
        public override void PostUpdateEquips()
        {
            if (hasAbyssalSet)
            {
                Player.maxMinions += 1;
            }
            if (abyssalCooldown > 0)
            {
                abyssalCooldown--;
            }
            int buffType = ModContent.BuffType<AbyssalSurgeBuff>();
            int buffIndex = Player.FindBuffIndex(buffType);

            if (buffIndex != -1)
            {
                float progress = (float)Player.buffTime[buffIndex] / MaxBuffDuration;

                Player.GetDamage(DamageClass.Summon) += 1.5f * progress;
                Player.whipRangeMultiplier += 1.0f * progress;
                Player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 1.5f * progress;

                if (Main.rand.NextBool((int)(10 / progress + 1)))
                {
                    int dustAmount = (int)(3 * progress) + 1;
                    for (int i = 0; i < dustAmount; i++)
                    {
                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.Shadowflame, 0f, -2f, 150, Color.OrangeRed, 1.2f);
                    }
                }
            }
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (AbyssalSystem.SurgeKey.JustPressed && hasAbyssalSet && abyssalCooldown <= 0)
            {
                int buffType = ModContent.BuffType<AbyssalSurgeBuff>();
                if (Player.FindBuffIndex(buffType) == -1)
                {
                    Player.AddBuff(buffType, MaxBuffDuration);
                    abyssalCooldown = 40 * 60;
                    SoundEngine.PlaySound(SoundID.Item122, Player.Center);
                    
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDust(Player.Center, 0, 0, DustID.Shadowflame, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 150, Color.OrangeRed, 1.5f);
                    }
                }
            }
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            abyssalCooldown = 0;
        }
    }
    public class AbyssalSystem : ModSystem
    {
        public static ModKeybind SurgeKey { get; private set; }
        public override void Load()
        {
            SurgeKey = KeybindLoader.RegisterKeybind(Mod, "Abyssal Surge", "H");
        }
        public override void Unload()
        {
            SurgeKey = null;
        }
    }
}
