using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;

namespace jarkos.projectiles.magic.staffs
{
    public class Goc_proj01 : ModProjectile
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
            Projectile.width = 21;
            Projectile.height = 52;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 800;
			Projectile.penetrate = 3;
        }
        public override void AI()
        {
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X, Projectile.position.Y - 180, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, ModContent.ProjectileType<Goc_proj02>(), (int)(Projectile.damage * 1), 0f, Projectile.owner, 0f, 0f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X - 90, Projectile.position.Y + 180, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, ModContent.ProjectileType<Goc_proj02>(), (int)(Projectile.damage * 1), 0f, Projectile.owner, 0f, 0f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 90, Projectile.position.Y + 180, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, ModContent.ProjectileType<Goc_proj02>(), (int)(Projectile.damage * 1), 0f, Projectile.owner, 0f, 0f);
            base.OnHitNPC(target, hit, damageDone);
        }
        public override void OnKill(int timeLeft)
        {
            for (int a = 0; a < 45; a++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f,2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Wraith, speed * 3, Scale: 2f);
			d.noGravity = true;
            }
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
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
		    Goc_proj01._vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    Goc_proj01._vertexStrip.DrawTrail();
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
