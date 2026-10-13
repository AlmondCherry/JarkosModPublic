using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
namespace jarkos.projectiles.melee.other_melee
{
    public class SteelBoomerangProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1200;
        }
        public override void AI()
        {
            float currentSpeed = Projectile.velocity.Length();
            float rotSpeed = 0.15f + (currentSpeed * 0.025f);
            Projectile.rotation += rotSpeed * (float)Projectile.direction;
            Projectile.localAI[1] = rotSpeed;
            Player player = Main.player[Projectile.owner];
            if (player.dead)
            {
                Projectile.Kill();
                return;
            }
            Projectile.localAI[0]++;
            if (Projectile.ai[0] == 0f)
            {
                if (Projectile.localAI[0] > 35f)
                {
                    Projectile.ai[0] = 1f;
                    Projectile.netUpdate = true;
                }
            }
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.tileCollide = false;
                Vector2 vectorToPlayer = player.Center - Projectile.Center;
                float distance = vectorToPlayer.Length();
                if (distance < 30f)
                {
                    Projectile.Kill();
                    return;
                }
                float returnSpeed = 22f;
                vectorToPlayer.Normalize();
                vectorToPlayer *= returnSpeed;
                Projectile.velocity = (Projectile.velocity * 4f + vectorToPlayer) / 5f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            float radius = Projectile.width / 2f;
            float rotSpeed = Projectile.localAI[1];
            float intensity = MathHelper.Clamp(rotSpeed / 0.4f, 0.2f, 1.5f);
            for (int trailId = 0; trailId < 2; trailId++)
            {
                float angleOffset = (trailId == 0 ? 0f : MathHelper.Pi) + MathHelper.PiOver4;
                for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                        continue;
                    Vector2 centerStart = Projectile.oldPos[i] + Projectile.Size / 2f;
                    Vector2 centerEnd = Projectile.oldPos[i + 1] + Projectile.Size / 2f;
                    float rotStart = Projectile.oldRot[i];
                    float rotEnd = Projectile.oldRot[i + 1];
                    Vector2 bladeStart = centerStart + new Vector2(radius, 0).RotatedBy(rotStart + angleOffset) - Main.screenPosition;
                    Vector2 bladeEnd = centerEnd + new Vector2(radius, 0).RotatedBy(rotEnd + angleOffset) - Main.screenPosition;
                    float progress = (float)i / (Projectile.oldPos.Length - 1);
                    float taper = 1f - progress;
                    float width = 5f * taper * intensity;
                    Color color = new Color(200, 220, 255) * taper * intensity * 0.8f;
                    color.A = 0;
                    Vector2 direction = bladeStart - bladeEnd;
                    float length = direction.Length();
                    float rotation = direction.ToRotation();
                    Main.EntitySpriteDraw(magicPixel, bladeEnd, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0);
                }
            }
            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 0f)
            {
                Projectile.ai[0] = 1f;
                Projectile.velocity = -oldVelocity;
                Projectile.netUpdate = true;
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 40; i++)
            {
                Vector2 speed = new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-8f, 2f));
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, speed.X, speed.Y, 100, default, 1.8f);
                dust.noGravity = false;
            }
        }
    }
}
