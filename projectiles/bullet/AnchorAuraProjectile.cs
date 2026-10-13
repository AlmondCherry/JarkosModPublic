using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
    public class AnchorAuraProjectile : ModProjectile
    {
        private float CurrentRadius
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.damage = 0;
            Projectile.hide = true;
        }
        public override void AI()
        {
            float maxRadius = 300f;
            float expandSpeed = 8f;
            
            if (CurrentRadius < maxRadius)
            {
                CurrentRadius += expandSpeed;
            }
            Projectile.Center = Main.player[Projectile.owner].Center;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.hostile && !p.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, p.Center);
                    if (dist <= CurrentRadius && dist >= CurrentRadius - 30f)
                    {
                        p.velocity *= -1f;
                        p.hostile = false;
                        p.friendly = true;
                        p.owner = Projectile.owner;
                        p.damage = (int)(p.damage * 1.5f);
                        p.netUpdate = true;

                        for (int d = 0; d < 5; d++)
                        {
                            Dust.NewDust(p.Center, 0, 0, DustID.Electric, 0f, 0f, 150, Color.Cyan);
                        }
                    }
                }
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= CurrentRadius && dist >= CurrentRadius - 30f)
                    {
                        Vector2 direction = npc.Center - Projectile.Center;
                        direction.Normalize();

                        float pushForce = npc.boss ? 5f : 20f;
                        npc.velocity += direction * pushForce;
                        npc.netUpdate = true;

                        if (Projectile.timeLeft % 10 == 0)
                        {
                            Player player = Main.player[Projectile.owner];
                            npc.SimpleStrikeNPC((int)player.GetDamage(DamageClass.Ranged).ApplyTo(50), 0, true);
                        }
                    }
                }
            }
            if (CurrentRadius > 0)
            {
                int dustCount = (int)(CurrentRadius / 10f);
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi / dustCount * i;
                    Vector2 offset = new Vector2(CurrentRadius, 0).RotatedBy(angle);
                    Vector2 dustPos = Projectile.Center + offset;

                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.BlueTorch, 0f, 0f, 0, Color.Cyan, 1.5f);
                    dust.noGravity = true;
                    dust.velocity *= 0f;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (CurrentRadius > 0)
            {
                Texture2D tex = TextureAssets.MagicPixel.Value;
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Cyan * 0.1f, 0f, new Vector2(0.5f, 0.5f), new Vector2(CurrentRadius * 2, CurrentRadius * 2), SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
