using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
    public class InfraatomicLine_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/Empty_proj";
        public Vector2 StartPos
        {
            get => new Vector2(Projectile.ai[0], Projectile.ai[1]);
            set
            {
                Projectile.ai[0] = value.X;
                Projectile.ai[1] = value.Y;
            }
        }
        public int TargetIndex
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private bool hasDealtDamage = false;
        private const int MaxTimeLeft = 15;
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }
        public override void AI()
        {
            if (!hasDealtDamage)
            {
                hasDealtDamage = true;
                int targetIdx = TargetIndex;
                if (targetIdx >= 0 && targetIdx < Main.maxNPCs)
                {
                    NPC target = Main.npc[targetIdx];
                    if (target.active && !target.friendly && !target.dontTakeDamage)
                    {
                        Player player = Main.player[Projectile.owner];
                        int direction = target.Center.X > StartPos.X ? 1 : -1;
                        bool crit = Main.rand.Next(100) < player.GetTotalCritChance(Projectile.DamageType);
                        target.SimpleStrikeNPC(Projectile.damage, direction, crit, Projectile.knockBack, Projectile.DamageType);
                        for (int i = 0; i < 6; i++)
                        {
                            Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                            Dust Spark = Dust.NewDustPerfect(target.Center, DustID.Torch, sparkVel, 0, new Color(255, 180, 50), Main.rand.NextFloat(0.8f, 1.3f));
                            Spark.noGravity = true;
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int targetIdx = TargetIndex;
            Vector2 endPos = StartPos;
            if (targetIdx >= 0 && targetIdx < Main.maxNPCs)
            {
                NPC target = Main.npc[targetIdx];
                if (target.active)
                {
                    endPos = target.Center;
                }
            }
            Vector2 direction = endPos - StartPos;
            float length = direction.Length();
            if (length <= 0) return false;
            float rotation = direction.ToRotation();
            float progress = Projectile.timeLeft / (float)MaxTimeLeft;
            float scaleY = progress * 6f;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 origin = new Vector2(0f, 0.5f);
            Color color = new Color(255, 120, 20) * progress * 0.9f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(pixel, StartPos - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, new Vector2(length, scaleY), SpriteEffects.None, 0f);
            Color coreColor = Color.White * progress;
            Main.spriteBatch.Draw(pixel, StartPos - Main.screenPosition, new Rectangle(0, 0, 1, 1), coreColor, rotation, origin, new Vector2(length, scaleY * 0.35f), SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
