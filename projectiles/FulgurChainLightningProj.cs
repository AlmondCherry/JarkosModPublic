using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.trails;
using Terraria.DataStructures;
using jarkos.UI.Common;

namespace jarkos.projectiles
{
    public class FulgurChainLightningProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_1";

        private List<Vector2> _chainNodes = new List<Vector2>();
        private List<Vector2> _lightningPoints = new List<Vector2>();
        private List<int> _hitNPCs = new List<int>();

        private static Primitivedraw _outerTrail;
        private static Primitivedraw _innerTrail;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            _chainNodes.Clear();

            int initialNpcIndex = (int)Projectile.ai[0];
            if (initialNpcIndex >= 0 && initialNpcIndex < Main.maxNPCs)
            {
                NPC firstNpc = Main.npc[initialNpcIndex];
                if (firstNpc.active)
                {
                    _chainNodes.Add(firstNpc.Center);
                    _hitNPCs.Add(firstNpc.whoAmI);
                }
            }

            if (_chainNodes.Count == 0)
            {
                _chainNodes.Add(Projectile.Center);
            }

            PerformChainLightning();
            GenerateZigzagPoints();
            SpawnSparks();
        }

        private void PerformChainLightning()
        {
            int maxChains = 4;
            float maxDistance = 420f;

            for (int step = 0; step < maxChains; step++)
            {
                Vector2 lastPos = _chainNodes[_chainNodes.Count - 1];
                NPC closestNpc = null;
                float closestDist = maxDistance;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage && !_hitNPCs.Contains(npc.whoAmI))
                    {
                        float dist = Vector2.Distance(lastPos, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestNpc = npc;
                        }
                    }
                }

                if (closestNpc != null)
                {
                    _hitNPCs.Add(closestNpc.whoAmI);
                    _chainNodes.Add(closestNpc.Center);

                    closestNpc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, DamageClass.Generic, true, Projectile.owner);
                }
                else
                {
                    break;
                }
            }
        }

        private void GenerateZigzagPoints()
        {
            _lightningPoints.Clear();
            if (_chainNodes.Count < 2) return;

            for (int i = 0; i < _chainNodes.Count - 1; i++)
            {
                Vector2 start = _chainNodes[i];
                Vector2 end = _chainNodes[i + 1];
                Vector2 dir = end - start;
                float dist = dir.Length();
                if (dist <= 1f) continue;

                dir.Normalize();
                Vector2 normal = new Vector2(-dir.Y, dir.X);

                int segments = Math.Max(4, (int)(dist / 28f));
                _lightningPoints.Add(start);

                for (int s = 1; s < segments; s++)
                {
                    float frac = s / (float)segments;
                    Vector2 basePos = Vector2.Lerp(start, end, frac);
                    float offset = Main.rand.NextFloat(-16f, 16f) * MathF.Sin(frac * MathF.PI);
                    _lightningPoints.Add(basePos + normal * offset);
                }
            }
            _lightningPoints.Add(_chainNodes[_chainNodes.Count - 1]);
        }

        private void SpawnSparks()
        {
            if (Main.dedServ) return;

            foreach (Vector2 node in _chainNodes)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustDirect(node - new Vector2(12, 12), 24, 24, DustID.Electric);
                    dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                    dust.scale = Main.rand.NextFloat(1.1f, 1.5f);
                    dust.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_lightningPoints.Count < 2) return false;

            float alpha = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);

            if (_outerTrail == null)
            {
                _outerTrail = new Primitivedraw(t => 14f, t => Color.Cyan);
            }
            if (_innerTrail == null)
            {
                _innerTrail = new Primitivedraw(t => 5f, t => Color.White);
            }

            _outerTrail.WidthFunc = t => MathHelper.Lerp(16f, 6f, t) * alpha;
            _outerTrail.ColorFunc = t => Color.Lerp(Color.Cyan, Color.DeepSkyBlue, t) * (0.85f * alpha);

            _innerTrail.WidthFunc = t => MathHelper.Lerp(5f, 2f, t) * alpha;
            _innerTrail.ColorFunc = t => Color.White * (0.95f * alpha);

            _outerTrail.DrawPixelPrims(_lightningPoints, -Main.screenPosition, _lightningPoints.Count);
            _innerTrail.DrawPixelPrims(_lightningPoints, -Main.screenPosition, _lightningPoints.Count);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (Vector2 node in _chainNodes)
            {
                Vector2 nodeScreen = node - Main.screenPosition;
                Draw.RadialGlow(Main.spriteBatch, nodeScreen, 30f * alpha, Color.Cyan * (0.85f * alpha));
                Draw.RadialGlow(Main.spriteBatch, nodeScreen, 15f * alpha, Color.White * (0.90f * alpha));
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
