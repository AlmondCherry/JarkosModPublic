using Jarkos.trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.magic.other
{
	public class VoidPlasmaCharged : ModProjectile
	{
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 28;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.6f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        private float timer = 0f;
        private float spiralAngle = 0f;
        public override void AI()
        {
            timer++;
            spiralAngle += 0.2f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float pulse = (float)Math.Sin(timer * 0.15f) * 0.3f + 0.8f;
            Lighting.AddLight(Projectile.Center, 0.7f * pulse, 0.2f * pulse, 1f * pulse);
            for (int i = 0; i < 2; i++)
            {
                float angle = spiralAngle + i * MathHelper.Pi;
                float radius = 10f + (float)Math.Sin(timer * 0.3f) * 3f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Dust dust = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.PurpleTorch, 0, 0, 80, new Color(200, 80, 255), 0.8f);
                dust.noGravity = true;
                dust.velocity = offset.RotatedBy(MathHelper.PiOver2) * 0.03f;
            }
            if (Main.rand.NextBool(3))
            {
                Dust dark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Shadowflame, 0, 0, 100, default, 0.6f);
                dark.noGravity = true;
                dark.velocity *= 0.2f;
            }
            NPC target = null;
            float bestDist = 200f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < bestDist) { bestDist = dist; target = npc; }
                }
            }
            if (target != null && timer > 10)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.03f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D px = TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            for (int layer = 0; layer < 3; layer++)
            {
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
                    Color c;
                    float w;
                    switch (layer)
                    {
                        case 0:
                            c = new Color(40, 5, 80, 0) * fade * 0.25f;
                            w = MathHelper.Lerp(16f, 3f, p);
                            break;
                        case 1:
                            c = new Color(140, 40, 220, 0) * fade * 0.45f;
                            w = MathHelper.Lerp(8f, 1.5f, p);
                            break;
                        default:
                            c = new Color(220, 160, 255, 0) * fade * 0.55f;
                            w = MathHelper.Lerp(3f, 0.4f, p);
                            break;
                    }
                    sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), c, rot, new Vector2(0, 0.5f), new Vector2(len, w), SpriteEffects.None, 0f);
                }
            }
            for (int side = -1; side <= 1; side += 2)
            {
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
                    float wave = (float)Math.Sin(p * 12f + timer * 0.2f * side) * 5f * (1f - p);
                    Vector2 perp = dir.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2)
                        * wave * side;
                    sb.Draw(px, p1 + perp, new Rectangle(0, 0, 1, 1), new Color(160, 60, 255, 0) * (1f - p) * 0.2f, dir.ToRotation(), new Vector2(0, 0.5f), new Vector2(len, 2.5f * (1f - p)), SpriteEffects.None, 0f);
                }
            }
            float glow = (float)Math.Sin(timer * 0.12f) * 0.3f + 0.8f;
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(80, 15, 150, 0) * 0.2f * glow, 0f, new Vector2(0.5f), new Vector2(40f) * glow, SpriteEffects.None, 0f);
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(160, 60, 240, 0) * 0.35f * glow, 0f, new Vector2(0.5f), new Vector2(20f) * glow, SpriteEffects.None, 0f);
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(220, 180, 255, 0) * 0.6f, 0f, new Vector2(0.5f), new Vector2(10f), SpriteEffects.None, 0f);
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(245, 235, 255, 0) * 0.9f, 0f, new Vector2(0.5f), new Vector2(4f), SpriteEffects.None, 0f);
            for (int i = 0; i < 5; i++)
            {
                float angle = spiralAngle * 0.5f + i * MathHelper.TwoPi / 5f;
                float rayLen = 12f + (float)Math.Sin(timer * 0.2f + i * 1.3f) * 5f;
                sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(180, 80, 255, 0) * 0.3f, angle, new Vector2(0, 0.5f), new Vector2(rayLen, 2f), SpriteEffects.None, 0f);
            }
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType, vel.X, vel.Y, 0, default, Main.rand.NextFloat(1f, 1.8f));
                d.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 180);
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.PurpleTorch, vel.X, vel.Y, 0, new Color(200, 80, 255), 1.4f);
                d.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Shadowflame, vel.X, vel.Y, 100, default, 1.5f);
                d.noGravity = true;
            }
        }
    }
}
