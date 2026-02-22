using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace StarDo.UI.Components
{
    public class ScrollableListComponent
    {
        private Rectangle bounds;
        private int scrollOffset;
        private int totalContentHeight;
        private bool isDragging;
        private int dragStartY;
        private int dragStartOffset;

        private const int ScrollbarWidth = 24;
        private const int ScrollbarPad = 8;

        public int ScrollOffset => this.scrollOffset;
        public Rectangle Bounds => this.bounds;

        public ScrollableListComponent(Rectangle bounds)
        {
            this.bounds = bounds;
        }

        public void UpdateBounds(Rectangle newBounds)
        {
            this.bounds = newBounds;
            this.ClampScroll();
        }

        public void SetContentHeight(int height)
        {
            this.totalContentHeight = height;
            this.ClampScroll();
        }

        public void Scroll(int direction)
        {
            this.scrollOffset += direction * 40;
            this.ClampScroll();
        }

        public bool HandleScrollWheel(int direction)
        {
            if (!this.NeedsScrolling())
                return false;

            this.scrollOffset -= direction / 3;
            this.ClampScroll();
            return true;
        }

        public bool HandleLeftClick(int x, int y)
        {
            if (!this.bounds.Contains(x, y))
                return false;

            var scrollbarBounds = this.GetScrollbarTrackBounds();
            if (scrollbarBounds.Contains(x, y) && this.NeedsScrolling())
            {
                this.isDragging = true;
                this.dragStartY = y;
                this.dragStartOffset = this.scrollOffset;
                return true;
            }

            return false;
        }

        public void HandleLeftClickHeld(int x, int y)
        {
            if (!this.isDragging)
                return;

            int trackHeight = this.GetScrollbarTrackBounds().Height;
            int maxScroll = this.MaxScroll();
            if (trackHeight <= 0 || maxScroll <= 0)
                return;

            float ratio = (float)(y - this.dragStartY) / trackHeight;
            this.scrollOffset = this.dragStartOffset + (int)(ratio * maxScroll);
            this.ClampScroll();
        }

        public void HandleLeftClickReleased()
        {
            this.isDragging = false;
        }

        public bool NeedsScrolling()
        {
            return this.totalContentHeight > this.bounds.Height;
        }

        public void BeginScissorRect(SpriteBatch b)
        {
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                null, new RasterizerState { ScissorTestEnable = true });
            b.GraphicsDevice.ScissorRectangle = this.bounds;
        }

        public void EndScissorRect(SpriteBatch b)
        {
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }

        public void DrawScrollbar(SpriteBatch b)
        {
            if (!this.NeedsScrolling())
                return;

            var track = this.GetScrollbarTrackBounds();
            int thumbHeight = Math.Max(40, (int)((float)this.bounds.Height / this.totalContentHeight * track.Height));
            int maxScroll = this.MaxScroll();
            float scrollFraction = maxScroll > 0 ? (float)this.scrollOffset / maxScroll : 0;
            int thumbY = track.Y + (int)(scrollFraction * (track.Height - thumbHeight));

            // Track
            b.Draw(Game1.staminaRect, track, Color.Black * 0.2f);

            // Thumb
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(403, 383, 6, 6),
                track.X, thumbY, track.Width, thumbHeight,
                this.isDragging ? Color.White : Color.LightGray, 4f, false);
        }

        public void Reset()
        {
            this.scrollOffset = 0;
        }

        private Rectangle GetScrollbarTrackBounds()
        {
            return new Rectangle(
                this.bounds.Right - ScrollbarWidth - ScrollbarPad,
                this.bounds.Y + ScrollbarPad,
                ScrollbarWidth,
                this.bounds.Height - ScrollbarPad * 2
            );
        }

        private int MaxScroll()
        {
            return Math.Max(0, this.totalContentHeight - this.bounds.Height);
        }

        private void ClampScroll()
        {
            this.scrollOffset = Math.Clamp(this.scrollOffset, 0, this.MaxScroll());
        }
    }
}
