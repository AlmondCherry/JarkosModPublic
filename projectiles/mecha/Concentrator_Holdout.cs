using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using Jarkos.items.weapons.ranged.firing;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.projectiles.mecha
{
    public class Concentrator_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Concentrator";
        public override float MuzzleForwardOffset => 127f;
        public override float MuzzleVerticalOffset => -6f;
        public override Vector2 HoldoutOrigin => new Vector2(25f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override int FirstShotDelay => 30;
        public override float CustomShootSpeed => 12f;
        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 12 : useTime;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            var weapon = player.HeldItem.ModItem as Concentrator;
            int stacks = 0;
            if (weapon != null)
            {
                if (weapon.concentrationStacks < Concentrator.MaxStacks)
                    weapon.concentrationStacks++;
                weapon.concentrationTimer = Concentrator.DecayTime;
                stacks = weapon.concentrationStacks;
            }
            float pitch = 0.8f + (stacks / (float)Concentrator.MaxStacks) * 0.4f;
            SoundEngine.PlaySound(SoundID.Item91 with
            {
                Pitch = pitch - 1f, Volume = 0.7f + (stacks / (float)Concentrator.MaxStacks) * 0.3f
            }, Projectile.position);
            recoil = 3f + (stacks / (float)Concentrator.MaxStacks) * 2f;
            for (int i = 0; i < 2 + stacks; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 6f);
                new Spark(muzzlePos, sparkVel, new Color(100, 200, 255), 12 + stacks, Main.rand.NextFloat(0.8f, 1.2f) + (stacks / (float)Concentrator.MaxStacks) * 0.5f, 1f).Spawn();
            }
            if (weapon != null)
            {
                float stackMultiplier = 1f + (stacks / (float)Concentrator.MaxStacks) * 1f;
                damage = (int)(20 * stackMultiplier);
            }
            float speedMultiplier = 1f + (stacks / (float)Concentrator.MaxStacks) * 0.5f;
            speed *= speedMultiplier;
            Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(3f - stacks * 0.2f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Concentrator_proj>(), damage, player.HeldItem.knockBack, Projectile.owner, stacks);
        }
    }
}
