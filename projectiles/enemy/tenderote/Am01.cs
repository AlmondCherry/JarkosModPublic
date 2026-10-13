using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using System.Collections.Generic;
namespace jarkos.projectiles.enemy.tenderote
{
	public class Am01 : ModProjectile
	{
public override string Texture => "Terraria/Images/MagicPixel";
    private const int STRIKE_DELAY = 50;
    private const int DAMAGE_WINDOW = 15;
    private const int TOTAL_LIFETIME = 110;
    private const float LIGHTNING_WIDTH = 50f;
    private bool hasStruck = false;
    private int strikeTimer = 0;
    private Vector2 topPosition;
    private Vector2 bottomPosition;
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = TOTAL_LIFETIME;
        Projectile.alpha = 255;
        Projectile.light = 0.3f;
    }
    public override void AI()
    {
        Projectile.localAI[0]++;
        if (Projectile.localAI[0] == 1)
        {
            topPosition = Projectile.Center;
            bottomPosition = GetPartnerCenter();
            if (bottomPosition == Vector2.Zero)
                bottomPosition = topPosition + new Vector2(0, 1500f);
        }
        if (Projectile.localAI[0] < STRIKE_DELAY)
        {
            if (Projectile.localAI[0] % 4 == 0)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.Electric, 0, 0, 120, default, 0.7f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
            float pulse = (float)Math.Sin(Projectile.localAI[0] * 0.25f) * 0.3f + 0.3f;
            Lighting.AddLight(Projectile.Center, 0.2f * pulse, 0.3f * pulse, 0.8f * pulse);
            return;
        }
        if (!hasStruck)
        {
            hasStruck = true;
            Vector2 partnerPos = GetPartnerCenter();
            if (partnerPos != Vector2.Zero)
                bottomPosition = partnerPos;
            SetLightningHitbox();
            for (int i = 0; i < 15; i++)
            {
                Dust d = Dust.NewDustDirect(bottomPosition + Main.rand.NextVector2Circular(20, 10), 1, 1, DustID.Electric, 0, -3f, 0, default, 1.8f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(5, 5);
            }
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(topPosition + Main.rand.NextVector2Circular(15, 15), 1, 1, DustID.Electric, 0, 0, 0, default, 1.3f);
                d.noGravity = true;
            }
            Lighting.AddLight(bottomPosition, 1.2f, 1.2f, 2.5f);
            Lighting.AddLight(topPosition, 0.8f, 0.8f, 1.5f);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.6f, Pitch = -0.3f }, bottomPosition);
        }
        strikeTimer++;
        if (strikeTimer <= DAMAGE_WINDOW)
        {
            SetLightningHitbox();
            Lighting.AddLight(Projectile.Center, 0.6f, 0.6f, 1.2f);
            if (strikeTimer % 2 == 0)
            {
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(topPosition, bottomPosition, t);
                Dust d = Dust.NewDustDirect(pos - new Vector2(5, 5), 10, 10, DustID.Electric, 0, 0, 0, default, 1f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(2, 2);
            }
        }
        if (strikeTimer > DAMAGE_WINDOW + 20)
        {
            Projectile.Kill();
        }
    }
    private void SetLightningHitbox()
    {
        float minY = Math.Min(topPosition.Y, bottomPosition.Y);
        float maxY = Math.Max(topPosition.Y, bottomPosition.Y);
        float centerX = (topPosition.X + bottomPosition.X) / 2f;
        float height = maxY - minY;
        if (height < 100f) height = 100f;
        Projectile.position = new Vector2(centerX - LIGHTNING_WIDTH / 2f, minY);
        Projectile.width = (int)LIGHTNING_WIDTH;
        Projectile.height = (int)height;
    }
    private Vector2 GetPartnerCenter()
    {
        int idx = (int)Projectile.ai[0];
        if (idx >= 0 && idx < Main.maxProjectiles)
        {
            Projectile p = Main.projectile[idx];
            if (p.active && p.type == Projectile.type)
                return p.Center;
        }
        return Vector2.Zero;
    }
    public override bool CanHitPlayer(Player target)
    {
        if (!hasStruck || strikeTimer > DAMAGE_WINDOW)
            return false;
        float minY = Math.Min(topPosition.Y, bottomPosition.Y) - 10f;
        float maxY = Math.Max(topPosition.Y, bottomPosition.Y) + 10f;
        float centerX = (topPosition.X + bottomPosition.X) / 2f;
        Rectangle playerRect = target.Hitbox;
        bool inYRange = playerRect.Bottom > minY && playerRect.Top < maxY;
        bool inXRange = Math.Abs(playerRect.Center.X - centerX) < LIGHTNING_WIDTH;
        return inYRange && inXRange;
    }
    public override bool? CanHitNPC(NPC target) => false;
    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.ScalingArmorPenetration += 0.3f;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.ai[1] != 0f)
            return false;
        SpriteBatch spriteBatch = Main.spriteBatch;
        Vector2 partnerPos = GetPartnerCenter();
        if (partnerPos == Vector2.Zero)
            partnerPos = bottomPosition;
        Vector2 myPos = topPosition;
        if (Projectile.localAI[0] < 2)
            myPos = Projectile.Center;
        float time = Projectile.localAI[0];
        if (time >= STRIKE_DELAY)
        {
            float elapsed = time - STRIKE_DELAY;
            float totalAfter = TOTAL_LIFETIME - STRIKE_DELAY;
            float fade = 1f;
            if (elapsed > DAMAGE_WINDOW)
                fade = 1f - (elapsed - DAMAGE_WINDOW) / (totalAfter - DAMAGE_WINDOW);
            if (fade < 0f) fade = 0f;
            DrawFullLightning(spriteBatch, myPos, partnerPos, fade);
        }
        else
        {
            float warn = (float)Math.Sin(time * 0.3f) * 0.12f + 0.08f;
            DrawFullLightning(spriteBatch, myPos, partnerPos, warn);
        }
        DrawMarker(spriteBatch, myPos, time);
        DrawMarker(spriteBatch, partnerPos, time);
        return false;
    }
    private List<Vector2> GenerateLightningPoints(Vector2 start, Vector2 end)
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(start);
        Vector2 direction = end - start;
        float distance = direction.Length();
        if (distance < 10f)
        {
            points.Add(end);
            return points;
        }
        int segments = Math.Max(6, (int)(distance / 22f));
        Vector2 dirNorm = direction / distance;
        Vector2 perp = new Vector2(-dirNorm.Y, dirNorm.X);
        for (int i = 1; i < segments; i++)
        {
            float progress = i / (float)segments;
            Vector2 basePoint = Vector2.Lerp(start, end, progress);
            float envelope = (float)Math.Sin(progress * MathHelper.Pi);
            float maxOffset = distance * 0.035f * envelope;
            float offset = Main.rand.NextFloat(-maxOffset, maxOffset);
            offset += (float)Math.Sin(Main.GameUpdateCount * 0.5f + i * 1.7f) * maxOffset * 0.35f;
            points.Add(basePoint + perp * offset);
        }
        points.Add(end);
        return points;
    }
    private void DrawFullLightning(SpriteBatch sb, Vector2 start, Vector2 end, float intensity)
    {
        if (intensity <= 0.01f) return;
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        List<Vector2> points = GenerateLightningPoints(start, end);
        float pulse = 0.75f + (float)Math.Sin(Main.GameUpdateCount * 0.35f) * 0.25f;
        float fi = intensity * pulse;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[i + 1];
            Vector2 diff = b - a;
            float len = diff.Length();
            if (len < 0.5f) continue;
            float rot = diff.ToRotation();
            DrawSeg(sb, pixel, a, len, rot, new Color(30, 70, 255) * 0.12f * fi, 20f);
            DrawSeg(sb, pixel, a, len, rot, Color.Cyan * 0.25f * fi, 9f);
            DrawSeg(sb, pixel, a, len, rot, Color.White * 0.65f * fi, 3f);
            DrawSeg(sb, pixel, a, len, rot, Color.White * 0.85f * fi, 1.2f);
        }
        if (fi > 0.15f)
            DrawBranches(sb, pixel, points, Vector2.Distance(start, end), fi);
        if (intensity > 0.3f)
        {
            Vector2 impactDraw = end - Main.screenPosition;
            float flashSize = 45f * fi;
            sb.Draw(pixel, impactDraw, new Rectangle(0, 0, 1, 1), new Color(140, 200, 255) * fi * 0.35f, 0f, new Vector2(0.5f, 0.5f), new Vector2(flashSize, 3f), SpriteEffects.None, 0f);
            sb.Draw(pixel, impactDraw, new Rectangle(0, 0, 1, 1), Color.White * fi * 0.6f, 0f, new Vector2(0.5f, 0.5f), new Vector2(6f, 6f), SpriteEffects.None, 0f);
        }
    }
    private void DrawBranches(SpriteBatch sb, Texture2D pixel, List<Vector2> mainPoints, float mainDist, float intensity)
    {
        for (int i = 2; i < mainPoints.Count - 2; i++)
        {
            float hash = (float)Math.Sin(i * 13.7f + Main.GameUpdateCount * 0.1f);
            if (hash > -0.6f) continue;
            Vector2 branchStart = mainPoints[i];
            Vector2 mainDir = mainPoints[mainPoints.Count - 1] - mainPoints[0];
            Vector2 perpDir = new Vector2(-mainDir.Y, mainDir.X);
            if (perpDir.LengthSquared() > 0) perpDir.Normalize();
            float side = (hash < -0.8f) ? 1f : -1f;
            float angle = Main.rand.NextFloat(0.35f, 0.9f) * side;
            Vector2 branchDir = perpDir.RotatedBy(angle);
            float branchLen = mainDist * Main.rand.NextFloat(0.05f, 0.12f);
            int count = Main.rand.Next(2, 4);
            float segLen = branchLen / count;
            Vector2 cur = branchStart;
            Vector2 prev = branchStart;
            for (int j = 0; j < count; j++)
            {
                float decay = 1f - (float)j / count;
                cur += branchDir * segLen * decay + Main.rand.NextVector2Circular(6f * decay, 6f * decay);
                Vector2 diff = cur - prev;
                float len = diff.Length();
                if (len < 0.5f) { prev = cur; continue; }
                float rot = diff.ToRotation();
                float bi = intensity * 0.4f;
                DrawSeg(sb, pixel, prev, len, rot, Color.Cyan * 0.12f * bi, 8f);
                DrawSeg(sb, pixel, prev, len, rot, Color.White * 0.35f * bi, 3f);
                DrawSeg(sb, pixel, prev, len, rot, Color.White * 0.5f * bi, 1f);
                prev = cur;
            }
        }
    }
    private void DrawSeg(SpriteBatch sb, Texture2D pixel, Vector2 pos, float length, float rotation, Color color, float thickness)
    {
        sb.Draw(pixel, pos - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
    }
    private void DrawMarker(SpriteBatch sb, Vector2 worldPos, float time)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 drawPos = worldPos - Main.screenPosition;
        if (time < STRIKE_DELAY)
        {
            float pulse = (float)Math.Sin(time * 0.25f) * 0.35f + 0.35f;
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(255, 120, 80) * pulse, 0f, new Vector2(0.5f, 0.5f), new Vector2(24f, 24f), SpriteEffects.None, 0f);
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.Cyan * pulse * 0.25f, 0f, new Vector2(0.5f, 0.5f), new Vector2(40f, 40f), SpriteEffects.None, 0f);
        }
        else
        {
            float fade = 1f - (time - STRIKE_DELAY) / (float)(TOTAL_LIFETIME - STRIKE_DELAY);
            if (fade < 0f) fade = 0f;
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White * fade * 0.5f, 0f, new Vector2(0.5f, 0.5f), new Vector2(16f * fade, 16f * fade), SpriteEffects.None, 0f);
        }
    }
}
}
