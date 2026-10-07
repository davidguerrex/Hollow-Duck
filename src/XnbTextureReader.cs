using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace HollowDuck
{
    internal static class XnbTextureReader
    {
        public static Sprite LoadSprite(string path, int frameWidth, int frameHeight, int frameIndex)
        {
            XnbTexture textureData = ReadTexture(path);
            int columns = textureData.Width / frameWidth;
            int rows = textureData.Height / frameHeight;
            if (frameWidth <= 0 || frameHeight <= 0 || columns == 0 || frameIndex < 0 || frameIndex >= columns * rows)
            {
                throw new InvalidDataException("The requested frame is outside the XNB texture.");
            }

            Texture2D texture = new Texture2D(textureData.Width, textureData.Height, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(textureData.Pixels);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply(false, true);

            int x = frameIndex % columns * frameWidth;
            int y = textureData.Height - (frameIndex / columns + 1) * frameHeight;
            return Sprite.Create(
                texture,
                new Rect(x, y, frameWidth, frameHeight),
                new Vector2(0.5f, 0.5f),
                frameWidth);
        }

        private static XnbTexture ReadTexture(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
            {
                if (reader.ReadByte() != 'X' || reader.ReadByte() != 'N' || reader.ReadByte() != 'B')
                {
                    throw new InvalidDataException("Not an XNB asset.");
                }

                if (reader.ReadByte() != 'w' || reader.ReadByte() != 5)
                {
                    throw new InvalidDataException("Only Windows XNB version 5 assets are supported.");
                }

                byte flags = reader.ReadByte();
                int fileSize = reader.ReadInt32();
                if ((flags & 0xC0) != 0 || fileSize != stream.Length)
                {
                    throw new InvalidDataException("Compressed or malformed XNB assets are not supported.");
                }

                int readerCount = Read7BitEncodedInt(reader);
                bool hasTextureReader = false;
                for (int i = 0; i < readerCount; i++)
                {
                    string readerName = ReadString(reader);
                    reader.ReadInt32();
                    hasTextureReader |= readerName.StartsWith(
                        "Microsoft.Xna.Framework.Content.Texture2DReader,",
                        StringComparison.Ordinal);
                }

                int sharedResourceCount = Read7BitEncodedInt(reader);
                int typeReaderIndex = Read7BitEncodedInt(reader);
                if (!hasTextureReader || typeReaderIndex <= 0 || typeReaderIndex > readerCount || sharedResourceCount != 0)
                {
                    throw new InvalidDataException("The XNB does not contain a supported standalone texture.");
                }

                int surfaceFormat = reader.ReadInt32();
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                int mipCount = reader.ReadInt32();
                if (surfaceFormat != 0 || width <= 0 || height <= 0 || mipCount <= 0)
                {
                    throw new InvalidDataException("Only XNA Color surface textures are supported.");
                }

                int dataLength = reader.ReadInt32();
                int expectedLength = checked(width * height * 4);
                if (dataLength != expectedLength || dataLength > stream.Length - stream.Position)
                {
                    throw new InvalidDataException("The XNB texture payload size is invalid.");
                }

                return new XnbTexture(width, height, reader.ReadBytes(dataLength));
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            int length = Read7BitEncodedInt(reader);
            return Encoding.UTF8.GetString(reader.ReadBytes(length));
        }

        private static int Read7BitEncodedInt(BinaryReader reader)
        {
            int value = 0;
            int shift = 0;
            byte current;
            do
            {
                if (shift == 35)
                {
                    throw new InvalidDataException("Invalid XNB 7-bit integer.");
                }

                current = reader.ReadByte();
                value |= (current & 0x7F) << shift;
                shift += 7;
            }
            while ((current & 0x80) != 0);

            return value;
        }

        private sealed class XnbTexture
        {
            public XnbTexture(int width, int height, byte[] pixels)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
            }

            public int Width { get; }
            public int Height { get; }
            public byte[] Pixels { get; }
        }
    }
}