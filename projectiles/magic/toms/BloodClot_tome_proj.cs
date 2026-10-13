using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.particles;

namespace jarkos.projectiles.magic.toms
{
    public class BloodClot_tome_proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.4f;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.rotation += 0.2f;
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, 0, 0, 80, default, 1.2f);
                d.noGravity = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Point hitTile = Projectile.Center.ToTileCoordinates();
                if (oldVelocity.Y > 0) hitTile.Y += 1;
                else if (oldVelocity.Y < 0) hitTile.Y -= 1;
                else if (oldVelocity.X > 0) hitTile.X += 1;
                else if (oldVelocity.X < 0) hitTile.X -= 1;
                for (int i = -1; i <= 1; i++)
                {
                    int x = hitTile.X + i;
                    int y = hitTile.Y;
                    Vector2 spawnPos = new Vector2(x * 16 + 8, y * 16 + 8);
                    bool infected = false;
                    for (int j = 0; j < Main.maxProjectiles; j++)
                    {
                        Projectile p = Main.projectile[j];
                        if (p.active && p.type == ModContent.ProjectileType<BloodInfectedBlock_proj>() && p.Center == spawnPos)
                        {
                            infected = true;
                            break;
                        }
                    }
                    if (!infected)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<BloodInfectedBlock_proj>(), Projectile.damage / 2, 0f, Projectile.owner);
                    }
                }
            }
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.6f, Pitch = 0.5f }, Projectile.Center);
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int count = Main.rand.Next(3, 6);
                for (int i = 0; i < count; i++)
                {
                    Vector2 vel = Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.8f, 1.4f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, vel, ModContent.ProjectileType<BloodClotFragment>(), Projectile.damage / 2, Projectile.knockBack / 2f, Projectile.owner);
                }
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 speed = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(4f, 8f);
                if (speed == Vector2.Zero) speed = Main.rand.NextVector2Circular(4f, 4f);
                new Spark(target.Center, speed, new Color(255, 30, 30), 15, Main.rand.NextFloat(0.5f, 1f), 1f, 0f).Spawn();
            }
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 20, 20).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 100, 100).ToVector4());
                int numPoints = Projectile.oldPos.Length;
                Vector2[] points = new Vector2[numPoints];
                Vector2 half = Projectile.Size * 0.5f;
                for (int i = 0; i < numPoints; i++)
                {
                    points[i] = Projectile.oldPos[i] == Vector2.Zero ? Projectile.Center : Projectile.oldPos[i] + half;
                }
                TrailRenderer.Draw(effect, points, t => 30f * MathF.Pow(1f - t, 0.2f) * MathHelper.Clamp(t * 10f, 0f, 1f), t => Color.White * (1f - t), HealShaders.HealNoise);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
