using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jarkos;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.systems
{
    public class PlayerFightStats
    {
        public int WhoAmI;
        public string PlayerName;
        public Color ChartColor;
        public long TotalDamageDealt;
        public long TotalDamageTaken;
        public long TotalDamageBlocked;
        public List<float> DamageTimeline = new List<float>();

        public float CurrentSecondDamage;

        public PlayerFightStats(int whoAmI, string name, Color color)
        {
            WhoAmI = whoAmI;
            PlayerName = string.IsNullOrEmpty(name) ? "Player" : name;
            ChartColor = color;
            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
            TotalDamageBlocked = 0;
            CurrentSecondDamage = 0;
            DamageTimeline = new List<float>();
        }
    }

    public class BossFightRecord
    {
        public string BossName;
        public int BossType;
        public int DurationSeconds;
        public DateTime FightTime;
        public Dictionary<int, PlayerFightStats> PlayerStats = new Dictionary<int, PlayerFightStats>();

        public BossFightRecord(string bossName, int bossType)
        {
            BossName = bossName;
            BossType = bossType;
            DurationSeconds = 0;
            FightTime = DateTime.Now;
            PlayerStats = new Dictionary<int, PlayerFightStats>();
        }
    }

    public class BossStatsTrackerSystem : ModSystem
    {
        public static BossFightRecord CurrentFight = null;
        public static BossFightRecord LastCompletedFight = null;

        public static bool IsFightActive => CurrentFight != null;

        private static float _secondAccumulator = 0f;
        private static int _noBossTimer = 0;

        public static readonly Color[] Palette = new Color[]
        {
            new Color(0, 180, 245),
            new Color(245, 60, 60),
            new Color(50, 205, 50),
            new Color(255, 185, 0),
            new Color(180, 80, 240),
            new Color(255, 120, 30),
            new Color(255, 105, 180),
            new Color(0, 220, 180)
        };

        public override void ClearWorld()
        {
            CurrentFight = null;
            LastCompletedFight = null;
            _secondAccumulator = 0f;
            _noBossTimer = 0;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (LastCompletedFight != null)
            {
                TagCompound recordTag = new TagCompound
                {
                    ["BossName"] = LastCompletedFight.BossName,
                    ["BossType"] = LastCompletedFight.BossType,
                    ["DurationSeconds"] = LastCompletedFight.DurationSeconds,
                    ["FightTime"] = LastCompletedFight.FightTime.Ticks
                };

                List<TagCompound> playersList = new List<TagCompound>();
                foreach (var kvp in LastCompletedFight.PlayerStats)
                {
                    PlayerFightStats stats = kvp.Value;
                    if (stats == null) continue;

                    TagCompound pTag = new TagCompound
                    {
                        ["WhoAmI"] = stats.WhoAmI,
                        ["PlayerName"] = stats.PlayerName ?? "Player",
                        ["ColorR"] = stats.ChartColor.R,
                        ["ColorG"] = stats.ChartColor.G,
                        ["ColorB"] = stats.ChartColor.B,
                        ["TotalDamageDealt"] = stats.TotalDamageDealt,
                        ["TotalDamageTaken"] = stats.TotalDamageTaken,
                        ["TotalDamageBlocked"] = stats.TotalDamageBlocked,
                        ["DamageTimeline"] = stats.DamageTimeline.ToList()
                    };
                    playersList.Add(pTag);
                }
                recordTag["Players"] = playersList;
                tag["LastCompletedFight"] = recordTag;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("LastCompletedFight"))
            {
                TagCompound recordTag = tag.GetCompound("LastCompletedFight");
                string bossName = recordTag.GetString("BossName");
                int bossType = recordTag.GetInt("BossType");
                int duration = recordTag.GetInt("DurationSeconds");
                long fightTime = recordTag.GetLong("FightTime");

                BossFightRecord record = new BossFightRecord(bossName, bossType)
                {
                    DurationSeconds = duration,
                    FightTime = new DateTime(fightTime)
                };

                if (recordTag.ContainsKey("Players"))
                {
                    var playersList = recordTag.GetList<TagCompound>("Players");
                    foreach (var pTag in playersList)
                    {
                        int whoAmI = pTag.GetInt("WhoAmI");
                        string pName = pTag.GetString("PlayerName");
                        byte r = pTag.GetByte("ColorR");
                        byte g = pTag.GetByte("ColorG");
                        byte b = pTag.GetByte("ColorB");
                        long dealt = pTag.GetLong("TotalDamageDealt");
                        long taken = pTag.GetLong("TotalDamageTaken");
                        long blocked = pTag.GetLong("TotalDamageBlocked");

                        PlayerFightStats stats = new PlayerFightStats(whoAmI, pName, new Color(r, g, b))
                        {
                            TotalDamageDealt = dealt,
                            TotalDamageTaken = taken,
                            TotalDamageBlocked = blocked
                        };

                        if (pTag.ContainsKey("DamageTimeline"))
                        {
                            stats.DamageTimeline = new List<float>(pTag.GetList<float>("DamageTimeline"));
                        }

                        record.PlayerStats[whoAmI] = stats;
                    }
                }

                LastCompletedFight = record;
            }
            else
            {
                LastCompletedFight = null;
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
        }

        public override void NetReceive(BinaryReader reader)
        {
        }

        public static void WriteRecord(BinaryWriter writer, BossFightRecord record)
        {
            if (record == null)
            {
                writer.Write("");
                writer.Write(0);
                writer.Write(0);
                writer.Write(0L);
                writer.Write(0);
                return;
            }

            writer.Write(record.BossName ?? "");
            writer.Write(record.BossType);
            writer.Write(record.DurationSeconds);
            writer.Write(record.FightTime.Ticks);

            lock (record.PlayerStats)
            {
                writer.Write(record.PlayerStats.Count);
                foreach (var kvp in record.PlayerStats)
                {
                    PlayerFightStats stats = kvp.Value;
                    writer.Write(stats.WhoAmI);
                    writer.Write(stats.PlayerName ?? "Player");
                    writer.Write(stats.ChartColor.R);
                    writer.Write(stats.ChartColor.G);
                    writer.Write(stats.ChartColor.B);
                    writer.Write(stats.TotalDamageDealt);
                    writer.Write(stats.TotalDamageTaken);
                    writer.Write(stats.TotalDamageBlocked);

                    float[] timelineArray;
                    lock (stats.DamageTimeline)
                    {
                        timelineArray = stats.DamageTimeline.ToArray();
                    }

                    writer.Write(timelineArray.Length);
                    for (int i = 0; i < timelineArray.Length; i++)
                    {
                        writer.Write(timelineArray[i]);
                    }
                }
            }
        }

        public static BossFightRecord ReadRecord(BinaryReader reader)
        {
            string bossName = reader.ReadString();
            int bossType = reader.ReadInt32();
            int duration = reader.ReadInt32();
            long fightTimeTicks = reader.ReadInt64();

            BossFightRecord record = new BossFightRecord(bossName, bossType)
            {
                DurationSeconds = duration,
                FightTime = new DateTime(fightTimeTicks)
            };

            int playerStatsCount = reader.ReadInt32();
            for (int p = 0; p < playerStatsCount; p++)
            {
                int whoAmI = reader.ReadInt32();
                string pName = reader.ReadString();
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                long dealt = reader.ReadInt64();
                long taken = reader.ReadInt64();
                long blocked = reader.ReadInt64();

                Color color = new Color(r, g, b);
                PlayerFightStats stats = new PlayerFightStats(whoAmI, pName, color)
                {
                    TotalDamageDealt = dealt,
                    TotalDamageTaken = taken,
                    TotalDamageBlocked = blocked
                };

                int timelineCount = reader.ReadInt32();
                for (int t = 0; t < timelineCount; t++)
                {
                    stats.DamageTimeline.Add(reader.ReadSingle());
                }

                record.PlayerStats[whoAmI] = stats;
            }

            return record;
        }

        public static void SendBossFightRecordPacket(BossFightRecord record, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer || record == null) return;

            try
            {
                ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                packet.Write((byte)JarkosPacketType.SyncBossFightRecord);
                WriteRecord(packet, record);
                packet.Send(toClient, ignoreClient);
            }
            catch { }
        }

        public static void ReceiveBossFightRecordPacket(BinaryReader reader)
        {
            try
            {
                BossFightRecord record = ReadRecord(reader);
                LastCompletedFight = record;

                if (Main.netMode == NetmodeID.Server)
                {
                    SendBossFightRecordPacket(record, -1, -1);
                }
            }
            catch { }
        }

        public override void PostUpdateEverything()
        {
            if (CurrentFight == null) return;


            if (!IsAnyBossAlive())
            {
                _noBossTimer++;

                if (_noBossTimer >= 90)
                {
                    CompleteFight();
                    _noBossTimer = 0;
                    _secondAccumulator = 0f;
                }
                return;
            }

            _noBossTimer = 0;

            _secondAccumulator += 1f / 60f;
            if (_secondAccumulator >= 1.0f)
            {
                _secondAccumulator -= 1.0f;
                CurrentFight.DurationSeconds++;

                foreach (var kvp in CurrentFight.PlayerStats)
                {
                    PlayerFightStats stats = kvp.Value;
                    lock (stats.DamageTimeline)
                    {
                        stats.DamageTimeline.Add(stats.CurrentSecondDamage);
                        stats.CurrentSecondDamage = 0f;
                    }
                }

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    Player localPlayer = Main.LocalPlayer;
                    if (CurrentFight.PlayerStats.TryGetValue(localPlayer.whoAmI, out PlayerFightStats localStats))
                    {
                        int secDamage = 0;
                        lock (localStats.DamageTimeline)
                        {
                            if (localStats.DamageTimeline.Count > 0)
                            {
                                secDamage = (int)localStats.DamageTimeline[localStats.DamageTimeline.Count - 1];
                            }
                        }
                        SendRecordDamagePacket(localPlayer.whoAmI, secDamage);
                    }
                }
            }
        }

        public static bool IsAnyBossAlive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc != null && npc.active && npc.life > 0)
                {
                    if (npc.boss || IsExtraBossType(npc.type) || (npc.realLife >= 0 && Main.npc[npc.realLife].active && (Main.npc[npc.realLife].boss || IsExtraBossType(Main.npc[npc.realLife].type))))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsExtraBossType(int type)
        {
            return type == NPCID.EaterofWorldsHead || type == NPCID.EaterofWorldsBody || type == NPCID.EaterofWorldsTail;
        }

        public static bool IsBossOrPartOrMinion(NPC npc)
        {
            if (npc == null || !npc.active) return false;

            if (npc.boss || IsExtraBossType(npc.type)) return true;

            if (npc.realLife >= 0 && Main.npc[npc.realLife].active && (Main.npc[npc.realLife].boss || IsExtraBossType(Main.npc[npc.realLife].type)))
                return true;

            int type = npc.type;
            if (type == NPCID.Creeper ||
                type == NPCID.TheHungry || type == NPCID.TheHungryII || type == NPCID.LeechHead || type == NPCID.LeechBody || type == NPCID.LeechTail ||
                type == NPCID.ServantofCthulhu ||
                type == NPCID.SkeletronHand ||
                type == NPCID.PrimeCannon || type == NPCID.PrimeSaw || type == NPCID.PrimeVice || type == NPCID.PrimeLaser ||
                type == NPCID.Retinazer || type == NPCID.Spazmatism ||
                type == NPCID.TheDestroyerBody || type == NPCID.TheDestroyerTail || type == NPCID.Probe ||
                type == NPCID.PlanterasHook || type == NPCID.PlanterasTentacle ||
                type == NPCID.GolemHead || type == NPCID.GolemHeadFree || type == NPCID.GolemFistLeft || type == NPCID.GolemFistRight ||
                type == NPCID.Sharkron || type == NPCID.Sharkron2 || type == NPCID.DetonatingBubble ||
                type == NPCID.CultistBoss || type == NPCID.CultistDragonHead || type == NPCID.CultistDragonBody1 || type == NPCID.CultistDragonBody2 || type == NPCID.CultistDragonTail || type == NPCID.AncientLight || type == NPCID.AncientDoom ||
                type == NPCID.MoonLordCore || type == NPCID.MoonLordFreeEye || type == NPCID.MoonLordHand || type == NPCID.MoonLordHead ||
                type == NPCID.SlimeSpiked ||
                type == NPCID.QueenSlimeMinionBlue || type == NPCID.QueenSlimeMinionPink || type == NPCID.QueenSlimeMinionPurple ||
                type == NPCID.MartianSaucerTurret || type == NPCID.MartianSaucerCannon ||
                type == NPCID.BloodSquid ||
                type == NPCID.PirateShip)
            {
                return true;
            }

            if (CurrentFight != null && type == CurrentFight.BossType)
            {
                return true;
            }

            return false;
        }

        public static PlayerFightStats GetOrCreatePlayerStats(Player player)
        {
            if (CurrentFight == null || player == null || !player.active) return null;

            if (!CurrentFight.PlayerStats.TryGetValue(player.whoAmI, out PlayerFightStats stats))
            {
                Color color = Palette[CurrentFight.PlayerStats.Count % Palette.Length];
                stats = new PlayerFightStats(player.whoAmI, player.name, color);

                for (int s = 0; s < CurrentFight.DurationSeconds; s++)
                {
                    stats.DamageTimeline.Add(0f);
                }

                CurrentFight.PlayerStats[player.whoAmI] = stats;
            }
            return stats;
        }

        public static void StartFight(NPC bossNPC)
        {
            if (CurrentFight != null || bossNPC == null) return;

            string bossName = string.IsNullOrEmpty(bossNPC.GivenName) ? bossNPC.TypeName : bossNPC.GivenName;
            CurrentFight = new BossFightRecord(bossName, bossNPC.type);
            _noBossTimer = 0;
            _secondAccumulator = 0f;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player != null && player.active)
                {
                    GetOrCreatePlayerStats(player);
                }
            }
        }

        public static void RecordDamageDealt(Player player, NPC npc, int damage)
        {
            if (player == null || !player.active || damage <= 0) return;

            if (CurrentFight == null)
            {
                if (npc != null && (npc.boss || IsExtraBossType(npc.type)))
                {
                    StartFight(npc);
                }
                else
                {
                    return;
                }
            }

            if (npc != null && !IsBossOrPartOrMinion(npc))
            {
                return;
            }

            PlayerFightStats stats = GetOrCreatePlayerStats(player);
            if (stats != null)
            {
                stats.TotalDamageDealt += damage;
                stats.CurrentSecondDamage += damage;
            }
        }

        public static void RecordDamageTakenAndBlocked(Player player, int damageTaken, int damageBlocked)
        {
            if (CurrentFight == null || player == null || !player.active) return;

            PlayerFightStats stats = GetOrCreatePlayerStats(player);
            if (stats != null)
            {
                stats.TotalDamageTaken += damageTaken;
                stats.TotalDamageBlocked += damageBlocked;
            }
        }

        public static void CompleteFight()
        {
            if (CurrentFight == null) return;

            foreach (var kvp in CurrentFight.PlayerStats)
            {
                PlayerFightStats stats = kvp.Value;
                lock (stats.DamageTimeline)
                {
                    stats.DamageTimeline.Add(stats.CurrentSecondDamage);
                    stats.CurrentSecondDamage = 0f;
                }
            }

            if (CurrentFight.PlayerStats.Count > 0)
            {
                int maxCount = 0;
                foreach (var kvp in CurrentFight.PlayerStats)
                {
                    if (kvp.Value.DamageTimeline.Count > maxCount)
                    {
                        maxCount = kvp.Value.DamageTimeline.Count;
                    }
                }
                CurrentFight.DurationSeconds = Math.Max(1, maxCount);
            }

            LastCompletedFight = CurrentFight;
            CurrentFight = null;
            _noBossTimer = 0;
            _secondAccumulator = 0f;

            if (Main.netMode == NetmodeID.Server)
            {
                SendBossFightRecordPacket(LastCompletedFight, -1, -1);
            }
        }

        public static void SendRecordDamagePacket(int playerWhoAmI, int damage)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;

            try
            {
                ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                packet.Write((byte)JarkosPacketType.RecordBossDamage);
                packet.Write((byte)playerWhoAmI);
                packet.Write((int)damage);
                packet.Send();
            }
            catch { }
        }

        public static void SendRecordHurtPacket(int playerWhoAmI, int damageTaken, int damageBlocked)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;

            try
            {
                ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                packet.Write((byte)JarkosPacketType.RecordBossHurt);
                packet.Write((byte)playerWhoAmI);
                packet.Write(damageTaken);
                packet.Write(damageBlocked);
                packet.Send();
            }
            catch { }
        }
    }

    public class BossStatsGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void AI(NPC npc)
        {
            if (npc.active && npc.boss && BossStatsTrackerSystem.CurrentFight == null)
            {
                BossStatsTrackerSystem.StartFight(npc);
            }
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (BossStatsTrackerSystem.IsBossOrPartOrMinion(npc))
            {
                if (!npc.friendly && player != null && player.active)
                {
                    int damage = damageDone > 0 ? damageDone : hit.Damage;
                    BossStatsTrackerSystem.RecordDamageDealt(player, npc, damage);
                }
            }
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (BossStatsTrackerSystem.IsBossOrPartOrMinion(npc))
            {
                if (!npc.friendly && projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
                {
                    Player player = Main.player[projectile.owner];
                    if (player != null && player.active)
                    {
                        int damage = damageDone > 0 ? damageDone : hit.Damage;
                        BossStatsTrackerSystem.RecordDamageDealt(player, npc, damage);
                    }
                }
            }
        }

        public override void OnKill(NPC npc)
        {
            if (BossStatsTrackerSystem.IsFightActive && (npc.boss || BossStatsTrackerSystem.IsExtraBossType(npc.type)))
            {
                if (!BossStatsTrackerSystem.IsAnyBossAlive())
                {
                    BossStatsTrackerSystem.CompleteFight();
                }
            }
        }
    }

    public class BossStatsGlobalPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.Server && BossStatsTrackerSystem.LastCompletedFight != null)
            {
                BossStatsTrackerSystem.SendBossFightRecordPacket(BossStatsTrackerSystem.LastCompletedFight, Player.whoAmI, -1);
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (BossStatsTrackerSystem.IsFightActive && Player != null && Player.active)
            {
                int damageTaken = Math.Max(0, info.Damage);
                int damageBlocked = Math.Max(0, info.SourceDamage - info.Damage);

                BossStatsTrackerSystem.RecordDamageTakenAndBlocked(Player, damageTaken, damageBlocked);
                if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
                {
                    BossStatsTrackerSystem.SendRecordHurtPacket(Player.whoAmI, damageTaken, damageBlocked);
                }
            }
        }
    }
}
