using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class FuryMarkDebuff : ModBuff
  {
    public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            FuryMarkGlobalNPC markNPC = npc.GetGlobalNPC<FuryMarkGlobalNPC>();
            int stacks = markNPC.furyMarkStacks;

            if (stacks > 0)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(npc.position - new Vector2(2f), npc.width + 4, 5, DustID.RedTorch);
                    dust.velocity.X = Main.rand.NextFloat(-0.5f, 0.5f);
                    dust.velocity.Y = Main.rand.NextFloat(-2f, -1f);
                    dust.scale = 0.8f + (stacks * 0.1f);
                    dust.noGravity = true;
                    dust.alpha = 100;
                }
                Lighting.AddLight(npc.Center, 0.6f * (stacks / 10f), 0.1f, 0.1f);
            }
        }
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            foreach(NPC n in Main.npc)
            {
                if (n.active && n.HasBuff(Type))
                {
                    FuryMarkGlobalNPC markNPC = n.GetGlobalNPC<FuryMarkGlobalNPC>();
                    int stacks = markNPC.furyMarkStacks;
                    float damageBonus = stacks * 2.5f;

                    tip = Language.GetTextValue("Mods.jarkos.Buffs.FuryMarkDebuff.Description", damageBonus, stacks);
                    break;
                }
            }
        }
    }
    public class FuryMarkGlobalNPC : GlobalNPC
    {
        public int furyMarkStacks = 0;
        public float FuryMarkDamageMultiplier => 1f + (furyMarkStacks * 0.025f);
        public override bool InstancePerEntity => true;
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (furyMarkStacks > 0 && npc.HasBuff(ModContent.BuffType<FuryMarkDebuff>()))
            {
                modifiers.FinalDamage *= FuryMarkDamageMultiplier;
                modifiers.CritDamage += 0.05f * furyMarkStacks;
            }
        }
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (furyMarkStacks > 0)
            {
                for (int i = 0; i < furyMarkStacks / 2; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.RedTorch, Scale: 1.2f);
                }
            }
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (furyMarkStacks > 0)
            {
                for (int i = 0; i < furyMarkStacks / 3; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.RedTorch, Scale: 1.5f);
                }
            }
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (furyMarkStacks > 0 && npc.HasBuff(ModContent.BuffType<FuryMarkDebuff>()))
            {
                float intensity = furyMarkStacks / 10f;
                drawColor = Color.Lerp(drawColor, new Color(255, 200 - (int)(intensity * 100), 200 - (int)(intensity * 100)), intensity * 0.5f);
            }
        }
    }
}
