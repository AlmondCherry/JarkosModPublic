using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class AegisDrone : ModProjectile
    {
        private ref float State => ref Projectile.ai[0];
        private ref float DroneIndex => ref Projectile.ai[1];
        private ref float TargetWho => ref Projectile.ai[2];
        private ref float Timer => ref Projectile.localAI[0];
        private Player Owner => Main.player[Projectile.owner];
        public override void SetDefaults() {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60 * 60;
            Projectile.netImportant = true;
        }
        private NPC Target {
            get {
                int i = (int)TargetWho;
                if (i >= 0 && i < Main.maxNPCs && Main.npc[i].active && Main.npc[i].CanBeChasedBy())
                    return Main.npc[i];
                return null;
            }
        }
        public override void AI() {
            Timer++;
            Projectile.spriteDirection = Projectile.velocity.X >= 0 ? 1 : -1;
            switch ((int)State) {
                case 0: EjectPhase(); break;
                case 1: StabilizePhase(); break;
                case 2: SeekPhase(); break;
                case 3: OrbitPhase(); break;
                case 4: ChargePhase(); break;
                case 5: KamikazePhase(); break;
                case 6: ReturnPhase(); break;
            }
            Lighting.AddLight(Projectile.Center, State == 4f ? new Vector3(1f, 0.2f, 0.1f) : new Vector3(0.9f, 0.7f, 0.2f));
        }
        private void SwitchState(float s) { State = s; Timer = 0f; Projectile.netUpdate = true; }
        private void EjectPhase() {
            Projectile.velocity.Y += 0.28f;
            Projectile.rotation += 0.25f * (DroneIndex == 0 ? -1 : 1);
            if (Main.rand.NextBool(3))
                Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, Main.rand.NextVector2Circular(1f, 1f), 120, default, 0.8f).noGravity = true;
            if (Timer >= 30f) {
                SwitchState(1);
                SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.6f }, Projectile.Center);
            }
        }
        private void StabilizePhase() {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, -Vector2.UnitY * 4f, 0.12f);
            Projectile.rotation = Projectile.rotation.AngleLerp(0f, 0.2f);
            Dust d = Dust.NewDustPerfect(Projectile.Bottom, DustID.YellowTorch, Vector2.UnitY * 3f + Main.rand.NextVector2Circular(1f, 1f), 0, default, 1.2f);
            d.noGravity = true;
            if (Timer >= 25f)
                SwitchState(Target != null ? 2 : 6);
        }
        private void SeekPhase() {
            NPC target = Target;
            if (target == null) { SwitchState(6); return; }
            Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 16f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.07f);
            Projectile.rotation = Projectile.velocity.X * 0.04f;
            TrailDust();
            if (Vector2.Distance(Projectile.Center, target.Center) < 180f)
                SwitchState(3);
        }
        private void OrbitPhase() {
            NPC target = Target;
            if (target == null) { SwitchState(6); return; }
            float angle = Timer * 0.06f + DroneIndex * MathHelper.Pi;
            Vector2 orbitPos = target.Center + angle.ToRotationVector2() * 130f + new Vector2(0f, (float)Math.Sin(Timer * 0.1f) * 15f);
            Projectile.velocity = (orbitPos - Projectile.Center) * 0.15f;
            Projectile.rotation = Projectile.velocity.X * 0.03f;
            if (Main.myPlayer == Projectile.owner && (Timer + DroneIndex * 17f) % 35f == 0f) {
                Vector2 vel = (target.Center + target.velocity * 8f - Projectile.Center).SafeNormalize(Vector2.UnitX) * 18f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                    ModContent.ProjectileType<AegisDroneLaser>(), (int)(Projectile.damage * 0.4f), 1f, Projectile.owner);
                SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.4f, Volume = 0.6f }, Projectile.Center);
            }
            if (Timer >= 240f) {
                SwitchState(4);
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.5f }, Projectile.Center);
            }
        }
        private void ChargePhase() {
            NPC target = Target;
            if (target == null) { SwitchState(6); return; }
            float radius = MathHelper.Lerp(130f, 60f, Timer / 60f);
            float angle = (240f + Timer) * 0.12f + DroneIndex * MathHelper.Pi;
            Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
            Projectile.velocity = (pos - Projectile.Center) * 0.2f;
            Projectile.rotation += 0.3f;
            if (Timer % 10f == 0f)
                SoundEngine.PlaySound(SoundID.Item13 with { Pitch = Timer / 60f, Volume = 0.5f }, Projectile.Center);
            if (Timer >= 60f) {
                SwitchState(5);
                Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 24f;
            }
        }
        private void KamikazePhase() {
            NPC target = Target;
            if (target != null)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                    (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 26f, 0.2f);
            Projectile.rotation = Projectile.velocity.ToRotation();
            TrailDust(red: true);
            bool hit = target != null && Projectile.Hitbox.Intersects(target.Hitbox);
            if (hit || target == null || Timer > 60f) {
                if (target != null && target.TryGetGlobalNPC<MarkedNPC>(out var marked)) {
                    marked.RemoveMark();
                }
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<AegisExplosion>(), Projectile.damage * 3, 10f, Projectile.owner);
                SwitchState(6);
                Projectile.velocity = -Projectile.velocity * 0.3f;
            }
        }
        private void ReturnPhase() {
            NPC target = Target;
            if (target != null && target.TryGetGlobalNPC<MarkedNPC>(out var marked)) {
                marked.RemoveMark();
            }
            Vector2 dest = Owner.Top - new Vector2(0f, 40f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, (dest - Projectile.Center).SafeNormalize(Vector2.UnitX) * 20f, 0.1f);
            Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.X * 0.04f, 0.2f);
            TrailDust();
            if (Vector2.Distance(Projectile.Center, dest) < 40f) {
                SoundEngine.PlaySound(SoundID.Item156 with { Pitch = 1f, Volume = 0.5f }, Projectile.Center);
                for (int i = 0; i < 12; i++)
                    Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(3f, 3f), 0, default, 0.9f).noGravity = true;
                Projectile.Kill();
            }
        }
        private void TrailDust(bool red = false) {
            if (Main.rand.NextBool(2)) {
                Dust d = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f,
                    red ? DustID.Torch : DustID.YellowTorch, -Projectile.velocity * 0.15f, 0, default, 1.1f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor) {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            SpriteEffects fx = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (State == 5f) {
                for (int i = 1; i < 6; i++) {
                    Vector2 pos = Projectile.Center - Projectile.velocity * i * 0.8f - Main.screenPosition;
                    Main.EntitySpriteDraw(tex, pos, null, new Color(255, 60, 30, 0) * (0.5f - i * 0.08f),
                        Projectile.rotation, tex.Size() / 2f, 1f, fx, 0);
                }
            }
            Color c = lightColor;
            if (State == 4f) {
                float blink = (float)Math.Sin(Timer * (0.3f + Timer / 60f)) * 0.5f + 0.5f;
                c = Color.Lerp(lightColor, Color.Red, blink);
            }
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, c,
                Projectile.rotation, tex.Size() / 2f, 1f, fx, 0);
            return false;
        }
    }
    public class MarkedNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool Marked;
        public int MarkTimer;
        public int FadeOutTimer = -1;

        public void ApplyMark(int duration) {
            Marked = true;
            MarkTimer = duration;
            FadeOutTimer = -1;
        }

        public void RemoveMark() {
            if (Marked && FadeOutTimer < 0) {
                FadeOutTimer = 30;
            }
        }

        public override void PostAI(NPC npc) {
            if (!Marked)
                return;

            if (FadeOutTimer >= 0) {
                FadeOutTimer--;
                if (FadeOutTimer <= 0) {
                    Marked = false;
                    FadeOutTimer = -1;
                }
            }
            else {
                MarkTimer--;
                if (MarkTimer <= 0 || !npc.active) {
                    Marked = false;
                }
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if (!Marked)
                return;

            float fadeAlpha = 1f;
            if (FadeOutTimer >= 0) {
                fadeAlpha = MathHelper.Clamp(FadeOutTimer / 30f, 0f, 1f);
            }
            else {
                fadeAlpha = MathHelper.Clamp(MarkTimer / 30f, 0f, 1f);
            }

            float pulse = (0.6f + 0.4f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f)) * fadeAlpha;
            Texture2D tex = TextureAssets.Npc[npc.type].Value;
            Vector2 origin = npc.frame.Size() / 2f;
            SpriteEffects fx = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (AegisEffects.ShadersLoaded) {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Effect shader = AegisEffects.MarkShader?.Value;
                if (shader != null)
                {
                    shader.Parameters["uColor"]?.SetValue(new Vector3(1f, 0.15f, 0.1f));
                    shader.Parameters["uOpacity"]?.SetValue(pulse * 0.8f);
                    shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                    shader.CurrentTechnique?.Passes[0]?.Apply();
                }
                spriteBatch.Draw(tex, npc.Center - screenPos, npc.frame, Color.White * fadeAlpha,
                    npc.rotation, origin, npc.scale, fx, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else {
                Color outline = new Color(255, 40, 30, 0) * (0.5f * pulse);
                for (int i = 0; i < 4; i++) {
                    Vector2 off = (MathHelper.PiOver2 * i).ToRotationVector2() * 2f;
                    spriteBatch.Draw(tex, npc.Center + off - screenPos, npc.frame, outline,
                        npc.rotation, origin, npc.scale, fx, 0f);
                }
            }
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float rot = Main.GlobalTimeWrappedHourly * 2f;
            float radius = npc.Size.Length() * 0.6f + 8f + pulse * 4f;
            for (int i = 0; i < 4; i++) {
                float a = rot + MathHelper.PiOver2 * i;
                Vector2 corner = npc.Center + a.ToRotationVector2() * radius - screenPos;
                spriteBatch.Draw(pixel, corner, new Rectangle(0, 0, 1, 1), Color.Red * pulse,
                    a + MathHelper.PiOver4, new Vector2(0.5f), new Vector2(12f, 2f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, corner, new Rectangle(0, 0, 1, 1), Color.Red * pulse,
                    a - MathHelper.PiOver4, new Vector2(0.5f), new Vector2(12f, 2f), SpriteEffects.None, 0f);
            }
        }
    }
}
