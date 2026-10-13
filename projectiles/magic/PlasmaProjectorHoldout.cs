using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.items.weapons.magic.toms;
using jarkos;

namespace jarkos.projectiles.magic
{
    public class PlasmaProjectorHoldout : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/magic/toms/Plasma_projector";

        public const int MAX_CHARGE_TIME = 180; 

        private ref float ChargeTimer => ref Projectile.ai[0];
        private ref float PlayedSoundFlag => ref Projectile.ai[1];

        private static BasicEffect basicEffect;

        private static BasicEffect GetBasicEffect(GraphicsDevice gd)
        {
            if (basicEffect == null || basicEffect.GraphicsDevice != gd)
            {
                basicEffect = new BasicEffect(gd)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = true
                };
            }
            return basicEffect;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            if (!player.channel)
            {
                ReleaseBarrage(player);
                Projectile.Kill();
                return;
            }

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 aimDir = Vector2.Normalize(Main.MouseWorld - player.Center);
                if (aimDir.HasNaNs()) aimDir = Vector2.UnitX * player.direction;

                Projectile.velocity = aimDir;
                Projectile.netUpdate = true;
                player.ChangeDir(Main.MouseWorld.X > player.Center.X ? 1 : -1);
            }

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            float rotation = Projectile.velocity.ToRotation();
            player.itemRotation = MathHelper.WrapAngle(rotation - (player.direction == -1 ? MathHelper.Pi : 0));
            Projectile.Center = player.Center + Projectile.velocity * 16f;
            Projectile.rotation = rotation;

            if (ChargeTimer < MAX_CHARGE_TIME)
            {
                ChargeTimer++;

                float chargeRatio = ChargeTimer / (float)MAX_CHARGE_TIME;

                if (ChargeTimer % 35 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.35f + chargeRatio * 0.35f, Pitch = 0.1f + chargeRatio * 0.7f }, player.Center);
                }

                if (Main.netMode != NetmodeID.Server && Main.rand.NextFloat() < 0.4f + chargeRatio * 0.4f)
                {
                    float maxConeAngle = MathHelper.ToRadians(50f);
                    float minConeAngle = MathHelper.ToRadians(6f);
                    float currentConeAngle = MathHelper.Lerp(maxConeAngle, minConeAngle, chargeRatio);

                    float randomAngle = rotation + Main.rand.NextFloat(-currentConeAngle * 0.5f, currentConeAngle * 0.5f);
                    float dist = Main.rand.NextFloat(150f, 400f);
                    Vector2 particleStart = player.Center + randomAngle.ToRotationVector2() * dist;
                    Vector2 particleVel = (player.Center - particleStart) * 0.08f;

                    Color cStart = Color.Cyan;
                    Color cEnd = Color.DeepSkyBlue;
                    new SpectralPlasmaParticle(particleStart, particleVel, cStart, cEnd, 16, Main.rand.NextFloat(0.3f, 0.6f), 0.8f).Spawn();
                }

                if (ChargeTimer >= MAX_CHARGE_TIME && PlayedSoundFlag == 0f)
                {
                    PlayedSoundFlag = 1f;
                    SoundEngine.PlaySound(SoundID.Item117 with { Volume = 0.9f, Pitch = 0.3f }, player.Center);
                    SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.7f, Pitch = 0.5f }, player.Center);

                    var modPlayer = player.GetModPlayer<Screenshake_system>();
                    if (modPlayer != null) modPlayer.weak_shake = 2.0f;

                    if (Main.netMode != NetmodeID.Server)
                    {
                        new SpectralRingParticle(player.Center, Color.Cyan, 20, 0.1f, 0.8f, 1f).Spawn();
                        for (int i = 0; i < 16; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                            new StarParticle(player.Center, vel, Color.White, 20, 0.3f, 0.9f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                        }
                    }
                }
            }

            Lighting.AddLight(player.Center, 0.2f, 0.7f, 0.9f);
        }

        private void ReleaseBarrage(Player player)
        {
            
            player.GetModPlayer<PlasmaProjectorPlayer>().cooldown = 30;
            player.itemTime = 30;
            player.itemAnimation = 30;
            player.reuseDelay = 30;

            
            if (ChargeTimer < 15f)
            {
                SoundEngine.PlaySound(SoundID.Item16 with { Volume = 0.5f, Pitch = 0.5f }, player.Center);
                if (Main.netMode != NetmodeID.Server)
                {
                    new SpectralRingParticle(player.Center, Color.Cyan * 0.5f, 10, 0.1f, 0.25f, 0.5f).Spawn();
                }
                return;
            }

            float chargeRatio = Math.Min(1f, ChargeTimer / (float)MAX_CHARGE_TIME);
            float chargePercent = chargeRatio;

            int count = chargePercent < 0.5f ? Main.rand.Next(2, 5) : Main.rand.Next(6, 9);

            float maxConeAngle = MathHelper.ToRadians(50f);
            float minConeAngle = MathHelper.ToRadians(6f);
            float currentConeAngle = MathHelper.Lerp(maxConeAngle, minConeAngle, chargeRatio);

            Vector2 baseVel = Projectile.velocity * 18f;

            for (int i = 0; i < count; i++)
            {
                float offsetAngle = Main.rand.NextFloat(-currentConeAngle * 0.5f, currentConeAngle * 0.5f);
                Vector2 knifeVel = baseVel.RotatedBy(offsetAngle) * Main.rand.NextFloat(0.85f, 1.15f);

                bool isEmpowered = (chargePercent >= 1.0f && (i == 0 || i == count - 1));
                int damage = Projectile.damage;
                if (chargePercent >= 0.5f) damage = (int)(damage * 1.25f);
                if (isEmpowered) damage = (int)(damage * 1.4f);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    player.Center + knifeVel * 1.2f,
                    knifeVel,
                    ModContent.ProjectileType<SpectralKnifeProj>(),
                    damage,
                    Projectile.knockBack,
                    player.whoAmI,
                    isEmpowered ? 1f : 0f
                );
            }

            SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.1f, 0.2f) }, player.Center);

            var modPlayer = player.GetModPlayer<Screenshake_system>();
            if (modPlayer != null) modPlayer.weak_shake = chargePercent >= 0.5f ? 2.5f : 1.2f;

            if (Main.netMode != NetmodeID.Server)
            {
                new SpectralRingParticle(player.Center, Color.Cyan, 16, 0.1f, 0.4f + chargePercent * 0.4f, 0.9f).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            GraphicsDevice gd = Main.instance.GraphicsDevice;

            float chargeRatio = Math.Min(1f, ChargeTimer / (float)MAX_CHARGE_TIME);
            float maxConeAngle = MathHelper.ToRadians(50f);
            float minConeAngle = MathHelper.ToRadians(6f);
            float currentConeAngle = MathHelper.Lerp(maxConeAngle, minConeAngle, chargeRatio);

            float baseAngle = Projectile.velocity.ToRotation();
            float halfAngle = currentConeAngle * 0.5f;

            int segments = 24;
            var vertices = new VertexPositionColorTexture[(segments + 1) * 2];

            Vector2 center = player.Center;
            float radius = 550f;

            Color centerColor = (ChargeTimer >= MAX_CHARGE_TIME)
                ? Color.Lerp(Color.Cyan, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.3f + 0.3f) * 0.75f
                : new Color(40, 220, 255) * (0.45f + chargeRatio * 0.35f);

            Color outerColor = Color.Transparent;

            Texture2D texture = TextureAssets.MagicPixel.Value;

            int vIndex = 0;
            for (int i = 0; i <= segments; i++)
            {
                float progress = i / (float)segments;
                float angle = baseAngle - halfAngle + currentConeAngle * progress;
                Vector2 outerPos = center + angle.ToRotationVector2() * radius;

                vertices[vIndex++] = new VertexPositionColorTexture(new Vector3(center, 0f), centerColor, new Vector2(0.5f, 0f));
                vertices[vIndex++] = new VertexPositionColorTexture(new Vector3(outerPos, 0f), outerColor, new Vector2(progress, 1f));
            }

            Main.spriteBatch.End();

            gd.BlendState = BlendState.Additive;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.DepthStencilState = DepthStencilState.None;
            gd.SamplerStates[0] = SamplerState.LinearClamp;

            Matrix wvp = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
                * Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            BasicEffect effect = GetBasicEffect(gd);
            effect.World = Matrix.Identity;
            effect.View = Matrix.Identity;
            effect.Projection = wvp;
            effect.Texture = texture;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
            }

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
