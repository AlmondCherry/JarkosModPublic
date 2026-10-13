using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace jarkos.core.particles
{
    public abstract class Particles
    {
        private bool hasSpawned;
        public int ID
        {
            get;
            private set;
        }
        public Texture2D Texture
        {
            get;
            protected set;
        }
        public int Frame;
        public int Time;
        public int Lifetime;
        public int Direction;
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Rotation;
        public float Scale = 1f;
        public float Opacity = 1f;
        public float LifetimeRatio => Time / (float)Lifetime;
        public abstract string TexturePath
        {
            get;
        }
        public virtual int FrameCount => 1;
        public virtual BlendState DrawBlendState => BlendState.AlphaBlend;
        public virtual void Load()
        {
        }
        public virtual void Update()
        {
        }
        public virtual void Draw()
        {
            Rectangle frame = Texture.Frame(1, FrameCount, 0, Frame);
            SpriteEffects visualDirection = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, frame, Color * Opacity, Rotation, frame.Size() * 0.5f, Scale, visualDirection, 0f);
        }
        public void Spawn()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            if (hasSpawned)
                return;

            ID = ParticlesManagers.particleIDLookup[GetType()];
            Texture = ParticlesManagers.particleTextureLookup[ID];
            ParticlesManagers.activeParticles.Add(this);

            hasSpawned = true;
        }
        public void Kill() => Time = Lifetime;
    }
}
