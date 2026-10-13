using System;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.dusts
{
    public class PurpleAbyssDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.fadeIn = dust.position.X;
            dust.velocity.Y = -Main.rand.NextFloat(0.3f, 0.8f);
            dust.velocity.X = 0f;

            dust.rotation = Main.rand.NextFloat(0f, (float)Math.PI * 2f);
            dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
        }
        public override bool Update(Dust dust)
        {
            dust.position.Y += dust.velocity.Y;
            dust.position.X = dust.fadeIn + (float)Math.Sin(dust.position.Y * 0.04f + dust.rotation) * 8f;
            dust.scale *= 0.985f;
            float light = 0.5f * dust.scale;

            Lighting.AddLight(dust.position, light * 0.6f, 0.1f, light * 0.8f);
            if (dust.scale < 0.15f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}
