using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class CollapsarBeam_proj : ModProjectile
    {
        private int TargetNPC
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float SearchTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private float LifeTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private const int MaxLife = 110;
        private const int HomingActivationDelay = 6;
        private const float MaxSpeed = 14f;
        private const float HomingStrength = 0.18f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = MaxLife;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            LifeTimer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            NPC current = GetTargetNPC();
            if (current == null && SearchTimer <= 0f)
            {
                current = FindClosestNPC();
                if (current != null)
                    TargetNPC = current.whoAmI;
                SearchTimer = 12f;
            }
            SearchTimer = Math.Max(0f, SearchTimer - 1f);
            if (LifeTimer > HomingActivationDelay && current != null)
            {
                Vector2 toTarget = current.Center - Projectile.Center;
                float dist = toTarget.Length();
                if (dist > 1f)
                {
                    Vector2 desired = toTarget / dist * MaxSpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingStrength);
                }
            }
            else
            {
                if (Projectile.velocity.LengthSquared() < MaxSpeed * MaxSpeed)
                    Projectile.velocity *= 1.04f;
            }
            float currentSpeed = Projectile.velocity.Length();
            if (currentSpeed > MaxSpeed)
                Projectile.velocity = Projectile.velocity / currentSpeed * MaxSpeed;
            if (Main.rand.NextBool(2))
            {
                Vector2 dv = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(3f), 6, 6, DustID.BlueFairy, dv.X, dv.Y, 120, default, 1.2f);
                d.noGravity = true;
                d.velocity *= 0.4f;
            }
            Lighting.AddLight(Projectile.Center, 0.5f, 0.8f, 1.1f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.45f }, target.Center);
            for (int i = 0; i < 10; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.BlueFairy, dv.X, dv.Y, 120, default, 1.4f);
                d.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueFairy, dv.X, dv.Y, 130, default, 1.1f);
                d.noGravity = true;
            }
        }
        private NPC GetTargetNPC()
        {
            int idx = TargetNPC;
            if (idx < 0 || idx >= Main.maxNPCs)
                return null;
            NPC n = Main.npc[idx];
            if (!n.active || n.friendly || n.dontTakeDamage || n.lifeMax <= 5 || n.CountsAsACritter)
                return null;
            if (n.Distance(Projectile.Center) > 2000f)
                return null;
            return n;
        }
        private NPC FindClosestNPC()
        {
            NPC best = null;
            float bestDist = 1500f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage || n.lifeMax <= 5 || n.CountsAsACritter)
                    continue;
                float d = n.Distance(Projectile.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = n;
                }
            }
            return best;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/CollapsarBeam_proj");
            Texture2D texture = tex.IsLoaded ? tex.Value : TextureAssets.MagicPixel.Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 p = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                if (p == -Main.screenPosition + Projectile.Size * 0.5f)
                    continue;
                float t = 1f - i / (float)Projectile.oldPos.Length;
                Color c = Color.Lerp(new Color(80, 140, 255, 0), new Color(200, 230, 255, 0), t) * t * t * 0.8f;
                float scale = Projectile.scale * (0.4f + t * 0.6f);
                Main.EntitySpriteDraw(texture, p, null, c, Projectile.rotation, origin, new Vector2(scale * 1.6f, scale * 0.45f), SpriteEffects.None, 0);
            }
            for (int i = 0; i < 3; i++)
            {
                Color glow = new Color(140, 200, 255, 0) * 0.55f;
                Main.EntitySpriteDraw(texture, drawPos, null, glow, Projectile.rotation, origin, new Vector2(Projectile.scale * (1.4f + i * 0.15f), Projectile.scale * 0.6f), SpriteEffects.None, 0);
            }
            Color head = new Color(230, 245, 255, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, head, Projectile.rotation, origin, new Vector2(Projectile.scale * 1.2f, Projectile.scale * 0.55f), SpriteEffects.None, 0);
            return false;
        }
    }
}
