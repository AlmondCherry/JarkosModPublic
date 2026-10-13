using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using ReLogic.Content;
using Jarkos.filters;
using Jarkos.UI;
using jarkos.projectiles.summon.whip;
using Terraria.DataStructures;
using Jarkos.tiles;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using jarkos.items.accs.amulets;
using jarkos.tiles.CradleTiles;
using Terraria.GameContent.UI;
using jarkos.items.materials;
using System.Linq;
using Terraria.Audio;
using jarkos.core.PlayerInteract;
using jarkos.core.Data;
using jarkos.tiles;
using jarkos.UI;
using jarkos.items.accs.melee;
using jarkos.items.weapons.melee.true_melee;
using System.IO;
using jarkos.systems;
using jarkos.npc.Punisher.Cutscene;
using Terraria.Localization;

namespace Jarkos
{
    public enum JarkosPacketType : byte
    {
        ManaEssencePickup, ManaEssenceStateSync, InductionSlotSync, SpawnSoulStealerAura, HemostabilizerSync, ConverterSlotSync, MessageType_StartAltarRitual, MessageType_DepositMinorAltar, MessageType_TakeMinorAltar, MessageType_ToggleNeutralizer, MessageType_StartSpecificAltarRitual, AqueousPathSync, DistillerSlotSync, DistillerCollect, EssenceOrbPickup, SyncResearchPlayer, DistillerStateSync, DistillerProcessItem, SpreaderSync, SyncPlayerEssences, SpreaderTransferVisual, AlchEnchantStateSync, AlchEnchantSlotSync, AlchEnchantAction, PlaceDistiller, StartResearchGlobal, AbortResearchGlobal, CompleteResearchGlobal, UpdatePuzzleGlobal, SyncGlobalResearch, SetResearchTableOwner, HarakiriCutsceneSync, SyncSoulStealerPlayer, SyncBossFightRecord, RecordBossDamage, RecordBossHurt, ArmorPlateSync, EvolutionBladeSync
    }
    public class Jarkos_mod : Mod
    {
        public static int PollenCurrencyId;
        public const string VortexShaderKey = "Jarkos:PortalVortex";
        public const string GlowShaderKey = "Jarkos:PortalGlow";
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            JarkosPacketType packetType = (JarkosPacketType)reader.ReadByte();
            switch (packetType)
            {
                case JarkosPacketType.SyncBossFightRecord:
                    BossStatsTrackerSystem.ReceiveBossFightRecordPacket(reader);
                    break;
                case JarkosPacketType.RecordBossDamage:
                {
                    byte playerWhoAmI = reader.ReadByte();
                    int damage = reader.ReadInt32();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        if (playerWhoAmI >= 0 && playerWhoAmI < Main.maxPlayers)
                        {
                            Player player = Main.player[playerWhoAmI];
                            if (player != null && player.active)
                            {
                                BossStatsTrackerSystem.RecordDamageDealt(player, null, damage);

                                ModPacket packet = GetPacket();
                                packet.Write((byte)JarkosPacketType.RecordBossDamage);
                                packet.Write(playerWhoAmI);
                                packet.Write(damage);
                                packet.Send(-1, whoAmI);
                            }
                        }
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        if (playerWhoAmI >= 0 && playerWhoAmI < Main.maxPlayers && playerWhoAmI != Main.myPlayer)
                        {
                            Player player = Main.player[playerWhoAmI];
                            if (player != null && player.active)
                            {
                                BossStatsTrackerSystem.RecordDamageDealt(player, null, damage);
                            }
                        }
                    }
                    break;
                }
                case JarkosPacketType.RecordBossHurt:
                {
                    byte playerWhoAmI = reader.ReadByte();
                    int damageTaken = reader.ReadInt32();
                    int damageBlocked = reader.ReadInt32();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        if (playerWhoAmI >= 0 && playerWhoAmI < Main.maxPlayers)
                        {
                            Player player = Main.player[playerWhoAmI];
                            if (player != null && player.active)
                            {
                                BossStatsTrackerSystem.RecordDamageTakenAndBlocked(player, damageTaken, damageBlocked);

                                ModPacket packet = GetPacket();
                                packet.Write((byte)JarkosPacketType.RecordBossHurt);
                                packet.Write(playerWhoAmI);
                                packet.Write(damageTaken);
                                packet.Write(damageBlocked);
                                packet.Send(-1, whoAmI);
                            }
                        }
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        if (playerWhoAmI >= 0 && playerWhoAmI < Main.maxPlayers && playerWhoAmI != Main.myPlayer)
                        {
                            Player player = Main.player[playerWhoAmI];
                            if (player != null && player.active)
                            {
                                BossStatsTrackerSystem.RecordDamageTakenAndBlocked(player, damageTaken, damageBlocked);
                            }
                        }
                    }
                    break;
                }
                case JarkosPacketType.ManaEssencePickup:
                    ManaEssenceNetHandler.HandlePickup(reader, whoAmI);
                    break;
                case JarkosPacketType.ManaEssenceStateSync:
                    ManaEssenceNetHandler.HandleStateSync(reader, whoAmI);
                    break;
                 case JarkosPacketType.HarakiriCutsceneSync:
                {
                    byte syncPlayerId = reader.ReadByte();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)JarkosPacketType.HarakiriCutsceneSync);
                        packet.Write(syncPlayerId);
                        packet.Send(-1, whoAmI);
                        Main.player[syncPlayerId].GetModPlayer<HarakiriCutscenePlayer>().StartCutscene();
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        Main.player[syncPlayerId].GetModPlayer<HarakiriCutscenePlayer>().StartCutscene();
                    }
                    break;
                }
                case JarkosPacketType.PlaceDistiller:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        int limit = 1;
                        var plr = Main.player[whoAmI].GetModPlayer<ResearchPlayer>();
                        if (plr != null && plr.IsCompleted("jarkos:AquaNode8"))
                        {
                            limit = 2;
                        }
                        int count = TileEntity.ByID.Values.OfType<DistillerTileEntity>().Count();
                        if (count >= limit)
                        {
                            WorldGen.KillTile(teX, teY, fail: false, effectOnly: false, noItem: false);
                            NetMessage.SendTileSquare(-1, teX, teY, 4);
                        }
                        else
                        {
                            int newID = ModContent.GetInstance<DistillerTileEntity>().Place(teX, teY);
                            if (newID != -1)
                            {
                                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, newID, teX, teY);
                            }
                        }
                    }
                    break;
                }
                case JarkosPacketType.HemostabilizerSync:
                    HemostabilizerPacketHandler.HandlePacket(reader, whoAmI);
                    break;
                case JarkosPacketType.ConverterSlotSync:
                {
                    int teID = reader.ReadInt32();
                    byte slotIndex = reader.ReadByte();
                    bool hasItem = reader.ReadBoolean();
                    Item item;

                    if (hasItem)
                        item = ItemIO.Receive(reader, true);
                    else
                    {
                        item = new Item();
                        item.TurnToAir();
                    }
                    if (TileEntity.ByID.TryGetValue(teID, out TileEntity te2) && te2 is EnergyConverterTE convTE)
                    {
                        if (slotIndex == 0)
                            convTE.InputSlot = item;
                        else
                            convTE.OutputSlot = item;
                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket packet = GetPacket();
                            packet.Write((byte)JarkosPacketType.ConverterSlotSync);
                            packet.Write(teID);
                            packet.Write(slotIndex);

                            bool sending = !item.IsAir;
                            packet.Write(sending);

                            if (sending)
                                ItemIO.Send(item, packet, true);
                            packet.Send(-1, whoAmI);
                        }
                    }
                    break;
                }
				case JarkosPacketType.MessageType_StartAltarRitual: 
                {
					int x = reader.ReadInt32();
					int y = reader.ReadInt32();
					HandleStartAltarRitual(x, y, whoAmI);
					break;
				}
				case JarkosPacketType.MessageType_DepositMinorAltar: 
                {
					int x = reader.ReadInt32();
					int y = reader.ReadInt32();
					Item itemToDeposit = ItemIO.Receive(reader, readStack: true);
					HandleDepositMinorAltar(x, y, itemToDeposit);
					break;
				}
				case JarkosPacketType.MessageType_TakeMinorAltar: 
                {
					int x = reader.ReadInt32();
					int y = reader.ReadInt32();
					HandleTakeMinorAltar(x, y);
					break;
				}
                case JarkosPacketType.MessageType_ToggleNeutralizer: 
                {
					int x = reader.ReadInt32();
					int y = reader.ReadInt32();
					HandleToggleNeutralizer(x, y);
					break;
				}
                case JarkosPacketType.MessageType_StartSpecificAltarRitual: 
                {
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    int recipeIndex = reader.ReadInt32();
                    HandleStartSpecificAltarRitual(x, y, recipeIndex, whoAmI);
                    break;
                }
                case JarkosPacketType.AqueousPathSync:
                    AqueousPathPlayer.HandlePacket(reader, whoAmI);
                    break;
                case JarkosPacketType.DistillerSlotSync:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    byte slotIndex = reader.ReadByte();
                    bool hasItem = reader.ReadBoolean();
                    Item item;
                    if (hasItem)
                        item = ItemIO.Receive(reader, true);
                    else
                    {
                        item = new Item();
                        item.TurnToAir();
                    }
                    if (!TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te2))
                    {
                        if (Main.netMode == NetmodeID.Server)
                        {
                            int newID = ModContent.GetInstance<DistillerTileEntity>().Place(teX, teY);
                            if (newID != -1)
                            {
                                te2 = TileEntity.ByID[newID];
                                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, newID, teX, teY);
                            }
                        }
                    }
                    if (te2 is DistillerTileEntity distTE)
                    {
                        distTE.InputSlots[slotIndex] = item;
                        distTE.ProcessTimers[slotIndex] = 0;
                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket packet = GetPacket();
                            packet.Write((byte)JarkosPacketType.DistillerSlotSync);
                            packet.Write(teX);
                            packet.Write(teY);
                            packet.Write(slotIndex);
                            bool sending = !item.IsAir;
                            packet.Write(sending);

                            if (sending)
                                ItemIO.Send(item, packet, true);

                            packet.Send(-1, whoAmI);
                        }
                    }
                    break;
                }
                case JarkosPacketType.DistillerCollect:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        if (TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te3) && te3 is DistillerTileEntity distTE)
                        {
                            var rp = Main.player[whoAmI].GetModPlayer<ResearchPlayer>();
                            var activeEssences = distTE.StoredEssences.Where(kvp => kvp.Value > 0).ToList();
                            
                            ModPacket packet = GetPacket();
                            packet.Write((byte)JarkosPacketType.DistillerCollect);
                            packet.Write(teX);
                            packet.Write(teY);
                            packet.Write((byte)activeEssences.Count);

                            foreach (var kvp in activeEssences)
                            {
                                rp.AddEssence(kvp.Key, kvp.Value);
                                packet.Write((byte)kvp.Key);
                                packet.Write(kvp.Value);
                            }
                            distTE.ClearEssences();
                            distTE.SyncData();
                            packet.Send(whoAmI);
                        }
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        byte count = reader.ReadByte();
                        var rp = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                        for (int i = 0; i < count; i++)
                        {
                            EssenceType type = (EssenceType)reader.ReadByte();
                            int amount = reader.ReadInt32();
                            rp.AddEssence(type, amount);
                        }
                        SoundEngine.PlaySound(SoundID.Item4);
                    }
                    break;
                }
                case JarkosPacketType.EssenceOrbPickup:
                {
                    EssenceType type = (EssenceType)reader.ReadByte();
                    int amount = reader.ReadInt32();
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        var rp = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                        rp.AddEssence(type, amount);
                        int currentTotal = rp.GetEssence(type);
                        EssencePopupSystem.AddPopup(Main.LocalPlayer, type, currentTotal);
                        SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0.8f }, Main.LocalPlayer.Center);

                        Color essenceColor = EssencePalette.ColorOf(type);
                        for (int i = 0; i < 15; i++)
                        {
                            Dust d = Dust.NewDustDirect(Main.LocalPlayer.Center, 0, 0, DustID.MagicMirror);
                            d.color = essenceColor;
                            d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                            d.noGravity = true;
                            d.scale = 1.5f;
                        }
                    }
                    break;
                }
                case JarkosPacketType.DistillerStateSync:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    bool ignoreLaser = reader.ReadBoolean();
                    ushort pt0 = reader.ReadUInt16();
                    ushort pt1 = reader.ReadUInt16();
                    byte count = reader.ReadByte();

                    Dictionary<EssenceType, int> essences = new Dictionary<EssenceType, int>();
                    for (int i = 0; i < count; i++)
                    {
                        essences[(EssenceType)reader.ReadByte()] = reader.ReadUInt16();
                    }

                    bool hasSlot0 = reader.ReadBoolean();
                    Item slot0 = hasSlot0 ? ItemIO.Receive(reader, true) : new Item();
                    if (!hasSlot0) slot0.TurnToAir();

                    bool hasSlot1 = reader.ReadBoolean();
                    Item slot1 = hasSlot1 ? ItemIO.Receive(reader, true) : new Item();
                    if (!hasSlot1) slot1.TurnToAir();

                    if (TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te2) && te2 is DistillerTileEntity distTE)
                    {
                        distTE.IgnoreLaser = ignoreLaser;
                        distTE.ProcessTimers[0] = pt0;
                        distTE.ProcessTimers[1] = pt1;
                        distTE.StoredEssences = essences;
                        distTE.InputSlots[0] = slot0;
                        distTE.InputSlots[1] = slot1;
                        if (Main.netMode == NetmodeID.Server)
                        {
                            distTE.SyncData();
                        }
                    }
                    break;
                }
                case JarkosPacketType.DistillerProcessItem:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    byte slotIndex = reader.ReadByte();
                    Item item = ItemIO.Receive(reader, true);
                    if (!TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te2))
                    {
                        if (Main.netMode == NetmodeID.Server)
                        {
                            int newID = ModContent.GetInstance<DistillerTileEntity>().Place(teX, teY);
                            if (newID != -1)
                            {
                                te2 = TileEntity.ByID[newID];
                                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, newID, teX, teY);
                            }
                        }
                    }
                    if (te2 is DistillerTileEntity distTE)
                    {
                        if (Main.netMode == NetmodeID.Server)
                        {
                            distTE.InputSlots[slotIndex] = item;
                            var types = DistillerTileEntity.GetEssencesForItem(distTE.InputSlots[slotIndex]);
                            distTE.InputSlots[slotIndex].stack--;
                            if (distTE.InputSlots[slotIndex].stack <= 0) distTE.InputSlots[slotIndex].TurnToAir();
                            foreach (var type in types)
                                distTE.AddEssence(type, 1);
                            distTE.ProcessTimers[slotIndex] = 0;
                            distTE.SyncData();
                        }
                    }
                    break;
                }
                case JarkosPacketType.SpreaderSync:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    float rot = reader.ReadSingle();
                    int filter = reader.ReadInt32();
                    int aiming = reader.ReadInt32();
                    bool beams = reader.ReadBoolean();

                    if (!TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te3))
                    {
                        if (Main.netMode == NetmodeID.Server)
                        {
                            int newID = ModContent.GetInstance<EssenceSpreaderTileEntity>().Place(teX, teY);
                            if (newID != -1)
                            {
                                te3 = TileEntity.ByID[newID];
                                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, newID, teX, teY);
                            }
                        }
                    }
                    if (te3 is EssenceSpreaderTileEntity spreader)
                    {
                        if (Main.netMode == NetmodeID.Server)
                        {
                            spreader.TargetRotation = rot;
                            spreader.FilterType = (EssenceType)filter;
                            spreader.AimingPlayer = aiming;
                            spreader.ShowBeams = beams;
                            spreader.SyncData();
                        }
                    }
                    break;
                }
                case JarkosPacketType.SyncPlayerEssences:
                {
                    byte who = reader.ReadByte();
                    byte count = reader.ReadByte();
                    var rp = Main.player[who].GetModPlayer<ResearchPlayer>();
                    rp.Essences.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        EssenceType type = (EssenceType)reader.ReadByte();
                        int amount = reader.ReadInt32();
                        if (System.Enum.IsDefined(typeof(EssenceType), type) && amount > 0)
                        {
                            rp.Essences[type] = amount;
                            rp.DiscoveredEssences.Add((int)type);
                        }
                    }
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)JarkosPacketType.SyncPlayerEssences);
                        packet.Write(who);
                        packet.Write((byte)rp.Essences.Count);
                        foreach (var kvp in rp.Essences)
                        {
                            packet.Write((byte)kvp.Key);
                            packet.Write(kvp.Value);
                        }
                        packet.Send(-1, who);
                    }
                    break;
                }
                case JarkosPacketType.SpreaderTransferVisual:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    Vector2 startPos = reader.ReadVector2();
                    Vector2 targetPos = reader.ReadVector2();
                    EssenceType type = (EssenceType)reader.ReadByte();
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        if (TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te) && te is EssenceSpreaderTileEntity spreader)
                        {
                            spreader.SpawnTransferParticles(startPos, targetPos, type);
                        }
                    }
                    break;
                }
                case JarkosPacketType.AlchEnchantStateSync:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    int selectedRecipe = reader.ReadInt32();
                    int craftingProgress = reader.ReadInt32();
                    bool isCrafting = reader.ReadBoolean();
                    bool pullEssenceMode = reader.ReadBoolean();
                    byte count = reader.ReadByte();

                    Dictionary<EssenceType, int> essences = new Dictionary<EssenceType, int>();
                    for (int i = 0; i < count; i++)
                    {
                        essences[(EssenceType)reader.ReadByte()] = reader.ReadInt32();
                    }
                    bool hasSlot0 = reader.ReadBoolean();
                    Item slot0 = hasSlot0 ? ItemIO.Receive(reader, true) : new Item();
                    if (!hasSlot0) slot0.TurnToAir();
                    bool hasSlot1 = reader.ReadBoolean();
                    Item slot1 = hasSlot1 ? ItemIO.Receive(reader, true) : new Item();
                    if (!hasSlot1) slot1.TurnToAir();
                    bool hasOut = reader.ReadBoolean();
                    Item slotOut = hasOut ? ItemIO.Receive(reader, true) : new Item();
                    if (!hasOut) slotOut.TurnToAir();

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        if (TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te) && te is AlchemicalEnchantmentTE ate)
                        {
                            ate.SelectedRecipeIndex = selectedRecipe;
                            ate.CraftingProgress = craftingProgress;
                            ate.IsCrafting = isCrafting;
                            ate.PullEssenceMode = pullEssenceMode;
                            ate.StoredEssences = essences;
                            ate.InputSlots[0] = slot0;
                            ate.InputSlots[1] = slot1;
                            ate.OutputSlot = slotOut;
                        }
                    }
                    break;
                }
                case JarkosPacketType.AlchEnchantSlotSync:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    byte slotIndex = reader.ReadByte();
                    bool hasItem = reader.ReadBoolean();
                    Item item = hasItem ? ItemIO.Receive(reader, true) : new Item();

                    if (!hasItem) item.TurnToAir();
                    if (TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te))
                    {
                        if (te is AlchemicalEnchantmentTE ate)
                        {
                            if (slotIndex == 2) ate.OutputSlot = item;
                            else ate.InputSlots[slotIndex] = item;
                            if (Main.netMode == NetmodeID.Server)
                            {
                                ate.SyncData();
                            }
                        }
                    }
                    break;
                }
                case JarkosPacketType.AlchEnchantAction:
                {
                    short teX = reader.ReadInt16();
                    short teY = reader.ReadInt16();
                    byte actionType = reader.ReadByte();
                    int arg = reader.ReadInt32();

                    if (TileEntity.ByPosition.TryGetValue(new Point16(teX, teY), out TileEntity te))
                    {
                        if (te is AlchemicalEnchantmentTE ate)
                        {
                            if (actionType == 0) 
                            {
                                ate.PullEssenceMode = !ate.PullEssenceMode;
                            }
                            else if (actionType == 1) 
                            {
                                ate.CycleRecipe(arg);
                            }
                            else if (actionType == 2) 
                            {
                                ate.StartCrafting();
                            }
                            if (Main.netMode == NetmodeID.Server)
                            {
                                ate.SyncData();
                            }
                        }
                    }
                    break;
                }
                case JarkosPacketType.StartResearchGlobal:
                {
                    string id = reader.ReadString();
                    ResearchSystem.GlobalActiveResearchID = id;
                    ResearchSystem.GlobalActivePuzzleProgress.Clear();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)JarkosPacketType.StartResearchGlobal);
                        packet.Write(id);
                        packet.Send(-1, whoAmI);
                    }
                    break;
                }
                case JarkosPacketType.AbortResearchGlobal:
                {
                    ResearchSystem.GlobalActiveResearchID = null;
                    ResearchSystem.GlobalActivePuzzleProgress.Clear();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)JarkosPacketType.AbortResearchGlobal);
                        packet.Send(-1, whoAmI);
                    }
                    break;
                }
                case JarkosPacketType.CompleteResearchGlobal:
                {
                    string id = reader.ReadString();
                    ResearchSystem.GlobalCompleted.Add(id);

                    if (ResearchSystem.GlobalActiveResearchID == id)
                    {
                        ResearchSystem.GlobalActiveResearchID = null;
                        ResearchSystem.GlobalActivePuzzleProgress.Clear();
                    }
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)JarkosPacketType.CompleteResearchGlobal);
                        packet.Write(id);
                        packet.Send(-1, whoAmI);
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        if (ConstellationRegistry.TryGet(id, out var entry))
                        {
                            entry.OnUnlock(Main.LocalPlayer);
                        }
                        ResearchScreen.Instance?.TriggerNodeCompletion(id);
                    }
                    break;
                }
                case JarkosPacketType.UpdatePuzzleGlobal:
                {
                    int nodeId = reader.ReadInt32();
                    int type = reader.ReadInt32();
                    ResearchSystem.GlobalActivePuzzleProgress[nodeId] = type;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)JarkosPacketType.UpdatePuzzleGlobal);
                        packet.Write(nodeId);
                        packet.Write(type);
                        packet.Send(-1, whoAmI);
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ResearchScreen.Instance?.OnPuzzleProgressReceived(nodeId, type);
                    }
                    break;
                }
                case JarkosPacketType.SetResearchTableOwner:
                {
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    string name = reader.ReadString();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        int index = ModContent.GetInstance<ResearchTableTileEntity>().Find(x, y);
                        if (index == -1)
                        {
                            index = ModContent.GetInstance<ResearchTableTileEntity>().Place(x, y);
                        }
                        if (index != -1)
                        {
                            var te = (ResearchTableTileEntity)TileEntity.ByID[index];
                            te.OwnerName = name;
                            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, te.ID, te.Position.X, te.Position.Y);
                        }
                    }
                    break;
                }
                case JarkosPacketType.ArmorPlateSync:
                {
                    byte playerId = reader.ReadByte();
                    int cooldown = reader.ReadInt32();
                    if (playerId >= 0 && playerId < Main.maxPlayers)
                    {
                        Player player = Main.player[playerId];
                        if (player != null && player.active && player.TryGetModPlayer(out ArmorPlatePlayer modPlayer))
                        {
                            modPlayer.ArmorPlateCooldown = cooldown;
                            if (Main.netMode == NetmodeID.Server)
                            {
                                ModPacket packet = GetPacket();
                                packet.Write((byte)JarkosPacketType.ArmorPlateSync);
                                packet.Write(playerId);
                                packet.Write(cooldown);
                                packet.Send(-1, whoAmI);
                            }
                        }
                    }
                    break;
                }
                case JarkosPacketType.EvolutionBladeSync:
                {
                    byte playerId = reader.ReadByte();
                    byte stage = reader.ReadByte();
                    int kills = reader.ReadInt32();
                    if (playerId >= 0 && playerId < Main.maxPlayers)
                    {
                        Player player = Main.player[playerId];
                        if (player != null && player.active && player.TryGetModPlayer(out EvolutionBladePlayer modPlayer))
                        {
                            modPlayer.evolutionStage = stage;
                            modPlayer.killCount = kills;
                            if (Main.netMode == NetmodeID.Server)
                            {
                                ModPacket packet = GetPacket();
                                packet.Write((byte)JarkosPacketType.EvolutionBladeSync);
                                packet.Write(playerId);
                                packet.Write(stage);
                                packet.Write(kills);
                                packet.Send(-1, whoAmI);
                            }
                        }
                    }
                    break;
                }
            }
        }
        private static void HandleToggleNeutralizer(int posX, int posY) 
        {
			if (Main.netMode != NetmodeID.Server) 
            {
				return;
			}
			CorruptionNeutralizerTileEntity entity = CorruptionNeutralizerTile.GetOrCreate(posX, posY);
			if (entity is null) 
            {
				return;
			}
			CorruptionNeutralizerTile.ToggleAndAnnounce(entity);
		}
        private static void HandleStartAltarRitual(int posX, int posY, int whoAmI) 
        {
			if (Main.netMode != NetmodeID.Server) 
            {
				return;
			}
			PetrifiedAltarTileEntity entity = PetrifiedAltarTile.GetOrCreate(posX, posY);
			if (entity == null || entity.IsAnimating) 
            {
				return;
			}
			bool started = entity.TryStartRitual(Main.player[whoAmI]);
			if (!started) {
				Terraria.Chat.ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("Something is missing"), Color.SlateGray, whoAmI);
			}
		}
        private static void HandleStartSpecificAltarRitual(int posX, int posY, int recipeIndex, int whoAmI) 
        {
            if (Main.netMode != NetmodeID.Server) 
            {
                return;
            }
            PetrifiedAltarTileEntity entity = PetrifiedAltarTile.GetOrCreate(posX, posY);
            if (entity == null || entity.IsAnimating) 
            {
                return;
            }
            entity.StartRitualWithRecipeIndex(recipeIndex, Main.player[whoAmI]);
        }
		private static void HandleDepositMinorAltar(int posX, int posY, Item itemToDeposit) 
        {
			if (Main.netMode != NetmodeID.Server) 
            {
				return;
			}
			if (itemToDeposit is null || itemToDeposit.IsAir) 
            {
				return;
			}
			PetrifiedMinorAltarTileEntity entity = PetrifiedMinorAltarTile.GetOrCreate(posX, posY);
			if (entity is null || entity.HasItem) 
            {
				return;
			}
			entity.SetItem(itemToDeposit);
			NetMessage.SendData(MessageID.TileEntitySharing, number: entity.ID, number2: posX, number3: posY);
		}
		private static void HandleTakeMinorAltar(int posX, int posY) 
        {
			if (Main.netMode != NetmodeID.Server) 
            {
				return;
			}
			PetrifiedMinorAltarTileEntity entity = PetrifiedMinorAltarTile.GetOrCreate(posX, posY);
            
			if (entity is null || !entity.HasItem) 
            {
				return;
			}
			Item taken = entity.TakeItem();
			int newIdx = Item.NewItem(new EntitySource_TileEntity(entity), posX * 16, posY * 16 - 8, 32, 16, taken.type, taken.stack);

			if (newIdx >= 0 && newIdx < Main.maxItems) 
            {
				Main.item[newIdx].prefix = taken.prefix;
				Main.item[newIdx].velocity = new Vector2(0f, -2f);
				Main.item[newIdx].noGrabDelay = 0;
			}
			NetMessage.SendData(MessageID.TileEntitySharing, number: entity.ID, number2: posX, number3: posY);
		}
        public override void PostSetupContent()
        {
            PetrifiedAltarTileEntity.LoadRecipes();
        }
        public override void Load()
        {
            if (!Main.dedServ)
            {
                UITextures.Load(this);
            }

            PollenCurrencyId = CustomCurrencyManager.RegisterCurrency(new PollenCurrency(ModContent.ItemType<WastefulnessPollen>(), 9999L, "Pollen"));

            Asset<Effect> TooltipShader = ModContent.Request<Effect>("Jarkos/Shaders/ScreenShader", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc["Jarkos:TooltipSh"] = new MiscShaderData(TooltipShader, "TooltipSh");

            Asset<Effect> BlackHoleShader = ModContent.Request<Effect>("Jarkos/Shaders/BlackHole", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc["Jarkos:BlackHole"] = new MiscShaderData(BlackHoleShader, "BlackHole");

            if (Main.netMode != NetmodeID.Server)
            {
                SkyManager.Instance.Bind("TenderoteSky", new TenderoteSky());
                SkyManager.Instance.Bind("GodSky", new GodSky());

                Asset<Effect> GenericLaserShader = ModContent.Request<Effect>("Jarkos/Shaders/LaserShader", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc["Jarkos:LaserShader"] = new MiscShaderData(GenericLaserShader, "LaserShader");

                Asset<Effect> GenericRayShader = ModContent.Request<Effect>("Jarkos/Shaders/RayShader", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc["Jarkos:RayShader"] = new MiscShaderData(GenericRayShader, "RayShader");

                Asset<Effect> GenericCosmicShader = ModContent.Request<Effect>("Jarkos/Shaders/CosmicShader", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc["Jarkos:CosmicShader"] = new MiscShaderData(GenericCosmicShader, "CosmicShader");

                Asset<Effect> SlashEffectShader = ModContent.Request<Effect>("Jarkos/Shaders/SlashEffect", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc["Jarkos:SlashEffect"] = new MiscShaderData(SlashEffectShader, "SlashEffect");

                Asset<Effect> vortexAsset = ModContent.Request<Effect>("Jarkos/Shaders/PortalVortex", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc[VortexShaderKey] = new MiscShaderData(vortexAsset, VortexShaderKey);

                Asset<Effect> glowAsset = ModContent.Request<Effect>("Jarkos/Shaders/PortalGlow", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc[GlowShaderKey] = new MiscShaderData(glowAsset, GlowShaderKey);

                Asset<Effect> shaderAsset = ModContent.Request<Effect>("jarkos/Shaders/WaterFlow", AssetRequestMode.ImmediateLoad);
                Filters.Scene["Jarkos:GroundPulse"] = new Filter(new ScreenShaderData(shaderAsset, "GroundPulse"), EffectPriority.High);
                Filters.Scene["Jarkos:GroundPulse"].Load();

                GameShaders.Misc["Jarkos:WaterFlowText"] = new MiscShaderData(shaderAsset, "WaterFlowText");

                Asset<Effect> roarFilterAsset = ModContent.Request<Effect>("jarkos/Shaders/TenderoteRoarFilter", AssetRequestMode.ImmediateLoad);
                Filters.Scene["Jarkos:TenderoteRoarFilter"] = new Filter(new ScreenShaderData(roarFilterAsset, "TenderoteRoarFilter"), EffectPriority.VeryHigh);
                Filters.Scene["Jarkos:TenderoteRoarFilter"].Load();

                Asset<Effect> sanctuaryAsset = ModContent.Request<Effect>("jarkos/Shaders/PurgatorySanctuaryOuterGlow", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc["Jarkos:PurgatorySanctuaryOuterGlow"] = new MiscShaderData(sanctuaryAsset, "PurgatorySanctuaryOuterGlow");

                Asset<Effect> auraAsset = ModContent.Request<Effect>("jarkos/Shaders/PurgatoryAuraShader", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc["Jarkos:PurgatoryAuraShader"] = new MiscShaderData(auraAsset, "PurgatoryAuraShader");
            }
        }
        public override void Unload()
        {
            if (Main.dedServ)
            return;
            if (Filters.Scene["Jarkos:GroundPulse"] != null)
                Filters.Scene["Jarkos:GroundPulse"].Deactivate();
            GameShaders.Misc.Remove(VortexShaderKey);
            GameShaders.Misc.Remove(GlowShaderKey);
            UITextures.Unload();
        }
    }
    public class PollenCurrency : CustomCurrencySingleCoin
    {
        public PollenCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap)
        {
            this.CurrencyTextKey = CurrencyTextKey;
            CurrencyTextColor = Color.Gold;
        }
    }
}
