using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
    public class ArrowGlobalProjectile : GlobalProjectile
    {
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (IsArrow(projectile))
            {
                if (target.velocity.LengthSquared() > 0.25f)
                {
                    modifiers.SourceDamage *= 1.25f;
                }
            }
        }

        public static bool IsArrow(Projectile projectile)
        {
            if (projectile.arrow)
            {
                return true;
            }
            if (projectile.ModProjectile != null && projectile.ModProjectile.Mod.Name == "jarkos")
            {
                string name = projectile.ModProjectile.Name.ToLower();
                if (name.Contains("arrow"))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (IsArrow(projectile))
            {
                SpawnBlockHitEffects(projectile, oldVelocity);
            }
            return base.OnTileCollide(projectile, oldVelocity);
        }

        public static void SpawnBlockHitEffects(Projectile projectile, Vector2 oldVelocity)
        {
            if (Main.netMode == NetmodeID.Server) return;

            Vector2 hitPos = projectile.Center + oldVelocity;
            int tileX = (int)(hitPos.X / 16f);
            int tileY = (int)(hitPos.Y / 16f);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int x = tileX + dx;
                    int y = tileY + dy;

                    if (WorldGen.InWorld(x, y, 10))
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                        {
                            WorldGen.KillTile(x, y, fail: true, effectOnly: true);

                            Vector2 hitNormal = -oldVelocity.SafeNormalize(Vector2.Zero);
                            for (int i = 0; i < 6; i++)
                            {
                                int dustIndex = WorldGen.KillTile_MakeTileDust(x, y, tile);
                                if (dustIndex >= 0 && dustIndex < Main.maxDust)
                                {
                                    Dust d = Main.dust[dustIndex];
                                    d.velocity = hitNormal.RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 5.0f);
                                    d.noGravity = false;
                                    d.scale = Main.rand.NextFloat(0.9f, 1.4f);
                                }
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
}
