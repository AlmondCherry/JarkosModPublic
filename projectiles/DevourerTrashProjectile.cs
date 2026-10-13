using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;

namespace jarkos.projectiles
{
    public class DevourerTrashProjectile : ModProjectile
    {
        private static readonly int[] TrashItemTypes = 
        {
            ItemID.Wood, 
            ItemID.StoneBlock, 
            ItemID.DirtBlock, 
            ItemID.MudBlock, 
            ItemID.SandBlock, 
            ItemID.ClayBlock, 
            ItemID.GrayBrick, 
            ItemID.Rope, 
            ItemID.Bone, 
            ItemID.Cobweb, 
            ItemID.RottenChunk, 
            ItemID.Vertebrae, 
            ItemID.AntlionMandible, 
            ItemID.Gel, 
            ItemID.CopperOre, 
            ItemID.IronOre, 
            ItemID.SilverOre, 
            ItemID.LeadOre, 
            ItemID.WoodHelmet, 
            ItemID.EmptyBucket, 
            ItemID.BrokenHeroSword, 
            ItemID.Coral, 
            ItemID.Seashell, 
            ItemID.Lens, 
        };
        private int TrashIndex => (int)(Projectile.ai[0] % TrashItemTypes.Length);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.2f;
            Projectile.velocity.X *= 0.99f;
            Projectile.rotation += Projectile.velocity.X * 0.07f + 0.04f;
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.Next(3) 
                switch 
                {
                    0 => DustID.Dirt, 1 => DustID.Stone, _ => DustID.WoodFurniture
                };
                
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
                Main.dust[d].velocity *= 0.3f;
                Main.dust[d].scale = 0.5f + Main.rand.NextFloat() * 0.4f;
                Main.dust[d].noGravity = true;
                Main.dust[d].alpha = 150;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                int dustType = i % 3 == 0 ? DustID.Stone : i % 3 == 1 ? DustID.Dirt : DustID.WoodFurniture;
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
                Main.dust[d].velocity *= 2.5f;
                Main.dust[d].scale = 0.7f + Main.rand.NextFloat() * 0.6f;
                Main.dust[d].noGravity = false;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            int itemType = TrashItemTypes[TrashIndex];
            var texAsset = TextureAssets.Item[itemType];

            Texture2D tex = texAsset.IsLoaded ? texAsset.Value : null;
            if (tex == null) return true;

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                float alpha = progress * 0.35f;
                float scale = Projectile.scale * (0.4f + progress * 0.4f);
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = lightColor * alpha;
                trailColor.A = 0;

                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0f);
            }

            float glowPulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 15f) * 0.15f + 0.2f;
            Color glowColor = new Color(255, 80, 80) * glowPulse;
            glowColor.A = 0;

            spriteBatch.Draw(tex, pos, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, pos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }
    }
}
