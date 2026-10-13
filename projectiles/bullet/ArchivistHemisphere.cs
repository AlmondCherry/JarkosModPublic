using System;
using jarkos.items.armor.archivist;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.bullet
{
    public class ArchivistHemisphere : ModProjectile
    {
        private int[] healCooldowns = new int[255];
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.damage = 150;
            Projectile.alpha = 60;
        }
        public override void AI()
        {
            for (int i = 0; i < healCooldowns.Length; i++)
            {
                if (healCooldowns[i] > 0) healCooldowns[i]--;
            }
            Projectile.velocity.Y += (float)Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI) * 0.02f;
            Projectile.velocity *= 0.98f;
            if (ArchivistSystem.IsGathering)
            {
                Vector2 dir = ArchivistSystem.GatherTarget - Projectile.Center;
                float dist = dir.Length();
                if (dist > 10f)
                {
                    dir.Normalize();
                    float speed = MathHelper.Clamp(dist * 0.05f, 2f, 20f);
                    Projectile.velocity = dir * speed;
                }
                else
                {
                    Projectile.velocity *= 0.5f;
                }
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (other.active && other.whoAmI != Projectile.whoAmI && other.type == Projectile.type && other.owner == Projectile.owner)
                {
                    Vector2 diff = Projectile.Center - other.Center;
                    float dist = diff.Length();
                    float minDist = 30f;
                    if (dist < minDist && dist > 0.01f)
                    {
                        diff.Normalize();
                        float pushForce = (minDist - dist) * 0.3f;
                        Projectile.velocity += diff * pushForce;
                        other.velocity -= diff * pushForce;
                    }
                }
            }
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead && p.Hitbox.Intersects(Projectile.Hitbox))
                {
                    if (healCooldowns[i] <= 0)
                    {
                        int healAmt = Main.rand.Next(2, 4);
                        p.Heal(healAmt);
                        healCooldowns[i] = 60;
                        for (int d = 0; d < 5; d++)
                        {
                            Dust.NewDust(p.Center + new Vector2(-10, -10), 20, 20, DustID.GreenTorch, 0f, -1f);
                        }
                    }
                }
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.Hitbox.Intersects(Projectile.Hitbox))
                {
                    npc.SimpleStrikeNPC(Projectile.damage, 0, true, 0, null, false, 0, true);
                    for (int d = 0; d < 20; d++)
                    {
                        Dust.NewDust(Projectile.Center, 0, 0, DustID.PurpleTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 150, Color.Purple, 1.5f);
                    }
                    SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
                    Projectile.Kill();
                    return;
                }
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.6f);
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, -1f, 100, default, 0.8f);
            }
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override bool CanHitPvp(Player target) => false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color color = Color.MediumPurple * 0.7f;
            float rotation = Main.GameUpdateCount * 0.02f + Projectile.whoAmI;
            Main.EntitySpriteDraw(tex, drawPos, null, color, rotation, tex.Size() / 2f, new Vector2(0.6f, 0.4f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, drawPos, null, Color.White * 0.4f, -rotation, tex.Size() / 2f, new Vector2(0.3f, 0.2f), SpriteEffects.None, 0);
            return false;
        }
    }
}
