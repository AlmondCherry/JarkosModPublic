using System;
using jarkos.projectiles.melee.unique_melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.melee.unique_melee
{
    public class KarmaBlade : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 165;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 54;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;

            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ModContent.RarityType<jarkos.rare.StormRarity>();
            
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<KarmaBladeSwing>();
            Item.shootSpeed = 1f;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<KarmaBladePlayer>();
            if (player.altFunctionUse == 2)
            {
                if (modPlayer.dashCharges <= 0)
                    return false;
                Item.useTime = 26;
                Item.useAnimation = 26;
                return true;
            }
            int step = modPlayer.comboStep;
            int swingTime = step == 2 ? KarmaBladeSwing.FINISHER_TIME : KarmaBladeSwing.SWING_TIME;
            Item.useTime = swingTime + 2;
            Item.useAnimation = swingTime + 2;
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<KarmaBladePlayer>();
            if (player.altFunctionUse == 2)
            {
                int burnedK = modPlayer.BurnKarma();
                modPlayer.dashCharges--;
                int dashDamage = (int)(damage * (1.1f + 0.22f * burnedK));
                Vector2 dashDir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
                Projectile.NewProjectile(source, player.Center, dashDir, ModContent.ProjectileType<KarmaDash>(), dashDamage, knockback * 0.6f, player.whoAmI, burnedK);
                return false;
            }
            int step = modPlayer.UseSwing();
            int angDir = step == 1 ? -1 : 1;
            if (step == 2)
            {
                int burned = modPlayer.BurnKarma();
                damage = (int)(damage * (1.35f + 0.12f * burned));
            }
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
            Projectile.NewProjectile(source, player.MountedCenter, dir, ModContent.ProjectileType<KarmaBladeSwing>(), damage, knockback, player.whoAmI, step, angDir);
            return false;
        }
    }
    public class KarmaBladePlayer : ModPlayer
    {
        public const int COMBO_WINDOW = 65;
        public const int KARMA_MAX = 8;
        public const int DASH_CHARGES_MAX = 3;
        public const int DASH_RECHARGE_TIME = 300;
        public int comboStep;
        public int comboTimer;
        public int karma;
        public int dashCharges = DASH_CHARGES_MAX;
        public int dashRecharge;
        public float RechargeProgress => dashRecharge / (float)DASH_RECHARGE_TIME;
        public override void Initialize()
        {
            dashCharges = DASH_CHARGES_MAX;
            dashRecharge = 0;
        }
        public int UseSwing()
        {
            int step = comboStep;
            comboStep = (comboStep + 1) % 3;
            comboTimer = COMBO_WINDOW;
            return step;
        }
        public void AddKarma(int amount)
        {
            int before = karma;
            karma = Math.Min(karma + amount, KARMA_MAX);
            if (karma == KARMA_MAX && before < KARMA_MAX && Main.myPlayer == Player.whoAmI)
                SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.6f, Pitch = 0.5f }, Player.Center);
        }
        public int BurnKarma()
        {
            int burned = karma;
            karma = 0;
            return burned;
        }
        public override void PostUpdate()
        {
            if (comboTimer > 0)
            {
                comboTimer--;
                if (comboTimer == 0)
                    comboStep = 0;
            }
            if (dashCharges < DASH_CHARGES_MAX)
            {
                dashRecharge++;
                if (dashRecharge >= DASH_RECHARGE_TIME)
                {
                    dashRecharge = 0;
                    dashCharges++;
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        new jarkos.particles.Spark(Player.MountedCenter, -Vector2.UnitY * 2f, KarmaBladeSwing.ColorEdge, 22, 0.55f, 0.9f).Spawn();
                        SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.45f, Pitch = 0.2f }, Player.Center);
                    }
                }
            }
            else
            {
                dashRecharge = 0;
            }
            if (Main.myPlayer == Player.whoAmI && !Player.dead
                && Player.HasItem(ModContent.ItemType<KarmaBlade>())
                && Player.ownedProjectileCounts[ModContent.ProjectileType<KarmaCharm>()] < 1)
            {
                Projectile.NewProjectile(Player.GetSource_Misc("KarmaCharm"), Player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<KarmaCharm>(), 0, 0f, Player.whoAmI);
            }
        }
    }
    public class KarmaBladeSheathLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Torso);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            return !player.dead
                && player.HeldItem != null
                && player.HeldItem.type == ModContent.ItemType<KarmaBlade>();
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            Texture2D texture = ModContent.Request<Texture2D>("jarkos/items/weapons/melee/unique_melee/KarmaBladeSheath", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            int dir = player.direction;
            Vector2 off = new Vector2(-4f * dir, 6f);
            float rot = 0.69f;
            if (dir == -1)
            {
                rot = -rot;
            }
            if (player.gravDir == -1f)
            {
                off.Y = -off.Y;
                rot = -rot;
            }
            Vector2 drawPos = (player.Center - Main.screenPosition + off).Floor();
            Color color = player.GetImmuneAlpha(drawInfo.colorArmorBody, drawInfo.shadow);
            drawInfo.DrawDataCache.Add(new DrawData(texture, drawPos, null, color, rot, texture.Size() * 0.5f, 1f, drawInfo.playerEffect, -2));
        }
    }
}
