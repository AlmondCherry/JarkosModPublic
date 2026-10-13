using System;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class Dante_sword_thrown : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/Dante_sword";
        private static Asset<Effect> riftShader;
        private static bool shaderTried;
        private static BasicEffect basicFallback;
        private const int MAX_TRAIL = 24;
        private readonly float[] trailRot = new float[MAX_TRAIL];
        private readonly float[] trailLen = new float[MAX_TRAIL];
        private readonly Vector2[] trailPos = new Vector2[MAX_TRAIL];
        private int trailCount;
        private float TargetDistance => Projectile.ai[0];
        private ref float State => ref Projectile.ai[1];
        private Vector2 startPos;
        private float rotSpeed;
        private Vector2 initialVelocity;
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(startPos);
            writer.WriteVector2(initialVelocity);
            writer.Write(rotSpeed);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            startPos = reader.ReadVector2();
            initialVelocity = reader.ReadVector2();
            rotSpeed = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            startPos = Projectile.Center;
            initialVelocity = Projectile.velocity;
            rotSpeed = MathHelper.Lerp(0.3f, 0.65f, MathHelper.Clamp(initialVelocity.Length() / 45f, 0.3f, 1f));
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            if (rotSpeed <= 0.05f)
            {
                rotSpeed = MathHelper.Lerp(0.3f, 0.65f, MathHelper.Clamp(initialVelocity.Length() / 45f, 0.3f, 1f));
            }
            Projectile.rotation += rotSpeed * player.direction;
            PushTrail(Projectile.rotation, Projectile.Center, 148f * 0.8f);
            if (Projectile.timeLeft % 10 == 0 && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.4f, Pitch = 0.8f }, Projectile.Center);
            }
            if (State == 0)
            {
                float dist = Vector2.Distance(startPos, Projectile.Center);
                if (dist > TargetDistance)
                {
                    State = 1;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }
                else
                {
                    float speedScale = 1f - MathHelper.Clamp(dist / TargetDistance, 0f, 0.9f);
                    Projectile.velocity = initialVelocity * speedScale;
                }
            }
            else
            {
                ref float returnTimer = ref Projectile.localAI[0];
                returnTimer++;
                Vector2 toPlayer = player.Center - Projectile.Center;
                float distToPlayer = toPlayer.Length();
                float speedMultiplier = MathHelper.Clamp(initialVelocity.Length() / 45f, 0.6f, 1.2f);
                float returnSpeed = MathHelper.Min(22f + returnTimer * 1.5f, 45f) * speedMultiplier;

                if (distToPlayer <= returnSpeed || distToPlayer < 45f)
                {
                    Projectile.Kill();
                    return;
                }

                Projectile.velocity = toPlayer.SafeNormalize(Vector2.UnitX) * returnSpeed;

                if (Main.rand.NextBool(3) && Main.netMode != NetmodeID.Server)
                {
                    new Spark(Projectile.Center + Main.rand.NextVector2Circular(30, 30), -Projectile.velocity * 0.5f, Dante_sword_swing.ColorCore, 15, 0.4f, 0.8f).Spawn();
                }
            }
        }
        private void PushTrail(float rot, Vector2 pos, float len)
        {
            int last = Math.Min(trailCount, MAX_TRAIL - 1);
            for (int i = last; i > 0; i--)
            {
                trailRot[i] = trailRot[i - 1];
                trailPos[i] = trailPos[i - 1];
                trailLen[i] = trailLen[i - 1];
            }
            trailRot[0] = rot;
            trailPos[0] = pos;
            trailLen[0] = len;
            trailCount = Math.Min(trailCount + 1, MAX_TRAIL);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.8f, Pitch = -0.3f }, target.Center);
                SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = 0.7f, Pitch = -0.1f }, target.Center);
                if (Main.LocalPlayer.whoAmI == Projectile.owner)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(target.Center, (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), 4f, 5f, 8, 800f, FullName));
                }
                Vector2 hitPos = target.Center;
                Vector2 dirOut = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                for (int i = 0; i < 12; i++)
                {
                    Vector2 v = dirOut.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(5f, 12f);
                    Color bloodColor = Main.rand.NextBool() ? new Color(180, 10, 10) : new Color(130, 0, 0);
                    new Spark(hitPos, v, bloodColor, 30, Main.rand.NextFloat(0.5f, 0.9f), 0.9f).Spawn();
                }
                for (int i = 0; i < 5; i++)
                {
                    Vector2 v = dirOut.RotatedBy(Main.rand.NextFloat(-1.2f, 1.2f)) * Main.rand.NextFloat(7f, 15f);
                    Color bloodColor = new Color(150, 0, 0);
                    new Stretching_particle(hitPos, v, bloodColor, 20, Main.rand.NextFloat(30f, 60f), Main.rand.NextFloat(3f, 6f), 0.05f).Spawn();
                }
            }
        }
        private static bool TryGetShader(out Effect fx)
        {
            if (!shaderTried)
            {
                shaderTried = true;
                try
                {
                    riftShader = ModContent.Request<Effect>("jarkos/Shaders/RiftSlash", AssetRequestMode.ImmediateLoad);
                }
                catch { riftShader = null; }
            }
            fx = riftShader != null ? riftShader.Value : null;
            return fx != null;
        }
        private static BasicEffect GetBasicFallback(GraphicsDevice gd)
        {
            if (basicFallback == null || basicFallback.IsDisposed)
                basicFallback = new BasicEffect(gd) { TextureEnabled = true, VertexColorEnabled = true };
            return basicFallback;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            float texDiag = (float)Math.Sqrt(tex.Width * tex.Width + tex.Height * tex.Height);
            float scale = 148f * 0.8f / texDiag * 1.08f;
            Color glow = new Color(Dante_sword_swing.ColorBlade.R, Dante_sword_swing.ColorBlade.G, Dante_sword_swing.ColorBlade.B, 0) * 0.6f;
            Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation + MathHelper.PiOver4, origin, scale * 1.15f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation + MathHelper.PiOver4, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
        private void DrawTrail()
        {
            if (trailCount < 2)
                return;
            bool useShader = TryGetShader(out Effect fx);
            const int SUB = 2;
            int segs = (trailCount - 1) * SUB;
            var verts = new VertexPositionColorTexture[(segs + 1) * 2];
            float innerR = 15f;
            int v = 0;
            for (int i = 0; i <= segs; i++)
            {
                float fi = i / (float)SUB;
                int i0 = (int)fi;
                int i1 = Math.Min(i0 + 1, trailCount - 1);
                float fr = fi - i0;
                float rot = trailRot[i0] + MathHelper.WrapAngle(trailRot[i1] - trailRot[i0]) * fr;
                Vector2 pos = Vector2.Lerp(trailPos[i0], trailPos[i1], fr);
                float outerR = MathHelper.Lerp(trailLen[i0], trailLen[i1], fr);
                float p = i / (float)segs;
                float fade = (1f - p) * (1f - p);
                Vector2 dir = rot.ToRotationVector2();
                Vector2 inner = pos + dir * innerR;
                Vector2 outer = pos + dir * outerR;
                Color cIn = Dante_sword_swing.ColorBlade * (0.6f * fade);
                Color cOut = Color.Lerp(Dante_sword_swing.ColorCyan, Dante_sword_swing.ColorGold, p * 0.8f) * (0.8f * fade);
                float u = p * 2f;
                verts[v] = new VertexPositionColorTexture(new Vector3(inner, 0f), cIn, new Vector2(u, 0f));
                verts[v + 1] = new VertexPositionColorTexture(new Vector3(outer, 0f), cOut, new Vector2(u, 1f));
                v += 2;
            }
            Main.spriteBatch.End();
            GraphicsDevice gd = Main.instance.GraphicsDevice;
            gd.BlendState = BlendState.Additive;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.DepthStencilState = DepthStencilState.None;
            gd.SamplerStates[0] = SamplerState.LinearWrap;
            Matrix wvp = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
                * Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/RiftNoise").Value;
            BasicEffect be = GetBasicFallback(gd);
            be.World = Matrix.Identity;
            be.View = Matrix.Identity;
            be.Projection = wvp;
            be.Texture = noise;
            be.CurrentTechnique.Passes[0].Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, verts.Length - 2);
            RestartSpriteBatch();
        }
        private static void RestartSpriteBatch()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
