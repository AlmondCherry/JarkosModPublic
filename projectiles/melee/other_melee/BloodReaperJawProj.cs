using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.particles;
using System;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;

namespace jarkos.projectiles.melee.other_melee
{
    public class BloodReaperJawProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 72;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.velocity = Main.MouseWorld;
                Projectile.netUpdate = true;
            }
        }
        private bool initialized = false;
        private Vector2 targetPosition;
        private int timer = 0;
        private const int attack_before = 35;
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1f, 0.1f, 0.1f);
            if (!initialized)
            {
                targetPosition = Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
                Player player = Main.player[Projectile.owner];
                Projectile.Center = player.Center;
                initialized = true;
            }
            timer++;
            if (Projectile.alpha > 0 && timer <= 24)
            {
                Projectile.alpha -= 12;
                if (Projectile.alpha < 0) Projectile.alpha = 0;
            }
            if (timer <= 24)
            {
                Player player = Main.player[Projectile.owner];
                float progress = timer / 24f;
                float easeOut = 1f - (float)Math.Pow(1f - progress, 3);
                Projectile.Center = Vector2.Lerp(player.Center, targetPosition, easeOut);
            }
            else
            {
                Projectile.Center = targetPosition;
            }
            if (timer < attack_before)
            {
                Projectile.frame = 0;
            }
            else if (timer == attack_before)
            {
                Projectile.frame = 1;
                SoundEngine.PlaySound(SoundID.NPCDeath13, Projectile.Center);
                float damageRadius = 120f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Distance(Projectile.Center) <= damageRadius)
                    {
                        Main.player[Projectile.owner].Heal(Main.rand.Next(1, 3));
                        NPC.HitInfo hitInfo = new NPC.HitInfo
                        {
                            Damage = Projectile.damage, Knockback = Projectile.knockBack, HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1
                        };
                        npc.StrikeNPC(hitInfo);
                    }
                }
                if (!Main.dedServ)
                {
                    new Circleedge(Projectile.Center, Vector2.Zero, Color.DarkRed, 60, 0.4f, 1f, 0f).Spawn();
                    new Circleedge(Projectile.Center, Vector2.Zero, Color.Red, 60, 0.8f, 0.7f, 0f).Spawn();
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2Circular(10f, 10f);
                        new Spark(Projectile.Center, sparkVel, Color.Red, 40, Main.rand.NextFloat(0.8f, 1.4f), 1f, 0f).Spawn();
                        new Spark(Projectile.Center, sparkVel * 0.5f, Color.DarkRed, 40, Main.rand.NextFloat(0.5f, 1.0f), 1f, 0f).Spawn();
                    }
                    for (int i = 0; i < 35; i++)
                    {
                        Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 0, 100, default, 1.6f);
                        d.velocity = Main.rand.NextVector2Circular(7f, 7f);
                        d.noGravity = false;
                    }
                }
            }
            else if (timer > attack_before)
            {
                Projectile.frame = 1;
                Projectile.alpha += 15;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Main.EntitySpriteDraw(texture, drawPos, sourceRectangle, lightColor * ((255 - Projectile.alpha) / 255f), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
