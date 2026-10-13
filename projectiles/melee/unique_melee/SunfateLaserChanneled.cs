using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee
{
    public class SunfateLaserChanneled : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/Sunfate";

        private const int CHARGE_TIME = 60; 
        private const int BEAM_TIME = 480;  
        private const int TOTAL_LIFETIME = CHARGE_TIME + BEAM_TIME;

        private float aimAngle;
        private bool soundPlayed = false;
        private Vector2 vibrateOffset = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 48; 
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TOTAL_LIFETIME;
            Projectile.ownerHitCheck = true;
            
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(aimAngle);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            aimAngle = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                aimAngle = (Main.MouseWorld - Main.player[Projectile.owner].MountedCenter).ToRotation();
                Projectile.rotation = aimAngle;
                Projectile.netUpdate = true;
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            if (Projectile.owner == Main.myPlayer)
            {
                float targetAngle = (Main.MouseWorld - player.MountedCenter).ToRotation();
                aimAngle = MathHelper.WrapAngle(aimAngle);
                targetAngle = MathHelper.WrapAngle(targetAngle);
                float oldAngle = aimAngle;
                aimAngle = aimAngle.AngleLerp(targetAngle, 0.025f);
                if (Math.Abs(MathHelper.WrapAngle(aimAngle - oldAngle)) > 0.008f || Projectile.timeLeft % 15 == 0)
                {
                    Projectile.netUpdate = true;
                }
            }
            
            Projectile.rotation = aimAngle;
            player.ChangeDir(Math.Cos(aimAngle) >= 0 ? 1 : -1);

            
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            Vector2 armPosition = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            armPosition.Y += player.gfxOffY;
            Projectile.Center = armPosition;

            int elapsed = TOTAL_LIFETIME - Projectile.timeLeft;

            if (elapsed < CHARGE_TIME)
            {
                
                float chargeProgress = elapsed / (float)CHARGE_TIME;

                
                if (elapsed >= CHARGE_TIME - 10)
                {
                    vibrateOffset = Main.rand.NextVector2Circular(2f, 2f);
                }
                else
                {
                    vibrateOffset = Vector2.Zero;
                }

                
                Vector2 tip = Projectile.Center + Projectile.rotation.ToRotationVector2() * (60f * Projectile.scale);
                if (Main.rand.NextBool(3))
                {
                    Vector2 particlePos = tip + Main.rand.NextVector2CircularEdge(120f, 120f);
                    Vector2 particleVel = (tip - particlePos) * 0.06f;
                    Dust d = Dust.NewDustPerfect(particlePos, DustID.GemDiamond, particleVel, 100, Color.White, 1.1f);
                    d.noGravity = true;
                }
            }
            else
            {
                
                vibrateOffset = Main.rand.NextVector2Circular(1f, 1f);

                if (!soundPlayed && Main.netMode != NetmodeID.Server)
                {
                    soundPlayed = true;
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/SunfateLaser") with { Volume = 0.9f, Pitch = Main.rand.NextFloat(-0.4f, 0.4f) }, Projectile.Center);
                    jarkos.systems.SunfateScreenSystem.TriggerContrastEffect();


                    if (Projectile.owner == Main.myPlayer)
                    {
                        Player p = Main.player[Projectile.owner];
                        p.GetModPlayer<Screenshake_system>().regular_shake += 2;
                        Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(
                            Projectile.Center,
                            Projectile.rotation.ToRotationVector2(),
                            7f,
                            5f,
                            16,
                            1000f
                        ));
                    }
                }

                if (Main.netMode != NetmodeID.Server)
                {
                    Vector2 tip = Projectile.Center + Projectile.rotation.ToRotationVector2() * (60f * Projectile.scale);
                    if (Main.rand.NextBool(2))
                    {
                        float along = Main.rand.NextFloat(0f, 1500f);
                        Vector2 sparkPos = tip + Projectile.rotation.ToRotationVector2() * along;
                        Vector2 sparkVel = Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(10f, 18f);
                        Dust d = Dust.NewDustPerfect(sparkPos, DustID.Torch, sparkVel, 0, default, 1.4f);
                        d.noGravity = true;
                    }
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int elapsed = TOTAL_LIFETIME - Projectile.timeLeft;
            if (elapsed < CHARGE_TIME) 
                return false; 

            float point = 0f;
            Vector2 start = Projectile.Center + Projectile.rotation.ToRotationVector2() * (60f * Projectile.scale);
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * 1500f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 32f, ref point);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/SunfateLaserHit") with { Volume = 0.4f, Pitch = Main.rand.NextFloat(-0.4f, 0.4f) }, Projectile.Center);

            
            if (player.active && !player.dead)
            {
                int healLife = 1;
                player.statLife += healLife;
                if (player.statLife > player.statLifeMax2)
                    player.statLife = player.statLifeMax2;
                player.HealEffect(healLife);

                int healMana = 1;
                player.statMana += healMana;
                if (player.statMana > player.statManaMax2)
                    player.statMana = player.statManaMax2;
                player.ManaEffect(healMana);

                
                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<SunfateHealRay>(), 0, 0f, Projectile.owner, player.whoAmI);
                }
            }

            
            if (Main.netMode == NetmodeID.MultiplayerClient || Main.netMode == NetmodeID.Server)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player other = Main.player[i];
                    if (i != player.whoAmI && other.active && !other.dead && Vector2.Distance(other.Center, target.Center) < 800f)
                    {
                        other.statLife += 1;
                        if (other.statLife > other.statLifeMax2)
                            other.statLife = other.statLifeMax2;
                        other.HealEffect(1);

                        other.statMana += 1;
                        if (other.statMana > other.statManaMax2)
                            other.statMana = other.statManaMax2;
                        other.ManaEffect(1);

                        if (Projectile.owner == Main.myPlayer)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<SunfateHealRay>(), 0, 0f, Projectile.owner, other.whoAmI);
                        }
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D swordTex = ModContent.Request<Texture2D>(Texture).Value;

            
            Vector2 armPosition = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            armPosition.Y += player.gfxOffY;

            
            Vector2 origin;
            float rotationOffset;
            SpriteEffects effects;
            if (player.direction > 0)
            {
                origin = new Vector2(6f, swordTex.Height - 6f);
                rotationOffset = MathHelper.ToRadians(45f);
                effects = SpriteEffects.None;
            }
            else
            {
                origin = new Vector2(swordTex.Width - 6f, swordTex.Height - 6f);
                rotationOffset = MathHelper.ToRadians(135f);
                effects = SpriteEffects.FlipHorizontally;
            }

            Vector2 drawPos = armPosition - Main.screenPosition;
            Main.spriteBatch.Draw(swordTex, drawPos, null, lightColor, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0f);

            int elapsed = TOTAL_LIFETIME - Projectile.timeLeft;
            Vector2 tipPos = armPosition + Projectile.rotation.ToRotationVector2() * (60f * Projectile.scale) + vibrateOffset;

            
            Texture2D glowTex = TextureAssets.Extra[98].Value;
            float sphereScale = 0f;
            if (elapsed < CHARGE_TIME)
            {
                sphereScale = (elapsed / (float)CHARGE_TIME) * 45f;
            }
            else
            {
                sphereScale = 48f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 30f) * 4f;
            }

            if (sphereScale > 0f)
            {
                float scaleMultiplier = sphereScale / glowTex.Width;
                
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                
                
                Main.spriteBatch.Draw(glowTex, tipPos - Main.screenPosition, null, Color.Orange * 0.75f, Main.GlobalTimeWrappedHourly * 2f, glowTex.Size() * 0.5f, scaleMultiplier * 1.4f, SpriteEffects.None, 0f);
                
                Main.spriteBatch.Draw(glowTex, tipPos - Main.screenPosition, null, Color.White * 0.85f, -Main.GlobalTimeWrappedHourly * 1.5f, glowTex.Size() * 0.5f, scaleMultiplier * 0.7f, SpriteEffects.None, 0f);
                
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            
            if (elapsed >= CHARGE_TIME)
            {
                Effect effect = HealShaders.Stream;
                if (effect != null && HealShaders.HealNoise != null)
                {
                    Main.spriteBatch.End();

                    
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                    effect.Parameters["uOpacity"]?.SetValue(1.0f);
                    effect.Parameters["uColor"]?.SetValue(new Color(255, 120, 0).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(255, 230, 180).ToVector4());

                    Vector2 start = armPosition + Projectile.rotation.ToRotationVector2() * (60f * Projectile.scale);
                    Vector2 end = start + Projectile.rotation.ToRotationVector2() * 1500f;

                    
                    List<Vector2> points = new List<Vector2>();
                    for (int i = 0; i <= 20; i++)
                    {
                        points.Add(Vector2.Lerp(start, end, i / 20f));
                    }

                    
                    TrailRenderer.Draw(effect, points.ToArray(), t => 32f, t => Color.White, HealShaders.HealNoise);

                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            return false;
        }
    }
}
