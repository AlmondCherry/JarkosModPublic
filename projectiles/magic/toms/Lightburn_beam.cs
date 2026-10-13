using jarkos.particles;
using jarkos.projectiles.magic.toms;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
    public class Lightburn_beam : Laserbeam, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "jarkos/textures/beam_start";
        public Player Owner => Main.player[Projectile.owner];
        public override Color LightCastColor => Color.White;
        public override float Lifetime => 25f;
        public override float MaxScale => 1f;
        public override float MaxLaserLength => 2400f;
        public override bool Glowing => false;
        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("jarkos/textures/beam_start", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("jarkos/textures/beam_mid", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("jarkos/textures/beam_end", AssetRequestMode.ImmediateLoad).Value;
        public override bool UseFlowShader => true;
        public override bool UseGlowShader => true;
        public override void SetDefaults()
        {
             Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 2;
            Projectile.tileCollide = false;
            Projectile.timeLeft = (int)Lifetime;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawBeamWithColor(Color.Yellow * 1.1f, Projectile.scale * 1f);
            DrawBeamWithColor(Color.White * 1.1f, Projectile.scale * 0.5f);
            return false;
        }
        public override void DetermineScale() => Projectile.scale = Projectile.timeLeft / Lifetime * MaxScale;
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.95f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.NewProjectile(null, target.Center + new Vector2(Main.rand.NextFloat(-150, 150), Main.rand.NextFloat(-150, 150)), Main.rand.NextVector2CircularEdge(8f, 8f), ModContent.ProjectileType<Emp_magic_proj02>(), player.HeldItem.damage, 0, player.whoAmI);
            new Circular_glow(target.Center, Main.rand.NextVector2CircularEdge(1f, 2f), Color.Orange, 100, 1f, 2f, 0f).Spawn();
            LB_Penta.reset = true;
            Projectile.netUpdate = true;
        }
    }
}
