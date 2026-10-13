using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.CameraModifiers;
using Terraria.Audio;
using Terraria.ID;

namespace jarkos.npc.Punisher
{
    public class DimensionSunderProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public bool IsVertical => Projectile.ai[0] == 1f;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
            Projectile.netImportant = true;
        }
        public override void AI()
        {
            float intensity = 1f;
            if (Projectile.timeLeft > 540)
            {
                Projectile.hostile = false;
                float warnProgress = (600f - Projectile.timeLeft) / 60f;
                intensity = warnProgress * 0.3f;

                if (Projectile.timeLeft % 10 == 0)
                {
                   PunchCameraModifier modifier = new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), 3f, 6f, 10, 3000f, Projectile.Name);
                    Main.instance.CameraModifiers.Add(modifier);
                }
            }
            else
            {
                Projectile.hostile = true;
                if (Projectile.timeLeft == 540)
                {
                    PunchCameraModifier modifier = new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), 15f, 6f, 20, 3000f, Projectile.Name);
                    Main.instance.CameraModifiers.Add(modifier);
                    SoundEngine.PlaySound(SoundID.Item73 with { Volume = 2f, Pitch = -0.5f }, Projectile.Center);
                }
                float progress = 1f - (Projectile.timeLeft / 540f);
                if (progress > 0.9f) intensity = (1f - progress) / 0.1f;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                if (!Filters.Scene["jarkos:DimensionSunder"].IsActive())
                {
                    Filters.Scene.Activate("jarkos:DimensionSunder");
                }
                Vector2 dir = IsVertical ? new Vector2(0, 1) : new Vector2(1, 0);

                var effect = Filters.Scene["jarkos:DimensionSunder"].GetShader().Shader;
                if (effect.Parameters["uTearPosition"] != null)
                    effect.Parameters["uTearPosition"].SetValue(Projectile.Center);
                if (effect.Parameters["uTearDir"] != null)
                    effect.Parameters["uTearDir"].SetValue(dir);
                if (effect.Parameters["uOpacity"] != null)
                    effect.Parameters["uOpacity"].SetValue(intensity);
                if (effect.Parameters["uScreenPos"] != null)
                    effect.Parameters["uScreenPos"].SetValue(Main.screenPosition);
                    
                Vector2 zoomedSize = new Vector2(Main.screenWidth, Main.screenHeight) / Main.GameViewMatrix.Zoom;
                if (effect.Parameters["uScreenRes"] != null)
                    effect.Parameters["uScreenRes"].SetValue(zoomedSize);
                if (effect.Parameters["uColor"] != null)
                    effect.Parameters["uColor"].SetValue(new Vector3(1.5f, 0.2f, 0.5f));
                if (effect.Parameters["uTime"] != null)
                    effect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
            }
            if (Projectile.timeLeft == 580)
            {
                PunchCameraModifier modifier = new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), 15f, 6f, 20, 3000f, Projectile.Name);
                Main.instance.CameraModifiers.Add(modifier);
                SoundEngine.PlaySound(SoundID.Item73 with { Volume = 2f, Pitch = -0.5f }, Projectile.Center);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.timeLeft > 540) return false;
            float collisionPoint = 0f;
            Vector2 startPos = Projectile.Center;
            Vector2 endPos = Projectile.Center;
            if (IsVertical)
            {
                startPos.Y -= 5000f;
                endPos.Y += 5000f;
            }
            else
            {
                startPos.X -= 5000f;
                endPos.X += 5000f;
            }
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), startPos, endPos, 60f, ref collisionPoint);
        }
        public override void Kill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (Filters.Scene["jarkos:DimensionSunder"].IsActive())
                {
                    Filters.Scene.Deactivate("jarkos:DimensionSunder");
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 spawnPos = Projectile.Center;
                    float offset = (i - 5.5f) * 200f;
                    if (IsVertical) spawnPos.Y += offset;
                    else spawnPos.X += offset;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<HomingDaggerProj>(), Projectile.damage / 2, 0f, Main.myPlayer);
                }
            }
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
