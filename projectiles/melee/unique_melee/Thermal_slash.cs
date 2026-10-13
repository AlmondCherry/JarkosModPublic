using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;

namespace jarkos.projectiles.melee.unique_melee
{
  public class Thermal_slash : ModProjectile
  {
    public override void SetStaticDefaults()
    {
      Main.projFrames[Projectile.type] = 7;
    }
    public override void SetDefaults()
    {
      Projectile.width = 212;
      Projectile.height = 248;
      Projectile.aiStyle = 0;
      Projectile.timeLeft = 100;
      Projectile.penetrate = -1;
      Projectile.scale = 1f;
      Projectile.alpha = 0;
      Projectile.hide = true;
      Projectile.ownerHitCheck = true;
      Projectile.tileCollide = false;
      Projectile.friendly = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
      target.immune[Projectile.owner] = 12;
      SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/hit"), Projectile.position);
      for (int a = 0; a < 12; a++)
      {
        if (Main.rand.NextBool())
        {
          Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
          Dust dustt = Dust.NewDustPerfect(target.position, DustID.Firework_Red, speed * 2);
          dustt.noGravity = true;
          dustt.scale = 1f;
        }
      }
    }

    public override void AI()
    {
      Player player = Main.player[Projectile.owner];
      Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.4f, 0.64f));
      Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);

      UpdatePlayerVisuals(player, vector2);
      Projectile.direction = player.direction;
      player.heldProj = Projectile.whoAmI;
      player.itemTime = player.itemAnimation;
      Projectile.position.X = vector2.X - (Projectile.width / 2);
      Projectile.position.Y = vector2.Y - (Projectile.height / 2);

      ++Projectile.frameCounter;
      if (Projectile.frameCounter >= 2)
      {
        ++Projectile.frame;
        Projectile.frameCounter = 0;
      }
      if (Projectile.frame >= 7)
      {
        Projectile.Kill();
      }

      Projectile.spriteDirection = Projectile.direction = Terraria.Utils.ToDirectionInt((double)Projectile.velocity.X > 0.0);
      Projectile.rotation = Terraria.Utils.ToRotation(Projectile.velocity) + (Projectile.spriteDirection == 1 ? 0.0f : 3.141593f);
    }

    private void UpdatePlayerVisuals(Player player, Vector2 playerHandPos)
    {
      Projectile.Center = playerHandPos;
      Projectile.spriteDirection = Projectile.direction;
      player.ChangeDir(Projectile.direction);
      player.heldProj = Projectile.whoAmI;
      player.itemTime = 2;
      player.itemAnimation = 2;
      player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
    }
  }
}
