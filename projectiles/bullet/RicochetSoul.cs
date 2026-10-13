using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.systems;
using jarkos.particles.storm;
using System;
using System.IO;
using jarkos.particles;
namespace jarkos.projectiles.bullet
{
	public class RicochetSoul : ModProjectile
	{
		private enum SoulState
        {
            FlyingOut, Returning
        }
        private int TargetNPC
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float AngleOffset
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
		private float orbitAngle;
        private SoulState current = 0;
        private float EffectTimer = 0f;
        private const int FlyOutTime = 160;
        private const int ReturnTime = 80;
        private const float MaxDistance = 120f;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(EffectTimer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            EffectTimer = reader.ReadSingle();
        }
        public override void AI()
        {
            if(Projectile.timeLeft > 150)
            {
                current = SoulState.Returning;
            }
            else
            {
                current = SoulState.FlyingOut;
            }
            NPC target = Main.npc[TargetNPC];
            if (!target.active || target.life <= 0)
            {
                Projectile.Kill();
                return;
            }
            Lighting.AddLight(Projectile.Center, 1f, 1f, 0.3f);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowStarDust);
                dust.velocity = Vector2.Zero;
                dust.scale = 0.7f;
                dust.noGravity = true;
            }
            switch (current)
            {
                case SoulState.FlyingOut:
                    EffectTimer++;
                    FlyOut(target);
                    break;
                case SoulState.Returning:
                    EffectTimer--;
                    ReturnToTarget(target);
                    break;
            }
        }
        private void FlyOut(NPC target)
		{
			orbitAngle = AngleOffset + Main.GlobalTimeWrappedHourly * 4f;
			Vector2 offset = new Vector2((float)Math.Cos(orbitAngle) * EffectTimer, (float)Math.Sin(orbitAngle) * EffectTimer);
			Projectile.Center = target.Center + offset;
		}
        private void ReturnToTarget(NPC target)
		{
			orbitAngle = AngleOffset + Main.GlobalTimeWrappedHourly * 4f;
			Vector2 offset = new Vector2((float)Math.Cos(orbitAngle) * EffectTimer, (float)Math.Sin(orbitAngle) * EffectTimer);
			Projectile.Center = target.Center + offset;
		}
        private void Explode(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 5; i++)
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), Color.Yellow, 225, 0.75f, 1.5f, 0f).Spawn();
            }
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowStarDust);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.scale = 0.65f;
                dust.noGravity = true;
            }
            if (target.active && target.life > 0)
            {
                NPC.HitInfo hitInfo = new NPC.HitInfo
                {
                    Damage = Projectile.damage, Knockback = 0f, HitDirection = (target.Center.X < Projectile.Center.X) ? 1 : -1
                };
                target.StrikeNPC(hitInfo);
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    NetMessage.SendStrikeNPC(target, hitInfo);
                }
            }
            Projectile.Kill();
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.timeLeft < 150 && target.whoAmI == TargetNPC)
                return true;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode(target);
        }
    }
}
