using System;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.ranged.firearm
{
	public class Toxic_pistol_proj : ModProjectile
	{
		public float rotate = Main.rand.NextFloat(-1f, 1f);
        public bool impact = false;
        public float rotating_timer_sound = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
		public override void SetDefaults() {
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
            Projectile.timeLeft = 200;
		}
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1f, 0.8f, 0f);
            rotate = Math.Sign(rotate);
            Player player = Main.player[Projectile.owner];
            rotating_timer_sound++;
            float potency = 0.35f;
            if (impact)
            {
                Vector2 homing_back = player.Center - Projectile.Center;
                homing_back.Normalize();
                Projectile.velocity = homing_back * 18;
                Projectile.rotation += rotate * Projectile.velocity.X / 12 * potency;
                if (rotating_timer_sound > 40)
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/toxic_pistol_rotating") with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.position);
                    rotating_timer_sound = 0;
                }
                float targetDist = Vector2.Distance(player.Center, Projectile.Center);
                if (targetDist < 20f)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.rotation += rotate * Projectile.velocity.X / 12 * potency;
                Projectile.velocity *= 0.96f;
                Projectile.velocity.Y += 0.35f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
            {
                float size = Main.rand.NextFloat(0.35f, 0.85f);
                new Solid_particle(target.Center, Main.rand.NextVector2Circular(1f, 2f) * 5, Color.LightGreen, 120, size, 2f, 0f).Spawn();
                new Solid_particle(target.Center, Main.rand.NextVector2Circular(2f, 1f) * 5, Color.Green, 120, size, 2f, 0f).Spawn();
            }
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/toxic_pistol_impact") with { Pitch = Main.rand.NextFloat(-0.05f, 0.05f) }, Projectile.position);
            Projectile.Kill();
            Projectile.netUpdate = true;
		}
        public override bool OnTileCollide(Vector2 oldVelocity) {
			impact = true;
            Projectile.tileCollide = false;
			return false;
		}
        public override void OnKill(int timeLeft)
        {
            Projectile.netUpdate = true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.velocity = (target.Center - Projectile.Center) * 0.25f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Color color26 = Color.LightGreen;
            color26 = Projectile.GetAlpha(color26);
            for (float i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i += 0.33f)
            {
                int max0 = Math.Max((int)i - 1, 0);
                Vector2 center = Vector2.Lerp(Projectile.oldPos[(int)i], Projectile.oldPos[max0], 1 - i % 1);
                if (i < 4)
                {
                    Color color27 = color26;
                    color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - (i * 1.5f)) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    Main.EntitySpriteDraw(texture2D13, center + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, Projectile.oldRot[(int)i], origin2, Projectile.scale, SpriteEffects.None, 0);
                }
                 }
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
