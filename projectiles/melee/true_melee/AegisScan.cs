using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class AegisScan : ModProjectile
    {
        private const float ScanRadius = 1200f;
        private ref float Timer => ref Projectile.ai[0];
        private ref float TargetIndex => ref Projectile.ai[1];
        private Player Owner => Main.player[Projectile.owner];
        private float swordAngle;
        public override void SetStaticDefaults() => Main.projFrames[Type] = 8;
        public override void SetDefaults() 
        {
            Projectile.width = Projectile.height = 72;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 155;
        }
        public override void OnSpawn(IEntitySource source) => TargetIndex = -1f;
        public override void AI() {
            Player p = Owner;

            if (p.dead || !p.active || p.noItems || p.CCed) 
            {
                Projectile.Kill();
                return;
            }
            Timer++;
            p.heldProj = Projectile.whoAmI;
            p.itemTime = p.itemAnimation = 2;
            float raise = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(Timer / 45f, 0f, 1f));
            if (Timer > 130f)
                raise = MathHelper.SmoothStep(1f, 0f, (Timer - 130f) / 25f);

            float startAngle = p.direction == 1 ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4;
            swordAngle = startAngle.AngleLerp(-MathHelper.PiOver2, raise);
            Vector2 armPos = p.RotatedRelativePoint(p.MountedCenter);
            Vector2 hand = armPos + swordAngle.ToRotationVector2() * 14f;
            Projectile.Center = hand + swordAngle.ToRotationVector2() * 36f;
            Projectile.rotation = swordAngle;
            p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, swordAngle - MathHelper.PiOver2);
            p.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, swordAngle - MathHelper.PiOver2 + p.direction * 0.4f);
            if (Timer >= 45f && Timer <= 60f) 
            {
                p.velocity.X *= 0.9f;
                Projectile.Center += Main.rand.NextVector2Circular(1.2f, 1.2f);
                Vector2 off = Main.rand.NextVector2CircularEdge(40f, 40f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + off, DustID.GoldFlame, -off * 0.06f, 0, default, 0.9f);
                d.noGravity = true;
            }
            if (Timer == 60f)
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.5f }, Projectile.Center);
            if (Timer >= 60f && Timer <= 110f) 
            {
                float scanProgress = (Timer - 60f) / 50f;
                float wave = scanProgress * ScanRadius;
                for (int i = 0; i < 6; i++) 
                {
                    float a = Main.rand.NextFloat(MathHelper.TwoPi);
                    Dust d = Dust.NewDustPerfect(Projectile.Center + a.ToRotationVector2() * wave,
                        DustID.GoldFlame, a.ToRotationVector2() * 0.5f, 0, new Color(255, 220, 80), 1.1f);
                    d.noGravity = true;
                }
                if (TargetIndex < 0f) 
                {
                    float best = float.MaxValue;
                    int bestWho = -1;
                    foreach (NPC npc in Main.ActiveNPCs) 
                    {
                        if (npc.friendly || npc.dontTakeDamage || !npc.CanBeChasedBy())
                            continue;
                        float dist = Vector2.Distance(npc.Center, Projectile.Center);
                        if (dist <= wave && dist < best) 
                        {
                            best = dist;
                            bestWho = npc.whoAmI;
                        }
                    }
                    if (bestWho >= 0) {
                        TargetIndex = bestWho;
                        Projectile.netUpdate = true;
                        Main.npc[bestWho].GetGlobalNPC<MarkedNPC>().ApplyMark(60 * 30);
                        SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.8f }, Main.npc[bestWho].Center);
                    }
                }
            }
            if (Timer > 110f && Timer <= 125f)
                Projectile.frame = (int)MathHelper.Clamp((Timer - 110f) / 2.1f, 0f, 7f);
            if (Timer == 125f) 
            {
                SoundEngine.PlaySound(SoundID.Item156, Projectile.Center);
                if (Main.myPlayer == Projectile.owner) 
                {
                    for (int i = 0; i < 2; i++) 
                    {
                        Vector2 vel = new Vector2((i == 0 ? -1f : 1f) * Main.rand.NextFloat(2.5f, 3.5f), -Main.rand.NextFloat(6f, 8f));
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                            ModContent.ProjectileType<AegisDrone>(), Projectile.damage, 3f, Projectile.owner, 0f, i, TargetIndex);
                    }
                }
                for (int i = 0; i < 20; i++)
                    Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(4f, 4f) - Vector2.UnitY * 2f, 0, default, 1.1f).noGravity = true;
            }
            if (Timer > 125f)
                Projectile.frame = (int)MathHelper.Clamp(7f - (Timer - 125f) / 4f, 0f, 7f);
            Lighting.AddLight(Projectile.Center, 1f, 0.8f, 0.2f);
        }
        public override bool PreDraw(ref Color lightColor) 
        {
            if (Timer >= 60f && Timer <= 110f)
                DrawSonarRings();
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, 8, 0, Projectile.frame);
            Player p = Owner;

            bool flip = p.direction < 0;
            SpriteEffects fx = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float drawRot = swordAngle + (flip ? MathHelper.Pi * 0.75f : MathHelper.PiOver4);
            Vector2 origin = flip ? new Vector2(frame.Width, frame.Height) : new Vector2(0, frame.Height);
            Vector2 hand = p.RotatedRelativePoint(p.MountedCenter) + swordAngle.ToRotationVector2() * 8f;

            Main.EntitySpriteDraw(tex, hand - Main.screenPosition, frame, lightColor, drawRot, origin, 1f, fx, 0);
            if (Timer >= 45f && Timer <= 110f) 
            {
                float glow = Timer < 60f ? (Timer - 45f) / 15f : (float)Math.Sin((Timer - 60f) / 50f * MathHelper.Pi);
                Main.EntitySpriteDraw(tex, hand - Main.screenPosition, frame, new Color(255, 220, 80, 0) * glow, drawRot, origin, 1f + glow * 0.06f, fx, 0);
                Texture2D flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;
                Vector2 tip = hand + swordAngle.ToRotationVector2() * 70f - Main.screenPosition;

                Main.EntitySpriteDraw(flare, tip, null, new Color(255, 235, 140, 0) * glow, 0f, flare.Size() / 2f, new Vector2(0.4f, 1.1f) * glow, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(flare, tip, null, Color.White with { A = 0 } * (glow * 0.7f), 0f, flare.Size() / 2f, 0.3f * glow, SpriteEffects.None, 0);
            }
            return false;
        }
        private void DrawSonarRings() {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float scanProgress = (Timer - 60f) / 50f;
            const int segments = 100;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f);
            for (int ring = 0; ring < 3; ring++) 
            {
                float prog = scanProgress - ring * 0.12f;
                if (prog <= 0f || prog > 1f)
                    continue;
                    
                float radius = prog * ScanRadius;
                float alpha = (1f - prog) * (1f - ring * 0.25f) * 0.85f * pulse;
                Color col = new Color(255, 220, 40, 0) * alpha;
                Color colCore = new Color(255, 255, 200, 0) * (alpha * 0.5f);
                float segLen = MathHelper.TwoPi * radius / segments + 2f;
                for (int i = 0; i < segments; i++) 
                {
                    float a = MathHelper.TwoPi * i / segments;
                    Vector2 pos = Projectile.Center + a.ToRotationVector2() * radius - Main.screenPosition;
                    Main.spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), col, a + MathHelper.PiOver2, new Vector2(0.5f), new Vector2(segLen, 5f), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), colCore, a + MathHelper.PiOver2, new Vector2(0.5f), new Vector2(segLen, 1.5f), SpriteEffects.None, 0f);
                }
            }
        }
    }
}
