using System;
using jarkos.npc.Punisher;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace jarkos.npc.Punisher.Cutscene
{
    public class HarakiriCutscenePlayer : ModPlayer
    {
        public static int BossToSummon => ModContent.NPCType<Punisher>();
        public const int RAISE_END = 120;
        public const int STAB_TICK = 135;
        public const int PORTAL_TICK = 280;
        public const int BOSS_TICK = 420;
        public const int BOSS_OUT_TICK = BOSS_TICK + PortalEmergence.EMERGE_TIME;
        public const int END_TICK = 660;
        public bool CutsceneActive;
        public int Timer;
        public Vector2 PortalPos;
        private float stabShake;
        private float flash;
        public void StartCutscene()
        {
            CutsceneActive = true;
            Timer = 0;
            stabShake = 0f;
            flash = 0f;
            Vector2 desired = Player.Center + new Vector2(Player.direction * 450f, -60f);
            PortalPos = SnapToGround(desired);
            Player.RemoveAllGrapplingHooks();
            if (Player.mount.Active)
                Player.mount.Dismount(Player);
            SoundEngine.PlaySound(SoundID.Item1, Player.Center);
        }
        private static Vector2 SnapToGround(Vector2 pos)
        {
            int tx = Terraria.Utils.Clamp((int)(pos.X / 16f), 10, Main.maxTilesX - 10);
            int startY = Terraria.Utils.Clamp((int)(pos.Y / 16f), 10, Main.maxTilesY - 60);
            for (int y = startY; y < startY + 50; y++)
            {
                Tile t = Framing.GetTileSafely(tx, y);
                if (t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
                    return new Vector2(pos.X, y * 16f - 132f);
            }
            return pos;
        }
        private void EndCutscene()
        {
            CutsceneActive = false;
            Timer = 0;
            stabShake = 0f;
            flash = 0f;
        }
        public override void PreUpdate()
        {
            if (!CutsceneActive)
                return;
            if (Player.dead)
            {
                EndCutscene();
                return;
            }
            Timer++;
            flash *= 0.9f;
            Player.immune = true;
            Player.immuneTime = Math.Max(Player.immuneTime, 2);
            if (Timer < STAB_TICK && Timer % 4 == 0)
            {
                Dust e = Dust.NewDustDirect(Player.Center + new Vector2(Main.rand.NextFloat(-60f, 60f), 40f), 4, 4, DustID.Shadowflame);
                e.noGravity = true;
                e.velocity = new Vector2(0f, -Main.rand.NextFloat(0.5f, 1.6f));
                e.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }
            if (Timer > STAB_TICK && Timer < BOSS_OUT_TICK && Timer % 3 == 0)
            {
                float range = MathHelper.Lerp(80f, 280f, (Timer - STAB_TICK) / (float)(BOSS_OUT_TICK - STAB_TICK));
                Vector2 spot = Player.Bottom + new Vector2(Main.rand.NextFloat(-range, range), 0f);
                Dust g = Dust.NewDustDirect(spot - new Vector2(4f, 4f), 8, 4, DustID.Dirt);
                g.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1.5f, 4f));
                g.scale = Main.rand.NextFloat(0.9f, 1.6f);
            }
            if (Timer > PORTAL_TICK - 70 && Timer < BOSS_TICK && Timer % 10 == 0)
            {
                Dust s = Dust.NewDustDirect(PortalPos + Main.rand.NextVector2Circular(120f, 140f), 4, 4, DustID.Electric);
                s.noGravity = true;
                s.velocity = Main.rand.NextVector2Circular(2f, 2f);
                s.scale = 0.9f;
            }
            if (Timer > STAB_TICK && Timer % 9 == 0)
            {
                Dust d = Dust.NewDustDirect(Player.Center - new Vector2(4f, 0f), 8, 8, DustID.Blood, 0f, 1.5f);
                d.velocity *= 0.4f;
            }
            if (Timer > STAB_TICK && Timer < BOSS_OUT_TICK && Timer % 40 == 0)
                SoundEngine.PlaySound(SoundID.WormDig, Player.Center);
            if (Timer == RAISE_END)
                SoundEngine.PlaySound(SoundID.Item71, Player.Center);
            if (Timer == STAB_TICK)
                DoStab();
            if (Timer == PORTAL_TICK)
                OpenPortal();
            if (Timer == BOSS_TICK)
                SummonBoss();
            if (Timer == BOSS_OUT_TICK)
            {
                stabShake = 16f;
                flash = 0.8f;
            }
            if (Timer >= END_TICK)
                EndCutscene();
        }
        private void DoStab()
        {
            int dmg = Math.Min(Player.statLifeMax2 / 3, Player.statLife - 1);
            if (dmg > 0)
            {
                Player.statLife -= dmg;
                CombatText.NewText(Player.getRect(), CombatText.DamagedFriendlyCrit, dmg);
            }
            SoundEngine.PlaySound(SoundID.NPCHit1, Player.Center);
            SoundEngine.PlaySound(SoundID.PlayerHit, Player.Center);
            for (int i = 0; i < 45; i++)
            {
                Dust d = Dust.NewDustDirect(Player.Center - new Vector2(8f, 4f), 16, 12, DustID.Blood);
                d.velocity = Main.rand.NextVector2Circular(4f, 3f) + new Vector2(0f, 1f);
                d.scale = Main.rand.NextFloat(1f, 1.8f);
            }
            stabShake = 9f;
            flash = 0.35f;
        }
        private void OpenPortal()
        {
            SoundEngine.PlaySound(SoundID.Item119, PortalPos);
            stabShake = 11f;
            flash = 1f;
            Vector2 ground = PortalPos + new Vector2(0f, 132f);
            DustRing(ground, 50, 9f, DustID.Shadowflame, 1.8f);
            DustRing(ground, 30, 5f, DustID.Torch, 1.5f);
            if (Player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(Player.GetSource_Misc("Jarkos:HarakiriRitual"), PortalPos, Vector2.Zero, ModContent.ProjectileType<RitualPortal>(), 0, 0f, Player.whoAmI);
            }
        }
        private void SummonBoss()
        {
            stabShake = 13f;
            flash = 1f;
            SoundEngine.PlaySound(SoundID.Roar, PortalPos);
            DustRing(PortalPos, 70, 12f, DustID.Torch, 2.2f);
            DustRing(PortalPos, 40, 7f, DustID.Shadowflame, 1.8f);
            for (int i = 0; i < 30; i++)
            {
                Dust s = Dust.NewDustDirect(PortalPos - new Vector2(20f, 100f), 40, 200, DustID.Electric);
                s.noGravity = true;
                s.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(2f, 7f));
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            int dirToPlayer = Player.Center.X < PortalPos.X ? -1 : 1;
            int npcIndex = NPC.NewNPC(Player.GetSource_Misc("jarkos:HarakiriRitual"), (int)PortalPos.X, (int)(PortalPos.Y + 130f), BossToSummon);
            if (npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                PortalEmergence.Begin(npc, dirToPlayer);
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: npcIndex);
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(npc.FullName + " Out of the portal!"), new Color(175, 75, 255));
                }
                else
                {
                    Main.NewText(npc.FullName + " Out of the portal!", new Color(175, 75, 255));
                }
            }
        }
        private static void DustRing(Vector2 center, int count, float speed, int dustType, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 v = (MathHelper.TwoPi * i / count).ToRotationVector2()
                            * speed * Main.rand.NextFloat(0.7f, 1.3f);
                Dust d = Dust.NewDustPerfect(center, dustType, v, 0, default, scale);
                d.noGravity = true;
            }
        }
        public override void SetControls()
        {
            if (!CutsceneActive)
                return;
            Player.controlLeft = Player.controlRight = Player.controlUp = Player.controlDown = false;
            Player.controlJump = Player.controlUseItem = Player.controlUseTile = false;
            Player.controlHook = Player.controlMount = Player.controlThrow = false;
        }
        public override void PreUpdateMovement()
        {
            if (!CutsceneActive)
                return;
            Player.velocity.X = 0f;
            if (Player.velocity.Y < 0f)
                Player.velocity.Y = 0f;
        }
        public override void FrameEffects()
        {
            if (!CutsceneActive)
                return;
            int dir = Player.direction;
            float armRot;
            if (Timer < RAISE_END)
            {
                float t = Timer / (float)RAISE_END;
                armRot = MathHelper.Lerp(0.6f, -2.6f, EaseOut(t)) * dir;
            }
            else if (Timer < STAB_TICK)
            {
                float t = (Timer - RAISE_END) / (float)(STAB_TICK - RAISE_END);
                armRot = MathHelper.Lerp(-2.6f, 0.9f, t * t) * dir;
            }
            else
            {
                armRot = (0.9f + (float)Math.Sin(Timer * 0.35f) * 0.04f) * dir;
            }
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);
            Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRot * 0.9f);
        }
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (!CutsceneActive || Timer < STAB_TICK)
                return;
            float t = Math.Min((Timer - STAB_TICK) / 70f, 1f);
            float lean = 0.38f * EaseOut(t) * Player.direction;
            lean += (float)Math.Sin(Timer * 0.5f) * 0.015f * t;
            drawInfo.rotation += lean;
            drawInfo.rotationOrigin = new Vector2(Player.width / 2f, Player.height);
        }
        public override void ModifyScreenPosition()
        {
            if (Player.whoAmI != Main.myPlayer) return;

            HarakiriCutscenePlayer activeCP = null;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    var cp = Main.player[i].GetModPlayer<HarakiriCutscenePlayer>();
                    if (cp.CutsceneActive)
                    {
                        activeCP = cp;
                        break;
                    }
                }
            }

            if (activeCP == null) return;

            float pan = activeCP.CameraPan();
            if (pan > 0f)
            {
                Vector2 target = activeCP.PortalPos - new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
                Main.screenPosition = Vector2.Lerp(Main.screenPosition, target, pan);
            }
            float intensity = activeCP.ShakeIntensity();
            if (activeCP.stabShake > 0.1f)
            {
                intensity += activeCP.stabShake;
                activeCP.stabShake *= 0.92f;
            }
            if (intensity <= 0f)
                return;
            Main.screenPosition += new Vector2(Main.rand.NextFloat(-intensity, intensity), Main.rand.NextFloat(-intensity, intensity));
        }
        private float CameraPan()
        {
            const float MAX = 0.85f;
            if (Timer < PORTAL_TICK - 60)
                return 0f;
            if (Timer < PORTAL_TICK)
                return MAX * EaseOut((Timer - (PORTAL_TICK - 60)) / 60f);
            if (Timer < END_TICK - 80)
                return MAX;
            if (Timer < END_TICK)
                return MAX * EaseOut((END_TICK - Timer) / 80f);
            return 0f;
        }
        private float ShakeIntensity()
        {
            if (Timer < RAISE_END)
                return MathHelper.Lerp(0f, 1.2f, Timer / (float)RAISE_END);
            if (Timer < PORTAL_TICK)
                return MathHelper.Lerp(1.5f, 5f, (Timer - RAISE_END) / (float)(PORTAL_TICK - RAISE_END));
            if (Timer < BOSS_OUT_TICK)
                return MathHelper.Lerp(5f, 8f, (Timer - PORTAL_TICK) / (float)(BOSS_OUT_TICK - PORTAL_TICK));
            return MathHelper.Lerp(7f, 0f, (Timer - BOSS_OUT_TICK) / (float)(END_TICK - BOSS_OUT_TICK));
        }
        public float FlashAlpha() => MathHelper.Clamp(flash, 0f, 1f);
        public float DarknessAlpha()
        {
            if (!CutsceneActive || Timer < STAB_TICK)
                return 0f;
            if (Timer < PORTAL_TICK)
                return MathHelper.Lerp(0f, 0.45f, (Timer - STAB_TICK) / (float)(PORTAL_TICK - STAB_TICK));
            if (Timer < BOSS_OUT_TICK)
                return 0.45f + 0.08f * (float)Math.Sin(Timer * 0.2f);
            return MathHelper.Lerp(0.45f, 0f, (Timer - BOSS_OUT_TICK) / (float)(END_TICK - BOSS_OUT_TICK));
        }
        public float LetterboxProgress()
        {
            if (!CutsceneActive)
                return 0f;
            if (Timer < 60)
                return Timer / 60f;
            if (Timer > END_TICK - 60)
                return (END_TICK - Timer) / 60f;
            return 1f;
        }
        internal static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
    }
}
