using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using jarkos.systems;
using jarkos.particles;
namespace jarkos.projectiles.mecha
{
    public class Magellonova_proj : ModProjectile
    {
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 30;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.light = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 0.9f;
            Lighting.AddLight(Projectile.Center, 1.3f * pulse, 1.1f * pulse, 0.2f * pulse);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), DustID.YellowTorch, Projectile.velocity * -0.2f, 0, default, 1.5f);
                dust.noGravity = true;
            }
            if (Projectile.timeLeft % 8 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.3f, Pitch = 0.6f }, Projectile.Center);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.1f, Volume = 1.1f }, Projectile.Center);
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10, 10);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, vel, 0, default, 2.2f);
                dust.noGravity = true;
            }
            Color sparkColor = new Color(255, 220, 20);
            for (int i = 0; i < 12; i++)
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(8f, 8f), sparkColor, 20, Main.rand.NextFloat(0.8f, 1.5f), 1f).Spawn();
            }
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                    float speed = Main.rand.NextFloat(4.5f, 7.5f);
                    Vector2 portalVel = angle.ToRotationVector2() * speed;
                    Projectile.NewProjectile(Projectile.GetSource_Death(), target.Center, portalVel, ModContent.ProjectileType<MagelloPortal>(), (int)(Projectile.damage * 0.45f), Projectile.knockBack * 0.3f, Projectile.owner, angle);
                }
            }
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 1.4f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.4f, Volume = 1.1f }, Projectile.Center);
            Color sparkColor = new Color(255, 230, 40);
            for (int i = 0; i < 20; i++)
            {
                new Spark(target.Center, Main.rand.NextVector2Circular(7f, 7f), sparkColor, 22, Main.rand.NextFloat(0.9f, 1.5f), 1f).Spawn();
            }
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(9, 9);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, vel, 0, default, 2.2f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = new Color(255, 200, 20);
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f + Projectile.identity * 0.15f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 230).ToVector4());
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < trailPositions.Count; i++)
                {
                    if (trailPositions[i] == Vector2.Zero) continue;
                    points.Add(trailPositions[i]);
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => 12f * (1f - t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                if (trailPositions.Count >= 2)
                {
                    for (int i = 0; i < trailPositions.Count - 1; i++)
                    {
                        float progress = (float)i / trailPositions.Count;
                        float alpha = (1f - progress) * 0.8f;
                        float width = (1f - progress) * 20f + 2f;
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[Math.Min(i + 1, trailPositions.Count - 1)] - Main.screenPosition;
                        float rotation = (end - start).ToRotation();
                        float length = Vector2.Distance(start, end);
                        if (length < 1) continue;
                        Color glowColor = baseColor * alpha * 0.3f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), glowColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2.5f), SpriteEffects.None, 0f);
                        Color coreColor = Color.White * alpha * 0.8f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.6f), SpriteEffects.None, 0f);
                    }
                }
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.15f + 1.3f;
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.95f, Projectile.rotation, origin, pulse, SpriteEffects.None, 0f);
            return false;
        }
    }
    #region Magello Portal (Stationary Emitting Portal)
    public class MagelloPortal : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private int fireTimer = 0;
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            if (Projectile.velocity.Length() < 0.05f)
            {
                Projectile.velocity = Vector2.Zero;
            }
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.3f + 1.2f;
            Lighting.AddLight(Projectile.Center, 1.4f * pulse, 1.2f * pulse, 0.1f * pulse);
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(20f, 20f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.YellowTorch, -offset.RotatedBy(0.3f) * 0.08f, 0, default, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
            }
            fireTimer++;
            if (fireTimer >= 6)
            {
                fireTimer = 0;
                if (Projectile.owner == Main.myPlayer)
                {
                    NPC closestNPC = null;
                    float closestDist = 800f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            float dist = Vector2.Distance(Projectile.Center, npc.Center);
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closestNPC = npc;
                            }
                        }
                    }
                    Vector2 shootVel;
                    if (closestNPC != null)
                    {
                        shootVel = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 16f;
                    }
                    else
                    {
                        float angle = Projectile.ai[0];
                        Projectile.ai[0] += 0.25f;
                        shootVel = angle.ToRotationVector2() * 16f;
                    }
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/MagellonovaMiniShot") with { Volume = 0.35f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.position);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVel, ModContent.ProjectileType<MagelloSubProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.3f + 1.2f;
            float rotation = Main.GameUpdateCount * 0.08f;
            spriteBatch.Draw(texture, drawPos, null, new Color(255, 200, 30) * 0.8f, rotation, origin, pulse * 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.95f, -rotation * 1.5f, origin, pulse * 0.3f, SpriteEffects.None, 0f);
            return false;
        }
        public override bool? CanDamage() => false;
    }
    #endregion
    #region Magello Sub-Projectile (Fast Piercing Golden Laser Shot)
    public class MagelloSubProj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 15;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.8f, 0.6f, 0.1f);
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, Projectile.velocity * -0.1f, 0, default, 1.0f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.8f, Volume = 0.35f }, Projectile.Center);
            Color sparkColor = new Color(255, 220, 20);
            for (int i = 0; i < 3; i++)
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), sparkColor, 12, Main.rand.NextFloat(0.5f, 0.9f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = new Color(255, 200, 20);
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 7f + Projectile.identity * 0.12f);
                effect.Parameters["uOpacity"]?.SetValue(1.2f);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 220).ToVector4());
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < trailPositions.Count; i++)
                {
                    if (trailPositions[i] == Vector2.Zero) continue;
                    points.Add(trailPositions[i]);
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => 9f * (1f - t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                if (trailPositions.Count >= 2)
                {
                    for (int i = 0; i < trailPositions.Count - 1; i++)
                    {
                        float progress = (float)i / trailPositions.Count;
                        float alpha = (1f - progress) * 0.8f;
                        float width = (1f - progress) * 8f + 1f;
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[Math.Min(i + 1, trailPositions.Count - 1)] - Main.screenPosition;
                        float rotation = (end - start).ToRotation();
                        float length = Vector2.Distance(start, end);
                        if (length < 1) continue;
                        Color glowColor = baseColor * alpha * 0.4f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), glowColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 3f), SpriteEffects.None, 0f);
                        Color coreColor = new Color(255, 255, 200) * alpha;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                    }
                }
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.5f) * 0.2f + 0.8f;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), baseColor * 0.5f * pulse, 0f, new Vector2(0.5f, 0.5f), 12f, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White * 0.9f, 0f, new Vector2(0.5f, 0.5f), 4f, SpriteEffects.None, 0f);
            return false;
        }
    }
    #endregion
}
