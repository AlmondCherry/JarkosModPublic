using jarkos.buffs.summonbuff;
using jarkos.items.armor.small_wooden;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.summon.staff
{
	public class EntlingProjectile : ModProjectile
    {
        private const int AttackCooldownMax = 30;
        private ref float AttackTimer => ref Projectile.ai[0];
        private const float DetectionRange = 400f;
        private const float FlySpeed = 4.5f;
        private float hoverAngle = 0f;

        private bool recoilDirection = false;
        private float currentRecoilAngle = 0f;
        private Vector2 squashScale = Vector2.One;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 58;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.netImportant = true;
            Projectile.scale = 1f;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            var modPlayer = owner.GetModPlayer<WoodArmorPlayer>();
            if (!modPlayer.hasWoodSet)
            {
                Projectile.Kill();
                return;
            }


            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 5;
            }


            currentRecoilAngle = MathHelper.Lerp(currentRecoilAngle, 0f, 0.1f);
            squashScale = Vector2.Lerp(squashScale, Vector2.One, 0.12f);

            Projectile.damage = modPlayer.GetEntlingDamage(owner);
            HoverAroundPlayer(owner);

            AttackTimer++;
            if (AttackTimer >= AttackCooldownMax)
            {
                NPC target = FindNearestTarget(owner);
                if (target != null)
                {
                    AttackTimer = 0;
                    ShootAtTarget(target, owner);
                }
            }

            if (Main.rand.NextBool(5))
            {
                SpawnLeafParticle();
            }
        }

        private void HoverAroundPlayer(Player owner)
        {
            hoverAngle += 0.03f;
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(hoverAngle) * 30f - 40f, -60f + (float)Math.Sin(hoverAngle * 2f) * 15f);
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();
            if (distance > 5f)
            {
                direction.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Math.Min(distance * 0.1f, FlySpeed), 0.2f);
            }
            else
            {
                Projectile.velocity *= 0.8f;
            }

            if (Projectile.velocity.X != 0)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
        }

        private NPC FindNearestTarget(Player owner)
        {
            NPC closestTarget = null;
            float closestDist = DetectionRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.lifeMax <= 5) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTarget = npc;
                }
            }
            return closestTarget;
        }

        private void ShootAtTarget(NPC target, Player owner)
        {
            if (Main.myPlayer != Projectile.owner) return;


            float targetRecoil = recoilDirection ? MathHelper.ToRadians(15f) : MathHelper.ToRadians(-15f);
            currentRecoilAngle = targetRecoil;
            recoilDirection = !recoilDirection;


            squashScale = new Vector2(1.28f, 0.6f);

            Vector2 direction = target.Center - Projectile.Center;
            direction.Normalize();
            direction = direction.RotatedByRandom(MathHelper.ToRadians(5f));
            float speed = 8f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * speed, ModContent.ProjectileType<EarthSpit>(), Projectile.damage, 1f, Projectile.owner);
            SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
        }

        private void SpawnLeafParticle()
        {
            int dustType = DustID.GreenMoss;
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f, 150, new Color(50, 200, 50), 0.8f);
            dust.noGravity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);

            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float totalRotation = Projectile.rotation + currentRecoilAngle;
            Vector2 drawScale = new Vector2(Projectile.scale) * squashScale;

            SpriteBatch sb = Main.spriteBatch;


            if (Projectile.velocity.Length() > 0.4f)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;

                    float progress = (float)i / Projectile.oldPos.Length;
                    float alpha = (1f - progress) * 0.10f;

                    Vector2 oldDrawPos = Projectile.oldPos[i] + new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f) - Main.screenPosition;
                    float oldRot = Projectile.oldRot[i] + currentRecoilAngle;

                    Color trailColor = new Color(50, 255, 60) * alpha;

                    sb.Draw(texture, oldDrawPos, frameRect, trailColor, oldRot, origin, drawScale, spriteEffects, 0f);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }


            Color glowColor = new Color(50, 255, 60) * 0.35f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(MathHelper.TwoPi / 4 * i);
                Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, frameRect, glowColor, totalRotation, origin, drawScale, spriteEffects, 0);
            }


            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frameRect, lightColor, totalRotation, origin, drawScale, spriteEffects, 0);
            return false;
        }
    }

	public class EarthSpit : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/summon/staff/dirtentling_projectile";
        private const int SlowDuration = 6;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.scale = 0.5f;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.02f;
            Projectile.velocity.X *= 0.99f;
            Projectile.rotation += 0.04f;
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position,Projectile.width,Projectile.height,DustID.Dirt,-Projectile.velocity.X * 0.2f,-Projectile.velocity.Y * 0.2f,100,default,0.7f);
                dust.noGravity = false;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.boss && !IsBossServant(target))
            {
                ApplySlowEffect(target);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust.NewDust(target.position, target.width, target.height, DustID.Dirt, vel.X, vel.Y, 150, default, 0.9f);
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust.NewDust(target.position, target.width, target.height, DustID.GreenMoss, vel.X, vel.Y, 100, new Color(100, 200, 50), 0.8f);
            }
        }
        private void ApplySlowEffect(NPC target)
        {
            target.AddBuff(ModContent.BuffType<EarthSlowBuff>(), SlowDuration);
        }
        private bool IsBossServant(NPC npc)
        {
            return NPCID.Sets.BossHeadTextures[npc.type] != -1;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, oldVelocity.X * -0.3f + Main.rand.NextFloat(-1f, 1f), oldVelocity.Y * -0.3f + Main.rand.NextFloat(-1f, 1f));
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            for (int i = 0; i < Math.Min(Projectile.oldPos.Length, 5); i++)
            {
                float alpha = (5 - i) / 5f * 0.4f;
                Color trailColor = new Color(139, 90, 43) * alpha;
                Main.EntitySpriteDraw(texture,Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,null,trailColor,Projectile.rotation,texture.Size() / 2f,Projectile.scale * (1f - i * 0.1f),SpriteEffects.None,0);
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
