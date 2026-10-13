using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace jarkos.projectiles.melee.true_melee
{
    public class CollapsarLightning : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private ref float TargetIndex => ref Projectile.ai[0];
        private ref float Seed => ref Projectile.ai[1];

        private const int Lifetime = 16;

        private List<Vector2> _mainBolt;
        private readonly List<List<Vector2>> _branches = new();
        private bool _generated;
        private bool _damaged;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.netImportant = true;
        }

        private static List<Vector2> GenerateBolt(Vector2 start, Vector2 end, float displacement, ref UnifiedRandom rng)
        {
            var points = new List<Vector2> { start, end };
            float disp = displacement;

            while (disp > 4f)
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

        private void Generate()
        {
            _generated = true;
            int targetIdx = (int)TargetIndex;
            NPC target = (targetIdx >= 0 && targetIdx < Main.maxNPCs) ? Main.npc[targetIdx] : null;
            Vector2 end = (target != null && target.active) ? target.Center : Projectile.Center + Main.rand.NextVector2Unit() * 200f;

            var rng = new UnifiedRandom((int)Seed);
            _mainBolt = GenerateBolt(Projectile.Center, end,
                Vector2.Distance(Projectile.Center, end) * 0.28f, ref rng);

            int branchCount = rng.Next(2, 4);

            for (int i = 0; i < branchCount; i++)
            {
                int idx = rng.Next(_mainBolt.Count / 4, _mainBolt.Count * 3 / 4);

                Vector2 bStart = _mainBolt[idx];
                Vector2 dir = (end - Projectile.Center).SafeNormalize(Vector2.UnitX).RotatedBy(rng.NextFloat(-1.1f, 1.1f));
                Vector2 bEnd = bStart + dir * rng.NextFloat(40f, 110f);

                _branches.Add(GenerateBolt(bStart, bEnd, 20f, ref rng));
            }
        }

        public override void AI()
        {
            if (!_generated)
            {
                Generate();
                SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.5f, Pitch = 0.7f },
                    Projectile.Center);
            }

            int targetIdx = (int)TargetIndex;
            NPC target = (targetIdx >= 0 && targetIdx < Main.maxNPCs) ? Main.npc[targetIdx] : null;

            if (!_damaged && target != null && target.active && Main.myPlayer == Projectile.owner)
            {
                _damaged = true;

                Projectile.Center = target.Center;
                Projectile.width = target.width;
                Projectile.height = target.height;
            }

            if (_mainBolt != null)
            {
                foreach (Vector2 p in _mainBolt)
                    if (Main.rand.NextBool(6))
                        Lighting.AddLight(p, 0.15f, 0.4f, 0.6f);

                if (Main.rand.NextBool(2))
                {
                    Vector2 p = _mainBolt[Main.rand.Next(_mainBolt.Count)];
                    Dust d = Dust.NewDustPerfect(p, DustID.Electric,
                        Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.9f);
                    d.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElectroStun>(), 30);

            for (int i = 0; i < 14; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Electric, Main.rand.NextVector2Circular(6f, 6f), 0, Color.Cyan, 1.5f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_mainBolt == null)
                return false;

            Texture2D pixel = TextureAssets.MagicPixel.Value;

            float life = Projectile.timeLeft / (float)Lifetime;
            float flicker = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 60f + Seed % 10);
            float alpha = life * flicker;

            DrawBoltPass(pixel, _mainBolt, 10f, new Color(60, 20, 180) * (alpha * 0.35f));
            DrawBoltPass(pixel, _mainBolt, 5f, new Color(40, 160, 255) * (alpha * 0.7f));
            DrawBoltPass(pixel, _mainBolt, 2f, Color.White * alpha);

            foreach (var branch in _branches)
            {
                DrawBoltPass(pixel, branch, 4f, new Color(40, 160, 255) * (alpha * 0.5f));
                DrawBoltPass(pixel, branch, 1.5f, Color.White * (alpha * 0.8f));
            }
            return false;
        }

        private static void DrawBoltPass(Texture2D pixel, List<Vector2> points, float thickness, Color color)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i] - Main.screenPosition;
                Vector2 b = points[i + 1] - Main.screenPosition;
                Vector2 diff = b - a;
                
                Main.spriteBatch.Draw(pixel, a,
                    new Rectangle(0, 0, 1, 1), color, diff.ToRotation(),
                    new Vector2(0f, 0.5f),
                    new Vector2(diff.Length(), thickness), SpriteEffects.None, 0f);
            }
        }
    }
}
