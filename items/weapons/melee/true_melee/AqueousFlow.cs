using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using Terraria.Audio;
using System;
using Terraria.Graphics.CameraModifiers;
using System.Collections.Generic;
using jarkos.buffs;
using jarkos.items.materials;
using System.IO;

namespace jarkos.items.weapons.melee.true_melee
{
	public class AqueousFlow : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 104;
            Item.height = 108;
            Item.damage = 70;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 7.5f;
            Item.crit = 12;
            Item.useTime = 13;
            Item.useAnimation = 13;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<AqueousFlowSwing>();
            Item.shootSpeed = 1f;
            Item.UseSound = null;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 2);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == type)
                    p.Kill();
            }

            var mp = player.GetModPlayer<AqueousComboPlayer>();
            int combo = mp.Combo;

            mp.Combo = (mp.Combo + 1) % 3;
            mp.ComboResetTimer = 75;

            float damageMult = combo == 2 ? 1.4f : 1f;

            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
            Projectile.NewProjectile(source, player.MountedCenter, dir, type, (int)(damage * damageMult), knockback, player.whoAmI, combo);
            return false;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            var mp = player.GetModPlayer<AqueousPathPlayer>();
            if (player.altFunctionUse == 2)
                return false;
            return mp.State == AqueousPathPlayer.PathState.None;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Muramasa)
                .AddIngredient(ModContent.ItemType<ElementalPowder>(), 2)
                .AddIngredient(ItemID.WaterBucket, 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
    public class AqueousComboPlayer : ModPlayer
    {
        public int Combo;
        public int ComboResetTimer;
        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
                ComboResetTimer--;
            else
                Combo = 0;
        }
    }
    public class AqueousPathPlayer : ModPlayer
    {
        public enum PathState { None, Drawing, Flying }
        public PathState State = PathState.None;
        public readonly List<Vector2> Path = new();
        public float[] CumLength = Array.Empty<float>();
        public float[] PointPhase = Array.Empty<float>();

        public float PathLength;
        public const float MaxPathLength = 2400f;
        public const float PointSpacing = 10f;
        public const float FlightSpeed = 38f;
        public const float HitRadius = 92f;
        public const float ExitPush = 21f;
        public float FlightDistance;
        public bool PhasingNow;
        public float PhaseGlow;
        public int PhasedTicks;
        public int Cooldown;
        public int DissolveTimer;
        public const int DissolveTime = 45;
        private int syncTimer;
        private Vector2 lockedCamera;
        private bool cameraLocked;
        private float cameraBlend = 1f;
        private readonly int[] npcHitCd = new int[Main.maxNPCs];
        private bool wasRight;
        private Vector2 flightPos;
        private bool HoldingKatana => Player.HeldItem?.ModItem is AqueousFlow;
        public override bool CanUseItem(Item item) => State == PathState.None;
        public override void UpdateDead()
        {
            State = PathState.None;
            DissolveTimer = 0;
            cameraBlend = 1f;
            Path.Clear();
        }
        public override void PreUpdate()
        {
            if (Cooldown > 0) Cooldown--;

            if (DissolveTimer > 0) DissolveTimer--;

            for (int i = 0; i < npcHitCd.Length; i++)
                if (npcHitCd[i] > 0) 
                npcHitCd[i]--;

            PhaseGlow = MathHelper.Lerp(PhaseGlow, PhasingNow ? 1f : 0f, 0.15f);

            if (cameraBlend < 1f) cameraBlend = Math.Min(1f, cameraBlend + 1f / 30f);
            
            if (State == PathState.Flying)
            {
                UpdateFlight();
            }

            if (Player.whoAmI != Main.myPlayer)
                return;

            bool right = Main.mouseRight && !Player.mouseInterface;

            switch (State)
            {
                case PathState.None:
                    if (right && !wasRight && HoldingKatana)
                    {
                        if (Cooldown <= 0) StartDrawing();
                        else SoundEngine.PlaySound(SoundID.MenuTick, Player.Center);
                    }
                    break;

                case PathState.Drawing:
                    if (!right)
                    {
                        if (Path.Count >= 3 && PathLength > 60f) StartFlight();
                        else CancelDrawing();
                    }
                    else UpdateDrawing();
                    break;
            }

            wasRight = right;
        }
        public override void PostUpdate()
        {
            if (State == PathState.Flying)
            {
                Player.Center = flightPos;
                Player.velocity = Vector2.Zero;
            }
        }
        public override void ModifyScreenPosition()
        {
            if (Player.whoAmI != Main.myPlayer) return;
            Vector2 normal = Main.screenPosition;

            if (State == PathState.Drawing || (State == PathState.Flying && cameraLocked))
            {
                Main.screenPosition = lockedCamera;
            }
            else if (cameraBlend < 1f)
            {
                float t = cameraBlend * cameraBlend * (3f - 2f * cameraBlend);
                Main.screenPosition = Vector2.Lerp(lockedCamera, normal, t);
            }
        }
        private void StartDrawing()
        {
            State = PathState.Drawing;
            Path.Clear();
            Path.Add(Player.Center);

            PathLength = 0f;
            PointPhase = Array.Empty<float>();
            lockedCamera = Main.screenPosition;
            cameraLocked = true;

            SoundEngine.PlaySound(SoundID.Item84 with { Pitch = 0.4f, Volume = 0.6f }, Player.Center);

            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.AqueousPathSync);
                packet.Write((byte)Player.whoAmI);
                packet.Write((byte)PathState.Drawing);
                packet.Write(Path.Count);

                foreach (var pos in Path) packet.WriteVector2(pos);
                packet.Send();
            }
        }
        private void UpdateDrawing()
        {
            Player.itemAnimation = 0;
            Player.itemTime = 0;
            Player.velocity *= 0.85f;

            Vector2 mouse = Main.MouseWorld;
            Vector2 last = Path[^1];

            float dist = Vector2.Distance(last, mouse);
            int oldCount = Path.Count;

            while (dist >= PointSpacing && PathLength < MaxPathLength)
            {
                Vector2 next = last + (mouse - last).SafeNormalize(Vector2.UnitX) * PointSpacing;
                Path.Add(next);
                PathLength += PointSpacing;

                last = next;
                dist = Vector2.Distance(last, mouse);
            }
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
            {
                if (Path.Count > oldCount) syncTimer++;

                if (syncTimer >= 3)
                {
                    syncTimer = 0;
                    ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                    packet.Write((byte)Jarkos.JarkosPacketType.AqueousPathSync);
                    packet.Write((byte)Player.whoAmI);
                    packet.Write((byte)PathState.Drawing);
                    packet.Write(Path.Count);

                    foreach (var pos in Path) packet.WriteVector2(pos);
                    packet.Send();
                }
            }

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Path[^1], DustID.Water, Main.rand.NextVector2Circular(1.5f, 1.5f), 100, default, 1.2f);
                d.noGravity = true;
            }

            if (PathLength >= MaxPathLength && Main.rand.NextBool(3))
                Dust.NewDustPerfect(Path[^1], DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.1f);
        }
        private void CancelDrawing()
        {
            State = PathState.None;
            Path.Clear();
            PathLength = 0f;
            cameraBlend = 0f;

            Cooldown = Math.Max(Cooldown, 30);

            if (Cooldown > 0)
                Player.AddBuff(ModContent.BuffType<AqueousCooldown>(), Cooldown);

            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.AqueousPathSync);
                packet.Write((byte)Player.whoAmI);
                packet.Write((byte)PathState.None);
                packet.Write(0);
                packet.Send();
            }
        }
        private void StartFlight()
        {
            State = PathState.Flying;
            FlightDistance = 0f;
            PhasedTicks = 0;

            CumLength = new float[Path.Count];
            for (int i = 1; i < Path.Count; i++)
                CumLength[i] = CumLength[i - 1] + Vector2.Distance(Path[i - 1], Path[i]);

            PathLength = CumLength[^1];
            PointPhase = new float[Path.Count];

            for (int i = 0; i < Path.Count; i++)
            {
                Vector2 tl = Path[i] - new Vector2(Player.width, Player.height) * 0.5f;
                PointPhase[i] = Collision.SolidCollision(tl, Player.width, Player.height) ? 1f : 0f;
            }

            for (int pass = 0; pass < 3; pass++)
            {
                float[] blurred = new float[Path.Count];
                for (int i = 0; i < Path.Count; i++)
                {
                    float sum = PointPhase[i] * 2f, cnt = 2f;
                    if (i > 0) { sum += PointPhase[i - 1]; cnt++; }
                    if (i < Path.Count - 1) { sum += PointPhase[i + 1]; cnt++; }
                    blurred[i] = sum / cnt;
                }
                PointPhase = blurred;
            }
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.AqueousPathSync);
                packet.Write((byte)Player.whoAmI);
                packet.Write((byte)PathState.Flying);
                packet.Write(Path.Count);

                foreach (var pos in Path)
                    packet.WriteVector2(pos);
                packet.Send();
            }

            Player.RemoveAllGrapplingHooks();

            if (Player.mount.Active) Player.mount.Dismount(Player);

            SoundEngine.PlaySound(SoundID.Item120 with { Pitch = 0.35f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Splash, Player.Center);

            for (int k = 0; k < 30; k++)
            {
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.DungeonWater,
                    Main.rand.NextVector2Circular(7f, 7f), 60, default, Main.rand.NextFloat(1.5f, 2.6f));
                d.noGravity = true;
            }

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Player.Center, Main.rand.NextVector2Unit(), 6f, 8f, 18, 1000f, "AqueousStart"));
        }
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte playerId = reader.ReadByte();
            byte stateByte = reader.ReadByte();

            PathState state = (PathState)stateByte;

            int count = reader.ReadInt32();
            var path = new List<Vector2>();

            for (int i = 0; i < count; i++)
                path.Add(reader.ReadVector2());

            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.AqueousPathSync);
                packet.Write(playerId);
                packet.Write(stateByte);
                packet.Write(count);

                foreach (var pos in path)
                    packet.WriteVector2(pos);
                packet.Send(-1, whoAmI);
            }

            if (Main.netMode != NetmodeID.Server && playerId != Main.myPlayer && playerId < Main.maxPlayers)
            {
                var modPlayer = Main.player[playerId].GetModPlayer<AqueousPathPlayer>();
                modPlayer.Path.Clear();
                modPlayer.Path.AddRange(path);

                if (state == PathState.Flying)
                {
                    modPlayer.StartFlightRemote();
                }
                else if (state == PathState.Drawing)
                {
                    modPlayer.State = PathState.Drawing;
                    modPlayer.PathLength = 0;
                    for (int i = 1; i < modPlayer.Path.Count; i++)
                        modPlayer.PathLength += Vector2.Distance(modPlayer.Path[i - 1], modPlayer.Path[i]);
                }
                else
                {
                    modPlayer.State = PathState.None;
                    modPlayer.DissolveTimer = 0;
                }
            }
        }
        private void StartFlightRemote()
        {
            State = PathState.Flying;
            FlightDistance = 0f;
            PhasedTicks = 0;

            CumLength = new float[Path.Count];

            for (int i = 1; i < Path.Count; i++)
                CumLength[i] = CumLength[i - 1] + Vector2.Distance(Path[i - 1], Path[i]);
            PathLength = CumLength[^1];
            PointPhase = new float[Path.Count];

            for (int i = 0; i < Path.Count; i++)
            {
                Vector2 tl = Path[i] - new Vector2(Player.width, Player.height) * 0.5f;
                PointPhase[i] = Collision.SolidCollision(tl, Player.width, Player.height) ? 1f : 0f;
            }

            for (int pass = 0; pass < 3; pass++)
            {
                float[] blurred = new float[Path.Count];
                for (int i = 0; i < Path.Count; i++)
                {
                    float sum = PointPhase[i] * 2f, cnt = 2f;
                    if (i > 0) { sum += PointPhase[i - 1]; cnt++; }
                    if (i < Path.Count - 1) { sum += PointPhase[i + 1]; cnt++; }
                    blurred[i] = sum / cnt;
                }
                PointPhase = blurred;
            }

            SoundEngine.PlaySound(SoundID.Item120 with { Pitch = 0.35f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Splash, Player.Center);

            for (int k = 0; k < 30; k++)
            {
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.DungeonWater,
                    Main.rand.NextVector2Circular(7f, 7f), 60, default, Main.rand.NextFloat(1.5f, 2.6f));
                d.noGravity = true;
            }
        }
        private void UpdateFlight()
        {
            FlightDistance += FlightSpeed;

            bool finished = FlightDistance >= PathLength;
            float d = Math.Min(FlightDistance, PathLength);

            Vector2 pos = SamplePath(d, out Vector2 dir, out float phase);

            flightPos = pos;
            Player.Center = pos;
            Player.velocity = Vector2.Zero;
            Player.fallStart = (int)(Player.position.Y / 16f);
            Player.immune = true;
            Player.immuneTime = 2;
            Player.immuneNoBlink = true;
            Player.invis = true;
            Player.noItems = true;
            Player.noBuilding = true;

            PhasingNow = phase > 0.35f || Collision.SolidCollision(Player.position, Player.width, Player.height);
            if (PhasingNow) PhasedTicks++;

            if (Player.whoAmI == Main.myPlayer)
            {
                DamageAlongPath(dir);
                if (cameraLocked && (FlightDistance > PathLength * 0.3f ||
                    Vector2.Distance(pos, lockedCamera + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f) > 900f))
                {
                    cameraLocked = false;
                    cameraBlend = 0f;
                }
            }

            SpawnFlightDust(pos, dir);
            if (finished) EndFlight(dir);
        }
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (State == PathState.Flying)
            {
                foreach (var layer in PlayerDrawLayerLoader.Layers)
                {
                    layer.Hide();
                }
            }
        }
        private Vector2 SamplePath(float dist, out Vector2 dir, out float phase)
        {
            int i = Array.BinarySearch(CumLength, dist);
            if (i < 0) i = ~i;

            i = Math.Clamp(i, 1, Path.Count - 1);

            float segLen = CumLength[i] - CumLength[i - 1];
            float t = segLen > 0f ? (dist - CumLength[i - 1]) / segLen : 0f;

            dir = (Path[i] - Path[i - 1]).SafeNormalize(Vector2.UnitX);
            phase = MathHelper.Lerp(PointPhase[i - 1], PointPhase[i], t);

            return Vector2.Lerp(Path[i - 1], Path[i], t);
        }
        private void DamageAlongPath(Vector2 dir)
        {
            Item heldItem = Player.HeldItem;
            int weaponDamage = 180;
            int critChance = 12;

            if (heldItem != null && heldItem.active && heldItem.ModItem is AqueousFlow)
            {
                weaponDamage = Player.GetWeaponDamage(heldItem);
                critChance = Player.GetWeaponCrit(heldItem);
            }

            int dmg = (int)(weaponDamage * 2.5f);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npcHitCd[i] > 0)
                    continue;

                if (Vector2.Distance(npc.Center, Player.Center) > HitRadius + npc.width * 0.5f)
                    continue;

                bool crit = Main.rand.Next(100) < critChance;
                Player.ApplyDamageToNPC(npc, dmg, 12f, dir.X >= 0f ? 1 : -1, crit, DamageClass.Melee, true);

                npcHitCd[i] = 20;
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(npc.Center, dir.RotatedBy(MathHelper.PiOver2), 10f, 10f, 14, 1200f, "AqueousHit"));

                SoundEngine.PlaySound(SoundID.Item89 with { Volume = 0.8f, Pitch = -0.2f }, npc.Center);
                for (int k = 0; k < 18; k++)
                {
                    Dust dd = Dust.NewDustPerfect(npc.Center, DustID.DungeonWater,
                        dir.RotatedByRandom(1.2f) * Main.rand.NextFloat(2f, 9f),
                        60, default, Main.rand.NextFloat(1.2f, 2.2f));
                    dd.noGravity = true;
                }
            }
        }
        private void SpawnFlightDust(Vector2 pos, Vector2 dir)
        {
            for (int k = 0; k < 4; k++)
            {
                int type = PhasingNow ? DustID.Shadowflame : DustID.DungeonWater;
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(26f, 26f), type,
                    -dir * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    60, default, Main.rand.NextFloat(1.3f, 2.4f));
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust g = Dust.NewDustPerfect(pos, DustID.TintableDustLighted, -dir * 2f, 120,
                    PhasingNow ? new Color(190, 120, 255) : new Color(120, 220, 255), 1.6f);
                g.noGravity = true;
            }
            Lighting.AddLight(pos, PhasingNow ? new Vector3(0.5f, 0.3f, 0.9f) : new Vector3(0.2f, 0.6f, 1f));
        }
        private void EndFlight(Vector2 dir)
        {
            State = PathState.None;
            PhasingNow = false;
            DissolveTimer = DissolveTime;

            Player.velocity = dir * ExitPush;
            Player.fallStart = (int)(Player.position.Y / 16f);
            Player.immune = true;
            Player.immuneTime = 40;

            if (Player.whoAmI == Main.myPlayer)
            {
                int safety = 0;
                while (Collision.SolidCollision(Player.position, Player.width, Player.height) && safety++ < 200)
                    Player.position -= dir * 4f;

                if (Collision.SolidCollision(Player.position, Player.width, Player.height))
                {
                    for (int i = Path.Count - 1; i >= 0; i--)
                        if (PointPhase[i] < 0.5f) { Player.Center = Path[i]; break; }
                }

                float lenFactor = PathLength / MaxPathLength;
                int cd = 45 + (int)(lenFactor * 255f);

                cd += (int)(PhasedTicks * 2.5f);
                Cooldown = Math.Min(cd, 60 * 12);

                Player.AddBuff(ModContent.BuffType<AqueousCooldown>(), Cooldown);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Player.Center, dir, 12f, 9f, 22, 1400f, "AqueousExit"));
            }

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.1f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Splash, Player.Center);

            for (int k = 0; k < 46; k++)
            {
                Vector2 vel = (MathHelper.TwoPi * k / 46f).ToRotationVector2() * Main.rand.NextFloat(5f, 11f);
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.DungeonWater,
                    vel + dir * 4f, 40, default, Main.rand.NextFloat(1.6f, 2.8f));
                d.noGravity = true;
            }

            if (PhasedTicks > 0)
            {
                for (int k = 0; k < 24; k++)
                {
                    Dust d = Dust.NewDustPerfect(Player.Center, DustID.Shadowflame,
                        Main.rand.NextVector2Circular(8f, 8f), 60, default, Main.rand.NextFloat(1.4f, 2.2f));
                    d.noGravity = true;
                }
            }
        }
    }
}
