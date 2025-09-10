using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    // Klasa reprezentująca plik WAD z prawdziwym parsowaniem katalogu lumpów.
    public class WadFile
    {
        private string filePath;
        private Dictionary<string, LumpEntry> lumpDictionary;

        // Struktura przechowująca wpis katalogowy lumpa.
        private class LumpEntry
        {
            public int filePos;
            public int diskSize;
            public int size;
            public byte type;
            public byte compression;
            public short pad;
            public string name;
        }

        public WadFile(string wadFilePath)
        {
            filePath = wadFilePath;
            lumpDictionary = new Dictionary<string, LumpEntry>(StringComparer.OrdinalIgnoreCase);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                // Odczytujemy magiczną wartość – oczekujemy "WAD3"
                string magic = new string(br.ReadChars(4));
                if (magic != "WAD3")
                    throw new Exception("Plik nie jest poprawnym WAD3.");

                int lumpCount = br.ReadInt32();
                int dirOffset = br.ReadInt32();

                fs.Seek(dirOffset, SeekOrigin.Begin);
                for (int i = 0; i < lumpCount; i++)
                {
                    LumpEntry entry = new LumpEntry();
                    entry.filePos = br.ReadInt32();
                    entry.diskSize = br.ReadInt32();
                    entry.size = br.ReadInt32();
                    entry.type = br.ReadByte();
                    entry.compression = br.ReadByte();
                    entry.pad = br.ReadInt16();
                    char[] nameChars = br.ReadChars(16);
                    entry.name = new string(nameChars).TrimEnd('\0');
                    lumpDictionary[entry.name] = entry;
                }
            }
        }

        // Metoda wyszukująca lumpa po nazwie i parsująca go jako MipTex.
        public WadLump GetFile(string textureName)
        {
            if (!lumpDictionary.TryGetValue(textureName, out LumpEntry entry))
                return null;

            byte[] lumpData;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(entry.filePos, SeekOrigin.Begin);
                lumpData = new byte[entry.diskSize];
                fs.Read(lumpData, 0, entry.diskSize);
            }

            // Zakładamy, że lump zawiera teksturę w formacie MipTex.
            return new MipTex(lumpData);
        }
    }

    // Bazowa klasa lumpa.
    public abstract class WadLump
    {
        public string Name;
    }

    // Klasa reprezentująca teksturę MipTex – parsowanie nagłówka, danych pikseli i palety.
    public class MipTex : WadLump
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Texture2D LoadedTexture { get; private set; }

        // Konstruktor parsuje dane binaryczne lumpa.
        public MipTex(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                // Nagłówek MipTex: 16 bajtów nazwy, 4 bajty szerokości, 4 bajty wysokości
                char[] nameChars = br.ReadChars(16);
                Name = new string(nameChars).TrimEnd('\0');
                Width = br.ReadInt32();
                Height = br.ReadInt32();

                // Odczytujemy offsety dla 4 poziomów mipmap.
                int[] offsets = new int[4];
                for (int i = 0; i < 4; i++)
                    offsets[i] = br.ReadInt32();

                // Minimalny rozmiar nagłówka wynosi 40 bajtów.
                int headerSize = 16 + 4 + 4 + 4 * 4;
                if (offsets[0] < headerSize || offsets[0] >= data.Length)
                    throw new Exception("Nieprawidłowy offset mipmapy 0 dla tekstury: " + Name);

                // Odczytujemy dane pikseli najwyższej rozdzielczości.
                int mipSize = Width * Height;
                ms.Seek(offsets[0], SeekOrigin.Begin);
                byte[] pixelIndices = br.ReadBytes(mipSize);

                // Paleta jest umieszczona na końcu lumpa: 2 bajty wskaźnika + 256*3 bajtów (768)
                int paletteSectionSize = 2 + 256 * 3;
                if (data.Length < paletteSectionSize)
                    throw new Exception("Zbyt mało danych, by odczytać paletę dla tekstury: " + Name);

                ms.Seek(data.Length - paletteSectionSize, SeekOrigin.Begin);
                short paletteIndicator = br.ReadInt16(); // Zwykle ignorowany.
                byte[] paletteData = br.ReadBytes(256 * 3);

                // Konwersja palety: każdy kolor zapisany jest jako 3 bajty (RGB).
                Color[] palette = new Color[256];
                for (int i = 0; i < 256; i++)
                {
                    int r = paletteData[i * 3];
                    int g = paletteData[i * 3 + 1];
                    int b = paletteData[i * 3 + 2];
                    palette[i] = new Color(r / 255f, g / 255f, b / 255f, 1f);
                }

                // Konwersja indeksów pikseli na rzeczywiste kolory.
                Color[] colors = new Color[mipSize];
                for (int i = 0; i < mipSize; i++)
                {
                    byte index = pixelIndices[i];
                    colors[i] = palette[index];
                }

                // Tworzymy Texture2D z danymi RGBA32.
                LoadedTexture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                LoadedTexture.SetPixels(colors);
                LoadedTexture.Apply();
            }
        }

        public Texture2D LoadTexture()
        {
            return LoadedTexture;
        }
    }
}
