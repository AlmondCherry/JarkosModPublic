using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.projectiles.mecha;
namespace jarkos.projectiles.mecha
{
    public class Astra_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Astra";
        public override float MuzzleForwardOffset => 185f;
        public override float MuzzleVerticalOffset => -4f;
        public override Vector2 HoldoutOrigin => new Vector2(64f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override bool ShouldFire(Player player) => false;
        public override void UpdateExtra(Player player)
        {
            Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
            if (shootTimer < 90)
            {
                if (shootTimer % 3 == 0)
                {
                    Vector2 spawnOffset = Main.rand.NextVector2CircularEdge(40f, 40f);
                    Vector2 spawnPos = muzzlePos + spawnOffset;
                    Vector2 particleVel = -spawnOffset * 0.1f;
                    new Spark(spawnPos, particleVel, new Color(255, 255, 255), 10 + (int)(shootTimer / 6f), 0.5f + (shootTimer / 90f) * 0.8f, 1f).Spawn();
                    Lighting.AddLight(muzzlePos, 0.2f * (shootTimer / 90f), 0.6f * (shootTimer / 90f), 1f * (shootTimer / 90f));
                }
            }
            else if (shootTimer == 90)
            {
                recoil = 12f;
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1.2f, Pitch = -0.3f }, Projectile.Center);
                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity, ModContent.ProjectileType<Astra_laser_proj>(), player.GetWeaponDamage(player.HeldItem), player.HeldItem.knockBack, Projectile.owner);
                }
                for (int i = 0; i < 20; i++)
                {
                    Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(4f, 15f);
                    new Spark(muzzlePos, sparkVel, new Color(150, 220, 255), 20, Main.rand.NextFloat(1f, 2f), 1f).Spawn();
                }
            }
            else if (shootTimer > 90)
            {
                Lighting.AddLight(muzzlePos, 0.5f, 1f, 1.5f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float currentRecoil = recoil;
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil;
            float rotation = Projectile.velocity.ToRotation();
            if (shootTimer > 60 && shootTimer < 90)
            {
                drawPos += Main.rand.NextVector2Circular(1f, 1f);
            }
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
            if (shootTimer > 0 && shootTimer <= 90)
            {
                Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
                Vector2 muzzleDrawPos = Projectile.Center + Projectile.velocity * 185f + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * -4f * Projectile.direction - Main.screenPosition - Projectile.velocity * currentRecoil;
                float chargeRatio = shootTimer / 90f;
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(glowTex, muzzleDrawPos, null, new Color(100, 200, 255) * chargeRatio, 0f, glowTex.Size() / 2f, chargeRatio * 0.4f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glowTex, muzzleDrawPos, null, Color.White * chargeRatio, 0f, glowTex.Size() / 2f, chargeRatio * 0.2f, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
