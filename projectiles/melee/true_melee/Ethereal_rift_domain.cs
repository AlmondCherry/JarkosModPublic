using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.ID;

namespace jarkos.projectiles.melee.true_melee
{
    public class Ethereal_rift_domain : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; 

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600; 
            Projectile.netImportant = true;
        }

        public float Radius
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void AI()
        {
            
            float maxRadius = 1900f;
            if (Radius < maxRadius)
            {
                Radius += (maxRadius - Radius) * 0.05f + 5f;
                if (Radius > maxRadius)
                    Radius = maxRadius;
            }

            if (Projectile.timeLeft % 60 == 0 && Main.myPlayer == Projectile.owner)
            {
                Main.NewText($"[DEBUG] Domain active! Radius: {Radius}");
            }

            
            for (int i = 0; i < 3; i++)
            {
                Vector2 borderOffset = Main.rand.NextVector2CircularEdge(Radius, Radius);
                Dust d = Dust.NewDustPerfect(Projectile.Center + borderOffset, DustID.BlueFairy, Vector2.Zero, 100, default, 1.5f);
                d.noGravity = true;
                d.velocity = borderOffset * 0.02f;
            }

            
            Projectile.ai[1]++;
            if (Projectile.ai[1] >= 20) 
            {
                Projectile.ai[1] = 0;
                if (Main.myPlayer == Projectile.owner)
                {
                    
                    Vector2 offset = Main.rand.NextVector2Circular(Radius * 0.8f, Radius * 0.8f);
                    Vector2 spawnPos = Projectile.Center + offset;

                    
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<Ethereal_ConstellationManager>(), Projectile.damage, Projectile.knockBack, Projectile.owner, -1f, 0f, 1f);
                }
            }
            
            if (!Main.dedServ)
            {
                if (!Terraria.Graphics.Effects.Filters.Scene["jarkos:EtherealDomain"].IsActive())
                {
                    Terraria.Graphics.Effects.Filters.Scene.Activate("jarkos:EtherealDomain");
                }
                Terraria.Graphics.Effects.Filters.Scene["jarkos:EtherealDomain"].GetShader()
                    .UseTargetPosition(Projectile.Center)
                    .UseOpacity(Radius);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false; 
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                Terraria.Graphics.Effects.Filters.Scene["jarkos:EtherealDomain"].Deactivate();
            }
        }
    }
}
