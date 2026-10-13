using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.ranged.bow
{
    public class BloodRainBeamProj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/Empty_proj";

        public float BeamLength
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 24;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 24;
        }

        public override void AI()
        {

            Vector2 beamDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);


            float maxRange = 500f;
            BeamLength = maxRange;

            for (float d = 0f; d <= maxRange; d += 6f)
            {
                Vector2 checkPos = Projectile.Center + beamDir * d;
                Point tileCoords = checkPos.ToTileCoordinates();
                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];


                if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && !TileID.Sets.Platforms[tile.TileType])
                {
                    BeamLength = d;
                    break;
                }
            }

            Vector2 endPos = Projectile.Center + beamDir * BeamLength;


            if (Projectile.timeLeft == 24)
            {
                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = -0.1f, Volume = 0.85f }, Projectile.Center);

                for (int i = 0; i < 14; i++)
                {
                    Vector2 vel = beamDir * Main.rand.NextFloat(2f, 8f) + Main.rand.NextVector2Circular(2f, 2f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, vel, 100, default, 1.4f);
                    d.noGravity = true;
                }
            }

            float lifeProgress = Projectile.timeLeft / 24f;


            if (Main.rand.NextBool(2))
            {
                float randDist = Main.rand.NextFloat(0f, BeamLength);
                Vector2 pPos = Projectile.Center + beamDir * randDist;
                Dust d = Dust.NewDustPerfect(pPos, DustID.Blood, beamDir * 2f + Main.rand.NextVector2Circular(1f, 1f), 100, default, 1.2f * lifeProgress);
                d.noGravity = true;
            }


            if (BeamLength < maxRange && Main.rand.NextBool(2))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 splashVel = -beamDir.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 5f);
                    Dust d = Dust.NewDustPerfect(endPos, DustID.Blood, splashVel, 100, default, 1.3f * lifeProgress);
                    d.noGravity = true;
                }
            }


            for (float d = 0f; d <= BeamLength; d += 90f)
            {
                Vector2 lightPos = Projectile.Center + beamDir * d;
                Lighting.AddLight(lightPos, new Vector3(0.9f, 0.1f, 0.15f) * lifeProgress);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float lifeProgress = Projectile.timeLeft / 24f;
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + Projectile.velocity.SafeNormalize(Vector2.UnitX) * BeamLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 16f * lifeProgress, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 beamDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float rotation = beamDir.ToRotation();
            Vector2 startPos = Projectile.Center - Main.screenPosition;


            float lifeProgress = Projectile.timeLeft / 24f;
            float alphaFade = (float)Math.Sin(lifeProgress * MathHelper.PiOver2);

            Vector2 origin = new Vector2(0f, 0.5f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


            Color outerGlowColor = new Color(200, 10, 20) * 0.45f * alphaFade;
            Main.spriteBatch.Draw(pixel, startPos, new Rectangle(0, 0, 1, 1), outerGlowColor, rotation, origin, new Vector2(BeamLength, 26f * alphaFade), SpriteEffects.None, 0f);


            Color redLayerColor = new Color(255, 30, 45) * 0.9f * alphaFade;
            Main.spriteBatch.Draw(pixel, startPos, new Rectangle(0, 0, 1, 1), redLayerColor, rotation, origin, new Vector2(BeamLength, 14f * alphaFade), SpriteEffects.None, 0f);


            Color whiteLayerColor = Color.White * 0.95f * alphaFade;
            Main.spriteBatch.Draw(pixel, startPos, new Rectangle(0, 0, 1, 1), whiteLayerColor, rotation, origin, new Vector2(BeamLength, 5f * alphaFade), SpriteEffects.None, 0f);


            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (glowTex != null)
            {
                Vector2 glowOrigin = glowTex.Size() * 0.5f;
                Vector2 endPos = (Projectile.Center + beamDir * BeamLength) - Main.screenPosition;

                Color flareColor = new Color(255, 80, 90) * alphaFade;
                Main.spriteBatch.Draw(glowTex, startPos, null, flareColor, 0f, glowOrigin, 0.35f * alphaFade, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glowTex, endPos, null, flareColor, 0f, glowOrigin, 0.45f * alphaFade, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
