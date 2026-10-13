using jarkos.particles;
using jarkos.projectiles.bullet;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class Feather_black_debuff : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = true;
      Main.debuff[Type] = false;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = true;
    }
    public override void Update(NPC npc, ref int buffIndex)
    {
      var global_npc = npc.GetGlobalNPC<Jarkos_npc>();
      global_npc.shadow_pact_debuff = true;
      global_npc.timer_before_burst++;
      
      if (Main.rand.NextBool(2))
      {
        Vector2 rand_pos = npc.Center + new Vector2(Main.rand.NextFloat(-200f, 200f), Main.rand.NextFloat(-200f, 200f));
        Vector2 velocity_to_center = Vector2.Normalize(npc.Center - rand_pos) * 12;
        new Solid_particle(rand_pos, velocity_to_center, Color.White, 120, 0.35f, 2f, 0f).Spawn();
      }
      if (global_npc.timer_before_burst > 199)
      {
        Player player = Main.LocalPlayer;
        int damage = 250 * global_npc.hits_in_SP_debuff * 3;

        //Main.NewText(player.GetDamage(DamageClass.Generic).Additive);
        float final_damage = damage / 75 * (float)player.GetDamage(DamageClass.Generic).Additive;

        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Feather_black_burst") with {Pitch = Main.rand.NextFloat(-0.05f, 0.05f)}, npc.position);
        Projectile.NewProjectile(null, npc.Center, Vector2.Zero, ModContent.ProjectileType<Feather_black_burst>(), (int)final_damage + 5, 0f);
        global_npc.shadow_pact_debuff_already_used = true;
        global_npc.timer_before_burst = 0;
        global_npc.hits_in_SP_debuff = 0;
        global_npc.cooldown_shadow_pact = 0;
      }
	  }
  }
}
