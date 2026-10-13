using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.npc.godofdeath
{
    public class Beam_hint : Laserbeam, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override float MaxScale => 2f;
        public override float MaxLaserLength => 600f;
        public override float Lifetime => 50f;
        public override bool Glowing => false;
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("Jarkos/projectiles/mecha/reflector_beam_start", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("Jarkos/projectiles/mecha/reflector_beam_mid", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("Jarkos/projectiles/mecha/reflector_beam_end", AssetRequestMode.ImmediateLoad).Value;
        public override string Texture => "Jarkos/projectiles/mecha/Reflector_beam";
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.tileCollide = false;
            Projectile.timeLeft = (int)Lifetime;
        }
        public override void DetermineScale() => Projectile.scale = Projectile.timeLeft / Lifetime * MaxScale;
        public override bool PreDraw(ref Color lightColor)
        {
            DrawBeamWithColor(Color.Red * 1.1f, Projectile.scale);
            DrawBeamWithColor(Color.White * 1.1f, Projectile.scale);
            return false;
        }
    }
}
