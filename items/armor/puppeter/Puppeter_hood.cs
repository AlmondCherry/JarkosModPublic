using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.Localization;

namespace jarkos.items.armor.puppeter
{
    [AutoloadEquip(EquipType.Head)]
    public class Puppeter_hood : ModItem
    {
        public static LocalizedText SetBonusText { get; private set; }
	    public override void SetStaticDefaults()
        {
		    SetBonusText = this.GetLocalization(nameof(SetBonusText));
	    }
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 26;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 11;
        }
        public override void UpdateEquip(Player player)
        {
            player.maxMinions += 1;
            player.GetDamage(DamageClass.Summon) += 0.12f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<Puppeter_breastplace>()
                && legs.type == ModContent.ItemType<Puppeter_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            var pp = player.GetModPlayer<PuppeteerPlayer>();
            pp.SetActive = true;
            
            player.setBonus = (string)SetBonusText;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.Silk, 12)
                .AddIngredient(ItemID.SpookyWood, 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class PuppeteerThreadLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            if (player == null || !player.active || player.dead) return false;

            var pp = player.GetModPlayer<PuppeteerPlayer>();

            return pp != null && pp.SetActive;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;

            if (player == null || !player.active || player.dead) return;
            var pp = player.GetModPlayer<PuppeteerPlayer>();
            if (pp == null || !pp.SetActive) return;

            Vector2 playerScreenCenter = player.MountedCenter - Main.screenPosition;
            SpriteBatch sb = Main.spriteBatch;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Rectangle src = new Rectangle(0, 0, 1, 1);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];

                if (!proj.active || proj.owner != player.whoAmI) continue;
                if (!proj.minion && proj.minionSlots <= 0f) continue;

                float dist = Vector2.Distance(player.Center, proj.Center);
                if (dist <= 600f)
                {
                    Vector2 minionScreen = proj.Center - Main.screenPosition;

                    const int segments = 24;
                    float sagAmount = 40f;
                    Vector2? previousPoint = null;

                    for (int j = 0; j <= segments; j++)
                    {
                        float t = j / (float)segments;
                        float sag = sagAmount * (float)Math.Sin(t * Math.PI) * (dist / 600f);

                        Vector2 currentPoint = Vector2.Lerp(playerScreenCenter, minionScreen, t) + new Vector2(0f, sag);

                        if (previousPoint.HasValue)
                        {
                            Vector2 segment = currentPoint - previousPoint.Value;
                            float length = segment.Length();

                            if (length >= 0.1f)
                            {
                                float rotation = (float)Math.Atan2(segment.Y, segment.X);

                                sb.Draw(pixel, previousPoint.Value, src, new Color(255, 50, 150) * 0.4f, rotation, new Vector2(0f, 0.5f), new Vector2(length + 1f, 4f), SpriteEffects.None, 0f);
                                sb.Draw(pixel, previousPoint.Value, src, new Color(255, 180, 220) * 0.9f, rotation, new Vector2(0f, 0.5f), new Vector2(length + 1f, 1.5f), SpriteEffects.None, 0f);
                            }
                        }
                        previousPoint = currentPoint;
                    }
                }
            }
        }
    }
    public class PuppeteerPlayer : ModPlayer
    {
        public bool SetActive;
        public override void ResetEffects()
        {
            SetActive = false;
        }
        public override void PostUpdateEquips()
        {
            if (SetActive)
            {
                int connectedCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == Player.whoAmI && (proj.minion || proj.minionSlots > 0f))
                    {
                        if (Vector2.Distance(Player.Center, proj.Center) <= 600f)
                        {
                            connectedCount++;
                        }
                    }
                }
                
                Player.statDefense += connectedCount * 4;
                Player.lifeRegen += connectedCount * 1;
                Player.manaRegen += connectedCount * 1;
            }
        }
    }
    public class PuppeteerGlobalProjectile : GlobalProjectile
    {
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (projectile.minion || projectile.minionSlots > 0f)
            {
                Player player = Main.player[projectile.owner];
                if (player != null && player.active && !player.dead)
                {
                    var pp = player.GetModPlayer<PuppeteerPlayer>();
                    if (pp != null && pp.SetActive)
                    {
                        if (Vector2.Distance(player.Center, projectile.Center) > 600f)
                        {
                            modifiers.SourceDamage += 0.15f;
                        }
                    }
                }
            }
        }
    }
}
