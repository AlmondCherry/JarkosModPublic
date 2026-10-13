using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.particles.storm;

namespace jarkos.projectiles.melee.true_melee
{
    public class FireExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        private const int Lifetime = 30;
        private const float Radius = 110f;
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = (int)(Radius * 2);
            Projectile.height = (int)(Radius * 2);
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.1f, Pitch = -0.2f }, Projectile.Center);
                if (Main.LocalPlayer.whoAmI == Projectile.owner)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                        Projectile.Center, Main.rand.NextVector2Unit(), 6f, 8f, 12, 800f));
                }
            }
            ScorchGround();
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = -2; i <= 2; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                        Projectile.Center + new Vector2(i * 34f, 0f),
                        new Vector2(i * 1.5f, -Main.rand.NextFloat(1f, 3f)),
                        ModContent.ProjectileType<FireGround>(),
                        (int)(Projectile.damage * 0.35f), 0f, Projectile.owner);
                }
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 10f);
                    new Fire_particle(
                        Projectile.Center,
                        vel,
                        new Color(255, 140, 10),
                        lifetime: Main.rand.Next(8, 14),
                        scale: Main.rand.NextFloat(0.5f, 0.9f),
                        opacity: 0.8f,
                        rotationSpeed: Main.rand.NextFloat(-0.06f, 0.06f)
                    ).Spawn();
                }
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) - Vector2.UnitY * 2f;
                    new Spark(
                        Projectile.Center,
                        vel,
                        new Color(255, 200, 50),
                        lifetime: Main.rand.Next(6, 12),
                        scale: Main.rand.NextFloat(0.4f, 0.7f),
                        opacity: 1f
                    ).Spawn();
                }
            }
        }
        private void ScorchGround()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            int cx = (int)(Projectile.Center.X / 16f);
            int cy = (int)(Projectile.Center.Y / 16f);
            int r = 6;
            for (int x = cx - r; x <= cx + r; x++)
            {
                for (int y = cy - r; y <= cy + r; y++)
                {
                    if (!WorldGen.InWorld(x, y, 10))
                        continue;
                    if (new Vector2(x - cx, y - cy).Length() > r)
                        continue;
                    Tile tile = Main.tile[x, y];
                    if (tile == null || !tile.HasTile)
                        continue;
                    bool burnable = tile.TileType == TileID.Plants || tile.TileType == TileID.Plants2
                        || tile.TileType == TileID.JunglePlants || tile.TileType == TileID.JunglePlants2
                        || tile.TileType == TileID.CorruptPlants || tile.TileType == TileID.CrimsonPlants
                        || tile.TileType == TileID.Vines || tile.TileType == TileID.JungleVines
                        || tile.TileType == TileID.Sunflower || tile.TileType == TileID.Pumpkins
                        || tile.TileType == TileID.LargePiles2 || tile.TileType == TileID.SmallPiles
                        || tile.TileType == TileID.Cobweb;
                    if (burnable)
                    {
                        WorldGen.KillTile(x, y, fail: false, effectOnly: false, noItem: true);
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y);
                    }
                    else if (tile.TileType == TileID.Grass || tile.TileType == TileID.JungleGrass
                        || tile.TileType == TileID.HallowedGrass)
                    {
                        tile.TileType = TileID.Dirt;
                        WorldGen.SquareTileFrame(x, y);
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.TileSquare, -1, -1, null, x, y, 1);
                    }
                }
            }
        }
        public override void AI()
        {
            float p = 1f - Projectile.timeLeft / (float)Lifetime;
            Lighting.AddLight(Projectile.Center, 2.5f * (1f - p), 1.2f * (1f - p), 0.3f * (1f - p));
            if (Projectile.timeLeft > Lifetime / 3)
            {
                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(Radius * p, Radius * p);
                        new Fire_particle(
                            pos,
                            -Vector2.UnitY * Main.rand.NextFloat(1f, 3f),
                            new Color(255, 120, 10),
                            lifetime: Main.rand.Next(5, 10),
                            scale: Main.rand.NextFloat(0.4f, 0.7f),
                            opacity: 0.7f,
                            rotationSpeed: Main.rand.NextFloat(-0.04f, 0.04f)
                        ).Spawn();
                    }
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2()) <= Radius;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 420);
        }
        private static Asset<Effect> explosionShader;
        private static Asset<Texture2D> noiseTex;
        public override bool PreDraw(ref Color lightColor)
        {
            explosionShader ??= ModContent.Request<Effect>(
                "jarkos/Shaders/FireExplosion", AssetRequestMode.ImmediateLoad);
            noiseTex ??= ModContent.Request<Texture2D>(
                "Terraria/Images/Misc/Perlin", AssetRequestMode.ImmediateLoad);
            float p = 1f - Projectile.timeLeft / (float)Lifetime;
            float drawR = Radius * 1.6f;
            Vector2 c = Projectile.Center - Main.screenPosition;
            var vertices = new VertexPositionColorTexture[4];
            vertices[0] = new VertexPositionColorTexture(new Vector3(c.X - drawR, c.Y - drawR, 0), Color.White, new Vector2(0, 0));
            vertices[1] = new VertexPositionColorTexture(new Vector3(c.X + drawR, c.Y - drawR, 0), Color.White, new Vector2(1, 0));
            vertices[2] = new VertexPositionColorTexture(new Vector3(c.X - drawR, c.Y + drawR, 0), Color.White, new Vector2(0, 1));
            vertices[3] = new VertexPositionColorTexture(new Vector3(c.X + drawR, c.Y + drawR, 0), Color.White, new Vector2(1, 1));
            GraphicsDevice device = Main.graphics.GraphicsDevice;
            Effect effect = explosionShader.Value;
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            effect.Parameters["uWorldViewProjection"].SetValue(view * projection);
            effect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["uProgress"].SetValue(p);
            effect.Parameters["uColorHot"].SetValue(new Color(255, 230, 140).ToVector4());
            effect.Parameters["uColorMid"].SetValue(new Color(255, 120, 10).ToVector4());
            effect.Parameters["uColorEdge"].SetValue(new Color(200, 40, 0).ToVector4());
            device.Textures[0] = noiseTex.Value;
            device.SamplerStates[0] = SamplerState.LinearWrap;
            device.RasterizerState = RasterizerState.CullNone;
            device.BlendState = BlendState.Additive;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
            }
            device.BlendState = BlendState.AlphaBlend;
            return false;
        }
    }
}
