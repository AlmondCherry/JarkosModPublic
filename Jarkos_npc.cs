using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using jarkos.items.weapons.magic.staffs;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using jarkos.items.weapons.magic.scrolls;
using jarkos.items.materials;
using jarkos.items.usable;
using jarkos.items.accs.ranged;
using jarkos.tiles;
using jarkos.systems;
using jarkos.items.accs.special;
using jarkos.items.accs.summon;
using jarkos.items.accs.mythical;
using jarkos.achievements;
using jarkos.projectiles.mecha;
using jarkos.projectiles.summon.whip;
using jarkos.items.weapons.summon.staff.PhantomDagger;
using jarkos.particles;
using jarkos.items.accs.amulets;
using System.Collections.Generic;
using jarkos.cradle_biome;
using Terraria.Audio;

namespace jarkos
{
    public class KilledMechBosses : IItemDropRuleCondition, IProvideItemConditionDescription
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return
            info.player.ZoneSnow
            && NPC.downedMechBoss1
            && NPC.downedMechBoss2
            && NPC.downedMechBoss3;
        }
	    public bool CanShowItemDropInUI() => true;
	    public string GetConditionDescription() => null;
    }
    public class BloodMoon : IItemDropRuleCondition, IProvideItemConditionDescription
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return
            Main.bloodMoon;
        }
	    public bool CanShowItemDropInUI() => true;
	    public string GetConditionDescription() => null;
    }
    public class NightTime : IItemDropRuleCondition, IProvideItemConditionDescription
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            NPC npc = info.npc;
            return
            !Main.dayTime && npc.lifeMax > 30;
        }
	    public bool CanShowItemDropInUI() => true;
	    public string GetConditionDescription() => null;
    }
    public class VileplasmDropCondition : IItemDropRuleCondition, IProvideItemConditionDescription
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return DownedBossSystem.killedPunisher && info.player.InModBiome<PetrifiedAbyssBiome>() && info.player.Center.Y > Main.rockLayer * 16f;
        }
        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => "Drops from mobs in the Underground Cradle after killing the Punisher";
    }
    public class Jarkos_npc : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool LB_Penta_target = false;
        public int hits_by_magic_projectiles = 0;
        public float timer_before_burst = 0f;
        public float cooldown_shadow_pact = 0f;
        public bool beer_debuff = false;
        public bool Hypothermia_debuff = false;
        public bool shadow_pact_debuff = false;
        public bool Seal_of_shadow_debuff = false;
        public int hits_in_SP_debuff = 0;
        public bool frag_weakness_debuff = false;
        public int damage_done_global;
        public bool shadow_pact_debuff_already_used = false;
        public int purple_mark_time = 0;
        public int cyan_mark_time = 0;
        public int cyan_projectile_damage = 80;
        public int purple_projectile_damage = 160;
        public override void ResetEffects(NPC npc)
        {
            beer_debuff = false;
            Hypothermia_debuff = false;
            shadow_pact_debuff = false;
            Seal_of_shadow_debuff = false;
            frag_weakness_debuff = false;
            cooldown_shadow_pact++;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (Hypothermia_debuff && !npc.boss)
            {
                npc.velocity *= 0.5f;
            }
            if (Seal_of_shadow_debuff && !npc.boss)
            {
                npc.velocity *= 0.85f;
            }
            if (frag_weakness_debuff)
            {
                damage = damage_done_global;
                if(damage < damage_done_global)
                    damage = damage_done_global;
            }
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (beer_debuff)
            {
                modifiers.Defense *= 0.25f;
            }
            if (shadow_pact_debuff)
            {
                modifiers.Defense *= 4.75f;
            }
        }
        public override void SetDefaults(NPC entity)
        {
            if (Jarkos_worldmodes.brokenpotion_buff == true)
            {
                entity.lifeMax -= entity.lifeMax / 2;
            }
            if (Jarkos_worldmodes.furiousfire_buff == true)
            {
                entity.lifeMax *= 2;
                entity.defense += entity.defense * 35 / 100;
                entity.damage += entity.damage * 5 / 100;
            }
            if (Jarkos_worldmodes.fragility_buff == true)
            {
                entity.damage += entity.damage * 75 / 100;
            }
        }
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (IsAnyBossAlive())
            {
                spawnRate = 0;
                maxSpawns = 0;
                return;
            }
            if (player.GetModPlayer<BranchMadnessPlayer>().amulet8spawnrate == true)
            {
                spawnRate /= 25;
            }
            if (Jarkos_worldmodes.brokenpotion_buff == true)
            {
                spawnRate /= 50000;
            }
        }
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (IsAnyBossAlive())
            {
                pool.Clear();
                return;
            }
            if (spawnInfo.Player.InModBiome<PetrifiedAbyssBiome>())
            {
                var keys = new List<int>(pool.Keys);
                foreach (int key in keys)
                {
                    if (key == NPCID.Mimic || key == NPCID.IceMimic || key == NPCID.BigMimicCorruption || key == NPCID.BigMimicCrimson || key == NPCID.BigMimicHallow || key == NPCID.BigMimicJungle)
                    {
                        continue;
                    }
                    ModNPC modNPC = NPCLoader.GetNPC(key);
                    if (modNPC != null && modNPC.Mod == Mod)
                    {
                        continue;
                    }
                    pool.Remove(key);
                }
            }
        }
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.ByCondition(new VileplasmDropCondition(), ModContent.ItemType<Vileplasm>(), 15, 1, 1));

            if (npc.type == NPCID.DemonEye || npc.type == NPCID.DemonEye2 || npc.type == NPCID.DemonEyeOwl || npc.type == NPCID.DemonEyeOwl || npc.type == NPCID.DemonEyeSpaceship)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EnchantedFragment>(), 1, 1, 2));
            }
            if (npc.type == NPCID.Bunny || npc.type == NPCID.BunnyXmas || npc.type == NPCID.BunnySlimed || npc.type == NPCID.PartyBunny)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FragmentOfNature>(), 2, 2, 3));
            }
            if (npc.type == NPCID.Bird || npc.type == NPCID.BirdBlue || npc.type == NPCID.BirdRed)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FragmentOfNature>(), 1, 1, 3));
            }
            if (npc.type == NPCID.Duck || npc.type == NPCID.Duck2 || npc.type == NPCID.DuckWhite || npc.type == NPCID.DuckWhite2)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FragmentOfNature>(), 1, 2, 4));
            }
            if (npc.type == NPCID.Squirrel || npc.type == NPCID.SquirrelRed)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FragmentOfNature>(), 3, 4, 6));
            }
            if (npc.type == NPCID.Owl)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FragmentOfNature>(), 3, 4, 6));
            }
            if (!npc.boss)
            {
                npcLoot.Add(ItemDropRule.ByCondition(new NightTime(), ModContent.ItemType<PulsatingFragment>(), 18, 1, 1));
            }

            if (npc.boss && npc.type is not (NPCID.Creeper or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FragmentOfFear>(), 1, 5, 10));
            }

            if(NPC.downedBoss2 && npc.boss && npc.type is not (NPCID.Creeper or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail))
            {
                
            }

            if (!npc.boss)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ChargerHealth>(), 3500, 1, 1));
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EssenceOfTigerDrop>(), 4500, 1, 1));
            }

            if (npc.boss)
            {
                npc.value *= 5000;
            }

            if (Jarkos_config.Instance == null || Jarkos_config.Instance.MoreVanillaLoot)
            {
                if (npc.type == NPCID.KingSlime)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemID.Terragrim, 2, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.Code1, 2, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.StickyGrenade, 1, 5, 15));
                    npcLoot.Add(ItemDropRule.Common(ItemID.SlimeStaff, 2, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.EnchantedBoomerang, 2, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.PinkGel, 1, 5, 10));
                }
                if (npc.type == NPCID.EyeofCthulhu)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemID.TissueSample, 1, 15, 25));
                    npcLoot.Add(ItemDropRule.Common(ItemID.CrimtaneOre, 1, 45, 125));
                    npcLoot.Add(ItemDropRule.Common(ItemID.ShadowScale, 1, 15, 25));
                    npcLoot.Add(ItemDropRule.Common(ItemID.DemoniteOre, 1, 45, 125));
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EnchantedFragment>(), 1, 3, 5));
                    npcLoot.Add(ItemDropRule.Common(ItemID.Rally, 3, 1, 1));
                }
                if (npc.type == NPCID.Creeper)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemID.TissueSample, 1, 2, 5));
                    npcLoot.Add(ItemDropRule.Common(ItemID.CrimtaneOre, 1, 5, 20));
                    npcLoot.Add(ItemDropRule.Common(ItemID.ShadowScale, 1, 2, 5));
                    npcLoot.Add(ItemDropRule.Common(ItemID.DemoniteOre, 1, 5, 20));
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FragmentOfFear>(), 3, 1, 1));
                }
                if (npc.type == NPCID.SkeletronHand)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemID.Bone, 1, 25, 100));
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FragmentOfFear>(), 1, 2, 4));
                }
                if (npc.type == NPCID.SkeletronHead)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemID.BlueMoon, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.Muramasa, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.AquaScepter, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.WaterBolt, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.Handgun, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.MagicMissile, 3, 1, 1));
                }
                if (npc.type == NPCID.Deerclops)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemID.IceBoomerang, 3, 1, 1));
                }
                if (npc.type == NPCID.WallofFlesh)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemID.HellstoneBar, 1, 5, 20));
                    npcLoot.Add(ItemDropRule.Common(ItemID.Hellstone, 1, 10, 75));
                    npcLoot.Add(ItemDropRule.Common(ItemID.Obsidian, 1, 10, 30));
                    npcLoot.Add(ItemDropRule.Common(ItemID.FlowerofFire, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.Sunfury, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.Flamelash, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.DarkLance, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.HelFire, 3, 1, 1));
                    npcLoot.Add(ItemDropRule.Common(ItemID.HellwingBow, 3, 1, 1));
                }
                if (npc.type == NPCID.Pumpking)
                {
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpookynightSeal>(), 20, 1, 1));
                }
                if (npc.type == NPCID.Golem)
                {
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BulbSunEnergy>(), 1, 6, 9));
                }
            }
            if (!npc.boss)
            {
                npcLoot.Add(ItemDropRule.ByCondition(new BloodMoon(), ModContent.ItemType<BloodDroplet>(), 4, 1, 4));
                npcLoot.Add(ItemDropRule.ByCondition(new KilledMechBosses(), ModContent.ItemType<Hypothermia>(), 20, 1, 1));
            }
        }
        public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
        {
            Player player = Main.LocalPlayer;
            var modPl = player.GetModPlayer<Jarkos_player>();
            foreach (Item item in items)
            {
                if (item is not null)
                {
                    item.shopCustomPrice = (int?)((item.shopCustomPrice ?? item.value) * 0.8f);
                }
            }
        }
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == 17)
            {
                shop.Add(new Item(ItemID.WormholePotion)
                {
                    shopCustomPrice = 1200
                });
            }
            if (shop.NpcType == 17 && NPC.downedBoss2)
            {
                shop.Add(new Item(ModContent.ItemType<FireballScroll>())
                {
                    shopCustomPrice = 198
                });
            }
            if (shop.NpcType == 17)
            {
                shop.Add(new Item(398)
                {
                    shopCustomPrice = 200000
                });
            }
            if (shop.NpcType == 20)
            {
                shop.Add(new Item(ModContent.ItemType<Mr_shooter>())
                {
                    shopCustomPrice = 6500
                });
            }
            if (shop.NpcType == 17)
            {
                shop.Add(new Item(3111)
                {
                    shopCustomPrice = 2500
                });
            }
            if (shop.NpcType == 38)
            {
                shop.Add(new Item(2590)
                {
                    shopCustomPrice = 450
                });
            }
            if (shop.NpcType == 38)
            {
                shop.Add(new Item(4423)
                {
                    shopCustomPrice = 350
                });
            }
            if (shop.NpcType == 17)
            {
                shop.Add(new Item(997)
                {
                    shopCustomPrice = 400000
                });
            }
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(new Item(4978)
                {
                    shopCustomPrice = 2500000
                });
            }
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(new Item(2273)
                {
                    shopCustomPrice = 500000
                });
            }
            if (shop.NpcType == NPCID.Merchant && NPC.downedAncientCultist)
            {
                shop.Add(new Item(ModContent.ItemType<Moonlord_bags>())
                {
                    shopCustomPrice = 4500000
                });
            }
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(new Item(ModContent.ItemType<Fruit_bag>())
                {
                    shopCustomPrice = 8500
                });
            }
            if (shop.NpcType == NPCID.BestiaryGirl)
            {
                shop.Add(new Item(ModContent.ItemType<Bugs_bag>())
                {
                    shopCustomPrice = 235250
                });
            }
            if (shop.NpcType == NPCID.BestiaryGirl)
            {
                shop.Add(new Item(ModContent.ItemType<FragmentOfNature>())
                {
                    shopCustomPrice = 15250
                });
            }
            if (shop.NpcType == NPCID.BestiaryGirl)
            {
                shop.Add(new Item(ModContent.ItemType<FragmentOfFear>())
                {
                    shopCustomPrice = 15250
                });
            }
            if (shop.NpcType == NPCID.BestiaryGirl)
            {
                shop.Add(new Item(ModContent.ItemType<EnchantedFragment>())
                {
                    shopCustomPrice = 15250
                });
            }
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(new Item(128)
                {
                    shopCustomPrice = 250000
                });
            }
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(new Item(2350)
                {
                    shopCustomPrice = 1000
                });
            }
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(new Item(75)
                {
                    shopCustomPrice = 1000
                });
            }
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(new Item(ModContent.ItemType<RicochetForce>())
                {
                    shopCustomPrice = 18000
                });
            }
        }
        public override void OnKill(NPC npc)
        {
            if (npc.type == NPCID.TheDestroyer || npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism || npc.type == NPCID.SkeletronPrime)
            {
                bool isMech1 = NPC.downedMechBoss1 || npc.type == NPCID.TheDestroyer;
                bool isMech2 = NPC.downedMechBoss2 || npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism;
                bool isMech3 = NPC.downedMechBoss3 || npc.type == NPCID.SkeletronPrime;

                if (isMech1 && isMech2 && isMech3 && !DownedBossSystem.generatedEtherionite)
                {
                    DownedBossSystem.generatedEtherionite = true;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Main.NewText(Language.GetTextValue((string)LangSystem.etherionite_gen), color: Color.MediumPurple);

                        int depthstoneType = ModContent.TileType<tiles.CradleTiles.PetrifiedDepthstoneTile>();
                        int sandType = ModContent.TileType<tiles.CradleTiles.PetrifiedSandTile>();

                        for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 2.6E-04); k++)
                        {
                            int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                            int y = WorldGen.genRand.Next((int)Main.rockLayer, Main.maxTilesY - 200);
                            Tile tile = Framing.GetTileSafely(x, y);

                            if (tile.HasTile && (tile.TileType == depthstoneType || tile.TileType == sandType))
                            {
                                int size = WorldGen.genRand.Next(3, 7);
                                for (int dx = -size; dx <= size; dx++)
                                {
                                    for (int dy = -size; dy <= size; dy++)
                                    {
                                        if (dx * dx + dy * dy > size * size || WorldGen.genRand.NextBool(3)) continue;
                                        int tx = x + dx, ty = y + dy;

                                        Tile t = Framing.GetTileSafely(tx, ty);
                                        if (t.HasTile && (t.TileType == depthstoneType || t.TileType == sandType))
                                        {
                                            t.TileType = (ushort)ModContent.TileType<Etherionite>();
                                            WorldGen.SquareTileFrame(tx, ty, true);

                                            if (Main.netMode == NetmodeID.Server)
                                                NetMessage.SendTileSquare(-1, tx, ty, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void RemoveBuffFromNPC(NPC npc, int buffType)
        {
            if (npc == null || npc.buffType == null)
                return;

            for (int i = 0; i < npc.buffType.Length; i++)
            {
                if (npc.buffType[i] == buffType)
                {
                    npc.DelBuff(i);
                    break;
                }
            }
        }

        public override void AI(NPC npc)
        {
            if (purple_mark_time > 0) purple_mark_time--;
            if (cyan_mark_time > 0) cyan_mark_time--;

            if (purple_mark_time > 0 && cyan_mark_time > 0)
            {
                purple_mark_time = 0;
                cyan_mark_time = 0;
                RemoveBuffFromNPC(npc, ModContent.BuffType<buffs.debuff.BlueTetherDebuff>());
                RemoveBuffFromNPC(npc, ModContent.BuffType<buffs.debuff.PurpleTetherDebuff>());

                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f }, npc.Center);

                int launchDamage = cyan_projectile_damage > 0 ? cyan_projectile_damage : 80;
                const float launchRadius = 200f;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.active && !other.friendly && Vector2.Distance(other.Center, npc.Center) <= launchRadius)
                    {
                        other.velocity.Y = -2f;
                        if (!IsBossOrBossPart(other))
                        {
                            other.velocity.X *= 0.5f;
                        }
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            other.SimpleStrikeNPC(launchDamage, 0, false, 0f, null, false, 0, true);
                        }
                    }
                }

                new UpGlow_particle(npc.Center, launchRadius * 2f, Color.Cyan, 35, 150f, 1.2f, 1.0f, 0.05f).Spawn();
                new UpGlow_particle(npc.Center, launchRadius * 1.5f, Color.MediumPurple, 30, 120f, 1.0f, 0.8f, 0.1f).Spawn();

                for (int i = 0; i < 35; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) - new Vector2(0f, Main.rand.NextFloat(4f, 10f));
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Clentaminator_Cyan, vel.X, vel.Y, 0, default, 1.8f);
                    d.noGravity = true;

                    Dust d2 = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.ShadowbeamStaff, vel.X * 0.8f, vel.Y * 0.8f, 0, default, 1.6f);
                    d2.noGravity = true;
                }

                for (int i = 0; i < 12; i++)
                {
                    Vector2 starVel = -Vector2.UnitY * Main.rand.NextFloat(6f, 14f) + Main.rand.NextVector2Circular(4f, 2f);
                    new StarParticle(npc.Center + Main.rand.NextVector2Circular(60f, 20f), starVel, Color.Cyan, 35, 0.05f, 1.2f, 0.1f).Spawn();
                }
            }

            if (cyan_mark_time > 0 && purple_mark_time == 0 && !IsBossOrBossPart(npc))
            {
                NPC partner = null;
                float minDistance = float.MaxValue;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.whoAmI != npc.whoAmI && other.active && !other.friendly && !IsBossOrBossPart(other))
                    {
                        Jarkos_npc otherGlobal = other.GetGlobalNPC<Jarkos_npc>();
                        if (otherGlobal.purple_mark_time > 0 && otherGlobal.cyan_mark_time == 0)
                        {
                            float dist = Vector2.Distance(npc.Center, other.Center);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                partner = other;
                            }
                        }
                    }
                }

                if (partner != null)
                {
                    Jarkos_npc partnerGlobal = partner.GetGlobalNPC<Jarkos_npc>();
                    Vector2 dir = (partner.Center - npc.Center).SafeNormalize(Vector2.Zero);
                    float currentDist = Vector2.Distance(npc.Center, partner.Center);

                    float pullSpeed = MathHelper.Clamp(14f - currentDist * 0.005f, 2f, 5f);
                    npc.velocity = Vector2.Lerp(npc.velocity, dir * pullSpeed, 0.25f);
                    partner.velocity = Vector2.Lerp(partner.velocity, -dir * pullSpeed, 0.25f);

                    if (Main.rand.NextBool(2))
                    {
                        float t = Main.rand.NextFloat();
                        Vector2 tetherPos = Vector2.Lerp(npc.Center, partner.Center, t);
                        Dust spark = Dust.NewDustDirect(tetherPos - new Vector2(4), 8, 8, DustID.Electric, 0f, 0f, 100, Color.DeepSkyBlue, 0.8f);
                        spark.noGravity = true;
                    }

                    float touchThreshold = (npc.width + partner.width) * 0.5f + 16f;
                    if (currentDist <= touchThreshold)
                    {
                        cyan_mark_time = 0;
                        partnerGlobal.purple_mark_time = 0;
                        RemoveBuffFromNPC(npc, ModContent.BuffType<buffs.debuff.BlueTetherDebuff>());
                        RemoveBuffFromNPC(partner, ModContent.BuffType<buffs.debuff.PurpleTetherDebuff>());

                        Vector2 explosionPos = (npc.Center + partner.Center) * 0.5f;
                        int combinedDamage = (cyan_projectile_damage > 0 ? cyan_projectile_damage : 80) + (partnerGlobal.purple_projectile_damage > 0 ? partnerGlobal.purple_projectile_damage : 160);

                        SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 1.2f }, explosionPos);
                        SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f, Volume = 0.9f }, explosionPos);

                        const float explosionRadius = 160f;
                        for (int k = 0; k < Main.maxNPCs; k++)
                        {
                            NPC target = Main.npc[k];
                            if (target.active && !target.friendly && Vector2.Distance(target.Center, explosionPos) <= explosionRadius)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    target.SimpleStrikeNPC(combinedDamage, 0, false, 0f, null, false, 0, true);
                                }
                            }
                        }

                        for (int i = 0; i < 40; i++)
                        {
                            Vector2 dustVel = Main.rand.NextVector2Circular(12f, 12f);
                            Dust d1 = Dust.NewDustDirect(explosionPos - new Vector2(8), 16, 16, DustID.Clentaminator_Cyan, dustVel.X, dustVel.Y, 0, default, 2f);
                            d1.noGravity = true;

                            Vector2 dustVel2 = Main.rand.NextVector2Circular(12f, 12f);
                            Dust d2 = Dust.NewDustDirect(explosionPos - new Vector2(8), 16, 16, DustID.ShadowbeamStaff, dustVel2.X, dustVel2.Y, 0, default, 2f);
                            d2.noGravity = true;
                        }

                        new UpGlow_particle(explosionPos, explosionRadius * 2f, Color.MediumPurple, 30, 80f, 1.4f, 1.0f, 0.1f).Spawn();
                        new UpGlow_particle(explosionPos, explosionRadius * 1.6f, Color.Cyan, 25, 100f, 1.2f, 1.0f, 0.05f).Spawn();

                        for (int i = 0; i < 16; i++)
                        {
                            Vector2 starVel = Main.rand.NextVector2Circular(10f, 10f);
                            Color starCol = Main.rand.NextBool() ? Color.Cyan : Color.MediumPurple;
                            new StarParticle(explosionPos, starVel, starCol, 30, 0.05f, 1.0f, 0.1f).Spawn();
                        }
                    }
                }
            }
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (purple_mark_time > 0)
            {
                if (Main.rand.NextBool(3))
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.GemAmethyst);
            }
            if (cyan_mark_time > 0)
            {
                if (Main.rand.NextBool(3))
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.GemSapphire);
            }
        }
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            Player playerr = Main.LocalPlayer;
            var modPl = playerr.GetModPlayer<Jarkos_player>();
            damage_done_global = damageDone * 5 / 100;

            if (modPl.burning_mark && !npc.boss)
            {
                npc.velocity.X *= 0.25f;
            }

            if (beer_debuff && !npc.boss)
            {
                Vector2 new_vector = new Vector2(npc.velocity.X / 1.5f, npc.velocity.Y / 1.05f);
                npc.velocity = new_vector;
            }

            if (shadow_pact_debuff)
            {
                hits_in_SP_debuff++;
            }

            if(projectile.type == ModContent.ProjectileType<Rollback_impact>() && ModContent.GetInstance<Max_damage_cap>().Condition.Value < damageDone)
            {
                ModContent.GetInstance<Max_damage_cap>().Condition.Value = damageDone;
            }

            if(projectile.type == ModContent.ProjectileType<Vine_of_Rage_proj>() && playerr.ownedProjectileCounts[ModContent.ProjectileType<PhantomDagger_proj>()] > 0)
            {
                ModContent.GetInstance<Phantom_dagger_got>().Condition.Complete();
            }
        }

        public static bool IsBossOrBossPart(NPC npc)
        {
            if (!npc.active) return false;
            if (npc.boss) return true;
            if (NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) return true;
            if (npc.realLife >= 0 && Main.npc[npc.realLife].active)
            {
                NPC parent = Main.npc[npc.realLife];
                if (parent.boss || NPCID.Sets.ShouldBeCountedAsBoss[parent.type])
                    return true;
            }

            switch (npc.type)
            {
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsTail:
                case NPCID.ServantofCthulhu:
                case NPCID.Creeper:
                case NPCID.TheHungry:
                case NPCID.TheHungryII:
                case NPCID.LeechHead:
                case NPCID.LeechBody:
                case NPCID.LeechTail:
                case NPCID.SkeletronHand:
                case NPCID.PrimeCannon:
                case NPCID.PrimeSaw:
                case NPCID.PrimeVice:
                case NPCID.PrimeLaser:
                case NPCID.PlanterasTentacle:
                case NPCID.PlanterasHook:
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                case NPCID.GolemHead:
                case NPCID.GolemHeadFree:
                case NPCID.Sharkron:
                case NPCID.Sharkron2:
                case NPCID.CultistDragonHead:
                case NPCID.CultistDragonBody1:
                case NPCID.CultistDragonBody2:
                case NPCID.CultistDragonTail:
                case NPCID.AncientLight:
                case NPCID.AncientDoom:
                case NPCID.MoonLordFreeEye:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordHead:
                case NPCID.MoonLordCore:
                case NPCID.MartianSaucer:
                case NPCID.MartianSaucerCannon:
                case NPCID.MartianSaucerTurret:
                case NPCID.MartianSaucerCore:
                    return true;
            }

            return false;
        }

        public static bool IsAnyBossAlive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (IsBossOrBossPart(npc))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
