using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee
{
    public class AntiCollapsarSingularity_proj : ModProjectile
    {
        private int Phase
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float PhaseTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private float Rotation
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        private const int FlightTimeMax = 32;
        private const int ChargeTime = 26;
        private const int FadeTime = 14;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = FlightTimeMax + ChargeTime + FadeTime + 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            PhaseTimer++;
            Rotation += 0.18f;
            switch (Phase)
            {
                case 0: UpdateFlight(); break;
                case 1: UpdateCharge(); break;
                case 2: UpdateFade(); break;
            }
            Lighting.AddLight(Projectile.Center, 0.7f, 0.85f, 1.1f);
            if (Main.rand.NextBool(Phase == 1 ? 1 : 3))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2.4f, 2.4f);
                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(4f), 8, 8, DustID.BlueFairy, dustVel.X, dustVel.Y, 100, default, Phase == 1 ? 1.8f : 1.1f);
                d.noGravity = true;
                d.velocity *= 0.7f;
            }
        }
        private void UpdateFlight()
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.07f);
            if (Projectile.velocity.LengthSquared() < 0.04f || PhaseTimer >= FlightTimeMax)
            {
                Projectile.velocity = Vector2.Zero;
                Phase = 1;
                PhaseTimer = 0f;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);
                for (int i = 0; i < 18; i++)
                {
                    Vector2 dv = Main.rand.NextVector2Circular(4f, 4f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueFairy, dv.X, dv.Y, 130, default, 1.6f);
                    d.noGravity = true;
                }
            }
        }
        private void UpdateCharge()
        {
            float wobble = MathF.Sin(PhaseTimer * 0.35f) * 0.3f;
            Projectile.position += new Vector2(MathF.Cos(PhaseTimer * 0.4f), MathF.Sin(PhaseTimer * 0.5f)) * 0.4f;
            Projectile.scale = 1f + wobble * 0.15f;
            if (PhaseTimer >= ChargeTime)
            {
                FireBeams();
                Phase = 2;
                PhaseTimer = 0f;
                SoundEngine.PlaySound(SoundID.Item72 with { Pitch = 0.4f, Volume = 0.7f }, Projectile.Center);
            }
        }
        private void UpdateFade()
        {
            Projectile.alpha = (int)MathHelper.Clamp(PhaseTimer / FadeTime * 255f, 0f, 255f);
            Projectile.scale = MathHelper.Lerp(1f, 1.6f, PhaseTimer / FadeTime);
            if (PhaseTimer >= FadeTime)
                Projectile.Kill();
        }
        private void FireBeams()
        {
            if (Main.myPlayer != Projectile.owner)
                return;
            int beamCount = Main.rand.Next(1, 2);
            var candidates = new List<int>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage || n.lifeMax <= 5 || n.CountsAsACritter)
                    continue;
                if (n.Distance(Projectile.Center) > 1500f)
                    continue;
                candidates.Add(i);
            }
            candidates.Sort((a, b) =>
                Main.npc[a].Distance(Projectile.Center).CompareTo(Main.npc[b].Distance(Projectile.Center)));
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < beamCount; i++)
            {
                int target = -1;
                if (candidates.Count > 0)
                    target = candidates[i % candidates.Count];
                float a = baseAngle + MathHelper.TwoPi * i / beamCount;
                Vector2 vel = a.ToRotationVector2() * 6f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<CollapsarBeam_proj>(), Projectile.damage / 2, 1.5f, Projectile.owner, ai0: target, ai1: 0f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/AntiCollapsarSingularity_proj");
            if (!tex.IsLoaded)
                return false;
            Texture2D texture = tex.Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float intensity = Phase switch
            {
                0 => 0.85f, 1 => 0.6f + MathF.Sin(PhaseTimer * 0.45f) * 0.4f, 2 => MathHelper.Clamp(1f - PhaseTimer / FadeTime, 0f, 1f), _ => 0.5f
            };
            if (Phase == 0)
            {
                for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
                {
                    Vector2 p = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                    float t = 1f - i / (float)Projectile.oldPos.Length;
                    Color c = new Color(120, 200, 255, 0) * t * 0.7f;
                    float s = Projectile.scale * (0.4f + t * 0.7f);
                    Main.EntitySpriteDraw(texture, p, null, c, Rotation * 0.5f + i * 0.1f, origin, s, SpriteEffects.None, 0);
                }
            }
            for (int i = 0; i < 5; i++)
            {
                float a = MathHelper.TwoPi * i / 5f;
                Vector2 off = a.ToRotationVector2() * (6f + MathF.Sin(Main.GameUpdateCount * 0.2f + i) * 2f);
                Color glow = new Color(120, 200, 255, 0) * intensity * 0.6f;
                Main.EntitySpriteDraw(texture, drawPos + off, null, glow, Rotation + i * 0.3f, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0);
            }
            Color core = new Color(230, 240, 255, 0) * intensity;
            Main.EntitySpriteDraw(texture, drawPos, null, core, Rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            Color cross = new Color(180, 230, 255, 0) * intensity * 0.9f;
            Main.EntitySpriteDraw(texture, drawPos, null, cross, Rotation + MathHelper.PiOver4, origin, new Vector2(Projectile.scale * 1.6f, Projectile.scale * 0.4f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, cross, Rotation + MathHelper.PiOver4 + MathHelper.PiOver2, origin, new Vector2(Projectile.scale * 1.6f, Projectile.scale * 0.4f), SpriteEffects.None, 0);
            return false;
        }
    }
}
