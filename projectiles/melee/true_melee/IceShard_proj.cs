using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class IceShard_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/true_melee/IceShard_proj";
        private const float Gravity = 0.18f;
        private const float MaxFallSpeed = 14f;
        private const int Lifetime = 120;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 18;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }
        public override void AI()
        {
            Projectile.velocity.Y = MathF.Min(Projectile.velocity.Y + Gravity, MaxFallSpeed);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.18f, 0.40f, 0.65f);
            if (Main.rand.NextBool(2))
            {
                Vector2 dv = Projectile.velocity * -0.12f + Main.rand.NextVector2Circular(0.6f, 0.6f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SnowflakeIce, dv, 100, default, Main.rand.NextFloat(0.7f, 1.2f));
                d.noGravity = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SnowflakeIce, dv, 60, default, 1.1f);
                d.noGravity = Main.rand.NextBool(2);
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.4f, Volume = 0.25f }, Projectile.Center);
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            for (int i = 0; i < 8; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(3.5f, 3.5f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.SnowflakeIce, dv, 60, default, Main.rand.NextFloat(1.0f, 1.5f));
                d.noGravity = Main.rand.NextBool(2);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texAsset = TextureAssets.Projectile[Projectile.type];
            if (!texAsset.IsLoaded) return true;
            Texture2D texture = texAsset.Value;
            Vector2 origin = texture.Size() / 2f;
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 oldPos = Projectile.oldPos[i];
                if (oldPos == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                Color trailColor = new Color(150, 215, 255) * (fade * 0.45f);
                Vector2 screenPos = oldPos + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(texture, screenPos, null, trailColor, Projectile.rotation, origin, Projectile.scale * (0.85f + 0.2f * fade), SpriteEffects.None, 0);
            }
            Color baseColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, baseColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
