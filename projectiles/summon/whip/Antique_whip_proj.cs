using jarkos.buffs.debuff;
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
	public class Antique_whip_proj : ModProjectile
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
			Projectile.WhipSettings.Segments = 20;
			Projectile.WhipSettings.RangeMultiplier = 2.25f;
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
			if (Timer >= swingTime || owner.itemAnimation <= 0) {
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
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			target.AddBuff(ModContent.BuffType<Antiquewhip_buff>(), 600);
			target.AddBuff(ModContent.BuffType<Ancient_venom_buff>(), 600);

			Projectile.damage = (int)(Projectile.damage * 0.7f);

			new Circular_glow(target.Center, Projectile.velocity, Color.Orange, 160, 0.15f, 1.5f, 1f).Spawn();

			target.AddBuff(ModContent.BuffType<Ancient_venom_buff>(), 360);
			target.AddBuff(ModContent.BuffType<Antique_whip_mark>(), 300);

            if (Main.myPlayer == Projectile.owner)
            {
                Player owner = Main.player[Projectile.owner];
                float distance = Vector2.Distance(owner.Center, target.Center);
                float distanceFactor = MathHelper.Clamp(distance / 250f, 0.5f, 3.5f);
                int explosionDamage = (int)(Projectile.damage * 0.5f);
                float explosionScale = MathHelper.Clamp(distanceFactor, 0.8f, 2.5f);
                int explosionProj = Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<Antique_whip_explosion>(), explosionDamage, 6f * distanceFactor, Projectile.owner, ai0: distanceFactor, ai1: distance);

                float pitch = MathHelper.Clamp(-0.5f + distanceFactor * 0.2f, -0.5f, 0.3f);
                float volume = MathHelper.Clamp(0.4f + distanceFactor * 0.2f, 0.4f, 1.0f);
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = volume, Pitch = pitch }, target.Center);
                SoundEngine.PlaySound(SoundID.Item125 with { Volume = volume * 0.7f, Pitch = pitch + 0.2f }, target.Center);
            }
			Projectile.netUpdate = true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player owner = Main.player[Projectile.owner];
            float distance = Vector2.Distance(owner.Center, target.Center);
            float distanceMultiplier = MathHelper.Clamp(distance / 300f, 0.5f, 1.25f);
            modifiers.FinalDamage *= distanceMultiplier;
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
				Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.Orange);
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
