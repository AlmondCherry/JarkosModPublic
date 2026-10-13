using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;
using Jarkos.tiles;
using Terraria.ID;

namespace jarkos.tiles
{
    public class EssenceSpreaderTileEntity : ModTileEntity
    {
        public Dictionary<EssenceType, int> StoredEssences = new Dictionary<EssenceType, int>();
        public const int MaxCapacity = 150;
        public const float SuckRadius = 8.5f * 16f;
        public float TargetRotation = 0f;
        public int AimingPlayer = -1;
        public float VisualHitDistance = 50 * 16f;
        public bool IsTargetFull = false;
        public int BeamMode = 0; 
        public bool ShowBeams
        {
            get => BeamMode == 1;
            set => BeamMode = value ? 1 : 0;
        }
        public EssenceType FilterType = (EssenceType)(-1);
        public List<Vector2> ActiveBumps = new List<Vector2>();
        private int _timer = 0;
        public int TotalEssences => StoredEssences.Values.Sum();
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<EssenceSpreaderTile>();
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
                return -1;
            }
            return Place(i, j);
        }
        private int GetStateHash()
        {
            int hash = 0;
            foreach (var kvp in StoredEssences) hash += (int)kvp.Key * 1000 + kvp.Value;
            return hash;
        }

        public void SyncData()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.SpreaderSync);
                packet.Write(Position.X);
                packet.Write(Position.Y);
                packet.Write(TargetRotation);
                packet.Write((int)FilterType);
                packet.Write(AimingPlayer);
                packet.Write(BeamMode);
                packet.Send();
            }
        }
        public override void Update()
        {

            for (int i = ActiveBumps.Count - 1; i >= 0; i--)
            {
                Vector2 b = ActiveBumps[i];
                b.X += 0.015f * b.Y;
                ActiveBumps[i] = b;
                if (b.X < 0.0f || b.X > 1.0f) ActiveBumps.RemoveAt(i);
            }
            UpdateVisualHitDistance();
            _timer++;
            if (_timer >= 12)
            {
                _timer = 0;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int oldHash = GetStateHash();
                    SuckEssence();
                    PushEssence();
                    if (Main.netMode == NetmodeID.Server && GetStateHash() != oldHash)
                    {
                        SyncData();
                    }
                }
            }
        }
        public void UpdateVisualHitDistance()
        {
            Vector2 myPos = new Vector2(Position.X * 16 + 8, Position.Y * 16 + 8);
            Vector2 dir = TargetRotation.ToRotationVector2();
            float maxRayDist = 50 * 16f;
            VisualHitDistance = maxRayDist;
            for (float dist = 16f; dist <= maxRayDist; dist += 8f)
            {
                Vector2 targetPos = myPos + dir * dist;
                int tx = (int)(targetPos.X / 16);
                int ty = (int)(targetPos.Y / 16);
                if (!WorldGen.InWorld(tx, ty))
                {
                    VisualHitDistance = dist;
                    break;
                }
                Tile tile = Main.tile[tx, ty];
                if (tile.HasTile)
                {
                    if (tile.TileType == ModContent.TileType<DistillerTile>())
                    {
                        bool ignored = false;
                        foreach (var kvp in ByID)
                        {
                            if (kvp.Value is DistillerTileEntity dte && Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(dte.Position.X, dte.Position.Y)) <= 25f)
                            {
                                if (dte.IgnoreLaser) ignored = true;
                                break;
                            }
                        }
                        if (ignored) continue;
                        VisualHitDistance = dist;
                        break;
                    }
                    else if (tile.TileType == ModContent.TileType<DaylightGeneratorTile>())
                    {
                        bool ignored = false;
                        foreach (var kvp in ByID)
                        {
                            if (kvp.Value is DaylightGeneratorTileEntity dte && Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(dte.Position.X, dte.Position.Y)) <= 25f)
                            {
                                if (dte.IgnoreLaser) ignored = true;
                                break;
                            }
                        }
                        if (ignored) continue;
                        VisualHitDistance = dist;
                        break;
                    }
                    else if (tile.TileType == ModContent.TileType<DaylightStationTile>())
                    {
                        bool ignored = false;
                        foreach (var kvp in ByID)
                        {
                            if (kvp.Value is DaylightStationTileEntity dte && Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(dte.Position.X, dte.Position.Y)) <= 25f)
                            {
                                if (dte.IgnoreLaser) ignored = true;
                                break;
                            }
                        }
                        if (ignored) continue;
                        VisualHitDistance = dist;
                        break;
                    }
                    else if (tile.TileType == ModContent.TileType<AlchemicalEnchantmentTile>() || tile.TileType == ModContent.TileType<ResearchTableTile>() || tile.TileType == ModContent.TileType<EssenceFlaskTile>() || tile.TileType == ModContent.TileType<LargeEssenceFlaskTile>() || tile.TileType == ModContent.TileType<ReplicatorTile>())
                    {
                        VisualHitDistance = dist;
                        break;
                    }

                    if (Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        VisualHitDistance = dist;
                        break;
                    }
                }
            }
        }
        private void SuckEssence()
        {
            if (TotalEssences >= MaxCapacity) return;

            float maxDistSq = SuckRadius * SuckRadius;
            Vector2 myPos = new Vector2(Position.X * 16 + 8, Position.Y * 16 + 8);

            foreach (var kvp in ByID)
            {
                if (kvp.Value is EssenceFlaskTileEntity flask)
                {
                    Vector2 flaskPos = new Vector2(flask.Position.X * 16 + 8, flask.Position.Y * 16 + 8);
                    if (Vector2.DistanceSquared(myPos, flaskPos) <= maxDistSq)
                    {
                        if ((int)flask.storedType != -1 && flask.storedAmount > 0)
                        {
                            if ((int)FilterType != -1 && flask.storedType != FilterType) continue;

                            int activeTypes = StoredEssences.Count(kv => kv.Value > 0);
                            bool hasType = StoredEssences.ContainsKey(flask.storedType) && StoredEssences[flask.storedType] > 0;

                            if (!hasType && activeTypes >= 3) continue;
                            if (!StoredEssences.ContainsKey(flask.storedType)) StoredEssences[flask.storedType] = 0;

                            StoredEssences[flask.storedType]++;
                            flask.storedAmount--;

                            if (flask.storedAmount <= 0) flask.storedType = (EssenceType)(-1);

                            SpawnTransferParticles(flaskPos, myPos, flask.storedType);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, flask.ID, flask.Position.X, flask.Position.Y);
                            return;
                        }
                    }
                }
                else if (kvp.Value is LargeEssenceFlaskTileEntity largeFlask)
                {
                    Vector2 largeFlaskPos = new Vector2(largeFlask.Position.X * 16 + 24, largeFlask.Position.Y * 16 + 24);
                    if (Vector2.DistanceSquared(myPos, largeFlaskPos) <= maxDistSq)
                    {
                        var availableTypes = largeFlask.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();

                        if ((int)FilterType != -1) availableTypes.RemoveAll(t => t != FilterType);

                        int activeTypes = StoredEssences.Count(kv => kv.Value > 0);

                        availableTypes.RemoveAll(t => !(StoredEssences.ContainsKey(t) && StoredEssences[t] > 0) && activeTypes >= 3);

                        if (availableTypes.Count > 0)
                        {
                            EssenceType typeToSuck = availableTypes[Main.rand.Next(availableTypes.Count)];

                            if (!StoredEssences.ContainsKey(typeToSuck)) StoredEssences[typeToSuck] = 0;

                            StoredEssences[typeToSuck]++;
                            largeFlask.StoredEssences[typeToSuck]--;

                            SpawnTransferParticles(largeFlaskPos, myPos, typeToSuck);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, largeFlask.ID, largeFlask.Position.X, largeFlask.Position.Y);
                            return;
                        }
                    }
                }
                else if (kvp.Value is AlchemicalEnchantmentTE ate)
                {

                    if (ate.PullEssenceMode) continue;

                    Vector2 atePos = new Vector2(ate.Position.X * 16 + 32, ate.Position.Y * 16 + 16);
                    float distMax = 8.5f * 16f;
                    if (Vector2.DistanceSquared(myPos, atePos) <= distMax * distMax)
                    {
                        var availableTypes = ate.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
                        if ((int)FilterType != -1) availableTypes.RemoveAll(t => t != FilterType);
                        int activeTypes = StoredEssences.Count(kv => kv.Value > 0);
                        availableTypes.RemoveAll(t => !(StoredEssences.ContainsKey(t) && StoredEssences[t] > 0) && activeTypes >= 3);
                        if (availableTypes.Count > 0)
                        {
                            EssenceType typeToSuck = availableTypes[Main.rand.Next(availableTypes.Count)];
                            if (!StoredEssences.ContainsKey(typeToSuck)) StoredEssences[typeToSuck] = 0;
                            StoredEssences[typeToSuck]++;
                            ate.StoredEssences[typeToSuck]--;
                            SpawnTransferParticles(atePos, myPos, typeToSuck);
                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ate.ID, ate.Position.X, ate.Position.Y);
                            return;
                        }
                    }
                }
                else if (kvp.Value is DistillerTileEntity distiller)
                {
                    if (distiller.IgnoreLaser) continue;

                    Vector2 distPos = new Vector2(distiller.Position.X * 16 + 16, distiller.Position.Y * 16 + 32);
                    float distMax = 8.5f * 16f;

                    if (Vector2.DistanceSquared(myPos, distPos) <= distMax * distMax)
                    {
                        var availableTypes = distiller.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();

                        if ((int)FilterType != -1) availableTypes.RemoveAll(t => t != FilterType);

                        int activeTypes = StoredEssences.Count(kv => kv.Value > 0);
                        availableTypes.RemoveAll(t => !(StoredEssences.ContainsKey(t) && StoredEssences[t] > 0) && activeTypes >= 3);

                        if (availableTypes.Count > 0)
                        {
                            EssenceType typeToSuck = availableTypes[Main.rand.Next(availableTypes.Count)];

                            if (!StoredEssences.ContainsKey(typeToSuck)) StoredEssences[typeToSuck] = 0;
                            StoredEssences[typeToSuck]++;
                            distiller.StoredEssences[typeToSuck]--;
                            SpawnTransferParticles(distPos, myPos, typeToSuck);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, distiller.ID, distiller.Position.X, distiller.Position.Y);
                            return;
                        }
                    }
                }
                else if (kvp.Value is DaylightGeneratorTileEntity daylightgen)
                {
                    if (daylightgen.IgnoreLaser) continue;

                    Vector2 distPos = new Vector2(daylightgen.Position.X * 16 + 16, daylightgen.Position.Y * 16 + 32);
                    float distMax = 8.5f * 16f;

                    if (Vector2.DistanceSquared(myPos, distPos) <= distMax * distMax)
                    {
                        var availableTypes = daylightgen.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();

                        if ((int)FilterType != -1) availableTypes.RemoveAll(t => t != FilterType);

                        int activeTypes = StoredEssences.Count(kv => kv.Value > 0);
                        availableTypes.RemoveAll(t => !(StoredEssences.ContainsKey(t) && StoredEssences[t] > 0) && activeTypes >= 3);

                        if (availableTypes.Count > 0)
                        {
                            EssenceType typeToSuck = availableTypes[Main.rand.Next(availableTypes.Count)];

                            if (!StoredEssences.ContainsKey(typeToSuck)) StoredEssences[typeToSuck] = 0;
                            StoredEssences[typeToSuck]++;
                            daylightgen.StoredEssences[typeToSuck]--;
                            SpawnTransferParticles(distPos, myPos, typeToSuck);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, daylightgen.ID, daylightgen.Position.X, daylightgen.Position.Y);
                            return;
                        }
                    }
                }
                else if (kvp.Value is DaylightStationTileEntity daylightsta)
                {
                    if (daylightsta.IgnoreLaser) continue;

                    Vector2 distPos = new Vector2(daylightsta.Position.X * 16 + 16, daylightsta.Position.Y * 16 + 32);
                    float distMax = 8.5f * 16f;

                    if (Vector2.DistanceSquared(myPos, distPos) <= distMax * distMax)
                    {
                        var availableTypes = daylightsta.StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();

                        if ((int)FilterType != -1) availableTypes.RemoveAll(t => t != FilterType);

                        int activeTypes = StoredEssences.Count(kv => kv.Value > 0);
                        availableTypes.RemoveAll(t => !(StoredEssences.ContainsKey(t) && StoredEssences[t] > 0) && activeTypes >= 3);

                        if (availableTypes.Count > 0)
                        {
                            EssenceType typeToSuck = availableTypes[Main.rand.Next(availableTypes.Count)];

                            if (!StoredEssences.ContainsKey(typeToSuck)) StoredEssences[typeToSuck] = 0;
                            StoredEssences[typeToSuck]++;
                            daylightsta.StoredEssences[typeToSuck]--;
                            SpawnTransferParticles(distPos, myPos, typeToSuck);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, daylightsta.ID, daylightsta.Position.X, daylightsta.Position.Y);
                            return;
                        }
                    }
                }
            }
            for (int i = Position.X - 2; i <= Position.X + 2; i++)
            {
                for (int j = Position.Y - 2; j <= Position.Y + 2; j++)
                {
                    if (WorldGen.InWorld(i, j) && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == ModContent.TileType<ResearchTableTile>())
                    {
                        Tile hitTile = Main.tile[i, j];
                        int topX = i - hitTile.TileFrameX / 16;
                        int topY = j - hitTile.TileFrameY / 16;
                        int index = ModContent.GetInstance<ResearchTableTileEntity>().Find(topX, topY);
                        Player closestPlayer = null;
                        if (index != -1)
                        {
                            var te = (ResearchTableTileEntity)ByID[index];
                            if (!string.IsNullOrEmpty(te.OwnerName))
                            {
                                for (int p = 0; p < Main.maxPlayers; p++)
                                {
                                    if (Main.player[p].active && Main.player[p].name == te.OwnerName)
                                    {
                                        closestPlayer = Main.player[p];
                                        break;
                                    }
                                }
                            }
                        }
                        
                        
                        if (closestPlayer == null)
                        {
                            Vector2 tablePos = new Vector2(i * 16 + 8, j * 16 + 8);
                            float closestDist = 1000f * 1000f;
                            for (int p = 0; p < Main.maxPlayers; p++)
                            {
                                Player player = Main.player[p];
                                if (player.active && !player.dead)
                                {
                                    float dist = Vector2.DistanceSquared(player.Center, tablePos);
                                    if (dist < closestDist && dist < 1000f * 1000f)
                                    {
                                        closestDist = dist;
                                        closestPlayer = player;
                                    }
                                }
                            }
                        }
                        if (closestPlayer != null)
                        {
                            var rPlayer = closestPlayer.GetModPlayer<ResearchPlayer>();
                            var availableTypes = rPlayer.Essences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();

                            if ((int)FilterType != -1) availableTypes.RemoveAll(t => t != FilterType);
                            int activeTypes = StoredEssences.Count(kv => kv.Value > 0);
                            availableTypes.RemoveAll(t => !(StoredEssences.ContainsKey(t) && StoredEssences[t] > 0) && activeTypes >= 3);

                            if (availableTypes.Count > 0)
                            {
                                EssenceType typeToSuck = availableTypes[Main.rand.Next(availableTypes.Count)];

                                if (!StoredEssences.ContainsKey(typeToSuck)) 
                                StoredEssences[typeToSuck] = 0;

                                StoredEssences[typeToSuck]++;
                                rPlayer.AddEssence(typeToSuck, -1);

                                SpawnTransferParticles(new Vector2(i * 16 + 8, j * 16 + 8), myPos, typeToSuck);
                                return;
                            }
                        }
                    }
                }
            }
        }
        private void PushEssence()
        {
            IsTargetFull = false;

            if (TotalEssences <= 0) return;

            var availableTypes = StoredEssences.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();

            if (availableTypes.Count == 0) return;

            EssenceType typeToPush = availableTypes[Main.rand.Next(availableTypes.Count)];

            Vector2 myPos = new Vector2(Position.X * 16 + 8, Position.Y * 16 + 8);
            Vector2 dir = TargetRotation.ToRotationVector2();
            float maxRayDist = 50 * 16f;

            Vector2 targetPos = myPos;

            for (float dist = 16f; dist <= maxRayDist; dist += 8f)
            {
                targetPos = myPos + dir * dist;
                int tx = (int)(targetPos.X / 16);
                int ty = (int)(targetPos.Y / 16);

                if (!WorldGen.InWorld(tx, ty)) break;
                Tile tile = Main.tile[tx, ty];

                if (tile.HasTile)
                {
                    if (tile.TileType == ModContent.TileType<DistillerTile>())
                    {
                        DistillerTileEntity targetTE = null;
                        float closest = 1000f;
                        foreach(var kvp in ByID) 
                        {
                            if (kvp.Value is DistillerTileEntity dte) 
                            {
                                float tDist = Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(dte.Position.X, dte.Position.Y));
                                if (tDist < closest && tDist <= 25f) 
                                {
                                    closest = tDist;
                                    targetTE = dte;
                                }
                            }
                        }
                        if (targetTE != null)
                        {
                            if (targetTE.IgnoreLaser) continue;
                            if (targetTE.GetTotalEssences() >= DistillerTileEntity.MaxCapacity)
                            {
                                IsTargetFull = true;
                                return;
                            }

                            if (!targetTE.StoredEssences.ContainsKey(typeToPush)) targetTE.StoredEssences[typeToPush] = 0;
                            targetTE.StoredEssences[typeToPush]++;
                            StoredEssences[typeToPush]--;

                            SpawnTransferParticles(myPos, targetPos, typeToPush);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, targetTE.ID, targetTE.Position.X, targetTE.Position.Y);
                            return;
                        }
                    }
                    else if (tile.TileType == ModContent.TileType<AlchemicalEnchantmentTile>())
                    {
                        AlchemicalEnchantmentTE targetTE = null;
                        float closest = 1000f;
                        foreach(var kvp in ByID) 
                        {
                            if (kvp.Value is AlchemicalEnchantmentTE ate) 
                            {
                                float tDist = Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(ate.Position.X, ate.Position.Y));
                                if (tDist < closest && tDist <= 25f) 
                                {
                                    closest = tDist;
                                    targetTE = ate;
                                }
                            }
                        }
                        if (targetTE != null)
                        {
                            if (targetTE.GetTotalEssence() >= 1000)
                            {
                                IsTargetFull = true;
                                return;
                            }

                            if (!targetTE.StoredEssences.ContainsKey(typeToPush)) targetTE.StoredEssences[typeToPush] = 0;
                            targetTE.StoredEssences[typeToPush]++;
                            StoredEssences[typeToPush]--;

                            SpawnTransferParticles(myPos, targetPos, typeToPush);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, targetTE.ID, targetTE.Position.X, targetTE.Position.Y);
                            return;
                        }
                    }
                    else if (tile.TileType == ModContent.TileType<EssenceFlaskTile>())
                    {
                        EssenceFlaskTileEntity targetTE = null;
                        float closest = 1000f;
                        foreach(var kvp in ByID) 
                        {
                            if (kvp.Value is EssenceFlaskTileEntity flask) 
                            {
                                float tDist = Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(flask.Position.X, flask.Position.Y));
                                if (tDist < closest && tDist <= 25f) 
                                {
                                    closest = tDist;
                                    targetTE = flask;
                                }
                            }
                        }
                        if (targetTE != null)
                        {
                            if (targetTE.storedAmount >= EssenceFlaskTileEntity.MaxCapacity)
                            {
                                IsTargetFull = true;
                                return;
                            }
                            if ((int)targetTE.storedType == -1 || targetTE.storedType == typeToPush)
                            {
                                targetTE.storedType = typeToPush;
                                targetTE.storedAmount++;
                                StoredEssences[typeToPush]--;

                                SpawnTransferParticles(myPos, targetPos, typeToPush);

                                if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, targetTE.ID, targetTE.Position.X, targetTE.Position.Y);
                            }
                            return;
                        }
                    }
                    else if (tile.TileType == ModContent.TileType<ReplicatorTile>())
                    {
                        ReplicatorTileEntity targetTE = null;
                        float closest = 1000f;
                        foreach(var kvp in ByID) 
                        {
                            if (kvp.Value is ReplicatorTileEntity rte) 
                            {
                                float tDist = Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(rte.Position.X + 1, rte.Position.Y + 1));
                                if (tDist < closest && tDist <= 9f) 
                                {
                                    closest = tDist;
                                    targetTE = rte;
                                }
                            }
                        }
                        if (targetTE != null)
                        {
                            if (targetTE.GetTotalEssences() >= ReplicatorTileEntity.MaxCapacity)
                            {
                                IsTargetFull = true;
                                return;
                            }
                            if (!targetTE.StoredEssences.ContainsKey(typeToPush)) targetTE.StoredEssences[typeToPush] = 0;
                            targetTE.StoredEssences[typeToPush]++;
                            StoredEssences[typeToPush]--;

                            SpawnTransferParticles(myPos, targetPos, typeToPush);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, targetTE.ID, targetTE.Position.X, targetTE.Position.Y);
                            return;
                        }
                    }
                    else if (tile.TileType == ModContent.TileType<LargeEssenceFlaskTile>())
                    {
                        LargeEssenceFlaskTileEntity targetTE = null;
                        float closest = 1000f;
                        foreach(var kvp in ByID) 
                        {
                            if (kvp.Value is LargeEssenceFlaskTileEntity largeFlask) 
                            {
                                float tDist = Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(largeFlask.Position.X + 1, largeFlask.Position.Y + 1));
                                if (tDist < closest && tDist <= 9f) 
                                {
                                    closest = tDist;
                                    targetTE = largeFlask;
                                }
                            }
                        }
                        if (targetTE != null)
                        {
                            if (targetTE.TotalEssences >= LargeEssenceFlaskTileEntity.MaxCapacity)
                            {
                                IsTargetFull = true;
                                return;
                            }
                            int activeTypes = targetTE.StoredEssences.Count(kv => kv.Value > 0);
                            bool hasType = targetTE.StoredEssences.ContainsKey(typeToPush) && targetTE.StoredEssences[typeToPush] > 0;
                            if (hasType || activeTypes < LargeEssenceFlaskTileEntity.MaxTypes)
                            {
                                if (!targetTE.StoredEssences.ContainsKey(typeToPush)) targetTE.StoredEssences[typeToPush] = 0;
                                targetTE.StoredEssences[typeToPush]++;
                                StoredEssences[typeToPush]--;

                                SpawnTransferParticles(myPos, targetPos, typeToPush);

                                if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, targetTE.ID, targetTE.Position.X, targetTE.Position.Y);
                            }
                            return;
                        }
                    }
                    else if (tile.TileType == ModContent.TileType<ResearchTableTile>())
                    {
                        Tile hitTile = Main.tile[tx, ty];
                        int topX = tx - hitTile.TileFrameX / 16;
                        int topY = ty - hitTile.TileFrameY / 16;
                        int index = ModContent.GetInstance<ResearchTableTileEntity>().Find(topX, topY);
                        Player closestPlayer = null;

                        if (index != -1)
                        {
                            var te = (ResearchTableTileEntity)ByID[index];
                            if (!string.IsNullOrEmpty(te.OwnerName))
                            {
                                for (int p = 0; p < Main.maxPlayers; p++)
                                {
                                    if (Main.player[p].active && Main.player[p].name == te.OwnerName)
                                    {
                                        closestPlayer = Main.player[p];
                                        break;
                                    }
                                }
                            }
                        }
                        
                        
                        if (closestPlayer == null)
                        {
                            float closestDist = 1000f * 1000f;
                            Vector2 tablePos = new Vector2(tx * 16 + 8, ty * 16 + 8);
                            for (int p = 0; p < Main.maxPlayers; p++)
                            {
                                Player player = Main.player[p];
                                if (player.active && !player.dead)
                                {
                                    float pDist = Vector2.DistanceSquared(player.Center, tablePos);
                                    if (pDist < closestDist && pDist < 1000f * 1000f)
                                    {
                                        closestDist = pDist;
                                        closestPlayer = player;
                                    }
                                }
                            }
                        }
                        if (closestPlayer != null)
                        {
                            var rPlayer = closestPlayer.GetModPlayer<ResearchPlayer>();
                            if (rPlayer.GetEssence(typeToPush) >= rPlayer.GetCap(typeToPush))
                            {
                                IsTargetFull = true;
                                return;
                            }
                            rPlayer.AddEssence(typeToPush, 1);
                            StoredEssences[typeToPush]--;

                            SpawnTransferParticles(myPos, targetPos, typeToPush);
                            return;
                        }
                    }
                    else if (tile.TileType == ModContent.TileType<DaylightGeneratorTile>())
                    {
                        DaylightGeneratorTileEntity targetTE = null;
                        float closest = 1000f;
                        foreach(var kvp in ByID) {
                            if (kvp.Value is DaylightGeneratorTileEntity dte) 
                            {
                                float tDist = Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(dte.Position.X, dte.Position.Y));
                                if (tDist < closest && tDist <= 25f) 
                                {
                                    closest = tDist;
                                    targetTE = dte;
                                }
                            }
                        }
                        if (targetTE != null)
                        {
                            if (targetTE.IgnoreLaser) continue;
                            if (targetTE.GetTotalEssences() >= DistillerTileEntity.MaxCapacity)
                            {
                                IsTargetFull = true;
                                return;
                            }
                            if (!targetTE.StoredEssences.ContainsKey(typeToPush)) targetTE.StoredEssences[typeToPush] = 0;
                            targetTE.StoredEssences[typeToPush]++;
                            StoredEssences[typeToPush]--;
                            SpawnTransferParticles(myPos, targetPos, typeToPush);
                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, targetTE.ID, targetTE.Position.X, targetTE.Position.Y);
                            return;
                        }
                    }
                    else if (tile.TileType == ModContent.TileType<DaylightStationTile>())
                    {
                        DaylightStationTileEntity targetTE = null;
                        float closest = 1000f;
                        foreach(var kvp in ByID) 
                        {
                            if (kvp.Value is DaylightStationTileEntity dte) 
                            {
                                float tDist = Vector2.DistanceSquared(new Vector2(tx, ty), new Vector2(dte.Position.X, dte.Position.Y));
                                if (tDist < closest && tDist <= 25f) 
                                {
                                    closest = tDist;
                                    targetTE = dte;
                                }
                            }
                        }
                        if (targetTE != null)
                        {
                            if (targetTE.IgnoreLaser) continue;
                            if (targetTE.GetTotalEssences() >= DistillerTileEntity.MaxCapacity)
                            {
                                IsTargetFull = true;
                                return;
                            }

                            if (!targetTE.StoredEssences.ContainsKey(typeToPush)) targetTE.StoredEssences[typeToPush] = 0;
                            targetTE.StoredEssences[typeToPush]++;
                            StoredEssences[typeToPush]--;

                            SpawnTransferParticles(myPos, targetPos, typeToPush);

                            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, targetTE.ID, targetTE.Position.X, targetTE.Position.Y);
                            return;
                        }
                    }

                    if (Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        break;
                    }
                }
            }
        }
        public void SpawnTransferParticles(Vector2 startPos, Vector2 targetPos, EssenceType type)
        {
            Vector2 myPos = new Vector2(Position.X * 16 + 8, Position.Y * 16 + 8);
            if (Vector2.DistanceSquared(startPos, myPos) < 100f)
            {
                if (ActiveBumps.Count >= 4) ActiveBumps.RemoveAt(0);
                ActiveBumps.Add(new Vector2(0.0f, 1.0f));
            }
            else
            {
                if (ActiveBumps.Count >= 4) ActiveBumps.RemoveAt(0);
                ActiveBumps.Add(new Vector2(1.0f, -1.0f));
            }
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.SpreaderTransferVisual);
                packet.Write(Position.X);
                packet.Write(Position.Y);
                packet.WriteVector2(startPos);
                packet.WriteVector2(targetPos);
                packet.Write((byte)type);
                packet.Send();
                return;
            }
            Color color = EssencePalette.ColorOf(type);
            for (int i = 0; i < 3; i++)
            {
                int dust = Dust.NewDust(startPos, 16, 16, DustID.MagicMirror, 0f, 0f, 100, color, 1.2f);
                Main.dust[dust].noGravity = true;
                Vector2 dir = targetPos - startPos;
                dir.Normalize();
                Main.dust[dust].velocity = dir * Main.rand.NextFloat(2f, 4f);
                Main.dust[dust].color = color;
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag["TargetRotation"] = TargetRotation;
            tag["FilterType"] = (int)FilterType;
            tag["BeamMode"] = BeamMode;
            var keys = StoredEssences.Keys.Select(k => (int)k).ToList();
            var vals = StoredEssences.Values.ToList();
            tag["essKeys"] = keys;
            tag["essVals"] = vals;
        }
        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("TargetRotation"))
                TargetRotation = tag.GetFloat("TargetRotation");
            if (tag.ContainsKey("FilterType"))
                FilterType = (EssenceType)tag.GetInt("FilterType");
            if (tag.ContainsKey("BeamMode"))
                BeamMode = tag.GetInt("BeamMode");
            else if (tag.ContainsKey("ShowBeams"))
                BeamMode = tag.GetBool("ShowBeams") ? 1 : 0;
            StoredEssences.Clear();
            if (tag.ContainsKey("essKeys"))
            {
                var keys = tag.GetList<int>("essKeys");
                var vals = tag.GetList<int>("essVals");
                for (int i = 0; i < keys.Count && i < vals.Count; i++)
                    StoredEssences[(EssenceType)keys[i]] = vals[i];
            }
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(TargetRotation);
            writer.Write(AimingPlayer);
            writer.Write((int)FilterType);
            writer.Write(BeamMode);
            writer.Write(StoredEssences.Count);

            foreach (var kvp in StoredEssences)
            {
                writer.Write((int)kvp.Key);
                writer.Write(kvp.Value);
            }
        }
        public override void NetReceive(BinaryReader reader)
        {
            float rot = reader.ReadSingle();
            int aiming = reader.ReadInt32();
            int filter = reader.ReadInt32();
            int beamMode = reader.ReadInt32();

            if (AimingPlayer != Main.myPlayer)
            {
                TargetRotation = rot;
                AimingPlayer = aiming;
            }

            FilterType = (EssenceType)filter;
            BeamMode = beamMode;
            StoredEssences.Clear();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                EssenceType type = (EssenceType)reader.ReadInt32();
                int amount = reader.ReadInt32();
                StoredEssences[type] = amount;
            }
        }
    }
}
