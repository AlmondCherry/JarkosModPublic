using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.summon.staff
{
	public class BloodDropletRocket : ModProjectile
	{
		private float rotationSpeed;
        private bool hasBouncedOnce = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.alpha = 50;
            rotationSpeed = Main.rand.NextFloat(-0.3f, 0.3f);
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.25f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
            Projectile.rotation += rotationSpeed;
            Projectile.velocity.X *= 0.99f;
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 150, default, 0.8f);
                dust.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.3f, 0.05f, 0.05f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!hasBouncedOnce)
            {
                hasBouncedOnce = true;
                Projectile.penetrate--;
                SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.position);
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0.1f)
                {
                    Projectile.velocity.Y = -oldVelocity.Y * 0.4f;
                }
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0.1f)
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.4f;
                }
                for (int i = 0; i < 5; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f), 100, default, 1.2f);
                    dust.noGravity = false;
                }
                return false;
            }
            else
            {
                CreateBloodSplatter();
                return true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
                dust.noGravity = false;
            }
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.7f }, target.position);
        }
        private void CreateBloodSplatter()
        {
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 0f), 150, default, Main.rand.NextFloat(0.8f, 1.5f));
                dust.noGravity = false;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 1f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = 1f - progress;
                float scale = MathHelper.Lerp(1f, 0.3f, progress);
                Color trailColor = new Color(139, 0, 0) * alpha * 0.6f;
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, scale * 0.7f, SpriteEffects.None, 0);
            }
            Color mainColor = new Color(180, 30, 30);
            Vector2 mainDrawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(texture, mainDrawPos, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(180, 30, 30, 200);
        }
	}
}
