using jarkos.buffs.buff;
using jarkos.projectiles.bullet;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.melee
{
	public class ArmorPlate : ModItem
    {
		public override void SetDefaults()
		{
			Item.width = 56;
			Item.height = 50;
			Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ArmorPlatePlayer>().ArmorPlateEquiped = true;
		}
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.IronBar), 12)
			.AddRecipeGroup(nameof(ItemID.Vertebrae), 8)
			.AddIngredient(ItemID.Silk, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class ArmorPlatePlayer : ModPlayer
	{
		public bool ArmorPlateEquiped = false;
		public int ArmorPlateCooldown = 0;
        private const int CooldownTime = 2 * 60;
        public override void ResetEffects()
        {
            ArmorPlateEquiped = false;
        }
		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (ArmorPlateEquiped && ArmorPlateCooldown <= 0 && hurtInfo.Damage > 0)
            {
                TriggerExplosion(npc);
                ArmorPlateCooldown = CooldownTime;
            }
        }
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (ArmorPlateEquiped && ArmorPlateCooldown <= 0 && hurtInfo.Damage > 0)
            {
                TriggerExplosion(null);
                ArmorPlateCooldown = CooldownTime;
            }
        }
        private void TriggerExplosion(NPC triggeringNPC)
        {
            int baseDamage = 20;
            int finalDamage = baseDamage;
            Projectile.NewProjectile(Player.GetSource_Accessory(new Item(ModContent.ItemType<ArmorPlate>())), Player.Center, Vector2.Zero, ModContent.ProjectileType<KnockbackExplosiveProjectile>(), finalDamage, 20f, Player.whoAmI);
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.InfernoFork);
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                dust.scale = 1.5f;
                dust.noGravity = true;
            }
        }
        public override void PostUpdate()
        {
            if (ArmorPlateCooldown > 0)
            {
                ArmorPlateCooldown--;
                if (ArmorPlateCooldown > 0)
                {
                    Player.AddBuff(ModContent.BuffType<ArmorPlateCooldown>(), 2);
                }
            }
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Jarkos.JarkosPacketType.ArmorPlateSync);
            packet.Write((byte)Player.whoAmI);
            packet.Write(ArmorPlateCooldown);
            packet.Send(toWho, fromWho);
        }
	}
}
