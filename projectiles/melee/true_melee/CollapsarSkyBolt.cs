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
    public class CollapsarSkyBolt : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private ref float TargetIndex => ref Projectile.ai[0];
        private ref float Seed => ref Projectile.ai[1];

        private const int Lifetime = 14;

        private List<Vector2> _bolt;
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

            while (disp > 5f)
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
            Vector2 end = (target != null && target.active) ? target.Center : Projectile.Center + Vector2.UnitY * 900f;

            var rng = new UnifiedRandom((int)Seed);
            _bolt = GenerateBolt(Projectile.Center, end,
                Vector2.Distance(Projectile.Center, end) * 0.16f, ref rng);

            int branchCount = rng.Next(1, 3);
            for (int i = 0; i < branchCount; i++)
            {
                int idx = rng.Next(_bolt.Count / 3, _bolt.Count * 3 / 4);
                Vector2 dir = Vector2.UnitY.RotatedBy(rng.NextFloat(-1.2f, 1.2f));
                _branches.Add(GenerateBolt(_bolt[idx],
                    _bolt[idx] + dir * rng.NextFloat(60f, 140f), 24f, ref rng));
            }
        }

        public override void AI()
        {
            if (!_generated)
            {
                Generate();
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CollapsarUltLightning") with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);
            }

            int targetIdx = (int)TargetIndex;
            NPC target = (targetIdx >= 0 && targetIdx < Main.maxNPCs) ? Main.npc[targetIdx] : null;

            if (!_damaged && target != null && target.active)
            {
                _damaged = true;
                Projectile.Center = target.Center;
                Projectile.width = target.width;
                Projectile.height = target.height;
            }

            if (_bolt != null)
                for (int i = 0; i < _bolt.Count; i += 4)
                    Lighting.AddLight(_bolt[i], 0.2f, 0.45f, 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElectroStun>(), 20);
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Electric, Main.rand.NextVector2Circular(6f, 6f), 0, Color.Cyan, 1.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_bolt == null)
                return false;

            Texture2D pixel = TextureAssets.MagicPixel.Value;

            float life = Projectile.timeLeft / (float)Lifetime;
            float alpha = life * (0.75f + 0.25f * MathF.Sin(Seed + Main.GlobalTimeWrappedHourly * 50f));

            DrawPass(pixel, _bolt, 9f, new Color(60, 20, 180) * (alpha * 0.35f));
            DrawPass(pixel, _bolt, 4f, new Color(40, 160, 255) * (alpha * 0.7f));
            DrawPass(pixel, _bolt, 1.8f, Color.White * alpha);

            foreach (var br in _branches)
            {
                DrawPass(pixel, br, 3f, new Color(40, 160, 255) * (alpha * 0.5f));
                DrawPass(pixel, br, 1.3f, Color.White * (alpha * 0.8f));
            }
            return false;
        }

        private static void DrawPass(Texture2D pixel, List<Vector2> pts, float thickness, Color color)
        {
            for (int i = 0; i < pts.Count - 1; i++)
            {
                Vector2 a = pts[i] - Main.screenPosition;
                Vector2 diff = pts[i + 1] - pts[i];
                
                Main.spriteBatch.Draw(pixel, a, new Rectangle(0, 0, 1, 1), color,
                    diff.ToRotation(), new Vector2(0f, 0.5f),
                    new Vector2(diff.Length(), thickness), SpriteEffects.None, 0f);
            }
        }
    }
}
