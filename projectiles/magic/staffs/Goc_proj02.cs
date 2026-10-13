using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;

namespace jarkos.projectiles.magic.staffs
{
    public class Goc_proj02 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.tileCollide = false;
            Projectile.width = 8;
            Projectile.height = 20;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 500;
			Projectile.penetrate = 1;
        }
        NPC targetNPC = null;
        public override void OnKill(int timeLeft)
		{
            for (int a = 0; a < 45; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f,2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Wraith, speed * 2, Scale: 1.50f);
			d.noGravity = true;
            }
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
        public override void AI()
        {
            if (Main.rand.NextBool(4)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.Wraith, Scale: 0.50f);
            dust.velocity /= 4;
            dust.noGravity = true;
            }
            Projectile.rotation += 0.035f * Projectile.velocity.Length();
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            if (Projectile.spriteDirection == 1)
            {
                DrawOffsetX = -2;
                DrawOriginOffsetY = -2;
                DrawOriginOffsetX = 0;
            }
            else
            {
                DrawOffsetX = 0;
                DrawOriginOffsetY = -2;
                DrawOriginOffsetX = 0;
            }
            float maxDetectRadius = 750f;
            if (targetNPC == null)
            {
                targetNPC = FindClosestNPC(maxDetectRadius);
                return;
            }
            Projectile.velocity += (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
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
		    MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
		    miscShaderData.UseSaturation(-2.8f);
		    miscShaderData.UseOpacity(4f);
		    miscShaderData.Apply();
		    Goc_proj02._vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    Goc_proj02._vertexStrip.DrawTrail();
		    Terraria.Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	    }
	    private Color StripColors(float progressOnStrip)
	    {
            Color result = Color.Black;
		    return result;
	    }
	    private float StripWidth(float progressOnStrip)
	    {
		    float num = 1f;
		    float lerpValue = Terraria.Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
		    num *= 1f - (1f - lerpValue) * (1f - lerpValue);
		    return MathHelper.Lerp(0f, 32f, num);
	    }
    }
}
