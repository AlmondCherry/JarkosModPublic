using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;
using Terraria.Audio;
using jarkos.particles;
using Terraria.GameContent;
namespace jarkos.projectiles.melee.true_melee
{
  public class Yamato_slash : ModProjectile
  {
        public float random_size_cut = Main.rand.NextFloat(0.25f, 0.75f);
        float pos = Main.rand.NextFloat(-36f, 36f);
        private float rand = Main.rand.NextFloat(-12, 12);
        private float random_sound = Main.rand.NextFloat(1f, 4f);
        private bool impact_frame = false;
        public override void SetStaticDefaults()
        {
          Main.projFrames[Projectile.type] = 7;
        }
        public override void SetDefaults()
        {
          Projectile.width = 212;
          Projectile.height = 248;
          Projectile.damage = 500;
          Projectile.alpha = 0;
          Projectile.aiStyle = 0;
          Projectile.timeLeft = 100;
          Projectile.scale = 1f;
          Projectile.friendly = true;
          Projectile.scale = 2f;
          Projectile.ownerHitCheck = true;
          Projectile.tileCollide = false;
          Projectile.light = 1f;
          Projectile.penetrate = -1;
          Projectile.hide = true;
        }
        public float moving_slash
        {
          get => Projectile.ai[0];
          set => Projectile.ai[0] = value;
         }
        public enum AttackType
        {
          Slowest_attack, Fastest_cut
        }
        public override void AI()
        {
          Lighting.AddLight(Projectile.Center, new Vector3(1f, 1f, 1f));
          Player player = Main.player[Projectile.owner];
          Vector2 player_relative = player.RotatedRelativePoint(player.MountedCenter, true);
          if (Projectile.owner == Main.myPlayer) {
          Aimm(player_relative, player.HeldItem.shootSpeed);
          }
          PlayerVisuals(player, player_relative);
          Projectile.direction = player.direction;
          player.heldProj = Projectile.whoAmI;
          player.itemTime = player.itemAnimation;
          Projectile.position.X = player_relative.X - (Projectile.width / 2);
          Projectile.position.Y = player_relative.Y - (Projectile.height / 2);
          if (!player.frozen)
          {
            if (moving_slash == 0.0f)
            {
              moving_slash = 14f;
              Projectile.netUpdate = true;
            }
            if (player.itemAnimation < player.itemAnimationMax / 3)
            moving_slash -= 0.0f;
            else
            moving_slash += 0.7f;
          }
          ++Projectile.frameCounter;
          if (Projectile.frameCounter == 2)
          {
          ++Projectile.frame;
          Projectile.frameCounter = 0;
          }
          if(Projectile.frame == 3)
          {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/yamato_swoosh"), Projectile.position);
            impact_frame = true;
          }
          else{
            impact_frame = false;
          }
          if (Projectile.frame >= 7)
          {
            Projectile.Kill();
          }
          Projectile.spriteDirection = Projectile.direction = Terraria.Utils.ToDirectionInt(Projectile.velocity.X > 0.0f);
          Projectile.rotation = Terraria.Utils.ToRotation(Projectile.velocity) + (Projectile.spriteDirection == 1 ? 0.0f : 3.141593f);
          int sign = Math.Sign(Projectile.velocity.X);
          float rotationValue = Projectile.rotation;
          Vector2 positionVector = (rotationValue + (sign == -1 ? (float)Math.PI : 0f)).ToRotationVector2() * 10f;
          Projectile.position += positionVector;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(impact_frame && random_sound < 2)
            {
              SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/slash_s2"), Projectile.position);
              for(int i = 0; i < 4; i++)
              {
              Vector2 vct = Projectile.velocity;
              Vector2 pos2 = Main.rand.NextVector2Circular(-6f, 6f);
			        Dust d = Dust.NewDustPerfect(target.Center, DustID.Firework_Yellow, (vct + pos2) * 4f, Scale: 1.10f);
              Dust d2 = Dust.NewDustPerfect(target.Center, DustID.Firework_Yellow, (vct + pos2) * 4f, Scale: 1.10f, newColor: Color.Red);
			        d.noGravity = true;
              d2.noGravity = true;
              }
              for(int i = 0; i < 2; i++)
              {
              float rand_rot = Main.rand.NextFloat(0.1f, 0.4f);
              float size = Main.rand.NextFloat(0.15f, 0.5f);
              new Line_glow_astra(target.Center, Vector2.Zero, Color.Yellow, 140, size / 2, 2f, rand_rot / 2).Spawn();
              }
            }
            else if(random_sound > 2)
            {
              SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/yamato_impact"), Projectile.position);
              for(int i = 0; i < 4; i++)
              {
              Vector2 vct = Projectile.velocity * 0.8f;
              Vector2 pos2 = Main.rand.NextVector2Circular(-6f, 6f);
			        Dust d = Dust.NewDustPerfect(target.Center, DustID.Firework_Yellow, (vct + pos2) * 4f, Scale: 1.10f);
              Dust d2 = Dust.NewDustPerfect(target.Center, DustID.Firework_Yellow, (vct + pos2) * 4f, Scale: 1.10f, newColor: Color.Red);
			        d.noGravity = true;
              d2.noGravity = true;
              }
              for(int i = 0; i < 1; i++)
              {
              float rand_rot = Main.rand.NextFloat(0.1f, 0.4f);
              float size = Main.rand.NextFloat(0.15f, 0.5f);
              new Line_glow_astra(target.Center, Vector2.Zero, Color.White, 140, size / 2, 2f, rand_rot / 2).Spawn();
              }
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
          if(!impact_frame)
          {
            hitbox.X = 0;
            hitbox.Y = 0;
          }
          else{
            hitbox.Inflate(Projectile.width / 8, Projectile.height / 8);
          }
        }
        private void Aimm(Vector2 source, float speed)
        {
        Vector2 aim = Vector2.Normalize(Main.MouseWorld - source);
			  if (aim.HasNaNs()) {
				  aim = -Vector2.UnitY;
			  }
          aim = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Projectile.velocity), aim, 0.988f));
          aim *= speed;
			  if (aim != Projectile.velocity) {
				  Projectile.netUpdate = true;
			  }
			    Projectile.velocity = aim;
        }
        private void PlayerVisuals(Player player, Vector2 playerhandpos)
        {
          Projectile.Center = playerhandpos;
			    Projectile.spriteDirection = Projectile.direction;
			    player.ChangeDir(Projectile.direction);
			    player.heldProj = Projectile.whoAmI;
			    player.itemTime = 2;
			    player.itemAnimation = 2;
			    player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.LocalPlayer;
            Texture2D texture = null;
            if(player.name == "Smelecov")
            {
              texture = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/yamato_slash_smel", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;;
            }
            if(player.name == "Almond")
            {
              texture = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/yamato_slash_almond", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;;
            }
            if(player.name != "Smelecov" && player.name != "Almond")
            {
              texture = TextureAssets.Projectile[Projectile.type].Value;
            }
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);
            if(Projectile.direction == -1)
            {
              Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, Projectile.rotation, origin2, Projectile.scale, SpriteEffects.FlipHorizontally, 0);
            }
            else
            {
              Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
