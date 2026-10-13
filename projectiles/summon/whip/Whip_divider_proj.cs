using jarkos.buffs.summonbuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.summon.whip
{
	public class Whip_divider_proj : ModProjectile
	{
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 25;
            Projectile.WhipSettings.RangeMultiplier = 1.3f;
            Projectile.netImportant = true;
        }
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
            Timer++;
            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (Timer >= swingTime || owner.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }
            owner.heldProj = Projectile.whoAmI;
            if (Timer == (int)(swingTime / 2))
            {
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
                Vector2 tip = points[points.Count - 1];
                for (int i = 0; i < 10; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust dust = Dust.NewDustDirect(tip, 0, 0, DustID.BlueTorch, vel.X, vel.Y, 100, new Color(100, 160, 255), 1.2f);
                    dust.noGravity = true;
                }
            }
            if (Timer % 4 == 0)
            {
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                int tipIndex = points.Count - 1;
                if (tipIndex > 0)
                {
                    Vector2 tipPos = points[tipIndex];
                    Dust dust = Dust.NewDustDirect(tipPos, 0, 0, DustID.BlueTorch, 0, 0, 150, new Color(80, 140, 255), 0.6f);
                    dust.noGravity = true;
                    dust.velocity *= 0.2f;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            target.AddBuff(ModContent.BuffType<Divider_summontag>(), 240);
            Projectile.damage = (int)(Projectile.damage * 0.8f);
            Player player = Main.player[Projectile.owner];
            if (Main.myPlayer == Projectile.owner)
            {
                int essenceCount = Main.rand.NextBool(3) ? 2 : 1;
                for (int i = 0; i < essenceCount; i++)
                {
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-5f, -2f));
                    Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, velocity, ModContent.ProjectileType<ManaEssence_divider_proj>(), 0, 0f, Projectile.owner, ai0: target.whoAmI, ai1: 0);
                }
                if (player.statMana >= 40)
                {
                    bool alreadyHasSlash = false;
                    for (int j = 0; j < Main.maxProjectiles; j++)
                    {
                        Projectile p = Main.projectile[j];
                        if (p.active && p.type == ModContent.ProjectileType<Divider_slash_proj>() && p.ai[0] == target.whoAmI)
                        {
                            alreadyHasSlash = true;
                            break;
                        }
                    }
                    if (!alreadyHasSlash)
                    {
                        player.statMana -= 40;
                        Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<Divider_slash_proj>(), hit.Damage, 0f, Projectile.owner, ai0: target.whoAmI);
                    }
                }
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.BlueTorch : DustID.PurpleTorch;
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, dustType, vel.X, vel.Y, 100, default, Main.rand.NextFloat(1f, 1.8f));
                dust.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 0, new Color(100, 150, 255), 1.2f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.6f, Pitch = 0.2f}, target.Center);
            Projectile.netUpdate = true;
        }
        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);
            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color baseColor = new Color(100, 150, 220);
                Color color = Lighting.GetColor(element.ToTileCoordinates(), baseColor);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
                pos += diff;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);
            DrawLine(list);
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame = new Rectangle(0, 0, 10, 26);
                Vector2 origin = new Vector2(5, 8);
                float scale = 1f;
                if (i == list.Count - 2)
                {
                    frame.Y = 74;
                    frame.Height = 18;
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Terraria.Utils.GetLerpValue(0.1f, 0.7f, t, true) * Terraria.Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }
                else if (i > 10)
                {
                    frame.Y = 58;
                    frame.Height = 16;
                }
                else if (i > 5)
                {
                    frame.Y = 42;
                    frame.Height = 16;
                }
                else if (i > 0)
                {
                    frame.Y = 26;
                    frame.Height = 16;
                }
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
                pos += diff;
            }
            return false;
        }
	}
    public class Divider_slash_proj : ModProjectile
    {
        public override string Texture => "jarkos/particles/Spark";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
        }
        public override void AI()
        {
            int npcIndex = (int)Projectile.ai[0];
            if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
            {
                Projectile.Kill();
                return;
            }
            NPC target = Main.npc[npcIndex];
            Projectile.Center = target.Center;
            int age = 30 - Projectile.timeLeft;
            if (age == 15)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 1f, Pitch = 0.4f }, target.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage = Projectile.damage, Crit = true, Knockback = 0f, HitDirection = Projectile.Center.X < target.Center.X ? 1 : -1
                    };
                    target.StrikeNPC(hitInfo);
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendStrikeNPC(target, hitInfo);
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D sparkTex = TextureAssets.Projectile[Projectile.type].Value;
            float progress = (30f - Projectile.timeLeft) / 30f;
            float alpha = 1f - progress;
            if (progress > 0.5f)
            {
                alpha = (1f - progress) * 2f;
            }
            alpha = MathHelper.Clamp(alpha, 0f, 1f);
            float halfLen = (float)Math.Sin(progress * MathHelper.Pi) * 25f;
            Vector2 start1 = Projectile.Center + new Vector2(-40f, -40f);
            Vector2 end1 = Projectile.Center + new Vector2(40f, 40f);
            Vector2 dir1 = (end1 - start1).SafeNormalize(Vector2.UnitX);
            Vector2 center1 = Vector2.Lerp(start1, end1, progress);
            Vector2 p1_1 = center1 - dir1 * halfLen;
            Vector2 p1_2 = center1 + dir1 * halfLen;
            DrawSlashLine(spriteBatch, sparkTex, p1_1, p1_2, alpha);
            Vector2 start2 = Projectile.Center + new Vector2(40f, -40f);
            Vector2 end2 = Projectile.Center + new Vector2(-40f, 40f);
            Vector2 dir2 = (end2 - start2).SafeNormalize(Vector2.UnitX);
            Vector2 center2 = Vector2.Lerp(start2, end2, progress);
            Vector2 p2_1 = center2 - dir2 * halfLen;
            Vector2 p2_2 = center2 + dir2 * halfLen;
            DrawSlashLine(spriteBatch, sparkTex, p2_1, p2_2, alpha);
            return false;
        }
        private void DrawSlashLine(SpriteBatch spriteBatch, Texture2D tex, Vector2 p1, Vector2 p2, float alpha)
        {
            Vector2 diff = p2 - p1;
            float dist = diff.Length();
            if (dist < 1f)
            {
                return;
            }
            float rot = diff.ToRotation();
            Vector2 origin = new Vector2(0f, tex.Height / 2f);
            Color drawColor = new Color(0, 180, 255, 0) * alpha * 0.8f;
            Vector2 scale = new Vector2(dist / tex.Width, 0.15f);
            spriteBatch.Draw(tex, p1 - Main.screenPosition, null, drawColor, rot, origin, scale, SpriteEffects.None, 0f);
            Color coreColor = new Color(255, 255, 255, 0) * alpha * 0.9f;
            Vector2 coreScale = new Vector2(dist / tex.Width, 0.05f);
            spriteBatch.Draw(tex, p1 - Main.screenPosition, null, coreColor, rot, origin, coreScale, SpriteEffects.None, 0f);
        }
    }
}
