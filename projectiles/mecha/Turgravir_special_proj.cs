using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.systems;
using Terraria.Audio;
namespace jarkos.projectiles.mecha
{
    public class Turgravir_special_proj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.8f, 0.7f, 0.1f);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, -Projectile.velocity * 0.5f, 100, default, 1.2f);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.5f);
                dust.noGravity = true;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                int fragments = Main.rand.Next(3, 6);
                for (int i = 0; i < fragments; i++)
                {
                    Vector2 fragVel = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(4f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fragVel, ModContent.ProjectileType<Turgravir_frag_proj>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Main.spriteBatch.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.0f);
                effect.Parameters["uColor"]?.SetValue(new Vector4(1f, 0.8f, 0.1f, 1f));
                effect.Parameters["uColorCore"]?.SetValue(new Vector4(1f, 1f, 1f, 1f));
                List<Vector2> positions = new List<Vector2>();
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        positions.Add(Projectile.oldPos[i] + Projectile.Size / 2f);
                    }
                }
                if (positions.Count > 1)
                {
                    TrailRenderer.Draw(effect, positions, t => MathHelper.Lerp(12f, 0f, t), t => Color.White, HealShaders.HealNoise);
                }
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
