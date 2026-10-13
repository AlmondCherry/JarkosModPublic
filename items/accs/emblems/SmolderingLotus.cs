using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Creative;
using Terraria.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using jarkos.systems;
using Terraria.Localization;

namespace jarkos.items.accs.emblems
{
	public class SmolderingLotus : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.width = 78;
			Item.height = 50;
			Item.accessory = true;
			Item.defense = 15;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<SmolderingLotusPlayer>().hasLotusAccessory = true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			var tooltipLine = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");

			if (tooltipLine != null)
			{
				var bindKey = SmolderingLotusKeybind.SmolderingLotusKey.GetAssignedKeys();
				string firstbindkey = bindKey.Count > 0 ? bindKey[0] : "None";
				tooltipLine.Text = Language.GetTextValue("Mods.jarkos.Items.SmolderingLotus.Tooltip", firstbindkey);
			}
        }
	}

	#region Keybind System
	public class SmolderingLotusKeybind : ModSystem
    {
		public static ModKeybind SmolderingLotusKey { get; private set; }
		public override void Load()
		{
			SmolderingLotusKey = KeybindLoader.RegisterKeybind(Mod, "SmolderingLotus", "G");
		}
		public override void Unload()
		{
			SmolderingLotusKey = null;
		}
    }
	#endregion

	#region ModPlayer
	public class SmolderingLotusPlayer : ModPlayer
    {
        public bool hasLotusAccessory = false;
        public int LotusCooldown = 0;
        public const int CooldownTime = 900;

        public override void ResetEffects()
        {
            hasLotusAccessory = false;
        }

        public override void PostUpdate()
        {
            if (LotusCooldown > 0)
                LotusCooldown--;

            if (hasLotusAccessory && LotusCooldown <= 0)
            {
                if (SmolderingLotusKeybind.SmolderingLotusKey.JustPressed)
                {
                    ActivateManaDrain();
                    LotusCooldown = CooldownTime;
                }
            }
        }

        private void ActivateManaDrain()
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                Projectile.NewProjectile(Player.GetSource_Accessory(Player.HeldItem), Player.Center, Vector2.Zero, ModContent.ProjectileType<SmolderingLotusDrainProj>(), 0, 0f, Player.whoAmI);
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.8f, Pitch = 0.3f }, Player.Center);
            }
        }
	}
	#endregion

	#region Mana Drain Projectile
	public class SmolderingLotusDrainProj : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_0";

		private const float SearchRadius = 380f;
		private int drainTimer = 0;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 600;
		}

		private List<NPC> GetTargets(Player owner)
		{
			List<NPC> targets = new List<NPC>();
			foreach (NPC npc in Main.npc)
			{
				if (npc.active && !npc.friendly && npc.CanBeChasedBy() && Vector2.Distance(owner.Center, npc.Center) <= SearchRadius)
				{
					targets.Add(npc);
				}
			}
			return targets.OrderBy(npc => Vector2.DistanceSquared(owner.Center, npc.Center)).Take(10).ToList();
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Center = owner.Center;
			drainTimer++;

			if (drainTimer % 15 == 0)
			{
				List<NPC> targets = GetTargets(owner);
				int totalManaGained = 0;

				foreach (NPC npc in targets)
				{
					int drained = Main.rand.Next(1, 2);
					totalManaGained += drained;

					for (int i = 0; i < 3; i++)
					{
						Vector2 flowVel = (owner.Center - npc.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(6f, 14f);
						Dust d = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(10f, 10f), DustID.ManaRegeneration, flowVel, 0, Color.Cyan, Main.rand.NextFloat(1f, 1.5f));
						d.noGravity = true;
					}
				}

				if (totalManaGained > 0)
				{
					owner.statMana = Math.Min(owner.statManaMax2, owner.statMana + totalManaGained);
					owner.ManaEffect(totalManaGained);
					SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.35f, Pitch = 0.6f }, owner.Center);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead) return false;

			Effect effect = HealShaders.Stream;
			if (effect == null || HealShaders.HealNoise == null) return false;

			List<NPC> targets = GetTargets(owner);
			if (targets.Count == 0) return false;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			float time = Main.GlobalTimeWrappedHourly * 6f;
			effect.Parameters["uTime"]?.SetValue(time);
			effect.Parameters["uOpacity"]?.SetValue(1.5f);
			effect.Parameters["uColor"]?.SetValue(new Color(0, 180, 255, 255).ToVector4());
			effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 255, 255).ToVector4());

			foreach (NPC npc in targets)
			{
				Vector2 start = owner.Center;
				Vector2 end = npc.Center;
				
				const int segmentCount = 14;
				List<Vector2> points = new List<Vector2>();
				
				for (int i = 0; i <= segmentCount; i++)
				{
					float t = i / (float)segmentCount;
					Vector2 basePos = Vector2.Lerp(start, end, t);

					Vector2 dir = (end - start).SafeNormalize(Vector2.UnitX);
					Vector2 perp = new Vector2(-dir.Y, dir.X);
					float wave = MathF.Sin(time * 4f + t * 6f + npc.whoAmI) * 8f * MathF.Sin(t * MathHelper.Pi);
					
					points.Add(basePos + perp * wave);
				}

				TrailRenderer.Draw(effect, points.ToArray(), t => 14f * MathF.Sin(t * MathHelper.Pi), color => Color.White, HealShaders.HealNoise);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}
	}
	#endregion
}
