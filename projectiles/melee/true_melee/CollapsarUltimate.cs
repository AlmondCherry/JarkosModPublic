using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace jarkos.projectiles.melee.true_melee
{
    public class CollapsarUltimate : ModProjectile
    {
        public override string Texture => "Jarkos/items/weapons/melee/true_melee/Collapsar";

        public const int RaiseEnd = 45;
        public const int BlastEnd = RaiseEnd + 40;
        public const int StormEnd = BlastEnd + 180;
        public const int TotalDuration = StormEnd + 35;

        private const float BladeLength = 132f;
        private const float Scale = 1.45f;

        private Player Owner => Main.player[Projectile.owner];
        private ref float Timer => ref Projectile.ai[0];

        private List<Vector2> _megaBolt;
        private int _megaBoltRegen;
        private int _skyBoltTimer;
        private bool _finaleFired;
        private bool _weStartedRain;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = TotalDuration + 10;
            Projectile.netImportant = true;
        }

        private float SwordRotation()
        {
            float up = -MathHelper.PiOver2;
            float back = up + 2.4f * Owner.direction;

            if (Timer < RaiseEnd)
            {
                float t = Timer / RaiseEnd;
                float ease = 1f - MathF.Pow(1f - t, 3f);
                float overshoot = MathF.Sin(t * MathHelper.Pi) * -0.12f * Owner.direction;
                return MathHelper.Lerp(back, up, ease) + overshoot;
            }
            if (Timer < StormEnd)
                return up + MathF.Sin(Timer * 0.7f) * 0.015f;

            float f = (Timer - StormEnd) / (float)(TotalDuration - StormEnd);
            return MathHelper.Lerp(up, back, f * f);
        }

        public override void AI()
        {
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Projectile.Center = Owner.MountedCenter;

            float rot = SwordRotation();
            Projectile.rotation = rot;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot - MathHelper.PiOver2);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, rot - MathHelper.PiOver2 + 0.35f * Owner.direction);

            Vector2 tip = Owner.MountedCenter + rot.ToRotationVector2() * (BladeLength * Scale);


            float darkness;
            if (Timer < RaiseEnd)
                darkness = (Timer / RaiseEnd) * 0.6f;
            else if (Timer < StormEnd)
                darkness = 1f;
            else
                darkness = 1f - (Timer - StormEnd) / (float)(TotalDuration - StormEnd);
            CollapsarSky.RequestDarkness(darkness);

            UpdateStormWeather();

            if (Timer < RaiseEnd)
            {
                if (Timer == 0)
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CollapsarPreUlt") with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);

                if (Main.rand.NextBool(2))
                {
                    Vector2 from = tip + Main.rand.NextVector2CircularEdge(90f, 90f);
                    Dust d = Dust.NewDustPerfect(from, DustID.Electric,
                        (tip - from) * 0.1f, 0, Color.Cyan, 1.2f);
                    d.noGravity = true;
                }
                if (Timer % 12 == 0)
                    SoundEngine.PlaySound(SoundID.Item15 with
                    { Pitch = -0.5f + (Timer / RaiseEnd), Volume = 0.35f }, tip);
            }

            if (Timer >= RaiseEnd && Timer < BlastEnd)
            {
                if (Timer == RaiseEnd)
                {
                    SoundEngine.PlaySound(SoundID.Thunder with { Volume = 1.2f }, tip);
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CollapsarUlt") with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                        tip, Main.rand.NextVector2Unit(), 18f, 10f, 40, 3000f, "CollapsarUlt"));
                    CollapsarSky.Flash(1f);
                    _weStartedRain = true;

                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                            Owner.MountedCenter, Vector2.Zero,
                            ModContent.ProjectileType<CollapsarShockwave>(),
                            (int)(Projectile.damage * 0.9f), 9f, Projectile.owner);
                    }

                    for (int i = 0; i < 60; i++)
                    {
                        Dust d = Dust.NewDustPerfect(tip, DustID.Electric,
                            Main.rand.NextVector2Circular(14f, 14f), 0, Color.White, 2.2f);
                        d.noGravity = true;
                    }
                }

                if (--_megaBoltRegen <= 0)
                {
                    _megaBoltRegen = 3;
                    Vector2 skyEnd = new(tip.X + Main.rand.NextFloat(-120f, 120f), tip.Y - 2200f);
                    var rng = new UnifiedRandom(Main.rand.Next());
                    _megaBolt = GenerateBolt(tip, skyEnd, 260f, ref rng);
                    CollapsarSky.Flash(0.35f);
                }

                Lighting.AddLight(tip, 1.2f, 1.5f, 2f);
                if (_megaBolt != null && Main.rand.NextBool(2))
                {
                    Vector2 p = _megaBolt[Main.rand.Next(_megaBolt.Count)];
                    Dust d = Dust.NewDustPerfect(p, DustID.Electric,
                        Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.5f);
                    d.noGravity = true;
                }
            }
            else if (Timer >= BlastEnd)
            {
                _megaBolt = null;
            }

            if (Timer >= BlastEnd && Timer < StormEnd)
            {
                if (Main.rand.NextBool(35))
                {
                    CollapsarSky.Flash(Main.rand.NextFloat(0.2f, 0.5f));
                    if (Main.rand.NextBool(3))
                        SoundEngine.PlaySound(SoundID.Thunder with
                        { Volume = 0.35f, Pitch = Main.rand.NextFloat(-0.4f, 0.2f) }, Owner.Center);
                }

                if (Main.myPlayer == Projectile.owner && ++_skyBoltTimer >= 7)
                {
                    _skyBoltTimer = 0;
                    int strikes = Main.rand.NextBool(3) ? 2 : 1;
                    for (int s = 0; s < strikes; s++)
                    {
                        NPC target = PickStormTarget();
                        if (target == null)
                            break;
                        SpawnSkyBolt(target, 0.6f);
                    }
                }
            }

            if (Timer == StormEnd && !_finaleFired)
            {
                _finaleFired = true;
                CollapsarSky.Flash(1f);
                SoundEngine.PlaySound(SoundID.Thunder with { Volume = 1.1f, Pitch = -0.3f }, Owner.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Owner.Center, Main.rand.NextVector2Unit(), 12f, 8f, 25, 3000f, "CollapsarFinale"));

                if (Main.myPlayer == Projectile.owner)
                {
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && !npc.friendly && npc.CanBeChasedBy(Projectile) &&
                            Vector2.Distance(npc.Center, Owner.Center) < 1100f)
                            SpawnSkyBolt(npc, 1.1f);
                    }
                }
            }

            Timer++;
            if (Timer >= TotalDuration)
                Projectile.Kill();
        }

        private void UpdateStormWeather()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || !_weStartedRain)
                return;

            if (Timer < StormEnd)
            {
                Main.raining = true;
                Main.rainTime = 60 * 10;
                Main.maxRaining = MathHelper.Lerp(Main.maxRaining, 0.95f, 0.03f);
                Main.cloudAlpha = MathHelper.Lerp(Main.cloudAlpha, 0.9f, 0.03f);
                if (Main.netMode == NetmodeID.Server && Timer == RaiseEnd)
                {
                    NetMessage.SendData(MessageID.WorldData);
                }
            }
            else
            {
                float fade = 1f - (Timer - StormEnd) / (float)(TotalDuration - StormEnd);
                Main.maxRaining = 0.95f * fade;
                Main.cloudAlpha = 0.9f * fade;
            }
        }

        private void StopStormWeather()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || !_weStartedRain)
                return;

            Main.StopRain();
            Main.maxRaining = 0f;
            Main.cloudAlpha = 0f;

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        private void SpawnSkyBolt(NPC target, float damageMult)
        {
            Vector2 spawn = target.Center - new Vector2(Main.rand.NextFloat(-100f, 100f), 950f);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawn,
                Vector2.Zero, ModContent.ProjectileType<CollapsarSkyBolt>(),
                (int)(Projectile.damage * damageMult), 3f, Projectile.owner,
                target.whoAmI, Main.rand.Next(100000));
        }

        private NPC PickStormTarget()
        {
            var candidates = new List<NPC>();
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy(Projectile) &&
                    Vector2.Distance(npc.Center, Owner.Center) < 1100f)
                    candidates.Add(npc);
            }
            return candidates.Count == 0 ? null : Main.rand.Next(candidates);
        }

        public override void OnKill(int timeLeft)
        {
            CollapsarSky.RequestDarkness(0f);
            StopStormWeather();
        }

        private static List<Vector2> GenerateBolt(Vector2 start, Vector2 end, float displacement, ref UnifiedRandom rng)
        {
            var points = new List<Vector2> { start, end };
            float disp = displacement;

            while (disp > 8f)
            {
                var next = new List<Vector2>();
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Vector2 a = points[i], b = points[i + 1];
                    Vector2 mid = (a + b) / 2f;
                    Vector2 normal = (b - a).SafeNormalize(Vector2.UnitX)
                        .RotatedBy(MathHelper.PiOver2);
                    mid += normal * rng.NextFloat(-disp, disp);
                    next.Add(a);
                    next.Add(mid);
                }
                next.Add(points[^1]);
                points = next;
                disp *= 0.5f;
            }
            return points;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawMegaBolt();
            DrawSword(lightColor);
            return false;
        }

        private void DrawSword(Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            float drawRot = Projectile.rotation + MathHelper.PiOver4;
            Vector2 origin = new(0f, tex.Height);
            SpriteEffects fx = SpriteEffects.None;

            if (Owner.direction < 0)
            {
                fx = SpriteEffects.FlipHorizontally;
                drawRot = Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver2;
                origin = new Vector2(tex.Width, tex.Height);
            }

            if (Timer >= RaiseEnd && Timer < BlastEnd)
            {
                Main.EntitySpriteDraw(tex, Owner.MountedCenter - Main.screenPosition,
                    null, new Color(150, 230, 255, 0) * 0.8f, drawRot, origin,
                    Scale * 1.08f, fx, 0);
            }

            Main.EntitySpriteDraw(tex, Owner.MountedCenter - Main.screenPosition,
                null, lightColor, drawRot, origin, Scale, fx, 0);
        }

        private void DrawMegaBolt()
        {
            if (_megaBolt == null)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;

            float phase = (Timer - RaiseEnd) / (BlastEnd - RaiseEnd);
            float flicker = 0.85f + 0.15f * MathF.Sin(Timer * 2.3f);
            float alpha = MathHelper.Clamp(1.6f - phase, 0.5f, 1f) * flicker;

            DrawBoltPass(pixel, _megaBolt, 44f, new Color(50, 15, 160) * (alpha * 0.30f));
            DrawBoltPass(pixel, _megaBolt, 22f, new Color(40, 140, 255) * (alpha * 0.55f));
            DrawBoltPass(pixel, _megaBolt, 10f, new Color(140, 220, 255) * (alpha * 0.85f));
            DrawBoltPass(pixel, _megaBolt, 4f, Color.White * alpha);
        }

        private static void DrawBoltPass(Texture2D pixel, List<Vector2> points, float thickness, Color color)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i] - Main.screenPosition;
                Vector2 b = points[i + 1] - Main.screenPosition;
                Vector2 diff = b - a;
                Main.spriteBatch.Draw(pixel, a, new Rectangle(0, 0, 1, 1), color,
                    diff.ToRotation(), new Vector2(0f, 0.5f),
                    new Vector2(diff.Length(), thickness), SpriteEffects.None, 0f);
            }
        }
    }
}
