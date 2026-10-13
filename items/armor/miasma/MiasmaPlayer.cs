using jarkos.items.armor.miasma.jarkos.items.armor.miasma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.miasma
{
	public class MiasmaPlayer : ModPlayer
	{
		public bool miasmaSet;
		public int rendCooldown;
		public int slashTimer;
		public override void ResetEffects()
		{
			miasmaSet = false;
		}
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (MiasmaShatterSystem.RendKeybind == null || !MiasmaShatterSystem.RendKeybind.JustPressed)
				return;
			if (!miasmaSet || rendCooldown > 0 || slashTimer > 0 || MiasmaShatterSystem.Active)
				return;

			rendCooldown = 60 * 45;
			slashTimer = 600;

			ActivationBurst();
		}
		public override void PostUpdateMiscEffects()
		{
			if (rendCooldown > 0)
				rendCooldown--;
			if (slashTimer <= 0)
				return;

			slashTimer--;
			if (Player.whoAmI == Main.myPlayer && slashTimer % 4 == 0)
				SpawnSlashOnEnemy();

			if (slashTimer == 0 && Player.whoAmI == Main.myPlayer)
				MiasmaShatterSystem.Activate(Player.Center);
		}
		private void SpawnSlashOnEnemy()
		{
			int found = -1;
			int count = 0;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
					continue;
				if (Vector2.DistanceSquared(npc.Center, Player.Center) > 1200f * 1200f)
					continue;

				count++;
				if (Main.rand.NextBool(count))
					found = i;
			}
			if (found < 0)
				return;

			NPC target = Main.npc[found];
			int damage = (int)Player.GetDamage(DamageClass.Generic).ApplyTo(130);
			float rot = Main.rand.NextFloat(MathHelper.TwoPi);

			Projectile.NewProjectile(Player.GetSource_Misc("MiasmaSlash"), target.Center, Vector2.Zero, ModContent.ProjectileType<MiasmaSlashProjectile>(), damage, 4f, Player.whoAmI, rot, Main.rand.NextFloat());

			if (slashTimer % 12 == 0)
				SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.6f, Volume = 0.5f }, target.Center);
		}
		private void ActivationBurst()
		{
			SoundEngine.PlaySound(SoundID.Zombie103 with { Pitch = -0.4f, Volume = 1.2f }, Player.Center);
			SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.3f }, Player.Center);

			for (int i = 0; i < 60; i++)
			{
				Vector2 off = (MathHelper.TwoPi * i / 60f).ToRotationVector2() * Main.rand.NextFloat(70f, 120f);
				Dust d = Dust.NewDustPerfect(Player.Center + off, DustID.PurpleTorch, -off * 0.07f, 0, default, 1.8f);
				d.noGravity = true;
			}
			MiasmaShatterSystem.ShakePower = 8f;
		}
		public override void PostUpdateEquips()
		{
			if (!miasmaSet || Main.dedServ)
				return;

			Lighting.AddLight(Player.Center, 0.45f, 0.15f, 0.75f);
			if (slashTimer > 0 && Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(28f, 40f), DustID.ShadowbeamStaff, Vector2.UnitY * -1.2f, 0, default, 1.1f);
				d.noGravity = true;
			}
		}
		public override void ModifyScreenPosition()
		{
			if (MiasmaShatterSystem.ShakePower > 0.5f)
			{
				Main.screenPosition += new Vector2(Main.rand.NextFloat(-MiasmaShatterSystem.ShakePower, MiasmaShatterSystem.ShakePower), Main.rand.NextFloat(-MiasmaShatterSystem.ShakePower, MiasmaShatterSystem.ShakePower));
			}
		}
	}
    namespace jarkos.items.armor.miasma
    {
	public class MiasmaShatterSystem : ModSystem
	{
		public const string FilterName = "jarkos:MiasmaShatter";
		public const int Duration = 150;
		public static ModKeybind RendKeybind;
		public static bool Active;
		public static int Timer;
		public static float ShakePower;
		public static Vector2 Origin;
		public override void Load()
		{
			RendKeybind = KeybindLoader.RegisterKeybind(Mod, "MiasmaRend", "V");

			if (Main.dedServ)
				return;

			Asset<Effect> shader = ModContent.Request<Effect>("jarkos/Effects/MiasmaShatter", AssetRequestMode.ImmediateLoad);
			Filters.Scene[FilterName] = new Filter(new ScreenShaderData(shader, "P0"), EffectPriority.VeryHigh);
		}
		public override void Unload()
		{
			RendKeybind = null;
			Active = false;
		}
		public static void Activate(Vector2 origin)
		{
			Active = true;
			Timer = 0;
			Origin = origin;
			ShakePower = 20f;

			SoundEngine.PlaySound(SoundID.Shatter with { Pitch = -0.4f, Volume = 1.4f });
			SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.2f });
		}
		public override void PostUpdateEverything()
		{
			if (Main.dedServ)
				return;

			ShakePower *= 0.9f;

			if (!Active)
			{
				if (Filters.Scene[FilterName].IsActive())
					Filters.Scene.Deactivate(FilterName);
				return;
			}

			Timer++;
			float progress = Timer / (float)Duration;

			if (Timer == 30)
			{
				ShakePower = 14f;
				SoundEngine.PlaySound(SoundID.Shatter with { Pitch = 0.2f, Volume = 1.2f });
			}

			if (!Filters.Scene[FilterName].IsActive())
				Filters.Scene.Activate(FilterName, Origin);
				Filters.Scene[FilterName].GetShader()
				.UseProgress(progress)
				.UseIntensity(MathHelper.Clamp((30f - Timer) / 30f, 0f, 1f) * 0.6f)
				.UseTargetPosition(Origin);

			if (Timer >= Duration)
			{
				Active = false;
				Timer = 0;
				Filters.Scene.Deactivate(FilterName);
			}
		}
    }
}
}
