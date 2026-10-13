using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using jarkos.projectiles.bullet;
using jarkos.particles;
using ReLogic.Content;
using jarkos.buffs.debuff;
using jarkos.buffs.buff;
using jarkos.items.accs.general;
using jarkos.achievements;
using jarkos.npc.Punisher;
using jarkos.systems;
using Terraria.Audio;

namespace jarkos
{
    public class Jarkos_player : ModPlayer
    {
        #region ArmorSet Effects
        public bool anticvarSet = false;
        public bool liquid_plasma_set = false;
        public bool diffusalSet = false;
        public int diffusalWheelCooldown = 0;
        public int diffusalBlueStacks = 0;
        public int diffusalOrangeStacks = 0;
        public int lastDiffusalOutcome = 0;
        public int diffusalRepeatTimer = 0;
        public int diffusalSilhouetteTimer = 0;
        public int diffusalSilhouetteType = 0;
        #endregion

        #region Summon Accessory Effects
        public bool Pergament_Of_Evil_God = false;
        public bool nature_dar = false;
        public bool RingOfDysterian = false;
        public bool AviatorsOfDysterian = false;
        public bool Radiant_Generator = false;
        public bool Evil_Gods_Parchment_equip = false;
        #endregion
        #region Emblem Accessory Effects
        public bool lotos_equiped = false;
        #endregion
        #region General Effects
        public bool core_of_trouble = false;
        public bool seal_of_shadow = false;
        public bool bleeding_moon_amulet = false;
        public bool burning_mark = false;
        public bool biomaterialConverter = false;
        public int biomaterialConverterTimer = 0;
        public bool Efflux_Crystal_eqipped = false;
        public bool clockwithtimer = false;
        #endregion
        #region Special Effects
        public bool charger_health = false;
        public bool lotos_active = false;
        #endregion
        #region Melee Effects
        public bool bloodyamulet = false;
        public bool goldengorn = false;
        public float goldengornstrength = 1f;
        public bool armorplate = false;
        public float global_timer = 0f;
        public bool fragment_weakness = false;
        public bool rage_eye = false;
        #endregion
        #region Melee Weapons Effects
        public float thermal_charge = 0;
        public bool thermal_scythe_held = false;
        public int thermal_attack = 0;
        public float timer02 = 0;
        public int thermalScytheCooldown = 0;
        public bool thermalScytheDashing = false;
        public Vector2 thermalScytheStartPos;
        public Vector2 thermalScytheTargetPos;
        public int thermalScytheDashTimer = 0;
        public int thermalScytheDashDuration = 14;
        public NPC thermalScytheTargetNPC = null;
        public int thermalScytheDamage = 0;
        public List<Vector2> thermalDashTrailPoints = new List<Vector2>();
        public float thermalDashTrailAlpha = 0f;
        #endregion
        #region Mecha Weapons Effects
        public bool nova_held = false;
        public float nova_charge = 0;
        public float nova_charge_max = 40;
        #endregion
        #region Ranged Effects
        public bool rocketmk1 = false;
        public bool rocketmk2 = false;
        public bool rocketmk2_active = false;
        public bool rifle_scope02 = false;
        public bool rifle_scope03 = false;
        #endregion
        #region Magic Effects
        public bool amulet_of_star_power = false;
        public bool sofs01equiped = false;
        public bool magegauntlet01 = false;
        public bool taranisring = false;
        public bool taranis_boots = false;
        public bool taranis_wings = false;
        public bool taranisringattack = false;
        public bool gemstaffs_buff = false;
        public bool amethyst_shard = false;
        public bool topaz_shard = false;
        public bool emerald_shard = false;
        public bool sapphire_shard = false;
        public bool ruby_shard = false;
        public bool diamond_shard = false;
        public bool celestial_flask = false;
        public bool eom_equiped = false;
        public bool seal_taranis_equiped = false;
        #endregion
        #region Runes Buffs Minimum Effects
        public bool filth_min = false;
        public bool steampunk_min = false;
        public bool elder_min = false;
        public bool flame_min = false;
        public float filth_tp_repeats = 0f;
        public float filth_tp_max = 10f;
        public bool all_runes = false;
        public bool TaranisAccessoryEquipped;
        public int DashTimer = 0;
        #endregion
        #region Potion Effects
        public float random_endurance = 1f;
        public float random_defense = 1f;
        public float random_attack_speed = 0f;
        public float random_damage = 0f;
        #endregion
        #region Other
        public float blood_charge_percent = 0f;
        #endregion
        #region BloodMage
        public bool parazm = false;
        public bool parazm_attack = false;
        public bool blood_tank_equiped = false;
        public bool blood_tank_in_inventory = false;
        #endregion
        #region Debuffs
        public int timerin_radioactive = 0;
        #endregion
        #region Gem Discovery
        public bool discoveredAmethyst = false;
        public bool discoveredTopaz = false;
        public bool discoveredSapphire = false;
        public bool discoveredEmerald = false;
        public bool discoveredRuby = false;
        public bool discoveredDiamond = false;
        #endregion
        #region Achievements
        public int timer_maxcostenergy = 300;
        #endregion
        public override void OnEnterWorld()
        {
            Main.NewText(LangSystem.enterworld_message, Color.Cyan);
        }
        #region Initialize Method
        public override void Initialize()
        {
            goldengornstrength = 1f;
            global_timer = 0f;
            random_endurance = 1f;
            random_defense = 1f;
            random_attack_speed = 0f;
            random_damage = 0f;
            timerin_radioactive = 0;
            timer_maxcostenergy = 300;
            biomaterialConverterTimer = 0;
            if (Main.playerInventory)
            {
            }
        }
        #endregion
        #region SaveData Method
        public override void SaveData(TagCompound tag)
        {
            tag.Add("goldengornstrength", goldengornstrength);
            tag.Add("global_timer", global_timer);
            tag.Add("timerin_radioactive", timerin_radioactive);
            tag.Add("filth_min", filth_min);
            tag.Add("steampunk_min", steampunk_min);
            tag.Add("flame_min", flame_min);
            tag.Add("elder_min", elder_min);
            tag.Add("discoveredAmethyst", discoveredAmethyst);
            tag.Add("discoveredTopaz", discoveredTopaz);
            tag.Add("discoveredSapphire", discoveredSapphire);
            tag.Add("discoveredEmerald", discoveredEmerald);
            tag.Add("discoveredRuby", discoveredRuby);
            tag.Add("discoveredDiamond", discoveredDiamond);
        }
        #endregion
        #region LoadData Method
        public override void LoadData(TagCompound tag)
        {
            goldengornstrength = tag.GetFloat("goldengornstrength");
            global_timer = tag.GetFloat("global_timer");
            timerin_radioactive = tag.GetInt("timerin_radioactive");
            filth_min = tag.GetBool("filth_min");
            steampunk_min = tag.GetBool("steampunk_min");
            flame_min = tag.GetBool("flame_min");
            elder_min = tag.GetBool("elder_min");
            discoveredAmethyst = tag.GetBool("discoveredAmethyst");
            discoveredTopaz = tag.GetBool("discoveredTopaz");
            discoveredSapphire = tag.GetBool("discoveredSapphire");
            discoveredEmerald = tag.GetBool("discoveredEmerald");
            discoveredRuby = tag.GetBool("discoveredRuby");
            discoveredDiamond = tag.GetBool("discoveredDiamond");
        }
        #endregion

        public override bool OnPickup(Item item)
        {
            if (item.type == ItemID.Amethyst) discoveredAmethyst = true;
            else if (item.type == ItemID.Topaz) discoveredTopaz = true;
            else if (item.type == ItemID.Sapphire) discoveredSapphire = true;
            else if (item.type == ItemID.Emerald) discoveredEmerald = true;
            else if (item.type == ItemID.Ruby) discoveredRuby = true;
            else if (item.type == ItemID.Diamond) discoveredDiamond = true;
            return base.OnPickup(item);
        }

        private void CheckGemDiscovery()
        {
            if (!discoveredAmethyst && HasGemInInventoryOrBanks(ItemID.Amethyst)) discoveredAmethyst = true;
            if (!discoveredTopaz && HasGemInInventoryOrBanks(ItemID.Topaz)) discoveredTopaz = true;
            if (!discoveredSapphire && HasGemInInventoryOrBanks(ItemID.Sapphire)) discoveredSapphire = true;
            if (!discoveredEmerald && HasGemInInventoryOrBanks(ItemID.Emerald)) discoveredEmerald = true;
            if (!discoveredRuby && HasGemInInventoryOrBanks(ItemID.Ruby)) discoveredRuby = true;
            if (!discoveredDiamond && HasGemInInventoryOrBanks(ItemID.Diamond)) discoveredDiamond = true;
        }

        private bool HasGemInInventoryOrBanks(int itemType)
        {
            if (Player.HasItem(itemType)) return true;
            if (Player.bank?.item != null) { foreach (var i in Player.bank.item) if (i != null && i.type == itemType) return true; }
            if (Player.bank2?.item != null) { foreach (var i in Player.bank2.item) if (i != null && i.type == itemType) return true; }
            if (Player.bank3?.item != null) { foreach (var i in Player.bank3.item) if (i != null && i.type == itemType) return true; }
            if (Player.bank4?.item != null) { foreach (var i in Player.bank4.item) if (i != null && i.type == itemType) return true; }
            return false;
        }
        public override void PreUpdate()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && Main.projHook[p.type])
                {
                    if (p.Center.Y > Player.Center.Y + 8f)
                    {
                        Player.GoingDownWithGrapple = true;
                        Player.releaseDown = true;
                        if (Player.velocity.Y >= 0f && Player.oldPosition.Y == Player.position.Y)
                        {
                            Player.position.Y += 2f;
                        }
                    }
                }
            }
        }
        #region ResetEffects Method
        public override void ResetEffects()
        {
            if (diffusalSilhouetteTimer > 0)
            {
                diffusalSilhouetteTimer--;
                if (diffusalSilhouetteTimer <= 0)
                {
                    diffusalSilhouetteType = 0;
                }
            }
            if(taranis_wings && taranis_boots && taranisringattack)
            {
				ModContent.GetInstance<Taranis_kit>().Condition_wings.Complete();
            }
                Pergament_Of_Evil_God = false;
                nature_dar = false;
                RingOfDysterian = false;
                AviatorsOfDysterian = false;
                Radiant_Generator = false;
                Evil_Gods_Parchment_equip = false;
                burning_mark = false;
                biomaterialConverter = false;
                core_of_trouble = false;
                seal_of_shadow = false;
                bleeding_moon_amulet = false;
                Efflux_Crystal_eqipped = false;
                clockwithtimer = false;
                bloodyamulet = false;
                goldengorn = false;
                armorplate = false;
                fragment_weakness = false;
                rage_eye = false;
                global_timer++;
                if (goldengornstrength > 15)
                {
                    goldengornstrength = 1;
                }
                if (global_timer == 30)
                {
                    global_timer = 0;
                }
                thermal_scythe_held = false;
                if (thermalScytheCooldown > 0)
                {
                    thermalScytheCooldown--;
                }

                if (thermalScytheDashing)
                {
                    thermalScytheDashTimer++;
                    float t = (float)thermalScytheDashTimer / thermalScytheDashDuration;
                    if (t >= 1f)
                    {
                        t = 1f;
                        thermalScytheDashing = false;
                    }

                    float easedT = 1f - MathF.Pow(1f - t, 3f);
                    Player.Center = Vector2.Lerp(thermalScytheStartPos, thermalScytheTargetPos, easedT);
                    Player.velocity = Vector2.Zero;
                    Player.immune = true;
                    Player.immuneTime = 2;
                    Player.noFallDmg = true;

                    thermalDashTrailPoints.Clear();
                    int numTrailPts = Math.Max(4, (int)(24 * easedT));
                    for (int i = 0; i < numTrailPts; i++)
                    {
                        float frac = i / (float)(numTrailPts - 1);
                        thermalDashTrailPoints.Add(Vector2.Lerp(thermalScytheStartPos, Player.Center, frac));
                    }
                    thermalDashTrailAlpha = 1f;

                    for (int i = 0; i < 3; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(16, 16), DustID.Firework_Red, Vector2.Zero);
                        dust.noGravity = true;
                        dust.scale = 1.2f;
                    }

                    if (!thermalScytheDashing)
                    {
                        if (thermalScytheTargetNPC != null && thermalScytheTargetNPC.active && !thermalScytheTargetNPC.dontTakeDamage)
                        {
                            Player.ApplyDamageToNPC(thermalScytheTargetNPC, thermalScytheDamage, 6f, Player.direction, Main.rand.NextBool((int)Player.GetTotalCritChance(DamageClass.Melee)));

                            for (int i = 0; i < 25; i++)
                            {
                                Vector2 speed = Main.rand.NextVector2CircularEdge(5f, 5f);
                                Dust d = Dust.NewDustPerfect(thermalScytheTargetNPC.Center, DustID.Firework_Red, speed);
                                d.noGravity = true;
                                d.scale = 1.4f;
                            }
                            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ThermalScytheHit") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f)}, Player.position);
                        }
                        else
                        {
                            SoundEngine.PlaySound(SoundID.Item71, Player.Center);
                        }
                    }
                }
                else
                {
                    if (thermalDashTrailAlpha > 0f)
                    {
                        thermalDashTrailAlpha -= 1f / 20f;
                        if (thermalDashTrailAlpha <= 0f)
                        {
                            thermalDashTrailAlpha = 0f;
                            thermalDashTrailPoints.Clear();
                        }
                    }
                }
                nova_held = false;
                rocketmk1 = false;
                rocketmk2 = false;
                rifle_scope02 = false;
                rifle_scope03 = false;
                amulet_of_star_power = false;
                sofs01equiped = false;
                taranisringattack = false;
                magegauntlet01 = false;
                taranisring = false;
                taranis_boots = false;
                taranis_wings = false;
                TaranisAccessoryEquipped = false;
                gemstaffs_buff = false;
                amethyst_shard = false;
                topaz_shard = false;
                emerald_shard = false;
                sapphire_shard = false;
                ruby_shard = false;
                diamond_shard = false;
                celestial_flask = false;
                eom_equiped = false;
                seal_taranis_equiped = false;
                lotos_equiped = false;
                anticvarSet = false;
                liquid_plasma_set = false;
                diffusalSet = false;
            if (!Player.HasBuff(ModContent.BuffType<DiffusalAttackSpeedBuff>()))
            {
                diffusalBlueStacks = 0;
            }
            if (!Player.HasBuff(ModContent.BuffType<DiffusalDefenseBuff>()))
            {
                diffusalOrangeStacks = 0;
            }
            if (!Player.HasBuff(ModContent.BuffType<DiffusalAttackSpeedBuff>()) && !Player.HasBuff(ModContent.BuffType<DiffusalDefenseBuff>()))
            {
                lastDiffusalOutcome = 0;
            }
            if (diffusalRepeatTimer > 0)
            {
                diffusalRepeatTimer--;
            }
            if (diffusalSet)
            {
                Player.GetDamage(DamageClass.Generic) += 0.05f;
                if (diffusalRepeatTimer > 0)
                {
                    Player.GetDamage(DamageClass.Generic) += 0.05f;
                }
            }
                parazm = false;
                parazm_attack = false;
                blood_tank_equiped = false;
                blood_tank_in_inventory = false;
        }
        #endregion
        #region OnHitByNPC
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (celestial_flask)
            {
                Player.statMana += hurtInfo.Damage / 2;
                CombatText.NewText(Player.Hitbox, Colors.RarityBlue, hurtInfo.Damage / 2, false, false);
            }
            if (rage_eye)
            {
                Player.AddBuff(ModContent.BuffType<Rage_eye_buff>(), 600);
                Player.Heal(hurtInfo.Damage * 10 / 100);
            }
        }
        #endregion
        #region OnHitNPC(Item)
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
        }
        #endregion
        #region OnHitNPC(Projectile)
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (bleeding_moon_amulet && Main.rand.NextBool(4) && proj.type != ModContent.ProjectileType<Burst_hitbox>())
            {
                new Circleedge(target.Center, Vector2.Zero, Color.Red, 120, 0.35f, 2f, 0f).Spawn();
                Projectile.NewProjectile(null, target.Center, Vector2.Zero, ModContent.ProjectileType<Burst_hitbox>(), damageDone * 25 / 100, 1f);
                Player.Heal(1);
            }
            if (fragment_weakness)
            {
                target.AddBuff(ModContent.BuffType<Fantom_debuff>(), 300);
            }
            if (taranisringattack == true && proj.DamageType == DamageClass.Magic)
            {
                target.AddBuff(ModContent.BuffType<Taranis_buff>(), 300);
            }
            if (burning_mark)
            {
                target.AddBuff(ModContent.BuffType<Burning_mark_debuff>(), 300);
            }
            if (RingOfDysterian && proj.DamageType == DamageClass.Summon || proj.DamageType == DamageClass.SummonMeleeSpeed)
            {
                target.AddBuff(BuffID.Poisoned, 60);
            }
            const int RoD_cd_max = 30 * 60;
            if (RingOfDysterian && !Player.HasBuff(ModContent.BuffType<Ring_of_Dysterian_CD>()) && proj.DamageType == DamageClass.SummonMeleeSpeed
            || RingOfDysterian && !Player.HasBuff(ModContent.BuffType<Ring_of_Dysterian_CD>()) && proj.DamageType == DamageClass.Summon)
            {
                Player.AddBuff(ModContent.BuffType<Ring_of_Dysterian_CD>(), RoD_cd_max);
                Player.Heal(Player.statLifeMax2 * 5 / 100);
                ModContent.GetInstance<Dysterian_ring_max>().Condition.Value += Player.statLifeMax2 * 5 / 100;
            }
            if (amulet_of_star_power && Player.statMana != Player.statManaMax2)
            {
                CombatText.NewText(Player.Hitbox, Colors.RarityBlue, 1);
                Player.statMana += 1;
            }
            if (nature_dar && (ProjectileID.Sets.IsAWhip[proj.type] || proj.DamageType == DamageClass.SummonMeleeSpeed))
            {
                target.AddBuff(BuffID.Ichor, 1200);
                Player.AddBuff(ModContent.BuffType<Nature_Gods_Bless>(), 180);
            }
            if (Radiant_Generator && proj.DamageType == DamageClass.Summon || Radiant_Generator && proj.DamageType == DamageClass.SummonMeleeSpeed)
            {
                target.AddBuff(BuffID.Bleeding, 600);
                target.AddBuff(BuffID.Electrified, 300);
            }
            if (eom_equiped && proj.DamageType == DamageClass.Magic)
            {
                target.AddBuff(ModContent.BuffType<Vulnerability_debuff>(), 3000);
            }
            if (parazm_attack && parazm && !target.boss)
            {
                target.AddBuff(ModContent.BuffType<Beer_buff>(), 900);
            }
        }
        #endregion
        #region ModifyWeaponCrit
        public override void ModifyWeaponCrit(Item item, ref float crit)
        {
            base.ModifyWeaponCrit(item, ref crit);
        }
        #endregion
        #region ModifyHitNPC
        public int RoD_heal_cooldown = 0;
        public bool hitted_with_RoD = false;
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            ShadowCursorDebuff(target);
        }
        #endregion
        #region ModifyHitNPC(Proj)
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            ShadowCursorDebuff(target);
        }
        private void ShadowCursorDebuff(NPC target)
        {
            if (seal_of_shadow && !target.friendly && target.active && !target.boss)
            {
                Vector2 vector_to_target = target.Center - Main.MouseWorld;
                float distance_to_cursor = vector_to_target.Length();
                if (distance_to_cursor < 100f)
                {
                    target.AddBuff(ModContent.BuffType<Seal_of_shadow_debuff>(), 60);
                }
            }
        }
        #endregion
        public override void UpdateDead()
        {
            ResetEffects();
        }
        #region DrawEffects(Player)
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (DashTimer > 0)
            {
                Color color21 = Color.Lerp(Color.Red, Color.White, 0.7f);
                Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/shield_field", AssetRequestMode.ImmediateLoad).Value;
                var origin = texture.Size() / 2f;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(texture, drawInfo.Position, null, color21, 0f, origin, 1f, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            }
        }
        #endregion
        public override void ModifyLuck(ref float luck)
        {
        }
        public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price)
        {
        }
        #region ModifyWeaponDamage
        public override void ModifyWeaponDamage(Item item, ref StatModifier statmodifier)
        {
            if (Player.name == "smelecov")
            {
                item.damage *= 10;
            }
        }
        #endregion
        #region Kill(Player)
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
        }
        #endregion
        #region OnHurt(Player)
        public override void OnHurt(Player.HurtInfo info)
        {
            if (seal_taranis_equiped)
            {
                if (Player.statMana > 0)
                {
                    if (info.Damage < Player.statMana)
                    {
                        Player.statMana -= info.Damage;
                    }
                    else if (info.Damage > Player.statMana)
                    {
                        Player.statMana = 0;
                    }
                }
                else if (Player.statMana < 0)
                {
                    Player.statMana = 0;
                }
            }
            if(burning_mark)
            {
                for (int i = 0; i < 10; i++) {
                Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.Blood, speed * 4.5f, Scale: 1.5f);
                d.noGravity = true;
        	    }
            }
            if(ModContent.GetInstance<Jarkos_config>().small_gameplay_changes)
            {
                Player.AddBuff(ModContent.BuffType<Player_hurt_debuff>(), 600);
            }
        }
        #endregion
        #region PreKill(Player)
        public bool clockwithtimer_used = false;
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if(clockwithtimer && Player.statLife < 10)
            {
                clockwithtimer_used = true;
                ModContent.GetInstance<Clockused>().Condition.Complete();
                if(seal_taranis_equiped)
                {
                    ModContent.GetInstance<Useless_seal_taranis>().Condition.Complete();
                }
                int heal = Player.statLifeMax2 * 20 / 100;
                Player.statLife = heal;
                Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ModContent.ProjectileType<Clockwithtimer_proj>(), 0, 0f);
                CombatText.NewText(Player.Hitbox, Colors.RarityLime, heal, false, true);
                return false;
            }
            if(seal_taranis_equiped && Player.statMana > 0)
            {
                return false;
            }
            return true;
        }
        #endregion
        #region PostHurt(Player)
        public override void PostHurt(Player.HurtInfo info)
        {
        }
        #endregion
        private readonly int accessoryToRemove = ModContent.ItemType<ClockWithTimer>();
        public override void PostUpdate()
        {
            CheckGemDiscovery();
            for (int i = 3; i < 10; i++)
            {
                if (!Player.armor[i].IsAir && Player.armor[i].type == accessoryToRemove && clockwithtimer_used)
                {
                    Player.armor[i].TurnToAir();
                    clockwithtimer_used = false;
                }
            }
            if (NPC.AnyNPCs(ModContent.NPCType<Punisher>()))
            {
                Player.AddBuff(ModContent.BuffType<Infinite_wingtime>(), 360);
            }
            if (biomaterialConverter)
            {
                biomaterialConverterTimer++;
                bool bossAlive = false;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && (Main.npc[i].boss || Main.npc[i].type == NPCID.EaterofWorldsHead || Main.npc[i].type == NPCID.EaterofWorldsBody || Main.npc[i].type == NPCID.EaterofWorldsTail))
                    {
                        bossAlive = true;
                        break;
                    }
                }
                int interval = bossAlive ? 15 * 60 : 5 * 60;
                if (biomaterialConverterTimer >= interval)
                {
                    biomaterialConverterTimer = 0;
                    if (Player.whoAmI == Main.myPlayer)
                    {
                        int healAmount = (int)(Player.statLifeMax2 * 0.01f);
                        if (healAmount < 1) healAmount = 1;
                        Player.Heal(healAmount);
                    }
                }
            }
            else
            {
                biomaterialConverterTimer = 0;
            }
            if (diffusalWheelCooldown > 0)
            {
                diffusalWheelCooldown--;
            }
            if (diffusalSilhouetteTimer > 0)
            {
                if (Main.rand.NextBool(4))
                {
                    Color color = diffusalSilhouetteType == 1 ? Color.SkyBlue : Color.Orange;
                    Vector2 spawnPos = Player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 vel = -Player.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
                    new Spark(spawnPos, vel, color, Main.rand.Next(15, 25), Main.rand.NextFloat(0.6f, 1.0f), 0.8f).Spawn();
                }
                if (Main.rand.NextBool(12))
                {
                    Color color = diffusalSilhouetteType == 1 ? Color.SkyBlue : Color.Orange;
                    Vector2 spawnPos = Player.Center - Player.velocity * 2f + Main.rand.NextVector2Circular(30f, 30f);
                    float dist = Vector2.Distance(spawnPos, Player.Center);
                    new Stretching_particle(spawnPos, Player.Center, color * 0.7f, Main.rand.Next(20, 30), Math.Max(40f, dist * 1.5f), Main.rand.NextFloat(6f, 10f), 0.02f, Player).Spawn();
                }
            }
        }
    }
    public class DiffusalTrailBehindLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<Jarkos_player>();
            return modPlayer.diffusalSilhouetteTimer > 0 && drawInfo.DrawDataCache.Count > 0;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<Jarkos_player>();
            int dyeItem = modPlayer.diffusalSilhouetteType == 1 ? ItemID.IntenseBlueFlameDye : ItemID.IntenseFlameDye;
            int shaderId = Terraria.Graphics.Shaders.GameShaders.Armor.GetShaderIdFromItemId(dyeItem);
            Color trailColor = modPlayer.diffusalSilhouetteType == 1 ? Color.SkyBlue : Color.Orange;
            Vector2 playerCenter = drawInfo.drawPlayer.Center - Main.screenPosition;
            float globalTime = (float)Main.GlobalTimeWrappedHourly;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int ring = 0; ring < 3; ring++)
            {
                float scale = 1f;
                float baseDistance = 0f;
                float opacity = 0f;
                float rotationAngle = 0f;
                if (ring == 0)
                {
                    scale = 1.03f;
                    baseDistance = 2.5f + (float)Math.Sin(globalTime * 7f) * 0.5f;
                    opacity = 0.45f;
                    rotationAngle = globalTime * 3.5f;
                }
                else if (ring == 1)
                {
                    scale = 1.06f;
                    baseDistance = 5f + (float)Math.Cos(globalTime * 5f) * 1f;
                    opacity = 0.25f;
                    rotationAngle = -globalTime * 2.5f;
                }
                else if (ring == 2)
                {
                    scale = 1.09f;
                    baseDistance = 8f + (float)Math.Sin(globalTime * 3f) * 1.5f;
                    opacity = 0.12f;
                    rotationAngle = globalTime * 1.5f;
                }
                for (int k = 0; k < 4; k++)
                {
                    float angle = rotationAngle + (k * MathHelper.TwoPi / 4f);
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * baseDistance;
                    foreach (var data in drawInfo.DrawDataCache)
                    {
                        DrawData clone = data;
                        Vector2 offsetFromCenter = clone.position - playerCenter;
                        clone.position = playerCenter + offsetFromCenter * scale + offset;
                        clone.scale *= scale;
                        clone.color = trailColor * opacity;
                        clone.shader = shaderId;
                        clone.Draw(Main.spriteBatch);
                    }
                }
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
