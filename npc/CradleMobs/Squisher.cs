using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.CradleMobs
{
    public class Squisher : ModNPC
    {
        public List<Item> StolenItems = new List<Item>();
        public ref float GlowTimer => ref NPC.ai[0];
        public ref float FleeTimer => ref NPC.ai[1];
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.TrailCacheLength[Type] = 4;
            NPCID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            NPC.width = 54;
            NPC.height = 42;
            NPC.damage = 0;
            NPC.defense = 8;
            NPC.lifeMax = 150;
            NPC.HitSound = new SoundStyle("jarkos/sounds/squealer_hurt") { PitchVariance = 0.3f };
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(silver: 3);
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }
        public override void AI()
        {
            GlowTimer += 0.05f;
            if (FleeTimer > 0)
            {
                FleeTimer--;
                Player player = Main.player[NPC.target];
                if (player.active && !player.dead)
                {
                    NPC.noTileCollide = false;
                    Vector2 direction = NPC.Center - player.Center;
                    if (direction.Length() > 0) direction.Normalize();
                    float speed = 7f;
                    float inertia = 15f;
                    NPC.velocity = (NPC.velocity * (inertia - 1) + direction * speed) / inertia;
                    if (NPC.collideX)
                    {
                        NPC.velocity.X = NPC.oldVelocity.X * -0.8f;
                        NPC.velocity.Y += NPC.Center.Y < player.Center.Y ? -2f : 2f;
                    }
                    if (NPC.collideY)
                    {
                        NPC.velocity.Y = NPC.oldVelocity.Y * -0.8f;
                        NPC.velocity.X += NPC.Center.X < player.Center.X ? -2f : 2f;
                    }
                }
            }
            else
            {
                NPC.noTileCollide = true;
            Item targetItem = null;
            float closestDistance = 800f;
            if (StolenItems.Count < 5)
            {
                for (int i = 0; i < Main.maxItems; i++)
                {
                    Item item = Main.item[i];
                    if (item.active && item.type != ItemID.None)
                    {
                        float dist = Vector2.Distance(NPC.Center, item.Center);
                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            targetItem = item;
                        }
                    }
                }
            }
            if (targetItem != null)
            {
                Vector2 direction = targetItem.Center - NPC.Center;
                float length = direction.Length();
                if (length > 0)
                {
                    direction.Normalize();
                }
                float speed = 5f;
                float inertia = 25f;
                NPC.velocity = (NPC.velocity * (inertia - 1) + direction * speed) / inertia;
                if (length < 40f)
                {
                    StolenItems.Add(targetItem.Clone());
                    targetItem.active = false;
                    SoundEngine.PlaySound(SoundID.Grab, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<SquisherLight>(), 0, 0f, Main.myPlayer, NPC.whoAmI, StolenItems.Count - 1);
                    }
                }
            }
            else
            {
                if (Main.rand.NextBool(40))
                {
                    Vector2 randomVel = Main.rand.NextVector2Circular(2f, 2f);
                    NPC.velocity += randomVel;
                }
                NPC.velocity *= 0.96f;
                if (NPC.position.X < 160f || NPC.position.X > (Main.maxTilesX * 16) - 160f)
                    NPC.velocity.X *= -1f;
                if (NPC.position.Y < 160f || NPC.position.Y > (Main.maxTilesY * 16) - 160f)
                    NPC.velocity.Y *= -1f;
            }
            }
            if (NPC.velocity.X > 0)
                NPC.spriteDirection = 1;
            else if (NPC.velocity.X < 0)
                NPC.spriteDirection = -1;
            Lighting.AddLight(NPC.Center, 0.4f, 0.1f, 0.5f);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            FleeTimer = 300;
        }
        public override void FindFrame(int frameHeight)
        {
            float speed = NPC.velocity.Length();
            int frameDelay = 12 - (int)(speed * 1.5f);
            if (frameDelay < 3) frameDelay = 3;
            NPC.frameCounter++;
            if (NPC.frameCounter >= frameDelay)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 6)
                {
                    NPC.frame.Y = 0;
                }
            }
        }
        public override void OnKill()
        {
            foreach (var item in StolenItems)
            {
                int itemIndex = Item.NewItem(NPC.GetSource_Death(), NPC.getRect(), item.type, item.stack, false, item.prefix);
                if (Main.netMode == NetmodeID.MultiplayerClient && itemIndex >= 0)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
                }
            }
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PurpleTorch, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), 100, default, 1.5f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, NPC.frame.Height / 2);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float pulse = (float)Math.Sin(GlowTimer) * 0.5f + 0.5f;
            Color glowColor = new Color(150, 50, 200, 0) * (0.3f + pulse * 0.7f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(3f + pulse * 3f, 0).RotatedBy(MathHelper.PiOver2 * i + GlowTimer);
                spriteBatch.Draw(texture, drawPos + offset, NPC.frame, glowColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            return false;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<cradle_biome.PetrifiedAbyssBiome>())
            {
                return 0.15f;
            }
            return 0f;
        }
    }
    public class SquisherLight : ModProjectile
    {
        public override string Texture => "jarkos/textures/MobsProjectile/Squisher_Proj";
        public int ParentIndex => (int)Projectile.ai[0];
        public int ItemIndex => (int)Projectile.ai[1];
        private float angleOffset;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.damage = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            NPC parent = Main.npc[ParentIndex];
            if (!parent.active || parent.type != ModContent.NPCType<Squisher>())
            {
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 2;
            if (angleOffset == 0)
            {
                angleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            float orbitSpeed = 0.05f;
            float orbitRadius = 45f;
            float angle = (Main.GlobalTimeWrappedHourly * orbitSpeed * 60f) + angleOffset + (ItemIndex * MathHelper.TwoPi / 5f);
            Vector2 targetPos = parent.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
            Projectile.velocity = (targetPos - Projectile.Center) * 0.15f;
            Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.5f);
            if (Main.rand.NextBool(5))
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.PurpleTorch, Vector2.Zero, 0, default, 1.2f).noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            NPC parent = Main.npc[ParentIndex];
            if (!parent.active) return false;
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Vector2 diff = parent.Center - Projectile.Center;
            float dist = diff.Length();
            float rot = diff.ToRotation();
            Main.EntitySpriteDraw(magicPixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), new Color(150, 50, 200, 100), rot, new Vector2(0, 0.5f), new Vector2(dist, 2f), SpriteEffects.None, 0);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, new Color(200, 100, 255, 0), Projectile.rotation, tex.Size() / 2, Projectile.scale * 1.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
