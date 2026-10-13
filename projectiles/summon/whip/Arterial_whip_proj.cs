using jarkos.buffs.summonbuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon.whip
{
	public class Arterial_whip_proj : ModProjectile
	{
        private const float ExecuteChance = 0.25f;
        private const float HealthThreshold = 0.20f;
        private HashSet<int> hitNPCs = new HashSet<int>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 12;
            Projectile.WhipSettings.RangeMultiplier = 1.3f;
            Projectile.netImportant = true;
        }
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
            Timer++;
            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (Timer >= swingTime || owner.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }
            owner.heldProj = Projectile.whoAmI;
            if (Timer == swingTime / 2)
            {
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
            }
            if (Timer > 5 && Main.rand.NextBool(3))
            {
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                int index = Main.rand.Next(points.Count / 2, points.Count);
                Vector2 pos = points[index];
                Dust dust = Dust.NewDustDirect(pos - new Vector2(4), 8, 8, DustID.Blood, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
                dust.velocity *= 0.3f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hitNPCs.Contains(target.whoAmI))
                return;
            hitNPCs.Add(target.whoAmI);
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            target.AddBuff(ModContent.BuffType<Arterial_whip_tag>(), 240);
            SpawnHitParticles(target.Center, 10);

            bool isBoss = target.boss || NPCID.Sets.ShouldBeCountedAsBoss[target.type];
            if (!isBoss && (float)target.life / target.lifeMax <= HealthThreshold && Main.rand.NextFloat() < ExecuteChance)
            {
                PerformExecution(target);
            }
        }
        private void PerformExecution(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.5f }, target.Center);
            SpawnDeathRing(target.Center, 40, 6f);
            SpawnSoulSpiral(target.Center, 20);
            SpawnDeathExplosion(target.Center, 50);
            SpawnLightBurst(target.Center);
            SpawnDeathCross(target.Center, 15);
            
            target.life = 0;
            target.HitEffect();
            target.checkDead();
        }
        private void SpawnHitParticles(Vector2 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(position - new Vector2(4), 8, 8, DustID.Blood, velocity.X, velocity.Y, 100, default, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        private void SpawnDeathRing(Vector2 center, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi / count * i;
                Vector2 velocity = angle.ToRotationVector2() * speed;
                Dust dust = Dust.NewDustDirect(center, 0, 0, DustID.Blood, velocity.X, velocity.Y, 0, default, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.8f;
                if (i % 2 == 0)
                {
                    Vector2 innerVel = velocity * 0.5f;
                    Dust inner = Dust.NewDustDirect(center, 0, 0, DustID.Blood, innerVel.X, innerVel.Y, 80, default, 2f);
                    inner.noGravity = true;
                    inner.fadeIn = 1.5f;
                }
            }
        }
        private void SpawnSoulSpiral(Vector2 center, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = progress * MathHelper.TwoPi * 3f;
                float radius = 10f + progress * 40f;
                float yOffset = -progress * 80f;
                Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, yOffset + (float)Math.Sin(angle) * radius * 0.3f);
                Vector2 velocity = new Vector2((float)Math.Cos(angle + MathHelper.PiOver2) * 2f, -3f - progress * 4f);
                Dust dust = Dust.NewDustDirect(center + offset, 0, 0, DustID.Blood, velocity.X, velocity.Y, 150, new Color(160, 50, 255, 100), 1.8f - progress * 0.8f);
                dust.noGravity = true;
                dust.fadeIn = 2f;
            }
        }
        private void SpawnDeathExplosion(Vector2 center, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(12f, 12f);
                int dustType = Main.rand.NextBool(3) ? DustID.Blood : DustID.Blood;
                float scale = Main.rand.NextFloat(1.5f, 3.5f);
                Dust dust = Dust.NewDustDirect(center, 0, 0, dustType, velocity.X, velocity.Y, Main.rand.Next(50, 150), default, scale);
                dust.noGravity = true;
                dust.fadeIn = Main.rand.NextFloat(1f, 2f);
            }
            for (int i = 0; i < count / 3; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust dark = Dust.NewDustDirect(center, 0, 0, DustID.Smoke, velocity.X, velocity.Y, 200, Color.Black, 3f);
                dark.noGravity = true;
                dark.fadeIn = 2.5f;
            }
        }
        private void SpawnLightBurst(Vector2 center)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust glow = Dust.NewDustDirect(center, 0, 0, DustID.Blood, 0f, 0f, 0, default, 4f);
                glow.noGravity = true;
                glow.velocity = Main.rand.NextVector2Circular(1f, 1f);
                glow.fadeIn = 3f;
            }
            Lighting.AddLight(center, 1.5f, 0.3f, 2f);
        }
        private void SpawnDeathCross(Vector2 center, int armLength)
        {
            for (int i = -armLength; i <= armLength; i++)
            {
                float progress = Math.Abs((float)i / armLength);
                Vector2 posV = center + new Vector2(0, i * 4);
                Dust dv = Dust.NewDustDirect(posV, 0, 0, DustID.Blood, 0f, Math.Sign(i) * progress * 3f, 50, default, 2f - progress * 0.8f);
                dv.noGravity = true;
                dv.fadeIn = 1.5f;
                Vector2 posH = center + new Vector2(i * 4, -8);
                Dust dh = Dust.NewDustDirect(posH, 0, 0, DustID.Blood, Math.Sign(i) * progress * 3f, 0f, 50, default, 2f - progress * 0.8f);
                dh.noGravity = true;
                dh.fadeIn = 1.5f;
            }
        }
        private const int HandleY = 0;
        private const int HandleH = 28;
        private const int Segment1Y = 28;
        private const int Segment1H = 20;
        private const int Segment2Y = 48;
        private const int Segment2H = 20;
        private const int Segment3Y = 68;
        private const int Segment3H = 20;
        private const int TipY = 88;
        private const int TipH = 22;
        private const int FrameWidth = 14;
        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);
            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                float progress = (float)i / (list.Count - 1);
                Color baseColor = Color.Lerp(new Color(80, 20, 120), new Color(180, 60, 220), progress);
                Color color = Lighting.GetColor(element.ToTileCoordinates(), baseColor);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
                pos += diff;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);
            DrawLine(list);
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame;
                Vector2 origin;
                float scale = 1f;
                if (i == list.Count - 2)
                {
                    frame = new Rectangle(0, TipY, FrameWidth, TipH);
                    origin = new Vector2(FrameWidth / 2, TipH / 2);
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = 1.5f;
                    Lighting.AddLight(list[i], 0.6f, 0.15f, 0.8f);
                }
                else if (i == 0)
                {
                    frame = new Rectangle(0, HandleY, FrameWidth, HandleH);
                    origin = new Vector2(FrameWidth / 2, HandleH - 4);
                }
                else
                {
                    int totalMiddle = list.Count - 3;
                    int middleIndex = i - 1;
                    float segmentProgress = totalMiddle > 0
                        ? (float)middleIndex / totalMiddle
                        : 0f;
                    if (segmentProgress < 0.33f)
                    {
                        frame = new Rectangle(0, Segment1Y, FrameWidth, Segment1H);
                        origin = new Vector2(FrameWidth / 2, Segment1H / 2);
                    }
                    else if (segmentProgress < 0.66f)
                    {
                        frame = new Rectangle(0, Segment2Y, FrameWidth, Segment2H);
                        origin = new Vector2(FrameWidth / 2, Segment2H / 2);
                    }
                    else
                    {
                        frame = new Rectangle(0, Segment3Y, FrameWidth, Segment3H);
                        origin = new Vector2(FrameWidth / 2, Segment3H / 2);
                    }
                }
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
                pos += diff;
            }
            return false;
        }
	}
}
