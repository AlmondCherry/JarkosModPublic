using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.BloodMoon
{
    public class ConnectingGrave : ModNPC
    {
        private ref float SpawnTimer => ref NPC.ai[0];
        private ref float TickTimer => ref NPC.ai[1];
        private const float AuraRadius = 400f;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
        }
        public override void SetDefaults()
        {
            NPC.width = 68;
            NPC.height = 90;
            NPC.damage = 0;
            NPC.defense = 20;
            NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = Item.buyPrice(silver: 50);
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.lavaImmune = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.position.Y += NPC.height - 2;
            NPC.height = 2;
            SpawnTimer = 0;
        }
        public override void AI()
        {
            if (SpawnTimer < 60)
            {
                SpawnTimer++;
                float progress = SpawnTimer / 60f;
                float scaleY = 1f - (float)Math.Pow(1f - progress, 3);
                int targetHeight = 90;
                int newHeight = (int)(targetHeight * scaleY);
                if (newHeight < 2) newHeight = 2;
                int diff = newHeight - NPC.height;
                NPC.position.Y -= diff;
                NPC.height = newHeight;
                if (Main.rand.NextBool(3))
                {
                    Dust.NewDust(NPC.BottomLeft, NPC.width, 4, DustID.Smoke, 0, -2f);
                }
            }
            NPC.velocity.X = 0f;
            if (NPC.velocity.Y == 0f)
            {
                NPC.velocity.Y = 0f;
            }
            TickTimer++;
            if (SpawnTimer >= 60)
            {
                Lighting.AddLight(NPC.Center, 1f, 0.2f, 0.2f);
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.active && other.whoAmI != NPC.whoAmI && other.type != NPC.type && !other.friendly && !other.boss && other.lifeMax <= 10000 && !other.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(NPC.Center, other.Center);
                        if (dist <= AuraRadius)
                        {
                            other.AddBuff(ModContent.BuffType<MechaAuraBuff>(), 5);
                            if (TickTimer % 60 == 0)
                            {
                                int healAmount = (int)(other.lifeMax * 0.01f);
                                if (healAmount < 1) healAmount = 1;
                                if (other.life < other.lifeMax)
                                {
                                    other.life += healAmount;
                                    if (other.life > other.lifeMax)
                                    {
                                        other.life = other.lifeMax;
                                    }
                                    other.HealEffect(healAmount, true);
                                }
                            }
                            if (Main.rand.NextBool(120))
                            {
                                DrawLinkDust(NPC.Center, other.Center);
                            }
                        }
                    }
                }
            }
        }
        private void DrawLinkDust(Vector2 start, Vector2 end)
        {
            Vector2 diff = end - start;
            float dist = diff.Length();
            diff.Normalize();
            for (float i = 0; i < dist; i += 10f)
            {
                Dust dust = Dust.NewDustPerfect(start + diff * i, DustID.LifeDrain, Vector2.Zero, 100, default, 1f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (SpawnTimer >= 60)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                int segments = 120;
                float time = Main.GlobalTimeWrappedHourly * 4f;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = MathHelper.TwoPi * i / segments;
                    float angle2 = MathHelper.TwoPi * (i + 1) / segments;
                    float wave1 = (float)Math.Sin(angle1 * 6 + time) * 12f;
                    float wave2 = (float)Math.Sin(angle2 * 6 + time) * 12f;
                    Vector2 pos1 = NPC.Center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * (AuraRadius + wave1);
                    Vector2 pos2 = NPC.Center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * (AuraRadius + wave2);
                    Vector2 diff = pos2 - pos1;
                    float rot = diff.ToRotation();
                    float len = diff.Length();
                    spriteBatch.Draw(pixel, pos1 - screenPos, new Rectangle(0, 0, 1, 1), new Color(255, 50, 50, 0), rot, new Vector2(0, 0.5f), new Vector2(len, 4f), SpriteEffects.None, 0f);
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            }
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height);
            Vector2 drawPos = NPC.Bottom - screenPos;
            float progress = Math.Min(SpawnTimer / 60f, 1f);
            float scaleY = 1f - (float)Math.Pow(1f - progress, 3);
            float scaleX = 1f;
            if (SpawnTimer >= 60)
            {
                scaleX = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.05f;
                scaleY = 1f - (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.05f;
            }
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, origin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            return false;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Iron, hit.HitDirection, -1f, 0, default, 1.2f);
            }
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && spawnInfo.SpawnTileY < Main.worldSurface)
            {
                return 0.01f;
            }
            return 0f;
        }
    }
    public class MechaAuraBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_14";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
    public class MechaAuraGlobalNPC : GlobalNPC
    {
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<MechaAuraBuff>()))
            {
                modifiers.Defense += 50;
            }
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<MechaAuraBuff>()))
            {
                modifiers.Defense += 50;
            }
        }
    }
}
