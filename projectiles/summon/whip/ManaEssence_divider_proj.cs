using jarkos.buffs.summonbuff;
using jarkos.systems;
using Jarkos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon.whip
{
	public class ManaEssence_divider_proj : ModProjectile
	{
        private float State
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private bool initialized = false;
        private float pulsePhase;
        private float orbitAngle;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.4f;
            Projectile.netImportant = true;
        }
        public override void AI()
        {
            if (!initialized)
            {
                pulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                initialized = true;
            }
            Projectile.ai[1]++;
            orbitAngle += 0.03f;
            float lightPulse = (float)Math.Sin(Projectile.ai[1] * 0.1f + pulsePhase) * 0.2f + 0.6f;
            Lighting.AddLight(Projectile.Center, 0.2f * lightPulse, 0.4f * lightPulse, 0.9f * lightPulse);
            if (State == 0)
            {
                FloatingAI();
            }
            else if (State == 1)
            {
                ReturningAI();
            }
            else if (State == 2)
            {
                Projectile.scale *= 0.9f;
                Projectile.alpha += 15;
                if (Projectile.alpha >= 255 || Projectile.scale < 0.1f)
                    Projectile.Kill();
            }
        }
        private void FloatingAI()
        {
            float floatTime = Projectile.ai[1];
            if (floatTime < 20f)
            {
                Projectile.velocity *= 0.92f;
                Projectile.velocity.Y -= 0.1f;
            }
            else
            {
                Projectile.velocity *= 0.9f;
                Projectile.velocity.Y += (float)Math.Sin(floatTime * 0.05f) * 0.05f;
                Projectile.velocity.X += (float)Math.Cos(floatTime * 0.04f) * 0.03f;
            }
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 0, 0, DustID.BlueTorch, 0, -0.5f, 100, new Color(100, 160, 255), 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            if (Main.rand.NextBool(8))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.MagicMirror, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, 0f), 0, new Color(150, 200, 255), 0.5f);
                Spark.noGravity = true;
            }
            if (floatTime > 10f)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    int rawTarget = 0;
                    Player targetPlayer = null;
                    float nearestDistSq = float.MaxValue;
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player p = Main.player[i];
                        if (p.active && !p.dead && !p.ghost && p.statMana < p.statManaMax2)
                        {
                            float dSq = Vector2.DistanceSquared(Projectile.Center, p.Center);
                            if (dSq < nearestDistSq)
                            {
                                nearestDistSq = dSq;
                                targetPlayer = p;
                            }
                        }
                    }
                    if (targetPlayer != null)
                    {
                        rawTarget = 1 + targetPlayer.whoAmI;
                    }
                    else
                    {
                        int npcIndex = (int)Projectile.ai[0];
                        if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
                        {
                            rawTarget = -1 - npcIndex;
                        }
                        else
                        {
                            rawTarget = 1000;
                        }
                    }
                    if (Projectile.localAI[1] != rawTarget)
                    {
                        Projectile.localAI[1] = rawTarget;
                        Projectile.netUpdate = true;
                    }
                }

                int currentTarget = (int)Projectile.localAI[1];
                if (currentTarget > 0 && currentTarget < 1000)
                {
                    Player targetPlayer = Main.player[currentTarget - 1];
                    if (targetPlayer.active && !targetPlayer.dead && !targetPlayer.ghost)
                    {
                        float dist = Vector2.Distance(Projectile.Center, targetPlayer.Center);
                        Vector2 toPlayer = (targetPlayer.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        float speed = MathHelper.Lerp(1.5f, 12f, Math.Min(1f, dist / 300f));
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * speed, 0.12f);
                        if (dist < 24f)
                        {
                            PickupByPlayer(targetPlayer);
                        }
                    }
                    else if (Projectile.owner == Main.myPlayer)
                    {
                        Projectile.localAI[1] = 0;
                    }
                }
                else if (currentTarget < 0)
                {
                    int npcIndex = -currentTarget - 1;
                    NPC targetNPC = Main.npc[npcIndex];
                    if (targetNPC.active)
                    {
                        float dist = Vector2.Distance(Projectile.Center, targetNPC.Center);
                        Vector2 targetPos = targetNPC.Center;
                        bool npcHasFullLife = targetNPC.life >= targetNPC.lifeMax;
                        if (npcHasFullLife && dist < 48f)
                        {
                            float orbitRad = 24f;
                            float angle = orbitAngle;
                            targetPos += new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRad;
                        }
                        Vector2 toNPC = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
                        float speed = MathHelper.Lerp(1.5f, 12f, Math.Min(1f, dist / 300f));
                        if (npcHasFullLife && dist < 48f)
                        {
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toNPC * 4f, 0.15f);
                        }
                        else
                        {
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toNPC * speed, 0.12f);
                        }
                        if (!npcHasFullLife && dist < 20f)
                        {
                            HealNPC(targetNPC);
                        }
                    }
                    else if (Projectile.owner == Main.myPlayer)
                    {
                        Projectile.localAI[1] = 0;
                    }
                }
                else if (currentTarget == 1000)
                {
                    Player owner = Main.player[Projectile.owner];
                    if (owner.active && !owner.dead && !owner.ghost)
                    {
                        float dist = Vector2.Distance(Projectile.Center, owner.Center);
                        Vector2 targetPos = owner.Center;
                        if (dist < 48f)
                        {
                            float orbitRad = 24f;
                            float angle = orbitAngle;
                            targetPos += new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRad;
                        }
                        Vector2 toOwner = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
                        float speed = MathHelper.Lerp(1.5f, 12f, Math.Min(1f, dist / 300f));
                        if (dist < 48f)
                        {
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner * 4f, 0.15f);
                        }
                        else
                        {
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner * speed, 0.12f);
                        }
                    }
                }
            }
        }
        private void PickupByPlayer(Player player)
        {
            int manaAmount = Main.rand.Next(2, 8);
            player.statMana = Math.Min(player.statMana + manaAmount, player.statManaMax2);
            player.AddBuff(ModContent.BuffType<Divider_summontag>(), 300);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, null, player.whoAmI);
                NetMessage.SendData(MessageID.PlayerBuffs, -1, -1, null, player.whoAmI);
            }
            SoundEngine.PlaySound(SoundID.Item4 with
            { Pitch = 0.3f, Volume = 0.7f, MaxInstances = 3}, Projectile.Center);
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueTorch, vel.X, vel.Y, 0, new Color(100, 180, 255), Main.rand.NextFloat(1f, 1.8f));
                dust.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 0, new Color(150, 200, 255), 1.2f);
                dust.noGravity = true;
            }
            State = 2;
            Projectile.netUpdate = true;
        }
        private void ReturningAI()
        {
            int npcIndex = (int)Projectile.ai[0];
            if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
            {
                DissipateEffect();
                Projectile.Kill();
                return;
            }
            HealNPC(Main.npc[npcIndex]);
        }
        private void HealNPC(NPC target)
        {
            int healAmount = Main.rand.Next(5, 15);
            target.life = Math.Min(target.life + healAmount, target.lifeMax);
            target.HealEffect(healAmount);
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.BlueTorch, vel.X, vel.Y, 0, new Color(60, 100, 200), 1f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item8 with
            {
                Pitch = -0.3f, Volume = 0.4f
            }, target.Center);
            Projectile.Kill();
        }
        private void DissipateEffect()
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueTorch, vel.X, vel.Y, 0, new Color(80, 140, 220), 1f);
                dust.noGravity = true;
            }
        }
        private Vector2 GetCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float tension = 0.2f;
            return
                p1 * (2f * t * t * t - 3f * t * t + 1f) + (p2 - p0) * tension * (t * t * t - 2f * t * t + t) + p2 * (-2f * t * t * t + 3f * t * t) + (p3 - p1) * tension * (t * t * t - t * t);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("jarkos/textures/MagicSphereProj").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float scale = Projectile.scale;
            float alpha = 1f - (Projectile.alpha / 255f);
            if (HealShaders.Stream != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
                effect.Parameters["uOpacity"]?.SetValue(alpha);
                float colorCycle = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + Projectile.whoAmI) * 0.5f + 0.5f;
                Color deepBlue = new Color(0, 50, 255);
                Color lightBlue = new Color(0, 160, 255);
                Color outerColor = Color.Lerp(deepBlue, lightBlue, colorCycle);
                Color innerColor = Color.Lerp(lightBlue, deepBlue, colorCycle);
                effect.Parameters["uColor"]?.SetValue(outerColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(innerColor.ToVector4());
                Vector2 half = Projectile.Size * 0.5f;
                var rawPoints = new List<Vector2>();
                rawPoints.Add(Projectile.Center);
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        Vector2 nextPos = Projectile.oldPos[i] + half;
                        if (Vector2.DistanceSquared(rawPoints[rawPoints.Count - 1], nextPos) > 16f)
                        {
                            rawPoints.Add(nextPos);
                        }
                    }
                }
                var smoothPoints = new List<Vector2>();
                if (rawPoints.Count >= 2)
                {
                    int subdivisions = 5;
                    for (int i = 0; i < rawPoints.Count - 1; i++)
                    {
                        Vector2 p1 = rawPoints[i];
                        Vector2 p2 = rawPoints[i + 1];
                        Vector2 p0 = i > 0 ? rawPoints[i - 1] : p1 + (p1 - p2);
                        Vector2 p3 = i < rawPoints.Count - 2 ? rawPoints[i + 2] : p2 + (p2 - p1);
                        for (int j = 0; j < subdivisions; j++)
                        {
                            float t = (float)j / subdivisions;
                            Vector2 pt = GetCatmullRom(p0, p1, p2, p3, t);
                            if (smoothPoints.Count == 0 || Vector2.DistanceSquared(smoothPoints[smoothPoints.Count - 1], pt) > 0.25f)
                            {
                                smoothPoints.Add(pt);
                            }
                        }
                    }
                    Vector2 lastPt = rawPoints[rawPoints.Count - 1];
                    if (smoothPoints.Count == 0 || Vector2.DistanceSquared(smoothPoints[smoothPoints.Count - 1], lastPt) > 0.25f)
                    {
                        smoothPoints.Add(lastPt);
                    }
                }
                TrailRenderer.Draw(effect, smoothPoints, t => 16f * MathF.Pow(1f - t, 1.2f) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            float glowPulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f + pulsePhase) * 0.15f + 0.85f;
            Color glowColor = new Color(0, 150, 255, 0) * 0.5f * alpha;

            spriteBatch.Draw(glowTex, drawPos, null, glowColor * glowPulse, Projectile.rotation, glowOrigin, scale * 0.375f, SpriteEffects.None, 0f);
            float mainPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + pulsePhase) * 0.12f;
            Color mainColor = new Color(0, 150, 255) * alpha;

            spriteBatch.Draw(tex, drawPos, null, mainColor, Projectile.rotation, origin, scale * mainPulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, Color.White * 0.8f * alpha, Projectile.rotation, origin, scale * 0.7f * mainPulse, SpriteEffects.None, 0f);
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }
	}
    public static class ManaEssenceNetHandler
    {
        public static void SendPickup(Mod mod, int projIdentity, int playerWhoAmI)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)JarkosPacketType.ManaEssencePickup);
            packet.Write(projIdentity);
            packet.Write((byte)playerWhoAmI);
            packet.Send();
        }
        public static void SendStateSync(Mod mod, int projIdentity, byte newState, int targetNPC)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)JarkosPacketType.ManaEssenceStateSync);
            packet.Write(projIdentity);
            packet.Write(newState);
            packet.Write(targetNPC);
            packet.Send();
        }
        public static void HandlePickup(BinaryReader reader, int whoAmI)
        {
            int projId = reader.ReadInt32();
            int playerId = reader.ReadByte();
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                packet.Write((byte)JarkosPacketType.ManaEssencePickup);
                packet.Write(projId);
                packet.Write((byte)playerId);
                packet.Send(-1, whoAmI);
            }
        }
        public static void HandleStateSync(BinaryReader reader, int whoAmI)
        {
            int projId = reader.ReadInt32();
            byte newState = reader.ReadByte();
            int targetNPC = reader.ReadInt32();
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                packet.Write((byte)JarkosPacketType.ManaEssenceStateSync);
                packet.Write(projId);
                packet.Write(newState);
                packet.Write(targetNPC);
                packet.Send(-1, whoAmI);
            }
        }
    }
}
