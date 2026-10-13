using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using jarkos.core.Data;
using System;

namespace jarkos.items
{
    public class EnchantedArmorPlayer : ModPlayer
    {
        public Dictionary<EssenceType, float> EffectiveEssences = new Dictionary<EssenceType, float>();

        public override void ResetEffects()
        {
            EffectiveEssences.Clear();
        }

        public override void UpdateEquips()
        {
            Dictionary<EssenceType, int> rawEssences = new Dictionary<EssenceType, int>();
            Item[] armorSlots = { Player.armor[0], Player.armor[1], Player.armor[2] };
            foreach (Item item in armorSlots)
            {
                if (item != null && !item.IsAir && item.TryGetGlobalItem(out EnchantedArmorGlobalItem gItem) && gItem.IsEnchantedByRune)
                {
                    foreach (var kvp in gItem.Essences)
                    {
                        if (!rawEssences.ContainsKey(kvp.Key)) rawEssences[kvp.Key] = 0;
                        rawEssences[kvp.Key] += kvp.Value;
                    }
                }
            }

            if (rawEssences.Count == 0) return;

            int ordoAmount = rawEssences.ContainsKey(EssenceType.Ordo) ? rawEssences[EssenceType.Ordo] : 0;
            float essenceMultiplier = Jarkos_config.Instance?.EssenceEffectMultiplier ?? 1.0f;
            foreach (EssenceType type in Enum.GetValues(typeof(EssenceType)))
            {
                int raw = rawEssences.ContainsKey(type) ? rawEssences[type] : 0;
                float total = raw;
                if (type != EssenceType.Ordo && ordoAmount > 0)
                {
                    total += ordoAmount;
                }
                if (total > 0)
                {
                    if (ordoAmount > 0)
                    {
                        total *= 0.1f;
                    }
                    EffectiveEssences[type] = total * essenceMultiplier;
                }
            }

            if (EffectiveEssences.TryGetValue(EssenceType.Terra, out float terra)) Player.statDefense += (int)(terra * 0.0002f);
            if (EffectiveEssences.TryGetValue(EssenceType.Vita, out float vita)) Player.statLifeMax2 += (int)(vita * 0.0005f);
            if (EffectiveEssences.TryGetValue(EssenceType.Aer, out float aer)) Player.moveSpeed += aer * 0.00002f;
            if (EffectiveEssences.TryGetValue(EssenceType.Aqua, out float aqua)) Player.breathMax += (int)(aqua * 0.00005f);
            if (EffectiveEssences.TryGetValue(EssenceType.Pyros, out float pyro)) Player.GetCritChance(DamageClass.Generic) += pyro * 0.00005f;
            if (EffectiveEssences.TryGetValue(EssenceType.Arena, out float arena)) Player.runAcceleration += arena * 0.00001f;
            if (EffectiveEssences.TryGetValue(EssenceType.Acus, out float acus)) Player.maxMinions += (int)(acus * 0.00001f);
            if (EffectiveEssences.TryGetValue(EssenceType.Caro, out float caro)) Player.lavaMax += (int)(caro * 0.00005f);
            if (EffectiveEssences.TryGetValue(EssenceType.Mechanus, out float mechanus)) Player.GetAttackSpeed(DamageClass.Generic) += mechanus * 0.000001f;
            if (EffectiveEssences.TryGetValue(EssenceType.Silva, out float silva)) Player.lifeRegen += (int)(silva * 0.000075f);
            if (EffectiveEssences.TryGetValue(EssenceType.Chitinus, out float chitinus)) Player.endurance += chitinus * 0.0000005f;
            if (EffectiveEssences.TryGetValue(EssenceType.Pelagus, out float pelagus)) Player.GetKnockback(DamageClass.Generic) += pelagus * 0.000001f;
            if (EffectiveEssences.TryGetValue(EssenceType.Solar, out float solar)) Player.GetDamage(DamageClass.Melee) += solar * 0.0000005f;
            if (EffectiveEssences.TryGetValue(EssenceType.Vortex, out float vortex)) Player.GetDamage(DamageClass.Ranged) += vortex * 0.0000005f;
            if (EffectiveEssences.TryGetValue(EssenceType.Nebula, out float nebula)) Player.GetDamage(DamageClass.Magic) += nebula * 0.0000005f;
            if (EffectiveEssences.TryGetValue(EssenceType.Stellaris, out float stellaris)) Player.GetDamage(DamageClass.Summon) += stellaris * 0.0000005f;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (EffectiveEssences.TryGetValue(EssenceType.Anima, out float anima))
            {
                bool isUndead = NPCID.Sets.Zombies[target.type] || NPCID.Sets.Skeletons[target.type] || target.HitSound == SoundID.NPCHit2;
                if (isUndead)
                {
                    modifiers.FinalDamage *= 1f + (anima * 0.0000005f);
                }
            }
            if (EffectiveEssences.TryGetValue(EssenceType.Regalis, out float regalis))
            {
                if (target.life > target.lifeMax / 2)
                {
                    modifiers.FinalDamage *= 1f + (regalis * 0.0000005f);
                }
            }
            if (EffectiveEssences.TryGetValue(EssenceType.Lux, out float lux))
            {
                if (!target.boss)
                {
                    modifiers.FinalDamage *= 1f + (lux * 0.0000001f);
                }
            }
            if (EffectiveEssences.TryGetValue(EssenceType.Imperium, out float imperium))
            {
                if (target.boss)
                {
                    modifiers.FinalDamage *= 1f + (imperium * 0.0000001f);
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TriggerOnHitEssenceEffects(target, hit, damageDone, null);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj != null && (proj.type == ModContent.ProjectileType<projectiles.LunaStarExplosionProj>() || proj.type == ModContent.ProjectileType<projectiles.FulgurChainLightningProj>()))
            {
                return;
            }
            TriggerOnHitEssenceEffects(target, hit, damageDone, proj);
        }

        private void TriggerOnHitEssenceEffects(NPC target, NPC.HitInfo hit, int damageDone, Projectile proj)
        {
            if (EffectiveEssences.TryGetValue(EssenceType.Umbra, out float umbra))
            {
                if (Main.rand.NextFloat() * 100f < MathHelper.Clamp(umbra * 0.05f, 5f, 50f))
                {
                    target.AddBuff(BuffID.Weak, 60 * 5);
                }
            }
            if (EffectiveEssences.TryGetValue(EssenceType.Cruor, out float cruor))
            {
                if (Main.rand.NextFloat() * 100f < MathHelper.Clamp(cruor * 0.05f, 5f, 40f))
                {
                    int healAmount = (int)(Player.statLifeMax2 * 0.05f);
                    Player.statLife += healAmount;
                    if (Player.statLife > Player.statLifeMax2) Player.statLife = Player.statLifeMax2;
                    Player.HealEffect(healAmount, true);
                }
            }
            if (EffectiveEssences.TryGetValue(EssenceType.Venenum, out float venenum))
            {
                if (Main.rand.NextFloat() * 100f < MathHelper.Clamp(venenum * 0.05f, 5f, 50f))
                {
                    target.AddBuff(BuffID.Poisoned, 60 * 5);
                }
            }
            if (EffectiveEssences.TryGetValue(EssenceType.Glacies, out float glacies))
            {
                if (Main.rand.NextFloat() * 100f < MathHelper.Clamp(glacies * 0.05f, 5f, 50f))
                {
                    target.AddBuff(ModContent.BuffType<buffs.GlaciesSlowBuff>(), 60 * 5);
                }
            }
            if (EffectiveEssences.TryGetValue(EssenceType.Fulgur, out float fulgur))
            {
                float chance = MathHelper.Clamp(15f + fulgur * 0.1f, 15f, 50f);
                if (Main.rand.NextFloat() * 100f < chance)
                {
                    int chainDamage = (int)(hit.SourceDamage * 0.50f);
                    if (chainDamage < 1) chainDamage = 1;

                    Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<projectiles.FulgurChainLightningProj>(), chainDamage, hit.Knockback * 0.5f, Player.whoAmI, target.whoAmI);
                }
            }
            if (EffectiveEssences.TryGetValue(EssenceType.Luna, out float luna))
            {
                float chance = MathHelper.Clamp(10f + luna * 0.1f, 10f, 40f);
                if (Main.rand.NextFloat() * 100f < chance)
                {
                    int baseDamage = Player.HeldItem != null && Player.HeldItem.damage > 0 ? Player.HeldItem.damage : hit.SourceDamage;
                    int explosionDamage = (int)(baseDamage * Main.rand.NextFloat(3f, 4f));
                    Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<projectiles.LunaStarExplosionProj>(), explosionDamage, hit.Knockback, Player.whoAmI);
                }
            }
        }
    }
}
