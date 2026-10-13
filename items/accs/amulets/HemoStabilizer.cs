using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jarkos;
using jarkos.buffs.buff;
using jarkos.systems;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.usetiles;
using Terraria.Localization;

namespace jarkos.items.accs.amulets
{
	public class HemoStabilizer : ModItem 
    {
		public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightPurple;
			Item.value = Item.sellPrice(gold: 1);
            Item.defense = 0;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<HemostabilizerPlayer>().hemostabilizerEquipped = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			var tooltipLine = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
			if (tooltipLine != null)
			{
				var bindKey = HemostabilizerKeybindSystem.HemostabilizerKeybind.GetAssignedKeys();
				string firstbindkey = bindKey.Count > 0 ? bindKey[0] : "None";
                
				tooltipLine.Text = Language.GetTextValue("Mods.jarkos.Items.HemoStabilizer.Tooltip", firstbindkey);
			}
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 6)
                .AddIngredient(ItemID.TitanGlove, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
	public class HemostabilizerPlayer : ModPlayer
    {
        public bool hemostabilizerEquipped;
        public int hemostabilizerCooldown = 0;
        public const int CooldownTime = 60 * 60;
        private const int BuffDuration = 15 * 60;
        public override void ResetEffects()
        {
            hemostabilizerEquipped = false;
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (hemostabilizerEquipped && HemostabilizerKeybindSystem.HemostabilizerKeybind.JustPressed && hemostabilizerCooldown <= 0 && !Player.dead)
            {
                ActivateHemostabilizer();
            }
        }
        private void ActivateHemostabilizer()
        {
            int healthCost = (int)(Player.statLife * 0.25f);
            if (healthCost < 1) healthCost = 1;
            if (Player.statLife - healthCost <= 0)
                healthCost = Player.statLife - 1;
            if (healthCost > 0)
            {
                Player.AddBuff(ModContent.BuffType<HemoStabilizer_buff>(), BuffDuration);
                for (int i = 0; i < 30; i++)
                {
                    Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Blood);
                    dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                    dust.scale = 1.5f;
                    dust.noGravity = false;
                }
                SoundEngine.PlaySound(SoundID.NPCHit19, Player.Center);
                hemostabilizerCooldown = CooldownTime;
            }
            else
            {
                SoundEngine.PlaySound(SoundID.MenuClose, Player.Center);
            }
        }
        public override void PostUpdate()
        {
            if (hemostabilizerCooldown > 0)
            {
                hemostabilizerCooldown--;
            }
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)JarkosPacketType.HemostabilizerSync);
            packet.Write((byte)Player.whoAmI);
            packet.Write(hemostabilizerCooldown);
            packet.Send(toWho, fromWho);
        }
    }
    public class HemostabilizerKeybindSystem : ModSystem
    {
		public static ModKeybind HemostabilizerKeybind { get; private set; }
		public override void Load()
		{
			HemostabilizerKeybind = KeybindLoader.RegisterKeybind(Mod, "Hemostabilizer", "G");
		}
		public override void Unload()
		{
			HemostabilizerKeybind = null;
		}
    }
    public class HemostabilizerPacketHandler
    {
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            int playerId = reader.ReadByte();
            int cooldown = reader.ReadInt32();
            Player player = Main.player[playerId];
            if (player != null && player.TryGetModPlayer(out HemostabilizerPlayer modPlayer))
            {
                modPlayer.hemostabilizerCooldown = cooldown;
            }
        }
    }
}
