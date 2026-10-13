using jarkos.particles;
using jarkos.projectiles.bullet;
using Jarkos.trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
	public class Mage_slayer_alt_attack : ModProjectile {
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = true;
			Projectile.scale = 1f;
			Projectile.timeLeft = 100;
			Projectile.light = 0.95f;
			DrawOffsetX = -(int)((30 / 2) - (Projectile.Size.X / 2));
			DrawOriginOffsetY = -(int)((88 / 2) - (Projectile.Size.Y / 2));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<Burst_hitbox_one>(), Projectile.damage * 3, 0f, Projectile.owner);
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<Mage_slayer_alt_hit_visual>(), 0, 0f, Projectile.owner);
			}
		}
        public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.direction = player.direction;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		public Primitivedraw TrailDrawer { get; private set; } = null;
        public float WidthFunction(float completionRatio)
        {
            float width_trail = 4f;
            float baseWidth = Projectile.scale * (Projectile.width / 4) * width_trail;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }
        public Color ColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.DarkViolet, Color.Transparent, completionRatio) * 0.7f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            TrailDrawer ??= new Primitivedraw(WidthFunction, ColorFunction, GameShaders.Misc["Jarkos:LaserShader"]);
            GameShaders.Misc["Jarkos:LaserShader"].SetShaderTexture(Trailreg.test);
            TrailDrawer.DrawPrims(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 155);
            return true;
        }
	}

	public class Mage_slayer_alt_hit_visual : ModProjectile
	{
		public override string Texture => "Terraria/Images/Item_0";
		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.timeLeft = 2;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}
		public override void AI()
		{
			if (Projectile.timeLeft == 2 && Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 10; i++)
				{
					float modifier = Main.rand.NextFloat(0.25f, 2.25f);
					float size = Main.rand.NextFloat(0.5f, 0.75f);
					new Spark_gravit(Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-35f, 35f))) * modifier, Color.MediumPurple, 300, size * 2f, 2f, 0f).Spawn();
				}
				for (int i = 0; i < 2; i++)
				{
					float modifier = Main.rand.NextFloat(0.25f, 2.25f);
					float size = Main.rand.NextFloat(0.5f, 0.75f);
					new Spark_gravit(Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-35f, 35f))) * modifier, Color.HotPink, 300, size * 4f, 2f, 0f).Spawn();
				}
				new Line_glow_smaller(Projectile.Center, Projectile.velocity * 2f, Color.HotPink, 300, 0.75f, 2f, 0f).Spawn();
				SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Mage_slayer_impact") with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.position);
			}
		}
	}
}
