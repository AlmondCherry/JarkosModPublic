using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.bastion
{
    public class BastionPlayer : ModPlayer
    {
        public const int MaxStacks = 5;
        public const int ResolveDuration = 600;
        public bool HasBastionSet;
        public int Stacks;
        public float AuraIntensity;
        public float BurstTimer;
        public float DigitFlash;
        public float DigitBump;
        public Vector2 DigitPos;
        public Vector2 DigitVel;
        private bool digitInit;
        public bool ResolveActive => Player.HasBuff(ModContent.BuffType<BastionResolveBuff>());
        public override void ResetEffects()
        {
            HasBastionSet = false;
        }
        public override void PostUpdateEquips()
        {
            if (!HasBastionSet)
            {
                Stacks = 0;
                digitInit = false;
            }
            if (ResolveActive)
                Player.GetDamage(DamageClass.Generic) += 0.05f;
        }
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (ResolveActive)
                modifiers.FinalDamage *= 0.80f;
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            if (!HasBastionSet || ResolveActive)
                return;
            Stacks++;
            DigitBump = 1f;
            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.35f, Pitch = 0.1f + Stacks * 0.13f }, Player.Center);
            if (Stacks >= MaxStacks)
            {
                Stacks = 0;
                DigitBump = 1.6f;
                BurstTimer = 1f;
                DigitFlash = 1f;
                AuraIntensity = MathHelper.Max(AuraIntensity, 0.9f);
                Player.AddBuff(ModContent.BuffType<BastionResolveBuff>(), ResolveDuration);
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.85f, Pitch = -0.15f }, Player.Center);
                SoundEngine.PlaySound(SoundID.Item45 with { Volume = 0.5f, Pitch = 0.3f }, Player.Center);
                for (int i = 0; i < 30; i++)
                {
                    Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(14f, 20f), DustID.Torch, Main.rand.NextVector2Circular(4.5f, 4.5f) - new Vector2(0f, 2.5f), 0, default, Main.rand.NextFloat(1.5f, 2.6f));
                    d.noGravity = true;
                }
            }
        }
        public override void PostUpdate()
        {
            float target = ResolveActive ? 1f : 0f;
            AuraIntensity = MathHelper.Lerp(AuraIntensity, target, target > AuraIntensity ? 0.10f : 0.045f);
            if (AuraIntensity < 0.004f)
                AuraIntensity = 0f;

            DigitBump *= 0.88f;
            BurstTimer = MathHelper.Max(0f, BurstTimer * 0.965f - 0.0015f);
            DigitFlash *= 0.975f;

            if (DigitFlash < 0.01f)
                DigitFlash = 0f;

            Vector2 anchor = Player.Top + new Vector2(0f, -36f + Player.gfxOffY);

            if (!digitInit)
            {
                DigitPos = anchor;
                DigitVel = Vector2.Zero;
                digitInit = true;
            }

            DigitVel += (anchor - DigitPos) * 0.085f;
            DigitVel *= 0.80f;
            DigitPos += DigitVel;

            if (AuraIntensity > 0.08f)
            {
                Lighting.AddLight(Player.Center, 0.95f * AuraIntensity, 0.5f * AuraIntensity, 0.12f * AuraIntensity);
                if (Main.rand.NextFloat() < AuraIntensity * 0.55f)
                {
                    Dust d = Dust.NewDustDirect(Player.position - new Vector2(8f, 10f), Player.width + 16, Player.height + 10, DustID.Torch);
                    d.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2.8f, -1.2f));
                    d.scale = Main.rand.NextFloat(1.0f, 1.9f);
                    d.noGravity = true;
                }
            }
            
            if (HasBastionSet && Main.myPlayer == Player.whoAmI
                && Player.ownedProjectileCounts[ModContent.ProjectileType<BastionAuraProj>()] == 0)
            {
                Projectile.NewProjectile(Player.GetSource_FromThis("BastionAura"), Player.Center, Vector2.Zero, ModContent.ProjectileType<BastionAuraProj>(), 0, 0f, Player.whoAmI);
            }
        }
    }
}
