using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	public class AlephOne_Proj : ModProjectile 
	{
		public override void SetStaticDefaults() 
		{
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = true;
			Projectile.scale = 1f;
			Projectile.timeLeft = 300;
			Projectile.light = 0.95f;
		}

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/AlephOneHit") with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.4f, 0.4f) }, Projectile.Center);
            float size = Main.rand.NextFloat(0.75f, 0.85f);
            new Circleedge_reducing(target.Center, Vector2.Zero, Color.MediumPurple, 300, size * 3f, 2f, 0f).Spawn();
			new StarParticleBigger(target.Center, Vector2.Zero, Color.HotPink, 100, size / 2f, 0.75f, 0.1f).Spawn();


            if (Projectile.owner == Main.myPlayer && damageDone > 0)
            {
                int totalDotDamage = (int)(damageDone * 0.40f);
                if (totalDotDamage < 1) totalDotDamage = 1;

                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<AlephOneDotProj>(),
                    totalDotDamage,
                    0f,
                    Projectile.owner,
                    ai0: target.whoAmI,
                    ai1: totalDotDamage
                );
            }
        }

        public override void AI() 
		{
			Player player = Main.player[Projectile.owner];
			Projectile.direction = player.direction;
			Projectile.velocity *= .9998f;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
		}

		public override bool PreDraw(ref Color lightColor) 
		{
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) 
			{
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Main.EntitySpriteDraw(texture, drawPos, null, Color.HotPink, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}
			return true;
		}
	}

    public class AlephOneDotProj : ModProjectile
    {
        public override string Texture => "jarkos/particles/Stretching_particle";

        private int TargetWhoAmI => (int)Projectile.ai[0];
        private int TotalDamage => (int)Projectile.ai[1];
        private int tickCounter = 0;
        private int damageDealtSoFar = 0;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
        }

        public override void AI()
        {
            NPC target = Main.npc[TargetWhoAmI];
            if (!target.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = target.Center;
            tickCounter++;


            if (tickCounter % 12 == 0)
            {
                int ticksRemaining = (60 - tickCounter) / 12 + 1;
                int remainingDamage = TotalDamage - damageDealtSoFar;
                int damageThisTick = Math.Max(1, remainingDamage / ticksRemaining);

                damageDealtSoFar += damageThisTick;

                if (Projectile.owner == Main.myPlayer)
                {
                    target.SimpleStrikeNPC(damageThisTick, 0, false, 0f, damageType: DamageClass.Melee);
                }


                float size = Main.rand.NextFloat(0.4f, 0.6f);

                for(int i = 0; i < 3; i++)
                new Spark(target.Center + Main.rand.NextVector2Circular(target.width * 0.1f, target.height * 0.1f), Main.rand.NextVector2Circular(7f, 7f), Color.Pink, 60, size, 0.6f, 0.1f).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
