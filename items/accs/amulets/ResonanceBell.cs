using System.Collections.Generic;
using System.Linq;
using jarkos.buffs.debuff;
using jarkos.projectiles.bullet;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.items.accs.amulets
{
	public class ResonanceBell : ModItem
    {
		public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ResonanceBellPlayer>().hasResonanceBell = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			var tooltipLine = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");

			if (tooltipLine != null)
			{
                var bindkey = ResonanceKeybindSystem.ResonanceBellKey.GetAssignedKeys();
                string firstbindkey = bindkey.Count > 0 ? bindkey[0] : "None";

				tooltipLine.Text = Language.GetTextValue("Mods.jarkos.Items.ResonanceBell.Description", firstbindkey);;
			}
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SoulofSight, 10);
            recipe.AddIngredient(ItemID.Bell, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
	}
	public class ResonanceBellPlayer : ModPlayer
    {
        public bool hasResonanceBell = false;
        public int bellCooldown = 0;
        public const int BellCooldownMax = 900;
        public bool justActivated = false;
        public override void ResetEffects()
        {
            hasResonanceBell = false;
            justActivated = false;
        }
        public override void PostUpdate()
        {
            if (bellCooldown > 0)
                bellCooldown--;

            if (!hasResonanceBell)
                return;

            if (ResonanceKeybindSystem.ResonanceBellKey.JustPressed && bellCooldown <= 0)
            {
                ActivateBell();
                bellCooldown = BellCooldownMax;
                justActivated = true;
            }
        }
        private void ActivateBell()
        {
            if (Main.myPlayer != Player.whoAmI)
                return;

            Vector2 spawnPos = Player.Center + new Vector2(0, -70);
            Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[0]), spawnPos, Vector2.Zero, ModContent.ProjectileType<ResonanceBell_proj>(), 0, 0f, Player.whoAmI);
        }
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TriggerResonance(target, damageDone);
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<MiniShockwave>())
                return;
            if (proj.type == ModContent.ProjectileType<ResonanceShockwave>())
                return;

            TriggerResonance(target, damageDone);
        }
        private void TriggerResonance(NPC target, int damageDone)
        {
            if (!hasResonanceBell)
                return;
            if (!target.HasBuff(ModContent.BuffType<ResonanceDebuff>()))
                return;

            int resonanceDamage = (int)(damageDone * 0.35f);

            if (resonanceDamage < 1) resonanceDamage = 1;
			if (resonanceDamage > 100) resonanceDamage = 100;
            
            Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[0]), target.Center, Vector2.Zero, ModContent.ProjectileType<MiniShockwave>(), resonanceDamage, 2f, Player.whoAmI, ai0: target.whoAmI);
        }
    }
	public class ResonanceKeybindSystem : ModSystem
    {
		public static ModKeybind ResonanceBellKey { get; private set; }
		public override void Load()
		{
			ResonanceBellKey = KeybindLoader.RegisterKeybind(Mod, "Resonance Bell", "G");
		}
		public override void Unload()
		{
			ResonanceBellKey = null;
        }
    }
}
