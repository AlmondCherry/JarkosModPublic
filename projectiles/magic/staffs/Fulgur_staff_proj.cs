using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using ReLogic.Content;

namespace jarkos.projectiles.magic.staffs
{
    public class Fulgur_staff_proj : ModProjectile
    {
        private const float MAX_BEAM_LENGTH = 450f;
        private const int BASE_BEAM_WIDTH = 24;

        private float BeamLength
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float ChargeTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private float SoundTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || !player.channel || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            if (Main.GameUpdateCount % 15 == 0 && Projectile.owner == Main.myPlayer)
            {
                if (!player.CheckMana(player.HeldItem, pay: true))
                {
                    Projectile.Kill();
                    return;
                }
            }

            UpdatePlayerVisuals(player);
            UpdateBeamAiming(player);
            UpdateBeamLength();
            CreateBeamLighting();
            UpdateSound();
            ChargeTimer++;
        }

        private void UpdatePlayerVisuals(Player player)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                player.ChangeDir(Main.MouseWorld.X > player.Center.X ? 1 : -1);
            }
            else
            {
                player.ChangeDir(Projectile.velocity.X >= 0 ? 1 : -1);
            }

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            float rotation = Projectile.velocity.ToRotation();
            player.itemRotation = MathHelper.WrapAngle(rotation - (player.direction == -1 ? MathHelper.Pi : 0));
        }

        private void UpdateBeamAiming(Player player)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 targetDirection = Vector2.Normalize(Main.MouseWorld - player.Center);
                if (targetDirection.HasNaNs()) targetDirection = Vector2.UnitX * player.direction;

                float turnSpeed = 0.15f;
                Vector2 newVel = Vector2.Lerp(Projectile.velocity, targetDirection, turnSpeed);
                if (newVel != Vector2.Zero)
                    newVel.Normalize();

                if (Vector2.DistanceSquared(newVel, Projectile.velocity) > 0.0001f)
                {
                    Projectile.velocity = newVel;
                    Projectile.netUpdate = true;
                }
            }

            Projectile.Center = player.Center + Projectile.velocity * 50f;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        private void UpdateBeamLength()
        {
            Vector2 start = Projectile.Center;
            Vector2 direction = Projectile.velocity;
            float targetLength = MAX_BEAM_LENGTH;
            for (float dist = 0; dist < MAX_BEAM_LENGTH; dist += 8f)
            {
                Vector2 checkPos = start + direction * dist;
                Point tileCoords = checkPos.ToTileCoordinates();
                if (!WorldGen.InWorld(tileCoords.X, tileCoords.Y, 10))
                {
                    targetLength = dist;
                    break;
                }
                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    targetLength = dist;
                    break;
                }
            }
            BeamLength = MathHelper.Lerp(BeamLength, targetLength, 0.3f);
        }

        private void CreateBeamLighting()
        {
            float lightIntensity = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            Lighting.AddLight(Projectile.Center, 0.3f * lightIntensity, 0.6f * lightIntensity, 1f * lightIntensity);
            Vector2 endPoint = Projectile.Center + Projectile.velocity * BeamLength;
            Lighting.AddLight(endPoint, 0.4f * lightIntensity, 0.7f * lightIntensity, 1f * lightIntensity);
            for (int i = 0; i < 5; i++)
            {
                float dist = BeamLength * i / 5f;
                Vector2 lightPos = Projectile.Center + Projectile.velocity * dist;
                Lighting.AddLight(lightPos, 0.2f * lightIntensity, 0.4f * lightIntensity, 0.8f * lightIntensity);
            }
        }

        private void UpdateSound()
        {
            SoundTimer++;
            if (SoundTimer % 25 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with
                {
                    Volume = 0.4f,
                    Pitch = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f,
                    MaxInstances = 2
                }, Projectile.Center);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + Projectile.velocity * BeamLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, BASE_BEAM_WIDTH, ref point);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.CritDamage += 0.3f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            int particleCount = Main.rand.Next(1, 4);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 spawnPos = target.Center;
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                new Electric_particle(spawnPos, vel, new Color(50, 180, 255), Main.rand.Next(15, 25), Main.rand.NextFloat(0.2f, 0.6f), 1f, 0.92f).Spawn();
            }

            if (hit.Crit)
            {
                SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.8f, Pitch = 0.2f }, target.Center);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.NPCHit53 with { Volume = 0.6f }, target.Center);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawLaserBeam();
            return false;
        }

        private void DrawLaserBeam()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 start = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.velocity.ToRotation();

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


            float blueStartWidth = 16f;
            float blueEndWidth = 2f;
            DrawTaperedLine(sb, pixel, start, rotation, BeamLength, blueStartWidth, blueEndWidth, new Color(50, 180, 255) * 0.85f);


            float whiteStartWidth = 6f;
            float whiteEndWidth = 1f;
            DrawTaperedLine(sb, pixel, start, rotation, BeamLength, whiteStartWidth, whiteEndWidth, Color.White * 0.95f);


            DrawStaffTipGlow(sb, start);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawTaperedLine(SpriteBatch sb, Texture2D pixel, Vector2 start, float rotation, float length, float startWidth, float endWidth, Color color)
        {
            int segments = (int)(length / 8f);
            if (segments < 1) segments = 1;
            float segLen = length / segments;
            Vector2 dir = Projectile.velocity;

            for (int i = 0; i < segments; i++)
            {
                float progress = (float)i / segments;
                float currentWidth = MathHelper.Lerp(startWidth, endWidth, progress);
                Vector2 segPos = start + dir * (i * segLen);

                sb.Draw(pixel, segPos, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0f, 0.5f), new Vector2(segLen + 1f, currentWidth), SpriteEffects.None, 0f);
            }
        }

        private void DrawStaffTipGlow(SpriteBatch sb, Vector2 start)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            if (glowTex != null)
            {
                Vector2 glowOrigin = glowTex.Size() * 0.5f;
                float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
                Color glowColor = new Color(50, 180, 255) * 0.75f * pulse;
                float scale = 0.55f * pulse;
                sb.Draw(glowTex, start, null, glowColor, 0f, glowOrigin, scale, SpriteEffects.None, 0f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.5f, Pitch = 0.8f }, Projectile.Center);
        }
    }
}
