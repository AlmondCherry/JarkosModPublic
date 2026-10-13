using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class Kaliburn_VerticalRay : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; 

        private const int TOTAL_LIFETIME = 24;

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TOTAL_LIFETIME;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.type == NPCID.TheDestroyer || target.type == NPCID.TheDestroyerBody || target.type == NPCID.TheDestroyerTail)
            {
                modifiers.FinalDamage *= 0.25f;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                
                Projectile.rotation = Main.rand.NextFloat(-0.4f, 0.4f);
                Projectile.netUpdate = true;
            }
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            
            float t = 1f - (Projectile.timeLeft / (float)TOTAL_LIFETIME);
            float endOffset = MathHelper.Lerp(-800f, 1200f, t);
            Vector2 direction = new Vector2(0f, 1f).RotatedBy(Projectile.rotation);
            Vector2 currentTip = Projectile.Center + direction * endOffset;

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(currentTip, DustID.Torch, -direction * Main.rand.NextFloat(1f, 3f), 0, default, 1.5f);
                d.noGravity = true;
            }
        }

        private void GetSegmentOffsets(out float startOffset, out float endOffset)
        {
            float t = 1f - (Projectile.timeLeft / (float)TOTAL_LIFETIME);
            startOffset = MathHelper.Lerp(-1200f, 400f, t);
            endOffset = MathHelper.Lerp(-800f, 1200f, t);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            GetSegmentOffsets(out float startOffset, out float endOffset);
            Vector2 direction = new Vector2(0f, 1f).RotatedBy(Projectile.rotation);
            
            Vector2 start = Projectile.Center + direction * startOffset;
            Vector2 end = Projectile.Center + direction * endOffset;
            
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30f, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float t = 1f - (Projectile.timeLeft / (float)TOTAL_LIFETIME);
            
            
            float opacity = 1f;
            if (t < 0.15f)
                opacity = t / 0.15f;
            else if (t > 0.6f)
                opacity = (1f - t) / 0.4f;

            GetSegmentOffsets(out float startOffset, out float endOffset);
            Vector2 direction = new Vector2(0f, 1f).RotatedBy(Projectile.rotation);
            
            Vector2 start = Projectile.Center + direction * startOffset;
            Vector2 end = Projectile.Center + direction * endOffset;
            
            Color beamColor = Color.Lerp(Color.Red, Color.Orange, t);

            
            Effect effect = HealShaders.Stream;
            if (effect != null && HealShaders.HealNoise != null)
            {
                Main.spriteBatch.End();

                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 10f);
                effect.Parameters["uOpacity"]?.SetValue(opacity);
                effect.Parameters["uColor"]?.SetValue(beamColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(Color.White.ToVector4());

                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i <= 10; i++)
                {
                    points.Add(Vector2.Lerp(start, end, i / 10f));
                }

                
                TrailRenderer.Draw(effect, points.ToArray(), t => 28f * opacity * MathF.Sin(t * MathHelper.Pi), color => Color.White, HealShaders.HealNoise);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
