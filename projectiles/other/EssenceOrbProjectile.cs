using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;

namespace jarkos.projectiles.other
{
    public class EssenceOrbProjectile : ModProjectile
    {
        public override string Texture => "jarkos/textures/Essences/Terra";

        public EssenceType OrbEssence
        {
            get => (EssenceType)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public int TargetPlayerIndex
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private float Timer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            Timer++;
            Color essenceColor = EssencePalette.ColorOf(OrbEssence);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror);
                dust.color = essenceColor;
                dust.velocity = Projectile.velocity * 0.5f;
                dust.noGravity = true;
                dust.scale = 1.2f;
            }
            Lighting.AddLight(Projectile.Center, essenceColor.ToVector3() * 0.5f);

            Player target = Main.player[TargetPlayerIndex];
            if (target == null || !target.active || target.dead)
            {
                Projectile.Kill();
                return;
            }

            ref float state = ref Projectile.ai[2];
            ref float chaseTimer = ref Projectile.localAI[1];

            if (state == 0)
            {
                Projectile.velocity *= 0.93f;
                chaseTimer++;
                if (chaseTimer >= 12f || Projectile.Distance(target.Center) < 500f)
                {
                    state = 1;
                    chaseTimer = 0f;
                }
            }
            else if (state == 1)
            {
                chaseTimer++;
                float homingSpeed = 14f + chaseTimer * 0.4f;
                Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * homingSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.18f);

                
                if (Projectile.Distance(target.Center) < 40f)
                {
                    Collect(target);
                }
            }
        }

        private void Collect(Player player)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                var rp = player.GetModPlayer<ResearchPlayer>();
                rp.AddEssence(OrbEssence, 1);

                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.EssenceOrbPickup);
                packet.Write((byte)OrbEssence);
                packet.Write((int)1);
                packet.Send(TargetPlayerIndex);
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                var rp = player.GetModPlayer<ResearchPlayer>();
                rp.AddEssence(OrbEssence, 1);
                int currentTotal = rp.GetEssence(OrbEssence);
                jarkos.systems.EssencePopupSystem.AddPopup(player, OrbEssence, currentTotal);
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0.8f }, player.Center);
                Color essenceColor = EssencePalette.ColorOf(OrbEssence);
                for (int i = 0; i < 15; i++)
                {
                    Dust d = Dust.NewDustDirect(player.Center, 0, 0, DustID.MagicMirror);
                    d.color = essenceColor;
                    d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                    d.noGravity = true;
                    d.scale = 1.5f;
                }
            }
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = EssencePalette.GetTexture(OrbEssence);
            if (texture == null) return false;
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Color color = EssencePalette.ColorOf(OrbEssence);
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                Vector2 startPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 endPos = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                float width = 5f * progress;
                Color trailColor = color * progress * 0.8f;
                Vector2 direction = endPos - startPos;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                Main.EntitySpriteDraw(magicPixel, startPos, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float baseScale = 0.55f;
            float pulse = baseScale + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow").Value;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            float glowScale = pulse * 1.5f;
            Main.EntitySpriteDraw(glowTex, drawPos, null, color * 0.5f, Projectile.rotation, glowTex.Size() / 2f, glowScale, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, texture.Size() / 2f, pulse, SpriteEffects.None, 0);
            return false;
        }
    }
}
