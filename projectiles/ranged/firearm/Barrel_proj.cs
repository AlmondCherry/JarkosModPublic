using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.CameraModifiers;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace jarkos.projectiles.ranged.firearm
{
    public class Barrel_proj : ModProjectile
    {
        private const int MaxChargeTime = 120;
        private const int MaxPenetrate = -1;
        private const float MinSpeed = 8f;
        private const float MaxSpeed = 28f;
        private const float Gravity = 0.35f;
        private const float GroundFriction = 0.985f;
        private const int ExplosionRadius = 180;
        private const int ExplosionDamageMultiplier = 3;
        private const float SelfDamageMultiplier = 0.5f;
        public bool IsCharging
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }
        public int ChargeTime
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public int BounceCount
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public int MaxBounces { get; set; } = 3;
        public bool IsOnGround
        {
            get => Projectile.localAI[1] == 1f;
            set => Projectile.localAI[1] = value ? 1f : 0f;
        }
        private bool hasExploded = false;
        private int blinkTimer = 0;
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxPenetrate;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = -1;
            Projectile.light = 0.3f;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (IsCharging)
            {
                ChargePhase(owner);
            }
            else
            {
                RollingPhase(owner);
            }
        }
        private void ChargePhase(Player owner)
        {
            if (!owner.channel || owner.dead || !owner.active)
            {
                LaunchBomb(owner);
                return;
            }
            ChargeTime++;
            if (ChargeTime > MaxChargeTime)
                ChargeTime = MaxChargeTime;
            Projectile.Center = owner.Center + new Vector2(owner.direction * 25, -5);
            Projectile.velocity = Vector2.Zero;
            float chargeProgress = (float)ChargeTime / MaxChargeTime;
            Projectile.rotation += 0.15f + chargeProgress * 0.5f;
            Projectile.scale = 1f + chargeProgress * 0.3f;
            if (ChargeTime % 3 == 0)
            {
                Color dustColor = Color.Lerp(Color.Gray, Color.OrangeRed, chargeProgress);
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, -2f, 100, dustColor, 1f + chargeProgress * 1.5f);
                dust.noGravity = true;
                dust.velocity *= chargeProgress;
            }
            if (chargeProgress > 0.8f && Main.rand.NextBool(2))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), 0, 0, DustID.Electric, 0f, 0f, 0, default, 0.8f);
                Spark.noGravity = true;
            }
            if (ChargeTime == MaxChargeTime)
            {
                SoundEngine.PlaySound(SoundID.MaxMana, Projectile.position);
                for (int i = 0; i < 20; i++)
                {
                    Dust flash = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, default, 1.5f);
                    flash.noGravity = true;
                }
            }
            owner.ChangeDir(Main.MouseWorld.X > owner.Center.X ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
        }
        private void LaunchBomb(Player owner)
        {
            IsCharging = false;
            Projectile.tileCollide = true;
            Projectile.scale = 1f;
            float chargeProgress = (float)ChargeTime / MaxChargeTime;
            float speed = MathHelper.Lerp(MinSpeed, MaxSpeed, chargeProgress);
            Vector2 direction = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
            direction.Y -= 0.15f;
            direction = direction.SafeNormalize(Vector2.UnitX);
            Projectile.velocity = direction * speed;
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.5f }, Projectile.position);
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 0.4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, dustVel.X, dustVel.Y, 100, default, Main.rand.NextFloat(1f, 2f));
                dust.noGravity = Main.rand.NextBool();
            }
            for (int i = 0; i < 15; i++)
            {
                Dust fire = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, -Projectile.velocity.X * 0.3f + Main.rand.NextFloat(-2f, 2f), -Projectile.velocity.Y * 0.3f + Main.rand.NextFloat(-2f, 2f), 0, default, 1.5f);
                fire.noGravity = true;
            }
        }
        private void RollingPhase(Player owner)
        {
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > 18f)
                Projectile.velocity.Y = 18f;
            CheckGroundContact();
            if (IsOnGround)
            {
                Projectile.velocity.X *= GroundFriction;
                if (Math.Abs(Projectile.velocity.X) > 2f && Main.rand.NextBool(2))
                {
                    Dust.NewDust(new Vector2(Projectile.position.X, Projectile.Bottom.Y - 4), Projectile.width, 4, DustID.Smoke, Projectile.velocity.X * 0.1f, -1f, 100, default, 0.8f);
                }
            }
            Projectile.rotation += Projectile.velocity.X * 0.06f;
            int bouncesRemaining = MaxBounces - BounceCount;
            if (bouncesRemaining <= 1 && !IsCharging)
            {
                blinkTimer++;
                if (blinkTimer % 5 == 0)
                {
                    Dust danger = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f), 0, Color.Red, 1.2f);
                    danger.noGravity = true;
                }
            }
            if (Projectile.velocity.Length() > 5f && Main.rand.NextBool(3))
            {
                Dust trail = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.5f, 0, 0, DustID.Torch, 0f, 0f, 100, default, 0.8f);
                trail.noGravity = true;
                trail.velocity *= 0.3f;
            }
            if (Math.Abs(Projectile.velocity.X) < 0.3f && IsOnGround && Projectile.timeLeft < 880)
            {
                Explode();
            }
        }
        private void CheckGroundContact()
        {
            int tileX = (int)(Projectile.Center.X / 16);
            int tileY = (int)((Projectile.Bottom.Y + 4) / 16);
            if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY)
            {
                Tile tile = Main.tile[tileX, tileY];
                IsOnGround = tile.HasTile && Main.tileSolid[tile.TileType];
            }
            else
            {
                IsOnGround = false;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bool hitWall = Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0.1f;
            bool hitFloorOrCeiling = Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0.1f;
            if (hitWall)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.75f;
                BounceCount++;
                SoundEngine.PlaySound(SoundID.Dig with { Pitch = -0.2f }, Projectile.position);
                for (int i = 0; i < 12; i++)
                {
                    Dust Spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Iron, -oldVelocity.X * 0.3f + Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 1f), 0, default, 1f);
                    Spark.noGravity = Main.rand.NextBool();
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    PunchCameraModifier shake = new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), 3f, 6f, 10, 1000f);
                    Main.instance.CameraModifiers.Add(shake);
                }
                if (BounceCount >= MaxBounces)
                {
                    Explode();
                    return false;
                }
            }
            if (hitFloorOrCeiling)
            {
                if (oldVelocity.Y > 0)
                {
                    Projectile.velocity.Y = -oldVelocity.Y * 0.4f;
                    if (Math.Abs(Projectile.velocity.Y) < 2.5f)
                    {
                        Projectile.velocity.Y = 0;
                    }
                    if (oldVelocity.Y > 5f)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Dust.NewDust(new Vector2(Projectile.position.X, Projectile.Bottom.Y - 4), Projectile.width, 4, DustID.Smoke, Main.rand.NextFloat(-2f, 2f), -2f, 100, default, 1f);
                        }
                        SoundEngine.PlaySound(SoundID.Dig with { Pitch = 0.3f, Volume = 0.5f }, Projectile.position);
                    }
                }
                else
                {
                    Projectile.velocity.Y = -oldVelocity.Y * 0.25f;
                }
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust blood = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 0, default, 1.2f);
                blood.noGravity = Main.rand.NextBool();
            }
            Projectile.velocity *= 0.92f;
            if (Main.netMode != NetmodeID.Server)
            {
                PunchCameraModifier shake = new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), 2f, 4f, 5, 500f);
                Main.instance.CameraModifiers.Add(shake);
            }
        }
        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.position);
            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, Projectile.position);
            if (Main.netMode != NetmodeID.Server)
            {
                PunchCameraModifier megaShake = new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f), 25f, 16f, 30, 3000f);
                Main.instance.CameraModifiers.Add(megaShake);
            }
            for (int ring = 0; ring < 3; ring++)
            {
                int particleCount = 20 + ring * 10;
                float ringSpeed = 8f + ring * 4f;
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount;
                    Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringSpeed;
                    Dust fire = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, dustVel.X, dustVel.Y, 0, default, 2.5f - ring * 0.5f);
                    fire.noGravity = true;
                    fire.fadeIn = 1.5f;
                }
            }
            for (int i = 0; i < 80; i++)
            {
                Vector2 randomVel = Main.rand.NextVector2Circular(15f, 15f);
                Dust explosion = Dust.NewDustDirect(Projectile.Center, 0, 0, Main.rand.Next(new[] { DustID.Torch, DustID.Smoke, DustID.SolarFlare }), randomVel.X, randomVel.Y, 100, default, Main.rand.NextFloat(1.5f, 3f));
                explosion.noGravity = Main.rand.NextBool(3);
                explosion.fadeIn = 1f;
            }
            for (int i = 0; i < 40; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(8f, 8f);
                smokeVel.Y -= 3f;
                Dust smoke = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(30, 30), 0, 0, DustID.Smoke, smokeVel.X, smokeVel.Y, 150, new Color(50, 50, 50), Main.rand.NextFloat(2f, 4f));
                smoke.noGravity = false;
            }
            for (int i = 0; i < 25; i++)
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, Main.rand.NextFloat(-12f, 12f), Main.rand.NextFloat(-12f, 12f), 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                Spark.noGravity = true;
            }
            for (int g = 0; g < 10; g++)
            {
                Vector2 goreVel = Main.rand.NextVector2Circular(8f, 8f);
                goreVel.Y -= 4f;
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, goreVel, GoreID.Smoke1 + Main.rand.Next(3));
            }
            for (int g = 0; g < 5; g++)
            {
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(6f, 6f), GoreID.ChimneySmoke1 + Main.rand.Next(3));
            }
            for (int i = 0; i < 15; i++)
            {
                Dust flash = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, Color.White, 3f);
                flash.noGravity = true;
                flash.fadeIn = 2f;
            }
            ExplosionDamage();
            Projectile.Kill();
        }
        private void ExplosionDamage()
        {
            int explosionDamage = Projectile.damage * ExplosionDamageMultiplier;
            Player owner = Main.player[Projectile.owner];
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < ExplosionRadius)
                {
                    float damageMultiplier = 1f - distance / ExplosionRadius * 0.5f;
                    int finalDamage = (int)(explosionDamage * damageMultiplier);
                    Vector2 hitDirection = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    float explosionKnockback = Projectile.knockBack * 2f * damageMultiplier;
                    npc.SimpleStrikeNPC(finalDamage, hitDirection.X > 0 ? 1 : -1, true, explosionKnockback, Projectile.DamageType);
                    npc.AddBuff(BuffID.OnFire, 300);
                }
            }
            if (owner.active && !owner.dead)
            {
                float distanceToOwner = Vector2.Distance(Projectile.Center, owner.Center);
                if (distanceToOwner < ExplosionRadius)
                {
                    float damageMultiplier = 1f - distanceToOwner / ExplosionRadius * 0.5f;
                    int selfDamage = (int)(explosionDamage * damageMultiplier * SelfDamageMultiplier);
                    if (selfDamage < 10) selfDamage = 10;
                    Vector2 knockbackDir = (owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    owner.Hurt(PlayerDeathReason.ByCustomReason($"{owner.name} was blown up by their own Rolling Bomb"), selfDamage, knockbackDir.X > 0 ? 1 : -1, false, false, -1, false, Projectile.knockBack * 1.5f * damageMultiplier);
                    owner.velocity += knockbackDir * 12f * damageMultiplier;
                    owner.AddBuff(BuffID.OnFire, 180);
                }
            }
            if (owner.hostile)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active || player.dead || player.whoAmI == Projectile.owner)
                        continue;
                    if (!player.hostile)
                        continue;
                    float distance = Vector2.Distance(Projectile.Center, player.Center);
                    if (distance < ExplosionRadius)
                    {
                        float damageMultiplier = 1f - distance / ExplosionRadius * 0.5f;
                        int pvpDamage = (int)(explosionDamage * damageMultiplier * 0.7f);
                        Vector2 knockbackDir = (player.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        player.Hurt(PlayerDeathReason.ByCustomReason($"{player.name} was obliterated by {owner.name}'s Rolling Bomb"), pvpDamage, knockbackDir.X > 0 ? 1 : -1, true, false, -1, false, Projectile.knockBack * damageMultiplier);
                        player.velocity += knockbackDir * 10f * damageMultiplier;
                        player.AddBuff(BuffID.OnFire, 180);
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color drawColor = lightColor;
            float scale = Projectile.scale;
            if (IsCharging)
            {
                float chargeProgress = (float)ChargeTime / MaxChargeTime;
                drawColor = Color.Lerp(lightColor, Color.OrangeRed, chargeProgress * 0.6f);
                scale += (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.05f * chargeProgress;
            }
            int bouncesRemaining = MaxBounces - BounceCount;
            if (bouncesRemaining <= 1 && !IsCharging)
            {
                if (blinkTimer / 3 % 2 == 0)
                {
                    drawColor = Color.Lerp(drawColor, Color.Red, 0.7f);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2, 0).RotatedBy(MathHelper.PiOver2 * i);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, drawColor * 0.3f, Projectile.rotation, origin, scale * 1.1f, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
