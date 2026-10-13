using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Shaders;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using System.Collections.Generic;
using jarkos.systems;
using Terraria.DataStructures;
namespace jarkos.projectiles.ranged.firearm
{
	public class Nox_round : ModProjectile
	{
		private List<TrailPoint> trailPoints = new List<TrailPoint>();
        private const int MaxTrailPoints = 25;
        private struct TrailPoint
        {
            public Vector2 Position;
            public float Width;
            public float Alpha;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.15f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
            Projectile.rotation += Projectile.velocity.X * 0.05f;
            float speed = Projectile.velocity.Length();
            float trailWidth = MathHelper.Clamp(speed * 1.5f, 4f, 24f);
            trailPoints.Insert(0, new TrailPoint            {
                Position = Projectile.Center, Width = trailWidth, Alpha = 1f
            });
            if (trailPoints.Count > MaxTrailPoints)
                trailPoints.RemoveAt(trailPoints.Count - 1);
            for (int i = 0; i < trailPoints.Count; i++)
            {
                var point = trailPoints[i];
                point.Alpha = 1f - (float)i / MaxTrailPoints;
                trailPoints[i] = point;
            }
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.1f, 0.1f));
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode();
        }
        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            int explosionRadius = 120;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < explosionRadius)
                    {
                        float damageMultiplier = 1f - (distance / explosionRadius) * 0.5f;
                        int explosionDamage = (int)(Projectile.damage * damageMultiplier);
                        Vector2 knockbackDir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        npc.SimpleStrikeNPC(explosionDamage, knockbackDir.X > 0 ? 1 : -1, false, Projectile.knockBack);
                        npc.AddBuff(BuffID.Ichor, 300);
                    }
                }
            }
            CreateBloodExplosion();
            Projectile.Kill();
        }
        private void CreateBloodExplosion()
        {
            for (int i = 0; i < 60; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(12f, 12f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, velocity.X, velocity.Y);
                dust.noGravity = Main.rand.NextBool(3);
                dust.scale = Main.rand.NextFloat(1.5f, 3f);
                dust.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
            }
            for (int i = 0; i < 30; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.LifeDrain, velocity.X, velocity.Y);
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(1f, 2f);
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, velocity.X, velocity.Y);
                dust.color = new Color(150, 30, 30);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, velocity, GoreID.Smoke1 + Main.rand.Next(3), Main.rand.NextFloat(0.8f, 1.2f));
            }
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi / 10 * i;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * 30f;
                Lighting.AddLight(pos, new Vector3(1f, 0.2f, 0.2f));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawBloodTrail();
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color drawColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            Color glowColor = new Color(200, 50, 50, 0) * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi / 4 * i).ToRotationVector2() * 2f;
                Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }
            return false;
        }
        private const int TrailPixelSize = 14;
        private const float TrailPixelStep = 2f;
        private void DrawBloodTrail()
{
    if (trailPoints.Count < 2)
        return;
    Texture2D pixel = TextureAssets.MagicPixel.Value;
    Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
    MiscShaderData shader = GameShaders.Misc[NoxShaderSystem.NoxPixelTrailShaderKey]
        .UseSecondaryColor(new Color(255, 70, 70))
        .UseOpacity(1f)
        .UseShaderSpecificData(new Vector4(0.04f, 0.42f, 0.35f, 0f));
    HashSet<Point> drawnCells = new HashSet<Point>();
    for (int i = 0; i < trailPoints.Count - 1; i++)
    {
        TrailPoint current = trailPoints[i];
        TrailPoint next = trailPoints[i + 1];
        Vector2 segment = next.Position - current.Position;
        float length = segment.Length();
        if (length <= 0.001f)
            continue;
        Vector2 direction = segment / length;
        Vector2 normal = new Vector2(-direction.Y, direction.X);
        int steps = Math.Max(1, (int)(length / TrailPixelStep));
        for (int step = 0; step <= steps; step++)
        {
            float t = step / (float)steps;
            Vector2 center = Vector2.Lerp(current.Position, next.Position, t);
            float width = MathHelper.Lerp(current.Width, next.Width, t);
            float alpha = MathHelper.Lerp(current.Alpha, next.Alpha, t) * 0.85f;
            int halfWidthInCells = Math.Max(1, (int)Math.Ceiling(width / TrailPixelSize));
            for (int side = -halfWidthInCells; side <= halfWidthInCells; side++)
            {
                float sideOffset = side * TrailPixelSize;
                Vector2 worldPoint = center + normal * sideOffset;
                Vector2 screenPoint = worldPoint - Main.screenPosition;
                Point cell = new Point((int)Math.Floor(screenPoint.X / TrailPixelSize), (int)Math.Floor(screenPoint.Y / TrailPixelSize));
                if (!drawnCells.Add(cell))
                    continue;
                float sideFade = 1f - MathHelper.Clamp(Math.Abs(sideOffset) / (width + TrailPixelSize), 0f, 1f);
                if (sideFade <= 0f)
                    continue;
                Color blockColor = Color.Lerp(new Color(90, 5, 5), new Color(210, 25, 25), sideFade) * (alpha * (0.35f + sideFade * 0.65f));
                Vector2 drawPosition = new Vector2(cell.X * TrailPixelSize, cell.Y * TrailPixelSize);
                DrawData drawData = new DrawData(pixel, drawPosition, sourceRect, blockColor, 0f, Vector2.Zero, new Vector2(TrailPixelSize), SpriteEffects.None, 0);
                shader.Apply(drawData);
                drawData.Draw(Main.spriteBatch);
            }
        }
    }
}
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 200, 200, 200);
        }
	}
}
