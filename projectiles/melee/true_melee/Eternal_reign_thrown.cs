using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class Eternal_reign_thrown : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/Eternal_reign";

        private Vector2 aimDirection = Vector2.UnitX;


        private int State
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float Timer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(aimDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            aimDirection = reader.ReadVector2();
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }


            if (State == 0)
            {
                if (Timer == 0)
                {
                    if (Projectile.velocity != Vector2.Zero)
                    {
                        aimDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    }
                    else
                    {
                        aimDirection = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                    }
                    Projectile.velocity = Vector2.Zero;
                }

                Timer++;

                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                player.ChangeDir(aimDirection.X >= 0 ? 1 : -1);


                float windupProgress = MathHelper.Clamp(Timer / 12f, 0f, 1f);
                float easeWindup = (float)Math.Sin(windupProgress * MathHelper.PiOver2);


                Vector2 upOffset = new Vector2(0f, -24f * easeWindup);
                Vector2 backOffset = -aimDirection * (12f + easeWindup * 20f);
                Vector2 windupPos = player.MountedCenter + upOffset + backOffset;

                float baseAngle = aimDirection.ToRotation();
                float currentAngle = baseAngle - player.direction * MathHelper.ToRadians(125f * easeWindup);

                Projectile.Center = windupPos;
                Projectile.rotation = currentAngle + MathHelper.PiOver4;
                player.itemRotation = MathHelper.WrapAngle(currentAngle - (player.direction == -1 ? MathHelper.Pi : 0));

                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.CursedTorch, -aimDirection * 2f, 100, default, 1.2f);
                    d.noGravity = true;
                }


                if (Timer >= 12f)
                {
                    State = 1;
                    Timer = 0f;

                    if (Projectile.owner == Main.myPlayer)
                    {
                        aimDirection = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                    }
                    Projectile.velocity = aimDirection * 34f;
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.9f }, Projectile.Center);
                    Projectile.netUpdate = true;
                }
            }

            else if (State == 1)
            {
                Timer++;


                Projectile.velocity *= 0.92f;
                Projectile.rotation += 0.45f * player.direction;

                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.CursedTorch, Projectile.velocity * -0.2f, 100, default, 1.3f);
                    d.noGravity = true;
                }


                if (Projectile.velocity.Length() < 4f || Timer >= 30f)
                {
                    State = 2;
                    Timer = 0f;
                }
            }

            else if (State == 2)
            {
                Timer++;
                Projectile.tileCollide = false;
                Projectile.rotation += 0.55f * player.direction;

                Vector2 toPlayer = player.Center - Projectile.Center;
                float distToPlayer = toPlayer.Length();

                if (distToPlayer < 40f)
                {

                    player.AddBuff(ModContent.BuffType<buffs.buff.Eternal_reign_buff>(), 15 * 60);
                    int healAmount = 200;
                    player.statLife += healAmount;
                    player.HealEffect(healAmount);

                    SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.8f, Pitch = 0.3f }, player.Center);

                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                        Dust d = Dust.NewDustPerfect(player.Center, DustID.CursedTorch, vel, 100, default, 1.5f);
                        d.noGravity = true;
                    }

                    Projectile.Kill();
                    return;
                }


                float speed = MathHelper.Clamp(distToPlayer * 0.09f + 10f, 12f, 32f);
                Vector2 desiredVel = toPlayer.SafeNormalize(Vector2.Zero) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.14f);

                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, Projectile.velocity * -0.2f, 100, default, 1.1f);
                    d.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            target.AddBuff(ModContent.BuffType<buffs.debuff.Necrotization>(), 300);


            if (State < 2)
            {
                State = 2;
                Timer = 0f;
                SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.Center);

                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.CursedTorch, vel, 100, default, 1.4f);
                    d.noGravity = true;
                }

                Projectile.netUpdate = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;


            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (glowTex != null && State > 0)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Color greenGlow = new Color(50, 255, 80) * 0.6f;
                Main.spriteBatch.Draw(glowTex, drawPos, null, greenGlow, 0f, glowTex.Size() * 0.5f, Projectile.scale * 1.1f, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Main.spriteBatch.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
