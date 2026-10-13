using System;
using System.Collections.Generic;
using jarkos.items.weapons.melee.true_melee;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	public class Saw_blade : ModProjectile
    {
        private const float MIN_ROTATION_SPEED = 0.02f;
        private const float MAX_ROTATION_SPEED = 0.7f;

        private const float MIN_DAMAGE_MULT = 0.1f;
        private const float MAX_DAMAGE_MULT = 1f;

        private ref float HandleIndex => ref Projectile.ai[0];
        private ref float ChargeProgress => ref Projectile.ai[1];
        private ref float BladeRotation => ref Projectile.localAI[0];

        private float currentRotationSpeed = MIN_ROTATION_SPEED;

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 999999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.scale = 1.45f;
            Projectile.alpha = 0;
            Projectile.hide = true;
            Projectile.netImportant = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override void AI()
        {
            int handleIdx = (int)HandleIndex;
            if (handleIdx < 0 || handleIdx >= Main.maxProjectiles
                || !Main.projectile[handleIdx].active
                || Main.projectile[handleIdx].type != ModContent.ProjectileType<Circular_saw_handle>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity = Vector2.Zero;
            float targetSpeed = MathHelper.Lerp(MIN_ROTATION_SPEED, MAX_ROTATION_SPEED, ChargeProgress);
            currentRotationSpeed = MathHelper.Lerp(currentRotationSpeed, targetSpeed, 0.05f);
            BladeRotation += currentRotationSpeed;

            if (Main.netMode != NetmodeID.Server)
            {
                SpawnDust();
            }

            if (ChargeProgress > 0.5f)
            {
                float intensity = (ChargeProgress - 0.5f) * 0.6f;
                Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.2f, 0.1f) * intensity);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float damageMult = MathHelper.Lerp(MIN_DAMAGE_MULT, MAX_DAMAGE_MULT, ChargeProgress);
            modifiers.FinalDamage *= damageMult;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                int dustCount = (int)(2 + ChargeProgress * 4);
                for (int i = 0; i < dustCount; i++)
                {
                    float dustAngle = BladeRotation + Main.rand.NextFloat(-0.5f, 0.5f);
                    Vector2 dustVel = new Vector2(MathF.Cos(dustAngle), MathF.Sin(dustAngle))
                        * Main.rand.NextFloat(2f, 5f * (1 + ChargeProgress));
                    Dust d = Dust.NewDustDirect(target.Center, 1, 1, DustID.Blood);
                    d.velocity = dustVel;
                    d.scale = Main.rand.NextFloat(0.8f, 1.4f);
                    d.noGravity = false;
                    new Spark(target.Center, dustVel, Color.Red, 200, 0.7f, 1.25f, 0f).Spawn();
                }
                if (Main.rand.NextBool(3))
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1 with
                    {
                        Volume = 0.3f, Pitch = 0.2f + ChargeProgress * 0.3f
                    }, target.Center);
                }
            }
        }

        private void SpawnDust()
        {
            if (ChargeProgress < 0.1f) return;
            if (Main.rand.NextBool(6))
            {
                float particleAngle = BladeRotation + MathHelper.PiOver2;
                Vector2 spawnPos = Projectile.Center + new Vector2(MathF.Cos(BladeRotation), MathF.Sin(BladeRotation)) * 20f;
                Vector2 particleVel = new Vector2(MathF.Cos(particleAngle), MathF.Sin(particleAngle)) * currentRotationSpeed * 15f;
                int dustType = Main.rand.NextBool(3) ? DustID.Blood : DustID.Iron;
                Dust d = Dust.NewDustDirect(spawnPos, 1, 1, dustType);
                d.velocity = particleVel + Main.rand.NextVector2Circular(0.5f, 0.5f);
                d.scale = Main.rand.NextFloat(0.4f, 0.8f);
                d.noGravity = dustType == DustID.Iron;
            }
            if (ChargeProgress > 0.7f && Main.rand.NextBool(10))
            {
                float sparkAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + new Vector2(MathF.Cos(sparkAngle), MathF.Sin(sparkAngle)) * 22f;
                Dust Spark = Dust.NewDustDirect(sparkPos, 1, 1, DustID.Torch);
                Spark.velocity = new Vector2(MathF.Cos(sparkAngle), MathF.Sin(sparkAngle)) * Main.rand.NextFloat(1f, 3f);
                Spark.scale = Main.rand.NextFloat(0.3f, 0.6f);
                Spark.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            Color bladeColor = Color.Lerp(lightColor, new Color(255, 180, 160), ChargeProgress * 0.3f);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, bladeColor, BladeRotation, origin, Projectile.scale, SpriteEffects.None, 0);

            if (ChargeProgress > 0.3f)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                float glowAlpha = (ChargeProgress - 0.3f) / 0.7f;
                Color glowColor = new Color(200, 60, 30, 0) * glowAlpha * 0.3f;
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, glowColor, BladeRotation, origin, Projectile.scale * 1.08f, SpriteEffects.None, 0);

                if (ChargeProgress > 0.6f)
                {
                    float blurAlpha = (ChargeProgress - 0.6f) / 0.4f * 0.15f;
                    for (int i = 0; i < 3; i++)
                    {
                        float blurAngle = BladeRotation + (i + 1) * currentRotationSpeed * 3f;
                        Color blurColor = new Color(180, 50, 30, 0) * blurAlpha;
                        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, blurColor, blurAngle, origin, Projectile.scale * 1.02f, SpriteEffects.None, 0);
                    }
                }
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }

        public override bool ShouldUpdatePosition() => false;
        public override bool? CanCutTiles() => true;
    }
}
