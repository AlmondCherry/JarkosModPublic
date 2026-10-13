using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;
using System;

namespace jarkos.npc.Punisher
{
    public class PunisherPhantomDash : ModProjectile
    {
        public override string Texture => "jarkos/npc/Punisher/Punisher";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 52;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 255;
        }
        public ref float AI_Timer => ref Projectile.ai[0];
        public ref float DashTargetX => ref Projectile.ai[1];
        public ref float DashTargetY => ref Projectile.ai[2];
        public override void AI()
        {
            AI_Timer++;
            if (AI_Timer < 50)
            {
                if (Projectile.alpha > 100)
                {
                    Projectile.alpha -= 10;
                }
                Vector2 target = new Vector2(DashTargetX, DashTargetY);
                Vector2 dir = target - Projectile.Center;
                Projectile.direction = Math.Sign(dir.X);
                if (Projectile.direction != 0) Projectile.spriteDirection = Projectile.direction;
                if (AI_Timer > 30)
                {
                    Projectile.Center += new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                }
            }
            if (AI_Timer == 50)
            {
                Vector2 target = new Vector2(DashTargetX, DashTargetY);
                Vector2 dir = Vector2.Normalize(target - Projectile.Center);
                Projectile.velocity = dir * 35f;
                SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
            }
            if (AI_Timer > 65)
            {
                Projectile.velocity *= 0.8f;
                Projectile.alpha += 15;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Npc[ModContent.NPCType<Punisher>()].Value;
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + Projectile.Size / 2f + new Vector2(0f, Projectile.gfxOffY);
                Color color = Color.MediumPurple * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * ((255 - Projectile.alpha) / 255f);
                color.A = 0;

                SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Rectangle frame = new Rectangle(0, 0, texture.Width, texture.Height / Main.npcFrameCount[ModContent.NPCType<Punisher>()]);
                Main.EntitySpriteDraw(texture, drawPos, frame, color, Projectile.rotation, frame.Size() / 2f, Projectile.scale, effects, 0);
            }

            SpriteEffects mainEffects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Rectangle mainFrame = new Rectangle(0, 0, texture.Width, texture.Height / Main.npcFrameCount[ModContent.NPCType<Punisher>()]);
            Color mainColor = Color.White * ((255 - Projectile.alpha) / 255f);
            
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), mainFrame, mainColor, Projectile.rotation, mainFrame.Size() / 2f, Projectile.scale, mainEffects, 0);
            return false;
        }
    }
}
