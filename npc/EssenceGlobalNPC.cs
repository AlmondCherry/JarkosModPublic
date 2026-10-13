using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.core.Data;
using jarkos.projectiles.other;
using jarkos.npc.hell_mobs;
using jarkos.core.PlayerInteract;
using jarkos.buffs;
using System;

namespace jarkos.npc
{
    public class EssenceGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.friendly || npc.lifeMax <= 5 || npc.SpawnedFromStatue) return;

            int killerIndex = npc.lastInteraction;
            
            if (killerIndex < 0 || killerIndex >= Main.maxPlayers || !Main.player[killerIndex].active || Main.player[killerIndex].dead)
            {
                killerIndex = Player.FindClosest(npc.position, npc.width, npc.height);
            }

            if (killerIndex >= 0 && killerIndex < Main.maxPlayers && Main.player[killerIndex].active && !Main.player[killerIndex].dead)
            {
                Player killer = Main.player[killerIndex];

                float dropMult = Jarkos_config.Instance != null ? Jarkos_config.Instance.EssenceDropMultiplier : 1.0f;
                float globalDropChance = Main.rand.NextFloat(0.02f, 0.05f) * dropMult;
                if (killer.HasBuff(ModContent.BuffType<buffs.EtherTinctureBuff>()))
                {
                    globalDropChance *= 2.5f;
                }
                if (Main.rand.NextFloat() < globalDropChance)
                {
                    EssenceType[] basicEssences = new EssenceType[] { EssenceType.Aer, EssenceType.Aqua, EssenceType.Vita, EssenceType.Terra };
                    EssenceType chosenEssence = basicEssences[Main.rand.Next(basicEssences.Length)];
                    int amount = Main.rand.Next(1, 3);
                    SpawnEssenceOrbs(npc, killer, chosenEssence, amount);
                }
            }

            bool isBoss = npc.boss || npc.type == NPCID.LunarTowerSolar || npc.type == NPCID.LunarTowerVortex || npc.type == NPCID.LunarTowerNebula || npc.type == NPCID.LunarTowerStardust || npc.type == NPCID.MoonLordCore || npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail;
            List<int> playersToReward = new List<int>();
            if (isBoss)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active && !Main.player[i].dead && npc.playerInteraction[i])
                    {
                        playersToReward.Add(i);
                    }
                }
            }
            if (playersToReward.Count == 0)
            {
                if (killerIndex >= 0 && killerIndex < Main.maxPlayers && Main.player[killerIndex].active && !Main.player[killerIndex].dead)
                    playersToReward.Add(killerIndex);
            }
            if (playersToReward.Count == 0) return;

            Player target1 = Main.player[playersToReward[0]];
            List<EssenceType> possibleDrops = new List<EssenceType>();

            if (npc.type == NPCID.BlueSlime || npc.type == NPCID.Zombie || npc.type == NPCID.IceSlime || npc.type == NPCID.SandSlime || npc.type == NPCID.JungleSlime || npc.type == NPCID.SpikedIceSlime || npc.type == NPCID.SpikedJungleSlime || npc.type == NPCID.ZombieRaincoat || npc.type == NPCID.ZombieEskimo || npc.type == NPCID.ZombieMushroom || npc.type == NPCID.ArmedZombie || npc.type == NPCID.ArmedZombieEskimo || npc.type == NPCID.PincushionZombie || npc.type == NPCID.BloodZombie || npc.type == NPCID.MaggotZombie || npc.type == NPCID.Zombie || npc.type == NPCID.MotherSlime || npc.type == NPCID.CorruptSlime || npc.type == NPCID.Crimslime)
            {
                possibleDrops.Add(EssenceType.Terra);
                possibleDrops.Add(EssenceType.Vita);
            }
            if (npc.type == NPCID.DemonEye || npc.type == NPCID.CaveBat || npc.type == NPCID.IceBat || npc.type == NPCID.JungleBat || npc.type == NPCID.Hellbat || npc.type == NPCID.GiantFlyingFox || npc.type == NPCID.VampireBat || npc.type == NPCID.Bee || npc.type == NPCID.BeeSmall || npc.type == NPCID.WanderingEye)
            {
                possibleDrops.Add(EssenceType.Aer);
            }
            if (npc.type == NPCID.Crab || npc.type == NPCID.BlueJellyfish || npc.type == NPCID.PinkJellyfish || npc.type == NPCID.GreenJellyfish || target1.ZoneBeach)
            {
                possibleDrops.Add(EssenceType.Aqua);
            }
            if (npc.type == NPCID.LavaSlime || npc.type == NPCID.FireImp || npc.type == NPCID.Demon || npc.type == NPCID.VoodooDemon || npc.type == NPCID.RedDevil || npc.type == NPCID.Lavabat || npc.type == ModContent.NPCType<Hell_priest>())
            {
                possibleDrops.Add(EssenceType.Pyros);
            }
            if (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail || npc.type == NPCID.Corruptor || target1.ZoneCorrupt)
            {
                possibleDrops.Add(EssenceType.Umbra);
            }
            if (npc.type == NPCID.BrainofCthulhu || npc.type == NPCID.Crimera || target1.ZoneCrimson || npc.type == NPCID.Creeper)
            {
                possibleDrops.Add(EssenceType.Cruor);
            }
            if (npc.type == NPCID.Hornet || npc.type == NPCID.ManEater || npc.type == NPCID.Snatcher || target1.ZoneJungle)
            {
                possibleDrops.Add(EssenceType.Venenum);
            }
            if (npc.type == NPCID.Antlion || npc.type == NPCID.FlyingAntlion || npc.type == NPCID.WalkingAntlion || target1.ZoneDesert || target1.ZoneUndergroundDesert)
            {
                possibleDrops.Add(EssenceType.Arena);
            }
            if (npc.type == NPCID.Penguin || npc.type == NPCID.Wolf || target1.ZoneSnow || npc.type == NPCID.IceElemental || npc.type == NPCID.IceGolem)
            {
                possibleDrops.Add(EssenceType.Glacies);
            }
            if (npc.type == NPCID.FungoFish || npc.type == NPCID.SporeBat || target1.ZoneGlowshroom || npc.type == NPCID.MushiLadybug || npc.type == NPCID.AnomuraFungus)
            {
                possibleDrops.Add(EssenceType.Fungus);
            }
            if (npc.type == NPCID.Skeleton || npc.type == NPCID.SkeletronHead || npc.type == NPCID.SkeletronHand || npc.type == NPCID.Ghost || npc.type == NPCID.Wraith || target1.ZoneDungeon)
            {
                possibleDrops.Add(EssenceType.Anima);
            }
            if (npc.type == NPCID.KingSlime)
            {
                possibleDrops.Add(EssenceType.Regalis);
            }
            if (npc.type == NPCID.QueenBee)
            {
                possibleDrops.Add(EssenceType.Acus);
            }
            if (npc.type == NPCID.WallofFlesh || npc.type == NPCID.WallofFleshEye || npc.type == NPCID.TheHungry || npc.type == NPCID.TheHungryII || target1.ZoneUnderworldHeight)
            {
                possibleDrops.Add(EssenceType.Caro);
            }
            if (npc.type == NPCID.Unicorn || npc.type == NPCID.Pixie || target1.ZoneHallow || npc.type == NPCID.Gastropod)
            {
                possibleDrops.Add(EssenceType.Lux);
            }
            if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism || npc.type == NPCID.SkeletronPrime || npc.type == NPCID.PrimeCannon || npc.type == NPCID.PrimeLaser || npc.type == NPCID.PrimeSaw || npc.type == NPCID.PrimeVice || npc.type == NPCID.TheDestroyer || npc.type == NPCID.TheDestroyerBody || npc.type == NPCID.TheDestroyerTail)
            {
                possibleDrops.Add(EssenceType.Mechanus);
            }
            if (npc.type == NPCID.TheDestroyer || npc.type == NPCID.TheDestroyerBody || npc.type == NPCID.TheDestroyerTail || (Main.hardMode && npc.lifeMax > 300 && !npc.boss && Main.rand.NextBool(2)))
            {
                possibleDrops.Add(EssenceType.Fulgur);
            }
            if (npc.type == NPCID.Plantera || npc.type == NPCID.PlanterasHook || npc.type == NPCID.PlanterasTentacle || (Main.hardMode && target1.ZoneJungle))
            {
                possibleDrops.Add(EssenceType.Silva);
            }
            if (npc.type == NPCID.Golem || npc.type == NPCID.GolemFistLeft || npc.type == NPCID.GolemFistRight || npc.type == NPCID.GolemHead || npc.type == NPCID.GolemHeadFree || npc.type == NPCID.Lihzahrd || npc.type == NPCID.FlyingSnake || target1.ZoneLihzhardTemple)
            {
                possibleDrops.Add(EssenceType.Chitinus);
            }
            if (npc.type == NPCID.DukeFishron)
            {
                possibleDrops.Add(EssenceType.Pelagus);
            }
            if (npc.type == NPCID.QueenSlimeBoss || npc.type == NPCID.HallowBoss)
            {
                possibleDrops.Add(EssenceType.Imperium);
            }
            if (npc.type == NPCID.LunarTowerSolar || target1.ZoneTowerSolar || npc.type == NPCID.SolarCrawltipedeBody || npc.type == NPCID.SolarDrakomire || npc.type == NPCID.SolarSolenian)
            {
                possibleDrops.Add(EssenceType.Solar);
            }
            if (npc.type == NPCID.LunarTowerVortex || target1.ZoneTowerVortex || npc.type == NPCID.VortexHornet || npc.type == NPCID.VortexLarva)
            {
                possibleDrops.Add(EssenceType.Vortex);
            }
            if (npc.type == NPCID.LunarTowerNebula || target1.ZoneTowerNebula || npc.type == NPCID.NebulaBrain || npc.type == NPCID.NebulaHeadcrab)
            {
                possibleDrops.Add(EssenceType.Nebula);
            }
            if (npc.type == NPCID.LunarTowerStardust || target1.ZoneTowerStardust || npc.type == NPCID.StardustCellBig || npc.type == NPCID.StardustJellyfishBig)
            {
                possibleDrops.Add(EssenceType.Stellaris);
            }
            if (npc.type == NPCID.MoonLordCore || npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead)
            {
                possibleDrops.Add(EssenceType.Luna);
            }

            List<EssenceType> uniqueDrops = new List<EssenceType>();
            foreach (var e in possibleDrops)
            {
                if (!uniqueDrops.Contains(e))
                {
                    uniqueDrops.Add(e);
                }
            }

            foreach (int pIndex in playersToReward)
            {
                Player target = Main.player[pIndex];
                
                float baseChance = 0.10f;
                var rp = target.GetModPlayer<ResearchPlayer>();

                if (rp.IsCompleted("jarkos:AquaNode1"))
                {
                    baseChance += 0.05f;
                }

                if (rp.IsCompleted("jarkos:AquaSub1Node1"))
                {
                    baseChance += 0.05f;
                }

                if (rp.IsCompleted("jarkos:AquaSub1Node3"))
                {
                    baseChance += 0.05f;
                }

                if (target.HasBuff(ModContent.BuffType<EtherTinctureBuff>()))
                {
                    baseChance *= 2.5f;
                }

                baseChance *= ModContent.GetInstance<Jarkos_config>().EssenceDropMultiplier;

                foreach (var essence in uniqueDrops)
                {
                    if (isBoss || Main.rand.NextFloat() < baseChance)
                    {
                        int amount = isBoss ? Main.rand.Next(3, 5) : Main.rand.Next(1, 3);
                        
                        if (isBoss && rp.IsCompleted("jarkos:AquaSub1Node2"))
                        {
                            amount = (int)Math.Round(amount * 1.7f);
                        }
                        SpawnEssenceOrbs(npc, target, essence, amount);
                    }
                }
            }
        }

        private void SpawnEssenceOrbs(NPC npc, Player target, EssenceType essence, int amount)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            float dropMult = Jarkos_config.Instance != null ? Jarkos_config.Instance.EssenceDropMultiplier : 1.0f;
            int finalAmount = Math.Max(1, (int)Math.Round(amount * dropMult));
            for (int i = 0; i < finalAmount; i++)
            {
                Vector2 spawnVel = Main.rand.NextVector2Circular(8f, 8f);
                Projectile.NewProjectile(npc.GetSource_Death(), npc.Center, spawnVel, ModContent.ProjectileType<EssenceOrbProjectile>(), 0, 0f, Main.myPlayer, (int)essence, target.whoAmI);
            }
        }
    }
}
