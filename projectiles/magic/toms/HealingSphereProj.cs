using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.particles;
using Microsoft.Xna.Framework.Graphics;
using System;
using jarkos.systems;
using Terraria.Audio;
using jarkos.items.weapons.magic.toms;
using Terraria.GameContent;
using System.IO;

namespace jarkos.projectiles.magic.toms
{
    public class HealingSphereProj : ModProjectile
    {
        private const int HealAmount = 3;
        private const float RecallMaxSpeed = 15f;
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float Phase => Projectile.ai[1];
        private bool recalling;
        private int healTarget = -1;
        
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(recalling);
            writer.Write((short)healTarget);
        }
        
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            recalling = reader.ReadBoolean();
            healTarget = reader.ReadInt16();
        }

        private static ReLogic.Content.Asset<Texture2D> healGlow;
        public override string Texture => "jarkos/projectiles/magic/toms/HealingSphereProj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 55;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            if (!Main.dedServ)
                healGlow = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/HealGlow");
        }
        public override void Unload()
        {
            healGlow = null;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 720;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            Timer++;
            Lighting.AddLight(Projectile.Center, 0.2f, 0.55f, 0.25f);
            Player owner = Main.player[Projectile.owner];

            if (Projectile.owner == Main.myPlayer)
            {
                bool newRecalling = owner.active && !owner.dead && owner.GetModPlayer<HealPlayerTome>().Recalling;
                if (newRecalling)
                {
                    Player targetPlayer = FindHealTarget(owner);
                    int newHealTarget = targetPlayer != null ? targetPlayer.whoAmI : -1;
                    
                    if (!recalling || healTarget != newHealTarget)
                    {
                        recalling = true;
                        healTarget = newHealTarget;
                        Projectile.netUpdate = true;
                    }
                }
                else if (recalling)
                {
                    recalling = false;
                    healTarget = -1;
                    Projectile.netUpdate = true;
                }
            }

            if (recalling)
                RecallAI(owner);
            else
                FloatAI();
            if (Main.rand.NextBool(recalling ? 4 : 9))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.ChlorophyteWeapon, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f), 100, default, Main.rand.NextFloat(0.7f, 1.2f));
                d.noGravity = true;
            }
            if (Main.rand.NextBool(18))
            {
                new Spark(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.6f, 0.6f), new Color(170, 255, 180), Main.rand.Next(12, 20), Main.rand.NextFloat(0.25f, 0.45f), 0.85f).Spawn();
            }
        }
        private void FloatAI()
        {
            recallStart = -1f;
            Projectile.tileCollide = Projectile.velocity.Length() > 2f;
            Projectile.velocity *= 0.968f;
            float drift = Terraria.Utils.GetLerpValue(2.5f, 0.4f, Projectile.velocity.Length(), true);
            if (drift > 0f)
            {
                Projectile.velocity.Y += (float)Math.Sin(Timer * 0.045f + Phase) * 0.012f * drift;
                Projectile.velocity.X += (float)Math.Cos(Timer * 0.032f + Phase * 1.3f) * 0.008f * drift;
            }
            if (Projectile.timeLeft < 240 && Projectile.velocity.Length() < 1.5f)
                Projectile.timeLeft = 240;
        }
        private void RecallAI(Player owner)
        {
            Projectile.tileCollide = false;
            if (recallStart < 0f)
                recallStart = Timer;
            Player target = owner;
            if (healTarget >= 0 && healTarget < Main.maxPlayers)
            {
                Player p = Main.player[healTarget];
                if (p.active && !p.dead)
                    target = p;
            }
            Vector2 toTarget = target.Center - Projectile.Center;
            float dist = toTarget.Length();
            if (dist < 26f)
            {
                HealTarget(target);
                return;
            }
            float speed = MathHelper.Lerp(5f, RecallMaxSpeed, Terraria.Utils.GetLerpValue(0f, 55f, Timer - recallStart, true));
            Vector2 desired = Vector2.Normalize(toTarget) * Math.Min(speed, dist);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.11f);
        }
        private float recallStart = -1f;
        private Player FindHealTarget(Player owner)
        {
            Player best = owner;
            float bestScore = float.MaxValue;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead || p.ghost)
                    continue;
                float dist = Projectile.Distance(p.Center);
                if (dist > 2200f)
                    continue;
                float hpFrac = p.statLife / (float)Math.Max(1, p.statLifeMax2);
                float score = hpFrac * 10000f + dist * 0.01f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = p;
                }
            }
            return best;
        }
        private void HealTarget(Player target)
        {
            int missing = target.statLifeMax2 - target.statLife;
            int heal = Math.Min(HealAmount, missing);
            if (heal > 0)
            {
                target.statLife += heal;
                target.HealEffect(heal);
            }
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.45f, Pitch = 0.6f }, target.Center);
            new Circular_glow(target.Center, Vector2.Zero, new Color(150, 255, 160), 14, 0.4f, 0.8f).Spawn();
            new Circleedge(target.Center, Vector2.Zero, new Color(120, 240, 130), 16, 0.16f, 0.7f).Spawn();
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(14f, 18f), DustID.ChlorophyteWeapon, new Vector2(0f, -Main.rand.NextFloat(1f, 2.5f)), 80, default, Main.rand.NextFloat(0.9f, 1.4f));
                d.noGravity = true;
            }
            Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            new Circular_glow(target.Center, Vector2.Zero, new Color(140, 255, 150), 12, 0.35f, 0.75f).Spawn();
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.4f }, Projectile.Center);
            new Circular_glow(Projectile.Center, Vector2.Zero, new Color(150, 255, 160), 14, 0.45f, 0.8f).Spawn();
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.ChlorophyteWeapon, Main.rand.NextVector2Circular(2.5f, 2.5f), 80, default, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(3.5f, 3.5f), new Color(170, 255, 180), Main.rand.Next(12, 22), Main.rand.NextFloat(0.3f, 0.5f), 0.85f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawHead();
            return false;
        }
        private void DrawTrail()
        {
            if (HealShaders.Stream != null)
                DrawShaderTrail();
            else
                DrawFallbackTrail();
        }
        private void DrawShaderTrail()
        {
            Effect effect = HealShaders.Stream;
            Main.spriteBatch.End();
            float bright = recalling ? 1.25f : 1f;
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["uOpacity"]?.SetValue(bright);
            effect.Parameters["uColor"]?.SetValue(new Color(60, 200, 90).ToVector4());
            effect.Parameters["uColorCore"]?.SetValue(new Color(220, 255, 220).ToVector4());
            Vector2 half = Projectile.Size * 0.5f;
            Vector2[] points = new Vector2[Projectile.oldPos.Length + 1];
            points[0] = Projectile.Center;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
                points[i + 1] = Projectile.oldPos[i] == Vector2.Zero ? Vector2.Zero : Projectile.oldPos[i] + half;
            TrailRenderer.Draw(effect, points, t => 17f * MathF.Pow(1f - t, 1.2f) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawFallbackTrail()
        {
            Texture2D glow = healGlow?.Value;
            if (glow == null)
                return;
            Vector2 half = Projectile.Size * 0.5f;
            Vector2 glowOrigin = glow.Size() * 0.5f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            int length = Projectile.oldPos.Length;
            int steps = length * 2 - 1;
            for (int s = steps - 1; s >= 0; s--)
            {
                int i = s / 2;
                Vector2 posWorld;
                if ((s & 1) == 0)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero)
                        continue;
                    posWorld = Projectile.oldPos[i];
                }
                else
                {
                    if (i + 1 >= length || Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                        continue;
                    posWorld = Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i + 1], 0.5f);
                }
                float t = s / (float)steps;
                float taper = MathF.Pow(1f - t, 1.4f);
                Vector2 pos = posWorld + half - Main.screenPosition;
                Main.spriteBatch.Draw(glow, pos, null, new Color(80, 220, 100) * (taper * 0.25f), 0f, glowOrigin, 0.75f * taper + 0.04f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glow, pos, null, new Color(220, 255, 220) * (taper * taper * 0.3f), 0f, glowOrigin, 0.38f * taper + 0.03f, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawHead()
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glow = healGlow?.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + 0.09f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f + Phase);
            float bright = recalling ? 1.25f : 1f;
            if (glow != null)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(glow, drawPos, null, new Color(90, 230, 110) * (0.55f * bright), 0f, glow.Size() * 0.5f, 0.85f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glow, drawPos, null, new Color(220, 255, 220) * (0.4f * bright), 0f, glow.Size() * 0.5f, 0.45f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White * bright, Projectile.rotation, origin, pulse, SpriteEffects.None, 0);
        }
    }
}
