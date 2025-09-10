using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using uSource;                 // uResourceManager, CommandLineArgs, EntityLoaderEnhancements
using static uSource.KeyValues; // KVSerializer, KVSection
using uSource.Formats.VBSP;    // VBSPDocument, VBSPLumpType

/// <summary>
/// Realistic BSP loader matching Source SDK 13 feature set via command-line flags.
/// Otwiera BSP, parsuje go i zwraca root GameObject z instancjami encji.
/// </summary>
public static class RealBSPLoader
{
    /// <summary>
    /// Ładuje mapę BSP i zwraca GameObject grupujący wszystkie encje.
    /// </summary>
    /// <param name="bspPath">Relatywna ścieżka, np. "maps/de_dust2.bsp"</param>
    public static GameObject Load(string bspPath)
    {
        // Otwieramy plik przez uResourceManager
        using (var stream = uResourceManager.OpenFile(bspPath, false))
        {
            if (stream == null)
                throw new FileNotFoundException($"BSP file not found: {bspPath}");

            // Parsujemy dokument BSP
            VBSPDocument doc = VBSPDocument.Parse(stream);

            // Tworzymy importera i aplikujemy wszystkie convarsy
            var importer = new BSPImporter(bspPath);
            importer.ApplyConVars(CommandLineArgs.Args);

            // Importujemy do Unity
            return importer.ImportToUnity();
        }
    }
}

/// <summary>
/// Kompletny importer mapy BSP:
/// - wykorzystuje uResourceManager
/// - parsuje lump ENTITIES
/// - instancjuje prefabrykaty z Resources/Entities
/// - rejestruje pola, wywołuje Precache() i Think()
/// </summary>
public class BSPImporter
{
    private readonly string _bspPath;
    private readonly Dictionary<string, string> _conVars = new Dictionary<string, string>();
    private GameObject _root;

    /// <summary>
    /// Konstruktor przyjmuje ścieżkę do .bsp
    /// </summary>
    public BSPImporter(string bspPath)
    {
        if (string.IsNullOrEmpty(bspPath))
            throw new ArgumentException("Ścieżka do BSP nie może być pusta", nameof(bspPath));
        _bspPath = bspPath;
    }

    /// <summary>
    /// Zapisuje podane convarsy do globalnego CommandLineArgs i lokalnie.
    /// </summary>
    public void ApplyConVars(Dictionary<string, string> args)
    {
        foreach (var kv in args)
        {
            CommandLineArgs.Args[kv.Key] = kv.Value;
            _conVars[kv.Key] = kv.Value;
        }
    }

    /// <summary>
    /// Importuje BSP do Unity i zwraca GameObject-owy root.
    /// </summary>
    public GameObject ImportToUnity()
    {
        // Usuń wcześniej załadowany root
        if (_root != null)
            UnityEngine.Object.DestroyImmediate(_root);

        _root = new GameObject(Path.GetFileNameWithoutExtension(_bspPath));

        // Otwórz BSP przez uResourceManager (plik nadal zamknięty po RealBSPLoader.Load)
        using (var stream = uResourceManager.OpenFile(_bspPath, false))
        {
            // Parsuj cały dokument VBSP
            var doc = VBSPDocument.Parse(stream);

            // Wyciągnij lump ENTITIES
            var entLump = doc.Lumps.FirstOrDefault(l => l.Type == VBSPDocument.VBSPLumpType.Entities);
            if (entLump == null)
                throw new InvalidDataException("Brak lumpu ENTITIES w BSP");

            // Zamień bajty na tekst
            string kvText = System.Text.Encoding.UTF8.GetString(entLump.Data);

            // Zapisz tymczasowy plik do parsera KV
            string tempFile = Path.Combine(Application.temporaryCachePath,
                                           Path.GetFileName(_bspPath) + ".kv");
            File.WriteAllText(tempFile, kvText);

            // Parsuj drzewo KeyValues
            KeyValues.KVSection rootKv = KeyValues.KVSerializer.Load(tempFile);

            // Instancjuj każdą encję
            foreach (var sec in rootKv.Children)
            {
                string className = sec.Name;
                GameObject prefab = Resources.Load<GameObject>($"Entities/{className}");
                if (prefab == null)
                    continue;

                GameObject go = GameObject.Instantiate(prefab, _root.transform);
                var comp = go.GetComponent(className) ?? go.GetComponent(typeof(Component));
                if (comp != null)
                {
                    EntityLoaderEnhancements.RegisterFields(comp, sec);
                    EntityLoaderEnhancements.CallPrecache(comp);
                    EntityLoaderEnhancements.CallThink(comp);
                }
            }
        }

        return _root;
    }
}

/// <summary>
/// Dokument BSP (BSP v19 / Source SDK 13) z pełną listą lumpów.
/// </summary>
namespace uSource.Formats.VBSP
{
    public class VBSPDocument
    {
        private const int HEADER_LUMPS = 15;
        public int Version { get; private set; }
        public List<Lump> Lumps { get; private set; }

        private VBSPDocument()
        {
            Lumps = new List<Lump>(HEADER_LUMPS);
        }

        public static VBSPDocument Parse(Stream stream)
        {
            var doc = new VBSPDocument();
            using (var br = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                var magic = new string(br.ReadChars(4));
                if (magic != "VBSP")
                    throw new InvalidDataException($"Nieprawidłowy magic: {magic}");
                doc.Version = br.ReadInt32();

                var dir = new LumpDirectory[HEADER_LUMPS];
                for (int i = 0; i < HEADER_LUMPS; i++)
                {
                    dir[i] = new LumpDirectory {
                        Offset  = br.ReadInt32(),
                        Length  = br.ReadInt32(),
                        Version = br.ReadInt32(),
                        FourCC  = br.ReadInt32()
                    };
                }

                for (int i = 0; i < HEADER_LUMPS; i++)
                {
                    var d = dir[i];
                    byte[] data = new byte[d.Length];
                    if (d.Length > 0)
                    {
                        stream.Seek(d.Offset, SeekOrigin.Begin);
                        if (br.Read(data, 0, d.Length) != d.Length)
                            throw new EndOfStreamException($"Błąd wczytywania lumpu {i}");
                    }
                    doc.Lumps.Add(new Lump {
                        Type    = (VBSPLumpType)i,
                        Data    = data,
                        Version = d.Version,
                        FourCC  = d.FourCC
                    });
                }
            }
            return doc;
        }

        public class Lump
        {
            public VBSPLumpType Type   { get; internal set; }
            public byte[] Data         { get; internal set; }
            public int Version         { get; internal set; }
            public int FourCC          { get; internal set; }
        }

        public enum VBSPLumpType
        {
            Entities    = 0,
            Planes      = 1,
            Textures    = 2,
            Vertices    = 3,
            Visibility  = 4,
            Nodes       = 5,
            TextureInfo = 6,
            Faces       = 7,
            Lighting    = 8,
            Occlusion   = 9,
            Leaves      = 10,
            LeafFaces   = 11,
            Edges       = 12,
            FaceEdges   = 13,
            Models      = 14
        }

        private struct LumpDirectory
        {
            public int Offset, Length, Version, FourCC;
        }
    }
}
