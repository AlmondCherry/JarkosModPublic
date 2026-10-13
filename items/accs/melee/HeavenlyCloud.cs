using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.melee
{
	public class HeavenlyCloud : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightPurple;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SacredCloudPlayer>().HasSacredCloud = true;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.CloudinaBottle, 1)
			.AddIngredient(ItemID.SpectreBar, 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
	public class SacredCloudPlayer : ModPlayer
    {
        public const int CooldownTicks = 60 * 60;
        public const float CloudGravityMultiplier = 0.18f;
        public const float MaxFallSpeedOnCloud = 2.2f;
        public const float BoostUpwardVelocity = -14f;
        public const int ActivationGraceFrames = 6;
        public bool HasSacredCloud;
        public int CooldownLeft;
        public bool CloudActive;
        private int _activationGrace;
        private int _activeTicks;
        private int _postBoostRefillFrames;
        public override void ResetEffects()
        {
            HasSacredCloud = false;
        }
        public override void PreUpdateMovement()
        {
            if (CooldownLeft > 0)
            {
                CooldownLeft--;
            }
            if (!HasSacredCloud)
            {
                if (CloudActive)
                {
                    KillCloudProjectile();
                    CloudActive = false;
                }
                _activationGrace = 0;
                return;
            }
            if (!Player.active || Player.dead)
            {
                if (CloudActive)
                {
                    KillCloudProjectile();
                    CloudActive = false;
                }
                _activationGrace = 0;
                return;
            }
            if (CloudActive)
            {
                UpdateCloudActive();
            }
            else
            {
                CheckActivationConditions();
            }
            if (_postBoostRefillFrames > 0)
            {
                _postBoostRefillFrames--;
                if (!Player.controlJump)
                {
                    RestoreFlightResources();
                }
            }
        }
        private void RestoreFlightResources()
        {
            if (Player.wingTimeMax > 0)
            {
                Player.wingTime = Player.wingTimeMax;
            }
            if (Player.rocketBoots > 0)
            {
                Player.rocketTime = Player.rocketTimeMax;
            }
        }
        private void CheckActivationConditions()
        {
            bool wingsExhausted = Player.wingTimeMax > 0 && Player.wingTime <= 0f;
            bool airborne = Player.velocity.Y != 0f;
            bool descendingOrFlat = Player.velocity.Y >= -0.5f;
            bool conditionsMet = CooldownLeft <= 0
                && wingsExhausted
                && Player.controlJump
                && airborne
                && descendingOrFlat;

            if (!conditionsMet)
            {
                _activationGrace = 0;
                return;
            }
            _activationGrace++;
            if (_activationGrace < ActivationGraceFrames)
            {
                return;
            }
            ActivateCloud();
        }
        private void ActivateCloud()
        {
            CloudActive = true;
            _activationGrace = 0;
            _activeTicks = 0;

            if (Player.whoAmI == Main.myPlayer)
            {
                int type = ModContent.ProjectileType<SacredCloudProjectile>();
                if (Player.ownedProjectileCounts[type] == 0)
                {
                    Projectile.NewProjectile(Player.GetSource_FromThis("SacredCloud"), Player.Bottom + new Vector2(0f, 6f), Vector2.Zero, type, 0, 0f, Player.whoAmI);
                }
            }
            SoundEngine.PlaySound(SoundID.Item117 with { Volume = 0.55f, Pitch = 0.10f }, Player.Center);
            for (int i = 0; i < 20; i++)
            {
                Vector2 spawn = Player.Bottom + new Vector2(Main.rand.NextFloat(-22f, 22f), Main.rand.NextFloat(-4f, 14f));
                Dust dust = Dust.NewDustPerfect(spawn, DustID.Cloud, Main.rand.NextVector2Circular(2.4f, 1.3f), 80, default, 1.3f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Player.Bottom + Main.rand.NextVector2Circular(18f, 8f), DustID.GoldFlame, Main.rand.NextVector2Circular(1.6f, 1.2f), 100, default, 1.05f);
                dust.noGravity = true;
            }
        }
        private void UpdateCloudActive()
        {
            _activeTicks++;
            if (_activeTicks > 4 && Player.velocity.Y == 0f)
            {
                KillCloudProjectile();
                CloudActive = false;
                for (int i = 0; i < 8; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Player.Bottom + Main.rand.NextVector2Circular(20f, 4f), DustID.Cloud, Main.rand.NextVector2Circular(1.6f, 0.6f), 100, default, 1.1f);
                    dust.noGravity = true;
                }
                return;
            }
            if (Player.velocity.Y > 0f)
            {
                Player.velocity.Y *= CloudGravityMultiplier;
            }
            if (Player.velocity.Y > MaxFallSpeedOnCloud)
            {
                Player.velocity.Y = MaxFallSpeedOnCloud;
            }
            Player.wingTime = 0f;
            if (!Player.controlJump)
            {
                ApplyBoost();
            }
        }
        private void ApplyBoost()
        {
            Player.velocity.Y = BoostUpwardVelocity;
            RestoreFlightResources();

            _postBoostRefillFrames = 6;
            CooldownLeft = CooldownTicks;

            CloudActive = false;
            _activeTicks = 0;

            KillCloudProjectile();

            SoundEngine.PlaySound(SoundID.Item68 with { Volume = 0.6f, Pitch = 0.20f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item117 with { Volume = 0.45f, Pitch = -0.10f }, Player.Center);

            for (int i = 0; i < 26; i++)
            {
                Vector2 spawn = Player.Bottom + Main.rand.NextVector2Circular(28f, 8f);
                Dust dust = Dust.NewDustPerfect(spawn, DustID.Cloud, new Vector2(Main.rand.NextFloat(-3.4f, 3.4f), Main.rand.NextFloat(0.5f, 3.5f)), 80, default, 1.4f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 16; i++)
            {
                Dust dust = Dust.NewDustPerfect(Player.Bottom + Main.rand.NextVector2Circular(22f, 6f), DustID.GoldFlame, Main.rand.NextVector2Circular(2.4f, 1.6f), 100, default, 1.2f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Player.Bottom + Main.rand.NextVector2Circular(18f, 6f), DustID.YellowStarDust, Main.rand.NextVector2Circular(3f, 2f), 120, default, 1.3f);
                dust.noGravity = true;
            }
        }
        private void KillCloudProjectile()
        {
            int type = ModContent.ProjectileType<SacredCloudProjectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == type && p.owner == Player.whoAmI)
                {
                    p.Kill();
                }
            }
        }
    }
	public class SacredCloudProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;
        public const int FootOffset = 6;
        public const int LifetimeCap = 60 * 30;
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 36;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LifetimeCap;
            Projectile.netImportant = true;
        }
        public override bool? CanDamage() => false;
        public override void AI()
        {
            if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            {
                Projectile.Kill();
                return;
            }
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                var mp = owner.GetModPlayer<SacredCloudPlayer>();
                if (!mp.CloudActive)
                {
                    Projectile.Kill();
                    return;
                }
            }
            Vector2 anchor = owner.Bottom + new Vector2(0f, FootOffset);
            if (Projectile.ai[0] <= 0f)
            {
                Projectile.Center = anchor;
            }
            else
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, anchor, 0.55f);
            }
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0] += 1f;
            Lighting.AddLight(Projectile.Center, 0.45f, 0.40f, 0.22f);
            if (Main.rand.NextBool(2))
            {
                Vector2 spawn = Projectile.Center + new Vector2(Main.rand.NextFloat(-32f, 32f), Main.rand.NextFloat(-6f, 8f));
                Dust dust = Dust.NewDustPerfect(spawn, DustID.GoldFlame, new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-0.4f, 0.4f)), 150, default, 0.85f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(4))
            {
                Vector2 spawn = Projectile.Center + new Vector2(Main.rand.NextFloat(-36f, 36f), Main.rand.NextFloat(-2f, 6f));
                Dust dust = Dust.NewDustPerfect(spawn, DustID.Cloud, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.05f, 0.4f)), 140, default, 0.8f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);

            Vector2 center = Projectile.Center - Main.screenPosition;
            float age = Projectile.ai[0];

            Texture2D pix = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow").Value;
            Rectangle pixRect = new Rectangle(0, 0, 1, 1);
            Vector2 pixOrigin = new Vector2(0.5f, 0.5f);
            
            DrawHalo(pix, pixRect, pixOrigin, center, 120f, new Color(255, 230, 170), 0.20f);
            DrawHalo(pix, pixRect, pixOrigin, center, 80f, new Color(255, 245, 200), 0.30f);
            Texture2D cloudTex = null;

            if (TextureAssets.Cloud != null
                && TextureAssets.Cloud.Length > 0
                && TextureAssets.Cloud[0] != null
                && TextureAssets.Cloud[0].IsLoaded)
            {
                cloudTex = TextureAssets.Cloud[0].Value;
            }
            if (cloudTex != null)
            {
                DrawCloudPuffs(cloudTex, center, age);
            }
            else
            {
                DrawFallbackPuffs(pix, pixRect, pixOrigin, center, age);
            }

            float corePulse = 1f + MathF.Sin(age / 14f) * 0.05f;
            Color coreColor = new Color(255, 250, 220, 0) * 0.45f;
            
            Main.spriteBatch.Draw(pix, center, pixRect, coreColor, 0f, pixOrigin, 32f * corePulse, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private static void DrawHalo(Texture2D pix, Rectangle pixRect, Vector2 pixOrigin, Vector2 center, float size, Color color, float intensity)
        {
            Color c = color * intensity;
            c.A = 0;
            Main.spriteBatch.Draw(pix, center, pixRect, c, 0f, pixOrigin, size, SpriteEffects.None, 0f);
        }
        private static void DrawCloudPuffs(Texture2D cloudTex, Vector2 center, float age)
        {
            DrawPuff(cloudTex, center + new Vector2(-30f, 4f), 0.55f, age, 0.0f, new Color(255, 250, 235));
            DrawPuff(cloudTex, center + new Vector2(0f, -2f), 0.70f, age, 1.5f, new Color(255, 255, 245));
            DrawPuff(cloudTex, center + new Vector2(30f, 4f), 0.55f, age, 3.0f, new Color(255, 250, 235));
            DrawPuff(cloudTex, center + new Vector2(-14f, -8f), 0.48f, age, 4.2f, new Color(255, 252, 240));
            DrawPuff(cloudTex, center + new Vector2(16f, -8f), 0.48f, age, 5.7f, new Color(255, 252, 240));
        }
        private static void DrawPuff(Texture2D tex, Vector2 pos, float baseScale, float age, float phase, Color tint)
        {
            float pulse = MathF.Sin((age + phase * 60f) / 18f) * 0.06f;
            float scale = baseScale * (1f + pulse);
            Vector2 origin = tex.Size() * 0.5f;
            Main.spriteBatch.Draw(tex, pos, null, tint, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        private static void DrawFallbackPuffs(Texture2D pix, Rectangle pixRect, Vector2 pixOrigin, Vector2 center, float age)
        {
            DrawEllipse(pix, pixRect, pixOrigin, center + new Vector2(-30f, 4f), 40f, 18f, age, 0f);
            DrawEllipse(pix, pixRect, pixOrigin, center + new Vector2(0f, -2f), 50f, 24f, age, 1.5f);
            DrawEllipse(pix, pixRect, pixOrigin, center + new Vector2(30f, 4f), 40f, 18f, age, 3f);
            DrawEllipse(pix, pixRect, pixOrigin, center + new Vector2(-12f, -10f), 28f, 16f, age, 4.2f);
            DrawEllipse(pix, pixRect, pixOrigin, center + new Vector2(14f, -10f), 28f, 16f, age, 5.7f);
        }
        private static void DrawEllipse(Texture2D pix, Rectangle pixRect, Vector2 pixOrigin, Vector2 pos, float sx, float sy, float age, float phase)
        {
            float pulse = MathF.Sin((age + phase * 60f) / 18f) * 0.06f;
            float scaleX = sx * (1f + pulse);
            float scaleY = sy * (1f + pulse);
            Color soft = new Color(255, 252, 240, 0) * 0.55f;
            Main.spriteBatch.Draw(pix, pos, pixRect, soft, 0f, pixOrigin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            Color bright = new Color(255, 255, 255, 0) * 0.30f;
            Main.spriteBatch.Draw(pix, pos, pixRect, bright, 0f, pixOrigin, new Vector2(scaleX * 0.55f, scaleY * 0.55f), SpriteEffects.None, 0f);
        }
    }
}
