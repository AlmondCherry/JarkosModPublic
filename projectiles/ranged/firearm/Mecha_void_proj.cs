using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.ranged.firearm
{
    public class Mecha_void_proj : ModProjectile
    {
        private List<Vector2> trailPositions;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BulletHighVelocity;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
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
            if (trailPositions.Count > 18)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, default, 1.2f);
                d.noGravity = true;
                d.velocity *= 0.1f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Player player = Main.player[Projectile.owner];
                int healAmount = (int)(damageDone * 0.05f);
                if (healAmount > 0)
                {
                    player.statLife += healAmount;
                    player.HealEffect(healAmount);
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Shadowflame, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1.5f);
                d.noGravity = true;
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
                    float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.3f + 1f;
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(new Color(140, 0, 255, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(200, 100, 255, 255).ToVector4());
                    TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(12f * pulse, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
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
