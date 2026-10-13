using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace jarkos.core.particles
{
    public class ParticlesManagers : ModSystem
    {
        internal static readonly List<Particles> activeParticles = new();
        internal static readonly Dictionary<Type, int> particleIDLookup = new();
        internal static readonly Dictionary<int, Texture2D> particleTextureLookup = new();
        public override void OnModLoad()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            int particleCount = activeParticles.Count;
            LoadParticles(Mod.Code);
            
            On_Main.DrawInfernoRings += DrawParticlesDetour;
        }
        private void DrawParticlesDetour(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            DrawParticles();
            orig(self);
        }
        private static void DrawParticles()
        {
            if (!activeParticles.Any())
                return;
            Main.spriteBatch.End();
            
            var blendGroups = activeParticles.GroupBy(p => p.DrawBlendState);
            foreach (var blendGroup in blendGroups)
            {
                RasterizerState screenCull = null;
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendGroup.First().DrawBlendState, Main.DefaultSamplerState, DepthStencilState.None, screenCull, null, Main.GameViewMatrix.TransformationMatrix);
                foreach (Particles p in blendGroup)
                    p.Draw();
                Main.spriteBatch.End();
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public override void PostUpdateDusts()
        {
            activeParticles.RemoveAll(p => p.Time >= p.Lifetime);
            int particleCount = activeParticles.Count;

            for (int i = 0; i < particleCount; i++)
            {
                activeParticles[i].Time++;
                activeParticles[i].Update();
                activeParticles[i].Position += activeParticles[i].Velocity;

                if(particleCount > ModContent.GetInstance<Jarkos_config>().maxparticles)
                {
                    for (int k = ModContent.GetInstance<Jarkos_config>().maxparticles; k < particleCount; k++)
                    {
                        activeParticles[k].Kill();
                    }
                }
            }
        }
        public static void LoadParticles(Assembly assembly)
        {
            int currentParticleID = 0;
            if (particleIDLookup?.Any() ?? false)
                currentParticleID = particleIDLookup.Values.Max() + 1;

            foreach (Type particleType in AssemblyManager.GetLoadableTypes(assembly))
            {
                if (!particleType.IsSubclassOf(typeof(Particles)) || particleType.IsAbstract)
                    continue;

                Particles particle = (Particles)FormatterServices.GetUninitializedObject(particleType);
                particleIDLookup[particleType] = currentParticleID;
                Texture2D particleTexture = ModContent.Request<Texture2D>(particle.TexturePath, AssetRequestMode.ImmediateLoad).Value;
                particleTextureLookup[currentParticleID] = particleTexture;
                particle.Load();

                currentParticleID++;
                if(currentParticleID > ModContent.GetInstance<Jarkos_config>().maxparticles)
                {
                }
            }
        }
    }
}
