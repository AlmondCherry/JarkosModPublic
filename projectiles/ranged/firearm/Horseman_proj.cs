using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Shaders;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Jarkos.trails;
using static Terraria.ModLoader.ModContent;
using jarkos.systems;
namespace jarkos.projectiles.ranged.firearm
{
	public class Horseman_proj : ModProjectile
	{
		NPC targetNPC = null;
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults() {
            Projectile.width = 6;
            Projectile.height = 18;
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.scale = 1f;
            Projectile.penetrate = 1;
            Projectile.light = 0.35f;
            Projectile.timeLeft = 125;
		}
        public override void AI(){
		NPChoming.HomingProj(Projectile, 350f, 1f, 590);
        }
        public NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC target = Main.npc[k];
                if (target.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }
            return closestNPC;
        }
        public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(Color.Cyan) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale - 0.1f;
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, scale, SpriteEffects.None, 0);
			}
			return true;
		}
    }
}
