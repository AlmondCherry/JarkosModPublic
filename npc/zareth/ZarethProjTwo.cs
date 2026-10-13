using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.zareth
{
    public class ZarethProjTwo : ModProjectile
    {
        private ref float WaveIndex => ref Projectile.ai[0];
        private bool foundGround;
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 140;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Point tile = Projectile.Center.ToTileCoordinates();
            for (int y = tile.Y - 2; y < tile.Y + 12; y++)
            {
                if (WorldGen.SolidTile(tile.X, y) && !WorldGen.SolidTile(tile.X, y - 1))
                {
                    Projectile.Bottom = new Vector2(Projectile.Center.X, y * 16f);
                    foundGround = true;
                    break;
                }
            }
            if (!foundGround)
            {
                Projectile.Kill();
                return;
            }

            Projectile.scale = 0.7f + WaveIndex * 0.2f;
            Projectile.width = (int)(28 * Projectile.scale);
            Projectile.height = (int)(40 * Projectile.scale);
            Projectile.Bottom = new Vector2(Projectile.Center.X, Projectile.Bottom.Y);
        }
        public override void AI()
        {
            if (!foundGround)
                return;
            int delay = (int)WaveIndex * 5;
            int localTime = 140 - Projectile.timeLeft;

            if (localTime < delay)
                return;

            if (localTime == delay)
            {
                Projectile.hide = false;
                SoundEngine.PlaySound(SoundID.Item70 with { Pitch = 0.4f - WaveIndex * 0.07f }, Projectile.Center);

                for (int i = 0; i < 12; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.BottomLeft - new Vector2(0, 8),
                        Projectile.width, 8, DustID.Dirt, 0f, -3f, 0, default, 1.4f);
                    d.velocity.X *= 1.5f;
                }
            }
            int life = localTime - delay;
            if (life < 10)
                Projectile.Opacity = life / 10f;

            else if (Projectile.timeLeft < 20)
                Projectile.Opacity = Projectile.timeLeft / 20f;
            else
                Projectile.Opacity = 1f;
        }
        public override bool CanHitPlayer(Player target) => !Projectile.hide && Projectile.Opacity >= 0.8f;
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.hide || !foundGround)
                return false;

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            
            float emerge = MathHelper.Clamp(Projectile.Opacity, 0f, 1f);
            Main.EntitySpriteDraw(tex, Projectile.Bottom - Main.screenPosition, null, lightColor * emerge, 0f, origin, new Vector2(Projectile.scale, Projectile.scale * emerge), SpriteEffects.None);
            return false;
        }
    }
}
