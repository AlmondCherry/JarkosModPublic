using System;
using jarkos.items.materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
    public class Autodafe : ModItem
    {
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.width = 30;
            Item.height = 30;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            AutodafePlayer modPlayer = player.GetModPlayer<AutodafePlayer>();
            modPlayer.HasAutodafe = true;
            modPlayer.HideAutodafeVisual = hideVisual;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<ThickSilk>(), 8)
			.AddIngredient(ItemID.SoulofLight, 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
    public class AutodafeWeaknessNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public const int MaxStacks = 30;
        public const float SlowPerStack = 0.02f;
        public const float DamageBonusPerStack = 0.1f;

        public int Stacks;
        private int _auraCountAccumulator;

        public void RegisterAuraTouch(NPC npc)
        {
            _auraCountAccumulator++;
            npc.AddBuff(ModContent.BuffType<buffs.debuff.AutodafeDebuff>(), 10);
        }

        public override void ResetEffects(NPC npc)
        {
            if (_auraCountAccumulator > 0)
            {
                Stacks = Math.Min(_auraCountAccumulator, MaxStacks);
                _auraCountAccumulator = 0;
            }
            else if (!npc.HasBuff(ModContent.BuffType<buffs.debuff.AutodafeDebuff>()))
            {
                Stacks = 0;
            }
        }

        public override void PostAI(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<buffs.debuff.AutodafeDebuff>()))
            {
                Stacks = 0;
                return;
            }

            if (Stacks > 0)
            {
                float slow = MathHelper.Clamp(Stacks * SlowPerStack, 0f, 0.95f);
                npc.velocity *= 1f - slow;
            }
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (Stacks > 0)
            {
                modifiers.FinalDamage *= 1f + Stacks * DamageBonusPerStack;
            }
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (Stacks > 0)
            {
                float t = MathHelper.Clamp(Stacks / (float)MaxStacks, 0f, 1f);
                drawColor.R = (byte)(drawColor.R * (1f - t * 0.35f));
                drawColor.G = (byte)(drawColor.G * (1f - t * 0.55f));
            }
        }
    }
    public class AutodafePlayer : ModPlayer
    {
        public bool HasAutodafe;
        public bool HideAutodafeVisual;
        public override void ResetEffects()
        {
            HasAutodafe = false;
            HideAutodafeVisual = false;
        }
    }
    public class AutodafeAura : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public const float AuraRadius = 240f;
        public const int VisualDustInterval = 4;
        private int _dustTimer;
        private float _auraTimer;

        public override void PostAI(Projectile projectile)
        {
            if (!projectile.minion || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            {
                return;
            }
            Player owner = Main.player[projectile.owner];
            if (!owner.active || owner.dead)
            {
                return;
            }
            AutodafePlayer modPlayer = owner.GetModPlayer<AutodafePlayer>();
            if (!modPlayer.HasAutodafe)
            {
                return;
            }

            _auraTimer += 1f;

            if (owner.whoAmI == Main.myPlayer && !modPlayer.HideAutodafeVisual)
            {
                _dustTimer++;
                if (_dustTimer >= VisualDustInterval)
                {
                    _dustTimer = 0;
                    Vector2 dir = Main.rand.NextVector2Unit();
                    Vector2 pos = projectile.Center + dir * AuraRadius;
                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, dir * 0.6f, Scale: Main.rand.NextFloat(0.9f, 1.4f));
                    d.noGravity = true;
                    d.fadeIn = 0.4f;
                }
            }

            float r2 = AuraRadius * AuraRadius;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage || n.lifeMax <= 5 || n.immortal)
                {
                    continue;
                }
                if (Vector2.DistanceSquared(n.Center, projectile.Center) > r2)
                {
                    continue;
                }
                n.GetGlobalNPC<AutodafeWeaknessNPC>().RegisterAuraTouch(n);
            }
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            if (!projectile.minion || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            {
                return;
            }
            Player owner = Main.player[projectile.owner];
            if (!owner.active || owner.dead)
            {
                return;
            }
            AutodafePlayer modPlayer = owner.GetModPlayer<AutodafePlayer>();
            if (!modPlayer.HasAutodafe || modPlayer.HideAutodafeVisual)
            {
                return;
            }

            Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/circle_edge_texture").Value;
            if (texture == null) return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float texRadius = texture.Width / 2f;
            float targetMaxScale = AuraRadius / texRadius;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float period = 75f;
            int waveCount = 3;

            for (int k = 0; k < waveCount; k++)
            {
                float progress = (_auraTimer + k * (period / waveCount)) % period / period;

                float currentScale = MathHelper.Lerp(0.02f, targetMaxScale, progress);

                float fadeIn = MathHelper.Clamp(progress * 5f, 0f, 1f);
                float fadeOut = 1f - progress;
                float alpha = fadeIn * fadeOut;

                Color purpleColor = new Color(170, 60, 255) * (alpha * 0.65f);
                Color coreColor = new Color(220, 120, 255) * (alpha * 0.35f);

                float rotation = _auraTimer * 0.015f + k * 1.047f;

                Main.spriteBatch.Draw(texture, drawPos, null, purpleColor, rotation, origin, currentScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(texture, drawPos, null, coreColor, -rotation, origin, currentScale * 0.96f, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
