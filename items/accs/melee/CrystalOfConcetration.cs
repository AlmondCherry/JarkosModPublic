using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.items.materials;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using System.Collections.Generic;
using jarkos.systems;
using System.Linq;
using Terraria.GameContent;
using Terraria.Localization;

namespace jarkos.items.accs.melee
{
	public class CrystalOfConcetration : ModItem
    {
		public override void SetDefaults()
		{
			Item.width = 52;
			Item.height = 70;
			Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FacetedCrystalPlayer>().hasCrystalAccessory = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipLine = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");

            if (tooltipLine != null)
            {
                var bindkey = CrystalKeybindsSystem.CrystalKey.GetAssignedKeys();
                string firstbindkey = bindkey.Count > 0 ? bindkey[0] : "None";
                tooltipLine.Text = Language.GetTextValue("Mods.jarkos.Items.CrystalOfConcetration.Tooltip", firstbindkey);
            }
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.CrystalShard, 50)
			.AddIngredient(ItemID.PurificationPowder, 15)
			.AddIngredient(ItemID.SoulofLight, 10)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class CrystalKeybindsSystem : ModSystem
    {
        public static ModKeybind CrystalKey { get; private set; }
        public override void Load()
        {
            CrystalKey = KeybindLoader.RegisterKeybind(Mod, "SummonCrystal", "G");
        }
        public override void Unload()
        {
            CrystalKey = null;
        }
    }
	public class FacetedCrystalPlayer : ModPlayer
    {
        public bool hasCrystalAccessory = false;
        public int crystalCooldown = 0;
        public const int CooldownTime = 300;
        public override void ResetEffects()
        {
            hasCrystalAccessory = false;
        }
        public override void PostUpdate()
        {
            if (crystalCooldown > 0)
                crystalCooldown--;

            if (hasCrystalAccessory && crystalCooldown <= 0)
            {
                if (CrystalKeybindsSystem.CrystalKey.JustPressed)
                {
                    SummonCrystal();
                }
            }
        }
        private void SummonCrystal()
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<FacetedCrystalProjectile>())
                {
                    return;
                }
            }

            Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[0]), Player.Center - new Microsoft.Xna.Framework.Vector2(0, 60), Microsoft.Xna.Framework.Vector2.Zero, ModContent.ProjectileType<FacetedCrystalProjectile>(), 0, 0f, Player.whoAmI);
            crystalCooldown = CooldownTime;
        }
    }
	#region Projectile
	public class FacetedCrystalProjectile : ModProjectile
    {
		public override string Texture => "jarkos/textures/AccProjectile/crystal_of_concetration_proj";
        private const int MaxHits = 30;
        private int currentHits = 0;
        private float rotationSpeed = 0f;
        private float pulseTimer = 0f;
        private float crystalScale = 0f;
        private bool isSpawning = true;
        private HashSet<int> reflectedProjectiles = new HashSet<int>();
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            if (isSpawning)
            {
                crystalScale += 0.05f;
                Projectile.alpha -= 15;

                if (crystalScale >= 1f)
                {
                    crystalScale = 1f;
                    isSpawning = false;
                }

                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
                SpawnAppearanceParticles();
            }

            rotationSpeed = MathHelper.Lerp(rotationSpeed, 0.03f, 0.02f);
            Projectile.rotation += rotationSpeed;
            pulseTimer += 0.05f;

            Projectile.position.Y += (float)Math.Sin(pulseTimer) * 0.3f;
            Lighting.AddLight(Projectile.Center, 0.4f, 0.6f, 1f);
            
            SpawnAmbientParticles();
            ReflectPlayerProjectiles();
            CleanupReflectedList();

            if (currentHits >= MaxHits)
            {
                ShatterCrystal();
                Projectile.Kill();
            }
        }
        private void SpawnAppearanceParticles()
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 4f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + velocity * 10f, DustID.BlueCrystalShard, -velocity * 0.5f, 0, new Color(150, 200, 255), Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        private void SpawnAmbientParticles()
        {
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float distance = Main.rand.NextFloat(20f, 40f);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.MagicMirror, -offset * 0.02f, 100, new Color(100, 180, 255), Main.rand.NextFloat(0.5f, 0.8f));
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(5))
            {
                float orbitAngle = pulseTimer * 2f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 orbitPos = Projectile.Center + new Vector2((float)Math.Cos(orbitAngle) * 30f, (float)Math.Sin(orbitAngle) * 15f);
                Dust orbit = Dust.NewDustPerfect(orbitPos, DustID.GemDiamond, Vector2.Zero, 0, Color.White, 0.6f);
                orbit.noGravity = true;
                orbit.noLight = false;
            }
        }
        private void ReflectPlayerProjectiles()
        {
            float reflectRadius = 60f;
            foreach (Projectile proj in Main.projectile)
            {
                if (!proj.active || proj.whoAmI == Projectile.whoAmI)
                    continue;
                if (proj.owner != Projectile.owner)
                    continue;
                if (!proj.friendly)
                    continue;
                if (reflectedProjectiles.Contains(proj.whoAmI))
                    continue;
                if (proj.minion || proj.sentry || proj.bobber)
                    continue;
                if (proj.type == Projectile.type)
                    continue;
                float distance = Vector2.Distance(proj.Center, Projectile.Center);

                if (distance < reflectRadius)
                {
                    NPC target = FindNearestEnemy(1000f);
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                        float speed = Math.Max(proj.velocity.Length(), 10f);
                        proj.velocity = direction * speed * 1.3f;
                        proj.Center = Projectile.Center + direction * (reflectRadius + 5f);

                        proj.damage = (int)(proj.damage * 0.7f);
                        if (proj.timeLeft < 120)
                            proj.timeLeft = 120;
                            
                        reflectedProjectiles.Add(proj.whoAmI);
                        SpawnReflectionEffects(Projectile.Center, target.Center);
                        currentHits++;

                        float pitch = 0.5f + currentHits / (float)MaxHits * 0.5f;
                        SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.6f, Pitch = pitch }, Projectile.Center);
                        rotationSpeed += 0.02f;

                        SpawnCrackEffect();
                    }
                    else
                    {
                        Vector2 randomDir = Main.rand.NextVector2CircularEdge(1f, 1f);
                        float speed = Math.Max(proj.velocity.Length(), 10f);
                        proj.velocity = randomDir * speed;
                        proj.Center = Projectile.Center + randomDir * (reflectRadius + 5f);

                        proj.damage = (int)(proj.damage * 0.7f);
                        reflectedProjectiles.Add(proj.whoAmI);

                        SpawnReflectionEffectsNoTarget(Projectile.Center);
                        currentHits++;
                        rotationSpeed += 0.01f;
                        SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);

                        SpawnCrackEffect();
                    }
                }
            }
        }
        private void CleanupReflectedList()
        {
            reflectedProjectiles.RemoveWhere(id => id < 0 || id >= Main.maxProjectiles || !Main.projectile[id].active);
        }
        private NPC FindNearestEnemy(float maxDistance)
        {
            NPC nearest = null;
            float nearestDist = maxDistance;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.CountsAsACritter)
                    continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = npc;
                }
            }
            return nearest;
        }
        private void SpawnReflectionEffects(Vector2 crystalPos, Vector2 targetPos)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustPerfect(crystalPos, DustID.BlueTorch, velocity, 0, new Color(200, 230, 255), Main.rand.NextFloat(1.2f, 1.8f));
                dust.noGravity = true;
            }

            Vector2 toTarget = targetPos - crystalPos;
            Vector2 direction = toTarget.SafeNormalize(Vector2.UnitX);
            int steps = 15;

            for (int i = 0; i < steps; i++)
            {
                Vector2 pos = crystalPos + direction * (i * 8f);
                Dust line = Dust.NewDustPerfect(pos, DustID.MagicMirror, direction * 2f, 100, new Color(150, 200, 255), 1f - (i / (float)steps) * 0.5f);
                line.noGravity = true;
                line.fadeIn = 0.3f;
            }
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 ringVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                Dust ring = Dust.NewDustPerfect(crystalPos, DustID.GemDiamond, ringVel, 0, Color.White, 1f);
                ring.noGravity = true;
            }
        }
        private void SpawnReflectionEffectsNoTarget(Vector2 crystalPos)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(crystalPos, DustID.BlueTorch, velocity, 0, new Color(150, 180, 220), Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }
        private void SpawnCrackEffect()
        {
            float damagePercent = currentHits / (float)MaxHits;
            int particleCount = (int)(5 + damagePercent * 10);

            for (int i = 0; i < particleCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Color color = Color.Lerp(new Color(150, 200, 255), new Color(255, 100, 100), damagePercent);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.GemDiamond, velocity, 0, color, Main.rand.NextFloat(0.5f, 1f));
                dust.noGravity = true;
            }
        }
        private void ShatterCrystal()
        {
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 1f, Pitch = -0.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f;
                float speed = Main.rand.NextFloat(4f, 12f);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, velocity, 0, new Color(150, 200, 255), Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            for (int i = 0; i < 30; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                velocity.Y -= Main.rand.NextFloat(2f, 5f);
                Dust shard = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), DustID.GemDiamond, velocity, 0, Color.White, Main.rand.NextFloat(1f, 2f));
                shard.noGravity = false;
            }
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                Dust ring = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, velocity, 100, new Color(200, 230, 255), 1.5f);
                ring.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() / 2f;

            float damagePercent = currentHits / (float)MaxHits;
            float pulse = 1f + (float)Math.Sin(pulseTimer * 2f) * 0.05f;
            float scale = crystalScale * pulse;

            Color crystalColor = Color.Lerp(new Color(150, 200, 255), new Color(255, 100, 100), damagePercent) * ((255 - Projectile.alpha) / 255f);
            for (int i = 0; i < 6; i++)
            {
                float glowScale = scale * (1.2f + i * 0.15f);
                Color glowColor = crystalColor * (0.15f - i * 0.02f);
                spriteBatch.Draw(texture, drawPos, frame, glowColor, Projectile.rotation + i * 0.1f, origin, glowScale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, drawPos, frame, crystalColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            Color highlightColor = Color.White * ((255 - Projectile.alpha) / 255f) * 0.5f;

            spriteBatch.Draw(texture, drawPos, frame, highlightColor, -Projectile.rotation * 0.5f, origin, scale * 0.8f, SpriteEffects.None, 0f);
            DrawHealthRing(spriteBatch, drawPos, damagePercent);

            return false;
        }
        private void DrawHealthRing(SpriteBatch spriteBatch, Vector2 center, float damagePercent)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int segments = 24;
            float radius = 35f;
            float remainingPercent = 1f - damagePercent;

            for (int i = 0; i < (int)(segments * remainingPercent); i++)
            {
                float angle = MathHelper.TwoPi * i / segments - MathHelper.PiOver2;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Color ringColor = Color.Lerp(Color.Cyan, Color.Red, damagePercent) * 0.7f;
                
                spriteBatch.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), ringColor, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (currentHits < MaxHits)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.MagicMirror, Main.rand.NextVector2Circular(2f, 2f), 100, new Color(150, 200, 255), 1f);
                    dust.noGravity = true;
                }
            }
        }
    }
	#endregion
}
