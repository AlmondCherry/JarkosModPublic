using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.items.usetiles;
using jarkos.buffs.buff;

namespace jarkos.items.accs.melee
{
	public class VengefulEye : ModItem
    {
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 20;
			Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<VengefulEyePlayer>().hasVengefulEye = true;
		}
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.LifeCrystal, 1)
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8)
			.AddIngredient(ItemID.BandofRegeneration, 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class VengefulEyePlayer : ModPlayer
    {
        public bool hasVengefulEye;
        public bool aggressionActive;
        public int lastAttackerNPC = -1;
        public int lastAttackerPlayer = -1;
        public override void ResetEffects()
        {
            hasVengefulEye = false;
            aggressionActive = false;
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            if (!hasVengefulEye)
                return;
                
            if (info.DamageSource.TryGetCausingEntity(out Entity attacker))
            {
                if (attacker is NPC npc)
                {
                    lastAttackerNPC = npc.whoAmI;
                    lastAttackerPlayer = -1;
                }
                else if (attacker is Player otherPlayer)
                {
                    lastAttackerPlayer = otherPlayer.whoAmI;
                    lastAttackerNPC = -1;
                }
                Player.AddBuff(ModContent.BuffType<Rage_eye_buff>(), 360);
            }
        }
        public override void PostUpdate()
        {
            if (!aggressionActive)
                return;
            Vector2? targetPosition = GetTargetPosition();
            if (targetPosition.HasValue)
            {
                Vector2 direction = targetPosition.Value - Player.Center;
                float distance = direction.Length();
                if (distance > 40f)
                {
                    direction.Normalize();
                    float pullStrength = MathHelper.Clamp(distance / 200f, 0.3f, 0.8f);
                    Player.velocity += direction * pullStrength;
                }
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                    dust.noGravity = true;
                    dust.velocity = direction * 2f;
                }
            }
        }
        private Vector2? GetTargetPosition()
        {
            if (lastAttackerNPC >= 0 && lastAttackerNPC < Main.maxNPCs)
            {
                NPC npc = Main.npc[lastAttackerNPC];
                if (npc.active && !npc.friendly)
                    return npc.Center;
            }
            if (lastAttackerPlayer >= 0 && lastAttackerPlayer < Main.maxPlayers)
            {
                Player otherPlayer = Main.player[lastAttackerPlayer];
                if (otherPlayer.active && !otherPlayer.dead && otherPlayer.whoAmI != Player.whoAmI)
                    return otherPlayer.Center;
            }
            return null;
        }
        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (aggressionActive)
            {
                modifiers.FinalDamage *= 1.35f;
            }
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (aggressionActive && proj.owner == Player.whoAmI)
            {
                modifiers.FinalDamage *= 1.35f;
            }
        }
        public override void PostUpdateRunSpeeds()
        {
            if (aggressionActive)
            {
                Player.maxRunSpeed *= 1.3f;
                Player.runAcceleration *= 1.3f;
                Player.moveSpeed *= 1.2f;
            }
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            lastAttackerNPC = -1;
            lastAttackerPlayer = -1;
        }
    }
}
