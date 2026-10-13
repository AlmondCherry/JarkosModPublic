using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.buffs.debuff;
using Terraria.Audio;

namespace jarkos.items.accs.general
{
    public class SealOfShadow : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<jarkos.rare.StormRarity>();
            Item.value = Item.sellPrice(gold: 1);
            Item.defense = 4;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PulsePendantPlayer>().pendantEquipped = true;
            player.GetModPlayer<Jarkos_player>().seal_of_shadow = true;
        }
    }

    public class PulseCircle
    {
        public Vector2 Center;
        public float Radius;
        public float ExpansionSpeed;
        public HashSet<int> HitNPCs = new HashSet<int>();

        public const float MaxRadius = 300f;
        public bool IsDead => Radius >= MaxRadius;
        public float Progress => MathHelper.Clamp(Radius / MaxRadius, 0f, 1f);
        public float Opacity
        {
            get
            {
                float p = Progress;
                if (p < 0.08f) return p / 0.08f;
                if (p > 0.65f) return (1f - p) / 0.35f;
                return 1f;
            }
        }
    }

    public class PulsePendantPlayer : ModPlayer
    {
        public bool pendantEquipped;
        public List<PulseCircle> activeCircles = new List<PulseCircle>();
        private float spawnTimer = 0f;

        public override void ResetEffects()
        {
            pendantEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!pendantEquipped)
            {
                activeCircles.Clear();
                spawnTimer = 0f;
                return;
            }

            float currentSpeed = Player.velocity.Length();

            if (currentSpeed > 0.5f)
            {
                spawnTimer += 0.05f + currentSpeed * 0.05f;

                float threshold = MathHelper.Clamp(40f - currentSpeed * 2.5f, 10f, 40f);

                if (spawnTimer >= threshold)
                {
                    spawnTimer = 0f;

                    float expandSpeed = 15.5f + currentSpeed * 0.05f;

                    activeCircles.Add(new PulseCircle
                    {
                        Center = Player.Center,
                        Radius = 10f,
                        ExpansionSpeed = expandSpeed
                    });

                    for (int i = 0; i < 10; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Player.Center, DustID.PurpleCrystalShard);
                        dust.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
                        dust.scale = 1.1f;
                        dust.noGravity = true;
                    }

                    SoundEngine.PlaySound(SoundID.Item104 with { Volume = 0.35f, Pitch = 0.2f }, Player.Center);
                }
            }
            else
            {
                spawnTimer = Math.Max(0f, spawnTimer - 0.05f);
            }

            for (int i = activeCircles.Count - 1; i >= 0; i--)
            {
                var circle = activeCircles[i];

                circle.ExpansionSpeed = MathHelper.Lerp(circle.ExpansionSpeed, 15.5f + currentSpeed * 0.05f, 0.15f);
                circle.Radius += circle.ExpansionSpeed;

                if (Main.myPlayer == Player.whoAmI)
                {
                    CheckCircleCollisions(circle);
                }

                if (circle.IsDead)
                {
                    activeCircles.RemoveAt(i);
                }
            }
        }

        private void CheckCircleCollisions(PulseCircle circle)
        {
            float hitThickness = circle.ExpansionSpeed + 24f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.immortal || npc.dontTakeDamage || npc.boss)
                    continue;

                if (circle.HitNPCs.Contains(npc.whoAmI))
                    continue;

                float distance = Vector2.Distance(circle.Center, npc.Center);

                if (distance <= circle.Radius && distance >= circle.Radius - hitThickness)
                {
                    circle.HitNPCs.Add(npc.whoAmI);

                    int baseDamage = 18;
                    float speedBonus = 1f + (circle.Radius / PulseCircle.MaxRadius);
                    int damage = (int)(baseDamage * speedBonus * Player.GetDamage(DamageClass.Generic).Additive);

                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage = damage,
                        Knockback = 2f,
                        HitDirection = (npc.Center.X > circle.Center.X) ? 1 : -1,
                        Crit = false
                    };
                    
                    npc.StrikeNPC(hitInfo);
                    npc.AddBuff(ModContent.BuffType<PulseSlowDebuff>(), 180);
                    npc.AddBuff(ModContent.BuffType<Seal_of_shadow_debuff>(), 180);

                    for (int j = 0; j < 4; j++)
                    {
                        Dust dust = Dust.NewDustPerfect(npc.Center, DustID.PurpleTorch);
                        dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                        dust.scale = 1f;
                        dust.noGravity = true;
                    }
                }
            }
        }
    }

    public class PulseRendererSystem : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawNPCs += DrawPulses;
        }

        private void DrawPulses(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            orig(self, behindTiles);
            if (!behindTiles)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active)
                        continue;

                    PulsePendantPlayer pulsePlayer = player.GetModPlayer<PulsePendantPlayer>();
                    if (pulsePlayer != null && pulsePlayer.activeCircles.Count > 0)
                    {
                        DrawCircles(pulsePlayer.activeCircles);
                    }
                }
            }
        }

        private void DrawCircles(List<PulseCircle> circles)
        {
            if (circles.Count == 0) return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            int segments = 64;

            foreach (var circle in circles)
            {
                float radius = circle.Radius;
                float opacity = circle.Opacity;
                if (radius <= 0 || opacity <= 0.01f) continue;

                float thickness = MathHelper.Lerp(14f, 3f, circle.Progress);

                for (int i = 0; i < segments; i++)
                {
                    float angle = MathHelper.TwoPi * i / segments;
                    float nextAngle = MathHelper.TwoPi * (i + 1) / segments;

                    Vector2 pos = circle.Center + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
                    Vector2 nextPos = circle.Center + new Vector2(MathF.Cos(nextAngle) * radius, MathF.Sin(nextAngle) * radius);

                    Vector2 diff = nextPos - pos;
                    float segLen = diff.Length();
                    float segRot = diff.ToRotation();

                    float colorVar = MathF.Sin(angle * 4f + circle.Progress * 10f) * 0.2f;
                    Color ringColor = new Color((int)(160 + colorVar * 40), (int)(50 + colorVar * 30), 245);
                    float drawAlpha = opacity * 0.7f;

                    sb.Draw(pixel, pos - Main.screenPosition, new Rectangle(0, 0, 1, 1), ringColor * drawAlpha, segRot, new Vector2(0.5f, 0.5f), new Vector2(segLen + 1.2f, thickness), SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
