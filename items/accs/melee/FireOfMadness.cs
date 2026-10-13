using System;
using System.Collections.Generic;
using System.Linq;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.items.accs.melee
{
	public class FireOfMadness : ModItem
    {
		public override void SetDefaults()
		{
			Item.width = 92;
			Item.height = 80;
			Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<NuminousVoicesPlayer>().hasNuminousVoices = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipLine = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");

            if (tooltipLine != null)
            {
                var bindkey = NuminousVoicesKeybind.SummonVoiceKey.GetAssignedKeys();
                string firstbindkey = bindkey.Count > 0 ? bindkey[0] : "None";
                tooltipLine.Text = Language.GetTextValue("Mods.jarkos.Items.FireOfMadness.Tooltip", firstbindkey);
            }
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.Meteorite, 150)
			.AddIngredient(ItemID.Spike, 45)
			.AddIngredient(ItemID.BattlePotion, 10)
			.AddIngredient(ItemID.WarriorEmblem, 1)
			.AddIngredient(ItemID.SoulofNight, 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}

	#region Buff
	public class NuminousVoiceBuff : ModBuff
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/Vigilante/Vigilante_buff";

		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = false;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			var modPlayer = player.GetModPlayer<NuminousVoicesPlayer>();
			float boostPercent = modPlayer.drainedHealthBonus * 0.001f;
			player.GetAttackSpeed(DamageClass.Generic) += boostPercent;
		}

		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		{
			Player player = Main.LocalPlayer;
			float bonus = player.GetModPlayer<NuminousVoicesPlayer>().drainedHealthBonus * 0.1f;
			tip = $"Increases attack speed by {bonus}%";
		}
	}
	#endregion

	#region ModPlayer
	public class NuminousVoicesPlayer : ModPlayer
    {
        public bool hasNuminousVoices;
        public int voiceCooldown;
        public int drainedHealthBonus;
        public const int MaxCooldown = 3600;

        public override void ResetEffects()
        {
            hasNuminousVoices = false;
        }

        public override void PostUpdate()
        {
            if (voiceCooldown > 0)
                voiceCooldown--;

            if (hasNuminousVoices && NuminousVoicesKeybind.SummonVoiceKey.JustPressed && voiceCooldown <= 0)
            {
                SummonVoice();
                voiceCooldown = MaxCooldown;
            }
        }

        private void SummonVoice()
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                Vector2 spawnPos = Player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), -40f);
                Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[3]), spawnPos, Vector2.Zero, ModContent.ProjectileType<NuminousVoiceProjectile>(), 25, 2f, Player.whoAmI);
                SoundEngine.PlaySound(SoundID.Item8, Player.Center);
            }
        }
    }
	#endregion

	#region Keybind System
	public class NuminousVoicesKeybind : ModSystem
    {
		public static ModKeybind SummonVoiceKey { get; private set; }
		public override void Load()
		{
			SummonVoiceKey = KeybindLoader.RegisterKeybind(Mod, "Summon Voice", "G");
		}
		public override void Unload()
		{
			SummonVoiceKey = null;
        }
    }
	#endregion

	#region Projectile
	public class NuminousVoiceProjectile : ModProjectile
    {
		public override string Texture => "jarkos/textures/AccProjectile/NuminousVoiceProjectile";

        private const float SeekRange = 1000f;
        private const float SeekSpeed = 18f;
        private const float ReturnSpeed = 22f;
        private const int MaxDrainTime = 420;

        private ref float State => ref Projectile.ai[0];
        private ref float DrainTimer => ref Projectile.ai[1];

        private NPC targetNPC = null;
        private int totalDrainedHP = 0;
        private float orbitAngle = 0f;
        private int tickTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 1200;
            Projectile.light = 0.6f;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Lighting.AddLight(Projectile.Center, 1.2f, 1.0f, 0.2f);

            if (State == 0)
            {
                if (targetNPC == null || !targetNPC.active || targetNPC.dontTakeDamage || targetNPC.life <= 0)
                {
                    targetNPC = FindClosestEnemy(owner);
                }

                if (targetNPC != null && targetNPC.active)
                {
                    Vector2 dir = targetNPC.Center - Projectile.Center;
                    float dist = dir.Length();
                    if (dist > 1f) dir.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * SeekSpeed, 0.12f);

                    if (dist <= 36f)
                    {
                        State = 1;
                        DrainTimer = 0;
                        totalDrainedHP = 0;
                        SoundEngine.PlaySound(SoundID.Item103 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.Center);
                    }
                }
                else
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                        Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, vel.X, vel.Y, 100, default, 1.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f }, Projectile.Center);
                    Projectile.Kill();
                    return;
                }
            }
            else if (State == 1)
            {
                if (targetNPC == null || !targetNPC.active || targetNPC.life <= 0)
                {
                    State = 2;
                    return;
                }

                DrainTimer++;

                orbitAngle += 0.08f;
                Vector2 orbitPos = targetNPC.Center + orbitAngle.ToRotationVector2() * 40f;
                Projectile.Center = Vector2.Lerp(Projectile.Center, orbitPos, 0.25f);
                Projectile.velocity = Vector2.Zero;

                tickTimer++;
                if (tickTimer >= 15)
                {
                    tickTimer = 0;
                    int drainDamage = Math.Max(12, Projectile.damage);
                    
                    targetNPC.SimpleStrikeNPC(drainDamage, 0, false, 0, null, false, 0, true);
                    totalDrainedHP += drainDamage;

                    var modPlayer = owner.GetModPlayer<NuminousVoicesPlayer>();
                    modPlayer.drainedHealthBonus = totalDrainedHP;
                    owner.AddBuff(ModContent.BuffType<NuminousVoiceBuff>(), 600);

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 flowVel = (owner.Center - targetNPC.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 10f);
                        new Spark_gravit(targetNPC.Center, flowVel, new Color(255, 210, 50), 180, Main.rand.NextFloat(0.8f, 1.4f), 1f, 0.1f).Spawn();
                    }

                    Dust d = Dust.NewDustDirect(targetNPC.Center - new Vector2(10), 20, 20, DustID.Torch, 0f, -2f, 100, default, 1.8f);
                    d.noGravity = true;

                    SoundEngine.PlaySound(SoundID.NPCHit5 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
                }

                if (DrainTimer >= MaxDrainTime)
                {
                    State = 2;
                    owner.AddBuff(ModContent.BuffType<NuminousVoiceBuff>(), 600);
                    SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.8f, Pitch = 0.4f }, Projectile.Center);
                }
            }
            else if (State == 2)
            {
                Vector2 dir = owner.Center - Projectile.Center;
                float dist = dir.Length();
                if (dist > 1f) dir.Normalize();

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * ReturnSpeed, 0.15f);

                if (dist <= 40f)
                {
                    SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.8f, Pitch = 0.3f }, owner.Center);
                    for (int i = 0; i < 18; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                        Dust d = Dust.NewDustDirect(owner.Center - new Vector2(10), 20, 20, DustID.Torch, vel.X, vel.Y, 100, default, 2f);
                        d.noGravity = true;
                    }
                    Projectile.Kill();
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.05f;
            Projectile.spriteDirection = Projectile.velocity.X >= 0 ? 1 : -1;
        }

        private NPC FindClosestEnemy(Player owner)
        {
            NPC closest = null;
            float closestDist = SeekRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;

            float pulse = 1f + 0.18f * (float)Math.Sin(Main.GameUpdateCount * 0.12f);
            Color yellowAdditiveGlow = new Color(255, 220, 60, 0) * 0.5f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(glowTex, drawPos, null, yellowAdditiveGlow, Projectile.rotation, glowOrigin, Projectile.scale * pulse * 1.5f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, fx, 0);

            return false;
        }
    }
	#endregion
}
