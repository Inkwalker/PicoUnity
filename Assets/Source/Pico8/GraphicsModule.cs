using System;
using System.Collections.Generic;
using UnityEngine;

namespace PicoUnity
{
    public class GraphicsModule : EmulatorModule
    {
        public const int ScreenWidth  = 128;
        public const int ScreenHeight = 128;
        public const int FontWidth  = 4;
        public const int FontHeight = 6;

        private MemoryModule memory;
        private Texture2D fontTexture;
        private byte[] buffer;

        public Texture2D Texture { get; private set; }

        public GraphicsModule(MemoryModule memory, string font)
        {
            Texture = new Texture2D(ScreenWidth / 2, ScreenHeight, TextureFormat.R8, false, true);
            Texture.filterMode = FilterMode.Point;

            this.memory = memory;
            buffer = new byte[MemoryModule.SIZE_VRAM];

            fontTexture = Resources.Load<Texture2D>(font);

            Flip();
        }

        public void Flip()
        {
            memory.CopyTo(buffer, MemoryModule.ADDR_VRAM, 0, MemoryModule.SIZE_VRAM);
            Texture.LoadRawTextureData(buffer);
            Texture.Apply();
        }

        public void PokeScreen(int x, int y, byte color)
        {
            byte clip_x0 = memory.Peek(MemoryModule.ADDR_CLIP_X0);
            byte clip_x1 = memory.Peek(MemoryModule.ADDR_CLIP_X1);
            byte clip_y0 = memory.Peek(MemoryModule.ADDR_CLIP_Y0);
            byte clip_y1 = memory.Peek(MemoryModule.ADDR_CLIP_Y1);

            if (x < clip_x0 || y < clip_y0 || x > clip_x1 || y > clip_y1) return;

            memory.PokeHalf(MemoryModule.ADDR_VRAM, y * ScreenWidth + x, color);
        }

        #region Pico8 API

        public void Circ(int? x0 = null, int? y0 = null, int r = 4, int? col = null)
        {
            if (r < 0) return;

            x0 = x0.HasValue ? x0 : 0;
            y0 = y0.HasValue ? y0 : 0;

            int d = (5 - r * 4) / 4;
            int x = 0;
            int y = r;

            do
            {
                Pset(x + x0.Value, y + y0.Value, col);
                Pset(x + x0.Value, -y + y0.Value, col);
                Pset(-x + x0.Value, y + y0.Value, col);
                Pset(-x + x0.Value, -y + y0.Value, col);
                Pset(y + x0.Value, x + y0.Value, col);
                Pset(y + x0.Value, -x + y0.Value, col);
                Pset(-y + x0.Value, x + y0.Value, col);
                Pset(-y + x0.Value, -x + y0.Value, col);

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            }
            while (x <= y);
        }

        public void Circfill(int? x0 = null, int? y0 = null, int r = 4, int? col = null)
        {
            if (r < 0) return;

            x0 = x0.HasValue ? x0 : 0;
            y0 = y0.HasValue ? y0 : 0;

            int d = (5 - r * 4) / 4;
            int x = 0;
            int y = r;

            do
            {
                Line(x0.Value - x, y0.Value + y, x0.Value + x, y0.Value + y, col);
                Line(x0.Value - y, y0.Value + x, x0.Value + y, y0.Value + x, col);
                Line(x0.Value - x, y0.Value - y, x0.Value + x, y0.Value - y, col);
                Line(x0.Value - y, y0.Value - x, x0.Value + y, y0.Value - x, col);

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            }
            while (x <= y);
        }

        public void Cls(int? col = 0)
        {
            //TODO: pal() support
            col = col.HasValue ? col : 0;

            byte color = (byte)(col.Value & 0xf);
            byte val   = (byte)(color | color << 4);
            memory.MemSet(MemoryModule.ADDR_VRAM, val, MemoryModule.SIZE_VRAM);

            Cursor();
        }

        public void Color(int? col = 0)
        {
            col = col.HasValue ? col : 0;

            memory.Poke(MemoryModule.ADDR_PEN_COLOR, (byte)(col.Value & 0xf));
        }

        public void Cursor(int? x = 0, int? y = 0, int? col = null)
        {
            x = x.HasValue ? x : 0;
            y = y.HasValue ? y : 0;

            memory.Poke(MemoryModule.ADDR_CURSOR_X, (byte)x.Value);
            memory.Poke(MemoryModule.ADDR_CURSOR_Y, (byte)y.Value);

            if (col.HasValue) Color(col.Value);
        }
        public void Line(int x0 = 0, int y0 = 0, int x1 = 0, int y1 = 0, int? col = null)
        {
            //TODO: lineTo

            int dx = x1 - x0;
            int dy = y1 - y0;

            int steps;

            if (Math.Abs(dx) > Math.Abs(dy))
                steps = Math.Abs(dx);
            else
                steps = Math.Abs(dy);

            float xIncrement = dx / (float)steps;
            float yIncrement = dy / (float)steps;

            float x = x0;
            float y = y0;

            for (int v = 0; v <= steps; v++)
            {
                Pset((int)Math.Round(x), (int)Math.Round(y), col);

                x = x + xIncrement;
                y = y + yIncrement;
            }
        }

        public byte Pget(int? x, int? y)
        {
            x = x.HasValue ? x : 0;
            y = y.HasValue ? y : 0;

            //TODO: camera position
            int real_x = x.Value;
            int real_y = y.Value;

            if (real_x < 0 || real_x >= ScreenWidth || real_y < 0 || real_y >= ScreenHeight) return 0;

            return memory.PeekHalf(MemoryModule.ADDR_VRAM, real_y * ScreenWidth + real_x);
        }

        public void Pset(int? x, int? y, int? col)
        {
            x = x.HasValue ? x : 0;
            y = y.HasValue ? y : 0;

            //TODO: camera position
            int real_x = x.Value;
            int real_y = y.Value;

            if (col.HasValue)
                Color(col.Value);

            if (real_x < 0 || real_x >= ScreenWidth || real_y < 0 || real_y >= ScreenHeight) return;

            //TODO: pal() support
            PokeScreen(real_x, real_y, memory.Peek(MemoryModule.ADDR_PEN_COLOR));
        }

        public void Rect(int x0 = 0, int y0 = 0, int x1 = 0, int y1 = 0, int? col = null)
        {
            Line(x0, y0, x1, y0, col);
            Line(x0, y0, x0, y1, col);
            Line(x1, y0, x1, y1, col);
            Line(x0, y1, x1, y1, col);
        }

        public void Rectfill(int x0 = 0, int y0 = 0, int x1 = 0, int y1 = 0, int? col = null)
        {
            int xMin = Math.Min(x0, x1);
            int yMin = Math.Min(y0, y1);

            int xMax = Math.Max(x0, x1);
            int yMax = Math.Max(y0, y1);

            for (int y = yMin; y <= yMax; y++)
            {
                if (y < 0 || y >= 128) continue;
                for (int x = xMin; x <= xMax; x++)
                {
                    if (x < 0 || x >= 128) continue;
                    Pset(x, y, col);
                }
            }
        }

        #endregion

        public override ApiTable GetApiTable()
        {
            return new ApiTable()
            {
                { "circ",     (Action<int?, int?, int, int?>)    Circ },
                { "circfill", (Action<int?, int?, int, int?>)    Circfill },
                { "cls",      (Action<int?>)                     Cls },
                { "color",    (Action<int?>)                     Color },
                { "cursor",   (Action<int?, int?, int?>)         Cursor },
                { "line",     (Action<int, int, int, int, int?>) Line },
                { "pget",     (Func<int?, int?, byte>)           Pget },
                { "pset",     (Action<int?, int?, int?>)         Pset },
                { "rect",     (Action<int, int, int, int, int?>) Rect },
                { "rectfill", (Action<int, int, int, int, int?>) Rectfill },
            };
        }
    }
}
