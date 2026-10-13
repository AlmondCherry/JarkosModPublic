using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace jarkos.projectiles.ranged.other
{
    public class FlamingPlateProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WoodenArrowFriendly;
        public override void SetDefaults() 
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.aiStyle = -1;
        }
        public override void AI() 
        {
            Projectile.velocity.Y += 0.2f;

            if (Projectile.velocity.Y > 16f) 
            {
                Projectile.velocity.Y = 16f;
            }

            Projectile.rotation += Projectile.velocity.X * 0.05f;
            if (Main.rand.NextBool(3)) 
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1f);
                Main.dust[dust].noGravity = true;
            }
        }
        public override void OnKill(int timeLeft) 
        {
            SoundEngine.PlaySound(SoundID.Item107, Projectile.position);

            for (int i = 0; i < 35; i++) 
            {
                Vector2 speed = Main.rand.NextVector2Circular(6f, 6f);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Grass, speed.X, speed.Y, 100, default, Main.rand.NextFloat(1f, 1.8f));
                Main.dust[dust].noGravity = Main.rand.NextBool(2);
            }

            int radius = 6;
            int centerTileX = (int)(Projectile.Center.X / 16f);
            int centerTileY = (int)(Projectile.Center.Y / 16f);

            for (int x = centerTileX - radius; x <= centerTileX + radius; x++) 
            {
                for (int y = centerTileY - radius; y <= centerTileY + radius; y++) 
                {
                    if (!WorldGen.InWorld(x, y)) continue;
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && tile.TileType == TileID.Saplings) 
                    {
                        bool isPalm = false;
                        for (int checkY = y + 1; checkY <= y + 3; checkY++) 
                        {
                            if (WorldGen.InWorld(x, checkY)) {
                                ushort type = Main.tile[x, checkY].TileType;
                                if (type == TileID.Sand || type == TileID.Ebonsand || type == TileID.Crimsand || type == TileID.Pearlsand) 
                                {
                                    isPalm = true;
                                    break;
                                }
                            }
                        }

                        bool success = false;
                        if (isPalm) 
                        {
                            success = WorldGen.GrowPalmTree(x, y);
                        } else 
                        {
                            success = WorldGen.GrowTree(x, y);
                        }
                        if (success) 
                        {
                            for (int d = 0; d < 15; d++) 
                            {
                                int dust = Dust.NewDust(new Vector2(x * 16, y * 16 - 32), 16, 48, DustID.Grass, 0f, -2f, 100, default, 1.5f);
                                Main.dust[dust].velocity *= 1.2f;
                            }
                            if (Main.netMode == NetmodeID.Server) 
                            {
                                NetMessage.SendTileSquare(-1, x - 5, y - 40, 10, 50);
                            }
                        }
                    }
                }
            }
        }
    }
}
