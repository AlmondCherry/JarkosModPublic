using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using ReLogic.Content;
using Terraria.UI;
using System.Collections.Generic;

namespace jarkos.npc.Punisher
{
    public class PunisherSystem : ModSystem
    {
        private float currentIntensity = 0f;
        internal UI.PunisherChoiceUI choiceUI;
        internal UserInterface choiceInterface;
        public static Asset<Effect> PurpleFogRef;
        public static Asset<Effect> UniverseTearRef;
        public static Asset<Effect> PainRef;
        public static Asset<Effect> VignetteRef;
        public static Asset<Effect> SlashRef;
        public static Asset<Effect> WhiteFlashRef;
        public static Asset<Effect> Phase2FogRef;
        public static Asset<Effect> SpaceDistortionRef;
        public static Asset<Effect> Phase2SlashRef;
        public static Asset<Effect> MultiSlashRef;
        public static Asset<Effect> KaleidoscopeRef;
        public static Asset<Effect> HexBarrierRef;
        public static Asset<Effect> DimensionSunderRef;
        public static Asset<Effect> VoidTearRef;
        public static Asset<Effect> DeathrayFocusRef;
        public static Asset<Effect> DeathrayOverbloomRef;
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (!Main.dedServ)
                {
                    choiceUI = new UI.PunisherChoiceUI();
                    choiceUI.Activate();

                    choiceInterface = new UserInterface();
                    choiceInterface.SetState(choiceUI);
                }

                VignetteRef = null;
                SlashRef = null;
                PurpleFogRef = null;
                UniverseTearRef = null;
                PainRef = null;
                WhiteFlashRef = null;
                Phase2FogRef = null;
                SpaceDistortionRef = null;
                HexBarrierRef = null;
                DimensionSunderRef = null;
                VoidTearRef = null;
                DeathrayFocusRef = null;
                DeathrayOverbloomRef = null;
                
                Filters.Scene["jarkos:PunisherVignette"] = new Filter(new ScreenShaderData(VignetteRef, "Vignette"), EffectPriority.High);
                Filters.Scene["jarkos:PunisherVignette"].Load();

                Filters.Scene["jarkos:VoidTear"] = new Filter(new ScreenShaderData(VoidTearRef, "VoidTearPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:VoidTear"].Load();

                Filters.Scene["jarkos:DeathrayFocus"] = new Filter(new ScreenShaderData(DeathrayFocusRef, "FocusPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:DeathrayFocus"].Load();

                Filters.Scene["jarkos:DeathrayOverbloom"] = new Filter(new ScreenShaderData(DeathrayOverbloomRef, "OverbloomPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:DeathrayOverbloom"].Load();

                GameShaders.Misc["jarkos:PunisherSlash"] = new MiscShaderData(SlashRef, "SlashPass");
                Filters.Scene["jarkos:PunisherPurpleFog"] = new Filter(new ScreenShaderData(PurpleFogRef, "PurpleFogPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherPurpleFog"].Load();

                Filters.Scene["jarkos:PunisherUniverseTear"] = new Filter(new ScreenShaderData(UniverseTearRef, "UniverseTearPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherUniverseTear"].Load();

                MultiSlashRef = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherMultiSlash", AssetRequestMode.ImmediateLoad);
                Filters.Scene["jarkos:PunisherMultiSlash"] = new Filter(new ScreenShaderData(MultiSlashRef, "MultiSlashPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherMultiSlash"].Load();

                KaleidoscopeRef = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherKaleidoscope", AssetRequestMode.ImmediateLoad);
                Filters.Scene["jarkos:PunisherKaleidoscope"] = new Filter(new ScreenShaderData(KaleidoscopeRef, "KaleidoscopePass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherKaleidoscope"].Load();

                PainRef = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherPain", AssetRequestMode.ImmediateLoad);
                Filters.Scene["jarkos:PunisherPain"] = new Filter(new ScreenShaderData(PainRef, "PainPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherPain"].Load();

                WhiteFlashRef = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherWhiteFlash", AssetRequestMode.ImmediateLoad);
                Filters.Scene["jarkos:PunisherWhiteFlash"] = new Filter(new ScreenShaderData(WhiteFlashRef, "PunisherWhiteFlash"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherWhiteFlash"].Load();

                Phase2FogRef = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherPhase2Fog", AssetRequestMode.ImmediateLoad);
                Filters.Scene["jarkos:PunisherPhase2Fog"] = new Filter(new ScreenShaderData(Phase2FogRef, "Phase2FogPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherPhase2Fog"].Load();

                SpaceDistortionRef = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherSpaceDistortion", AssetRequestMode.ImmediateLoad);
                Filters.Scene["jarkos:PunisherSpaceDistortion"] = new Filter(new ScreenShaderData(SpaceDistortionRef, "SpaceDistortionPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherSpaceDistortion"].Load();

                Phase2SlashRef = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherPhase2Slash", AssetRequestMode.ImmediateLoad);
                Filters.Scene["jarkos:PunisherPhase2Slash"] = new Filter(new ScreenShaderData(Phase2SlashRef, "Phase2SlashPass"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:PunisherPhase2Slash"].Load();

                DimensionSunderRef = ModContent.Request<Effect>("jarkos/Shaders/Punisher/DimensionSunder", AssetRequestMode.ImmediateLoad);
                Filters.Scene["jarkos:DimensionSunder"] = new Filter(new ScreenShaderData(DimensionSunderRef, "Pass1"), EffectPriority.VeryHigh);
                Filters.Scene["jarkos:DimensionSunder"].Load();
            }
        }
        public override void PostSetupContent()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                var asset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherVignette", AssetRequestMode.ImmediateLoad);
                VignetteRef= asset;

                var slashAsset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherSlash", AssetRequestMode.ImmediateLoad);
                SlashRef = slashAsset;

                var fogAsset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherPurpleFog", AssetRequestMode.ImmediateLoad);
                PurpleFogRef = fogAsset;

                var tearAsset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherUniverseTear", AssetRequestMode.ImmediateLoad);
                UniverseTearRef = tearAsset;

                var painAsset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherPain", AssetRequestMode.ImmediateLoad);
                PainRef = painAsset;

                var flashAsset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherWhiteFlash", AssetRequestMode.ImmediateLoad);
                WhiteFlashRef = flashAsset;

                var phase2FogAsset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherPhase2Fog", AssetRequestMode.ImmediateLoad);
                Phase2FogRef = phase2FogAsset;

                var spaceDistortionAsset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherSpaceDistortion", AssetRequestMode.ImmediateLoad);
                SpaceDistortionRef = spaceDistortionAsset;

                var phase2SlashAsset = ModContent.Request<Effect>("jarkos/Shaders/Punisher/PunisherPhase2Slash", AssetRequestMode.ImmediateLoad);
                Phase2SlashRef = phase2SlashAsset;

                var voidTearAsset = ModContent.Request<Effect>("jarkos/npc/Punisher/Effects/VoidTear", AssetRequestMode.ImmediateLoad);
                VoidTearRef = voidTearAsset;

                var deathrayFocusAsset = ModContent.Request<Effect>("jarkos/npc/Punisher/Effects/DeathrayFocus", AssetRequestMode.ImmediateLoad);
                DeathrayFocusRef = deathrayFocusAsset;

                var deathrayOverbloomAsset = ModContent.Request<Effect>("jarkos/npc/Punisher/Effects/DeathrayOverbloom", AssetRequestMode.ImmediateLoad);
                DeathrayOverbloomRef = deathrayOverbloomAsset;

                if (VignetteRef.Value != null)
                {
                    Mod.Logger.Info("Effect is NOT null, count: " + VignetteRef.Value.Techniques.Count);
                    foreach (var tech in VignetteRef.Value.Techniques)
                    {
                        Mod.Logger.Info("Technique: " + tech.Name + " | Passes: " + tech.Passes.Count);
                        foreach (var pass in tech.Passes)
                        {
                            Mod.Logger.Info("Pass: " + pass.Name);
                        }
                    }
                }
            }
        }
        public override void Unload()
        {
        }
        public override void ModifyScreenPosition()
        {
            if (Main.netMode == NetmodeID.Server) return;
            bool bossAlive = false;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.type == ModContent.NPCType<Punisher>())
                {
                    bossAlive = true;
                    break;
                }
            }
            if (bossAlive && Main.LocalPlayer.active && !Main.LocalPlayer.dead)
            {
                Player player = Main.LocalPlayer;
                float healthRatio = (float)player.statLife / player.statLifeMax2;
                float targetIntensity = 1f - healthRatio;

                currentIntensity = MathHelper.Lerp(currentIntensity, targetIntensity, 0.03f);

                float freq = 90f / 60f;
                float time = Main.GlobalTimeWrappedHourly;

                float rawPulse = (float)Math.Sin(time * freq * MathHelper.TwoPi);
                float smoothPulse = (rawPulse + 1f) / 2f;
                float pulse = smoothPulse * 0.2f;

                if (!Filters.Scene["jarkos:PunisherVignette"].IsActive())
                {
                    Filters.Scene.Activate("jarkos:PunisherVignette");
                }
                
                Filters.Scene["jarkos:PunisherVignette"].GetShader().Shader.Parameters["Intensity"]?.SetValue(currentIntensity);
                Filters.Scene["jarkos:PunisherVignette"].GetShader().Shader.Parameters["Pulse"]?.SetValue(pulse);
            }
            else
            {
                if (Filters.Scene["jarkos:PunisherVignette"].IsActive())
                {
                    Filters.Scene.Deactivate("jarkos:PunisherVignette");
                }
            }
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (UI.PunisherChoiceUI.Visible)
            {
                choiceInterface?.Update(gameTime);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("jarkos: Punisher Choice", delegate
                    {
                        if (UI.PunisherChoiceUI.Visible)
                        {
                            choiceInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
}
