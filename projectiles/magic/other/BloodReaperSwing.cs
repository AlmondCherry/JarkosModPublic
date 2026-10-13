using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using jarkos.systems;
using Terraria.GameContent;
using jarkos.particles;

namespace jarkos.projectiles.magic.other
{
    public class BloodReaperSwing : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/magic/other/BloodReaper";
        public int swingTime = 25;
        public const float BLADE_LEN = 135f;
        public static readonly Color ColorBlood = new Color(200, 30, 30);
        public static readonly Color ColorCore = new Color(255, 150, 150);
        private int ComboStep => (int)Projectile.ai[0];
        private int angDir = 1;
        private float aimAngle;
        private int timer;
        private List<Vector2> trailPositions;
        private List<float> trailRotations;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 25;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[0] > 0)
            {
                swingTime = (int)Projectile.ai[0];
            }
            else if (player.active && player.HeldItem != null && player.HeldItem.type == ModContent.ItemType<jarkos.items.weapons.magic.other.BloodReaper>())
            {
                swingTime = player.HeldItem.useAnimation;
            }
            else
            {
                swingTime = 25;
            }
            Projectile.timeLeft = swingTime;

            trailPositions = new List<Vector2>();
            trailRotations = new List<float>();

            aimAngle = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
            angDir = (int)Projectile.ai[1];

            if (angDir == 0) 
            angDir = 1;
            if (Math.Cos(aimAngle) < 0)
                angDir = -angDir;

            Projectile.rotation = CurrentRotation();
        }
        private float CurrentRotation()
        {
            float t = MathHelper.Clamp(timer / (float)swingTime, 0f, 1f);
            float ease = 1f - MathF.Pow(1f - t, 3.5f);
            float startAng = aimAngle - angDir * MathHelper.ToRadians(110f);
            float endAng = aimAngle + angDir * MathHelper.ToRadians(130f);

            return MathHelper.Lerp(startAng, endAng, ease);
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            timer++;
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            Vector2 playerCenter = player.MountedCenter;
            Projectile.Center = playerCenter;
            Projectile.rotation = CurrentRotation();

            Vector2 bladeTip = Projectile.Center + Projectile.rotation.ToRotationVector2() * BLADE_LEN;

            trailPositions.Insert(0, bladeTip);
            trailRotations.Insert(0, Projectile.rotation);

            if (trailPositions.Count > 20)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
                trailRotations.RemoveAt(trailRotations.Count - 1);
            }
            if (timer == swingTime / 2 && Projectile.owner == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.4f, Volume = 0.6f }, Projectile.Center);

                if (player.statLife > 15)
                {
                    player.statLife -= 5;
                    CombatText.NewText(player.Hitbox, new Color(200, 30, 30), "-5", false, true);
                }

                int projType = ModContent.ProjectileType<BloodPhantomScytheProj>();
                int count = Main.rand.NextBool(3) ? 2 : 1;

                for (int i = 0; i < count; i++)
                {
                    Vector2 perturbedSpeed = Projectile.rotation.ToRotationVector2().RotatedByRandom(MathHelper.ToRadians(20)) * Main.rand.NextFloat(8f, 12f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), bladeTip, perturbedSpeed, projType, Projectile.damage, Projectile.knockBack, player.whoAmI);
                }
            }

            if (Main.rand.NextBool(2))
            {
                float along = Main.rand.NextFloat(0.3f, 1.0f);
                Vector2 pos = Projectile.Center + Projectile.rotation.ToRotationVector2() * BLADE_LEN * along;
                Dust.NewDustPerfect(pos, DustID.Blood, Main.rand.NextVector2Circular(1f, 1f), 100, default, 1.4f).noGravity = true;
            }

            if (Main.rand.NextBool(4))
            {
                float along = Main.rand.NextFloat(0.5f, 1.0f);
                Vector2 pos = Projectile.Center + Projectile.rotation.ToRotationVector2() * BLADE_LEN * along;
                new Spark_gravit(pos, Main.rand.NextVector2Circular(2f, 2f), Color.Red, 30, Main.rand.NextFloat(0.3f, 0.6f), 1.2f, 0f).Spawn();
            }
            Projectile.position = playerCenter - Projectile.Size / 2f;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Projectile.owner];
            float collisionPoint = 0f;
            Vector2 start = player.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * BLADE_LEN;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30f, ref collisionPoint);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath21 with { Pitch = 0.2f, Volume = 0.6f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.7f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item21 with { Pitch = Main.rand.NextFloat(0.1f, 0.5f), Volume = 0.5f }, target.Center);

            for (int i = 0; i < 18; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, default, Main.rand.NextFloat(1.2f, 2.0f));
                d.velocity *= 1.2f;
            }
            
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<BloodReaperHitVisual>(), 0, 0, Projectile.owner);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (trailRotations != null && trailRotations.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                List<Vector2> currentTrailPos = new List<Vector2>();
                foreach (float rot in trailRotations)
                {
                    currentTrailPos.Add(Projectile.Center + rot.ToRotationVector2() * BLADE_LEN);
                }
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                try
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(ColorBlood.ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(ColorCore.ToVector4());
                    TrailRenderer.Draw(effect, currentTrailPos, t => MathHelper.Lerp(80f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                finally
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(10f, tex.Height - 10f);
            SpriteEffects effects = SpriteEffects.None;
            float drawRot = Projectile.rotation + MathHelper.PiOver4;
            if (angDir == -1)
            {
                effects = SpriteEffects.FlipHorizontally;
                origin.X = tex.Width - origin.X;
                drawRot += MathHelper.PiOver2;
            }
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, drawRot, origin, Projectile.scale * 1.2f, effects, 0f);
            return false;
        }
    }

    public class BloodReaperHitVisual : ModProjectile
    {
        public override string Texture => "jarkos/particles/Spark";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 2;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            if (Projectile.timeLeft == 2 && !Main.dedServ)
            {
                Vector2 hitPos = Projectile.Center;
                for (int i = 0; i < 8; i++)
                {
                    Vector2 v = Main.rand.NextVector2Circular(8f, 8f);
                    new jarkos.particles.Spark(hitPos, v, BloodReaperSwing.ColorCore, 24, Main.rand.NextFloat(0.5f, 0.9f), 0.9f).Spawn();
                    new jarkos.particles.Spark(hitPos, v * 0.5f, BloodReaperSwing.ColorBlood, 30, Main.rand.NextFloat(0.8f, 1.2f), 1f).Spawn();
                }
                for (int i = 0; i < 14; i++)
                {
                    Vector2 v = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-7f, -1.5f));
                    float size = Main.rand.NextFloat(0.5f, 0.9f);
                    new jarkos.particles.Spark_gravit(hitPos, v, Color.Red, 45, size, 1.5f, 0f).Spawn();
                }
            }
        }
    }
}
