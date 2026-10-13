using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using jarkos.projectiles;
using jarkos.items.materials;
using jarkos.systems;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using jarkos.cradle_biome;
using jarkos.items.critters;

namespace jarkos.npc.CradleMobs
{
    public class DirtyPiggy : ModNPC
    {
        private const int State_Passive = 0;
        private const int State_Chasing = 1;
        private const int State_Devouring = 2;
        private const int State_Enraged = 3;

        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.ai[1];
        public ref float AI_TargetItem => ref NPC.ai[2];
        public ref float AI_PollenCount => ref NPC.ai[3];

        private int _swallowedItemType = 0;
        private int _swallowedItemStack = 0;
        private Color _bodyColor = Color.White;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            Main.npcCatchable[NPC.type] = true;
        }
        public override void SetDefaults()
        {
            NPC.width = 102;
            NPC.height = 66;
            NPC.damage = 20;
            NPC.defense = 8;
            NPC.lifeMax = 240;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 70, 95);
            NPC.knockBackResist = 0.3f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.friendly = false;
            NPC.catchItem = ModContent.ItemType<DirtyPiggyItem>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<PetrifiedAbyssBiome>().Type };
        }
        public override void FindFrame(int frameHeight)
        {
            if ((int)AI_State == State_Devouring)
            {
                if (AI_Timer < 30)
                {
                    NPC.frame.Y = 4 * frameHeight;
                    NPC.frameCounter = 0;
                }
                else if (AI_Timer < 210)
                {
                    NPC.frameCounter++;
                    if (NPC.frameCounter > 10)
                    {
                        NPC.frameCounter = 0;
                        if (NPC.frame.Y == 5 * frameHeight)
                            NPC.frame.Y = 6 * frameHeight;
                        else
                            NPC.frame.Y = 5 * frameHeight;
                    }
                    if (NPC.frame.Y != 5 * frameHeight && NPC.frame.Y != 6 * frameHeight)
                    {
                        NPC.frame.Y = 5 * frameHeight;
                    }
                }
                else
                {
                    NPC.frame.Y = 7 * frameHeight;
                    NPC.frameCounter = 0;
                }
            }
            else
            {
                if (NPC.velocity.X == 0f)
                {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0;
                }
                else
                {
                    if (NPC.frame.Y >= 4 * frameHeight)
                    {
                        NPC.frame.Y = 0;
                    }
                    NPC.frameCounter += Math.Abs(NPC.velocity.X) * 0.2f + 0.1f;
                    if (NPC.frameCounter > 6.0)
                    {
                        NPC.frame.Y += frameHeight;
                        NPC.frameCounter = 0;
                    }
                    if (NPC.frame.Y >= 4 * frameHeight)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(LangSystem.ValueDevourer_desc.Value)
            });
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<PetrifiedAbyssBiome>())
                return SpawnCondition.Cavern.Chance * 0.005f;
            return 0f;
        }
        public override void AI()
        {
            NPC.localAI[0]++;
            switch ((int)AI_State)
            {
                case State_Passive: DoPassive(); break;
                case State_Chasing: DoChasing(); break;
                case State_Devouring: DoDevouring(); break;
                case State_Enraged: DoEnraged(); break;
            }

            bool isAiming = AI_State == State_Enraged && NPC.localAI[3] > 0;
            Color targetColor;

            if(AI_State == State_Devouring)
            {
                targetColor = Color.Lerp(Color.White, Color.LimeGreen, AI_Timer / 240f);
            }
            else if(isAiming)
            {
                targetColor = Color.Lerp(Color.Red, Color.OrangeRed, (float)Math.Sin(NPC.localAI[3] / 4f) * 0.5f + 0.5f);
            }
            else if(AI_State == State_Enraged)
            {
                targetColor = Color.Red;
            }
            else
            {
                targetColor = Color.White;
            }

            _bodyColor = Color.Lerp(_bodyColor, targetColor, 0.07f);
            NPC.color  = _bodyColor;
        }
        private void HandleWalking(float speed, int direction)
        {
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction * speed, 0.15f);
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            if (NPC.velocity.Y == 0f && NPC.collideX)
            {
                NPC.velocity.Y = -5f;
            }
        }
        private void DoPassive()
        {
            AI_Timer++;
            NPC.rotation = 0f;
            if (NPC.velocity.Y == 0f)
            {
                if (Main.rand.NextBool(150))
                {
                    NPC.direction *= -1;
                    if (NPC.direction == 0) NPC.direction = 1;
                }
                if (Main.rand.NextBool(180))
                {
                    NPC.localAI[0] = 60f + Main.rand.Next(60);
                }
                if (NPC.collideX)
                {
                    NPC.direction *= -1;
                    NPC.velocity.X = 0f;
                }
            }
            if (NPC.localAI[0] > 0)
            {
                NPC.localAI[0]--;
                NPC.velocity.X *= 0.8f;
            }
            else
            {
                if (NPC.direction == 0) NPC.direction = 1;
                HandleWalking(1f, NPC.direction);
            }

            NPC.spriteDirection = NPC.direction;

            if (AI_Timer % 30 == 0)
                ScanForItems();
        }
        private void DoChasing()
        {
            int idx = (int)AI_TargetItem;
            if (idx < 0 || idx >= Main.maxItems || !Main.item[idx].active)
            {
                AI_State = State_Passive;
                AI_Timer = 0;
                return;
            }

            NPC.rotation = 0f;
            Item target = Main.item[idx];
            Vector2 toItem = target.Center - NPC.Center;
            
            float dist = toItem.Length();
            float speed = 3.5f;

            int dir = toItem.X > 0 ? 1 : -1;
            NPC.direction = dir;
            NPC.spriteDirection = dir;

            if (Math.Abs(toItem.X) > 16f)
            {
                HandleWalking(speed, dir);
            }
            else
            {
                NPC.velocity.X *= 0.8f;
            }
            if (NPC.Hitbox.Intersects(target.Hitbox) || dist < 40f)
            {
                _swallowedItemType = target.type;
                _swallowedItemStack = target.stack;

                AI_PollenCount = CalcPollenAmount(target);
                target.active = false;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, idx);
                AI_State = State_Devouring;
                AI_Timer = 0;
            }
        }
        private void DoDevouring()
        {
            AI_Timer++;
            NPC.velocity.X *= 0.8f;
            NPC.rotation = 0f;
            NPC.localAI[1] = (float)Math.Sin(AI_Timer / 18f) * 0.06f;

            if (Main.rand.NextBool(4))
            {
                Color crumbColor = Main.rand.Next(2) switch 
                {
                    0 => new Color(255, 215, 0), 1 => new Color(100, 200, 255), _ => new Color(220, 180, 255)
                };
                int d = Dust.NewDust(NPC.Bottom - new Vector2(8, 8), 16, 8, DustID.TreasureSparkle);
                Main.dust[d].color = crumbColor;
                Main.dust[d].scale = 0.6f + Main.rand.NextFloat() * 0.4f;
                Main.dust[d].noGravity = false;
                Main.dust[d].velocity += new Vector2(Main.rand.NextFloat(-1f, 1f), -0.5f);
            }

            if (Main.rand.NextBool(8) && _swallowedItemType > 0)
            {
                Color rarityColor = GetRarityColor(_swallowedItemType);
                int d = Dust.NewDust(NPC.Center, 4, 4, DustID.MagicMirror);
                Main.dust[d].color = rarityColor;
                Main.dust[d].noGravity = true;
                Main.dust[d].scale  = 0.5f;
                Main.dust[d].velocity  = new Vector2(0, -1.5f);
            }

            if (AI_Timer >= 240)
            {
                SpawnPollen((int)AI_PollenCount);
                _swallowedItemType = 0;
                _swallowedItemStack = 0;
                AI_State = State_Passive;
                AI_Timer = 0;
                NPC.localAI[1] = 0f;
            }
        }
        private void DoEnraged()
        {
            AI_Timer++;
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            if (!player.active || player.dead)
            {
                AI_State = State_Passive;
                AI_Timer = 0;
                NPC.localAI[2] = 0;
                NPC.localAI[3] = 0;
                return;
            }

            Vector2 toPlayer = player.Center - NPC.Center;
            if (NPC.localAI[3] > 0)
            {
                NPC.localAI[3]++;
                NPC.velocity *= 0.85f;
                NPC.rotation = 0f;
                NPC.spriteDirection = toPlayer.X < 0 ? 1 : -1;

                if (Main.rand.NextBool(2))
                {
                    Vector2 spawnOffset = new Vector2(Main.rand.NextFloat(-30f, 30f), Main.rand.NextFloat(-30f, 30f));
                    Vector2 spawnPos = NPC.Center + spawnOffset;
                    Vector2 spawnVel = NPC.Center - spawnPos;

                    spawnVel.Normalize();
                    spawnVel *= 3f;

                    int d = Dust.NewDust(spawnPos, 1, 1, DustID.OrangeTorch);
                    Main.dust[d].velocity  = spawnVel;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].scale = 0.9f + Main.rand.NextFloat() * 0.5f;
                }

                if (NPC.localAI[3] >= 50 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.localAI[3] = 0;
                    NPC.localAI[2] = 0;
                    Vector2 shootDir = toPlayer;
                    shootDir.Normalize();
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 perturbedDir = shootDir.RotatedBy(i * 0.22f) * 17f;
                        int projIdx = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, perturbedDir, ModContent.ProjectileType<DevourerTrashProjectile>(), NPC.damage, 6f, Main.myPlayer);

                        if (projIdx >= 0 && projIdx < Main.maxProjectiles)
                            Main.projectile[projIdx].ai[0] = Main.rand.Next(5);
                    }
                    Vector2 kickback = -shootDir * 4f;
                    NPC.velocity = kickback;
                }
                return;
            }

            NPC.localAI[2]++;
            float speed = 4.5f;
            int dir = toPlayer.X > 0 ? 1 : -1;

            NPC.direction = dir;
            NPC.spriteDirection = dir;

            if (Math.Abs(toPlayer.X) > 60f)
            {
                HandleWalking(speed, dir);
            }
            else
            {
                NPC.velocity.X *= 0.8f;
            }
            if (NPC.localAI[2] >= 90)
                NPC.localAI[3] = 1;

            if (Main.rand.NextBool(3))
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
                Main.dust[d].scale = 0.8f + Main.rand.NextFloat() * 0.5f;
                Main.dust[d].noGravity = true;
            }
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            TryEnrage();
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            TryEnrage();
        }
        private void TryEnrage()
        {
            if ((int)AI_State != State_Devouring) return;
            if (_swallowedItemType > 0)
            {
                int dropped = Item.NewItem(NPC.GetSource_Loot(), (int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.height, _swallowedItemType, _swallowedItemStack);
                if (Main.netMode != NetmodeID.MultiplayerClient) 
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, dropped);
                _swallowedItemType = 0;
                _swallowedItemStack = 0;
            }

            AI_State = State_Enraged;
            AI_Timer = 0;
            NPC.localAI[1] = 0f;
            NPC.damage = 40;
            NPC.defense = 16;
            for (int i = 0; i < 20; i++)
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
                Main.dust[d].velocity *= 3f;
                Main.dust[d].scale = 1.2f;
            }
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return AI_State == State_Enraged;
        }
        public override bool CanHitNPC(NPC target)
        {
            return AI_State == State_Enraged;
        }
        public override void OnKill()
        {
            for (int i = 0; i < 25; i++)
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TreasureSparkle);
                Main.dust[d].velocity *= 3f + Main.rand.NextFloat();
                Main.dust[d].noGravity = false;
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WastefulnessPollen>(), 1, 5, 10));
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Vector2 pos = NPC.Center - screenPos;

            float pulseScale = 1f + NPC.localAI[1];
            Vector2 scale = new Vector2(1f, pulseScale);
            SpriteEffects fx = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (AI_State == State_Devouring)
            {
                float t = AI_Timer / 240f;
                Color glow = Color.Lerp(Color.Transparent, Color.LimeGreen * 0.4f, t);

                spriteBatch.Draw(tex, pos, NPC.frame, glow, NPC.rotation, NPC.frame.Size() / 2f, scale * NPC.scale * 1.08f, fx, 0f);
            }
            if (AI_State == State_Enraged && NPC.localAI[3] <= 0)
            {
                float flicker = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f) * 0.15f + 0.3f;
                Color rage = Color.Red * flicker;

                spriteBatch.Draw(tex, pos, NPC.frame, rage, NPC.rotation, NPC.frame.Size() / 2f, scale * NPC.scale * 1.1f, fx, 0f);
            }
            if (AI_State == State_Enraged && NPC.localAI[3] > 0)
            {
                float aimPulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 40f) * 0.25f + 0.5f;
                float aimScale = 1.05f + (float)Math.Sin(NPC.localAI[3] / 4f) * 0.07f;
                Color aimGlow = new Color(255, 120, 0) * aimPulse;

                spriteBatch.Draw(tex, pos, NPC.frame, aimGlow, NPC.rotation, NPC.frame.Size() / 2f, scale * NPC.scale * aimScale, fx, 0f);
            }
            spriteBatch.Draw(tex, pos, NPC.frame, _bodyColor * drawColor.ToVector4().W, NPC.rotation, NPC.frame.Size() / 2f, scale * NPC.scale, fx, 0f);
            return false;
        }
        private void ScanForItems()
        {
            float bestScore = 0f;
            int bestIdx = -1;
            float scanRadius  = 600f;

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active || item.IsAir) continue;
                if (item.type == ModContent.ItemType<WastefulnessPollen>()) continue;
                if (Vector2.Distance(NPC.Center, item.Center) > scanRadius) continue;

                float score = CalcPollenAmount(item);

                if (score > 0f && score > bestScore)
                {
                    bestScore = score;
                    bestIdx = i;
                }
            }
            if (bestIdx >= 0)
            {
                AI_State = State_Chasing;
                AI_TargetItem = bestIdx;
                AI_Timer = 0;
            }
        }
        private static float CalcPollenAmount(Item item)
        {
            float valuePts = item.value / 10000f;
            float rarityPts = item.rare switch 
            {
                ItemRarityID.White => 0, 
                ItemRarityID.Blue => 2, 
                ItemRarityID.Green => 3, 
                ItemRarityID.Orange => 5,
                ItemRarityID.LightRed => 7, 
                ItemRarityID.Pink => 8, 
                ItemRarityID.LightPurple => 9, 
                ItemRarityID.Lime => 10,
                ItemRarityID.Yellow => 11,
                ItemRarityID.Cyan => 12, 
                _ => 0
            };
            float craftPts = 0f;

            foreach (var recipe in Main.recipe)
            {
                if (recipe == null || recipe.createItem.IsAir) continue;
                if (recipe.createItem.type == item.type)
                {
                    craftPts += recipe.requiredItem.Count * 2f;
                    break;
                }
            }
            float total = valuePts + (rarityPts * 3f) + craftPts;

            if (item.type == ItemID.DirtBlock || item.type == ItemID.StoneBlock || item.type == ItemID.Wood)
                return Math.Min(9999f, 1f * item.stack);
            if (item.value < Item.sellPrice(0, 0, 50, 0) && item.rare <= ItemRarityID.White && craftPts < 4)
                return 0f;

            return Math.Min(9999f, Math.Max(1f, (float)Math.Ceiling(total)) * item.stack);
        }
        private static Color GetRarityColor(int itemType)
        {
            if (itemType <= 0) return Color.White;
            int rare = new Item { type = itemType }.rare;
            return rare switch {
                ItemRarityID.Blue => new Color(100, 180, 255), 
                ItemRarityID.Green => new Color(100, 255, 100), 
                ItemRarityID.Orange => new Color(255, 160, 50), 
                ItemRarityID.LightRed => new Color(255, 100, 100), 
                ItemRarityID.Pink => new Color(255, 150, 200), 
                ItemRarityID.LightPurple => new Color(200, 100, 255), 
                _ => new Color(255, 215, 0)
            };
        }
        private void SpawnPollen(int count)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            int dropped = Item.NewItem(NPC.GetSource_Loot(), (int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.height, ModContent.ItemType<WastefulnessPollen>(), Math.Max(1, count));
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, dropped);
            
            for (int i = 0; i < 30; i++)
            {
                int d = Dust.NewDust(NPC.Bottom - new Vector2(8, 4), 16, 4, DustID.TreasureSparkle);
                Main.dust[d].velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(1f, 4f));
                Main.dust[d].scale = 0.8f + Main.rand.NextFloat() * 0.6f;
                Main.dust[d].color = Color.Lerp(new Color(200, 255, 100), new Color(255, 200, 50), Main.rand.NextFloat());
                Main.dust[d].noGravity = false;
            }
        }
    }
}
