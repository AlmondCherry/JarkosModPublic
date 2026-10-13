using System;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent;
namespace jarkos.projectiles.melee.unique_melee
{
    public class KarmaCharm : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/KarmaBlade";
        private const float RADIUS = 14f;
        private const float SECTOR_ARC = MathHelper.Pi * 100f / 180f;
        private const int ARC_STEPS = 34;
        private float ringRot;
        private int prevCharges = -1;
        private int flashTimer;
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 2;
        }
        private int lastNetDashCharges = -1;
        private int lastNetDashRecharge = -1;

        public override void SendExtraAI(BinaryWriter writer)
        {
            Player player = Main.player[Projectile.owner];
            var modPlayer = player.GetModPlayer<KarmaBladePlayer>();
            writer.Write((byte)modPlayer.dashCharges);
            writer.Write((short)modPlayer.dashRecharge);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Player player = Main.player[Projectile.owner];
            var modPlayer = player.GetModPlayer<KarmaBladePlayer>();
            modPlayer.dashCharges = reader.ReadByte();
            modPlayer.dashRecharge = reader.ReadInt16();
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead
                || !player.HasItem(ModContent.ItemType<KarmaBlade>()))
            {
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 2;
            var modPlayer = player.GetModPlayer<KarmaBladePlayer>();

            if (Projectile.owner == Main.myPlayer)
            {
                if (modPlayer.dashCharges != lastNetDashCharges || Math.Abs(modPlayer.dashRecharge - lastNetDashRecharge) > 30)
                {
                    lastNetDashCharges = modPlayer.dashCharges;
                    lastNetDashRecharge = modPlayer.dashRecharge;
                    Projectile.netUpdate = true;
                }
            }

            float bob = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.1f) * 6f;
            float sway = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.3f) * 4f;
            Vector2 target = player.MountedCenter
                + new Vector2(-player.direction * 34f + sway, -42f + bob);
            if (Vector2.Distance(Projectile.Center, target) > 400f)
                Projectile.Center = target;
            else
                Projectile.Center = Vector2.Lerp(Projectile.Center, target, 0.09f);
            Projectile.velocity = Vector2.Zero;
            bool full = modPlayer.dashCharges >= KarmaBladePlayer.DASH_CHARGES_MAX;
            ringRot += full ? 0.022f : 0.012f;
            if (prevCharges >= 0 && modPlayer.dashCharges > prevCharges && Main.netMode != NetmodeID.Server)
            {
                flashTimer = 16;
                new Circleedge(Projectile.Center, Vector2.Zero, KarmaBladeSwing.ColorEdge, 16, 0.12f, 0.9f).Spawn();
            }
            if (prevCharges >= 0 && modPlayer.dashCharges < prevCharges && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 5; i++)
                    new Spark(Projectile.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.5f, 3.5f), KarmaBladeSwing.ColorMid, 16, Main.rand.NextFloat(0.2f, 0.35f), 0.8f).Spawn();
            }
            prevCharges = modPlayer.dashCharges;
            if (flashTimer > 0)
                flashTimer--;
            Lighting.AddLight(Projectile.Center, KarmaBladeSwing.ColorMid.ToVector3() * 0.25f);
            if (full && Main.rand.NextBool(40) && Main.netMode != NetmodeID.Server)
                new Spark(Projectile.Center, -Vector2.UnitY * Main.rand.NextFloat(0.5f, 1.2f), KarmaBladeSwing.ColorCore, 18, 0.22f, 0.8f).Spawn();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            var modPlayer = player.GetModPlayer<KarmaBladePlayer>();
            Vector2 center = Projectile.Center - Main.screenPosition;
            float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.4f);
            float flash = flashTimer / 16f;
            Texture2D px = TextureAssets.MagicPixel.Value;
            Rectangle src = new(0, 0, 1, 1);
            Vector2 origin = new(0.5f, 0.5f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            float step = SECTOR_ARC / ARC_STEPS;
            float segLen = RADIUS * step + 1.4f;
            for (int s = 0; s < 3; s++)
            {
                bool charged = s < modPlayer.dashCharges;
                bool charging = s == modPlayer.dashCharges
                    && modPlayer.dashCharges < KarmaBladePlayer.DASH_CHARGES_MAX;
                float sectorStart = ringRot + s * MathHelper.TwoPi / 3f - SECTOR_ARC * 0.5f;
                float progress = charging ? modPlayer.RechargeProgress : 0f;
                for (int k = 0; k < ARC_STEPS; k++)
                {
                    float frac = (k + 0.5f) / ARC_STEPS;
                    float angle = sectorStart + frac * SECTOR_ARC;
                    Vector2 pos = center + angle.ToRotationVector2() * RADIUS;
                    float rot = angle + MathHelper.PiOver2;
                    float edgeFade = MathHelper.Clamp(Math.Min(frac, 1f - frac) * ARC_STEPS / 2.5f, 0f, 1f);
                    if (charged)
                    {
                        float bright = (0.80f + 0.20f * pulse + flash * 0.5f) * edgeFade;
                        Main.spriteBatch.Draw(px, pos, src, KarmaBladeSwing.ColorMid * (0.28f * bright), rot, origin, new Vector2(6.5f, segLen + 2f), SpriteEffects.None, 0f);
                        Main.spriteBatch.Draw(px, pos, src, KarmaBladeSwing.ColorEdge * (0.95f * bright), rot, origin, new Vector2(2.8f, segLen), SpriteEffects.None, 0f);
                        Main.spriteBatch.Draw(px, pos, src, KarmaBladeSwing.ColorCore * (0.65f * bright), rot, origin, new Vector2(1.1f, segLen), SpriteEffects.None, 0f);
                    }
                    else if (charging && frac <= progress)
                    {
                        float bright = 0.85f * edgeFade;
                        Main.spriteBatch.Draw(px, pos, src, KarmaBladeSwing.ColorMid * (0.22f * bright), rot, origin, new Vector2(5f, segLen + 2f), SpriteEffects.None, 0f);
                        Main.spriteBatch.Draw(px, pos, src, KarmaBladeSwing.ColorMid * bright, rot, origin, new Vector2(2.4f, segLen), SpriteEffects.None, 0f);
                    }
                    else
                    {
                        Main.spriteBatch.Draw(px, pos, src, KarmaBladeSwing.ColorDeep * (0.50f * edgeFade), rot, origin, new Vector2(1.5f, segLen), SpriteEffects.None, 0f);
                    }
                }
                if (charging && progress > 0.02f)
                {
                    float tipAngle = sectorStart + progress * SECTOR_ARC;
                    Vector2 tipPos = center + tipAngle.ToRotationVector2() * RADIUS;
                    float tw = 2.6f + 1.2f * pulse;
                    Main.spriteBatch.Draw(px, tipPos, src, KarmaBladeSwing.ColorCore * 0.9f, tipAngle, origin, new Vector2(tw, tw), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(px, tipPos, src, KarmaBladeSwing.ColorEdge * 0.45f, tipAngle, origin, new Vector2(tw + 3.5f, tw + 3.5f), SpriteEffects.None, 0f);
                }
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
