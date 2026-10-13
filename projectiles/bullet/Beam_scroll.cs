using jarkos.particles;
using jarkos.projectiles.mecha;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
    public class Beam_scroll : Laserbeam, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override float MaxScale => 1.25f;
        public override float MaxLaserLength => 15f;
        public override float Lifetime => 10f;
        public override bool Glowing => false;
        public override Color LightCastColor => Color.White;
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
            DrawBeamWithColor(Color.White * 1.1f, Projectile.scale * 0.5f);
            return false;
        }
        public override void OnSpawn(IEntitySource source)
        {
			new Cube_particle(Projectile.Center, Projectile.velocity * Main.rand.NextFloat(8f, 8f), Color.White, 260, Main.rand.NextFloat(0.05f, 0.15f), 1f, 0f).Spawn();
            new Cube_edge_particle(Projectile.Center, Projectile.velocity * Main.rand.NextFloat(8f, 8f), Color.White, 260, Main.rand.NextFloat(0.05f, 0.15f), 1f, 0f).Spawn();
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
            new Spark(target.Center, (Projectile.velocity + hz) * 4f, Color.White, 140, 0.5f, 5f, 0f).Spawn();
            Projectile.netUpdate = true;
            }
        }
    }
}
