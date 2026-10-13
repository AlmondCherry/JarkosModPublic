using System;
using System.Collections.Generic;
using jarkos.npc.CradleMobs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles
{
    public class ScavengerTentacleProj : ModProjectile
    {
        public override string Texture => "Jarkos/projectiles/Empty_proj";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.hide = true;
        }
        private NPC targetNPC = null;
        private int State
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private int Timer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void AI()
        {
            NPC ownerNPC = null;
            float minDist = 2000f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<ScavengerNPC>())
                {
                    float dist = Main.npc[i].Distance(Projectile.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        ownerNPC = Main.npc[i];
                    }
                }
            }
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }
            if (State == 0)
            {
                if (targetNPC == null || !targetNPC.active || targetNPC.boss)
                {
                    float closestDist = 800f;
                    targetNPC = null;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC n = Main.npc[i];
                        if (n.active && !n.friendly && !n.boss && n.CanBeChasedBy() && n.Distance(Projectile.Center) < closestDist)
                        {
                            targetNPC = n;
                            closestDist = n.Distance(Projectile.Center);
                        }
                    }
                    if (targetNPC == null)
                    {
                        Projectile.Kill();
                        return;
                    }
                }
                Vector2 dir = targetNPC.Center - Projectile.Center;
                if (dir.Length() < 20f)
                {
                    State = 1;
                    Timer = 0;
                    Projectile.Center = targetNPC.Center;
                }
                else
                {
                    dir.Normalize();
                    Projectile.velocity = dir * 25f;
                }
            }
            else if (State == 1)
            {
                if (!targetNPC.active)
                {
                    Projectile.Kill();
                    return;
                }
                Timer++;
                Projectile.Center = targetNPC.Center;
                if (Timer < 30)
                {
                    targetNPC.velocity = new Vector2(0, -6f);
                }
                else
                {
                    State = 2;
                    Timer = 0;
                }
            }
            else if (State == 2)
            {
                if (!targetNPC.active)
                {
                    Projectile.Kill();
                    return;
                }
                Timer++;
                Projectile.Center = targetNPC.Center;
                targetNPC.velocity = new Vector2(0, 30f);
                if (Timer > 5 && (targetNPC.velocity.Y == 0 || targetNPC.collideY || Collision.SolidCollision(targetNPC.position, targetNPC.width, targetNPC.height)))
                {
                    State = 3;
                }
            }
            else if (State == 3)
            {
                if (targetNPC.active)
                {
                    int damage = Main.rand.Next(500, 1000);
                    targetNPC.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0, true);
                    SoundEngine.PlaySound(SoundID.NPCDeath14, targetNPC.Center);
                    for (int i = 0; i < 30; i++)
                    {
                        Dust.NewDust(targetNPC.position, targetNPC.width, targetNPC.height, DustID.Blood, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 0f), 0, default, 2f);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        int goreIndex = Gore.NewGore(Projectile.GetSource_Death(), targetNPC.position, Main.rand.NextVector2Circular(5f, 5f), Main.rand.Next(61, 64), 1f);
                        Main.gore[goreIndex].velocity *= 1.5f;
                    }
                }
                Projectile.Kill();
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            NPC ownerNPC = null;
            float minDist = 2000f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<ScavengerNPC>())
                {
                    float dist = Main.npc[i].Distance(Projectile.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        ownerNPC = Main.npc[i];
                    }
                }
            }
            if (ownerNPC == null) return false;
            Texture2D texture = TextureAssets.MagicPixel.Value;

            Vector2 startPos = ownerNPC.Center;
            Vector2 endPos = Projectile.Center;
            Vector2 dir = endPos - startPos;
            float length = dir.Length();

            if (length < 4f) return false;
            dir.Normalize();

            Vector2 perp = new Vector2(-dir.Y, dir.X);

            int segments = 20;
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(startPos, endPos, t);

                float waveEndTaper = (float)Math.Pow(Math.Sin(t * Math.PI), 2);
                float wave = (float)Math.Sin(t * MathHelper.Pi * 4f + Main.GlobalTimeWrappedHourly * 20f) * 25f * waveEndTaper;

                points[i] = basePos + perp * wave;
            }

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                float waveEndTaper = (float)Math.Pow(Math.Sin(t * Math.PI), 2);
                float thickness = MathHelper.Lerp(1f, 8f, waveEndTaper);

                Vector2 p1 = points[i];
                Vector2 p2 = points[i + 1];
                Vector2 diff = p2 - p1;

                float dist = diff.Length();
                float rot = diff.ToRotation();

                Main.spriteBatch.Draw(texture, p1 - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Gold, rot, new Vector2(0f, 0.5f), new Vector2(dist, thickness), SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
