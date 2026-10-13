using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
namespace jarkos.items.usable
{
	public class Laser_minecart : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.flightTimeMax = int.MaxValue;
            MountData.fatigueMax = int.MaxValue;
            MountData.fallDamage = 0f;
            MountData.usesHover = true;
            MountData.runSpeed = 12f;
            MountData.dashSpeed = 12f;
            MountData.acceleration = 0.3f;
            MountData.swimSpeed = 8f;
            MountData.jumpHeight = 10;
            MountData.jumpSpeed = 6f;
            MountData.blockExtraJumps = true;
            MountData.constantJump = true;
            MountData.heightBoost = 10;
            MountData.playerYOffsets = new int[] { 0, 0, 0, 0 };
            MountData.xOffset = 0;
            MountData.yOffset = -5;
            MountData.bodyFrame = 3;
            MountData.totalFrames = 4;
            MountData.standingFrameCount = 1;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            MountData.runningFrameCount = 4;
            MountData.runningFrameDelay = 6;
            MountData.runningFrameStart = 0;
            MountData.flyingFrameCount = 4;
            MountData.flyingFrameDelay = 4;
            MountData.flyingFrameStart = 0;
            MountData.inAirFrameCount = 4;
            MountData.inAirFrameDelay = 6;
            MountData.inAirFrameStart = 0;
            MountData.idleFrameCount = 1;
            MountData.idleFrameDelay = 12;
            MountData.idleFrameStart = 0;
            if (!Main.dedServ)
            {
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }
        }
        public override void UpdateEffects(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Center + new Vector2(0, 20);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(2, 4)), 0, default, 1.5f);
                dust.noGravity = true;
            }
            float flyAcceleration = 0.4f;
            float maxFlySpeed = 15f;
            if (player.controlUp)
            {
                player.velocity.Y -= flyAcceleration;
                if (player.velocity.Y < -maxFlySpeed)
                    player.velocity.Y = -maxFlySpeed;
            }
            if (player.controlDown)
            {
                player.velocity.Y += flyAcceleration;
                if (player.velocity.Y > maxFlySpeed)
                    player.velocity.Y = maxFlySpeed;
            }
            if (!player.controlUp && !player.controlDown)
            {
                player.velocity.Y *= 0.9f;
            }
            player.gravity = 0f;
        }
    }
	public enum LaserMode
    {
        Normal, Circular, Tentacle
    }
    public class LaserMountPlayer : ModPlayer
    {
        public LaserMode CurrentMode { get; set; } = LaserMode.Normal;
        public bool IsFiring { get; private set; }
        public float CurrentLaserLength { get; private set; }
        public float CurrentLaserWidth { get; private set; }
        public Vector2 LaserDirection { get; private set; }
        public float CircularRotation { get; private set; }
        public const int CircularLaserCount = 8;
        public const float CircularRotationSpeed = 0.03f;
        public const int TentacleSegments = 60;
        public Vector2[] TentaclePoints { get; private set; } = new Vector2[TentacleSegments];
        public const float TentacleSegmentLength = 5f;
        public const float TentacleFollowSpeed = 0.15f;
        public const float TentacleWidth = 5f;
        public const float TentacleMaxLength = 1000f;
        public const float NormalMaxLength = 4500f;
        public const float NormalMinLength = 50f;
        public const float NormalMaxWidth = 360f;
        public const float NormalMinWidth = 4f;
        public const float NormalGrowthSpeed = 35f;
        public const float NormalShrinkSpeed = 120f;
        public const float NormalWidthGrowth = 2.5f;
        public const float CircularMaxLength = 500f;
        public const float CircularMinLength = 80f;
        public const float CircularMaxWidth = 25f;
        public const float CircularMinWidth = 6f;
        public const float CircularGrowthSpeed = 10f;
        public const float CircularShrinkSpeed = 30f;
        public const int LaserDamage = 60;
        public const int BlockBreakRate = 2;
        private int soundTimer = 0;
        private bool tentacleInitialized = false;
        public override void PreUpdate()
        {
            bool mountActive = Player.mount.Active && Player.mount.Type == ModContent.MountType<Laser_minecart>();
            if (!mountActive)
            {
                ResetLaserState();
                LaserModeUI.CloseUI();
                return;
            }
            if (Main.playerInventory)
            {
                LaserModeUI.CloseUI();
            }
            HandleRightClick();
            if (LaserModeUI.IsOpen)
            {
                IsFiring = false;
                return;
            }
            Vector2 toMouse = Main.MouseWorld - Player.Center;
            if (toMouse != Vector2.Zero)
                toMouse.Normalize();
            LaserDirection = toMouse;
            if (CurrentMode != LaserMode.Circular)
            {
                if (Main.MouseWorld.X > Player.Center.X)
                    Player.direction = 1;
                else
                    Player.direction = -1;
            }
            if (!tentacleInitialized)
            {
                InitializeTentacle();
                tentacleInitialized = true;
            }
            UpdateTentacle();
            bool leftClick = Main.mouseLeft && !Player.mouseInterface;
            if (leftClick)
            {
                IsFiring = true;
                switch (CurrentMode)
                {
                    case LaserMode.Normal:
                        UpdateNormalMode();
                        break;
                    case LaserMode.Circular:
                        UpdateCircularMode();
                        break;
                    case LaserMode.Tentacle:
                        UpdateTentacleMode();
                        break;
                }
                PlayLaserSound();
            }
            else
            {
                IsFiring = false;
                soundTimer = 0;
                ShrinkLaser();
            }
            if (CurrentMode == LaserMode.Circular)
            {
                float rotSpeed = IsFiring ? CircularRotationSpeed : CircularRotationSpeed * 0.3f;
                CircularRotation += rotSpeed;
                if (CircularRotation > MathHelper.TwoPi)
                    CircularRotation -= MathHelper.TwoPi;
            }
        }
        private bool wasRightClicking = false;
		private void HandleRightClick()
		{
			bool rightClick = Main.mouseRight && !Player.mouseInterface && !Main.playerInventory;
			if (rightClick && !wasRightClicking && !LaserModeUI.IsOpen && !LaserModeUI.IsAnimating)
			{
				LaserModeUI.OpenUI(Player);
			}
			wasRightClicking = rightClick;
		}
        private void InitializeTentacle()
        {
            Vector2 startPos = Player.Center;
            for (int i = 0; i < TentacleSegments; i++)
            {
                TentaclePoints[i] = startPos + LaserDirection * (i * TentacleSegmentLength);
            }
        }
        private void UpdateTentacle()
        {
            TentaclePoints[0] = Player.Center;
            Vector2 targetPos = Main.MouseWorld;
            Vector2 toTarget = targetPos - Player.Center;
            if (toTarget.Length() > TentacleMaxLength)
            {
                toTarget.Normalize();
                targetPos = Player.Center + toTarget * TentacleMaxLength;
            }
            for (int i = 1; i < TentacleSegments; i++)
            {
                float progress = (float)i / (TentacleSegments - 1);
                Vector2 segmentTarget = Vector2.Lerp(TentaclePoints[i - 1], targetPos, progress);
                float followSpeed = TentacleFollowSpeed * (1f + progress * 0.5f);
                TentaclePoints[i] = Vector2.Lerp(TentaclePoints[i], segmentTarget, followSpeed);
                Vector2 diff = TentaclePoints[i] - TentaclePoints[i - 1];
                if (diff.Length() > TentacleSegmentLength)
                {
                    diff.Normalize();
                    TentaclePoints[i] = TentaclePoints[i - 1] + diff * TentacleSegmentLength;
                }
                else if (diff.Length() < TentacleSegmentLength * 0.5f && i < TentacleSegments - 1)
                {
                    if (diff != Vector2.Zero)
                    {
                        diff.Normalize();
                        TentaclePoints[i] = TentaclePoints[i - 1] + diff * TentacleSegmentLength * 0.5f;
                    }
                }
            }
        }
        private void ResetLaserState()
        {
            IsFiring = false;
            tentacleInitialized = false;
            ResetLaserParameters();
        }
        public void ResetLaserParameters()
        {
            switch (CurrentMode)
            {
                case LaserMode.Normal:
                    CurrentLaserLength = NormalMinLength;
                    CurrentLaserWidth = NormalMinWidth;
                    break;
                case LaserMode.Circular:
                    CurrentLaserLength = CircularMinLength;
                    CurrentLaserWidth = CircularMinWidth;
                    break;
                case LaserMode.Tentacle:
                    CurrentLaserLength = TentacleMaxLength;
                    CurrentLaserWidth = TentacleWidth;
                    break;
            }
        }
        private void UpdateNormalMode()
        {
            CurrentLaserLength += NormalGrowthSpeed;
            if (CurrentLaserLength > NormalMaxLength)
                CurrentLaserLength = NormalMaxLength;
            CurrentLaserWidth += NormalWidthGrowth;
            if (CurrentLaserWidth > NormalMaxWidth)
                CurrentLaserWidth = NormalMaxWidth;
            ApplyNormalLaserEffects();
        }
        private void UpdateCircularMode()
        {
            CurrentLaserLength += CircularGrowthSpeed;
            if (CurrentLaserLength > CircularMaxLength)
                CurrentLaserLength = CircularMaxLength;
            CurrentLaserWidth += NormalWidthGrowth * 0.8f;
            if (CurrentLaserWidth > CircularMaxWidth)
                CurrentLaserWidth = CircularMaxWidth;
            ApplyCircularLaserEffects();
        }
        private void UpdateTentacleMode()
        {
            CurrentLaserLength = TentacleMaxLength;
            CurrentLaserWidth = TentacleWidth;
            ApplyTentacleLaserEffects();
        }
        private void ShrinkLaser()
        {
            switch (CurrentMode)
            {
                case LaserMode.Normal:
                    CurrentLaserLength -= NormalShrinkSpeed;
                    if (CurrentLaserLength < NormalMinLength)
                        CurrentLaserLength = NormalMinLength;
                    CurrentLaserWidth -= NormalWidthGrowth * 2;
                    if (CurrentLaserWidth < NormalMinWidth)
                        CurrentLaserWidth = NormalMinWidth;
                    break;
                case LaserMode.Circular:
                    CurrentLaserLength -= CircularShrinkSpeed;
                    if (CurrentLaserLength < CircularMinLength)
                        CurrentLaserLength = CircularMinLength;
                    CurrentLaserWidth -= NormalWidthGrowth * 2;
                    if (CurrentLaserWidth < CircularMinWidth)
                        CurrentLaserWidth = CircularMinWidth;
                    break;
                case LaserMode.Tentacle:
                    break;
            }
        }
        private void PlayLaserSound()
        {
            soundTimer++;
            int soundInterval = CurrentMode == LaserMode.Tentacle ? 6 : 8;
            if (soundTimer >= soundInterval)
            {
                float pitch = CurrentMode switch
                {
                    LaserMode.Normal => 0.2f, LaserMode.Circular => 0f, LaserMode.Tentacle => -0.2f, _ => 0f
                };
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.4f, Pitch = pitch }, Player.Center);
                soundTimer = 0;
            }
        }
        private void ApplyNormalLaserEffects()
        {
            Vector2 laserStart = Player.Center;
            Vector2 laserEnd = GetLaserEndPoint(laserStart, LaserDirection, CurrentLaserLength);
            SpawnLaserDust(laserStart, laserEnd, CurrentLaserWidth, Color.Orange);
            if (Main.GameUpdateCount % BlockBreakRate == 0)
            {
                BreakBlocksAlongLaser(laserStart, laserEnd, (int)(CurrentLaserWidth / 16) + 1);
            }
            DamageEnemiesAlongLaser(laserStart, laserEnd, CurrentLaserWidth);
        }
        private void ApplyCircularLaserEffects()
        {
            for (int i = 0; i < CircularLaserCount; i++)
            {
                float angle = CircularRotation + (MathHelper.TwoPi / CircularLaserCount) * i;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 laserStart = Player.Center;
                Vector2 laserEnd = GetLaserEndPoint(laserStart, direction, CurrentLaserLength);
                Color laserColor = Main.hslToRgb((float)i / CircularLaserCount, 1f, 0.6f);
                SpawnLaserDust(laserStart, laserEnd, CurrentLaserWidth, laserColor);
                if (Main.GameUpdateCount % BlockBreakRate == 0)
                {
                    BreakBlocksAlongLaser(laserStart, laserEnd, (int)(CurrentLaserWidth / 16) + 1);
                }
                DamageEnemiesAlongLaser(laserStart, laserEnd, CurrentLaserWidth);
            }
        }
        private void ApplyTentacleLaserEffects()
        {
            for (int i = 0; i < TentacleSegments - 1; i++)
            {
                Vector2 start = TentaclePoints[i];
                Vector2 end = TentaclePoints[i + 1];
                float progress = (float)i / (TentacleSegments - 1);
                float segmentWidth = TentacleWidth * (1f - progress * 0.5f);
            }
            if (Main.GameUpdateCount % BlockBreakRate == 0)
            {
                BreakBlocksAlongTentacle();
            }
            DamageEnemiesAlongTentacle();
        }
        private void SpawnTentacleDust(Vector2 start, Vector2 end, float width)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            if (distance == 0) return;
            direction.Normalize();
            int dustCount = Math.Max(1, (int)(distance / 15));
            for (int i = 0; i < dustCount; i++)
            {
                float progress = (float)i / dustCount;
                Vector2 pos = Vector2.Lerp(start, end, progress);
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                pos += perpendicular * Main.rand.NextFloat(-width / 3, width / 3);
                Color color = Color.Lerp(Color.Cyan, Color.Purple, Main.rand.NextFloat(0.3f, 0.7f));
                Dust dust = Dust.NewDustPerfect(pos, DustID.PurpleTorch, perpendicular * Main.rand.NextFloat(-0.5f, 0.5f), 0, color, 1.3f);
                dust.noGravity = true;
                dust.noLight = false;
            }
        }
        private void BreakBlocksAlongTentacle()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            for (int i = 0; i < TentacleSegments - 1; i++)
            {
                Vector2 start = TentaclePoints[i];
                Vector2 end = TentaclePoints[i + 1];
                float progress = (float)i / (TentacleSegments - 1);
                int radius = Math.Max(1, (int)((TentacleWidth * (1f - progress * 0.5f)) / 16));
                BreakBlocksAlongSegment(start, end, radius);
            }
        }
        private void BreakBlocksAlongSegment(Vector2 start, Vector2 end, int radius)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            if (distance == 0) return;
            direction.Normalize();
            float step = 16f;
            for (float dist = 0; dist < distance; dist += step)
            {
                Vector2 checkPos = start + direction * dist;
                Point centerTile = checkPos.ToTileCoordinates();
                for (int xOff = -radius; xOff <= radius; xOff++)
                {
                    for (int yOff = -radius; yOff <= radius; yOff++)
                    {
                        if (xOff * xOff + yOff * yOff > radius * radius)
                            continue;
                        int tileX = centerTile.X + xOff;
                        int tileY = centerTile.Y + yOff;
                        if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY)
                        {
                            Tile tile = Main.tile[tileX, tileY];
                            if (tile.HasTile && Main.tileSolid[tile.TileType])
                            {
                                WorldGen.KillTile(tileX, tileY, false, false, false);
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, tileX, tileY);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void DamageEnemiesAlongTentacle()
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                for (int i = 0; i < TentacleSegments - 1; i++)
                {
                    float progress = (float)i / (TentacleSegments - 1);
                    float segmentWidth = TentacleWidth * (1f - progress * 0.5f);
                    if (IsNearLine(npc.Center, TentaclePoints[i], TentaclePoints[i + 1], segmentWidth + npc.width / 2))
                    {
                        if (Main.GameUpdateCount % 5 == 0)
                        {
                            int damage = (int)(LaserDamage * 1.5f * Player.GetDamage(DamageClass.Generic).Multiplicative);
                            npc.SimpleStrikeNPC(damage, Player.direction, false, 0, null, false, 0, true);
                            for (int d = 0; d < 6; d++)
                            {
                                Dust.NewDust(npc.position, npc.width, npc.height, DustID.PurpleTorch, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), 0, default, 1.5f);
                            }
                        }
                        break;
                    }
                }
            }
        }
        public Vector2 GetLaserEndPoint(Vector2 start, Vector2 direction, float maxDistance)
        {
            float step = 8f;
            for (float dist = 0; dist < maxDistance; dist += step)
            {
                Vector2 checkPos = start + direction * dist;
                Point tilePos = checkPos.ToTileCoordinates();
                if (tilePos.X >= 0 && tilePos.X < Main.maxTilesX && tilePos.Y >= 0 && tilePos.Y < Main.maxTilesY)
                {
                    Tile tile = Main.tile[tilePos.X, tilePos.Y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        return checkPos + direction * 8;
                    }
                }
            }
            return start + direction * maxDistance;
        }
        private void SpawnLaserDust(Vector2 start, Vector2 end, float width, Color baseColor)
        {
            float distance = Vector2.Distance(start, end);
            Vector2 direction = end - start;
            if (direction != Vector2.Zero)
                direction.Normalize();
            int dustCount = (int)(distance / 20);
            for (int i = 0; i < dustCount; i++)
            {
                float progress = (float)i / dustCount;
                Vector2 pos = start + direction * (progress * distance);
                float widthAtPoint = width * (0.5f + progress * 0.5f);
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                pos += perpendicular * Main.rand.NextFloat(-widthAtPoint / 2, widthAtPoint / 2);
                Dust dust = Dust.NewDustPerfect(pos, DustID.Torch, perpendicular * Main.rand.NextFloat(-1, 1), 0, baseColor, 1.2f + progress * 0.5f);
                dust.noGravity = true;
                dust.noLight = false;
            }
            for (int i = 0; i < 3; i++)
            {
                Dust hitDust = Dust.NewDustPerfect(end, DustID.Torch, Main.rand.NextVector2Circular(4, 4), 0, baseColor, 1.8f);
                hitDust.noGravity = true;
            }
        }
        private void BreakBlocksAlongLaser(Vector2 start, Vector2 end, int radius)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            float distance = Vector2.Distance(start, end);
            Vector2 direction = end - start;
            if (direction != Vector2.Zero)
                direction.Normalize();
            float step = 16f;
            for (float dist = 0; dist < distance; dist += step)
            {
                Vector2 checkPos = start + direction * dist;
                Point centerTile = checkPos.ToTileCoordinates();
                for (int xOff = -radius; xOff <= radius; xOff++)
                {
                    for (int yOff = -radius; yOff <= radius; yOff++)
                    {
                        if (xOff * xOff + yOff * yOff > radius * radius)
                            continue;
                        int tileX = centerTile.X + xOff;
                        int tileY = centerTile.Y + yOff;
                        if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY)
                        {
                            Tile tile = Main.tile[tileX, tileY];
                            if (tile.HasTile && Main.tileSolid[tile.TileType])
                            {
                                WorldGen.KillTile(tileX, tileY, false, false, false);
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, tileX, tileY);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void DamageEnemiesAlongLaser(Vector2 start, Vector2 end, float width)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                float hitWidth = width + npc.width / 2;
                if (IsNearLine(npc.Center, start, end, hitWidth))
                {
                    if (Main.GameUpdateCount % 5 == 0)
                    {
                        float damageMultiplier = CurrentMode switch
                        {
                            LaserMode.Normal => CurrentLaserWidth / NormalMinWidth, LaserMode.Circular => CurrentLaserWidth / CircularMinWidth * 0.7f, _ => 1f
                        };
                        int damage = (int)(LaserDamage * damageMultiplier * Player.GetDamage(DamageClass.Generic).Multiplicative);
                        npc.SimpleStrikeNPC(damage, Player.direction, false, 0, null, false, 0, true);
                        for (int i = 0; i < 6; i++)
                        {
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), 0, default, 1.5f);
                        }
                    }
                }
            }
        }
        private bool IsNearLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float maxDistance)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.Length();
            if (lineLength == 0)
                return Vector2.Distance(point, lineStart) <= maxDistance;
            Vector2 lineDir = line / lineLength;
            Vector2 toPoint = point - lineStart;
            float projection = Vector2.Dot(toPoint, lineDir);
            projection = MathHelper.Clamp(projection, 0, lineLength);
            Vector2 closestPoint = lineStart + lineDir * projection;
            return Vector2.Distance(point, closestPoint) <= maxDistance;
        }
        public List<(Vector2 start, Vector2 end, float width, Color color)> GetAllLaserBeams()
        {
            var beams = new List<(Vector2, Vector2, float, Color)>();
            if (CurrentMode == LaserMode.Tentacle)
            {
                if (!IsFiring)
                    return beams;
                for (int i = 0; i < TentacleSegments - 1; i++)
                {
                    float progress = (float)i / (TentacleSegments - 1);
                    float segmentWidth = TentacleWidth * (1f - progress * 0.5f);
                    Color color = Color.Lerp(Color.Cyan, Color.Purple, progress);
                    beams.Add((TentaclePoints[i], TentaclePoints[i + 1], segmentWidth, color));
                }
                return beams;
            }
            if (!IsFiring && CurrentLaserLength <= GetMinLength() + 5)
                return beams;
            Vector2 start = Player.Center;
            switch (CurrentMode)
            {
                case LaserMode.Normal:
                    Vector2 endNormal = GetLaserEndPoint(start, LaserDirection, CurrentLaserLength);
                    beams.Add((start, endNormal, CurrentLaserWidth, Color.Orange));
                    break;
                case LaserMode.Circular:
                    for (int i = 0; i < CircularLaserCount; i++)
                    {
                        float angle = CircularRotation + (MathHelper.TwoPi / CircularLaserCount) * i;
                        Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                        Vector2 endCirc = GetLaserEndPoint(start, dir, CurrentLaserLength);
                        Color color = Main.hslToRgb((float)i / CircularLaserCount, 1f, 0.6f);
                        beams.Add((start, endCirc, CurrentLaserWidth, color));
                    }
                    break;
            }
            return beams;
        }
        private float GetMinLength()
        {
            return CurrentMode switch
            {
                LaserMode.Normal => NormalMinLength, LaserMode.Circular => CircularMinLength, LaserMode.Tentacle => 0, _ => NormalMinLength
            };
        }
    }
    public class LaserMountGlobalItem : GlobalItem
    {
        public override bool CanUseItem(Item item, Player player)
        {
            if (player.mount.Active && player.mount.Type == ModContent.MountType<Laser_minecart>())
            {
                LaserMountPlayer modPlayer = player.GetModPlayer<LaserMountPlayer>();
                if (modPlayer.IsFiring || Main.mouseLeft || LaserModeUI.IsOpen)
                {
                    return false;
                }
            }
            return base.CanUseItem(item, player);
        }
    }
    public class LaserDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            if (!player.mount.Active || player.mount.Type != ModContent.MountType<Laser_minecart>())
                return false;
            LaserMountPlayer modPlayer = player.GetModPlayer<LaserMountPlayer>();
            var beams = modPlayer.GetAllLaserBeams();
            return beams.Count > 0;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            LaserMountPlayer modPlayer = player.GetModPlayer<LaserMountPlayer>();
            var beams = modPlayer.GetAllLaserBeams();
            bool isTentacle = modPlayer.CurrentMode == LaserMode.Tentacle;
            foreach (var beam in beams)
            {
                Vector2 start = beam.start - Main.screenPosition;
                Vector2 end = beam.end - Main.screenPosition;
                if (isTentacle)
                {
                    DrawTentacleSegment(Main.spriteBatch, start, end, beam.width, beam.color);
                }
                else
                {
                    DrawLaserBeam(Main.spriteBatch, start, end, beam.width, beam.color);
                }
            }
            if (!LaserModeUI.IsOpen)
            {
                DrawModeIndicator(Main.spriteBatch, player, modPlayer.CurrentMode);
            }
        }
        private void DrawTentacleSegment(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float width, Color baseColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 diff = end - start;
            float segmentLength = diff.Length();
            float rotation = diff.ToRotation();
            if (segmentLength < 1) return;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.3f + start.X * 0.01f) * 0.1f;
            Color outerColor = baseColor * 0.25f;
            outerColor.A = 20;
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor, rotation, new Vector2(0, 0.5f), new Vector2(segmentLength, width * 3f * pulse), SpriteEffects.None, 0f);
            Color midColor = baseColor * 0.5f;
            midColor.A = 60;
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), midColor, rotation, new Vector2(0, 0.5f), new Vector2(segmentLength, width * 1.8f * pulse), SpriteEffects.None, 0f);
            Color bodyColor = Color.Lerp(baseColor, Color.White, 0.3f);
            bodyColor.A = 200;
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), bodyColor, rotation, new Vector2(0, 0.5f), new Vector2(segmentLength, width * pulse), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * 0.9f, rotation, new Vector2(0, 0.5f), new Vector2(segmentLength, width * 0.4f * pulse), SpriteEffects.None, 0f);
        }
        private void DrawLaserBeam(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float width, Color baseColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 diff = end - start;
            float beamLength = diff.Length();
            float rotation = diff.ToRotation();
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.15f;
            float fastPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.8f) * 0.1f;
            Color outerColor = baseColor * 0.3f;
            outerColor.A = 30;
            Color middleColor = baseColor * 0.7f;
            middleColor.A = 80;
            Color coreColor = Color.Lerp(baseColor, Color.White, 0.5f);
            coreColor.A = 255;
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor * 0.4f, rotation, new Vector2(0, 0.5f), new Vector2(beamLength, width * 2.5f * pulse), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor * 0.6f, rotation, new Vector2(0, 0.5f), new Vector2(beamLength, width * 1.8f * pulse), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), middleColor * 0.8f, rotation, new Vector2(0, 0.5f), new Vector2(beamLength, width * 1.2f * fastPulse), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(beamLength, width * 0.5f * fastPulse), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White, rotation, new Vector2(0, 0.5f), new Vector2(beamLength, width * 0.15f), SpriteEffects.None, 0f);
            float sourceSize = width * 0.6f * pulse;
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * 0.9f, 0f, new Vector2(0.5f, 0.5f), new Vector2(sourceSize, sourceSize), SpriteEffects.None, 0f);
            float hitPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.6f) * 0.25f;
            float hitSize = width * 1.2f * hitPulse;
            spriteBatch.Draw(pixel, end, new Rectangle(0, 0, 1, 1), outerColor * 0.6f, Main.GameUpdateCount * 0.1f, new Vector2(0.5f, 0.5f), new Vector2(hitSize * 1.8f, hitSize * 1.8f), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, end, new Rectangle(0, 0, 1, 1), coreColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(hitSize, hitSize), SpriteEffects.None, 0f);
        }
        private void DrawModeIndicator(SpriteBatch spriteBatch, Player player, LaserMode mode)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 indicatorPos = player.Center - Main.screenPosition + new Vector2(0, -50);
            Color indicatorColor = mode switch
            {
                LaserMode.Normal => Color.Orange, LaserMode.Circular => Color.Magenta, LaserMode.Tentacle => Color.Cyan, _ => Color.White
            };
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            spriteBatch.Draw(pixel, indicatorPos, new Rectangle(0, 0, 1, 1), indicatorColor * 0.3f, 0f, new Vector2(0.5f, 0.5f), new Vector2(20 * pulse, 20 * pulse), SpriteEffects.None, 0f);
            switch (mode)
            {
                case LaserMode.Normal:
                    spriteBatch.Draw(pixel, indicatorPos, new Rectangle(0, 0, 1, 1), indicatorColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(12, 4), SpriteEffects.None, 0f);
                    break;
                case LaserMode.Circular:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = (MathHelper.TwoPi / 8) * i + Main.GameUpdateCount * 0.05f;
                        Vector2 dotPos = indicatorPos + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8;
                        spriteBatch.Draw(pixel, dotPos, new Rectangle(0, 0, 1, 1), indicatorColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(3, 3), SpriteEffects.None, 0f);
                    }
                    break;
                case LaserMode.Tentacle:
                    for (int i = 0; i < 5; i++)
                    {
                        float xOff = (i - 2) * 4;
                        float yOff = (float)Math.Sin(i * 0.8f + Main.GameUpdateCount * 0.1f) * 4;
                        Vector2 segPos = indicatorPos + new Vector2(xOff, yOff);
                        spriteBatch.Draw(pixel, segPos, new Rectangle(0, 0, 1, 1), indicatorColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(4, 4), SpriteEffects.None, 0f);
                    }
                    break;
            }
        }
    }
	#region UI
	public class LaserModeButton
    {
        public LaserMode Mode { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public float Angle { get; set; }
        public float TargetAngle { get; set; }
        public float Scale { get; set; } = 0f;
        public float Radius { get; set; } = 100f;
        public bool WasHovered { get; set; } = false;
        public Vector2 GetPosition(Vector2 center)
        {
            return center + new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle)) * Radius * Scale;
        }
        public bool IsHovered(Vector2 mousePos, Vector2 center)
        {
            if (Scale < 0.9f) return false;
            Vector2 buttonPos = GetPosition(center);
            float buttonSize = 35f * Scale;
            return Vector2.Distance(mousePos, buttonPos) <= buttonSize;
        }
    }
    public static class LaserModeUI
    {
        public static bool IsOpen { get; private set; } = false;
        public static bool IsAnimating { get; private set; } = false;
        public static bool CanSelect { get; private set; } = false;
        private static List<LaserModeButton> buttons = new List<LaserModeButton>();
        private static Player targetPlayer;
        private static float animationProgress = 0f;
        private static float spinAngle = 0f;
        private static float spinVelocity = 0f;
        private const float InitialSpinSpeed = 0.12f;
        private const float SpinDeceleration = 0.0008f;
        private const float OpenAnimationSpeed = 0.025f;
        private const float CloseAnimationSpeed = 0.08f;
        private const float SettleSpeed = 0.03f;
        private static bool isClosing = false;
        private static bool wasMouseDown = false;
        private static int clickCooldown = 0;
        public static void OpenUI(Player player)
        {
            if (IsOpen || IsAnimating) return;
            targetPlayer = player;
            IsOpen = true;
            IsAnimating = true;
            CanSelect = false;
            isClosing = false;
            animationProgress = 0f;
            spinAngle = 0f;
            spinVelocity = InitialSpinSpeed;
            wasMouseDown = true;
            clickCooldown = 15;
            buttons.Clear();
            buttons.Add(new LaserModeButton            {
                Mode = LaserMode.Normal, Name = "Normal", Color = Color.Orange, Angle = 0, TargetAngle = -MathHelper.PiOver2, Scale = 0f
            });
            buttons.Add(new LaserModeButton            {
                Mode = LaserMode.Circular, Name = "Circular", Color = Color.Magenta, Angle = 0, TargetAngle = -MathHelper.PiOver2 + MathHelper.TwoPi / 3f, Scale = 0f
            });
            buttons.Add(new LaserModeButton            {
                Mode = LaserMode.Tentacle, Name = "Tentacle", Color = Color.Cyan, Angle = 0, TargetAngle = -MathHelper.PiOver2 - MathHelper.TwoPi / 3f, Scale = 0f
            });
            SoundEngine.PlaySound(SoundID.MenuOpen with { Pitch = 0.3f, Volume = 0.7f });
        }
        public static void CloseUI()
        {
            if (!IsOpen) return;
            if (!isClosing)
            {
                isClosing = true;
                IsAnimating = true;
                CanSelect = false;
                SoundEngine.PlaySound(SoundID.MenuClose with { Pitch = 0.3f, Volume = 0.7f });
            }
        }
        private static void ForceClose()
        {
            IsOpen = false;
            IsAnimating = false;
            CanSelect = false;
            isClosing = false;
            buttons.Clear();
            targetPlayer = null;
            wasMouseDown = false;
        }
        public static void Update()
        {
            if (!IsOpen) return;
            if (clickCooldown > 0)
                clickCooldown--;
            if (targetPlayer != null && targetPlayer.whoAmI == Main.myPlayer)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            if (targetPlayer == null || !targetPlayer.active || !targetPlayer.mount.Active || targetPlayer.mount.Type != ModContent.MountType<Laser_minecart>() || Main.playerInventory)
            {
                ForceClose();
                return;
            }
            if (isClosing)
            {
                UpdateClosingAnimation();
                return;
            }
            UpdateOpeningAnimation();
            HandleClicks();
            HandleHover();
        }
        private static void UpdateClosingAnimation()
        {
            animationProgress -= CloseAnimationSpeed;
            foreach (var button in buttons)
            {
                button.Scale = MathHelper.SmoothStep(0, 1, Math.Max(0, animationProgress));
            }
            spinAngle += spinVelocity * 2f;
            if (animationProgress <= 0)
            {
                ForceClose();
            }
        }
        private static void UpdateOpeningAnimation()
        {
            animationProgress += OpenAnimationSpeed;
            if (animationProgress > 1f)
                animationProgress = 1f;
            spinAngle += spinVelocity;
            spinVelocity -= SpinDeceleration;
            if (spinVelocity < 0)
                spinVelocity = 0;
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                button.Scale = MathHelper.SmoothStep(0, 1, animationProgress);
                float baseAngle = button.TargetAngle + spinAngle;
                if (spinVelocity <= 0.001f)
                {
                    float currentAngle = button.Angle;
                    float targetAngle = button.TargetAngle;
                    float angleDiff = targetAngle - currentAngle;
                    while (angleDiff > MathHelper.Pi) angleDiff -= MathHelper.TwoPi;
                    while (angleDiff < -MathHelper.Pi) angleDiff += MathHelper.TwoPi;
                    button.Angle = currentAngle + angleDiff * SettleSpeed;
                    if (Math.Abs(angleDiff) < 0.01f)
                    {
                        button.Angle = targetAngle;
                    }
                }
                else
                {
                    button.Angle = baseAngle;
                }
            }
            if (spinVelocity <= 0 && animationProgress >= 1f)
            {
                bool allSettled = true;
                foreach (var button in buttons)
                {
                    float angleDiff = Math.Abs(button.TargetAngle - button.Angle);
                    while (angleDiff > MathHelper.Pi) angleDiff = MathHelper.TwoPi - angleDiff;
                    if (angleDiff > 0.02f)
                    {
                        allSettled = false;
                        break;
                    }
                }
                if (allSettled)
                {
                    IsAnimating = false;
                    CanSelect = true;
                    foreach (var button in buttons)
                    {
                        button.Angle = button.TargetAngle;
                    }
                }
            }
        }
        private static void HandleClicks()
        {
            bool mouseDown = Main.mouseLeft;
            bool justClicked = !mouseDown && wasMouseDown && clickCooldown <= 0;
            wasMouseDown = mouseDown;
            if (!CanSelect || !justClicked) return;
            Vector2 center = targetPlayer.Center - Main.screenPosition;
            Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
            foreach (var button in buttons)
            {
                if (button.IsHovered(mousePos, center))
                {
                    SelectMode(button.Mode);
                    return;
                }
            }
            float distanceFromCenter = Vector2.Distance(mousePos, center);
            if (distanceFromCenter > 160f || distanceFromCenter < 40f)
            {
                CloseUI();
            }
        }
        private static void HandleHover()
        {
            if (!CanSelect) return;
            Vector2 center = targetPlayer.Center - Main.screenPosition;
            Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
            foreach (var button in buttons)
            {
                bool isHovered = button.IsHovered(mousePos, center);
                if (isHovered && !button.WasHovered)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.5f, Pitch = 0.3f });
                }
                button.WasHovered = isHovered;
            }
        }
        private static void SelectMode(LaserMode mode)
        {
            if (targetPlayer == null) return;
            var modPlayer = targetPlayer.GetModPlayer<LaserMountPlayer>();
            modPlayer.CurrentMode = mode;
            modPlayer.ResetLaserParameters();
            string modeName = mode switch
            {
                LaserMode.Normal => "Normal Mode", LaserMode.Circular => "Circular Mode", LaserMode.Tentacle => "Tentacle Mode", _ => ""
            };
            Color textColor = mode switch
            {
                LaserMode.Normal => Color.Orange, LaserMode.Circular => Color.Magenta, LaserMode.Tentacle => Color.Cyan, _ => Color.White
            };
            CombatText.NewText(targetPlayer.Hitbox, textColor, modeName);
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.5f, Volume = 0.8f });
            CloseUI();
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (!IsOpen || targetPlayer == null) return;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 center = targetPlayer.Center - Main.screenPosition;
            Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
            float globalAlpha = MathHelper.SmoothStep(0, 1, animationProgress);
            DrawRadialGradient(spriteBatch, center, 200f * globalAlpha, Color.Black * 0.4f * globalAlpha);
            float centerPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.08f;
            DrawRing(spriteBatch, center, 95f * globalAlpha, 105f * globalAlpha, Color.White * 0.2f * globalAlpha);
            spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), Color.White * 0.15f * globalAlpha, 0f, new Vector2(0.5f, 0.5f), new Vector2(60 * centerPulse * globalAlpha, 60 * centerPulse * globalAlpha), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), Color.White * 0.3f * globalAlpha, 0f, new Vector2(0.5f, 0.5f), new Vector2(35 * centerPulse * globalAlpha, 35 * centerPulse * globalAlpha), SpriteEffects.None, 0f);
            foreach (var button in buttons)
            {
                if (button.Scale <= 0.1f) continue;
                Vector2 buttonPos = button.GetPosition(center);
                Color lineColor = button.Color * 0.3f * button.Scale;
                DrawDashedLine(spriteBatch, center, buttonPos, lineColor, 3f, 8f, 4f);
            }
            foreach (var button in buttons)
            {
                if (button.Scale <= 0.1f) continue;
                Vector2 buttonPos = button.GetPosition(center);
                bool isHovered = CanSelect && button.IsHovered(mousePos, center);
                DrawButton(spriteBatch, buttonPos, button, isHovered);
            }
            if (targetPlayer != null && globalAlpha > 0.5f)
            {
                var modPlayer = targetPlayer.GetModPlayer<LaserMountPlayer>();
                Color currentColor = modPlayer.CurrentMode switch
                {
                    LaserMode.Normal => Color.Orange, LaserMode.Circular => Color.Magenta, LaserMode.Tentacle => Color.Cyan, _ => Color.White
                };
                DrawModeIconSmall(spriteBatch, center, modPlayer.CurrentMode, currentColor * globalAlpha, 12f);
            }
            if (globalAlpha > 0.7f)
            {
                string hint = CanSelect ? "Click to select" : "...";
                Color hintColor = CanSelect ? Color.White * 0.9f : Color.Gray * 0.5f;
                Vector2 hintPos = center + new Vector2(0, 140);
                DrawTextWithShadow(spriteBatch, hint, hintPos, hintColor * globalAlpha, 0.85f);
            }
        }
        private static void DrawButton(SpriteBatch spriteBatch, Vector2 position, LaserModeButton button, bool isHovered)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float baseSize = 32f * button.Scale;
            float hoverScale = isHovered ? 1.2f : 1f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + button.Angle) * 0.05f;
            float size = baseSize * hoverScale * pulse;
            spriteBatch.Draw(pixel, position + new Vector2(3, 3), new Rectangle(0, 0, 1, 1), Color.Black * 0.3f * button.Scale, 0f, new Vector2(0.5f, 0.5f), new Vector2(size * 1.8f, size * 1.8f), SpriteEffects.None, 0f);
            Color glowColor = button.Color * 0.25f * button.Scale;
            float glowPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            spriteBatch.Draw(pixel, position, new Rectangle(0, 0, 1, 1), glowColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(size * 2.5f * glowPulse, size * 2.5f * glowPulse), SpriteEffects.None, 0f);
            Color bgColor = isHovered ? Color.Lerp(button.Color, Color.White, 0.2f) : button.Color;
            spriteBatch.Draw(pixel, position, new Rectangle(0, 0, 1, 1), bgColor * 0.7f * button.Scale, 0f, new Vector2(0.5f, 0.5f), new Vector2(size * 1.6f, size * 1.6f), SpriteEffects.None, 0f);
            Color innerColor = isHovered ? Color.White : Color.Lerp(button.Color, Color.White, 0.5f);
            spriteBatch.Draw(pixel, position, new Rectangle(0, 0, 1, 1), innerColor * 0.9f * button.Scale, 0f, new Vector2(0.5f, 0.5f), new Vector2(size * 1f, size * 1f), SpriteEffects.None, 0f);
            DrawModeIcon(spriteBatch, position, button.Mode, button.Color * button.Scale, size * 0.4f);
            if (button.Scale > 0.7f)
            {
                float textAlpha = (button.Scale - 0.7f) / 0.3f;
                Vector2 textPos = position + new Vector2(0, size * 1.6f + 18);
                Color textColor = isHovered ? Color.White : button.Color;
                float textScale = isHovered ? 1f : 0.9f;
                DrawTextWithShadow(spriteBatch, button.Name, textPos, textColor * textAlpha, textScale);
            }
        }
        private static void DrawModeIcon(SpriteBatch spriteBatch, Vector2 position, LaserMode mode, Color color, float size)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            switch (mode)
            {
                case LaserMode.Normal:
                    for (int i = 0; i < 3; i++)
                    {
                        float xOff = (i - 1) * size * 0.6f;
                        float yScale = 1f - Math.Abs(i - 1) * 0.3f;
                        spriteBatch.Draw(pixel, position + new Vector2(xOff, 0), new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f, 0.5f), new Vector2(size * 0.5f, size * yScale), SpriteEffects.None, 0f);
                    }
                    break;
                case LaserMode.Circular:
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = (MathHelper.TwoPi / 6) * i + Main.GameUpdateCount * 0.03f;
                        Vector2 dotPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * size * 0.9f;
                        spriteBatch.Draw(pixel, dotPos, new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f, 0.5f), new Vector2(size * 0.35f, size * 0.35f), SpriteEffects.None, 0f);
                    }
                    break;
                case LaserMode.Tentacle:
                    for (int i = 0; i < 5; i++)
                    {
                        float t = (float)i / 4f;
                        float xOff = (t - 0.5f) * size * 2f;
                        float yOff = (float)Math.Sin(t * MathHelper.Pi * 2 + Main.GameUpdateCount * 0.08f) * size * 0.4f;
                        float segSize = size * 0.35f * (1f - t * 0.3f);
                        Vector2 segPos = position + new Vector2(xOff, yOff);
                        spriteBatch.Draw(pixel, segPos, new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f, 0.5f), new Vector2(segSize, segSize), SpriteEffects.None, 0f);
                    }
                    break;
            }
        }
        private static void DrawModeIconSmall(SpriteBatch spriteBatch, Vector2 position, LaserMode mode, Color color, float size)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            switch (mode)
            {
                case LaserMode.Normal:
                    spriteBatch.Draw(pixel, position, new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f, 0.5f), new Vector2(size, size * 0.4f), SpriteEffects.None, 0f);
                    break;
                case LaserMode.Circular:
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = (MathHelper.TwoPi / 4) * i + Main.GameUpdateCount * 0.03f;
                        Vector2 dotPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * size * 0.6f;
                        spriteBatch.Draw(pixel, dotPos, new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f, 0.5f), new Vector2(size * 0.25f, size * 0.25f), SpriteEffects.None, 0f);
                    }
                    break;
                case LaserMode.Tentacle:
                    for (int i = 0; i < 3; i++)
                    {
                        float xOff = (i - 1) * size * 0.5f;
                        float yOff = (float)Math.Sin(i + Main.GameUpdateCount * 0.1f) * size * 0.3f;
                        spriteBatch.Draw(pixel, position + new Vector2(xOff, yOff), new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f, 0.5f), new Vector2(size * 0.3f, size * 0.3f), SpriteEffects.None, 0f);
                    }
                    break;
            }
        }
        private static void DrawRadialGradient(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 5; i >= 1; i--)
            {
                float r = radius * (i / 5f);
                float alpha = (6 - i) / 6f;
                spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), color * alpha * 0.5f, 0f, new Vector2(0.5f, 0.5f), new Vector2(r * 2, r * 2), SpriteEffects.None, 0f);
            }
        }
        private static void DrawRing(SpriteBatch spriteBatch, Vector2 center, float innerRadius, float outerRadius, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float angle = (MathHelper.TwoPi / segments) * i;
                float nextAngle = (MathHelper.TwoPi / segments) * (i + 1);
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (innerRadius + outerRadius) * 0.5f;
                float segmentLength = MathHelper.TwoPi * (innerRadius + outerRadius) * 0.5f / segments;
                spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), color, angle + MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(outerRadius - innerRadius, segmentLength * 1.1f), SpriteEffects.None, 0f);
            }
        }
        private static void DrawDashedLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float width, float dashLength, float gapLength)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 diff = end - start;
            float totalLength = diff.Length();
            float rotation = diff.ToRotation();
            if (totalLength < 1) return;
            Vector2 direction = diff / totalLength;
            float currentDist = 0;
            bool isDash = true;
            while (currentDist < totalLength)
            {
                float segmentLength = isDash ? dashLength : gapLength;
                segmentLength = Math.Min(segmentLength, totalLength - currentDist);
                if (isDash)
                {
                    Vector2 segStart = start + direction * currentDist;
                    spriteBatch.Draw(pixel, segStart, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2(segmentLength, width), SpriteEffects.None, 0f);
                }
                currentDist += segmentLength;
                isDash = !isDash;
            }
        }
        private static void DrawTextWithShadow(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 1f)
        {
            var font = FontAssets.MouseText.Value;
            Vector2 textSize = font.MeasureString(text);
            Vector2 origin = textSize / 2f;
        }
    }
	public class LaserUISystem : ModSystem
    {
        public override void UpdateUI(GameTime gameTime)
        {
            LaserModeUI.Update();
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("YourModName: Laser Mode UI", delegate
                    {
                        LaserModeUI.Draw(Main.spriteBatch);
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
	#endregion
}
