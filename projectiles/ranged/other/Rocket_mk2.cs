using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Windows;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;
using Terraria.UI.Chat;
using ReLogic.Content;
using Newtonsoft.Json;
using Terraria.GameContent.UI.States;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using jarkos.projectiles.magic.toms;
using Terraria.Graphics;
using System.Runtime.InteropServices;
using jarkos.particles;
using jarkos.projectiles.ranged.firearm;
namespace jarkos.projectiles.ranged.other
{
	public class Rocket_mk2 : ModProjectile
	{
        NPC targetNPC = null;
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.netImportant = true;
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.scale = 0.80f;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.damage = 400;
            Projectile.timeLeft = 600;
            Projectile.light = 1f;
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.LocalPlayer;
            for(int i = 0; i < 4; i++)
            {
            new Circleedge(Projectile.Center, Vector2.Zero, Color.MediumPurple, 180, 1f, 2f, 0f).Spawn();
            }
            new StarParticle(Projectile.Center, Vector2.Zero, Color.MediumPurple, 180, 0.3f, 2f, 0.09f).Spawn();
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/short_burst_sound"), Projectile.position);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity * 0, ProjectileType<Rocketmk2_hitbox>(), 200, 0f, Projectile.owner, 0f, 0f);
        }
        public override void AI(){
		    float maxDetectRadius = 400f;
            if (targetNPC == null)
            {
                targetNPC = FindClosestNPC(maxDetectRadius);
                return;
            }
            if (targetNPC != null && Projectile.timeLeft < 500) {
            Projectile.velocity += (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.velocity *= 0.94f;
            }
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
        private static VertexStrip _vertexStrip = new VertexStrip();
        public override bool PreDraw(ref Color lightcolor) {
            Draw(Projectile);
            return true;
        }
	    public void Draw(Projectile proj)
	    {
		    MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
		    miscShaderData.UseSaturation(-2.8f);
		    miscShaderData.UseOpacity(4f);
		    miscShaderData.Apply();
		    _vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    _vertexStrip.DrawTrail();
		    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	    }
	    private Color StripColors(float progressOnStrip)
	    {
            Color result = Color.MediumPurple;
		    return result;
	    }
	    private float StripWidth(float progressOnStrip)
	    {
		    float num = 1f;
		    float lerpValue = Terraria.Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
		    num *= 1f - (1f - lerpValue) * (1f - lerpValue);
		    return MathHelper.Lerp(0f, 44f, num);
	    }
    }
}
