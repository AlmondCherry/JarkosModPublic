using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using jarkos.projectiles.melee.unique_melee;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using System;
using System.Linq;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class BladeOfWar : ModItem
	{
        public float DamageMultiplier => Item.GetGlobalItem<LegionBladeGlobalItem>().damageMultiplier;
        public float KnockbackMultiplier => Item.GetGlobalItem<LegionBladeGlobalItem>().knockbackMultiplier;
        public int CritBonus => Item.GetGlobalItem<LegionBladeGlobalItem>().critBonus;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.DamageType = DamageClass.Summon;
            Item.damage = 52;
            Item.knockBack = 6f;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.channel = false;
            Item.shoot = ModContent.ProjectileType<Blade_of_war_slashing>();
            Item.shootSpeed = 1f;
            Item.UseSound = new SoundStyle("Terraria/Sounds/Item_1")
            {
                Pitch = -0.2f, Volume = 0.8f
            };

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            var legionPlayer = player.GetModPlayer<LegionBladePlayer>();
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.noUseGraphic = true;
                return legionPlayer.SwordCount > 0;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.noUseGraphic = true;
                return true;
            }
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var legionPlayer = player.GetModPlayer<LegionBladePlayer>();
			if (player.altFunctionUse == 2)
			{
				CommandSwordsToAttack(player, legionPlayer);
				CreateCommandEffect(player);
				return false;
			}
			else
			{
				Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

				float rotation = direction.ToRotation();
				int swingDirection = (Main.MouseWorld.X >= player.Center.X) ? 1 : -1;
				int slashProj = Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<Blade_of_war_slashing>(), damage, knockback, player.whoAmI, rotation, swingDirection);
                
				if (Main.projectile[slashProj].active)
				{
					float swordBonus = 1f + (legionPlayer.SwordCount * 0.005f);
					Main.projectile[slashProj].damage = (int)(damage * swordBonus);
				}
				return false;
			}
		}
        private void CommandSwordsToAttack(Player player, LegionBladePlayer legionPlayer)
        {
            NPC target = FindTarget(player);
            if (target != null)
            {
                legionPlayer.CommandAttack(target);
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_103")
                {
                    Pitch = 0.5f, Volume = 1.2f
                }, player.Center);
            }
        }
        private NPC FindTarget(Player player)
        {
            float cursorRadius = 50f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    if (Vector2.Distance(npc.Center, Main.MouseWorld) < cursorRadius + npc.Size.Length() / 2)
                    {
                        return npc;
                    }
                }
            }
            NPC closest = null;
            float closestDist = 800f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(npc.Center, Main.MouseWorld);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        private void CreateCommandEffect(Player player)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(40, 40);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldCoin, (dustPos - player.Center).SafeNormalize(Vector2.Zero) * 3f);
                dust.noGravity = true;
                dust.scale = 1.5f;
            }
        }
        public override void HoldItem(Player player)
        {
            var legionPlayer = player.GetModPlayer<LegionBladePlayer>();
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                if (legionPlayer.FormationCooldown <= 0)
                {
                    legionPlayer.CycleFormation();
                    legionPlayer.FormationCooldown = 30;
                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f }, player.Center);
                }
            }
            legionPlayer.UpdateModifiers(DamageMultiplier, KnockbackMultiplier, CritBonus);
            Lighting.AddLight(player.Center, new Vector3(0.8f, 0.6f, 0.2f) * 0.5f);
            if (legionPlayer.SwordCount > 0)
            {
            }
            if (legionPlayer.SwordCount >= 100 && legionPlayer.UltimateCooldown <= 0)
            {
                if (Main.GameUpdateCount % 20 < 10)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(30, 30), DustID.GoldFlame);
                        dust.noGravity = true;
                        dust.velocity *= 0.3f;
                    }
                }
            }
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            var legionPlayer = player.GetModPlayer<LegionBladePlayer>();
            float swordBonus = legionPlayer.SwordCount * 0.003f;
            damage += swordBonus;
            damage *= legionPlayer.ComboMultiplier;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Excalibur, 1)
                .AddIngredient(ItemID.Ectoplasm, 12)
                .AddIngredient(ModContent.ItemType<FragmentOfNature>(), 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class LegionBladeGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public float damageMultiplier = 1f;
        public float knockbackMultiplier = 1f;
        public int critBonus = 0;
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (item.ModItem is BladeOfWar)
            {
                damageMultiplier = damage.ApplyTo(1f);
            }
        }
        public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            if (item.ModItem is BladeOfWar)
            {
                knockbackMultiplier = knockback.ApplyTo(1f);
            }
        }
        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            if (item.ModItem is BladeOfWar)
            {
                critBonus = (int)crit;
            }
        }
	}
    public enum Formation
    {
        Orbit, Circle, Spiral
    }
    public class LegionBladePlayer : ModPlayer
    {
        public int SwordCount { get; private set; } = 0;
        public int MaxSwords => 10;
        public Formation CurrentFormation { get; private set; } = Formation.Orbit;
        public int FormationCooldown = 0;
        public int ComboCount { get; private set; } = 0;
        public int ComboTimer = 0;
        private const int COMBO_DURATION = 180;
        public int UltimateCooldown = 0;
        private const int ULTIMATE_COOLDOWN_TIME = 600;
        public float DamageMultiplier { get; private set; } = 1f;
        public float KnockbackMultiplier { get; private set; } = 1f;
        public int CritBonus { get; private set; } = 0;
        public bool HasLegionBlade => Player.HeldItem?.ModItem is BladeOfWar;
        public bool IsCommanding { get; private set; } = false;
        public List<int> activeSwordProjectiles = new List<int>();
        public float ComboMultiplier
        {
            get
            {
                if (ComboCount <= 0) return 1f;
                return 1f + Math.Min(ComboCount * 0.01f, 0.5f);
            }
        }
        public override void ResetEffects()
        {
            if (FormationCooldown > 0) FormationCooldown--;
            if (UltimateCooldown > 0) UltimateCooldown--;
            if (ComboTimer > 0)
            {
                ComboTimer--;
            }
            else if (ComboCount > 0)
            {
                ComboCount = 0;
            }
            activeSwordProjectiles.RemoveAll(i =>
                i < 0 || i >= Main.maxProjectiles || !Main.projectile[i].active || Main.projectile[i].type != ModContent.ProjectileType<Blade_of_war_proj>());
			if (activeSwordProjectiles.Count > MaxSwords)
			{
				activeSwordProjectiles.RemoveRange(10, activeSwordProjectiles.Count - 10);
			}
            SwordCount = activeSwordProjectiles.Count;
        }
        public override void PostUpdate()
        {
            if (HasLegionBlade)
            {
                UpdateSwordFormations();
            }
            CheckUltimate();
        }
        public void UpdateModifiers(float damage, float knockback, int crit)
        {
            DamageMultiplier = damage;
            KnockbackMultiplier = knockback;
            CritBonus = crit;
        }
        #region Sword Management
        public void TryAddSword(Player player, Vector2 spawnPosition)
        {
            if (SwordCount >= MaxSwords) return;
            int projIndex = Projectile.NewProjectile(player.GetSource_FromThis(), spawnPosition, Vector2.Zero, ModContent.ProjectileType<Blade_of_war_proj>(), GetSwordDamage(), GetSwordKnockback(), player.whoAmI, 0, 0);
            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
            {
                var proj = Main.projectile[projIndex];
                if (proj.ModProjectile is Blade_of_war_proj sword)
                {
                    proj.localAI[0] = activeSwordProjectiles.Count;
                    activeSwordProjectiles.Add(projIndex);
                    CreateSwordSpawnEffect(spawnPosition);
                }
            }
        }
        private int GetSwordDamage()
        {
            int baseDamage = 45;
            return (int)(baseDamage * DamageMultiplier * ComboMultiplier);
        }
        private float GetSwordKnockback()
        {
            return 4f * KnockbackMultiplier;
        }
        private void CreateSwordSpawnEffect(Vector2 position)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustPerfect(position, DustID.GoldFlame, Main.rand.NextVector2Circular(5f, 5f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.8f);
            }
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f }, position);
        }
        public void RemoveSword()
        {
            if (activeSwordProjectiles.Count > 0)
            {
                int lastIndex = activeSwordProjectiles.Count - 1;
                int projIndex = activeSwordProjectiles[lastIndex];
                if (projIndex >= 0 && projIndex < Main.maxProjectiles && Main.projectile[projIndex].active)
                {
                    Main.projectile[projIndex].Kill();
                }
                activeSwordProjectiles.RemoveAt(lastIndex);
            }
		}
        #endregion
        #region Commands
        public void CommandAttack(NPC target)
        {
            IsCommanding = true;
            foreach (int projIndex in activeSwordProjectiles)
            {
                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                {
                    var proj = Main.projectile[projIndex];
                    if (proj.active && proj.ModProjectile is Blade_of_war_proj sword)
                    {
                        sword.SetAttackTarget(target.whoAmI);
                    }
                }
            }
            IsCommanding = false;
        }
        public void CycleFormation()
        {
            CurrentFormation = CurrentFormation switch
            {
                Formation.Orbit => Formation.Circle, Formation.Circle => Formation.Spiral, Formation.Spiral => Formation.Orbit, _ => Formation.Orbit
            };
            UpdateSwordFormations();
            CreateFormationChangeEffect();
        }
        private void UpdateSwordFormations()
        {
            int index = 0;
            foreach (int projIndex in activeSwordProjectiles)
            {
                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                {
                    var proj = Main.projectile[projIndex];
                    if (proj.active && proj.ModProjectile is Blade_of_war_proj sword)
                    {
                        proj.localAI[0] = index;
                        sword.SetFormationMode(CurrentFormation);
                        index++;
                    }
                }
            }
        }
        private void CreateFormationChangeEffect()
        {
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30;
                Vector2 pos = Player.Center + angle.ToRotationVector2() * 80f;
                Dust dust = Dust.NewDustPerfect(pos, DustID.GoldCoin, angle.ToRotationVector2() * 2f);
                dust.noGravity = true;
            }
        }
        #endregion
        #region Combo System
        public void AddCombo()
        {
            ComboCount++;
            ComboTimer = COMBO_DURATION;
            if (ComboCount == 10 || ComboCount == 25 || ComboCount == 50)
            {
                CreateComboEffect();
            }
        }
        private void CreateComboEffect()
        {
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(30, 30), DustID.GoldFlame, Main.rand.NextVector2Circular(3f, 3f));
                dust.noGravity = true;
                dust.scale = 1.5f;
            }
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.5f + ComboCount * 0.01f }, Player.Center);
        }
        #endregion
        #region Ultimate
        private int doubleClickTimer = 0;
        private int clickCount = 0;
        private void CheckUltimate()
        {
            if (!HasLegionBlade || SwordCount < 100 || UltimateCooldown > 0) return;
            if (Main.mouseRight && Main.mouseRightRelease)
            {
                if (doubleClickTimer > 0)
                {
                    clickCount++;
                    if (clickCount >= 2)
                    {
                        ActivateUltimate();
                        clickCount = 0;
                        doubleClickTimer = 0;
                    }
                }
                else
                {
                    clickCount = 1;
                    doubleClickTimer = 15;
                }
            }
            if (doubleClickTimer > 0)
            {
                doubleClickTimer--;
                if (doubleClickTimer == 0)
                {
                    clickCount = 0;
                }
            }
        }
        private void ActivateUltimate()
        {
            UltimateCooldown = ULTIMATE_COOLDOWN_TIME;
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.3f, Volume = 1.5f }, Player.Center);
            CreateUltimateEffect();
            List<NPC> targets = new List<NPC>();
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    if (Vector2.Distance(npc.Center, Player.Center) < 1000f)
                    {
                        targets.Add(npc);
                    }
                }
            }
            if (targets.Count == 0)
            {
                UltimateDirectionalAttack();
            }
            else
            {
                foreach (int projIndex in activeSwordProjectiles)
                {
                    if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                    {
                        var proj = Main.projectile[projIndex];
                        if (proj.active && proj.ModProjectile is Blade_of_war_proj sword)
                        {
                            NPC target = targets.OrderByDescending(n => n.boss ? 1000 : 0)
                                               .ThenBy(n => Main.rand.Next())
                                               .First();
                            proj.damage = (int)(proj.damage * 2.5f);
                            sword.SetAttackTarget(target.whoAmI);
                        }
                    }
                }
            }
        }
        private void UltimateDirectionalAttack()
        {
            Vector2 direction = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX);
            foreach (int projIndex in activeSwordProjectiles)
            {
                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                {
                    var proj = Main.projectile[projIndex];
                    if (proj.active)
                    {
                        proj.velocity = direction.RotatedByRandom(0.3f) * 30f;
                        proj.damage = (int)(proj.damage * 2f);
                    }
                }
            }
        }
        private void CreateUltimateEffect()
        {
            for (int i = 0; i < 100; i++)
            {
                float angle = MathHelper.TwoPi * i / 100;
                Vector2 vel = angle.ToRotationVector2() * 8f;
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.GoldFlame, vel);
                dust.noGravity = true;
                dust.scale = 2f;
            }
            for (int i = 0; i < 50; i++)
            {
                Dust flash = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(100, 100), DustID.GoldCoin, Main.rand.NextVector2Circular(5f, 5f));
                flash.noGravity = true;
                flash.scale = 2.5f;
            }
        }
        #endregion
        #region Save/Load
        public override void SaveData(TagCompound tag)
        {
            tag["SwordCount"] = SwordCount;
        }
        public override void LoadData(TagCompound tag)
        {
            int savedCount = tag.GetInt("SwordCount");
        }
        public override void OnEnterWorld()
        {
            activeSwordProjectiles.Clear();
            SwordCount = 0;
            ComboCount = 0;
            ComboTimer = 0;
        }
        #endregion
    }
}
