﻿using FixedPointy;
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

        private int CameraX
        {
            get => memory.Peek2(MemoryModule.ADDR_CAMERA_X);
            set => memory.Poke2(MemoryModule.ADDR_CAMERA_X, (short)value);
        }

        private int CameraY
        {
            get => memory.Peek2(MemoryModule.ADDR_CAMERA_Y);
            set => memory.Poke2(MemoryModule.ADDR_CAMERA_Y, (short)value);
        }

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

        private byte ClipX0
        {
            get => memory.Peek(MemoryModule.ADDR_CLIP_X0);
            set => memory.Poke(MemoryModule.ADDR_CLIP_X0, value);
        }

        private byte ClipX1
        {
            get => memory.Peek(MemoryModule.ADDR_CLIP_X1);
            set => memory.Poke(MemoryModule.ADDR_CLIP_X1, value);
        }

        private byte ClipY0
        {
            get => memory.Peek(MemoryModule.ADDR_CLIP_Y0);
            set => memory.Poke(MemoryModule.ADDR_CLIP_Y0, value);
        }

        private byte ClipY1
        {
            get => memory.Peek(MemoryModule.ADDR_CLIP_Y1);
            set => memory.Poke(MemoryModule.ADDR_CLIP_Y1, value);
        }

        private byte PenColor
        {
            get => memory.Peek(MemoryModule.ADDR_PEN_COLOR);
            set => memory.Poke(MemoryModule.ADDR_PEN_COLOR, value);
        }

        private int LineX
        {
            get => memory.Peek2(MemoryModule.ADDR_LINE_X);
            set => memory.Poke2(MemoryModule.ADDR_LINE_X, (short)value);
        }

        private int LineY
        {
            get => memory.Peek2(MemoryModule.ADDR_LINE_Y);
            set => memory.Poke2(MemoryModule.ADDR_LINE_Y, (short)value);
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
            //for (int i = 0; i < buffer.Length; i++)
            //{
            //    byte lo = (byte)(buffer[i] & 0xf);
            //    byte hi = (byte)(buffer[i] >> 4);
                
            //    buffer[i] = (byte)(GetScreenColor(hi) << 4 | GetScreenColor(lo));
            //}

            Texture.LoadRawTextureData(buffer);
            Texture.Apply();

            Cls();
        }

        private byte GetDrawColor(byte penColor)
        {
            int col = penColor & 0xf;
            return memory.PeekHalf(MemoryModule.ADDR_PALETTE_0 + col, 0);
        }

        private int GetPatternColor(int x, int y, byte penColor)
        {
            int fillPattern = memory.Peek2(MemoryModule.ADDR_FILL);
            if (fillPattern != 0)
            {
                x = x % 4;
                y = y % 4;

                x = x < 0 ? -x : 3 - x;
                y = y < 0 ? -y : 3 - y;

                int i = y * 4 + x;
                int bit = fillPattern >> i & 1;
                bool transparent = memory.Peek(MemoryModule.ADDR_FILL_T) > 0;

                if (bit > 0) penColor = (byte)(penColor >> 4);
                else if (transparent) return -1;
            }

            return GetDrawColor(penColor);
        }

        private byte GetScreenColor(byte color)
        {
            return memory.PeekHalf(MemoryModule.ADDR_PALETTE_1 + color, 0);
        }

        private bool IsTransparent(byte color)
        {
            return memory.PeekHalf(MemoryModule.ADDR_PALETTE_0 + color, 1) > 0;
        }

        private int GetCharacterWidth(byte character)
        {
            if (character < 32) return 0; //special characters
            if (character < 128) return FontWidth; //normal characters;

            return FontWidth * 2; // double charactes
        }

        private float MapValue(float val, float in_min, float in_max, float out_min, float out_max)
        {
            return (val - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
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
                        DrawPixel(x + xx, y + yy, GetDrawColor(PenColor));
                    }
                }
            }
        }

        private void DrawLine(int x0, int y0, int x1, int y1, int? color = null)
        {
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
                Pset((int)Math.Round(x), (int)Math.Round(y), color);

                x = x + xIncrement;
                y = y + yIncrement;
            }
        }

        private void DrawPixel(int x, int y, byte color)
        {
            byte clip_x0 = memory.Peek(MemoryModule.ADDR_CLIP_X0);
            byte clip_x1 = memory.Peek(MemoryModule.ADDR_CLIP_X1);
            byte clip_y0 = memory.Peek(MemoryModule.ADDR_CLIP_Y0);
            byte clip_y1 = memory.Peek(MemoryModule.ADDR_CLIP_Y1);

            int screen_x = x - CameraX;
            int screen_y = y - CameraY;

            if (screen_x < clip_x0 || screen_y < clip_y0 || screen_x > clip_x1 || screen_y > clip_y1) return;

            memory.PokeHalf(MemoryModule.ADDR_VRAM, screen_y * ScreenWidth + screen_x, color);
        }

        #region Pico8 API

        public void Camera(int? x = 0, int? y = 0)
        {
            x = x.HasValue ? x : 0;
            y = y.HasValue ? y : 0;

            CameraX = x.Value;
            CameraY = y.Value;
        }

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
                DrawLine(x0.Value - x, y0.Value + y, x0.Value + x, y0.Value + y, col);
                DrawLine(x0.Value - y, y0.Value + x, x0.Value + y, y0.Value + x, col);
                DrawLine(x0.Value - x, y0.Value - y, x0.Value + x, y0.Value - y, col);
                DrawLine(x0.Value - y, y0.Value - x, x0.Value + y, y0.Value - x, col);

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
            col = col.HasValue ? col : 0;

            byte color = (byte)(col.Value & 0xf);
            byte val   = (byte)(color | color << 4);
            memory.MemSet(MemoryModule.ADDR_VRAM, val, MemoryModule.SIZE_VRAM);

            Cursor();
        }

        public void Clip(int x = 0, int y = 0, int w = 127, int h = 127)
        {
            int x1 = x + w;
            int y1 = y + h;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x1 >= 128) x1 = 127;
            if (y1 >= 128) y1 = 127;

            ClipX0 = (byte)x;
            ClipX1 = (byte)x1;
            ClipY0 = (byte)y;
            ClipY1 = (byte)y1;
        }

        public void Color(int? col = 0)
        {
            col = col.HasValue ? col : 0;

            PenColor = (byte)col.Value;
        }

        public void Cursor(int? x = 0, int? y = 0, int? col = null)
        {
            x = x.HasValue ? x : 0;
            y = y.HasValue ? y : 0;

            CursorX = (byte)x.Value;
            CursorY = (byte)y.Value;

            if (col.HasValue) Color(col.Value);
        }

        public void Fillp(Fix? pat = null)
        {
            if (pat.HasValue == false)
            {
                memory.Poke2(MemoryModule.ADDR_FILL, 0);
                memory.Poke(MemoryModule.ADDR_FILL_T, 0);
                return;
            }

            int p = (int)pat.Value;
            int t = pat.Value.Raw & 1;

            memory.Poke2(MemoryModule.ADDR_FILL, (short)p);
            memory.Poke(MemoryModule.ADDR_FILL_T, (byte)t);
        }

        public object Fget(int n = -1, byte? f = null)
        {
            if (n < 0 || n >= 256) return null;
            byte flag = memory.Peek(MemoryModule.ADDR_FLAGS + n);
            if (!f.HasValue)
            {
                return flag;
            }
            byte mask = (byte)(1 << f.Value);
            return ((flag & mask) > 0);
        }

        public void Fset(int n = -1, byte? f = null, bool? val = null)
        {
            if (n < 0 || n >= 256) return;
            if (!f.HasValue) return;
            byte flag = 0;
            if (val.HasValue)
            {
                flag = (byte)Fget(n);
                if (val.Value)
                {
                    byte mask = (byte)(1 << f.Value);
                    flag = (byte)(flag | mask);

                }
                else
                {
                    byte mask = (byte)(0 << f.Value);
                    flag = (byte)(flag & mask);
                }
            }
            else
            {
                flag = f.Value;
            }
            memory.Poke(MemoryModule.ADDR_FLAGS + n, (byte)flag);
        }

        public void Line(int x0 = 0, int y0 = 0, int? x1 = null, int? y1 = null, int? col = null)
        {
            if (!x1.HasValue || !y1.HasValue)
            {
                x1 = x0;
                y1 = y0;

                x0 = LineX;
                y0 = LineY;
            }

            LineX = x1.Value;
            LineY = y1.Value;

            DrawLine(x0, y0, x1.Value, y1.Value, col);
        }

        public byte Pget(int? x, int? y)
        {
            x = x.HasValue ? x : 0;
            y = y.HasValue ? y : 0;

            int real_x = x.Value - CameraX;
            int real_y = y.Value - CameraY;

            if (real_x < 0 || real_x >= ScreenWidth || real_y < 0 || real_y >= ScreenHeight) return 0;

            return memory.PeekHalf(MemoryModule.ADDR_VRAM, real_y * ScreenWidth + real_x);
        }

        public void Pset(int? x, int? y, int? col)
        {
            x = x.HasValue ? x : 0;
            y = y.HasValue ? y : 0;

            if (col.HasValue)
                Color(col.Value);

            var color = GetPatternColor(x.Value, y.Value, PenColor);

            if (color > 0)
                DrawPixel(x.Value, y.Value, (byte)color);
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
            DrawLine(x0, y0, x1, y0, col);
            DrawLine(x0, y0, x0, y1, col);
            DrawLine(x1, y0, x1, y1, col);
            DrawLine(x0, y1, x1, y1, col);
        }

        public void Rectfill(int x0 = 0, int y0 = 0, int x1 = 0, int y1 = 0, int? col = null)
        {
            int xMin = Math.Min(x0, x1);
            int yMin = Math.Min(y0, y1);

            int xMax = Math.Max(x0, x1);
            int yMax = Math.Max(y0, y1);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
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

        public void Spr(int n = 0, int x = 0, int y = 0, double w = 1, double h = 1, bool flip_x = false, bool flip_y = false)
        {
            int width = (int)(8 * w);
            int height = (int)(8 * h);

            int spr_x = (n % 16) * 8;
            int spr_y = (n / 16) * 8;

            for (int yy = 0; yy < height; yy++)
            {
                int draw_y = y + yy;
                int s_y = spr_y + yy;
                int read_y = flip_y ? spr_y + (height - yy - 1) : spr_y + yy;

                for (int xx = 0; xx < width; xx++)
                {
                    int draw_x = x + xx;
                    int read_x = flip_x ? spr_x + (width - xx - 1)  : spr_x + xx;
  
                    byte color = Sget(read_x, read_y);

                    if (IsTransparent(color)) continue;

                    DrawPixel(draw_x, draw_y, GetDrawColor(color));
                }
            }
        }

        public void Sspr(int sx, int sy, int sw, int sh, int dx, int dy, int? dw, int? dh, bool flip_x = false, bool flip_y = false)
        {
            int dest_w = dw.HasValue ? dw.Value : sw;
            int dest_h = dh.HasValue ? dh.Value : sh;

            if (dest_w == 0 || dest_h == 0) return;

            for (int yy = 0; yy < dest_h; yy++)
            {
                int draw_y = dy + yy;

                int map_y = (int)MapValue(yy, 0, dest_h, 0, sh);
                int sprite_y = flip_y ? sy + (sh - map_y - 1) : sy + map_y;

                for (int xx = 0; xx < dest_w; xx++)
                {
                    int draw_x = dx + xx;
                    int map_x = (int)MapValue(xx, 0, dest_w, 0, sw);
                    int sprite_x = flip_x ? sx + (sw - map_x - 1) : sx + map_x;

                    byte color = Sget(sprite_x, sprite_y);

                    if (IsTransparent(color)) continue;

                    DrawPixel(draw_x, draw_y, GetDrawColor(color));
                }
            }
        }

        public void Map(int celx = 0, int cely = 0, int sx = 0, int sy = 0, int celw = 16, int celh = 16, int layer = 0)
        {
            int cel_y1 = cely + celh;
            int cel_x1 = celx + celw;
            for (int cy_i = cely; cy_i < cel_y1; cy_i++)
            {
                if (cy_i < 0 || cy_i >= 64) continue;
                int map_offset = MemoryModule.ADDR_MAP;
                int cy = cy_i;
                if (cy_i >= 32)
                {
                    map_offset = MemoryModule.ADDR_SHARED;
                    cy -= 32;
                }
                for (int cx = celx; cx < cel_x1; cx++)
                {
                    if (cx < 0 || cx >= 128) continue;
                    int mem_offset = map_offset + cy * 128 + cx;
                    int spr_n = memory.Peek(mem_offset);

                    if (spr_n == 0) continue;

                    byte flag = (byte)Fget(spr_n);
                    if ((flag & layer) != layer)
                    {
                        continue;
                    }
                    Spr(spr_n, sx + (cx - celx) * 8, sy + (cy_i - cely) * 8, 1, 1);
                }
            }
        }

        public byte Mget(int x = 0, int y = 0)
        {
            if (x < 0 || x >= 128 || y < 0 || y >= 128) return 0;
            int map_offset = MemoryModule.ADDR_MAP;
            if (y >= 32)
            {
                map_offset = MemoryModule.ADDR_SHARED;
                y -= 32;
            }
            int mem_offset = map_offset + y * 128 + x;
            byte spr = memory.Peek(mem_offset);
            return spr;
        }

        public void Mset(int x = 0, int y = 0, byte v = 0)
        {
            if (x < 0 || x >= 128 || y < 0 || y >= 128) return;
            int map_offset = MemoryModule.ADDR_MAP;
            if (y >= 32)
            {
                map_offset = MemoryModule.ADDR_SHARED;
                y -= 32;
            }
            int mem_offset = map_offset + y * 128 + x;
            memory.Poke(mem_offset, v);
        }

        #endregion

        public override ApiTable GetApiTable()
        {
            return new ApiTable()
            {
                { "circ",     (Action<int?, int?, int, int?>)                 Circ },
                { "circfill", (Action<int?, int?, int, int?>)                 Circfill },
                { "cls",      (Action<int?>)                                  Cls },
                { "clip",     (Action<int, int, int, int>)                    Clip },
                { "camera",   (Action<int?, int?>)                            Camera },
                { "color",    (Action<int?>)                                  Color },
                { "cursor",   (Action<int?, int?, int?>)                      Cursor },
                { "fillp",    (Action<Fix?>)                                  Fillp },
                { "fget",     (Func<int, byte?, object>)                      Fget },
                { "fset",     (Action<int, byte?, bool?>)                     Fset },
                { "line",     (Action<int, int, int?, int?, int?>)            Line },
                { "pget",     (Func<int?, int?, byte>)                        Pget },
                { "pset",     (Action<int?, int?, int?>)                      Pset },
                { "pal",      (Action<byte?, byte?, byte?>)                   Pal },
                { "palt",     (Action<byte, bool?>)                           Palt },
                { "print",    (Action<string, int?, int?, int?>)              Print },
                { "rect",     (Action<int, int, int, int, int?>)              Rect },
                { "rectfill", (Action<int, int, int, int, int?>)              Rectfill },
                { "sget",     (Func<int, int, byte>)                          Sget },
                { "sset",     (Action<int, int, byte>)                        Sset },
                { "spr",      (Action<int, int, int, double, double, bool, bool>) Spr },
                { "sspr",     (Action<int, int, int, int, int, int, int?, int?, bool, bool>) Sspr },
                { "map",      (Action<int, int, int, int, int, int, int>)     Map },
                { "mget",     (Func<int, int, byte>)                          Mget },
                { "mset",     (Action<int, int, byte>)                        Mset },
            };
        }
    }
}
