using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class PurgatoryStarProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StarCannonStar;
        private const int DriftFrames = 18;
        private const float HomingRange = 720f;
        private const float HomingTurn = 0.18f;
        private const float HomingMaxSpeed = 14f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Projectile.rotation += 0.25f;
            int driftedFor = (int)Projectile.ai[0];
            Projectile.ai[0] = driftedFor + 1;
            if (driftedFor < DriftFrames)
            {
                Projectile.velocity *= 0.95f;
            }
            else
            {
                NPC target = FindTarget();
                if (target != null)
                {
                    Vector2 toTarget = target.Center - Projectile.Center;
                    Vector2 desired = toTarget.SafeNormalize(Vector2.UnitY) * HomingMaxSpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingTurn);
                    if (Projectile.velocity.Length() < 4f)
                    {
                        Projectile.velocity = desired * 0.5f;
                    }
                }
                else
                {
                    Projectile.velocity *= 0.985f;
                }
            }
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowStarDust, Vector2.Zero, Scale: 1.1f);
                dust.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.55f, 0.45f, 0.15f);
        }
        private NPC FindTarget()
        {
            NPC best = null;
            float bestDist = HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
                {
                    continue;
                }
                if (!npc.CanBeChasedBy(Projectile))
                {
                    continue;
                }
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < bestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                {
                    best = npc;
                    bestDist = dist;
                }
            }
            return best;
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
                Color trailColor = new Color(255, 220, 130, 0) * (t * 0.65f);
                float trailScale = Projectile.scale * (0.4f + t * 0.6f);
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, null, trailColor, Projectile.rotation - i * 0.3f, origin, trailScale, SpriteEffects.None, 0);
            }
            Color glow = new Color(255, 230, 140, 0);
            for (int i = 0; i < 4; i++)
            {
                float a = i / 4f * MathHelper.TwoPi;
                Vector2 jitter = a.ToRotationVector2() * 1.6f;
                Main.EntitySpriteDraw(texture, Projectile.Center + jitter - Main.screenPosition, null, glow * 0.5f, Projectile.rotation, origin, Projectile.scale * 1.65f, SpriteEffects.None, 0);
            }
            Color mainColor = new Color(255, 248, 210);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
