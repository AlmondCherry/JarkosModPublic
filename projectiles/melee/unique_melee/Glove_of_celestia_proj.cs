using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using jarkos.systems;
using System.Collections.Generic;
namespace jarkos.projectiles.melee.unique_melee
{
    public class Glove_of_celestia_held : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/other_melee/GloveOfCelestia";
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            Projectile.timeLeft = 2;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
            if (Main.myPlayer != Projectile.owner)
            {
                aimDir = Projectile.velocity;
            }
            else
            {
                Projectile.velocity = aimDir;
            }
            player.ChangeDir(aimDir.X >= 0f ? 1 : -1);
            Projectile.rotation = aimDir.ToRotation();
            Projectile.Center = player.MountedCenter + aimDir * 24f;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, aimDir.ToRotation() - MathHelper.PiOver2);
            if (Projectile.ai[0] < 300f)
            {
                Projectile.ai[0]++;
            }
            float charge = Projectile.ai[0] / 300f;
            if (Main.rand.NextFloat() < 0.1f + 0.4f * charge)
            {
                Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2CircularEdge(80f - 40f * charge, 80f - 40f * charge);
                Vector2 vel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * (3f + 2f * charge);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.Shadowflame, vel, 0, new Color(180, 0, 255), 1f + charge * 0.5f);
                d.noGravity = true;
            }
            if (Main.rand.NextFloat() < 0.2f * charge)
            {
                Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2CircularEdge(60f - 30f * charge, 60f - 30f * charge);
                Vector2 vel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * (4f + 1f * charge);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.Vortex, vel, 0, new Color(0, 255, 200), 0.8f + charge * 0.4f);
                d.noGravity = true;
            }
            if (Projectile.ai[0] >= 300f && Projectile.localAI[1] == 0f)
            {
                SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.9f, Pitch = -0.1f }, Projectile.Center);
                Projectile.localAI[1] = 1f;
                for (int i = 0; i < 30; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Vortex, vel, 0, new Color(0, 255, 200), 1.4f);
                    d.noGravity = true;
                }
            }
            Projectile.localAI[0]++;
            int interval = (int)MathHelper.Lerp(28f, 6f, charge);
            if (Projectile.localAI[0] >= interval)
            {
                Projectile.localAI[0] = 0f;
                if (Main.myPlayer == Projectile.owner)
                {
                    int damage = (int)(Projectile.damage * (0.8f + charge * 1.2f));
                    int count = 1 + (int)(2f * charge);
                    SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.7f, Pitch = 0.1f }, Projectile.Center);
                    for (int i = 0; i < count; i++)
                    {
                        float angleSpread = 0.2f + 0.15f * charge;
                        float angle = aimDir.ToRotation() + Main.rand.NextFloat(-angleSpread, angleSpread);
                        Vector2 vel = angle.ToRotationVector2() * 7f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<Glove_of_celestia_proj>(), damage, Projectile.knockBack, Projectile.owner, ai0: Main.rand.NextFloat(0f, 100f));
                    }
                }
            }
            Lighting.AddLight(Projectile.Center, 0.2f + 0.3f * charge, 0.1f + 0.2f * charge, 0.5f + 0.5f * charge);
            if (Main.myPlayer == Projectile.owner && !player.channel)
            {
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float chargeProgress = Projectile.ai[0] / 300f;
            float coreScale = MathHelper.Lerp(0f, 2.5f, chargeProgress);
            if (coreScale > 0.01f)
            {
                Texture2D coreTex = ModContent.Request<Texture2D>("jarkos/textures/MagicSphereProj").Value;
                Vector2 coreOrigin = coreTex.Size() / 2f;
                float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.15f;
                Color outerGlow = Color.Lerp(new Color(0, 100, 255, 0), new Color(180, 0, 255, 0), chargeProgress);
                Color innerGlow = Color.Lerp(new Color(100, 200, 255), new Color(255, 255, 255), chargeProgress);
                Main.EntitySpriteDraw(coreTex, Projectile.Center - Main.screenPosition, null, outerGlow * 0.6f, -Main.GameUpdateCount * 0.05f, coreOrigin, coreScale * 1.3f * pulse, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(coreTex, Projectile.Center - Main.screenPosition, null, outerGlow * 0.8f, Main.GameUpdateCount * 0.07f, coreOrigin, coreScale * 1.0f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(coreTex, Projectile.Center - Main.screenPosition, null, innerGlow * 0.9f, 0f, coreOrigin, coreScale * 0.6f * pulse, SpriteEffects.None, 0);
            }
            return false;
        }
    }
    public class Glove_of_celestia_proj : ModProjectile
    {
        public override string Texture => "jarkos/textures/MagicSphereProj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            Projectile.localAI[0]++;
            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Projectile.oldPos[i] = Projectile.oldPos[i - 1];
            }
            Projectile.oldPos[0] = Projectile.position;
            Vector2 targetReturnPos = player.Center;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Projectile.owner && proj.type == ModContent.ProjectileType<Glove_of_celestia_held>())
                {
                    targetReturnPos = proj.Center;
                    break;
                }
            }
            if (Projectile.ai[1] != 1f && Projectile.localAI[0] >= 120f)
            {
                Projectile.ai[1] = 1f;
                Projectile.netUpdate = true;
            }
            bool returning = Projectile.ai[1] == 1f;
            if (returning)
            {
                Vector2 toTargetCore = targetReturnPos - Projectile.Center;
                float dist = toTargetCore.Length();
                if (dist < 32f)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        player.Heal(5);
                    }
                    SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
                    for (int i = 0; i < 12; i++)
                    {
                        Dust d = Dust.NewDustPerfect(targetReturnPos, DustID.Vortex, Main.rand.NextVector2Circular(4f, 4f), 0, new Color(0, 255, 200), 1.2f);
                        d.noGravity = true;
                    }
                    Projectile.Kill();
                    return;
                }
                float speed = 18f;
                Vector2 desired = toTargetCore.SafeNormalize(Vector2.Zero) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.15f);
                Projectile.position += Projectile.velocity;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
            else
            {
                NPC target = null;
                float closestDist = 1500f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            target = npc;
                        }
                    }
                }
                float speed = 12f;
                if (target != null)
                {
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.06f);
                }
                else
                {
                    if (Projectile.velocity == Vector2.Zero)
                    {
                        Projectile.velocity = Projectile.ai[0].ToRotationVector2() * speed;
                    }
                }
                float wave = (float)Math.Sin(Projectile.localAI[0] * 0.06f + Projectile.ai[0]) * 2f;
                Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
                Projectile.velocity = (Projectile.velocity + perp * wave).SafeNormalize(Vector2.Zero) * speed;
                Projectile.position += Projectile.velocity;
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, -Projectile.velocity * 0.1f, 0, new Color(180, 0, 255), 1.2f);
                    d.noGravity = true;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[1] != 1f)
            {
                Projectile.ai[1] = 1f;
                Projectile.netUpdate = true;
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(9f, 9f);
                    Dust dust = Dust.NewDustPerfect(target.Center, DustID.Shadowflame, vel, 0, new Color(200, 0, 255), 1.8f);
                    dust.noGravity = true;
                }
                for (int i = 0; i < 15; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(7f, 7f);
                    Dust dust = Dust.NewDustPerfect(target.Center, DustID.Vortex, vel, 0, new Color(0, 255, 200), 1.3f);
                    dust.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.2f }, target.Center);
            }
        }
        private Vector2 GetCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float tension = 0.2f;
            return
                p1 * (2f * t * t * t - 3f * t * t + 1f) + (p2 - p0) * tension * (t * t * t - 2f * t * t + t) + p2 * (-2f * t * t * t + 3f * t * t) + (p3 - p1) * tension * (t * t * t - t * t);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (HealShaders.Stream != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
                effect.Parameters["uOpacity"]?.SetValue(Projectile.Opacity);
                float colorCycle = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + Projectile.whoAmI) * 0.5f + 0.5f;
                Color blue = new Color(0, 100, 255);
                Color purple = new Color(180, 0, 255);
                Color outerColor = Color.Lerp(blue, purple, colorCycle);
                Color innerColor = Color.Lerp(purple, blue, colorCycle);
                effect.Parameters["uColor"]?.SetValue(outerColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(innerColor.ToVector4());
                Vector2 half = Projectile.Size * 0.5f;
                var rawPoints = new List<Vector2>();
                rawPoints.Add(Projectile.Center);
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        Vector2 nextPos = Projectile.oldPos[i] + half;
                        if (Vector2.DistanceSquared(rawPoints[rawPoints.Count - 1], nextPos) > 16f)
                        {
                            rawPoints.Add(nextPos);
                        }
                    }
                }
                var smoothPoints = new List<Vector2>();
                if (rawPoints.Count >= 2)
                {
                    int subdivisions = 5;
                    for (int i = 0; i < rawPoints.Count - 1; i++)
                    {
                        Vector2 p1 = rawPoints[i];
                        Vector2 p2 = rawPoints[i + 1];
                        Vector2 p0 = i > 0 ? rawPoints[i - 1] : p1 + (p1 - p2);
                        Vector2 p3 = i < rawPoints.Count - 2 ? rawPoints[i + 2] : p2 + (p2 - p1);
                        for (int j = 0; j < subdivisions; j++)
                        {
                            float t = (float)j / subdivisions;
                            Vector2 pt = GetCatmullRom(p0, p1, p2, p3, t);
                            if (smoothPoints.Count == 0 || Vector2.DistanceSquared(smoothPoints[smoothPoints.Count - 1], pt) > 0.25f)
                            {
                                smoothPoints.Add(pt);
                            }
                        }
                    }
                    Vector2 lastPt = rawPoints[rawPoints.Count - 1];
                    if (smoothPoints.Count == 0 || Vector2.DistanceSquared(smoothPoints[smoothPoints.Count - 1], lastPt) > 0.25f)
                    {
                        smoothPoints.Add(lastPt);
                    }
                }
                TrailRenderer.Draw(effect, smoothPoints, t => 20f * MathF.Pow(1f - t, 1.2f) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Color baseColor = new Color(180, 0, 255) * Projectile.Opacity;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(200, 100, 255, 0) * 0.4f, Projectile.rotation, origin, Projectile.scale * 1.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, baseColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * 0.9f, Projectile.rotation, origin, Projectile.scale * 0.8f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
