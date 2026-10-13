using System.Collections.Generic;
using jarkos.items.materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent.UI;
using jarkos.systems;

namespace jarkos.npc.CradleMobs
{
	[AutoloadHead]
    public class ScavengerNPC : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 0;
            NPCID.Sets.AttackTime[Type] = 90;
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4;
        }
        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = NPCAIStyleID.Passive;
            NPC.damage = 20;
            NPC.defense = 30;
            NPC.lifeMax = 2500;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Guide;
        }
        private int targetPollenIndex = -1;
        private int throwItemTimer = 0;
        private int targetDevourerIndex = -1;
        public override bool PreAI()
        {
            if (string.IsNullOrEmpty(NPC.GivenName) || NPC.GivenName.Contains("Mods.jarkos") || NPC.GivenName.Contains("NPCNameText"))
            {
                NPC.GivenName = ScavengerShopLangSystem.NPCNameText.Value;
            }
            if (NPC.target >= 0 && NPC.target < Main.maxNPCs)
            {
                NPC targetNPC = Main.npc[NPC.target];
                if (targetNPC.type == ModContent.NPCType<DirtyPiggy>() && targetNPC.ai[0] != 3f)
                {
                    NPC.target = -1;
                }
            }
            if (targetPollenIndex != -1)
            {
                if (!Main.item[targetPollenIndex].active || Main.item[targetPollenIndex].type != ModContent.ItemType<WastefulnessPollen>())
                {
                    Player closest = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
                    if (closest.active && Vector2.Distance(closest.Center, NPC.Center) < 800f)
                    {
                        ScavengerSystem.IsAngryAtPlayer = true;
                        EmoteBubble.NewBubble(135, new WorldUIAnchor(NPC), 120);
                    }
                    targetPollenIndex = -1;
                }
            }
            if (targetPollenIndex == -1 && Main.rand.NextBool(30))
            {
                float closestDist = 800f;
                for (int i = 0; i < Main.maxItems; i++)
                {
                    Item item = Main.item[i];
                    if (item.active && item.type == ModContent.ItemType<WastefulnessPollen>())
                    {
                        float dist = Vector2.Distance(item.Center, NPC.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targetPollenIndex = i;
                        }
                    }
                }
            }
            if (targetPollenIndex != -1 && Main.item[targetPollenIndex].active)
            {
                Item pollen = Main.item[targetPollenIndex];
                Vector2 dir = pollen.Center - NPC.Center;
                if (dir.Length() > 20f)
                {
                    NPC.direction = dir.X > 0 ? 1 : -1;
                    NPC.velocity.X = NPC.direction * 2f;
                    if (NPC.velocity.X != 0f)
                        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
                    Vector2 toNPC = NPC.Center - pollen.Center;
                    if (toNPC != Vector2.Zero)
                    {
                        toNPC.Normalize();
                        pollen.velocity = Vector2.Lerp(pollen.velocity, toNPC * 4f, 0.1f);
                    }
                    return false;
                }
                else
                {
                    pollen.TurnToAir();
                    pollen.active = false;
                    SoundEngine.PlaySound(SoundID.Grab, NPC.Center);
                    targetPollenIndex = -1;
                    EmoteBubble.NewBubble(87, new WorldUIAnchor(NPC), 120);
                    return false;
                }
            }
            if (targetDevourerIndex == -1 && Main.rand.NextBool(60))
            {
                float closestDist = 600f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (n.active && n.type == ModContent.NPCType<DirtyPiggy>())
                    {
                        float dist = Vector2.Distance(n.Center, NPC.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targetDevourerIndex = i;
                        }
                    }
                }
            }
            if (targetDevourerIndex != -1)
            {
                NPC devourer = Main.npc[targetDevourerIndex];
                if (!devourer.active || devourer.type != ModContent.NPCType<DirtyPiggy>() || Vector2.Distance(devourer.Center, NPC.Center) > 800f || devourer.ai[0] == 3f)
                {
                    targetDevourerIndex = -1;
                }
                else
                {
                    Vector2 dir = devourer.Center - NPC.Center;
                    if (dir.Length() > 150f)
                    {
                        NPC.direction = dir.X > 0 ? 1 : -1;
                        NPC.velocity.X = NPC.direction * 1.5f;
                        if (NPC.velocity.X != 0f)
                            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
                    }
                    else
                    {
                        NPC.velocity.X *= 0.8f;
                        NPC.direction = dir.X > 0 ? 1 : -1;
                        throwItemTimer++;
                        if (throwItemTimer % 580 == 0)
                        {
                            Terraria.GameContent.UI.EmoteBubble.NewBubble(87, new Terraria.GameContent.UI.WorldUIAnchor(NPC), 120);
                            int type = Main.rand.Next(3) switch {
                                0 => ItemID.DirtBlock, 1 => ItemID.StoneBlock, _ => ItemID.Wood
                            };
                            int itemIndex = Item.NewItem(NPC.GetSource_FromAI(), NPC.Center, type, 1);
                            if (itemIndex >= 0 && itemIndex < Main.maxItems)
                            {
                                Main.item[itemIndex].velocity = new Vector2(NPC.direction * 5f, -3f);
                            }
                        }
                    }
                    return false;
                }
            }
            return true;
        }
        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            if (ScavengerSystem.UnlockedScavenger)
                return true;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && player.HasItem(ModContent.ItemType<WastefulnessPollen>()))
                {
                    ScavengerSystem.UnlockedScavenger = true;
                    return true;
                }
            }
            return false;
        }
        public override List<string> SetNPCNameList()
        {
            return new List<string> { jarkos.systems.ScavengerShopLangSystem.NPCNameText.Value };
        }
        public override string GetChat()
        {
            if (ScavengerSystem.IsAngryAtPlayer)
            {
                return Main.rand.NextBool()
                    ? jarkos.systems.ScavengerShopLangSystem.AngryDialog1Text.Value
                    : jarkos.systems.ScavengerShopLangSystem.AngryDialog2Text.Value;
            }
            int r = Main.rand.Next(3);
            switch (r)
            {
                case 0: return jarkos.systems.ScavengerShopLangSystem.Dialog1Text.Value;
                case 1: return jarkos.systems.ScavengerShopLangSystem.Dialog2Text.Value;
                default: return jarkos.systems.ScavengerShopLangSystem.Dialog3Text.Value;
            }
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = jarkos.systems.ScavengerShopLangSystem.EssencesButtonText.Value;
            button2 = jarkos.systems.ScavengerShopLangSystem.ChatButtonText.Value;
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                float baseChance = 10f;
                var rp = Main.LocalPlayer.GetModPlayer<jarkos.core.PlayerInteract.ResearchPlayer>();
                if (rp.IsCompleted("jarkos:AquaNode1"))
                {
                    baseChance += 5f;
                }
                if (Main.LocalPlayer.HasBuff(ModContent.BuffType<buffs.EtherTinctureBuff>()))
                {
                    baseChance *= 2.5f;
                }
                
                baseChance *= ModContent.GetInstance<Jarkos_config>().EssenceDropMultiplier;
                string chanceStr = baseChance.ToString("0.#");
                Main.npcChatText = jarkos.systems.ScavengerShopLangSystem.EssenceChanceDialogText.Format(chanceStr);
            }
            else
            {
                jarkos.ui.ScavengerUISystem.Instance.OpenUI();
            }
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 10;
            knockback = 4f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 120;
            randExtraCooldown = 60;
        }
        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<projectiles.ScavengerTentacleProj>();
            attackDelay = 1;
        }
        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 15f;
            randomOffset = 0f;
        }
    }
    public class ScavengerSystem : ModSystem
    {
        public static bool UnlockedScavenger = false;
        public static bool IsAngryAtPlayer = false;
        public override void OnWorldLoad()
        {
            UnlockedScavenger = false;
            IsAngryAtPlayer = false;
        }
        public override void OnWorldUnload()
        {
            UnlockedScavenger = false;
            IsAngryAtPlayer = false;
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (UnlockedScavenger) tag["UnlockedScavenger"] = true;
            if (IsAngryAtPlayer) tag["IsAngryAtPlayer"] = true;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            UnlockedScavenger = tag.ContainsKey("UnlockedScavenger");
            IsAngryAtPlayer = tag.ContainsKey("IsAngryAtPlayer");
        }
    }
    public class ScavengerPlayerUnlock : ModPlayer
    {
        public override void PostUpdate()
        {
            if (!ScavengerSystem.UnlockedScavenger && Player.HasItem(ModContent.ItemType<WastefulnessPollen>()))
            {
                ScavengerSystem.UnlockedScavenger = true;
                Main.NewText(jarkos.systems.ScavengerShopLangSystem.SpawnMessageText.Value, 255, 200, 50);
            }
        }
    }
}
