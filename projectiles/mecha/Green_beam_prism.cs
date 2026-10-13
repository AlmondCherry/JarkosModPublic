using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
    public class Green_beam_prism : ModProjectile
    {
        private const int MaxTeslasPerPlayer = 2;
        private const float MaxLightningDistance = 800f;
        private const int DamageInterval = 8;
        private float GlowIntensity
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float AnimationTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60 * 60 * 5;
            Projectile.DamageType = DamageClass.Default;
            Projectile.netImportant = true;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            EnforceTeslaLimit(owner);
            if (!IsOnValidSurface())
            {
                Projectile.velocity.Y += 0.3f;
                if (Projectile.velocity.Y > 10f)
                    Projectile.velocity.Y = 10f;
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
            }
            AnimationTimer++;
            if (AnimationTimer >= 6)
            {
                AnimationTimer = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }
            GlowIntensity = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f;
            SpawnTeslaEffects();
            Projectile partnerTesla = FindPartnerTesla();
            if (partnerTesla != null)
            {
                if (Projectile.whoAmI < partnerTesla.whoAmI)
                {
                    DealLightningDamage(Projectile.Center, partnerTesla.Center);
                    SpawnLightningDust(Projectile.Center, partnerTesla.Center);
                }
            }
            Lighting.AddLight(Projectile.Center, 0.3f, 0.6f, 1f);
        }
        private void EnforceTeslaLimit(Player owner)
        {
            List<Projectile> playerTeslas = new List<Projectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == Projectile.type && proj.owner == owner.whoAmI)
                {
                    playerTeslas.Add(proj);
                }
            }
            while (playerTeslas.Count > MaxTeslasPerPlayer)
            {
                Projectile oldest = playerTeslas[0];
                for (int i = 1; i < playerTeslas.Count; i++)
                {
                    if (playerTeslas[i].timeLeft > oldest.timeLeft)
                    {
                        oldest = playerTeslas[i];
                    }
                }
                oldest.Kill();
                playerTeslas.Remove(oldest);
            }
        }
        private bool IsOnValidSurface()
        {
            int tileX = (int)(Projectile.Center.X / 16);
            int tileY = (int)((Projectile.position.Y + Projectile.height + 4) / 16);
            if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                return false;
            Tile tile = Main.tile[tileX, tileY];
            return tile.HasTile && (Main.tileSolid[tile.TileType] || TileID.Sets.Platforms[tile.TileType]);
        }
        private Projectile FindPartnerTesla()
        {
            Projectile partner = null;
            float closestDist = MaxLightningDistance;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == Projectile.type && proj.owner == Projectile.owner && proj.whoAmI != Projectile.whoAmI)
                {
                    float dist = Vector2.Distance(Projectile.Center, proj.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        partner = proj;
                    }
                }
            }
            return partner;
        }
        private void SpawnTeslaEffects()
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-12, 12), Main.rand.NextFloat(-18, -5));
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.Electric, 0f, -1f, 0, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
            if (Main.rand.NextBool(8))
            {
                Vector2 sparkPos = Projectile.Top + new Vector2(Main.rand.NextFloat(-4, 4), 0);
                Dust Spark = Dust.NewDustDirect(sparkPos, 1, 1, DustID.Electric, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, -1), 0, default, 1.2f);
                Spark.noGravity = true;
            }
        }
        private void SpawnLightningDust(Vector2 start, Vector2 end)
        {
            if (Main.GameUpdateCount % 3 != 0) return;
            Vector2 direction = end - start;
            float distance = direction.Length();
            direction.Normalize();
            int dustCount = (int)(distance / 50f);
            for (int i = 0; i < dustCount; i++)
            {
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(start, end, t);
                pos += new Vector2(Main.rand.NextFloat(-15, 15), Main.rand.NextFloat(-15, 15));
                Dust dust = Dust.NewDustDirect(pos, 1, 1, DustID.Electric, 0, 0, 0, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }
        private void DealLightningDamage(Vector2 start, Vector2 end)
        {
            if (Main.GameUpdateCount % DamageInterval != 0) return;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (IsNearLine(start, end, npc.Center, 35f))
                {
                    Player owner = Main.player[Projectile.owner];
                    int damage = Projectile.damage;
                    npc.SimpleStrikeNPC(damage, (npc.Center.X < owner.Center.X) ? -1 : 1, false, 0, DamageClass.Magic, true, owner.luck);
                    for (int d = 0; d < 10; d++)
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), 0, default, 1.3f);
                        dust.noGravity = true;
                    }
                    if (Main.rand.NextBool(3))
                    {
                        SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.5f, Pitch = 0.5f }, npc.Center);
                    }
                }
            }
        }
        private bool IsNearLine(Vector2 start, Vector2 end, Vector2 point, float threshold)
        {
            Vector2 line = end - start;
            float lineLengthSq = line.LengthSquared();
            if (lineLengthSq == 0) return Vector2.Distance(start, point) <= threshold;
            Vector2 toPoint = point - start;
            float t = MathHelper.Clamp(Vector2.Dot(toPoint, line) / lineLengthSq, 0f, 1f);
            Vector2 closest = start + t * line;
            return Vector2.Distance(closest, point) <= threshold;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Projectile partnerTesla = FindPartnerTesla();
            if (partnerTesla != null && Projectile.whoAmI < partnerTesla.whoAmI)
            {
                DrawLightningBolt(spriteBatch, Projectile.Top, partnerTesla.Top);
            }
            DrawGlow(spriteBatch);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2, 0).RotatedBy(i * MathHelper.PiOver2);
                spriteBatch.Draw(texture, drawPos + offset, sourceRect, Color.Cyan * 0.4f * GlowIntensity, 0f, origin, 1f, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, drawPos, sourceRect, lightColor, 0f, origin, 1f, SpriteEffects.None, 0f);
            return false;
        }
        private void DrawGlow(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 glowPos = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < 3; i++)
            {
                float scale = 30f + i * 15f;
                float alpha = (0.15f - i * 0.04f) * GlowIntensity;
                Color glowColor = Color.Cyan * alpha;
                spriteBatch.Draw(pixel, glowPos, new Rectangle(0, 0, 1, 1), glowColor, 0f, new Vector2(0.5f), scale, SpriteEffects.None, 0f);
            }
        }
        private void DrawLightningBolt(SpriteBatch spriteBatch, Vector2 start, Vector2 end)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            List<Vector2> points = GenerateLightningPoints(start, end, 12);
            DrawLightningLayer(spriteBatch, pixel, points, Color.White * 0.2f, 12f);
            DrawLightningLayer(spriteBatch, pixel, points, Color.Cyan * 0.4f, 8f);
            DrawLightningLayer(spriteBatch, pixel, points, Color.Cyan * 0.7f, 5f);
            DrawLightningLayer(spriteBatch, pixel, points, Color.White, 2f);
            DrawLightningBranches(spriteBatch, pixel, points);
            DrawLightningNodes(spriteBatch, pixel, points);
        }
        private List<Vector2> GenerateLightningPoints(Vector2 start, Vector2 end, int segments)
        {
            List<Vector2> points = new List<Vector2>();
            points.Add(start);
            Vector2 direction = end - start;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            perpendicular.Normalize();
            int seed = (int)(Main.GameUpdateCount / 4);
            Random rand = new Random(seed);
            for (int i = 1; i < segments; i++)
            {
                float t = i / (float)segments;
                Vector2 basePoint = Vector2.Lerp(start, end, t);
                float offsetMultiplier = (float)Math.Sin(t * Math.PI);
                float maxOffset = 25f * offsetMultiplier;
                float offset = (float)(rand.NextDouble() * 2 - 1) * maxOffset;
                points.Add(basePoint + perpendicular * offset);
            }
            points.Add(end);
            return points;
        }
        private void DrawLightningLayer(SpriteBatch spriteBatch, Texture2D pixel, List<Vector2> points, Color color, float thickness)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 segStart = points[i] - Main.screenPosition;
                Vector2 segEnd = points[i + 1] - Main.screenPosition;
                Vector2 edge = segEnd - segStart;
                float angle = (float)Math.Atan2(edge.Y, edge.X);
                float length = edge.Length();
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                Vector2 scale = new Vector2(length, thickness);
                spriteBatch.Draw(pixel, segStart, sourceRect, color, angle, new Vector2(0, 0.5f), scale, SpriteEffects.None, 0f);
            }
        }
        private void DrawLightningBranches(SpriteBatch spriteBatch, Texture2D pixel, List<Vector2> mainPoints)
        {
            int seed = (int)(Main.GameUpdateCount / 4);
            Random rand = new Random(seed + 1000);
            for (int i = 1; i < mainPoints.Count - 1; i++)
            {
                if (rand.NextDouble() > 0.4) continue;
                Vector2 branchStart = mainPoints[i];
                float branchLength = 20f + (float)rand.NextDouble() * 35f;
                float branchAngle = (float)(rand.NextDouble() * Math.PI * 2);
                Vector2 branchMid = branchStart + new Vector2((float)Math.Cos(branchAngle), (float)Math.Sin(branchAngle)) * branchLength * 0.5f;
                branchMid += new Vector2((float)(rand.NextDouble() * 2 - 1) * 8, (float)(rand.NextDouble() * 2 - 1) * 8);
                Vector2 branchEnd = branchStart + new Vector2((float)Math.Cos(branchAngle), (float)Math.Sin(branchAngle)) * branchLength;
                List<Vector2> branchPoints = new List<Vector2> { branchStart, branchMid, branchEnd };
                DrawLightningLayer(spriteBatch, pixel, branchPoints, Color.Cyan * 0.5f, 4f);
                DrawLightningLayer(spriteBatch, pixel, branchPoints, Color.White * 0.8f, 1.5f);
            }
        }
        private void DrawLightningNodes(SpriteBatch spriteBatch, Texture2D pixel, List<Vector2> points)
        {
            foreach (Vector2 point in points)
            {
                Vector2 drawPos = point - Main.screenPosition;
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f), 6f, SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.Cyan * 0.5f, 0f, new Vector2(0.5f), 12f, SpriteEffects.None, 0f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-6f, 2f));
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, velocity.X, velocity.Y, 0, default, 1.3f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-10f, -2f));
                Dust Spark = Dust.NewDustDirect(Projectile.Top, 1, 1, DustID.Electric, sparkVel.X, sparkVel.Y, 0, default, 1.5f);
                Spark.noGravity = false;
            }
            SoundEngine.PlaySound(SoundID.Item94, Projectile.position);
        }
        public override bool? CanDamage() => false;
    }
}
