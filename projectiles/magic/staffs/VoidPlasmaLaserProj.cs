using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using jarkos.particles;
using Terraria.GameContent;
namespace jarkos.projectiles.magic.staffs
{
    public class VoidPlasmaLaserProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Typhoon;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 45;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 6;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.2f, 0.8f, 0.2f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            NPC nextTarget = null;
            float closestDist = 800f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5 && npc.whoAmI != target.whoAmI)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        if (Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                        {
                            closestDist = dist;
                            nextTarget = npc;
                        }
                    }
                }
            }
            if (nextTarget != null)
            {
                Vector2 direction = nextTarget.Center - Projectile.Center;
                direction.Normalize();
                Projectile.velocity = direction * Projectile.velocity.Length();
                Projectile.netUpdate = true;
            }
            if (!Main.dedServ)
            {
                new Circleedge_tight(Projectile.Center, Vector2.Zero, Color.LimeGreen, 20, 0.2f, 1f, 0f).Spawn();
                for (int i = 0; i < 8; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f) + Projectile.velocity * 0.2f;
                    new Spark(Projectile.Center, sparkVel, Color.GreenYellow, 25, Main.rand.NextFloat(0.6f, 1.2f), 1f, 0f).Spawn();
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                new Circleedge_tight(Projectile.Center, Vector2.Zero, Color.LimeGreen, 25, 0.3f, 1f, 0f).Spawn();
                for (int i = 0; i < 15; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(8f, 8f) - Projectile.velocity * 0.3f;
                    new Spark(Projectile.Center, sparkVel, Color.Lime, 35, Main.rand.NextFloat(0.5f, 1f), 1f, 0f).Spawn();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Vector2[] positions = new Vector2[Projectile.oldPos.Length];
            int validCount = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    positions[validCount] = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    validCount++;
                }
            }
            if (validCount < 2) return false;
            Action<Color, float> DrawLaserLayer = (color, width) =>
            {
                for (int i = 0; i < validCount - 1; i++)
                {
                    Vector2 start = positions[i];
                    Vector2 end = positions[i + 1];
                    float length = Vector2.Distance(start, end);
                    float rotation = (end - start).ToRotation();
                    float progress = (float)i / (validCount - 1);
                    float currentWidth = width * (1f - progress);
                    Main.EntitySpriteDraw(magicPixel, end, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(1f, 0.5f), new Vector2(length, currentWidth), SpriteEffects.None, 0);
                }
            };
            Color outerColor = new Color(0, 255, 50, 0) * 0.4f;
            Color midColor = new Color(150, 255, 100, 0) * 0.8f;
            Color coreColor = new Color(220, 255, 220, 0);
            DrawLaserLayer(outerColor, 20f);
            DrawLaserLayer(midColor, 10f);
            DrawLaserLayer(coreColor, 3f);
            return false;
        }
    }
}
