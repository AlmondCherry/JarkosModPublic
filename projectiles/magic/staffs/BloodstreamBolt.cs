using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
namespace Jarkos.projectiles.magic.staffs
{
	public class BloodstreamBolt : ModProjectile
	{
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        private float timer = 0f;
        public override void AI()
        {
            if (Projectile.ai[0] >= 3)
            {
                Projectile.friendly = false;
            }
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.25f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
            Lighting.AddLight(Projectile.Center, 0.6f, 0.1f, 0.1f);
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, 0, 0, 100, default, 1.2f);
                d.noGravity = false;
                d.velocity = Projectile.velocity * 0.2f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<BloodstreamHitVisual>(), 0, 0, Projectile.owner);

                if (Projectile.ai[0] < 3)
                {
                    Projectile.ai[0]++;
                    NPC nextTarget = null;
                    float closestDist = 600f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5 && npc.whoAmI != target.whoAmI)
                        {
                            float dist = Vector2.Distance(Projectile.Center, npc.Center);
                            if (dist < closestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                            {
                                closestDist = dist;
                                nextTarget = npc;
                            }
                        }
                    }
                    if (nextTarget != null)
                    {
                        Vector2 direction = nextTarget.Center - Projectile.Center;
                        direction.Y -= MathHelper.Min(direction.Length() * 0.2f, 80f);
                        direction.Normalize();
                        float jumpSpeed = 15f;
                        Projectile.velocity = direction * jumpSpeed;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.ai[0] = 3;
                        Projectile.netUpdate = true;
                    }
                }
                if (Projectile.ai[0] >= 3)
                {
                    Projectile.friendly = false;
                    Projectile.velocity.X *= 0.5f;
                    Projectile.velocity.Y *= 0.5f;
                    Projectile.netUpdate = true;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D px = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float p = (float)i / Projectile.oldPos.Length;
                Vector2 p1 = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 p2 = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 dir = p2 - p1;
                float len = dir.Length();
                if (len == 0) continue;
                float rot = dir.ToRotation();
                float fade = 1f - p;
                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(80, 5, 5, 0) * fade * 0.45f * ((255 - Projectile.alpha) / 255f), rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(12f, 2f, p)), SpriteEffects.None, 0f);
                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(220, 30, 30, 0) * fade * 0.6f * ((255 - Projectile.alpha) / 255f), rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(6f, 1f, p)), SpriteEffects.None, 0f);
                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(150, 10, 10, 0) * fade * 0.8f * ((255 - Projectile.alpha) / 255f), rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(2.5f, 0.5f, p)), SpriteEffects.None, 0f);
            }
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                vel.Y -= 3f;
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, 1.4f);
                d.noGravity = false;
            }
        }
    }

    public class BloodstreamHitVisual : ModProjectile
    {
        public override string Texture => "jarkos/particles/Spark";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 2;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            if (Projectile.timeLeft == 2 && !Main.dedServ)
            {
                new Circleedge(Projectile.Center, Vector2.Zero, Color.DarkRed, 40, 0.4f, 1f, 0f).Spawn();
                for (int i = 0; i < 15; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(7f, 7f);
                    new Spark(Projectile.Center, sparkVel, Color.Red, 30, Main.rand.NextFloat(0.8f, 1.3f), 1f, 0f).Spawn();
                }
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, 1.5f);
                    d.noGravity = false;
                }
            }
        }
    }
}
