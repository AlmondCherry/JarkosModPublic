using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
namespace Jarkos.trails
{
    static class Trailreg
    {
        public static Asset<Texture2D> test => ModContent.Request<Texture2D>("Jarkos/textures/trail_test");
        public static MiscShaderData basic_test => GameShaders.Misc["Jarkos:TeslaTrail"];
        public static Asset<Texture2D> laser_beam1 => ModContent.Request<Texture2D>("Jarkos/textures/trail_test");
        public static MiscShaderData basic_test2 => GameShaders.Misc["Jarkos:RayTrail"];
        public static Asset<Texture2D> laser_beam2 => ModContent.Request<Texture2D>("Jarkos/textures/trail_test");
        public static MiscShaderData basic_test3 => GameShaders.Misc["Jarkos:CosmicTrail"];
        public static Asset<Texture2D> laser_beam3 => ModContent.Request<Texture2D>("Jarkos/textures/trail_test");
        public static MiscShaderData basic_test4 => GameShaders.Misc["Jarkos:SlashEffect"];
    }
}
