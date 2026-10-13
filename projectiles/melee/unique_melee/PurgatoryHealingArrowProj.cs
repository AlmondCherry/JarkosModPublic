using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class PurgatoryHealingArrowProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HolyArrow;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.55f;
            if (Projectile.velocity.Y > 42f)
            {
                Projectile.velocity.Y = 42f;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GoldFlame, Projectile.velocity * -0.05f, Scale: Main.rand.NextFloat(0.85f, 1.25f));
                dust.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.45f, 0.35f, 0.12f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            {
                return;
            }
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                return;
            }
            if (owner.GetModPlayer<PurgatoryPlayer>().TryHealFromArrowRain())
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(owner.Center, DustID.HealingPlus, Main.rand.NextVector2Circular(2f, 2f), Scale: 1.2f);
                    dust.noGravity = true;
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(2.5f, 2.5f), Scale: Main.rand.NextFloat(1.0f, 1.6f));
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            int trailLen = Projectile.oldPos.Length;
            for (int i = trailLen - 1; i >= 1; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                {
                    continue;
                }
                float t = 1f - i / (float)trailLen;
                Color trailColor = new Color(255, 220, 140, 0) * (t * 0.55f);
                float trailScale = Projectile.scale * (0.55f + t * 0.45f);
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, null, trailColor, Projectile.rotation, origin, trailScale, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 245, 200), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
