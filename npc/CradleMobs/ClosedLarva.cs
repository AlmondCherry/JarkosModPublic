using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.cradle_biome;
using jarkos.items.weapons.melee.other_melee;

namespace jarkos.npc.CradleMobs
{
    public class ClosedLarva : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
        }
        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 160;
            NPC.damage = 0;
            NPC.defense = 50;
            NPC.lifeMax = 1000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.aiStyle = -1;
            NPC.value = 0f;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<PetrifiedAbyssBiome>().Type };
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo) 
        {
            if (spawnInfo.Player.InModBiome<PetrifiedAbyssBiome>() && spawnInfo.Player.Center.Y > Main.rockLayer * 16f) 
            {
                return 0.001f;
            }
            return 0f;
        }
        public override bool? CanBeHitByItem(Player player, Item item) => true; 
        public override bool? CanBeHitByProjectile(Projectile projectile) => true; 
        
        public override void AI() 
        {
            NPC.velocity.X = 0f;

            Player player = Main.LocalPlayer;

            if (player.active && !player.dead) 
            {
                Vector2 mouseWorld = Main.MouseWorld;

                if (NPC.Hitbox.Contains((int)mouseWorld.X, (int)mouseWorld.Y) && player.Distance(NPC.Center) < 180f) 
                {
                    bool hasKnife = false;
                    int knifeType = ModContent.ItemType<NoxtiliumKnife>();

                    for (int i = 0; i < 10; i++) 
                    {
                        if (player.inventory[i].type == knifeType) 
                        {
                            hasKnife = true;
                            break;
                        }
                    }

                    if (hasKnife) 
                    {
                        player.cursorItemIconEnabled = true;
                        player.cursorItemIconID = knifeType;

                        if (Main.mouseRight && Main.mouseRightRelease) 
                        {
                            Main.mouseRightRelease = false;
                            SoundEngine.PlaySound(SoundID.Item71, NPC.Center);

                            NPC.StrikeInstantKill();
                        }
                    }
                }
            }
        }
        public override void OnKill() 
        {
            for (int i = 0; i < 35; i++) 
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Stone, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                d.scale = Main.rand.NextFloat(1.2f, 2.2f);
            }

            for (int i = 0; i < 25; i++) 
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PurpleTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.5f, 2.5f);
            }

            SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

            if (Main.netMode != NetmodeID.MultiplayerClient) 
            {
                int beetleType = ModContent.NPCType<Rhinobeetle>();
                int beetleIdx = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Bottom.Y - 45, beetleType);

                if (beetleIdx < Main.maxNPCs) 
                {
                    Main.npc[beetleIdx].netUpdate = true;
                }
            }
        }
    }
}
