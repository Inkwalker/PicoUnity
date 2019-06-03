using UnityEngine;
using System;

namespace PicoUnity
{
    [CreateAssetMenu(menuName = "PicoUnity/Cartridge")]
    public class CartridgeTex : ACartridge
    {
        [SerializeField] Texture2D texture = default;

        [SerializeField][HideInInspector]
        private byte[] rom;
        [SerializeField][HideInInspector]
        private string lua;

        public byte Version { get; private set; }
        public int Build { get; private set; }
        public override byte[] Rom => rom;
        public override string Lua => lua;

        private void OnValidate()
        {
            rom = new byte[CART_SIZE];
            lua = "";

            if (texture != null)
            {
                LoadFromTexture(texture);
                lua = ExtractScript();
            }
        }

        private static byte DecodeColor32(Color32 c)
        {
            return (byte)(((c.a & 0x03) << 6) | ((c.r & 0x03) << 4) | ((c.g & 0x03) << 2) | (c.b & 0x03));
        }

        private bool ValidateTexture(Texture2D texture)
        {
            if (texture.width != 160 || texture.height != 205)
            {
                Debug.LogError("Wrong texture size.");
                return false;
            }
            if (!texture.isReadable)
            {
                Debug.LogError("Texture is not readable.");
                return false;
            }
            if (texture.format != TextureFormat.RGBA32)
            {
                Debug.LogError("Wrong texture format. Only RGBA 32bit is supported");
            }

            return true;
        }

        private void LoadFromTexture(Texture2D texture)
        {
            if (ValidateTexture(texture) == false)
                throw new BadImageFormatException();

            texture.alphaIsTransparency = false;

            var pixels = texture.GetPixels32();

            int offset = 0;
            for (int y = texture.height - 1; y >= 0 && offset < CART_SIZE; y--)
            {
                for (int x = 0; x < texture.width && offset < CART_SIZE; x++)
                {
                    Color32 color = pixels[y * texture.width + x];
                    rom[offset] = DecodeColor32(color);
                    offset++;
                }
            }

            Version = DecodeColor32(pixels[0x80]);
            Build = (DecodeColor32(pixels[0x81]) << 24) + (DecodeColor32(pixels[0x82]) << 16) + (DecodeColor32(pixels[0x83]) << 8) + DecodeColor32(pixels[0x84]);
        }

        public string ExtractScript()
        {
            string script = "";
            int index = 0x4300;
            while (index < ROM_SIZE && rom[index] != 0x00)
            {
                script += (char)rom[index];
                index++;
            }

            // uncompressed code
            if (!script.Equals(":c:")) return script;

            int size = (rom[0x4304] << 8) + rom[0x4305];
            index = 0x4308;

            script = "";
            string lut = "\n 0123456789abcdefghijklmnopqrstuvwxyz!#%(){}[]<>+=/*:;.,~_";

            // comsume compressed data
            while (script.Length < size && index < 0x8000)
            {
                byte current_byte = rom[index];
                if (current_byte == 0x00)
                {
                    byte next_byte = rom[++index];
                    script += (char)next_byte;
                }
                else if (current_byte < 0x3c)
                {
                    script += lut[current_byte - 0x01];
                }
                else
                {
                    byte next_byte = rom[++index];
                    int copy_offset = (current_byte - 0x3c) * 16 + (next_byte & 0x0f);
                    int copy_length = (next_byte >> 4) + 2;
                    script += script.Substring(script.Length - copy_offset, copy_length);
                }
                index++;
            }
            return script;
        }
    }
}
