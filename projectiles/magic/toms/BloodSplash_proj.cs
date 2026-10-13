using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.toms
{
    public class BloodSplash_proj : ModProjectile
    {
        private const int Life = 30;
        private float Radius => 180f;
        private float Progress => 1f - Projectile.timeLeft / (float)Life;
        public override string Texture => "jarkos/items/weapons/magic/other/SporeGauntlet/SporeSoft";
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Life;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float r = Radius * Math.Min(1f, Progress * 2.2f + 0.25f);
            return Vector2.Distance(targetHitbox.Center.ToVector2(), Projectile.Center) <= r;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 1.2f, Pitch = -0.5f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.Center);
                if (Main.LocalPlayer.whoAmI == Projectile.owner)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 10f, 6f, 15, 1000f, "BloodSplash"));
                }
                for (int i = 0; i < 40; i++)
                {
                    Vector2 v = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(8f, 16f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, v, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                    d.noGravity = false;
                    if (Main.rand.NextBool(3))
                    {
                        Dust g = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, v * 1.5f, 0, default, Main.rand.NextFloat(2f, 3f));
                        g.noGravity = true;
                    }
                }
            }
        }
        public override void AI()
        {
            Color c = new Color(255, 20, 20);
            Lighting.AddLight(Projectile.Center, c.ToVector3() * (1.6f - Progress) * 2f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeNoise").Value;
            Texture2D ringTex = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeRingTex").Value;
            Texture2D soft = ModContent.Request<Texture2D>("jarkos/items/weapons/magic/other/SporeGauntlet/SporeSoft").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float prog = Progress;
            Color col = new Color(255, 20, 20);
            float quadSize = Radius * 2.6f;
            Main.spriteBatch.End();
            if (SporeShaders.Ring != null)
            {
                SporeShaders.Ring.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
                SporeShaders.Ring.Parameters["uProgress"]?.SetValue(prog);
                SporeShaders.Ring.Parameters["uColor"]?.SetValue(col.ToVector3());
                SporeShaders.Ring.Parameters["uSeed"]?.SetValue(Projectile.whoAmI * 0.137f);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, SporeShaders.Ring, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(noise, drawPos, null, Color.White, 0f, noise.Size() * 0.5f, quadSize / noise.Width, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            float flash = MathHelper.Clamp(1f - prog * 2.8f, 0f, 1f);
            if (flash > 0f)
            {
                Main.spriteBatch.Draw(soft, drawPos, null, Color.White * flash, 0f, soft.Size() * 0.5f, quadSize * 0.35f / soft.Width * (0.6f + prog * 2f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(soft, drawPos, null, col * (flash * 0.9f), 0f, soft.Size() * 0.5f, quadSize * 0.6f / soft.Width * (0.6f + prog * 2f), SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
