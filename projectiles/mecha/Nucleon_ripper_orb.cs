using System;
using jarkos.particles;
using jarkos.projectiles.melee.other_melee;
using jarkos.projectiles.melee.true_melee;
using Jarkos.projectiles.magic.staffs;
using Jarkos.projectiles.mecha;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
	public class Nucleon_ripper_orb : ModProjectile
	{
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 5;
            Projectile.DamageType = DamageClass.Default;
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);
            Player player = Main.player[Projectile.owner];
            Projectile.velocity *= 0.96f;
            if (Main.rand.NextBool(36))
            {
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Nucleon_noise_one") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
            }
            if (Main.rand.NextBool(12))
            {
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Nucleon_noise_two") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
            }
            if (Main.rand.NextBool(12))
            {
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Nucleon_noise_three") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
                CheckIntersections();
            }
            Dust dust;
            dust = Main.dust[Terraria.Dust.NewDust(Projectile.Center, 30, 30, DustID.PinkTorch, 0f, 0f, 0)];
            new Thunder_form_one(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), Color.MediumPurple, 300, Main.rand.NextFloat(0.15f, 1f), 2f, Main.rand.NextFloat(-0.25f, 0.25f)).Spawn();
        }
        private void CheckIntersections()
        {
            foreach (Projectile other in Main.projectile)
            {
                if (other.active && other.type == Projectile.type && other.whoAmI != Projectile.whoAmI)
                {
                    float distance = Vector2.Distance(Projectile.Center, other.Center);
                    if (distance < 80f)
                    {
                        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Nucleon_burst") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
                        Vector2 hz = Projectile.velocity;
                        if (hz.LengthSquared() < 0.1f)
                            hz = Main.rand.NextVector2CircularEdge(1f, 1f);
                        hz.Normalize();
                        other.velocity = hz * 16f;
                        Projectile.velocity = hz * -16f;
                        if (Projectile.owner == Main.myPlayer && Projectile.whoAmI < other.whoAmI)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Nucleon_wave_ring>(), Projectile.damage * 2, Projectile.knockBack * 2f, Projectile.owner);
                        }
                    }
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Nucleon_burst") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                new Spark(Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f))), Color.MediumPurple, 300, Main.rand.NextFloat(0.25f, 1.75f), 2f, 0f).Spawn();
            }
            Vector2 new_dir = Main.MouseWorld - Projectile.Center;
            new_dir.Normalize();
            Projectile.velocity = new_dir * 14f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f) {
                Projectile.velocity.X = oldVelocity.X * -0.9f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f) {
				Projectile.velocity.Y = oldVelocity.Y * -0.9f;
			}
			return false;
		}
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Texture2D glow = ModContent.Request<Texture2D>("jarkos/projectiles/mecha/Nucleon_ripper_orb_glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D glow2 = ModContent.Request<Texture2D>("jarkos/projectiles/mecha/Nucleon_ripper_orb_glow2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Color glowcolor = new(241, 164, 255);
            float size_multiplier = 0.3f;
            float size_multiplier2 = 0.25f;
            float glowscale = Projectile.scale * size_multiplier * (float)(Math.Sin(Projectile.timeLeft * 0.2f) * 2 + 5);
            float glowscale2 = Projectile.scale * size_multiplier2 * (float)(Math.Sin(Projectile.timeLeft * 0.2f) * 2 + 5);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, glowcolor, Projectile.rotation, glow.Size() / 2, glowscale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow2, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, glowcolor, Projectile.rotation, glow2.Size() / 2, glowscale2, SpriteEffects.None, 0);
            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
