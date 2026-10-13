using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
    public class Lighting_beam_cafas : Laserbeam, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override float MaxScale => 1.25f;
        public override float MaxLaserLength => 15f;
        public override float Lifetime => 10f;
        public override bool Glowing => false;
        public override Color LightCastColor => Color.Cyan;
        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("Jarkos/textures/lighting_beam_start", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("Jarkos/textures/lighting_beam_mid", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("Jarkos/textures/lighting_beam_end", AssetRequestMode.ImmediateLoad).Value;
        public override string Texture => "Jarkos/projectiles/mecha/Reflector_beam";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 200;
            Projectile.tileCollide = false;
            Projectile.timeLeft = (int)Lifetime;
        }
        public override void DetermineScale() => Projectile.scale = Projectile.timeLeft / Lifetime * MaxScale;
        public override bool PreDraw(ref Color lightColor)
        {
            DrawBeamWithColor(Color.Cyan * 1.1f, Projectile.scale * 0.5f);
            return false;
        }
        public override void OnSpawn(IEntitySource source)
        {
			new Accurate_circle_particle_tight(Projectile.Center, Projectile.velocity * Main.rand.NextFloat(2f, 4f), Color.Cyan, 260, 0.000015f, 1f, 0f).Spawn();
            for (int i = 0; i < 1; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Clentaminator_Cyan, speed * 4.5f, Scale: 1f);
                d.noGravity = true;
        	}
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.95f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for(int i = 0; i < 1; i++)
            {
                Vector2 hz = Main.rand.NextVector2Unit(Main.rand.NextFloat(-100f, 100f), Main.rand.NextFloat(-20f, 20f));
                new Spark(target.Center, (Projectile.velocity + hz) * 4f, Color.Cyan, 140, 0.5f, 5f, 0f).Spawn();
                Projectile.netUpdate = true;
            }
        }
    }
}
