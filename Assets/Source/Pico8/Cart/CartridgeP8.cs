using PicoUnity;
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
                ReadMusic(reader);

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

            lua = ReadSection(reader, out delimiter);

            if (lua == null) lua = "";

            return delimiter;
        }

        private string ReadGfx(StringReader reader)
        {
            string delimiter;

            string gfx = ReadSection(reader, out delimiter);

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

            string gff = ReadSection(reader, out delimiter);

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

            string label = ReadSection(reader, out delimiter);

            return delimiter;
        }

        private string ReadMap(StringReader reader)
        {
            string delimiter;

            string map = ReadSection(reader, out delimiter);

            if (string.IsNullOrEmpty(map) == false)
            {
                for (int i = 0; i < map.Length; i += 2)
                {
                    byte hi = HexToByte(map[i]);
                    byte lo = HexToByte(map[i + 1]);

                    rom[MemoryModule.ADDR_MAP + i / 2] = (byte)(hi << 4 | lo);
                }
            }

            return delimiter;
        }

        private string ReadSfx(StringReader reader)
        {
            //TODO

            string delimiter;

            string sfx = ReadSection(reader, out delimiter);

            return delimiter;
        }

        private string ReadMusic(StringReader reader)
        {

            //TODO
            string delimiter;

            string music = ReadSection(reader, out delimiter);

            return delimiter;
        }

        private string ReadSection(StringReader reader, out string delimiter)
        {
            string line = reader.ReadLine();

            StringBuilder builder = new StringBuilder();
            while (line != null && !delimiters.Contains(line))
            {
                builder.AppendLine(line);
                line = reader.ReadLine();
            }

            delimiter = line;

            return builder.ToString();
        }

        private byte HexToByte(char digit)
        {
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
    }
}
