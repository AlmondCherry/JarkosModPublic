using jarkos.buffs.summonbuff;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon.whip
{
	public class Diffusal_whip_proj : ModProjectile
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
			Projectile.WhipSettings.Segments = 12;
			Projectile.WhipSettings.RangeMultiplier = 1.35f;
			Projectile.netImportant = true;
		}
		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {
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
			if (Timer == swingTime / 2) 
			{
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
			}
        }
		private bool hasSpawnedBlades = false;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
            target.AddBuff(ModContent.BuffType<Diffusal_whip_mark>(), 240);
            if (!hasSpawnedBlades && Main.myPlayer == Projectile.owner)
            {
                hasSpawnedBlades = true;
                int bladeCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.type == ModContent.ProjectileType<Diffusal_phantom_blade>() && p.owner == Projectile.owner)
                    {
                        bladeCount++;
                    }
                }
                if (bladeCount < 2)
                {
                    Player owner = Main.player[Projectile.owner];

                    Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center + new Vector2(-80, -60), Vector2.Zero, ModContent.ProjectileType<Diffusal_phantom_blade>(), Projectile.damage, Projectile.knockBack * 0.5f, Projectile.owner, ai0: target.whoAmI, ai1: 0f);
                    Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center + new Vector2(80, -60), Vector2.Zero, ModContent.ProjectileType<Diffusal_phantom_blade>(), Projectile.damage, Projectile.knockBack * 0.5f, Projectile.owner, ai0: target.whoAmI, ai1: 1f);
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f, Pitch = 0.3f }, target.Center);
                }
            }
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			Projectile.damage = (int)(Projectile.damage * 0.7f);
			new Circular_glow(target.Center, Projectile.velocity, Color.SkyBlue, 160, 0.15f, 1.5f, 1f).Spawn();
			Projectile.netUpdate = true;
		}
		private void DrawLine(List<Vector2> list) {
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2);
			Vector2 pos = list[0];

			for (int i = 0; i < list.Count - 1; i++) 
			{
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;
				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.SkyBlue);
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
				float scale = 1;
				if (i == list.Count - 2) {
					frame.Y = 74;
					frame.Height = 18;
					Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
					float t = Timer / timeToFlyOut;
					scale = MathHelper.Lerp(0.5f, 1.5f, Terraria.Utils.GetLerpValue(0.1f, 0.7f, t, true) * Terraria.Utils.GetLerpValue(0.9f, 0.7f, t, true));
				}
				else if (i > 10) {
					frame.Y = 58;
					frame.Height = 16;
				}
				else if (i > 5) {
					frame.Y = 42;
					frame.Height = 16;
				}
				else if (i > 0) {
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
}
