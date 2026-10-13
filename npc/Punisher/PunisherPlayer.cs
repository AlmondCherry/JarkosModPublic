using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using jarkos.particles;
using Terraria.DataStructures;
using System;

namespace jarkos.npc.Punisher
{
    public class PunisherPlayer : ModPlayer
    {
        public bool isKicked = false;
        public bool inCutscene = false;
        private float customOldVelocityX = 0f;
        public override void ResetEffects()
        {
            isKicked = false;
            inCutscene = false;
        }
        public override bool CanUseItem(Item item)
        {
            if (inCutscene) return false;
            return base.CanUseItem(item);
        }
        public override void PreUpdate()
        {
            customOldVelocityX = Player.velocity.X;
        }
        public override void PostUpdate()
        {
            if (isKicked)
            {
                if (Player.velocity.Y > -2f)
                {
                    Player.velocity.Y = -2f;
                    Player.fallStart = (int)(Player.position.Y / 16f);
                }
                if (Math.Abs(customOldVelocityX) > 7f && Math.Abs(Player.velocity.X) < 1f)
                {
                    SoundEngine.PlaySound(SoundID.Item14, Player.Center);
                    for (int i = 0; i < 15; i++)
                    {
                        new Spark(Player.Center, new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), Color.DarkRed, 30, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
                        new Stretching_particle_out(Player.Center, new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f)), Color.IndianRed, 20, Main.rand.NextFloat(10f, 20f), Main.rand.NextFloat(1f, 3f), 0f).Spawn();
                        new High_smoke(Player.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Color.Gray, 40, Main.rand.NextFloat(0.5f, 1f), 0.5f).Spawn();
                    }
                    new Circular_glow(Player.Center, Vector2.Zero, Color.DarkRed, 30, 2f, 1f).Spawn();
                    
                    Player.AddBuff(BuffID.Bleeding, 600);
                    Player.AddBuff(BuffID.BrokenArmor, 300);
                    Player.AddBuff(BuffID.Confused, 120);
                    Player.ClearBuff(ModContent.BuffType<PunisherKicked>());
                }
            }
        }
    }
}
