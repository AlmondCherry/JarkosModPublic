using System;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class True_plague : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.rand.NextBool(8))
            {
                new Solid_particle(npc.Center, Main.rand.NextVector2Circular(5f, 5f), Color.IndianRed, 120, 0.35f, 2f, 0f).Spawn();
            }

            npc.GetGlobalNPC<SpreaderPlagueNPC>().plagueInfected = true;
        }
    }

    public class SpreaderPlagueNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool plagueInfected = false;
        public int darkpedSlashTimer = 0;
        private int particleTimer = 0;
        private int soundTimer = 0;

        private float damageMultiplier = 1.25f;

        public override void ResetEffects(NPC npc)
        {
            plagueInfected = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (plagueInfected)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                if (npc.boss)
                {
                    npc.lifeRegen = -15;
                    damage = 4;
                }
                else
                {
                    npc.lifeRegen -= 30;
                    damage = 8;
                }
            }
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (plagueInfected)
            {
                modifiers.FinalDamage *= damageMultiplier;
            }
        }

        public override void AI(NPC npc)
        {
            if (!plagueInfected) return;
            particleTimer++;
            soundTimer++;

            if (particleTimer >= 40)
            {
                particleTimer = 0;
                Dust plagueDust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.LifeDrain, 0, -2f, 120, Color.DarkRed, 1f);
                plagueDust.noGravity = true;
                plagueDust.fadeIn = 1.3f;
            }

            if (soundTimer >= 60 && Main.rand.NextBool(3))
            {
                soundTimer = 0;

                SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.3f, Pitch = -0.3f }, npc.position);
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (plagueInfected)
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
                drawColor = Color.Lerp(drawColor, new Color(150, 30, 30), 0.35f + pulse);

                if (Main.GameUpdateCount % 30 < 15)
                {
                    drawColor = Color.Lerp(drawColor, Color.DarkRed, 0.15f);
                }
            }
        }
        public override void OnKill(NPC npc)
        {
            if (plagueInfected)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust deathDust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.LifeDrain, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, Color.Crimson, 1.5f);
                    deathDust.noGravity = true;
                }
                float deathRadius = 550f;
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC nearby = Main.npc[i];
                    if (nearby.active && !nearby.friendly && nearby.whoAmI != npc.whoAmI)
                    {
                        float dist = Vector2.Distance(npc.Center, nearby.Center);
                        if (dist < deathRadius)
                        {
                            nearby.AddBuff(ModContent.BuffType<True_plague>(), 180);
                        }
                    }
                }

                SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.6f, Pitch = -0.4f }, npc.position);
            }
        }
    }
}
