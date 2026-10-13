using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;

namespace jarkos.UI
{
    public class TreeCamera
    {
        public Vector2 Offset;

        public float Zoom = 1f;
        public float TargetZoom = 1f;
        public float MinZoom = 0.35f;
        public float MaxZoom = 2.2f;

        private Rectangle _bounds;
        private bool _dragging;
        private Vector2 _dragLast;

        public void SetBounds(Rectangle worldBounds)
        {
            _bounds = worldBounds;
            Offset = worldBounds.Center.ToVector2();
            TargetZoom = 1f;
            Zoom = 1f;
        }

        private Vector2 ScreenCenter => new(Main.screenWidth / 2f, Main.screenHeight / 2f);
        public Vector2 WorldToScreen(Vector2 world) => (world - Offset) * Zoom + ScreenCenter;

        

        public void Update(bool overEmptyBackground)
        {
            var mouse = new Vector2(Main.mouseX, Main.mouseY);
            int scroll = PlayerInput.ScrollWheelDeltaForUI;

            if (scroll != 0)
            {
                float factor = scroll > 0 ? 1.25f : 1f / 1.25f;
                TargetZoom = MathHelper.Clamp(TargetZoom * factor, MinZoom, MaxZoom);
            }

            if (Math.Abs(Zoom - TargetZoom) > 0.001f)
            {
                float oldZoom = Zoom;
                
                Zoom = MathHelper.Lerp(Zoom, TargetZoom, 0.15f);
                Vector2 centerOffset = mouse - ScreenCenter;
                Offset += centerOffset / oldZoom - centerOffset / Zoom;
            }

            if (overEmptyBackground && Main.mouseLeft)
            {
                if (!_dragging) { _dragging = true; _dragLast = mouse; }
                else
                {
                    Offset -= (mouse - _dragLast) / Zoom;
                    _dragLast = mouse;
                }
            }
            else _dragging = false;
            ClampOffset();
        }
        private void ClampOffset()
        {
            Offset.X = MathHelper.Clamp(Offset.X, _bounds.Left, _bounds.Right);
            Offset.Y = MathHelper.Clamp(Offset.Y, _bounds.Top, _bounds.Bottom);
        }
    }
}
