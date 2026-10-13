using jarkos.buffs.summonbuff;
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
	public class Ancient_whip_proj : ModProjectile
	{
		private ref float ChargeProgress => ref Projectile.ai[0];
        private ref float RangeMultiplier => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
			Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.WhipSettings.Segments = 4;
            Projectile.WhipSettings.RangeMultiplier = 1.0f;
        }
		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() 
		{
			Projectile.WhipSettings.RangeMultiplier = RangeMultiplier;
            Projectile.WhipSettings.Segments = (int)MathHelper.Lerp(2, 8, ChargeProgress);
			Player owner = Main.player[Projectile.owner];

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
			Timer++;
			float swingTime = 180 * Projectile.MaxUpdates;

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
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int markDuration = (int)(180 + 300 * ChargeProgress);
            target.AddBuff(ModContent.BuffType<AncientNatureMark>(), markDuration);
            int dustCount = (int)(3 + ChargeProgress * 8);

            for (int i = 0; i < dustCount; i++)
            {
                int dustType = ChargeProgress >= 0.8f ? DustID.GreenTorch : DustID.Grass;
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, dustType);
                d.velocity = Main.rand.NextVector2Circular(2f + ChargeProgress * 2f, 3f + ChargeProgress * 0.07f);
                d.scale = Main.rand.NextFloat(0.01f, 0.2f + ChargeProgress * 0.01f);
                d.noGravity = true;
            }

            if (ChargeProgress >= 0.6f && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<Ancient_whip_explosion>(), Projectile.damage, Projectile.knockBack * ChargeProgress, Projectile.owner, ai0: ChargeProgress);
            }

            if (ChargeProgress >= 0.95f)
            {
                SoundEngine.PlaySound(SoundID.Item119 with{ Volume = 0.8f, Pitch = -0.4f }, target.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float offsetX = (i - 1) * 60f;
                        Vector2 rootPos = target.Bottom + new Vector2(offsetX, 0);
                        Projectile.NewProjectile(Projectile.GetSource_OnHit(target), rootPos, new Vector2(0, -8f), ModContent.ProjectileType<Ancient_whip_root>(), (int)(Projectile.damage * 0.4f), 2f, Projectile.owner);
                    }
                }
            }
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
