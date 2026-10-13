using System;
using System.Collections.Generic;
using jarkos.items.weapons.summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon
{
    public class HierarchyChainProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;
        public const int MaxChained = 3;
        public const float TargetingRadius = 100f;
        public const float MaxChainRange = 350f;
        public const int DeployFrames = 18;
        public const int BreakAnimDuration = 36;
        public bool HasChainedEnemies => _links.Count > 0;
        private struct ChainLink
        {
            public int NpcWhoAmI;
            public int AttachTick;
            public bool ImpactEmitted;
        }
        private struct BrokenChain
        {
            public Vector2[] LinkPositions;
            public Vector2[] LinkVelocities;
            public float[] LinkRotations;
            public float[] LinkAngularVels;
            public Vector2 BreakPoint;
            public int Age;
        }
        private readonly List<ChainLink> _links = new(MaxChained);
        private readonly List<BrokenChain> _broken = new();
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.netImportant = true;
            Projectile.minionSlots = 0f;
        }
        public override bool? CanDamage() => false;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            int chainItemType = ModContent.ItemType<HierarchyChainItem>();
            bool channelling = player.active && !player.dead && !player.CCed && player.channel && player.HeldItem != null && player.HeldItem.type == chainItemType;

            AdvanceBrokenChains();
            if (!channelling)
            {
                if (_links.Count > 0)
                {
                    SnapAllChainsAtChannelEnd(player);
                }
                return;
            }
            Projectile.timeLeft = 2;
            Projectile.Center = player.MountedCenter;
            Vector2 toCursor = Main.MouseWorld - player.MountedCenter;

            if (toCursor.LengthSquared() > 0.01f && player.whoAmI == Main.myPlayer)
            {
                player.ChangeDir(toCursor.X >= 0f ? 1 : -1);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, toCursor.ToRotation() - MathHelper.PiOver2);
                player.itemTime = 2;
                player.itemAnimation = 2;
            }

            PruneDeadOrEscapedLinks(player);
            TryAcquireNewTargets(player);
            ApplyChainSlow();
            EmitDeployFx(player);
        }
        private void PruneDeadOrEscapedLinks(Player player)
        {
            for (int i = _links.Count - 1; i >= 0; i--)
            {
                int idx = _links[i].NpcWhoAmI;
                if (idx < 0 || idx >= Main.maxNPCs)
                {
                    _links.RemoveAt(i);
                    continue;
                }

                NPC n = Main.npc[idx];
                if (!n.active || n.life <= 0 || n.friendly || n.dontTakeDamage)
                {
                    _links.RemoveAt(i);
                    continue;
                }

                float dist = Vector2.Distance(n.Center, player.MountedCenter);
                if (dist > MaxChainRange)
                {
                    int age = (int)Main.GameUpdateCount - _links[i].AttachTick;
                    float deployT = MathHelper.Clamp(age / (float)DeployFrames, 0f, 1f);
                    float sideSign = (idx % 2 == 0) ? 1f : -1f;
                    SpawnChainBreak(player.MountedCenter, n.Center, deployT, sideSign);
                    _links.RemoveAt(i);
                }
            }
        }
        private void TryAcquireNewTargets(Player player)
        {
            if (_links.Count >= MaxChained || player.whoAmI != Main.myPlayer)
            {
                return;
            }
            Vector2 cursor = Main.MouseWorld;
            float radiusSq = TargetingRadius * TargetingRadius;
            for (int i = 0; i < Main.maxNPCs && _links.Count < MaxChained; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.lifeMax <= 5 || n.dontTakeDamage || n.immortal)
                {
                    continue;
                }
                if (AlreadyLinked(i))
                {
                    continue;
                }
                if (Vector2.DistanceSquared(n.Center, cursor) > radiusSq)
                {
                    continue;
                }
                if (Vector2.Distance(n.Center, player.MountedCenter) > MaxChainRange)
                {
                    continue;
                }
                _links.Add(new ChainLink { NpcWhoAmI = i, AttachTick = (int)Main.GameUpdateCount, ImpactEmitted = false, });
                SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.45f, Pitch = 0.10f }, player.MountedCenter);
                for (int d = 0; d < 9; d++)
                {
                    Vector2 toward = n.Center - player.MountedCenter;
                    if (toward.LengthSquared() < 1f) toward = new Vector2(1f, 0f);
                    toward.Normalize();
                    Vector2 spread = Main.rand.NextVector2Circular(2.5f, 2.5f);
                    Dust dust = Dust.NewDustPerfect(player.MountedCenter + toward * 6f, DustID.GoldFlame, toward * 3f + spread, 120, default, 1.1f);
                    dust.noGravity = true;
                }
                Projectile.netUpdate = true;
            }
        }
        private bool AlreadyLinked(int npcIdx)
        {
            for (int i = 0; i < _links.Count; i++)
            {
                if (_links[i].NpcWhoAmI == npcIdx)
                {
                    return true;
                }
            }
            return false;
        }
        private void ApplyChainSlow()
        {
            for (int i = 0; i < _links.Count; i++)
            {
                NPC n = Main.npc[_links[i].NpcWhoAmI];
                HierarchyChainedNPC scratch = n.GetGlobalNPC<HierarchyChainedNPC>();
                scratch.ShackleTicks = 2;
            }
        }
        private void EmitDeployFx(Player player)
        {
            for (int i = 0; i < _links.Count; i++)
            {
                ChainLink link = _links[i];
                if (link.ImpactEmitted) continue;
                int age = (int)Main.GameUpdateCount - link.AttachTick;
                if (age < DeployFrames - 1) continue;
                int idx = link.NpcWhoAmI;
                if (idx < 0 || idx >= Main.maxNPCs) continue;
                NPC n = Main.npc[idx];
                if (!n.active) continue;
                Vector2 impact = n.Center;
                for (int d = 0; d < 18; d++)
                {
                    Dust dust = Dust.NewDustPerfect(impact, DustID.GoldFlame, Main.rand.NextVector2Circular(5f, 5f), 100, default, 1.45f);
                    dust.noGravity = true;
                    dust.fadeIn = 0.45f;
                }
                for (int d = 0; d < 10; d++)
                {
                    Dust dust = Dust.NewDustPerfect(impact, DustID.IceTorch, Main.rand.NextVector2Circular(3.6f, 3.6f), 130, new Color(180, 220, 255), 1.2f);
                    dust.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.45f, Pitch = 0.20f }, impact);
                link.ImpactEmitted = true;
                _links[i] = link;
            }
        }
        private void SnapAllChainsAtChannelEnd(Player player)
        {
            for (int i = _links.Count - 1; i >= 0; i--)
            {
                int idx = _links[i].NpcWhoAmI;
                if (idx < 0 || idx >= Main.maxNPCs)
                {
                    _links.RemoveAt(i);
                    continue;
                }
                NPC n = Main.npc[idx];
                if (!n.active)
                {
                    _links.RemoveAt(i);
                    continue;
                }
                int age = (int)Main.GameUpdateCount - _links[i].AttachTick;
                float deployT = MathHelper.Clamp(age / (float)DeployFrames, 0f, 1f);
                float sideSign = (idx % 2 == 0) ? 1f : -1f;
                SpawnChainBreak(player.MountedCenter, n.Center, deployT, sideSign);
            }
            _links.Clear();
        }
        private void SpawnChainBreak(Vector2 from, Vector2 to, float deployT, float sideSign)
        {
            float curveAmp = MathHelper.Lerp(25f, 0f, deployT);
            Vector2[] poly = BuildChainPolyline(from, to, curveAmp, sideSign, out int sampleCount);
            if (poly == null || sampleCount < 2)
            {
                return;
            }
            Texture2D chainTex = TextureAssets.Chain.Value;
            float linkLen = MathF.Max(chainTex.Height - 2f, 6f);
            var positions = new List<Vector2>();
            var rotations = new List<float>();

            SampleChainLinksAlongPolyline(poly, sampleCount, linkLen, positions, rotations);
            int n = positions.Count;

            if (n == 0) return;

            var velArr = new Vector2[n];
            var angArr = new float[n];
            var rotArr = new float[n];
            var posArr = new Vector2[n];
            Vector2 axis = to - from;

            if (axis.LengthSquared() < 1f) axis = new Vector2(1f, 0f);
            
            axis = Vector2.Normalize(axis);
            Vector2 perp = new Vector2(-axis.Y, axis.X);

            for (int i = 0; i < n; i++)
            {
                posArr[i] = positions[i];
                rotArr[i] = rotations[i];

                float perpSign = ((i + (int)(deployT * 7f)) % 2 == 0) ? 1f : -1f;
                float perpAmt = 1.6f + Main.rand.NextFloat(0f, 2.4f);
                float axialAmt = Main.rand.NextFloat(-1.5f, 1.5f);

                Vector2 v = perp * (perpSign * perpAmt) + axis * axialAmt;
                v.Y -= Main.rand.NextFloat(0.6f, 1.6f);

                velArr[i] = v;
                angArr[i] = Main.rand.NextFloat(-0.32f, 0.32f);
            }
            Vector2 breakPoint = Vector2.Lerp(from, to, 0.5f);
            _broken.Add(new BrokenChain { LinkPositions = posArr, LinkVelocities = velArr, LinkRotations = rotArr, LinkAngularVels = angArr, BreakPoint = breakPoint, Age = 0, });

            for (int d = 0; d < 14; d++)
            {
                Dust dust = Dust.NewDustPerfect(breakPoint, DustID.GoldFlame, Main.rand.NextVector2Circular(4.5f, 4.5f), 100, default, 1.35f);
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }
            for (int d = 0; d < 10; d++)
            {
                Dust dust = Dust.NewDustPerfect(breakPoint, DustID.IceTorch, Main.rand.NextVector2Circular(3.5f, 3.5f), 120, new Color(170, 210, 255), 1.15f);
                dust.noGravity = true;
            }
            for (int i = 0; i < n; i += 2)
            {
                Dust dust = Dust.NewDustPerfect(posArr[i], DustID.GoldFlame, velArr[i] * 0.3f, 150, default, 0.85f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.55f, Pitch = 0.15f }, breakPoint);
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.35f, Pitch = -0.20f }, breakPoint);
        }
        private void AdvanceBrokenChains()
        {
            for (int i = _broken.Count - 1; i >= 0; i--)
            {
                BrokenChain bc = _broken[i];
                bc.Age++;
                if (bc.Age >= BreakAnimDuration)
                {
                    _broken.RemoveAt(i);
                    continue;
                }
                for (int j = 0; j < bc.LinkPositions.Length; j++)
                {
                    bc.LinkVelocities[j] *= 0.93f;
                    bc.LinkVelocities[j].Y += 0.18f;
                    bc.LinkPositions[j] += bc.LinkVelocities[j];
                    bc.LinkRotations[j] += bc.LinkAngularVels[j];
                }
                _broken[i] = bc;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D chainTex = TextureAssets.Chain.Value;
            if (_links.Count > 0)
            {
                Player player = Main.player[Projectile.owner];
                Vector2 origin = player.MountedCenter;

                for (int i = 0; i < _links.Count; i++)
                {
                    ChainLink link = _links[i];
                    int idx = link.NpcWhoAmI;
                    if (idx < 0 || idx >= Main.maxNPCs) continue;

                    NPC n = Main.npc[idx];
                    if (!n.active) continue;

                    int age = (int)Main.GameUpdateCount - link.AttachTick;
                    float deployT = MathHelper.Clamp(age / (float)DeployFrames, 0f, 1f);
                    float sideSign = (idx % 2 == 0) ? 1f : -1f;

                    DrawChain(origin, n.Center, deployT, sideSign, chainTex);
                }
            }
            for (int i = 0; i < _broken.Count; i++)
            {
                DrawBrokenChain(_broken[i], chainTex);
            }
            return false;
        }
        private static Vector2[] BuildChainPolyline(Vector2 from, Vector2 to, float curveAmp, float sideSign, out int count)
        {
            Vector2 delta = to - from;
            float total = delta.Length();

            if (total < 1f)
            {
                count = 0;
                return null;
            }
            Vector2 dir = delta / total;
            Vector2 perp = new Vector2(-dir.Y, dir.X) * sideSign;

            float amplitude = MathF.Min(curveAmp, total * 0.4f);
            const int samples = 28;
            var points = new Vector2[samples + 1];

            for (int i = 0; i <= samples; i++)
            {
                float u = i / (float)samples;
                Vector2 straight = Vector2.Lerp(from, to, u);

                float wave = MathF.Sin(u * MathHelper.Pi) * amplitude;
                points[i] = straight + perp * wave;
            }
            count = samples + 1;
            return points;
        }
        private static void DrawChain(Vector2 from, Vector2 to, float deployT, float sideSign, Texture2D chainTex)
        {
            float curveAmp = MathHelper.Lerp(25f, 0f, deployT);
            Vector2[] points = BuildChainPolyline(from, to, curveAmp, sideSign, out int sampleCount);
            if (points == null) return;

            float baseLinkLen = MathF.Max(chainTex.Height - 2f, 6f);
            var segLens = new float[sampleCount - 1];
            float totalPathLen = 0f;

            for (int i = 0; i < sampleCount - 1; i++)
            {
                segLens[i] = Vector2.Distance(points[i], points[i + 1]);
                totalPathLen += segLens[i];
            }
            if (totalPathLen < 1f) return;

            int linkCount = Math.Max(1, (int)MathF.Round(totalPathLen / baseLinkLen));
            float arcStep = totalPathLen / linkCount;

            float scaleForLength = MathHelper.Clamp(arcStep / baseLinkLen, 0.5f, 1f);
            float waveU = MathHelper.Clamp(deployT * 1.15f, 0f, 1.15f);
            float deployBlend = MathHelper.Clamp(1f - deployT, 0f, 1f);

            float arcSoFar = 0f;
            float nextLinkAt = arcStep * 0.5f;
            int linksPlaced = 0;

            for (int i = 0; i < sampleCount - 1 && linksPlaced < linkCount; i++)
            {
                float segLen = segLens[i];
                if (segLen < 0.0001f) continue;
                Vector2 q0 = points[i];
                Vector2 q1 = points[i + 1];
                Vector2 stepDir = (q1 - q0) / segLen;
                float rot = stepDir.ToRotation() + MathHelper.PiOver2;

                while (linksPlaced < linkCount && nextLinkAt <= arcSoFar + segLen + 0.0001f)
                {
                    float intoSeg = MathHelper.Clamp(nextLinkAt - arcSoFar, 0f, segLen);
                    Vector2 pos = q0 + stepDir * intoSeg;
                    float linkU = nextLinkAt / totalPathLen;
                    float distFromWave = MathF.Abs(linkU - waveU);
                    float waveBoost = 0f;

                    if (deployBlend > 0f && distFromWave < 0.18f)
                    {
                        waveBoost = (1f - distFromWave / 0.18f) * deployBlend;
                    }
                    Color baseColor = Color.Lerp(Color.White, new Color(255, 245, 195), waveBoost);
                    float baseScale = scaleForLength * (1f + waveBoost * 0.6f);

                    Main.spriteBatch.Draw(chainTex, pos - Main.screenPosition, null, baseColor, rot, chainTex.Size() * 0.5f, baseScale, SpriteEffects.None, 0f);
                    if (waveBoost > 0.05f)
                    {
                        Color overlay = new Color(255, 235, 170, 0) * (waveBoost * 0.85f);
                        Main.spriteBatch.Draw(chainTex, pos - Main.screenPosition, null, overlay, rot, chainTex.Size() * 0.5f, baseScale * 1.15f, SpriteEffects.None, 0f);
                        Color overlayCool = new Color(190, 220, 255, 0) * (waveBoost * 0.5f);
                        Main.spriteBatch.Draw(chainTex, pos - Main.screenPosition, null, overlayCool, rot, chainTex.Size() * 0.5f, baseScale * 1.5f, SpriteEffects.None, 0f);
                    }

                    linksPlaced++;
                    nextLinkAt += arcStep;
                }
                arcSoFar += segLen;
            }
            if (deployBlend > 0f)
            {
                Vector2 headPos = SamplePolyline(points, sampleCount, MathHelper.Clamp(waveU, 0f, 1f));
                Texture2D pix = TextureAssets.MagicPixel.Value;
                Rectangle rect = new Rectangle(0, 0, 1, 1);
                Vector2 pixOrigin = new Vector2(0.5f, 0.5f);

                float coreSize = 12f + 18f * deployBlend;
                Color coreColor = new Color(255, 240, 200, 0) * (0.9f * deployBlend);

                Main.spriteBatch.Draw(pix, headPos - Main.screenPosition, rect, coreColor, 0f, pixOrigin, coreSize, SpriteEffects.None, 0f);
                Color halo = new Color(180, 215, 255, 0) * (0.55f * deployBlend);
                Main.spriteBatch.Draw(pix, headPos - Main.screenPosition, rect, halo, 0f, pixOrigin, coreSize * 1.9f, SpriteEffects.None, 0f);
                if (deployT >= 0.83f)
                {
                    float impactT = MathHelper.Clamp((deployT - 0.83f) / 0.17f, 0f, 1f);
                    float impactFade = 1f - MathF.Abs(impactT * 2f - 1f);
                    float flashSize = 26f + 30f * impactFade;

                    Color flashColor = new Color(255, 235, 170, 0) * (0.9f * impactFade);
                    Main.spriteBatch.Draw(pix, to - Main.screenPosition, rect, flashColor, 0f, pixOrigin, flashSize, SpriteEffects.None, 0f);
                    Color flashHalo = new Color(170, 210, 255, 0) * (0.55f * impactFade);
                    Main.spriteBatch.Draw(pix, to - Main.screenPosition, rect, flashHalo, 0f, pixOrigin, flashSize * 1.7f, SpriteEffects.None, 0f);
                }
            }
        }
        private static void SampleChainLinksAlongPolyline(Vector2[] points, int sampleCount, float linkLen, List<Vector2> outPositions, List<float> outRotations)
        {
            var segLens = new float[sampleCount - 1];
            float total = 0f;

            for (int i = 0; i < sampleCount - 1; i++)
            {
                segLens[i] = Vector2.Distance(points[i], points[i + 1]);
                total += segLens[i];
            }
            if (total < 1f) return;

            int linkCount = Math.Max(1, (int)MathF.Round(total / linkLen));
            float arcStep = total / linkCount;
            float arcSoFar = 0f;
            float nextLinkAt = arcStep * 0.5f;
            int placed = 0;

            for (int i = 0; i < sampleCount - 1 && placed < linkCount; i++)
            {
                float segLen = segLens[i];
                if (segLen < 0.0001f) continue;
                Vector2 q0 = points[i];
                Vector2 q1 = points[i + 1];
                Vector2 stepDir = (q1 - q0) / segLen;
                float rot = stepDir.ToRotation() + MathHelper.PiOver2;

                while (placed < linkCount && nextLinkAt <= arcSoFar + segLen + 0.0001f)
                {
                    float intoSeg = MathHelper.Clamp(nextLinkAt - arcSoFar, 0f, segLen);
                    outPositions.Add(q0 + stepDir * intoSeg);
                    outRotations.Add(rot);

                    placed++;
                    nextLinkAt += arcStep;
                }
                arcSoFar += segLen;
            }
        }
        private static Vector2 SamplePolyline(Vector2[] points, int count, float u)
        {
            float t = u * (count - 1);
            int i0 = (int)MathF.Floor(t);
            if (i0 >= count - 1) return points[count - 1];
            if (i0 < 0) return points[0];
            float frac = t - i0;
            return Vector2.Lerp(points[i0], points[i0 + 1], frac);
        }
        private static void DrawBrokenChain(BrokenChain bc, Texture2D chainTex)
        {
            float life = 1f - bc.Age / (float)BreakAnimDuration;
            if (life <= 0f) return;
            
            float alpha = MathHelper.Clamp(life * 1.4f, 0f, 1f);
            if (bc.Age < 6)
            {
                float flashT = 1f - bc.Age / 6f;
                Color flashColor = new Color(255, 240, 200, 0) * (flashT * 0.85f);
                Texture2D pix = TextureAssets.MagicPixel.Value;
                Vector2 flashPos = bc.BreakPoint - Main.screenPosition;
                float flashScale = 32f + flashT * 36f;

                Main.spriteBatch.Draw(pix, flashPos, new Rectangle(0, 0, 1, 1), flashColor, 0f, new Vector2(0.5f, 0.5f), flashScale, SpriteEffects.None, 0f);
            }
            Color linkColor = Color.Lerp(Color.White, new Color(255, 230, 170), 0.35f) * alpha;
            for (int j = 0; j < bc.LinkPositions.Length; j++)
            {
                float scale = 1f - (1f - life) * 0.35f;
                Main.spriteBatch.Draw(chainTex, bc.LinkPositions[j] - Main.screenPosition, null, linkColor, bc.LinkRotations[j], chainTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }
        }
    }
    public class HierarchyChainedNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        private const float SlowFactor = 0.20f;
        public int ShackleTicks;
        public override void PostAI(NPC npc)
        {
            if (ShackleTicks <= 0)
            {
                return;
            }
            npc.velocity *= SlowFactor;
            ShackleTicks--;
        }
    }
}
