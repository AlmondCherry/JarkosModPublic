using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;
using jarkos.buffs.buff;

namespace jarkos.items.accs.melee
{
	public class GlovesOfDysterian : ModItem
    {
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<NatureGodPlayer>().hasNatureGauntlets = true;
		}
	}
	public class NatureGodPlayer : ModPlayer
    {
        public bool hasNatureGauntlets;
        public float natureTrust;
        private const float MaxTrust = 100f;
        private const float MinTrust = -100f;
        private const float TrustGainPerTick = 0.02f;
        private const float TrustPassiveGain = 0.005f;
        private const float TrustLossOnHit = 15f;
        private const float TrustGainOnSummonHit = 0.1f;
        private const float BlessingThreshold = 50f;
        private const float CurseThreshold = -50f;
        public override void ResetEffects()
        {
            hasNatureGauntlets = false;
        }
        public override void PostUpdateEquips()
        {
            if (!hasNatureGauntlets)
                return;
                
            Item heldItem = Player.HeldItem;
            if (IsNatureApprovedWeapon(heldItem))
            {
                natureTrust += TrustGainPerTick;
            }
            else if (heldItem.damage > 0)
            {
                natureTrust -= TrustPassiveGain * 2f;
            }
            else
            {
                natureTrust += TrustPassiveGain;
            }
            natureTrust = MathHelper.Clamp(natureTrust, MinTrust, MaxTrust);
            ApplyTrustEffects();
        }
        private void ApplyTrustEffects()
        {
            if (natureTrust >= BlessingThreshold)
            {
                float blessingPower = (natureTrust - BlessingThreshold) / (MaxTrust - BlessingThreshold);
                Player.luck += 0.4f * blessingPower;

                Player.GetDamage(DamageClass.Summon) += 0.20f * blessingPower;
                Player.GetDamage(DamageClass.SummonMeleeSpeed) += 0.20f * blessingPower;

                if (natureTrust >= 90f)
                {
                    Player.maxMinions += 1;
                }
                Player.AddBuff(ModContent.BuffType<DisteryanBlessing>(), 2);
            }
            else if (natureTrust <= CurseThreshold)
            {
                float cursePower = (CurseThreshold - natureTrust) / (CurseThreshold - MinTrust);
                Player.luck -= 0.4f * cursePower;
                
                Player.GetDamage(DamageClass.Summon) -= 0.25f * cursePower;
                Player.GetDamage(DamageClass.SummonMeleeSpeed) -= 0.25f * cursePower;
                Player.AddBuff(ModContent.BuffType<DisteryanCurse>(), 2);
            }
        }
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasNatureGauntlets)
                return;
            if (!IsNatureApprovedWeapon(item))
            {
                natureTrust -= TrustLossOnHit;
                SpawnAngerDust();
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasNatureGauntlets)
                return;
            if (proj.minion || proj.sentry || IsWhipProjectile(proj))
            {
                natureTrust += TrustGainOnSummonHit;
                if (natureTrust >= 80f && Main.rand.NextBool(10))
                {
                    SpawnNatureDust(target.Center);
                }
            }
            else if (proj.owner == Player.whoAmI && !proj.minion && !proj.sentry)
            {
                Item sourceItem = GetSourceItem(proj);
                if (sourceItem != null && !IsNatureApprovedWeapon(sourceItem))
                {
                    natureTrust -= TrustLossOnHit * 0.5f;
                    SpawnAngerDust();
                }
            }
        }
        private Item GetSourceItem(Projectile proj)
        {
            for (int i = 0; i < 58; i++)
            {
                Item item = Player.inventory[i];
                if (item.shoot == proj.type)
                    return item;
            }
            return null;
        }
        private bool IsNatureApprovedWeapon(Item item)
        {
            if (item == null || item.IsAir)
                return true;
            if (item.damage <= 0)
                return true;
            if (item.DamageType == DamageClass.SummonMeleeSpeed)
                return true;
            if (item.DamageType == DamageClass.Summon)
                return true;
            if (item.DamageType.CountsAsClass(DamageClass.Summon) && !item.DamageType.CountsAsClass(DamageClass.Melee) && !item.DamageType.CountsAsClass(DamageClass.Ranged) && !item.DamageType.CountsAsClass(DamageClass.Magic))
                return true;
            return false;
        }
        private bool IsWhipProjectile(Projectile proj)
        {
            return ProjectileID.Sets.IsAWhip[proj.type];
        }
        private void SpawnAngerDust()
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Torch, 0f, -2f, 100, default, 1.5f);
                dust.noGravity = true;
            }
        }
        private void SpawnNatureDust(Vector2 position)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(position, 10, 10, DustID.GrassBlades, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.2f);
                dust.noGravity = true;
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag["natureTrust"] = natureTrust;
        }
        public override void LoadData(TagCompound tag)
        {
            natureTrust = tag.GetFloat("natureTrust");
        }
    }
}
