using System;
using System.Collections.Generic;
using jarkos.items.accs.melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
namespace jarkos.items.accs.amulets
{
	public class OverloadCloak : ModItem {
		public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
			Item.value = Item.sellPrice(silver: 50);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<AuraRegenPlayer>();
            modPlayer.AuraRegenActivated = true;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<SolarChip>(), 1);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 12);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
    public class AuraRegenPlayer : ModPlayer
    {
        public bool AuraRegenActivated;
        private const int AuraRadius = 250;
        private const int ParticleMax = 45;
        private const float MaxRegenBonus = 10f;
        private const float RegenPerEnemy = 1f;
        private int current_regen;
        public float damageBonusPercent = 0f;
        public override void ResetEffects()
        {
            damageBonusPercent = 0f;
        }
        public override void PostUpdateBuffs()
        {
            if (!AuraRegenActivated)
                return;
            int enemyCount = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.townNPC && npc.lifeMax > 5 && !npc.immortal)
                {
                    float distance = Vector2.Distance(Player.Center, npc.Center);
                    if (distance <= AuraRadius)
                    {
                        enemyCount++;
                    }
                }
            }
            current_regen = enemyCount * 2;
            if (enemyCount == 0)
                return;
            float regenBonus = Math.Min(enemyCount * RegenPerEnemy, MaxRegenBonus);
            Player.lifeRegen += (int)(regenBonus * 2);
            damageBonusPercent = regenBonus;
            Player.GetDamage(DamageClass.Generic) += damageBonusPercent / 100f;
        }
        public void DrawAuraParticles()
        {
            if (!AuraRegenActivated)
                return;
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < ParticleMax; i++)
            {
                float angle = MathHelper.TwoPi * i / current_regen;
                float timeOffset = (float)Main.timeForVisualEffects * 0.01f;
                angle += timeOffset;
                Vector2 particlePos = Player.Center + new Vector2((float)Math.Cos(angle) * AuraRadius,(float)Math.Sin(angle) * AuraRadius);
                float pulseScale = 0.05f * (1f + 0.2f * (float)Math.Sin(Main.timeForVisualEffects * 0.05f + i));
                Texture2D particleTexture = ModContent.Request<Texture2D>("jarkos/textures/StarTexture").Value;
                Vector2 origin = new Vector2(particleTexture.Width / 2, particleTexture.Height / 2);
                Main.spriteBatch.Draw(particleTexture, particlePos - Main.screenPosition, null, Color.Orange, angle, origin, pulseScale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
        }
    }
    public class AuraParticlesSystem : ModSystem
    {
        public override void PostDrawTiles()
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
                return;
            AuraRegenPlayer modPlayer = player.GetModPlayer<AuraRegenPlayer>();
            modPlayer.DrawAuraParticles();
        }
    }
}
