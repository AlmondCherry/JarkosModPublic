using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using jarkos.particles;
using Terraria.GameContent;
using Terraria.Audio;

namespace jarkos.npc.Celestia
{
    public class CelestiaPhantom : ModProjectile
    {
        public override string Texture => "jarkos/npc/Celestia/Celestia";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 24;
        }

        public override void SetDefaults()
        {
            Projectile.width = 158;
            Projectile.height = 154;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.alpha = 254;
        }

        public ref float Delay => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.ai[1];

        private bool hasDashed = false;
        private Vector2 dashDirection;

        public override void AI()
        {
            Timer++;

            bool isDecoy = Delay < 0;
            float realDelay = isDecoy ? -Delay : Delay;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 24 || Projectile.frame < 12)
            {
                Projectile.frame = 12;
            }

            if (Timer < realDelay)
            {
                if (Projectile.alpha > 0)
                {
                    Projectile.alpha -= 15;
                    if (Projectile.alpha < 0) Projectile.alpha = 0;
                }

                Player player = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];

                int direction = player.Center.X < Projectile.Center.X ? -1 : 1;
                Projectile.spriteDirection = -direction;

                if (Main.rand.NextBool(4))
                {
                    new Stretching_particle(Projectile.Center + Main.rand.NextVector2Circular(60, 60), (Projectile.Center - (Projectile.Center + Main.rand.NextVector2Circular(60, 60))) * 0.1f, Color.Magenta, 15, 0.4f, 1f, 0f).Spawn();
                }

                if (Timer == realDelay - 1 && Main.netMode != NetmodeID.MultiplayerClient && !isDecoy)
                {
                    dashDirection = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    
                    Projectile.velocity = dashDirection * 25f;
                    Projectile.netUpdate = true;
                }
            }
            else if (Timer >= realDelay && !hasDashed)
            {
                if (isDecoy)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        new Stretching_particle(Projectile.Center, Main.rand.NextVector2Circular(8, 8), Color.Magenta * 0.5f, 20, 0.5f, 1f, 0f).Spawn();
                    }
                    Projectile.Kill();
                    return;
                }

                hasDashed = true;
                dashDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
                
                if (dashDirection == Vector2.Zero)
                {
                    Player player = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                    dashDirection = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = dashDirection * 25f;
                }

                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);

                for (int i = 0; i < 10; i++)
                {
                    new Circleedge(Projectile.Center, dashDirection * -Main.rand.NextFloat(2f, 5f), Color.HotPink, 30, 1f, 1f, 0f).Spawn();
                }
                
                Projectile.timeLeft = 60;
            }
            else if (hasDashed)
            {
                if (Timer % 2 == 0)
                {
                    new Circular_glow(Projectile.Center, Vector2.Zero, Color.Magenta * 0.5f, 20, 1.5f, 1f, 0f).Spawn();
                }
                if (Main.rand.NextBool(2))
                {
                    new Spark(Projectile.Center + Main.rand.NextVector2Circular(40, 40), Projectile.velocity * -0.2f, Color.HotPink, 30, 0.6f, 1f, 0f).Spawn();
                }

                Projectile.rotation = Projectile.velocity.X * 0.02f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color phantomColor = new Color(255, 80, 220, (byte)(255 - Projectile.alpha));

            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPos, sourceRect, phantomColor, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            float eyeOffsetY = -53f;
            float eyeOffsetX = -10f * Projectile.spriteDirection;
            Vector2 baseEyeOffset = new Vector2(eyeOffsetX, eyeOffsetY);
            Vector2 eyePos = Projectile.Center + baseEyeOffset.RotatedBy(Projectile.rotation);
            
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Vector2 rectSize = new Vector2(40, 15);
            
            Main.EntitySpriteDraw(magicPixel, eyePos - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * (1f - Projectile.alpha / 255f), Projectile.rotation, new Vector2(0.5f, 0.5f), rectSize, SpriteEffects.None, 0);

            if (hasDashed)
            {
                for (int i = 1; i < 4; i++)
                {
                    Vector2 trailPos = drawPos - Projectile.velocity * i * 0.5f;
                    Main.EntitySpriteDraw(texture, trailPos, sourceRect, phantomColor * (1f - (i * 0.25f)), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
                }
            }

            return false;
        }
    }
}
