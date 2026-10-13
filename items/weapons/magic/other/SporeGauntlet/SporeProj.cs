using System;
using jarkos.systems;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.weapons.magic.other.SporeGauntlet
{
    public class SporeProj : ModProjectile
    {
        private float stageTimer;
        private int partner = -1;
        private float popScale;
        private float seed = -1f;
        private float floatPhase;
        private int Stage { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private int Tier { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }
        private float StageDuration => 200f + Tier * 90f;
        public float Radius => 12f * (0.8f + Stage * 0.22f) * (1f + Tier * 0.85f);
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X * 0.35f;
            if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y * 0.35f;
            return false;
        }
        public override void AI()
        {
            if (seed < 0f)
            {
                seed = Main.rand.NextFloat(1000f);
                floatPhase = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            float spd = Projectile.velocity.Length();
            if (spd > 0.55f)
                Projectile.velocity *= 0.962f;
            else
            {
                floatPhase += 0.025f;
                Projectile.velocity = new Vector2((float)Math.Sin(floatPhase * 0.7f + seed) * 0.12f, (float)Math.Sin(floatPhase + seed * 2f) * 0.16f);
            }
            if (Stage < 3)
            {
                stageTimer++;
                if (stageTimer >= StageDuration)
                {
                    Stage++;
                    stageTimer = 0f;
                    popScale = 0.5f;
                    SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.4f, Pitch = 0.4f }, Projectile.Center);
                    GrowPoof(8 + Tier * 6);
                    if (Stage == 3)
                        Projectile.ai[2] = Main.rand.Next(180, 361);
                }
            }
            else
            {
                Projectile.ai[2]--;
                if (Projectile.ai[2] <= 0f)
                {
                    Explode(forced: false);
                    return;
                }
                Rectangle touch = Terraria.Utils.CenteredRectangle(Projectile.Center, new Vector2(Radius * 2.2f));
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && npc.Hitbox.Intersects(touch))
                    {
                        Explode(forced: false);
                        return;
                    }
                }
            }
            HandleMerge();
            HandleSeparation();
            popScale *= 0.88f;
            Projectile.scale = (0.8f + Stage * 0.22f) * (1f + Tier * 0.85f) * (1f + popScale);
            Vector3 light = StageColor().ToVector3() * (0.25f + 0.12f * Stage + 0.2f * Tier);
            Lighting.AddLight(Projectile.Center, light);
            if (Main.rand.NextBool(14))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(Radius * 0.6f), (int)Radius, (int)Radius, DustID.JungleSpore, 0f, -0.3f, 120, default, 0.8f + Tier * 0.3f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }
        private void HandleMerge()
        {
            if (Tier >= 2)
                return;
            if (partner >= 0)
            {
                Projectile p = Main.projectile[partner];
                if (!PartnerValid(p))
                {
                    partner = -1;
                    return;
                }
                Vector2 to = p.Center - Projectile.Center;
                float dist = to.Length();
                float touchDist = (Radius + ((SporeProj)p.ModProjectile).Radius) * 0.6f;
                if (dist < touchDist)
                {
                    if (Projectile.whoAmI < partner)
                        MergeWith(p);
                }
                else
                {
                    Vector2 want = to.SafeNormalize(Vector2.Zero) * Math.Min(9.5f, 1.8f + dist * 0.06f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, want, 0.13f);
                    if (Main.rand.NextBool(6) && dist > 30f)
                    {
                        Vector2 mid = Projectile.Center + to * Main.rand.NextFloat();
                        Dust d = Dust.NewDustPerfect(mid, DustID.JungleSpore, Vector2.Zero, 150, default, 0.7f);
                        d.noGravity = true;
                    }
                }
            }
            else if (Projectile.timeLeft % 4 == 0 && Projectile.velocity.Length() < 4.5f)
            {
                float searchR = 260f + Tier * 110f;
                int best = -1;
                float bestDist = searchR;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (i == Projectile.whoAmI) continue;
                    Projectile p = Main.projectile[i];
                    if (!p.active || p.owner != Projectile.owner || p.type != Projectile.type) continue;
                    if ((int)p.ai[0] != Stage || (int)p.ai[1] != Tier) continue;
                    SporeProj sp = (SporeProj)p.ModProjectile;
                    if (sp.partner >= 0 && sp.partner != Projectile.whoAmI) continue;
                    float d = Vector2.Distance(Projectile.Center, p.Center);
                    if (d < bestDist) { bestDist = d; best = i; }
                }
                if (best >= 0)
                {
                    partner = best;
                    ((SporeProj)Main.projectile[best].ModProjectile).partner = Projectile.whoAmI;
                }
            }
        }
        private void HandleSeparation()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (i == Projectile.whoAmI || i == partner) continue;
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != Projectile.owner || p.type != Projectile.type) continue;
                SporeProj sp = (SporeProj)p.ModProjectile;
                if (sp.partner == Projectile.whoAmI) continue;
                Vector2 away = Projectile.Center - p.Center;
                float dist = away.Length();
                float minDist = (Radius + sp.Radius) * 0.95f;
                if (dist >= minDist) continue;
                if (dist < 0.01f)
                    away = Main.rand.NextVector2Unit();
                else
                    away /= dist;
                float push = (minDist - dist) * 0.05f;
                Projectile.velocity += away * Math.Min(push, 0.9f);
            }
            if (partner < 0 && Projectile.velocity.Length() > 2.6f && Projectile.timeLeft < 3560)
                Projectile.velocity *= 0.93f;
        }
        private bool PartnerValid(Projectile p)
        {
            return p.active && p.owner == Projectile.owner && p.type == Projectile.type
                && (int)p.ai[0] == Stage && (int)p.ai[1] == Tier
                && ((SporeProj)p.ModProjectile).partner == Projectile.whoAmI;
        }
        private void MergeWith(Projectile other)
        {
            Projectile.Center = (Projectile.Center + other.Center) * 0.5f;
            Projectile.velocity = (Projectile.velocity + other.velocity) * 0.25f;
            Tier++;
            stageTimer *= 0.5f;
            popScale = 0.9f;
            partner = -1;
            Projectile.timeLeft = 3600;
            if (Stage == 3)
                Projectile.ai[2] = Main.rand.Next(180, 361);
            ((SporeProj)other.ModProjectile).partner = -1;
            other.Kill();
            SoundEngine.PlaySound(SoundID.Item34 with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
            GrowPoof(16 + Tier * 8);
        }
        private void GrowPoof(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.ChlorophyteWeapon, Main.rand.NextVector2Circular(2.5f, 2.5f), 100, default, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = true;
            }
        }
        public void Explode(bool forced)
        {
            float radius;
            float mult;
            int style;
            if (Stage < 3)
            {
                radius = 80f + Tier * 55f;
                mult = 0.35f + Tier * 0.35f;
                style = 0;
            }
            else if (Tier == 0)
            {
                radius = 175f;
                mult = 1f;
                style = 1;
            }
            else if (Tier == 1)
            {
                radius = 360f;
                mult = 3f;
                style = 1;
            }
            else
            {
                radius = 1150f;
                mult = 10f;
                style = 2;
            }
            int dmg = (int)(Projectile.damage * mult);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SporeBlast>(), dmg, 6f + Tier * 4f, Projectile.owner, radius, style, 0f);
            Projectile.Kill();
        }
        private Color StageColor()
        {
            if (Stage == 1) return new Color(150, 220, 110);
            if (Stage == 2) return new Color(95, 230, 75);
            return new Color(255, 160, 60);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeNoise").Value;
            Texture2D soft = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeSoft").Value;
            Texture2D body = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeProj").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color col = StageColor();
            float unstable = Stage == 3 ? 1f : 0f;
            float quadSize = Radius * 4.4f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < 4; i++)
            {
                float ang = seed + i * 1.7f + Main.GlobalTimeWrappedHourly * (0.20f + i * 0.07f);
                Vector2 off = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang * 1.3f) * 0.65f) * Radius * (0.7f + i * 0.18f);
                float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.6f + seed + i * 2.4f);
                Main.spriteBatch.Draw(soft, drawPos + off, null, col * (0.10f * pulse), ang, soft.Size() * 0.5f, quadSize / soft.Width * (1.1f + i * 0.25f), SpriteEffects.None, 0f);
            }
            float halo = 0.35f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + seed);
            Main.spriteBatch.Draw(soft, drawPos, null, col * halo, 0f, soft.Size() * 0.5f, quadSize / soft.Width * 1.4f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            if (SporeShaders.Orb != null)
            {
                SporeShaders.Orb.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                SporeShaders.Orb.Parameters["uSeed"]?.SetValue(seed * 0.013f);
                SporeShaders.Orb.Parameters["uColor"]?.SetValue(col.ToVector3());
                SporeShaders.Orb.Parameters["uUnstable"]?.SetValue(unstable);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, SporeShaders.Orb, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(noise, drawPos, null, Color.White, 0f, noise.Size() * 0.5f, quadSize / noise.Width, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
            }
            else
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                float flick = 1f + unstable * (0.25f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 16f + seed));
                float breathe = 1f + 0.05f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.5f + seed);
                float bodyScale = Radius * 2.2f / body.Width * breathe;
                Main.spriteBatch.Draw(body, drawPos, null, Color.Black * 0.45f, Projectile.rotation, body.Size() * 0.5f, bodyScale * 1.12f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(body, drawPos, null, col * flick, Projectile.rotation, body.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(soft, drawPos - new Vector2(Radius * 0.3f, Radius * 0.35f), null, Color.White * 0.35f, 0f, soft.Size() * 0.5f, Radius * 0.7f / soft.Width, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
