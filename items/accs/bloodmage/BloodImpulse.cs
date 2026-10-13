using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using jarkos.items.usetiles;
using jarkos.items.materials;
using Microsoft.Xna.Framework.Graphics;
using jarkos.buffs.debuff;
using Terraria.GameContent;
using System.Collections.Generic;
using System.Linq;
using System;
using Terraria.Localization;

namespace jarkos.items.accs.bloodmage
{
	public class BloodImpulse : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 8));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 50;
			Item.height = 23;
			Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<CrimsonPulsePlayer>().hasCrimsonPulse = true;
		}
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			var tooltipLine = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");

			if (tooltipLine != null)
			{
				var bindKey = CrimsonPulseKeybinds.ActivateAbility.GetAssignedKeys();
                string firstbindkey = bindKey.Count > 0 ? bindKey[0] : "None";

				tooltipLine.Text = Language.GetTextValue("Mods.jarkos.Items.BloodImpulse.Tooltip", firstbindkey);
			}
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.HallowedBar, 4)
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class CrimsonPulseKeybinds : ModSystem
    {
		public static ModKeybind ActivateAbility { get; private set; }
		public override void Load()
		{
			ActivateAbility = KeybindLoader.RegisterKeybind(Mod, "Blood Impulse", "G");
		}
		public override void Unload()
		{
			ActivateAbility = null;
		}
    }
	public class CrimsonPulsePlayer : ModPlayer
    {
        public bool hasCrimsonPulse;
        public int cooldown;
        public const int cooldownMax = 60 * 60;
        public override void ResetEffects()
        {
            hasCrimsonPulse = false;
        }
        public override void PostUpdate()
        {
            if (cooldown > 0)
                cooldown--;

            if (hasCrimsonPulse && CrimsonPulseKeybinds.ActivateAbility.JustPressed && cooldown <= 0)
            {
                ActivateCrimsonPulse();
            }
        }
        private void ActivateCrimsonPulse()
        {
            if (Player.statLife <= 1)
                return;

            int healthCost = (int)(Player.statLife * 0.25f);
            if (healthCost < 1)
                healthCost = 1;

            Player.statLife -= healthCost;
            if (Player.statLife < 1)
                Player.statLife = 1;

            Player.HealEffect(-healthCost);
            Vector2 direction = Main.MouseWorld - Player.Center;
            direction.Normalize();
            
            int damage = healthCost * 3;

            Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[3]), Player.Center, direction * 25f, ModContent.ProjectileType<CrimsonPulseLaser>(), damage, 5f, Player.whoAmI, ai0: healthCost);
            cooldown = cooldownMax;

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Player.Center, 0, 0, DustID.Blood, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
                dust.noGravity = true;
            }
        }
    }
	#region Projectile
	public class CrimsonPulseLaser : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/bullet/silent_shard";
        private const int TrailLength = 60;
        private const float TrailWidth = 14f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            if (Projectile.ai[1]++ % 4 == 0)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CrimsonTrail>(), Projectile.damage / 2, 0f, Projectile.owner, ai0: Projectile.ai[0]);
            }

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4), 8, 8, DustID.Blood, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.1f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.8f, 0.1f, 0.1f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BloodInfection>(), 600);
            CrimsonPulseGlobalNPC globalNPC = target.GetGlobalNPC<CrimsonPulseGlobalNPC>();

            globalNPC.storedLaserDamage += damageDone;
            globalNPC.infectionTimer = 600;

            CreateBloodExplosion();
        }
        public override void OnKill(int timeLeft)
        {
            CreateBloodExplosion();
        }
        private void CreateBloodExplosion()
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, velocity.X, velocity.Y, 100, default, 1.5f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;

                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;

                float length = direction.Length();
                if (length < 1f)
                    continue;

                float rotation = direction.ToRotation();
                float progress = (float)i / Projectile.oldPos.Length;
                float width = TrailWidth * (1f - progress);

                Color trailColor = GetTrailColor(progress);
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                Vector2 origin = new Vector2(0f, 0.5f);
                Vector2 scale = new Vector2(length, width);

                Main.EntitySpriteDraw(pixel, start, sourceRect, trailColor, rotation, origin, scale, SpriteEffects.None, 0);
            }
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;

                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;

                float length = direction.Length();
                if (length < 1f)
                    continue;

                float rotation = direction.ToRotation();
                float progress = (float)i / Projectile.oldPos.Length;
                float width = TrailWidth * 2.5f * (1f - progress);

                Color glowColor = GetGlowColor(progress);
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);

                Vector2 origin = new Vector2(0f, 0.5f);
                Vector2 scale = new Vector2(length, width);

                Main.EntitySpriteDraw(pixel, start, sourceRect, glowColor, rotation, origin, scale, SpriteEffects.None, 0);
            }

            DrawLaserHead();
            return false;
        }
        private Color GetTrailColor(float progress)
        {
            int r = (int)MathHelper.Lerp(255, 80, progress);
            int g = (int)MathHelper.Lerp(50, 0, progress);
            int b = (int)MathHelper.Lerp(50, 0, progress);
            int a = (int)MathHelper.Lerp(255, 0, progress);

            return new Color(r, g, b, a);
        }
        private Color GetGlowColor(float progress)
        {
            int r = (int)MathHelper.Lerp(150, 20, progress);
            int a = (int)MathHelper.Lerp(80, 0, progress);

            return new Color(r, 0, 0, a);
        }
        private void DrawLaserHead()
        {
            if (Projectile.oldPos[0] == Vector2.Zero)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 headPos = Projectile.Center - Main.screenPosition;

            for (int i = 0; i < 3; i++)
            {
                float size = (3 - i) * 8f;
                int alpha = 100 + i * 50;
                Color coreColor = new Color(255, 100, 100, alpha);
                Main.EntitySpriteDraw(pixel, headPos, new Rectangle(0, 0, 1, 1), coreColor, 0f, new Vector2(0.5f), size, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(pixel, headPos, new Rectangle(0, 0, 1, 1), new Color(255, 0, 0, 50), 0f, new Vector2(0.5f), 30f, SpriteEffects.None, 0);
        }
    }
	#endregion
	#region Trail
	public class CrimsonTrail : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/bullet/silent_shard";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override void AI()
        {
            Projectile.scale = 0.8f + (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.2f;
            if (Projectile.timeLeft < 60)
            {
                Projectile.alpha += 4;
            }
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, -1f, 100, default, 0.8f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(30))
            {
                CreateMiniExplosion();
            }
            Projectile.rotation += 0.05f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BloodInfection>(), 600);
            CrimsonPulseGlobalNPC globalNPC = target.GetGlobalNPC<CrimsonPulseGlobalNPC>();

            globalNPC.storedLaserDamage += damageDone;
            globalNPC.infectionTimer = 600;

            CreateMiniExplosion();
        }
        private void CreateMiniExplosion()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, velocity.X, velocity.Y, 100, default, 1.2f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Color drawColor = new Color(150, 0, 0, 100) * ((255 - Projectile.alpha) / 255f);

            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, drawColor * 0.5f, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(200, 20, 20, 100);
        }
    }
	#endregion
	#region GlobalNPC
	public class CrimsonPulseGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool hasBloodInfection;
        public int storedLaserDamage;
        public int infectionTimer;
        public const float DebuffDamageMultiplier = 1.5f;
        public override void ResetEffects(NPC npc)
        {
            hasBloodInfection = false;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (hasBloodInfection)
            {
                infectionTimer--;
                if (npc.lifeRegen < 0)
                {
                    npc.lifeRegen = (int)(npc.lifeRegen * DebuffDamageMultiplier);
                }
                npc.lifeRegen -= 10;
                if (damage < 2)
                    damage = 2;
                if (infectionTimer <= 0 && storedLaserDamage > 0)
                {
                    HealEnemy(npc);
                }
            }
            else
            {
                if (infectionTimer > 0)
                {
                    infectionTimer = 0;
                    storedLaserDamage = 0;
                }
            }
        }
        private void HealEnemy(NPC npc)
        {
            if (npc.life <= 0 || !npc.active)
                return;

            int healAmount = storedLaserDamage;
            npc.life += healAmount;

            if (npc.life > npc.lifeMax)
                npc.life = npc.lifeMax;

            npc.HealEffect(healAmount);

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GreenTorch, 0f, -3f, 100, default, 1.5f);
                dust.noGravity = true;
            }

            storedLaserDamage = 0;
            infectionTimer = 0;
        }
        public override void OnKill(NPC npc)
        {
            if (hasBloodInfection)
            {
                storedLaserDamage = 0;
                infectionTimer = 0;
                for (int i = 0; i < 30; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                    Dust dust = Dust.NewDustDirect(npc.Center, 0, 0, DustID.Blood, velocity.X, velocity.Y, 100, default, 2f);
                    dust.noGravity = true;
                }
            }
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (hasBloodInfection)
            {
                bool hasOtherDebuffs = false;
                for (int i = 0; i < NPC.maxBuffs; i++)
                {
                    if (npc.buffType[i] > 0 && npc.buffType[i] != ModContent.BuffType<BloodInfection>() && Main.debuff[npc.buffType[i]])
                    {
                        hasOtherDebuffs = true;
                        break;
                    }
                }
                if (hasOtherDebuffs)
                {
                    modifiers.FinalDamage *= 1.25f;
                }
            }
        }
    }
	#endregion
}
