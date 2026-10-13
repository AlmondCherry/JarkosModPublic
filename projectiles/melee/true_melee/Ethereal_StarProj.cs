using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class Ethereal_StarProj : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/Ethereal_rift"; 
        
        public ref float TargetNPC => ref Projectile.ai[0];
        public ref float StarIndex => ref Projectile.ai[1];
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
        }

        private void FindTarget()
        {
            if (TargetNPC != -1f) return;
            
            List<int> possibleTargets = new List<int>();
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < 1000f)
                    {
                        possibleTargets.Add(i);
                    }
                }
            }
            
            if (possibleTargets.Count > 0)
            {
                
                int index = (int)StarIndex % possibleTargets.Count;
                TargetNPC = possibleTargets[index];
            }
            else
            {
                TargetNPC = -1f;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            FindTarget();
            if (TargetNPC != -1f)
            {
                NPC target = Main.npc[(int)TargetNPC];
                Vector2 dirToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                
                float curveAngle = (StarIndex % 2 == 0) ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                
                curveAngle += Main.rand.NextFloat(-0.2f, 0.2f);
                
                Projectile.velocity = dirToTarget.RotatedBy(curveAngle) * Main.rand.NextFloat(4f, 8f);
            }
            else
            {
                Vector2 dirToMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitY);
                float curveAngle = (StarIndex % 2 == 0) ? MathHelper.PiOver4 : -MathHelper.PiOver4;
                Projectile.velocity = dirToMouse.RotatedBy(curveAngle) * Main.rand.NextFloat(4f, 8f);
            }
        }

        public override void AI()
        {
            if (TargetNPC == -1f) FindTarget();
            
            if (TargetNPC == -1f || !Main.npc[(int)TargetNPC].active || !Main.npc[(int)TargetNPC].CanBeChasedBy())
            {
                
                Projectile.velocity *= 1.02f;
            }
            else
            {
                NPC target = Main.npc[(int)TargetNPC];
                
                Vector2 dirToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = 12f; 
                Vector2 targetVelocity = dirToTarget * speed;
                
                
                float turnSpeed = 0.05f + (Projectile.timeLeft < 250 ? 0.05f : 0f); 
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, turnSpeed);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, -Projectile.velocity * 0.3f, 0, Ethereal_rift_dante_swing.ColorCyan, 0.8f);
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (Projectile.ai[2] == 1f)
                {
                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = -0.2f }, Projectile.Center);
                    new Circleedge(Projectile.Center, Vector2.Zero, Ethereal_rift_dante_swing.ColorCore, 30, 0.4f, 2f).Spawn();
                    new Circleedge(Projectile.Center, Vector2.Zero, Ethereal_rift_dante_swing.ColorCyan, 25, 0.5f, 1.5f).Spawn();
                    for (int i = 0; i < 20; i++)
                    {
                        new Spark(Projectile.Center, Main.rand.NextVector2Circular(15f, 15f), Ethereal_rift_dante_swing.ColorCyan, 40, Main.rand.NextFloat(1f, 2f), 2f).Spawn();
                        Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(10, 10), 20, 20, DustID.BlueFairy, 0, 0, 100, default, 2f);
                        d.velocity = Main.rand.NextVector2Circular(10f, 10f);
                        d.noGravity = true;
                    }
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
                    new Circleedge(Projectile.Center, Vector2.Zero, Ethereal_rift_dante_swing.ColorCore, 12, 0.15f, 0.8f).Spawn();
                    for (int i = 0; i < 5; i++)
                    {
                        new Spark(Projectile.Center, Main.rand.NextVector2Circular(6f, 6f), Ethereal_rift_dante_swing.ColorCyan, 20, Main.rand.NextFloat(0.5f, 1f), 1f).Spawn();
                    }
                }
            }
            if (Projectile.ai[2] == 1f)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (n.active && !n.friendly && !n.dontTakeDamage && n.whoAmI != target.whoAmI && n.Distance(Projectile.Center) < 250f)
                    {
                        n.SimpleStrikeNPC(Projectile.damage / 2, 0, false, 0, Projectile.DamageType, true, Projectile.owner);
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D starTex = ModContent.Request<Texture2D>("jarkos/particles/StarParticle").Value;
            float baseSize = 0.7f + (float)Math.Sin(Projectile.whoAmI * 123.45f) * 0.4f;
            
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Color trailColor = Color.White * progress * 0.5f;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Main.spriteBatch.Draw(starTex, drawPos, null, trailColor, Projectile.rotation + i * 0.1f, starTex.Size() / 2f, progress * 0.24f * baseSize, SpriteEffects.None, 0f);
            }
            
            
            Color starColor = Color.White;
            starColor.A = 0;
            
            float rot = Main.GlobalTimeWrappedHourly * 1f;
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f + Projectile.whoAmI * 2f) * 0.1f * baseSize;
            float scale = 0.32f * baseSize + pulse;
            
            Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, starColor, rot, starTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, starColor, rot, starTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, Ethereal_rift_dante_swing.ColorCyan * 0.8f, -rot, starTex.Size() / 2f, scale * 1.5f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
}
