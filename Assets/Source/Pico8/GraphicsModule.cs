using FixedPointy;
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

        #region Draw State

        private byte CursorX
        {
            get => memory.Peek(MemoryModule.ADDR_CURSOR_X);
            set => memory.Poke(MemoryModule.ADDR_CURSOR_X, value);
        }

        private byte CursorY
        {
            get => memory.Peek(MemoryModule.ADDR_CURSOR_Y);
            set => memory.Poke(MemoryModule.ADDR_CURSOR_Y, value);
        }

        public byte PenColor
        {
            get => memory.Peek(MemoryModule.ADDR_PEN_COLOR);
            set => memory.Poke(MemoryModule.ADDR_PEN_COLOR, value);
        }

        #endregion

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

        public override void OnFrameEnd(float dt)
        {
            Flip();
        }

        public void Flip()
        {
            memory.CopyTo(buffer, MemoryModule.ADDR_VRAM, 0, MemoryModule.SIZE_VRAM);

            //TODO: color mapping in shaders
            for (int i = 0; i < buffer.Length; i++)
            {
                byte lo = (byte)(buffer[i] & 0xf);
                byte hi = (byte)(buffer[i] >> 4);
                
                buffer[i] = (byte)(GetScreenColor(hi) << 4 | GetScreenColor(lo));
            }

            Texture.LoadRawTextureData(buffer);
            Texture.Apply();
        }

        private void PokeScreen(int x, int y, byte color)
        {
            byte clip_x0 = memory.Peek(MemoryModule.ADDR_CLIP_X0);
            byte clip_x1 = memory.Peek(MemoryModule.ADDR_CLIP_X1);
            byte clip_y0 = memory.Peek(MemoryModule.ADDR_CLIP_Y0);
            byte clip_y1 = memory.Peek(MemoryModule.ADDR_CLIP_Y1);

            if (x < clip_x0 || y < clip_y0 || x > clip_x1 || y > clip_y1) return;

            memory.PokeHalf(MemoryModule.ADDR_VRAM, y * ScreenWidth + x, color);
        }

        private byte GetDrawColor(byte penColor)
        {
            return memory.PeekHalf(MemoryModule.ADDR_PALETTE_0 + penColor, 0);
        }

        private byte GetScreenColor(byte color)
        {
            return memory.PeekHalf(MemoryModule.ADDR_PALETTE_1 + color, 0);
        }

        private bool IsTransperent(byte color)
        {
            return memory.PeekHalf(MemoryModule.ADDR_PALETTE_0 + color, 1) > 0;
        }

        private int GetCharacterWidth(byte character)
        {
            if (character < 32) return 0; //special characters
            if (character < 128) return FontWidth; //normal characters;

            return FontWidth * 2; // double charactes
        }

        private void DrawCharacter(int x, int y, byte character)
        {
            if (character < 32) return; //skip special characters

            int font_index = character - 32;
            font_index = font_index > 95 ? (font_index - 96) * 2 + 96 : font_index; //double characters, 96 single characters in the font

            int tex_w = fontTexture.width / FontWidth;

            int char_x = font_index % tex_w * FontWidth;
            int char_y = fontTexture.height - font_index / tex_w * FontHeight;

            int char_w = GetCharacterWidth(character);

            for (int xx = 0; xx < char_w; xx++)
            {
                for (int yy = 0; yy < FontHeight; yy++)
                {
                    var c = fontTexture.GetPixel(char_x + xx, char_y - yy - 1);

                    if (c.grayscale > 0.5f)
                    {
                        Pset(x + xx, y + yy, null);
                    }
                }
            }
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

            PenColor = (byte)(col.Value & 0xf);
        }

        public void Cursor(int? x = 0, int? y = 0, int? col = null)
        {
            x = x.HasValue ? x : 0;
            y = y.HasValue ? y : 0;

            CursorX = (byte)x.Value;
            CursorY = (byte)y.Value;

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

            PokeScreen(real_x, real_y, GetDrawColor(PenColor));
        }

        public void Pal(byte? c0 = 0, byte? c1 = null, byte? p = 0)
        {
            if (c1.HasValue == false)
            {
                memory.Poke(MemoryModule.ADDR_PALETTE_0, 0x10);
                for (byte i = 1; i < 16; i++)
                {
                    memory.Poke(MemoryModule.ADDR_PALETTE_0 + i, i);
                }
                return;
            }

            int addr = (p == 0) ? MemoryModule.ADDR_PALETTE_0 : MemoryModule.ADDR_PALETTE_1;
            memory.PokeHalf(addr + (c0.Value & 0xf), 0, (byte)(c1.Value & 0xf));
        }

        public void Palt(byte c = 0, bool? t = false)
        {
            if (!t.HasValue)
            {
                memory.PokeHalf(MemoryModule.ADDR_PALETTE_0, 1, 1);
                for (byte i = 1; i < 16; i++)
                {
                    memory.PokeHalf(MemoryModule.ADDR_PALETTE_0 + i, 1, 0);
                }
                return;
            }

            memory.PokeHalf(MemoryModule.ADDR_PALETTE_0 + (c & 0xf), 1, (byte)(t.Value ? 1 : 0));
        }

        public void Print(string str, int? x, int? y, int? col = null)
        {
            if (str == null) str = "";

            var chars = str.ToCharArray();
            var ascii = System.Array.ConvertAll(chars, c => (byte)c);

            int start_x = x.HasValue ? x.Value : CursorX;
            int start_y = y.HasValue ? y.Value : CursorY;

            if (col.HasValue) Color(col.Value);

            int xx = start_x;
            int yy = start_y;

            Cursor(start_x, start_y);

            for (int i = 0; i < ascii.Length; i++)
            {
                if (ascii[i] == 10) //new line
                {
                    xx = start_x;
                    yy += 6;

                    if (y.HasValue == false)
                        CursorY += 6;
                }
                else
                {
                    DrawCharacter(xx, yy, ascii[i]);
                    xx += GetCharacterWidth(ascii[i]);
                }
            }

            if (y.HasValue == false)
                CursorY += 6;
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

        public byte Sget(int x = 0, int y = 0)
        {
            return memory.PeekHalf(MemoryModule.ADDR_SPRITE, y * 128 + x);
        }

        public void Sset(int x = 0, int y = 0, byte c = 0)
        {
            memory.PokeHalf(MemoryModule.ADDR_SPRITE, y * 128 + x, 0);
        }

        public void Spr(int n, int x, int y, Fix? w, Fix? h, bool flip_x = false, bool flip_y = false)
        {
            w = w.HasValue ? w : 1;
            h = h.HasValue ? h : 1;

            // TODO: flip
            int width = (int)(8 * w.Value);
            int height = (int)(8 * h.Value);

            int spr_x = (n % 16) * 8;
            int spr_y = (n / 16) * 8;

            for (int yy = 0; yy < height; yy++)
            {
                int screen_y = y + yy;// - CameraY;
                int s_y = spr_y + yy;

                if (screen_y < 0 || screen_y >= 128) continue;

                for (int xx = 0; xx < width; xx++)
                {
                    int screen_x = x + xx;// - CameraX;

                    if (screen_x < 0 || screen_x >= 128) continue;

                    byte color = 0;

                    int read_x = flip_x ? spr_x + (width - xx - 1)  : spr_x + xx;
                    int read_y = flip_y ? spr_y + (height - yy - 1) : spr_y + yy;
  
                    color = Sget(read_x, read_y);

                    if (IsTransperent(color)) continue;

                    PokeScreen(screen_x, screen_y, GetDrawColor(color));
                }
            }
        }

        #endregion

        public override ApiTable GetApiTable()
        {
            return new ApiTable()
            {
                { "circ",     (Action<int?, int?, int, int?>)                 Circ },
                { "circfill", (Action<int?, int?, int, int?>)                 Circfill },
                { "cls",      (Action<int?>)                                  Cls },
                { "color",    (Action<int?>)                                  Color },
                { "cursor",   (Action<int?, int?, int?>)                      Cursor },
                { "line",     (Action<int, int, int, int, int?>)              Line },
                { "pget",     (Func<int?, int?, byte>)                        Pget },
                { "pset",     (Action<int?, int?, int?>)                      Pset },
                { "pal",      (Action<byte?, byte?, byte?>)                   Pal },
                { "palt",     (Action<byte, bool?>)                           Palt },
                { "print",    (Action<string, int?, int?, int?>)              Print },
                { "rect",     (Action<int, int, int, int, int?>)              Rect },
                { "rectfill", (Action<int, int, int, int, int?>)              Rectfill },
                { "sget",     (Func<int, int, byte>)                          Sget },
                { "sset",     (Action<int, int, byte>)                        Sset },
                { "spr",      (Action<int, int, int, Fix?, Fix?, bool, bool>) Spr },
            };
        }
    }
}
