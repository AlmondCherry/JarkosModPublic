using System;
using System.Collections.Generic;
using jarkos.items.accs.shields;
using Jarkos.items.materials;
using Jarkos.projectiles.bullet;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.accs.amulets
{
	public class PhaseBloodShield : ModItem {
		public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            PhaseBloodPlayer phasePlayer = player.GetModPlayer<PhaseBloodPlayer>();
            phasePlayer.shieldEquipped = true;
            phasePlayer.shieldVisible = !hideVisual;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WoodenShield>(), 1)
                .AddIngredient(ModContent.ItemType<TrombosisStone>(), 10)
                .AddIngredient(ItemID.SoulofNight, 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
	public class PhaseBloodPlayer : ModPlayer
    {
        public bool shieldEquipped;
        public bool shieldVisible;
        public bool isDashing;
        private int dashTimer;
        private const int DASH_DURATION = 16;
        private const float DASH_SPEED = 38f;
        private Vector2 dashDirection;
        private Vector2 dashStartPos;
        private int dashCooldown;
        private const int DASH_COOLDOWN_DURATION = 60;
        private int markerSpawnTimer;
        private const int MARKER_SPAWN_INTERVAL = 3;
        private List<int> activeMarkers = new List<int>();
        public bool phaseImmune;
        public float shieldGlow;
        private List<GhostTrail> ghostTrails = new List<GhostTrail>();
        public static ModKeybind DashKeybind { get; set; }
        private struct GhostTrail
        {
            public Vector2 position;
            public float alpha;
        }
        public override void ResetEffects()
        {
            shieldEquipped = false;
            shieldVisible = false;
            if (!isDashing)
                phaseImmune = false;
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (!shieldEquipped || isDashing || dashCooldown > 0)
                return;
            if (DashKeybind != null && DashKeybind.JustPressed)
            {
                TryStartDash();
                return;
            }
            if (triggersSet.Down && Player.controlLeft || triggersSet.Down && Player.controlRight || triggersSet.Down && Player.controlUp || triggersSet.Down && Player.controlDown)
            {
            }
        }
        public override void PreUpdate()
        {
            if (!shieldEquipped)
            {
                if (isDashing)
                    EndDash();
                return;
            }
            if (isDashing)
            {
                Player.statDefense += 25;
            }
            else
            {
                Player.statDefense += 5;
            }
            if (dashCooldown > 0)
                dashCooldown--;
            HandleDashInput();
            if (isDashing)
                UpdateDash();
            shieldGlow = isDashing
                ? MathHelper.Lerp(shieldGlow, 1f, 0.2f)
                : MathHelper.Lerp(shieldGlow, 0f, 0.05f);
            UpdateGhostTrails();
        }
        private int leftPressCount;
        private int rightPressCount;
        private int upPressCount;
        private int downPressCount;
        private int leftReleaseTimer;
        private int rightReleaseTimer;
        private int upReleaseTimer;
        private int downReleaseTimer;
        private bool prevLeft;
        private bool prevRight;
        private bool prevUp;
        private bool prevDown;
        private const int TAP_WINDOW = 20;
        private void HandleDashInput()
        {
            if (isDashing || dashCooldown > 0 || !shieldEquipped)
                return;
            bool curLeft = Player.controlLeft;
            bool curRight = Player.controlRight;
            bool curUp = Player.controlUp;
            bool curDown = Player.controlDown;
            if (curLeft && !prevLeft)
            {
                if (leftReleaseTimer > 0 && leftPressCount >= 1)
                {
                    StartDash(-1, 0);
                    ResetAllTapTrackers();
                    UpdatePrev(curLeft, curRight, curUp, curDown);
                    return;
                }
                else
                {
                    leftPressCount = 1;
                    leftReleaseTimer = 0;
                }
            }
            if (!curLeft && prevLeft)
            {
                if (leftPressCount >= 1)
                    leftReleaseTimer = TAP_WINDOW;
            }
            if (leftReleaseTimer > 0)
            {
                leftReleaseTimer--;
                if (leftReleaseTimer <= 0)
                    leftPressCount = 0;
            }
            if (curRight && !prevRight)
            {
                if (rightReleaseTimer > 0 && rightPressCount >= 1)
                {
                    StartDash(1, 0);
                    ResetAllTapTrackers();
                    UpdatePrev(curLeft, curRight, curUp, curDown);
                    return;
                }
                else
                {
                    rightPressCount = 1;
                    rightReleaseTimer = 0;
                }
            }
            if (!curRight && prevRight)
            {
                if (rightPressCount >= 1)
                    rightReleaseTimer = TAP_WINDOW;
            }
            if (rightReleaseTimer > 0)
            {
                rightReleaseTimer--;
                if (rightReleaseTimer <= 0)
                    rightPressCount = 0;
            }
            if (curUp && !prevUp)
            {
                if (upReleaseTimer > 0 && upPressCount >= 1)
                {
                    StartDash(0, -1);
                    ResetAllTapTrackers();
                    UpdatePrev(curLeft, curRight, curUp, curDown);
                    return;
                }
                else
                {
                    upPressCount = 1;
                    upReleaseTimer = 0;
                }
            }
            if (!curUp && prevUp)
            {
                if (upPressCount >= 1)
                    upReleaseTimer = TAP_WINDOW;
            }
            if (upReleaseTimer > 0)
            {
                upReleaseTimer--;
                if (upReleaseTimer <= 0)
                    upPressCount = 0;
            }
            if (curDown && !prevDown)
            {
                if (downReleaseTimer > 0 && downPressCount >= 1)
                {
                    StartDash(0, 1);
                    ResetAllTapTrackers();
                    UpdatePrev(curLeft, curRight, curUp, curDown);
                    return;
                }
                else
                {
                    downPressCount = 1;
                    downReleaseTimer = 0;
                }
            }
            if (!curDown && prevDown)
            {
                if (downPressCount >= 1)
                    downReleaseTimer = TAP_WINDOW;
            }
            if (downReleaseTimer > 0)
            {
                downReleaseTimer--;
                if (downReleaseTimer <= 0)
                    downPressCount = 0;
            }
            UpdatePrev(curLeft, curRight, curUp, curDown);
        }
        private void UpdatePrev(bool l, bool r, bool u, bool d)
        {
            prevLeft = l;
            prevRight = r;
            prevUp = u;
            prevDown = d;
        }
        private void ResetAllTapTrackers()
        {
            leftPressCount = 0;
            rightPressCount = 0;
            upPressCount = 0;
            downPressCount = 0;
            leftReleaseTimer = 0;
            rightReleaseTimer = 0;
            upReleaseTimer = 0;
            downReleaseTimer = 0;
        }
        private void TryStartDash()
        {
            int dirX = 0;
            int dirY = 0;
            if (Player.controlLeft) dirX -= 1;
            if (Player.controlRight) dirX += 1;
            if (Player.controlUp) dirY -= 1;
            if (Player.controlDown) dirY += 1;
            if (dirX == 0 && dirY == 0)
                dirX = Player.direction;
            StartDash(dirX, dirY);
        }
        private void StartDash(int dirX, int dirY)
        {
            isDashing = true;
            dashTimer = 0;
            markerSpawnTimer = 0;
            dashStartPos = Player.Center;
            activeMarkers.Clear();
            dashDirection = new Vector2(dirX, dirY);
            if (dashDirection != Vector2.Zero)
                dashDirection = Vector2.Normalize(dashDirection);
            else
                dashDirection = new Vector2(Player.direction, 0);
            float healthRatio = (float)Player.statLife / Player.statLifeMax2;
            phaseImmune = healthRatio < 0.25f;
            SoundStyle dashSound = phaseImmune ? SoundID.Item119 with { Volume = 0.8f, Pitch = -0.3f } : SoundID.Item73 with { Volume = 0.7f, Pitch = 0.2f };
            SoundEngine.PlaySound(dashSound, Player.Center);
            SpawnDashStartEffect();
        }
        private void UpdateDash()
        {
            dashTimer++;
            float progress = (float)dashTimer / DASH_DURATION;
            float speedMult = 1f;
            if (progress > 0.7f)
            {
                float decel = (progress - 0.7f) / 0.3f;
                speedMult = 1f - decel * decel * 0.6f;
            }
            Player.velocity = dashDirection * DASH_SPEED * speedMult;
            Player.gravity = 0f;
            Player.fallStart = (int)(Player.position.Y / 16f);
            Player.noKnockback = true;
            if (phaseImmune)
            {
                Player.immune = true;
                Player.immuneTime = 5;
                Player.immuneNoBlink = true;
            }
            if (dashDirection.X != 0)
                Player.direction = dashDirection.X > 0 ? 1 : -1;
            markerSpawnTimer++;
            if (markerSpawnTimer >= MARKER_SPAWN_INTERVAL)
            {
                markerSpawnTimer = 0;
                SpawnMarker();
            }
            SpawnGhostTrail();
            SpawnDashTrailParticles();
            if (dashTimer >= DASH_DURATION)
                EndDash();
        }
        private void EndDash()
        {
            if (!isDashing)
                return;
            isDashing = false;
            phaseImmune = false;
            Player.velocity *= 0.3f;
            dashCooldown = DASH_COOLDOWN_DURATION;
            DetonateAllMarkers();
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.6f, Pitch = 0.3f }, Player.Center);
            SpawnDashEndEffect();
        }
        private void SpawnMarker()
        {
            if (Main.myPlayer != Player.whoAmI)
                return;
            int baseDamage = 60 + (int)(Player.GetDamage(DamageClass.Generic).Additive * 40);
            float healthRatio = (float)Player.statLife / Player.statLifeMax2;
            if (healthRatio < 0.25f)
                baseDamage = (int)(baseDamage * 1.5f);
            Vector2 offset = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f));
            int projIdx = Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[0]), Player.Center + offset, Vector2.Zero, ModContent.ProjectileType<PhaseBloodMarker>(), baseDamage, 4f, Player.whoAmI);
            if (projIdx >= 0 && projIdx < Main.maxProjectiles)
                activeMarkers.Add(projIdx);
        }
        private void DetonateAllMarkers()
        {
            int delay = 0;
            foreach (int idx in activeMarkers)
            {
                if (idx >= 0 && idx < Main.maxProjectiles)
                {
                    Projectile proj = Main.projectile[idx];
                    if (proj.active && proj.type == ModContent.ProjectileType<PhaseBloodMarker>())
                    {
                        if (proj.ModProjectile is PhaseBloodMarker marker)
                        {
                            marker.SetDetonationDelay(delay);
                            delay += 3;
                        }
                    }
                }
            }
            activeMarkers.Clear();
        }
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (isDashing && phaseImmune)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust d = Dust.NewDustDirect(Player.Center, 4, 4, DustID.SpectreStaff);
                    d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.color = new Color(180, 80, 255, 100);
                }
                return true;
            }
            return false;
        }
        public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
        {
            if (isDashing && phaseImmune)
                return true;
            return false;
        }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (isDashing)
            {
                if (phaseImmune)
                {
                    r = 0.7f;
                    g = 0.3f;
                    b = 1f;
                    a *= 0.5f;
                    fullBright = true;
                }
                else
                {
                    r = 1f;
                    g = 0.4f;
                    b = 0.4f;
                    fullBright = true;
                }
            }
            else if (shieldGlow > 0.05f)
            {
                float glow = shieldGlow * 0.3f;
                r = MathHelper.Lerp(r, 1f, glow);
                g = MathHelper.Lerp(g, 0.3f, glow);
                b = MathHelper.Lerp(b, 0.3f, glow);
            }
        }
        private void SpawnDashStartEffect()
        {
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi / 30f * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                int dustType = phaseImmune ? DustID.SpectreStaff : DustID.Blood;
                Dust d = Dust.NewDustDirect(Player.Center + vel * 10f, 4, 4, dustType);
                d.velocity = vel * 6f;
                d.noGravity = true;
                d.scale = phaseImmune ? 2f : 1.5f;
                d.color = phaseImmune ? new Color(180, 50, 255) : new Color(255, 50, 50);
                d.fadeIn = 1.5f;
            }
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                Dust ring = Dust.NewDustDirect(Player.Center, 2, 2, DustID.GoldFlame);
                ring.velocity = vel;
                ring.noGravity = true;
                ring.scale = 1.2f;
                ring.color = phaseImmune ? new Color(200, 100, 255) : new Color(255, 150, 50);
            }
        }
        private void SpawnDashEndEffect()
        {
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustDirect(Player.Center, 6, 6, DustID.Blood);
                d.velocity = vel;
                d.noGravity = true;
                d.scale = 1.8f;
                d.color = new Color(255, 80, 80);
            }
        }
        private void SpawnDashTrailParticles()
        {
            int count = phaseImmune ? 6 : 4;
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Player.width * 0.5f, Player.height * 0.5f);
                Vector2 vel = -dashDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f);
                int dustType = phaseImmune ? DustID.SpectreStaff : DustID.Blood;
                Dust d = Dust.NewDustDirect(Player.Center + offset, 4, 4, dustType);
                d.velocity = vel;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1f, 2f);
                d.fadeIn = 1.5f;
                d.color = phaseImmune ? Color.Lerp(new Color(150, 50, 255), new Color(255, 80, 180), Main.rand.NextFloat()) : Color.Lerp(new Color(200, 30, 30), new Color(255, 100, 50), Main.rand.NextFloat());
            }
            if (dashTimer % 2 == 0)
            {
                Vector2 perp = new Vector2(-dashDirection.Y, dashDirection.X);
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 sparkPos = Player.Center + perp * side * 15f;
                    Dust Spark = Dust.NewDustDirect(sparkPos, 2, 2, DustID.GoldFlame);
                    Spark.velocity = perp * side * 2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                    Spark.noGravity = true;
                    Spark.scale = 0.8f;
                    Spark.color = phaseImmune ? new Color(200, 150, 255) : new Color(255, 200, 100);
                }
            }
        }
        private void SpawnGhostTrail()
        {
            ghostTrails.Add(new GhostTrail            {
                position = Player.Center, alpha = 0.8f
            });
        }
        private void UpdateGhostTrails()
        {
            for (int i = ghostTrails.Count - 1; i >= 0; i--)
            {
                var trail = ghostTrails[i];
                trail.alpha -= 0.05f;
                ghostTrails[i] = trail;
                if (trail.alpha <= 0f)
                    ghostTrails.RemoveAt(i);
                else if (trail.alpha > 0.2f)
                {
                    if (Main.rand.NextBool(3))
                    {
                        int dustType = phaseImmune ? DustID.SpectreStaff : DustID.Blood;
                        Dust d = Dust.NewDustDirect(trail.position + Main.rand.NextVector2Circular(10f, 18f), 2, 2, dustType);
                        d.velocity = Vector2.Zero;
                        d.noGravity = true;
                        d.scale = trail.alpha * 1.5f;
                    }
                }
            }
        }
    }
    public class KeybindSystemPhaseBlood : ModSystem
    {
        public static ModKeybind PhaseBloodDashKey { get; private set; }
        public override void Load()
        {
            PhaseBloodDashKey = KeybindLoader.RegisterKeybind(Mod, "Phase Blood Dash", "LeftShift");
            PhaseBloodPlayer.DashKeybind = PhaseBloodDashKey;
        }
        public override void Unload()
        {
            PhaseBloodDashKey = null;
            PhaseBloodPlayer.DashKeybind = null;
        }
    }
}
