using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using jarkos.systems;
using Terraria.GameContent.Bestiary;

namespace jarkos.npc.CradleMobs
{
    public class Foulmouthed_frog : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.TrailCacheLength[NPC.type] = 4;
        }
        public override void SetDefaults()
        {
            NPC.width = 66;
            NPC.height = 72;
            NPC.damage = 0;
            NPC.defense = 9;
            NPC.lifeMax = 320;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 75, 95);
            NPC.knockBackResist = 0.5f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<cradle_biome.PetrifiedAbyssBiome>().Type };
            NPC.aiStyle = -1;
            NPC.friendly = false;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(LangSystem.Foulmouthed_frog_desc.Value)
            });
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<cradle_biome.PetrifiedAbyssBiome>())
                return SpawnCondition.Cavern.Chance * 0.05f;
            return 0f;
        }
        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.ai[1];
        private const int State_Passive = 0;
        private const int State_Aggressive = 1;
        public override void AI()
        {
            AI_Timer++;
            if (Main.rand.NextBool(600))
            {
                if (FrogLangSystem.Croaks != null && FrogLangSystem.Croaks.Length > 0)
                {
                    string croak = FrogLangSystem.Croaks[Main.rand.Next(FrogLangSystem.Croaks.Length)].Value;
                    CombatText.NewText(NPC.getRect(), Color.LightGreen, croak);
                }
            }
            if (AI_State == State_Passive)
            {
                NPC.TargetClosest(true);
                NPC.damage = 0;
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity.X *= 0.8f;
                    if (AI_Timer > 120 + Main.rand.Next(120))
                    {
                        AI_Timer = 0;
                        NPC.velocity.Y = -8f - Main.rand.NextFloat(4f);
                        NPC.velocity.X = NPC.direction * (3f + Main.rand.NextFloat(3f));
                    }
                }
            }
            else if (AI_State == State_Aggressive)
            {
                NPC.TargetClosest(true);
                NPC.damage = 30;
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity.X *= 0.8f;
                    if (AI_Timer > 60)
                    {
                        AI_Timer = 0;
                        Player target = Main.player[NPC.target];
                        Vector2 distance = target.Center - NPC.Center;
                        NPC.direction = distance.X > 0 ? 1 : -1;
                        NPC.velocity.Y = -10f - Main.rand.NextFloat(3f);
                        NPC.velocity.X = NPC.direction * (4f + Math.Min(Math.Abs(distance.X) * 0.05f, 6f));
                    }
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (NPC.velocity.Y == 0f)
            {
                NPC.frame.Y = 0;
            }
            else if (NPC.velocity.Y < 0f)
            {
                NPC.frame.Y = 1 * frameHeight;
            }
            else
            {
                NPC.frame.Y = 2 * frameHeight;
            }
            NPC.spriteDirection = NPC.direction;
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            HitReaction();
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            HitReaction();
        }
        private void HitReaction()
        {
            if (AI_State == State_Passive)
            {
                AI_State = State_Aggressive;
                AI_Timer = 60;
            }
            if (FrogLangSystem.Swears != null && FrogLangSystem.Swears.Length > 0)
            {
                if (Main.rand.NextBool(2))
                {
                    string swear = FrogLangSystem.Swears[Main.rand.Next(FrogLangSystem.Swears.Length)].Value;
                    CombatText.NewText(NPC.getRect(), Color.Red, swear);
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.frame.Height * 0.5f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4, 0).RotatedBy(Main.GlobalTimeWrappedHourly * 2f + MathHelper.TwoPi / 4 * i);
                Color auraColor = new Color(100, 255, 100, 0) * 0.5f;
                spriteBatch.Draw(texture, NPC.Center - screenPos + offset, NPC.frame, auraColor, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            return false;
        }
    }
}
