using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Localization;
using jarkos.projectiles.melee.unique_melee;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using jarkos.items.materials;
using Terraria.Audio;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Shpaga_mastera : ModItem
	{
        private static readonly int[] DustTypes = {
            DustID.Stone, DustID.Copper, DustID.Iron, DustID.Silver, DustID.Mythril, DustID.Adamantite, DustID.Vortex
        };
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.damage = 11;
            Item.knockBack = 5f;
            Item.DamageType = DamageClass.Melee;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Metamorph_slash>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            var mp = player.GetModPlayer<EvolutionBladePlayer>();
            float damageBonus = mp.evolutionStage switch
            {
                0 => 0f, 1 => 0.1f, 2 => 0.35f, 3 => 0.5f, 4 => 0.7f, 5 => 0.9f, 6 => 1.5f, _ => 0f
            };
            damage += damageBonus;
        }
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            var mp = player.GetModPlayer<EvolutionBladePlayer>();
            if (mp.evolutionStage >= 3)
            {
                crit += 10 + (mp.evolutionStage - 3) * 8;
            }
            if (mp.evolutionStage >= 6)
            {
                crit += 25;
            }
        }
        public override float UseSpeedMultiplier(Player player)
        {
            var mp = player.GetModPlayer<EvolutionBladePlayer>();
            return mp.evolutionStage switch
            {
                0 => 1f, 1 => 1.2f, 2 => 1.25f, 3 => 1.3f, 4 => 1.35f, 5 => 1.4f, 6 => 1.6f, _ => 1f
            };
        }
        public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback)
        {
            var mp = player.GetModPlayer<EvolutionBladePlayer>();
            knockback *= 1f + mp.evolutionStage * 0.15f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<EvolutionBladePlayer>();
			if(mp.evolutionStage > -1)
			{
				Projectile.NewProjectile(source, position, velocity * 1.2f, ModContent.ProjectileType<StoneShatter>(), (int)(damage * (1f + mp.evolutionStage * 0.1f)), knockback, player.whoAmI);
			}
			if(mp.evolutionStage > 0)
			{
				Projectile.NewProjectile(source, position, velocity * 1.2f, ModContent.ProjectileType<BronzeSpiral>(), (int)(damage * (1f + mp.evolutionStage * 0.1f)), knockback, player.whoAmI);
			}
			if(mp.evolutionStage > 1)
			{
				Projectile.NewProjectile(source, position, velocity * 1.2f, ModContent.ProjectileType<IronMagnet>(), (int)(damage * (1f + mp.evolutionStage * 0.1f)), knockback, player.whoAmI);
			}
			if(mp.evolutionStage > 2)
			{
				Projectile.NewProjectile(source, position, velocity * 1.2f, ModContent.ProjectileType<SteelRicochet>(), (int)(damage * (1f + mp.evolutionStage * 0.1f)), knockback, player.whoAmI);
			}
			if(mp.evolutionStage > 3)
			{
				Projectile.NewProjectile(source, position, velocity * 1.2f, ModContent.ProjectileType<MythrilWave>(), (int)(damage * (1f + mp.evolutionStage * 0.1f)), knockback, player.whoAmI);
			}
			if(mp.evolutionStage > 4)
			{
				Projectile.NewProjectile(source, position, velocity * 1.2f, ModContent.ProjectileType<AdamantiteVampire>(), (int)(damage * (1f + mp.evolutionStage * 0.1f)), knockback, player.whoAmI);
			}
			if(mp.evolutionStage > 5)
			{
				Projectile.NewProjectile(source, position, velocity * 1.2f, ModContent.ProjectileType<LuminiteLightning>(), (int)(damage * (1f + mp.evolutionStage * 0.1f)), knockback, player.whoAmI);
			}
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (mp.evolutionStage >= 6)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 rotatedVel = velocity.RotatedBy(MathHelper.ToRadians(15 * i));
                    Projectile.NewProjectile(source, position, rotatedVel, ModContent.ProjectileType<LuminiteLightning>(), damage / 2, knockback / 2, player.whoAmI);
                }
            }
            return false;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            var mp = player.GetModPlayer<EvolutionBladePlayer>();
            Color color = EvolutionBladePlayer.StageColors[mp.evolutionStage];
            int dustAmount = 2 + mp.evolutionStage;
            for (int i = 0; i < dustAmount; i++)
            {
                Vector2 dustPos = new Vector2(Main.rand.Next(hitbox.X, hitbox.X + hitbox.Width), Main.rand.Next(hitbox.Y, hitbox.Y + hitbox.Height));
                Dust d = Dust.NewDustDirect(dustPos, 0, 0, DustTypes[mp.evolutionStage]);
                d.noGravity = true;
                d.scale = 1.2f + mp.evolutionStage * 0.15f;
                d.velocity *= 0.3f;
                if (mp.evolutionStage >= 4)
                {
                    Dust d2 = Dust.NewDustDirect(dustPos, 0, 0, DustID.RainbowTorch);
                    d2.noGravity = true;
                    d2.scale = 0.8f;
                    d2.color = color;
                    d2.velocity *= 0.2f;
                }
            }
            if (mp.evolutionStage >= 6)
            {
                Lighting.AddLight(hitbox.Center.ToVector2(), color.ToVector3() * 0.8f);
            }
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            var mp = player.GetModPlayer<EvolutionBladePlayer>();
            if (mp.evolutionStage >= 5)
            {
                int healAmount = Math.Max(1, damageDone / 15);
                player.Heal(healAmount);
                for (int i = 0; i < 5; i++)
                {
                    Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.LifeDrain);
                    d.velocity = (player.Center - target.Center).SafeNormalize(Vector2.Zero) * 5f;
                    d.noGravity = true;
                }
            }
            if (mp.evolutionStage >= 3 && hit.Crit)
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Torch, vel.X, vel.Y);
                    d.noGravity = true;
                    d.scale = 2f;
                }
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && npc.Distance(target.Center) < 100f)
                    {
                        npc.SimpleStrikeNPC(damageDone / 3, hit.HitDirection, false, 0f, DamageClass.Melee);
                    }
                }
            }
            if (mp.evolutionStage >= 2)
            {
                target.AddBuff(BuffID.Slow, 60 + mp.evolutionStage * 20);
            }
            if (mp.evolutionStage >= 4 && target.life <= 0)
            {
                NPC nextTarget = FindNearestEnemy(player, target.Center, 400f);
                if (nextTarget != null)
                {
                    Vector2 teleportPos = nextTarget.Center - new Vector2(50 * player.direction, 0);
                    for (int i = 0; i < 30; i++)
                    {
                        Dust d = Dust.NewDustDirect(player.Center, 0, 0, DustID.Mythril);
                        d.noGravity = true;
                        d.velocity = Main.rand.NextVector2Circular(5f, 5f);
                    }
                    player.Teleport(teleportPos, TeleportationStyleID.RodOfDiscord);
                    for (int i = 0; i < 30; i++)
                    {
                        Dust d = Dust.NewDustDirect(player.Center, 0, 0, DustID.Mythril);
                        d.noGravity = true;
                        d.velocity = Main.rand.NextVector2Circular(5f, 5f);
                    }
                }
            }
            if (mp.evolutionStage >= 5 && Main.rand.NextBool(10))
            {
                for (int i = 0; i < 36; i++)
                {
                    float angle = MathHelper.TwoPi * i / 36f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * 50f;
                    Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RedTorch);
                    d.noGravity = true;
                    d.velocity = Vector2.Zero;
                    d.scale = 1.5f;
                }
            }
        }
        private NPC FindNearestEnemy(Player player, Vector2 position, float maxDistance)
        {
            NPC nearest = null;
            float nearestDist = maxDistance;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = npc.Distance(position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }
            return nearest;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer == null) return;
            var mp = Main.LocalPlayer.GetModPlayer<EvolutionBladePlayer>();
            Color color = EvolutionBladePlayer.StageColors[mp.evolutionStage];
            string stageName = EvolutionBladePlayer.StageNames[mp.evolutionStage];
            tooltips.Add(new TooltipLine(Mod, "Stage", Metamorph_langsystem.stage_current_desc.ToString() + $"[c/{ColorToHex(color)}:{stageName}]"));
            if (mp.evolutionStage < 6)
            {
                int needed = EvolutionBladePlayer.KillsRequired[mp.evolutionStage];
                float progress = (float)mp.killCount / needed * 100f;
                tooltips.Add(new TooltipLine(Mod, "Progress", Metamorph_langsystem.progress_current_desc.ToString() + $"{mp.killCount}/{needed} ({progress:F1}%)"));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "Progress", Metamorph_langsystem.final_desc.ToString()));
            }
            string stageDesc = mp.evolutionStage switch
            {
                0 => Metamorph_langsystem.primitive_stage_desc.ToString(), 1 => Metamorph_langsystem.bronze_stage_desc.ToString(), 2 => Metamorph_langsystem.iron_stage_desc.ToString(), 3 => Metamorph_langsystem.steel_stage_desc.ToString(), 4 => Metamorph_langsystem.mythril_stage_desc.ToString(), 5 => Metamorph_langsystem.adamantite_stage_desc.ToString(), 6 => Metamorph_langsystem.luminite_stage_desc.ToString(), _ => ""
            };
            tooltips.Add(new TooltipLine(Mod, "StageDesc", $"[c/AAAAAA:{stageDesc}]"));
        }
        private string ColorToHex(Color c)
        {
            return $"{c.R:X2}{c.G:X2}{c.B:X2}";
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (Main.LocalPlayer == null) return null;
            var mp = Main.LocalPlayer.GetModPlayer<EvolutionBladePlayer>();
            Color stageColor = EvolutionBladePlayer.StageColors[mp.evolutionStage];
            return Color.Lerp(lightColor, stageColor, 0.4f + mp.evolutionStage * 0.08f);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(nameof(ItemID.IronBar), 8)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddIngredient(ModContent.ItemType<FragmentOfNature>(), 5)
                .AddIngredient(ModContent.ItemType<FragmentOfFear>(), 5)
                .AddIngredient(ModContent.ItemType<EnchantedFragment>(), 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    #region Stages
    public class EvolutionStage
    {
        public string Name { get; set; }
        public string Subtitle { get; set; }
        public int KillsRequired { get; set; }
        public int BaseDamage { get; set; }
        public int UseTime { get; set; }
        public int CritBonus { get; set; }
        public float Scale { get; set; }
        public float KnockBack { get; set; }
        public Color Color { get; set; }
        public Color GlowColor { get; set; }
        public Color TrailColor { get; set; }
        public int DustType { get; set; }
        public string Description { get; set; }
    }
	public class EvolutionBladePlayer : ModPlayer
    {
        public int evolutionStage = 0;
        public int killCount = 0;
        public static readonly int[] KillsRequired = { 25, 50, 100, 150, 250, 400, int.MaxValue };
        public static readonly string[] StageNames = {
            Metamorph_langsystem.primitive_stage.ToString(), Metamorph_langsystem.bronze_stage.ToString(), Metamorph_langsystem.iron_stage.ToString(), Metamorph_langsystem.steel_stage.ToString(), Metamorph_langsystem.mythril_stage.ToString(), Metamorph_langsystem.adamantite_stage.ToString(), Metamorph_langsystem.luminite_stage.ToString()
        };
        public static readonly Color[] StageColors = {
            new Color(128, 128, 128), new Color(205, 127, 50), new Color(160, 160, 160), new Color(180, 180, 200), new Color(100, 200, 150), new Color(255, 100, 100), new Color(0, 255, 200)
        };
        public override void PostUpdate()
        {
            if (evolutionStage >= 4)
            {
                Color color = StageColors[evolutionStage];
                if (Main.rand.NextBool(10 - evolutionStage))
                {
                    Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.RainbowTorch);
                    d.noGravity = true;
                    d.scale = 0.8f;
                    d.velocity = new Vector2(0, -2f);
                    d.color = color;
                }
            }
            if (evolutionStage >= 6)
            {
                Lighting.AddLight(Player.Center, StageColors[6].ToVector3() * 0.3f);
                if (Main.rand.NextBool(5))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = Player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30f, 60f);
                    Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RainbowTorch);
                    d.noGravity = true;
                    d.scale = 1.2f;
                    d.color = Main.hslToRgb(Main.GameUpdateCount * 0.01f % 1f, 1f, 0.6f);
                    d.velocity = (Player.Center - pos).SafeNormalize(Vector2.Zero) * 2f;
                }
            }
        }
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (evolutionStage >= 6 && Main.rand.NextBool(7))
            {
                modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) =>
                {
                    info.Damage = 0;
                };
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust d = Dust.NewDustDirect(Player.Center, 0, 0, DustID.Vortex, vel.X, vel.Y);
                    d.noGravity = true;
                    d.scale = 1.5f;
                }
            }
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (evolutionStage > 0)
            {
                evolutionStage--;
                killCount = KillsRequired[evolutionStage] / 2;
                for (int i = 0; i < 50; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(10f, 10f);
                    Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Smoke, dustVel.X, dustVel.Y);
                    d.scale = 2f;
                    d.color = StageColors[evolutionStage];
                }
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 vel = angle.ToRotationVector2() * 8f;
                    Dust d = Dust.NewDustDirect(Player.Center, 0, 0, DustID.RainbowTorch, vel.X, vel.Y);
                    d.noGravity = true;
                    d.scale = 2f;
                    d.color = StageColors[evolutionStage + 1];
                }
                Main.NewText($"The Mutagen has been degraded to: {StageNames[evolutionStage]}", Color.Red);
                SoundEngine.PlaySound(SoundID.NPCDeath6, Player.Center);
            }
        }
        public void AddKill()
        {
            killCount++;
            if (evolutionStage < 6 && killCount >= KillsRequired[evolutionStage])
            {
                Evolve();
            }
            SpawnKillEffect();
        }
        private void Evolve()
        {
            evolutionStage++;
            killCount = 0;
            SpawnEvolutionEffect();
            SoundEngine.PlaySound(SoundID.Item119, Player.Center);
            SoundEngine.PlaySound(SoundID.Roar, Player.Center);
            Main.NewText($"The Mutagen has reached the stage of: {StageNames[evolutionStage]}!", StageColors[evolutionStage]);
            Player.Heal(Player.statLifeMax2 / 4);
            Player.AddBuff(BuffID.Wrath, 600);
            Player.AddBuff(BuffID.Rage, 600);
            if (evolutionStage >= 4)
            {
                Player.AddBuff(BuffID.Endurance, 600);
            }
            if (evolutionStage >= 6)
            {
                Player.AddBuff(BuffID.Lifeforce, 600);
                Player.AddBuff(BuffID.Ironskin, 600);
            }
        }
        private void SpawnEvolutionEffect()
        {
            Color color = StageColors[evolutionStage];
            for (int ring = 0; ring < 3; ring++)
            {
                for (int i = 0; i < 50; i++)
                {
                    float angle = MathHelper.TwoPi * i / 50f;
                    float speed = 10f + ring * 5f;
                    Vector2 velocity = angle.ToRotationVector2() * speed;
                    Dust d = Dust.NewDustDirect(Player.Center, 0, 0, DustID.RainbowTorch, velocity.X, velocity.Y);
                    d.noGravity = true;
                    d.scale = 2.5f - ring * 0.3f;
                    d.color = color;
                    d.fadeIn = 1.5f;
                }
            }
            for (int i = 0; i < 100; i++)
            {
                Vector2 pos = Player.Center + new Vector2(Main.rand.NextFloat(-40, 40), Main.rand.NextFloat(-300, 100));
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RainbowTorch);
                d.noGravity = true;
                d.scale = 3f;
                d.velocity = new Vector2(0, -8f);
                d.color = color;
            }
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float radius = i * 3f;
                float height = -i * 5f;
                Vector2 pos = Player.Center + new Vector2((float)Math.Cos(angle) * radius, height + (float)Math.Sin(angle) * radius * 0.3f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RainbowTorch);
                d.noGravity = true;
                d.scale = 2f;
                d.velocity = new Vector2(0, -3f);
                d.color = Color.Lerp(color, Color.White, 0.3f);
            }
            for (int i = 0; i < 30; i++)
            {
                Dust d = Dust.NewDustDirect(Player.Center, 0, 0, DustID.FireworkFountain_Yellow);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(15f, 15f);
                d.scale = 2f;
            }
            for (int i = 0; i < 5; i++)
            {
                Gore.NewGore(Player.GetSource_FromThis(), Player.Center, Main.rand.NextVector2Circular(5f, 5f), GoreID.Smoke1 + Main.rand.Next(3));
            }
        }
        private void SpawnKillEffect()
        {
            Color color = StageColors[evolutionStage];
            int dustCount = 5 + evolutionStage * 3;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Player.Center, 0, 0, DustID.RainbowTorch, vel.X, vel.Y);
                d.noGravity = true;
                d.scale = 1f + evolutionStage * 0.2f;
                d.color = color;
            }
        }
        public void ActivateShield()
        {
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 pos = Player.Center + angle.ToRotationVector2() * 50f;
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RedTorch);
                d.noGravity = true;
                d.velocity = angle.ToRotationVector2() * 3f;
                d.scale = 1.8f;
            }
            SoundEngine.PlaySound(SoundID.Item28, Player.Center);
        }
        public float GetEvolutionProgress()
        {
            if (evolutionStage >= 6) return 1f;
            return (float)killCount / KillsRequired[evolutionStage];
        }
        public override void SaveData(TagCompound tag)
        {
            tag["evolutionStage"] = evolutionStage;
            tag["killCount"] = killCount;
        }
        public override void LoadData(TagCompound tag)
        {
            evolutionStage = tag.GetInt("evolutionStage");
            killCount = tag.GetInt("killCount");
            evolutionStage = Math.Clamp(evolutionStage, 0, 6);
            killCount = Math.Max(0, killCount);
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Jarkos.JarkosPacketType.EvolutionBladeSync);
            packet.Write((byte)Player.whoAmI);
            packet.Write((byte)evolutionStage);
            packet.Write(killCount);
            packet.Send(toWho, fromWho);
        }
    }
    #region language system
    public class Metamorph_langsystem : ModSystem
	{
		public static LocalizedText primitive_stage { get; private set; }
		public static LocalizedText bronze_stage { get; private set; }
		public static LocalizedText iron_stage { get; private set; }
		public static LocalizedText steel_stage { get; private set; }
		public static LocalizedText mythril_stage { get; private set; }
		public static LocalizedText adamantite_stage { get; private set; }
		public static LocalizedText luminite_stage { get; private set; }
        public static LocalizedText primitive_stage_desc { get; private set; }
		public static LocalizedText bronze_stage_desc { get; private set; }
		public static LocalizedText iron_stage_desc { get; private set; }
		public static LocalizedText steel_stage_desc { get; private set; }
		public static LocalizedText mythril_stage_desc { get; private set; }
		public static LocalizedText adamantite_stage_desc { get; private set; }
		public static LocalizedText luminite_stage_desc { get; private set; }
        public static LocalizedText final_desc { get; private set; }
        public static LocalizedText stage_current_desc { get; private set; }
        public static LocalizedText progress_current_desc { get; private set; }
		public override void Load()
		{
			string category_1 = "MetamorphDesc";
			primitive_stage ??= Mod.GetLocalization($"{category_1}.primitive_stage");
            bronze_stage ??= Mod.GetLocalization($"{category_1}.bronze_stage");
            iron_stage ??= Mod.GetLocalization($"{category_1}.iron_stage");
            steel_stage ??= Mod.GetLocalization($"{category_1}.steel_stage");
            mythril_stage ??= Mod.GetLocalization($"{category_1}.mythril_stage");
            adamantite_stage ??= Mod.GetLocalization($"{category_1}.adamantite_stage");
            luminite_stage ??= Mod.GetLocalization($"{category_1}.luminite_stage");
            primitive_stage_desc ??= Mod.GetLocalization($"{category_1}.primitive_stage_desc");
            bronze_stage_desc ??= Mod.GetLocalization($"{category_1}.bronze_stage_desc");
            iron_stage_desc ??= Mod.GetLocalization($"{category_1}.iron_stage_desc");
            steel_stage_desc ??= Mod.GetLocalization($"{category_1}.steel_stage_desc");
            mythril_stage_desc ??= Mod.GetLocalization($"{category_1}.mythril_stage_desc");
            adamantite_stage_desc ??= Mod.GetLocalization($"{category_1}.adamantite_stage_desc");
            luminite_stage_desc ??= Mod.GetLocalization($"{category_1}.luminite_stage_desc");
            final_desc ??= Mod.GetLocalization($"{category_1}.final_desc");
            stage_current_desc ??= Mod.GetLocalization($"{category_1}.stage_current_desc");
            progress_current_desc ??= Mod.GetLocalization($"{category_1}.progress_current_desc");
		}
	}
}
#endregion
#endregion
