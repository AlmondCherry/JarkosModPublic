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
    public class Mecha_lightning_proj : ModProjectile
    {
        private List<Vector2> trailPositions;
        private List<int> hitTargets;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BulletHighVelocity;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;
        }
        public override void OnSpawn(IEntitySource source)
        {
            trailPositions = new List<Vector2>();
            hitTargets = new List<int>();
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 20)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 0.8f);
                d.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitTargets.Add(target.whoAmI);
            SoundEngine.PlaySound(SoundID.Item93, Projectile.Center);
            NPC nextTarget = null;
            float closestDist = 300f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage && !hitTargets.Contains(npc.whoAmI))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        nextTarget = npc;
                    }
                }
            }
            if (nextTarget != null)
            {
                Vector2 dir = (nextTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = dir * Projectile.velocity.Length();
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f)).noGravity = true;
                }
            }
            else
            {
                Projectile.Kill();
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
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 10f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(new Color(255, 255, 100, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 255, 255).ToVector4());
                    TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(6f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                finally
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return false;
        }
    }
}
