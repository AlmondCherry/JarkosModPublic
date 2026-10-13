using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using jarkos.buffs.debuff;
using jarkos.buffs.buff;
using Terraria.GameContent;
namespace jarkos.projectiles.melee.unique_melee
{
  public class Brewmaster_weapon_slash : ModProjectile
  {
        public float random_size_cut = Main.rand.NextFloat(0.25f, 0.75f);
        float pos = Main.rand.NextFloat(-36f, 36f);
        private float rand = Main.rand.NextFloat(-12, 12);
        private float random_sound = Main.rand.NextFloat(1f, 4f);
        private bool impact_frame = false;
        public override void SetStaticDefaults()
        {
          Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
          Projectile.width = 232;
          Projectile.height = 148;
          Projectile.damage = 500;
          Projectile.alpha = 0;
          Projectile.aiStyle = 0;
          Projectile.timeLeft = 100;
          Projectile.scale = 1f;
          Projectile.friendly = true;
          Projectile.scale = 1f;
          Projectile.ownerHitCheck = true;
          Projectile.tileCollide = false;
          Projectile.light = 1f;
          Projectile.penetrate = -1;
          Projectile.hide = true;
          Projectile.knockBack = 6.5f;
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
        public override void OnSpawn(IEntitySource source)
        {
          Player player = Main.player[Projectile.owner];
          Vector2 player_relative = player.RotatedRelativePoint(player.MountedCenter, true);
          if (Projectile.owner == Main.myPlayer) {
          Aimm(player_relative, player.HeldItem.shootSpeed);
          }
        }
        public override void AI()
        {
          Lighting.AddLight(Projectile.Center, new Vector3(1f, 1f, 1f));
          Player player = Main.player[Projectile.owner];
          Vector2 player_relative = player.RotatedRelativePoint(player.MountedCenter, true);
          PlayerVisuals(player, player_relative);
          float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
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
          if(Projectile.frameCounter == (int)(6 * (1 - (meleeSpeedMultiplier / 10))))
          {
          ++Projectile.frame;
          Projectile.frameCounter = 0;
          }
          if(Projectile.frame == 3)
          {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/brewmaster_slash") with {Pitch = Main.rand.NextFloat(-0.02f, 0.05f)}, Projectile.position);
            impact_frame = true;
          }
          else{
            impact_frame = false;
          }
          if (Projectile.frame >= 6)
          {
            Projectile.Kill();
          }
          Projectile.spriteDirection = Projectile.direction = Terraria.Utils.ToDirectionInt(Projectile.velocity.X > 0.0f);
          Projectile.rotation = Terraria.Utils.ToRotation(Projectile.velocity) + (Projectile.spriteDirection == 1 ? 0.0f : 3.141593f);
          int sign = Math.Sign(Projectile.velocity.X);
          float rotationValue = Projectile.rotation;
          Vector2 positionVector = (rotationValue + (sign == -1 ? (float)Math.PI : 0f)).ToRotationVector2() * 1f;
          Projectile.position += positionVector;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            target.AddBuff(ModContent.BuffType<Beer_buff>(), 600);
            player.AddBuff(ModContent.BuffType<Beer_buff_player>(), 600);
            if(impact_frame && random_sound < 2)
            {
              SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/brewmaster_impact") with {Pitch = Main.rand.NextFloat(-0.05f, 0.05f)}, Projectile.position);
            }
            else if(random_sound > 2)
            {
              SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/brewmaster_impact") with {Pitch = Main.rand.NextFloat(-0.05f, 0.05f)}, Projectile.position);
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
          aim = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Projectile.velocity), aim, 0.15f));
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
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
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
