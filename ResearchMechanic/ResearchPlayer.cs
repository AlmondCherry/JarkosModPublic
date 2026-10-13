using System;
using System.Collections.Generic;
using System.Linq;
using jarkos.core.Data;
using jarkos.cradle_biome;
using jarkos.NetPackets;
using Jarkos;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.core.PlayerInteract
{
    public class ResearchPlayer : ModPlayer
    {
        public string ActiveResearchID => ResearchSystem.GlobalActiveResearchID;
        public Dictionary<int, int> ActivePuzzleProgress => ResearchSystem.GlobalActivePuzzleProgress;

        public readonly HashSet<int> DiscoveredEssences = new HashSet<int>(); 
        public readonly Dictionary<EssenceType, int> Essences = new(); 
        public readonly Dictionary<EssenceType, int> EssenceCap = new();

        public const float AbortRefundRatio = 0.5f;

        public int GetEssence(EssenceType t) => Essences.TryGetValue(t, out var v) ? v : 0;
        public int GetCap(EssenceType t) => EssenceCap.TryGetValue(t, out var v) ? v : int.MaxValue;

        private bool _cheatBackupActive = false;

        private readonly Dictionary<EssenceType, int> _savedEssencesBackup = new(); 
        private readonly HashSet<int> _savedDiscoveredBackup = new(); 

        public int umbraDodgeCooldown = 0;
        private int _pyrosTrailTimer = 0;

        public override void PostUpdate()
        {
            UpdateCheatState();

            if (umbraDodgeCooldown > 0) umbraDodgeCooldown--;

            if (IsCompleted("jarkos:PyrosNode8") && Player.velocity.Y == 0f && Math.Abs(Player.velocity.X) > 2f)
            {
                _pyrosTrailTimer++;
                if (_pyrosTrailTimer >= 8)
                {
                    _pyrosTrailTimer = 0;
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        Projectile.NewProjectile(Player.GetSource_Misc("PyrosTrail"), Player.Bottom - new Vector2(0, 8), Vector2.Zero, ModContent.ProjectileType<projectiles.PyrosFireTrailProj>(), 15, 0f, Player.whoAmI);
                    }
                }
            }
        }

        private void UpdateCheatState()
        {
            bool cheatEnabled = Jarkos_config.Instance != null && Jarkos_config.Instance.UnlockAllEssences;

            if (cheatEnabled && !_cheatBackupActive)
            {
                _savedEssencesBackup.Clear();

                foreach (var kvp in Essences)
                {
                    _savedEssencesBackup[kvp.Key] = kvp.Value;
                }

                _savedDiscoveredBackup.Clear();

                foreach (var id in DiscoveredEssences)
                {
                    _savedDiscoveredBackup.Add(id);
                }
                _cheatBackupActive = true;

                foreach (EssenceType EssenceType in System.Enum.GetValues(typeof(EssenceType)))
                {
                    Essences[EssenceType] = 1000000;
                    DiscoveredEssences.Add((int)EssenceType);
                }
            }
            else if (!cheatEnabled && _cheatBackupActive)
            {
                Essences.Clear();
                foreach (var kvp in _savedEssencesBackup)
                {
                    Essences[kvp.Key] = kvp.Value;
                }
                DiscoveredEssences.Clear();
                foreach (var id in _savedDiscoveredBackup)
                {
                    DiscoveredEssences.Add(id);
                }

                _savedEssencesBackup.Clear();
                _savedDiscoveredBackup.Clear();
                _cheatBackupActive = false;
            }
            else if (cheatEnabled && _cheatBackupActive)
            {
                foreach (EssenceType et in System.Enum.GetValues(typeof(EssenceType)))
                {
                    Essences[et] = 1000000;
                    DiscoveredEssences.Add((int)et);
                }
            }
        }

        public void AddEssence(EssenceType t, int amount)
        {
            if (!System.Enum.IsDefined(typeof(EssenceType), t)) return;

            if (_cheatBackupActive)
            {
                if (amount > 0) _savedDiscoveredBackup.Add((int)t);

                int currentSaved = _savedEssencesBackup.TryGetValue(t, out var sv) ? sv : 0;
                int nextSaved = currentSaved + amount;

                _savedEssencesBackup[t] = nextSaved < 0 ? 0 : nextSaved;
                return;
            }

            if (amount > 0) DiscoveredEssences.Add((int)t);

            int next = GetEssence(t) + amount;
            int cap = GetCap(t);

            Essences[t] = next < 0 ? 0 : (next > cap ? cap : next);

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                this.SyncEssencess();
            }
        }
        public bool IsResearched(string id) => ResearchSystem.GlobalCompleted.Contains(id);

        public bool IsCompleted(string id)
        {
            return ResearchSystem.GlobalCompleted.Contains(id);
        }
        public bool HasActive => ActiveResearchID != null;
        public void CompleteResearch(string id)
        {
            if (!ConstellationRegistry.TryGet(id, out var entry)) return;
            ResearchSystem.GlobalCompleted.Add(id);

            if (ActiveResearchID == id)
            {
                ResearchSystem.GlobalActiveResearchID = null;
                ResearchSystem.GlobalActivePuzzleProgress.Clear();
            }

            
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                this.CompleteResearchSync(id);
            }
            entry.OnUnlock(Player);
        }
        public void SetPuzzleProgress(int nodeId, int type)
        {
            ResearchSystem.GlobalActivePuzzleProgress[nodeId] = type;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                this.SetPuzzleProgressSync(nodeId, type);
            }
        }
        public void StartResearch(string id)
        {
            if (HasActive) return;
            ResearchSystem.GlobalActiveResearchID = id;
            ResearchSystem.GlobalActivePuzzleProgress.Clear();

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                this.StartResearchSync(id);
            }
        }
        public void AbortResearch()
        {
            ResearchSystem.GlobalActiveResearchID = null;
            ResearchSystem.GlobalActivePuzzleProgress.Clear();

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)JarkosPacketType.AbortResearchGlobal);
                packet.Send();
            }
        }
        public override void UpdateEquips()
        {
            float pyrosRegenBoost = 0f;
            float pyrosMeleeBoost = 0f;

            if (IsCompleted("jarkos:PyrosNode1"))
            {
                pyrosRegenBoost += 0.2f;
                pyrosMeleeBoost += 0.02f;
            }

            if (IsCompleted("jarkos:PyrosSub1Node1")) { pyrosRegenBoost += 0.4f; pyrosMeleeBoost += 0.03f; }
            if (IsCompleted("jarkos:PyrosSub1Node2")) { pyrosRegenBoost += 0.8f; pyrosMeleeBoost += 0.05f; }
            if (IsCompleted("jarkos:PyrosSub1Node3")) { pyrosRegenBoost += 1.0f; pyrosMeleeBoost += 0.05f; }

            Player.lifeRegen += (int)Math.Round(pyrosRegenBoost);

            Player.GetDamage(DamageClass.Melee) += pyrosMeleeBoost;

            if (IsCompleted("jarkos:PyrosNode6"))
            {
                Player.lifeRegen += 1;
                if (Player.ZoneUnderworldHeight)
                {
                    Player.lifeRegen += 2;
                }
            }
            float vitaRegenBoost = 0f;
            if (IsCompleted("jarkos:VitaNode1"))
            {
                vitaRegenBoost += 1f;
            }
            if (IsCompleted("jarkos:VitaSub1Node1")) vitaRegenBoost += 0.6f;
            if (IsCompleted("jarkos:VitaSub1Node2")) vitaRegenBoost += 0.6f;
            if (IsCompleted("jarkos:VitaSub1Node3")) vitaRegenBoost += 1.0f;
            if (IsCompleted("jarkos:VitaSub1Node4")) vitaRegenBoost += 1.5f;

            Player.lifeRegen += (int)Math.Round(vitaRegenBoost);
            
            if (IsCompleted("jarkos:AquaSub1Node3"))
            {
                Player.luck += 0.10f;
            }
            if (IsCompleted("jarkos:VenenumSub1Node2"))
            {
                Player.GetDamage(DamageClass.Ranged) += 0.04f;
            }
            if (IsCompleted("jarkos:VitaNode8"))
            {
                Player.statLifeMax2 += 10;
            }
            if (IsCompleted("jarkos:VitaNode5"))
            {
                Player.lifeRegen += 2;
            }
            if (IsCompleted("jarkos:VitaNode6") && Main.dayTime)
            {
                if (Player.lifeRegen > 0) Player.lifeRegen = (int)(Player.lifeRegen * 1.10f) + 1;
                Player.manaRegenBonus += (int)(Player.manaRegen * 0.10f) + 1;
            }
            if (IsCompleted("jarkos:UmbraNode5"))
            {
                Player.GetDamage(DamageClass.Summon) += 0.10f;
                Player.GetDamage(DamageClass.Magic) += 0.10f;
            }
            if (IsCompleted("jarkos:UmbraNode6") && (Player.ZoneDirtLayerHeight || Player.ZoneRockLayerHeight || Player.ZoneUnderworldHeight))
            {
                Player.statDefense += 10;
            }
            if (IsCompleted("jarkos:UmbraNode7") && !Main.dayTime)
            {
                Player.GetDamage(DamageClass.Generic) += 0.05f;
            }
            if (IsCompleted("jarkos:UmbraNode1"))
            {
                if (Player.ZoneCorrupt || Player.ZoneCrimson || Player.InModBiome<PetrifiedAbyssBiome>())
                {
                    float umbraMult = 1f;
                    if (IsCompleted("jarkos:UmbraSub1Node1")) umbraMult += 0.50f;
                    if (IsCompleted("jarkos:UmbraSub1Node2")) umbraMult += 0.75f;
                    if (IsCompleted("jarkos:UmbraSub1Node3")) umbraMult += 1.00f;
                    Player.statDefense += (int)(1f * umbraMult);
                    Player.endurance += 0.005f * umbraMult;
                }
            }
        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            if (IsCompleted("jarkos:VitaNode8"))
            {
                healValue = (int)(healValue * 1.25f);
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (IsCompleted("jarkos:PyrosNode7") && Player.ZoneUnderworldHeight)
            {
                modifiers.FinalDamage *= 0.95f;
            }
        }
    
        public override void OnHurt(Player.HurtInfo info)
        {
            
            if (IsCompleted("jarkos:VitaNode7") && info.DamageSource.SourceNPCIndex >= 0 && info.DamageSource.SourceNPCIndex < Main.maxNPCs)
            {
                NPC attacker = Main.npc[info.DamageSource.SourceNPCIndex];

                if (attacker != null && attacker.active)
                {
                    int reflectDmg = (int)(info.Damage * 0.10f);
                    if (reflectDmg < 1) reflectDmg = 1;

                    NPC.HitInfo hit = new NPC.HitInfo
                    {
                        Damage = reflectDmg,
                        Knockback = 0f,
                        HitDirection = info.HitDirection,
                        Crit = false
                    };

                    attacker.StrikeNPC(hit);
                }
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (IsCompleted("jarkos:UmbraNode8") && umbraDodgeCooldown <= 0)
            {
                umbraDodgeCooldown = 1200;
                SoundEngine.PlaySound(SoundID.Item8, Player.Center);
                for (int i = 0; i < 15; i++)
                {
                    Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Shadowflame, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.3f);
                    d.noGravity = true;
                }
                return true;
            }
            return base.FreeDodge(info);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (IsCompleted("jarkos:VenenumNode5") && modifiers.DamageType.CountsAsClass(DamageClass.Ranged))
            {
                modifiers.CritDamage *= 2.0f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsCompleted("jarkos:PyrosNode6") && Main.rand.NextFloat() < 0.05f)
            {
                target.AddBuff(BuffID.OnFire3, 180);
            }

            if (IsCompleted("jarkos:VenenumNode8"))
            {
                target.AddBuff(ModContent.BuffType<buffs.debuff.WitherDebuff>(), 300);
            }
        }
        public override void UpdateBadLifeRegen()
        {
            if (IsCompleted("jarkos:PyrosNode1"))
            {
                if (Player.HasBuff(BuffID.OnFire))
                    Player.lifeRegen += 4;
                if (Player.HasBuff(BuffID.OnFire3))
                    Player.lifeRegen += 4;
                if (Player.HasBuff(BuffID.Burning))
                    Player.lifeRegen += 4;
                if (Player.HasBuff(BuffID.Frostburn))
                    Player.lifeRegen += 4;
            }

            int poisonWeakenLevel = 0;
            
            if (IsCompleted("jarkos:VenenumNode2"))
                poisonWeakenLevel = 2;
            else if (IsCompleted("jarkos:VenenumNode1"))
                poisonWeakenLevel = 1;
            if (poisonWeakenLevel > 0)
            {
                if (Player.HasBuff(BuffID.Poisoned))
                    Player.lifeRegen += (poisonWeakenLevel == 2) ? 4 : 2;
                if (Player.HasBuff(BuffID.Venom))
                    Player.lifeRegen += (poisonWeakenLevel == 2) ? 10 : 4;
            }
            if (IsCompleted("jarkos:VenenumSub1Node1"))
            {
                if (Player.HasBuff(BuffID.Poisoned) || Player.HasBuff(BuffID.Venom))
                    Player.lifeRegen += 2;
            }
            if (IsCompleted("jarkos:VenenumSub1Node3"))
            {
                if (Player.HasBuff(BuffID.Poisoned) || Player.HasBuff(BuffID.Venom))
                    Player.lifeRegen += 4;
            }
        }
        public override void SaveData(TagCompound tag)
        {
            var targetEss = _cheatBackupActive ? _savedEssencesBackup : Essences;
            var targetDisc = _cheatBackupActive ? _savedDiscoveredBackup : DiscoveredEssences;

            var keys = targetEss.Keys.Select(k => (int)k).ToList();
            var vals = targetEss.Values.ToList();
            tag["essKeys"] = keys;
            tag["essVals"] = vals;
            tag["discoveredEssences"] = targetDisc.ToList();
            tag["cheatBackupActive"] = _cheatBackupActive;
            if (_cheatBackupActive)
            {
                tag["savedEssKeys"] = _savedEssencesBackup.Keys.Select(k => (int)k).ToList();
                tag["savedEssVals"] = _savedEssencesBackup.Values.ToList();
                tag["savedDiscovered"] = _savedDiscoveredBackup.ToList();
            }
        }
        public override void LoadData(TagCompound tag)
        {
            Essences.Clear();
            DiscoveredEssences.Clear();
            _savedEssencesBackup.Clear();
            _savedDiscoveredBackup.Clear();
            _cheatBackupActive = tag.GetBool("cheatBackupActive");

            if (_cheatBackupActive)
            {
                if (tag.ContainsKey("savedEssKeys"))
                {
                    var keys = tag.GetList<int>("savedEssKeys");
                    var vals = tag.GetList<int>("savedVals");
                    if (vals.Count == 0 && tag.ContainsKey("savedEssVals")) vals = tag.GetList<int>("savedEssVals");
                    for (int i = 0; i < keys.Count && i < vals.Count; i++)
                    {
                        EssenceType et = (EssenceType)keys[i];
                        if (System.Enum.IsDefined(typeof(EssenceType), et) && vals[i] > 0)
                            _savedEssencesBackup[et] = vals[i];
                    }
                }
                if (tag.ContainsKey("savedDiscovered"))
                {
                    foreach (var i in tag.GetList<int>("savedDiscovered"))
                    {
                        if (System.Enum.IsDefined(typeof(EssenceType), (EssenceType)i))
                            _savedDiscoveredBackup.Add(i);
                    }
                }
            }
            else
            {
                if (tag.ContainsKey("essKeys"))
                {
                    var keys = tag.GetList<int>("essKeys");
                    var vals = tag.GetList<int>("essVals");
                    for (int i = 0; i < keys.Count && i < vals.Count; i++)
                    {
                        EssenceType et = (EssenceType)keys[i];
                        if (System.Enum.IsDefined(typeof(EssenceType), et) && vals[i] > 0)
                        {
                            Essences[et] = vals[i];
                        }
                    }
                }
                if (tag.ContainsKey("discoveredEssences"))
                {
                    foreach (var i in tag.GetList<int>("discoveredEssences"))
                    {
                        if (System.Enum.IsDefined(typeof(EssenceType), (EssenceType)i))
                            DiscoveredEssences.Add(i);
                    }
                }
            }

            UpdateCheatState();
        }
    }
}
