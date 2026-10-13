using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using jarkos.projectiles.bullet;
using Terraria.Audio;

namespace jarkos.items.accs.ranged
{
	public class RicochetForce : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 50);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RicochetPlayer>().hasRicochet = true;
        }
    }
    public class RicochetPlayer : ModPlayer
    {
        public bool hasRicochet;
        private int chargeTimer = 0;
        private const int ChargeInterval = 120;
        private HashSet<int> chargedProjectiles = new HashSet<int>();
        public override void ResetEffects()
        {
            hasRicochet = false;
        }
        public override void PostUpdate()
        {
            if (!hasRicochet) return;

            chargeTimer++;

            if (chargeTimer >= ChargeInterval)
            {
                chargeTimer = 0;
                ChargeNextRangedProjectile();
            }
            CleanupChargedProjectiles();
        }
        private void ChargeNextRangedProjectile()
        {
            Projectile bestProj = null;
            float bestTime = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];

                if (!proj.active || proj.owner != Player.whoAmI) continue;
                if (!proj.CountsAsClass(DamageClass.Ranged)) continue;
                if (chargedProjectiles.Contains(proj.identity)) continue;
                if (proj.timeLeft < 60) continue;
                if (proj.timeLeft > bestTime)
                {
                    bestTime = proj.timeLeft;
                    bestProj = proj;
                }
            }
            if (bestProj != null)
            {
                chargedProjectiles.Add(bestProj.identity);
                for (int i = 0; i < 8; i++)
                {
                    Dust dust = Dust.NewDustPerfect(bestProj.Center, DustID.YellowTorch);
                    dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                    dust.scale = 1.2f;
                    dust.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item82 with { Volume = 0.5f }, bestProj.Center);
            }
        }
        private void CleanupChargedProjectiles()
        {
            HashSet<int> toRemove = new HashSet<int>();
            foreach (int id in chargedProjectiles)
            {
                bool found = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].identity == id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) toRemove.Add(id);
            }
            foreach (int id in toRemove)
            {
                chargedProjectiles.Remove(id);
            }
        }
        public bool IsProjectileCharged(Projectile proj)
        {
            return chargedProjectiles.Contains(proj.identity);
        }
        public void ConsumeCharge(Projectile proj)
        {
            chargedProjectiles.Remove(proj.identity);
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasRicochet) return;
            if (!proj.CountsAsClass(DamageClass.Ranged)) return;
            if (!IsProjectileCharged(proj)) return;
            if (target.life <= 0) return;

            ConsumeCharge(proj);
            
            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectile(proj.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<RicochetSoul>(), (int)(damageDone * 0.45f), 0f, Player.whoAmI, target.whoAmI, i * 2);
            }
        }
    }
}
