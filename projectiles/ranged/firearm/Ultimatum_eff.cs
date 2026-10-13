using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.ranged.firearm
{
	public class Ultimatum_eff : ModProjectile {
		private const float MAX_RADIUS = 80f;
        private const int LIFETIME = 15;
        private bool dealtDamage = false;
        private float Progress => 1f - (float)Projectile.timeLeft / LIFETIME;
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LIFETIME;
            Projectile.light = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            if (!dealtDamage && Main.myPlayer == Projectile.owner)
            {
                dealtDamage = true;
                DealAreaDamage();
            }
            if (Projectile.timeLeft > LIFETIME - 3)
            {
                SpawnExplosionDust();
            }
            Projectile.light = (1f - Progress) * 1.2f;
        }
        private void DealAreaDamage()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy()) continue;
                float cx = MathHelper.Clamp(Projectile.Center.X, npc.position.X, npc.position.X + npc.width);
                float cy = MathHelper.Clamp(Projectile.Center.Y, npc.position.Y, npc.position.Y + npc.height);
                float dist = Vector2.Distance(Projectile.Center, new Vector2(cx, cy));
                if (dist < MAX_RADIUS)
                {
                    float mult = 1f - (dist / MAX_RADIUS) * 0.4f;
                    int dmg = (int)(Projectile.damage * mult);
                    if (dmg < 1) dmg = 1;
                    int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;
                    npc.SimpleStrikeNPC(dmg, dir, false, Projectile.knockBack * mult, Projectile.DamageType);
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, i, dmg, Projectile.knockBack * mult, dir);
                    }
                }
            }
        }
        private void SpawnExplosionDust()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f)
                    * Main.rand.NextFloat(0.3f, 1f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, vel.X, vel.Y, 0, default, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.6f;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueTorch, vel.X, vel.Y, 0, default, 1.2f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float progress = Progress;
            DrawShockwaveRing(pixel, center, progress);
            return false;
        }
        private void DrawShockwaveRing(Texture2D pixel, Vector2 center, float progress)
        {
            float radius = MAX_RADIUS * MathSystem.EaseOutCubic(progress);
            float alpha = (1f - progress) * 0.8f;
            float ringThickness = MathHelper.Lerp(12f, 3f, progress);
            int segments = 60;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi / segments * i;
                float nextAngle = MathHelper.TwoPi / segments * (i + 1);
                Vector2 start = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 end = center + new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * radius;
                Vector2 diff = end - start;
                float length = diff.Length();
                float rotation = diff.ToRotation();
                Color outerColor = new Color(30, 80, 200) * alpha * 0.4f;
                outerColor.A = 0;
                Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, ringThickness * 2.5f), SpriteEffects.None, 0);
                Color ringColor = new Color(100, 180, 255) * alpha * 0.7f;
                ringColor.A = 0;
                Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), ringColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, ringThickness), SpriteEffects.None, 0);
                Color coreColor = Color.White * alpha * 0.9f;
                coreColor.A = 0;
                Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, ringThickness * 0.4f), SpriteEffects.None, 0);
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - Progress);
        }
	}
}
