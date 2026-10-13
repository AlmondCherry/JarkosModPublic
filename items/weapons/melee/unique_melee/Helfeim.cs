using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.UI.Chat;
using jarkos.projectiles.melee.unique_melee;
using jarkos.systems;
using sdd.topor.Content.Projectiles;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.unique_melee;
public class Helfeim : ModItem
{
    private const float InventoryDrawScale = 1f;
    private const float WorldDrawScale = 0.42f;
    private const string RelativeIconTexturePath = "textures/Helfeim/CosmicVoidAxe_Icon";
    private const string RelativeIconCosmicMaskTexturePath = "textures/Helfeim/CosmicVoidAxe_Icon_CosmicMask";
    private const string RelativeWorldTexturePath = "textures/Helfeim/Weapons/CosmicVoidAxe";
    private const string RelativeWorldCosmicMaskTexturePath = "textures/Helfeim/CosmicVoidAxe_CosmicMask";
    public override string Texture => $"{Mod.Name}/{RelativeIconTexturePath}";
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 420;
        Item.DamageType = DamageClass.Melee;
        Item.width = 58;
        Item.height = 70;
        Item.useTime = 34;
        Item.useAnimation = 34;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 11f;
        Item.value = Item.buyPrice(platinum: 10);
        Item.rare = ItemRarityID.Purple;
        Item.UseSound = null;
        Item.autoReuse = true;
        Item.channel = true;
        Item.axe = 0;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<CosmicVoidAxeSwingProjectile>();
        Item.shootSpeed = 1f;
    }
    public override bool MeleePrefix() => true;
    public override bool CanUseItem(Player player)
    {
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        if (modPlayer.StarStormLockTimer > 0)
            return false;
        if (modPlayer.ShieldTimer > 0)
            return false;
        if (player.ownedProjectileCounts[ModContent.ProjectileType<CosmicVoidAxeShieldProjectile>()] > 0)
            return false;
        if (player.ownedProjectileCounts[ModContent.ProjectileType<CosmicVoidStarStormAxeProjectile>()] > 0)
            return false;
        if (player.altFunctionUse == 2)
        {
            if (modPlayer.Souls <= 0)
                return false;
            if (modPlayer.IsShieldOnCooldown)
            {
                modPlayer.TryDenyShieldActivation();
                return false;
            }
            Item.useTime = 34;
            Item.useAnimation = 150;
            Item.channel = true;
        }
        else
        {
            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.channel = false;
        }
        return player.ownedProjectileCounts[ModContent.ProjectileType<CosmicVoidAxeSwingProjectile>()] <= 0;
    }
    public override bool AltFunctionUse(Player player) => true;
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        for (int i = 0; i < tooltips.Count; i++)
        {
            TooltipLine line = tooltips[i];
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                line.OverrideColor = Color.White;
                continue;
            }
        }
    }
    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod != "Terraria" || line.Name != "ItemName")
            return true;
        DrawWaterInkText(Main.spriteBatch, line.Font, line.Text, new Vector2(line.X, line.Y), line.BaseScale, line.MaxWidth, line.Spread);
        return false;
    }
    private static void DrawWaterInkText(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 position, Vector2 scale, float maxWidth, float spread)
    {
        Vector2 cursor = position;
        float time = Main.GlobalTimeWrappedHourly * 0.42f;
        for (int i = 0; i < text.Length; i++)
        {
            string character = text[i].ToString();
            if (character == " ")
            {
                cursor.X += font.MeasureString(character).X * scale.X;
                continue;
            }
            float flow = i * 0.27f + time;
            float waveA = (float)Math.Sin((flow * 1.9f) * MathHelper.TwoPi);
            float waveB = (float)Math.Sin((flow * 0.73f + 0.31f) * MathHelper.TwoPi);
            float waveC = (float)Math.Sin((flow * 1.21f + 0.68f) * MathHelper.TwoPi);
            Color colorA = Color.Lerp(new Color(78, 16, 150), new Color(255, 255, 255), waveA * 0.5f + 0.5f);
            Color colorB = Color.Lerp(new Color(145, 55, 255), new Color(230, 205, 255), waveB * 0.5f + 0.5f);
            Color colorC = Color.Lerp(new Color(35, 0, 80), new Color(255, 245, 255), waveC * 0.5f + 0.5f);
            Color color = Color.Lerp(Color.Lerp(colorA, colorB, 0.48f + waveA * waveB * 0.14f), colorC, 0.16f + waveC * 0.08f);
            Vector2 offset = new Vector2((float)Math.Sin((flow * 0.82f) * MathHelper.TwoPi) * 0.65f, (float)Math.Sin((flow * 1.4f) * MathHelper.TwoPi) * 1.05f);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, character, cursor + offset + new Vector2(0f, 1.6f), Color.Black * 0.9f, 0f, Vector2.Zero, scale, maxWidth, spread);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, character, cursor + offset + new Vector2(-1.8f, -0.2f), colorA * 0.5f, 0f, Vector2.Zero, scale, maxWidth, spread);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, character, cursor + offset + new Vector2(1.8f, 0.2f), colorB * 0.5f, 0f, Vector2.Zero, scale, maxWidth, spread);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, character, cursor + offset + new Vector2(0f, -1.4f), colorC * 0.36f, 0f, Vector2.Zero, scale, maxWidth, spread);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, character, cursor + offset, color, 0f, Vector2.Zero, scale, maxWidth, spread);
            cursor.X += font.MeasureString(character).X * scale.X;
        }
    }
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        damage *= 1.35f;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        bool groundSlamReady = player.velocity.Y == 0f && modPlayer.GroundWaveCooldown <= 0;
        int attackType = player.altFunctionUse == 2
            ? CosmicVoidAxeSwingProjectile.AttackSoulChannel
            : (groundSlamReady ? CosmicVoidAxeSwingProjectile.AttackGroundSlam : CosmicVoidAxeSwingProjectile.AttackAirSpin);
        if (attackType == CosmicVoidAxeSwingProjectile.AttackGroundSlam || attackType == CosmicVoidAxeSwingProjectile.AttackAirSpin)
            CosmicVoidAxeSwingProjectile.GetNextCombo(player);
        Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI, attackType, player.direction, player.altFunctionUse == 2 ? 1f : 0f);
        return false;
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CelestiaStar>(), 2)
            .AddIngredient(ModContent.ItemType<NitoziumPlate>(), 1)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D baseTexture = TextureAssets.Item[Type].Value;
        float drawScale = scale * InventoryDrawScale;
        spriteBatch.Draw(baseTexture, position, frame, drawColor, 0f, origin, drawScale, SpriteEffects.None, 0f);
        DrawCosmicMask(spriteBatch, $"{Mod.Name}/{RelativeIconCosmicMaskTexturePath}", position, frame, origin, drawScale, 0f, SpriteEffects.None, Main.UIScaleMatrix, 1f);
        return false;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D baseTexture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeWorldTexturePath}").Value;
        Rectangle frame = baseTexture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 position = Item.Center - Main.screenPosition;
        Color worldColor = Item.GetAlpha(lightColor);
        SpriteEffects effects = SpriteEffects.None;
        float drawScale = scale * WorldDrawScale;
        spriteBatch.Draw(baseTexture, position, frame, worldColor, rotation, origin, drawScale, effects, 0f);
        DrawCosmicMask(spriteBatch, $"{Mod.Name}/{RelativeWorldCosmicMaskTexturePath}", position, frame, origin, drawScale, rotation, effects, Main.GameViewMatrix.TransformationMatrix, 1f - Item.alpha / 255f);
        return false;
    }
    private static void DrawCosmicMask(SpriteBatch spriteBatch, string maskTexturePath, Vector2 position, Rectangle? frame, Vector2 origin, float scale, float rotation, SpriteEffects effects, Matrix matrix, float opacity)
    {
        if (Main.dedServ || !GameShaders.Misc.ContainsKey(CosmicItemShaderSystem.CosmicItemShaderKey))
            return;
        Texture2D maskTexture = ModContent.Request<Texture2D>(maskTexturePath).Value;
        MiscShaderData shader = GameShaders.Misc[CosmicItemShaderSystem.CosmicItemShaderKey];
        int souls = Main.LocalPlayer.GetModPlayer<CosmicVoidAxePlayer>().Souls;
        shader.UseColor(CosmicVoidAxePlayer.GetSoulPrimaryColor(souls));
        shader.UseSecondaryColor(CosmicVoidAxePlayer.GetSoulSecondaryColor(souls));
        shader.UseOpacity(opacity);
        shader.UseSaturation(Main.GlobalTimeWrappedHourly);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
        shader.Apply(null);
        spriteBatch.Draw(maskTexture, position, frame, Color.White, rotation, origin, scale, effects, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
    }
    public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        noHitbox = true;
    }
    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        player.GetModPlayer<CosmicVoidAxePlayer>().RegisterAxeHit(target, damageDone);
        SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.35f, Pitch = 0.25f }, target.Center);
        for (int i = 0; i < 8; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Circular(2.4f, 2.4f);
            Dust dust = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, velocity);
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(0.8f, 1.25f);
        }
    }
}
