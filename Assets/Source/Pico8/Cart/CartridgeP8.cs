using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PicoUnity
{
    public class CartridgeP8 : ACartridge
    {
        private static List<string> delimiters = new List<string>
        {
            "__lua__",
            "__gfx__",
            "__gff__",
            "__label__",
            "__map__",
            "__sfx__",
            "__music__",
        };

        [SerializeField][HideInInspector]
        private string lua;
        [SerializeField][HideInInspector]
        private byte[] rom;

        public override byte[] Rom => rom;
        public override string Lua => lua;

        public void ReadData(string text)
        {
            rom = new byte[CART_SIZE];

            StringReader reader = new StringReader(text);

            string delimiter;

            delimiter = ReadHeader(reader);
            delimiter = ReadLua(reader);

            while (!string.IsNullOrEmpty(delimiter))
            {
                if (delimiter == "__gfx__")
                    delimiter = ReadGfx(reader);

                if (delimiter == "__gff__")
                    delimiter = ReadGff(reader);

                if (delimiter == "__label__")
                    delimiter = ReadLabel(reader);

                if (delimiter == "__map__")
                    delimiter = ReadMap(reader);

                if (delimiter == "__sfx__")
                    delimiter = ReadSfx(reader);

                if (delimiter == "__music__")
                    delimiter = ReadMusic(reader);
            }

            reader.Dispose();
        }

        private string ReadHeader(StringReader reader)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(reader.ReadLine());
            builder.AppendLine(reader.ReadLine());

            string header = builder.ToString();

            return reader.ReadLine();
        }

        private string ReadLua(StringReader reader)
        {
            string delimiter;

            lua = ReadSection(reader, out delimiter, true);

            if (lua == null) lua = "";

            return delimiter;
        }

        private string ReadGfx(StringReader reader)
        {
            string delimiter;

            string gfx = ReadSection(reader, out delimiter, false);

            if (string.IsNullOrEmpty(gfx) == false)
            {
                for (int i = 0; i < gfx.Length; i += 2)
                {
                    byte lo = HexToByte(gfx[i]);
                    byte hi = HexToByte(gfx[i + 1]);

                    rom[MemoryModule.ADDR_SPRITE + i / 2] = (byte)(hi << 4 | lo);
                }
            }

            return delimiter;
        }

        private string ReadGff(StringReader reader)
        {
            string delimiter;

            string gff = ReadSection(reader, out delimiter, false);

            if (string.IsNullOrEmpty(gff) == false)
            {
                for (int i = 0; i < gff.Length; i += 2)
                {
                    byte hi = HexToByte(gff[i]);
                    byte lo = HexToByte(gff[i + 1]);

                    rom[MemoryModule.ADDR_FLAGS + i / 2] = (byte)(hi << 4 | lo);
                }
            }

            return delimiter;
        }

        private string ReadLabel(StringReader reader)
        {
            string delimiter;

            string label = ReadSection(reader, out delimiter, false);

            return delimiter;
        }

        private string ReadMap(StringReader reader)
        {
            string delimiter;

            string map = ReadSection(reader, out delimiter,false);

            if (string.IsNullOrEmpty(map) == false)
            {
                if (string.IsNullOrEmpty(map) == false)
                {
                    for (int i = 0; i < map.Length; i += 2)
                    {
                        byte hi = HexToByte(map[i]);
                        byte lo = HexToByte(map[i + 1]);

                        rom[MemoryModule.ADDR_MAP + i / 2] = (byte)(hi << 4 | lo);
                    }
                }
            }

            return delimiter;
        }

        private string ReadSfx(StringReader reader)
        {
            string delimiter;

            string section = ReadSection(reader, out delimiter, true);

            using (var sfxReader = new StringReader(section))
            {
                var sfx = sfxReader.ReadLine();

                int sfx_i = 0;
                while(sfx != null)
                {
                    int sfx_addr = MemoryModule.ADDR_SOUND + sfx_i * 68;

                    var editorMode   = ReadByte(sfx, 0);
                    var noteDuration = ReadByte(sfx, 2);
                    var loopStart    = ReadByte(sfx, 4);
                    var loopEnd      = ReadByte(sfx, 6);

                    for (int n = 0; n < 32; n++)
                    {
                        int i = 8 + n * 5;

                        var pitch    = ReadByte(sfx, i);
                        var waveform = HexToByte(sfx[i + 2]);
                        var volume   = HexToByte(sfx[i + 3]);
                        var effect   = HexToByte(sfx[i + 4]);

                        int bin = 0;
                        bin |= pitch & 0b111111;
                        bin |= (waveform & 0b111) << 6;
                        bin |= (volume & 0b111) << 9;
                        bin |= (effect & 0b111) << 12;
                        bin |= (waveform & 0b1000) << 15; //custom waveform

                        byte lo = (byte)(bin & 0xFF);
                        byte hi = (byte)(bin >> 8);

                        int addr = sfx_addr + n * 2;

                        Rom[addr]     = lo;
                        Rom[addr + 1] = hi;
                    }

                    Rom[sfx_addr + 64] = editorMode;
                    Rom[sfx_addr + 65] = noteDuration;
                    Rom[sfx_addr + 66] = loopStart;
                    Rom[sfx_addr + 67] = loopEnd;

                    sfx_i++;
                    sfx = sfxReader.ReadLine();
                }
            }

            return delimiter;
        }

        private string ReadMusic(StringReader reader)
        {
            string delimiter;

            string section = ReadSection(reader, out delimiter, true);

            using (var musicReader = new StringReader(section))
            {
                var frame = musicReader.ReadLine();

                int frame_i = 0;
                while (!string.IsNullOrEmpty(frame))
                {
                    int frame_addr = MemoryModule.ADDR_MUSIC + frame_i * 4; //4 bytes per frame

                    var flags = ReadByte(frame, 0);
                    var sfx_0 = ReadByte(frame, 3);
                    var sfx_1 = ReadByte(frame, 5);
                    var sfx_2 = ReadByte(frame, 7);
                    var sfx_3 = ReadByte(frame, 9);

                    int loop_start = (flags & 1) << 7;
                    int loop_end   = (flags & 2) << 6;
                    int stop       = (flags & 4) << 5;

                    Rom[frame_addr]     = (byte)(sfx_0 | loop_start);
                    Rom[frame_addr + 1] = (byte)(sfx_1 | loop_end);
                    Rom[frame_addr + 2] = (byte)(sfx_2 | stop);
                    Rom[frame_addr + 3] = sfx_3;

                    frame_i++;
                    frame = musicReader.ReadLine();
                }
            }

            return delimiter;
        }

        private string ReadSection(StringReader reader, out string delimiter, bool keepFormating)
        {
            string line = reader.ReadLine();

            StringBuilder builder = new StringBuilder();
            while (line != null && !delimiters.Contains(line))
            {
                if (keepFormating)
                    builder.AppendLine(line);
                else
                    builder.Append(line);
                line = reader.ReadLine();
            }

            delimiter = line;

            return builder.ToString();
        }

        private byte HexToByte(char digit)
        {
            digit = char.ToLower(digit);

            switch (digit)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'a': return 0xa;
                case 'b': return 0xb;
                case 'c': return 0xc;
                case 'd': return 0xd;
                case 'e': return 0xe;
                case 'f': return 0xf;

                default:
                    return 0;
            }
        }

        private byte ReadByte(string str, int offset)
        {
            byte hi = HexToByte(str[offset]);
            byte lo = HexToByte(str[offset + 1]);

            var b = (byte)(hi << 4 | lo);

            return b;
        }
    }
}
