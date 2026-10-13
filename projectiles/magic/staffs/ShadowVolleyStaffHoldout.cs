using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

namespace jarkos.projectiles.magic.staffs
{
    public class ShadowVolleyStaffHoldout : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/magic/staffs/ShadowVolleyStaff";

        private const int CHARGE_TIME_MAX = 120;
        private ref float ChargeTimer => ref Projectile.ai[0];
        private ref float FireTimer => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || !player.channel || player.noItems || player.CCed)
            {
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
            Projectile.Center = player.Center + Projectile.velocity * 30f;
            Projectile.rotation = rotation + MathHelper.PiOver4;

            Vector2 staffTip = Projectile.Center + Projectile.velocity * 35f;

            if (ChargeTimer < CHARGE_TIME_MAX)
            {
                ChargeTimer++;


                float progress = ChargeTimer / (float)CHARGE_TIME_MAX;
                if (Main.rand.NextFloat() < progress * 0.8f + 0.2f)
                {
                    Vector2 particleStart = staffTip + Main.rand.NextVector2Circular(40f * (1f - progress * 0.5f), 40f * (1f - progress * 0.5f));
                    Vector2 particleVel = (staffTip - particleStart) * 0.12f;
                    Dust dust = Dust.NewDustPerfect(particleStart, DustID.Shadowflame, particleVel, 100, new Color(40, 10, 60), Main.rand.NextFloat(0.8f, 1.4f));
                    dust.noGravity = true;
                }

                if (ChargeTimer % 30 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.4f * progress, Pitch = 0.2f + progress * 0.5f }, staffTip);
                }

                if (ChargeTimer == CHARGE_TIME_MAX)
                {

                    SoundEngine.PlaySound(SoundID.Item117 with { Volume = 0.8f, Pitch = -0.2f }, staffTip);
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                        Dust dust = Dust.NewDustPerfect(staffTip, DustID.Shadowflame, vel, 100, new Color(20, 0, 40), Main.rand.NextFloat(1.2f, 2f));
                        dust.noGravity = true;
                    }
                }
            }
            else
            {

                FireTimer++;

                if (FireTimer % 5 == 0)
                {
                    if (Main.GameUpdateCount % 15 == 0 && Projectile.owner == Main.myPlayer)
                    {
                        if (!player.CheckMana(player.HeldItem, pay: true))
                        {
                            Projectile.Kill();
                            return;
                        }
                    }

                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 shootVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(18f)) * Main.rand.NextFloat(13f, 17f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            staffTip,
                            shootVel,
                            ModContent.ProjectileType<ShadowInkProj>(),
                            Projectile.damage,
                            Projectile.knockBack,
                            Projectile.owner
                        );
                    }

                    SoundEngine.PlaySound(SoundID.Item109 with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, staffTip);
                }
            }

            Lighting.AddLight(staffTip, 0.4f, 0.1f, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.spriteBatch.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);


            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            if (glowTex != null)
            {
                Vector2 staffTip = Projectile.Center + Projectile.velocity * 35f - Main.screenPosition;
                float progress = Math.Min(1f, ChargeTimer / (float)CHARGE_TIME_MAX);
                float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
                Color glowColor = new Color(80, 20, 120) * 0.7f * progress * pulse;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Main.spriteBatch.Draw(glowTex, staffTip, null, glowColor, 0f, glowTex.Size() * 0.5f, 0.45f * progress * pulse, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
