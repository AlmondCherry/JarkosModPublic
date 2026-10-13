using jarkos.buffs.debuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
	public class Atomic_projectile : ModProjectile
	{
		private List<Vector2> oldPositions = new List<Vector2>();
        private const int TrailLength = 20;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0f, 1f, 0.4f);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                dust.velocity = -Projectile.velocity * 0.2f;
                dust.noGravity = true;
                dust.scale = 1f;
            }
            if (Main.rand.NextBool(4))
            {
                Dust glow = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                glow.velocity = Vector2.Zero;
                glow.noGravity = true;
                glow.scale = 0.5f;
            }
            Projectile.scale = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.15f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateAtomicExplosion();
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            CreateAtomicExplosion();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateAtomicExplosion();
            return true;
        }
        private void CreateAtomicExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.6f }, Projectile.Center);
            int fragmentCount = Main.rand.Next(2, 3);
            for (int i = 0; i < fragmentCount; i++)
            {
                float angle = MathHelper.TwoPi / fragmentCount * i;
                angle += Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(6f, 12f);
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AtomicFragment>(), Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                dust.velocity = dustVel;
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                dust.color = new Color(0, 255, 100);
            }
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi / 40 * i;
                Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                Dust ring = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                ring.velocity = dustVel;
                ring.noGravity = true;
                ring.scale = 2f;
            }
            for (int i = 0; i < 5; i++)
            {
                Dust glow = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                glow.velocity = Main.rand.NextVector2Circular(4f, 4f);
                glow.noGravity = true;
                glow.scale = Main.rand.NextFloat(2f, 3f);
            }
            for (int i = 0; i < 2; i++)
            {
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), GoreID.Smoke1 + Main.rand.Next(3));
                gore.velocity *= 0.5f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float scale = Projectile.scale * (1f - progress) * 0.8f;
                float alpha = (1f - progress) * 0.6f;
                Color trailColor = Color.Lerp(new Color(255, 255, 0), new Color(255, 140, 0), progress) * alpha;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
            Color glowColor = new Color(255, 255, 0, 0) * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2, 0).RotatedBy(MathHelper.PiOver2 * i);
                Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 200);
        }
	}
	public class AtomicFragment : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private float rotationSpeed;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.light = 0.4f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.alpha = 50;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                rotationSpeed = Main.rand.NextFloat(0.2f, 0.4f) * (Main.rand.NextBool() ? 1 : -1);
                Projectile.ai[0] = 1;
                for (int i = 0; i < 5; i++)
                {
                    Dust spawn = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                    spawn.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    spawn.noGravity = true;
                    spawn.scale = 1f;
                    spawn.color = new Color(0, 255, 100);
                }
            }
            Projectile.rotation += rotationSpeed + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.05f;
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha += 8;
                Projectile.scale *= 0.98f;
            }
            float intensity = 0.4f * (Projectile.timeLeft / 120f);
            Lighting.AddLight(Projectile.Center, 0f, intensity, intensity * 0.4f);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                dust.velocity = Projectile.velocity * 0.1f;
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.6f, 1f);
            }
            if (Main.rand.NextBool(6))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                Spark.velocity = Main.rand.NextVector2Circular(2f, 2f);
                Spark.noGravity = true;
                Spark.scale = 0.7f;
                Spark.color = new Color(100, 255, 150);
            }
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f + Projectile.whoAmI) * 0.1f;
            Projectile.scale = 1f + pulse;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Splitting_buff>(), 180);
            CreateHitEffect();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                CreateHitEffect();
                return true;
            }
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X * 0.7f;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.4f }, Projectile.Center);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.color = new Color(0, 255, 100);
            }
            return false;
        }
        private void CreateHitEffect()
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                dust.color = new Color(0, 255, 100);
            }
            for (int i = 0; i < 4; i++)
            {
                Dust glow = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                glow.velocity = Main.rand.NextVector2Circular(3f, 3f);
                glow.noGravity = true;
                glow.scale = 1.2f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch);
                dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                dust.noGravity = true;
                dust.scale = 0.8f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float scale = Projectile.scale * (1f - progress) * 0.7f;
                float alpha = (1f - progress) * 0.5f;
                Color trailColor = Color.Lerp(new Color(255, 255, 0), new Color(255, 140, 0), progress) * alpha;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
            float glowPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.2f;
            Color glowColor = new Color(255, 255, 0, 0) * 0.4f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(3 * glowPulse, 0).RotatedBy(MathHelper.TwoPi / 6 * i + Main.GameUpdateCount * 0.05f);
                Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, glowColor, Projectile.rotation + MathHelper.PiOver2, origin, Projectile.scale / 2, SpriteEffects.None, 0);
            }
            Color mainColor = Color.Lerp(Color.White, new Color(255, 255, 255), 0.3f);
            mainColor *= (255 - Projectile.alpha) / 255f;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, mainColor, Projectile.rotation + MathHelper.PiOver2, origin, Projectile.scale / 2, SpriteEffects.None, 0);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 150) * ((255 - Projectile.alpha) / 255f);
        }
    }
}
