using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using jarkos.items.weapons.ranged.firing;
namespace jarkos.projectiles.mecha
{
    public class Blood_thinner_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Blood_thinner";
        public int charges = 0;
        public int chargeTimer = 0;
        public bool isFiringBurst = false;
        public int burstTimer = 0;
        public override Vector2 HoldoutOrigin => new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width * 0.25f, TextureAssets.Projectile[Projectile.type].Value.Height * 0.5f);
        public override float MuzzleForwardOffset => 75f;
        public override float MuzzleVerticalOffset => -10f;
        public override bool ConsumeAmmo() => false;
        public override bool KeepAliveCondition(Player player)
        {
            return player.channel || isFiringBurst || charges > 0;
        }
        public override bool ShouldFire(Player player)
        {
            return false;
        }
        public override void AI()
        {
            base.AI();
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.CCed)
            {
                Projectile.Kill();
                return;
            }
            if (player.channel && !isFiringBurst)
            {
                chargeTimer++;
                Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset;
                Lighting.AddLight(muzzlePos, (charges * 0.1f + chargeTimer / 30f * 0.1f + 0.1f), 0f, 0f);
                if (Main.rand.NextBool(3))
                {
                    Vector2 randomOffset = Main.rand.NextVector2CircularEdge(40f, 40f);
                    new jarkos.particles.Stretching_particle(muzzlePos + randomOffset, muzzlePos, Color.Red * 0.8f, 20, 30f, 4f, 0.5f, player).Spawn();
                }
                if (chargeTimer >= 30)
                {
                    if (charges < 5)
                    {
                        charges++;
                        SoundEngine.PlaySound(SoundID.Item17 with { Pitch = -0.5f + charges * 0.15f, Volume = 0.5f + charges * 0.1f }, player.Center);
                        for (int i = 0; i < charges * 3; i++)
                        {
                            Vector2 dustPos = muzzlePos + Main.rand.NextVector2Circular(35f, 35f);
                            Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.Blood, 0f, 0f, 150, default, 1.2f);
                            dust.noGravity = true;
                            dust.velocity = (muzzlePos - dustPos).SafeNormalize(Vector2.Zero) * 2.5f;
                        }
                    }
                    chargeTimer = 0;
                }
            }
            else if (!player.channel && charges > 0 && !isFiringBurst)
            {
                isFiringBurst = true;
                burstTimer = 0;
            }
            else if (!player.channel && charges == 0 && !isFiringBurst)
            {
                Projectile.Kill();
            }
            if (isFiringBurst)
            {
                burstTimer++;
                if (burstTimer % 6 == 0)
                {
                    FireProjectile(player);
                    charges--;
                    if (charges <= 0)
                    {
                        isFiringBurst = false;
                        Projectile.Kill();
                    }
                }
            }
        }
        private void FireProjectile(Player player)
        {
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Vector2 muzzle = Projectile.Center + Projectile.velocity * MuzzleForwardOffset;
            float speed = 14f + charges * 1.5f;
            SoundEngine.PlaySound(SoundID.Item95 with { Pitch = 0.1f * charges, Volume = 0.6f + 0.1f * charges }, player.Center);
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = direction.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(2f, 5f);
                Dust flash = Dust.NewDustDirect(muzzle, 1, 1, DustID.Blood, vel.X, vel.Y, 80, default, 1.2f);
                flash.noGravity = true;
            }
            Vector2 shootDir = direction.RotatedByRandom(MathHelper.ToRadians(2f));
            if (Main.myPlayer == Projectile.owner)
            {
                int damage = player.GetWeaponDamage(player.HeldItem);
                float knockback = player.GetWeaponKnockback(player.HeldItem, player.HeldItem.knockBack);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzle, shootDir * speed, ModContent.ProjectileType<Blood_thinner_proj>(), damage + charges * 5, knockback + charges * 0.5f, player.whoAmI, ai0: charges);
            }
            recoil = 8f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (charges > 0 && !isFiringBurst)
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                float glowIntensity = (float)charges / 5f;
                float pulse = (float)Math.Sin(chargeTimer * 0.2f) * 0.3f + 0.7f;
                Color glowColor = new Color(255, 30, 30, 0) * glowIntensity * pulse * 0.5f;
                Vector2 origin = HoldoutOrigin;
                if (Projectile.spriteDirection == -1)
                {
                    origin.Y = texture.Height - origin.Y;
                }
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                spriteBatch.Draw(texture, drawPos, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.08f, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            }
            return base.PreDraw(ref lightColor);
        }
    }
}
