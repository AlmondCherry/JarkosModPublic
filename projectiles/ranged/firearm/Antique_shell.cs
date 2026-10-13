using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.ranged.firearm
{
    public class Antique_shell : ModProjectile
    {
        private List<Vector2> trailPositions;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 35;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            trailPositions = new List<Vector2>();
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 35)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                d.velocity *= 0.1f;
                d.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, default, 1.5f);
                d.velocity = -Projectile.velocity * 0.2f;
                d.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, vel.X, vel.Y, 100, default, 2f);
                Vector2 vel2 = Main.rand.NextVector2Circular(12f, 12f);
                Dust d2 = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, vel2.X, vel2.Y, 0, default, 2.5f);
                d2.noGravity = true;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                int shrapnelCount = Main.rand.Next(3, 6);
                for (int i = 0; i < shrapnelCount; i++)
                {
                    Vector2 shrapnelVel = -Projectile.oldVelocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(6f, 12f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shrapnelVel, ProjectileID.Bullet, (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.5f, Projectile.owner);
                }
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.CanBeChasedBy())
                    {
                        if (Vector2.Distance(Projectile.Center, npc.Center) < 100f)
                        {
                            npc.SimpleStrikeNPC(Projectile.damage, 0, false, Projectile.knockBack);
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                try
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(new Color(255, 100, 0).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(Color.Yellow.ToVector4());
                    TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(12f, 0f, t), t => Color.White * (1f - t),HealShaders.HealNoise);
                }
                finally
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return true;
        }
    }
}
