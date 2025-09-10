using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

namespace uSource.Formats.Source.VPK
{
	internal class ArchiveParsingException : Exception
	{
		public ArchiveParsingException()
		{
		}

		public ArchiveParsingException(String message)
			: base(message)
		{
		}

		public ArchiveParsingException(String message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public sealed class VPKFile : IDisposable
	{
		public Boolean Loaded { get; private set; }
		public Boolean IsMultiPart
		{
			get
			{
				return Parts.Count > 1;
			}
		}

		private VPKReaderBase Reader { get; set; }
		private Boolean Disposed { get; set; } // To detect redundant calls

		public  Dictionary<String, VPKEntry> Entries = new Dictionary<String, VPKEntry>();
		internal Dictionary<Int32, VPKFilePart> Parts { get; } = new Dictionary<Int32, VPKFilePart>();
		internal VPKFilePart MainPart
		{
			get
			{
				return Parts[MainPartIndex];
			}
		}

		internal const Int32 MainPartIndex = -1;

		/// <summary>
		/// Loads the specified vpk archive by filename, if it's a _dir.vpk file it'll load related numbered vpks automatically
		/// </summary>
		/// <param name="FileName">A vpk archive ending in _dir.vpk</param>
		public VPKFile(String FileName)
		{
			Load(new FileStream(FileName, FileMode.Open, FileAccess.Read), FileName);
		}

		/// <summary>
		/// Loads the specified vpk archive by filename, if it's a _dir.vpk file it'll load related numbered vpks automatically
		/// </summary>
		/// <param name="FileName">A vpk archive ending in _dir.vpk</param>
		public void Load(String FileName)
		{
			Load(new FileStream(FileName, FileMode.Open, FileAccess.Read), FileName);
		}
		/// <summary>
		/// Zwraca zawartość pliku z archiwum w postaci tablicy bajtów.
		/// Rzuca wyjątkiem, jeśli pliku nie ma lub nie uda się go w całości odczytać.
		/// </summary>
		public byte[] GetFileBytes(string pathInVpk)
		{
			if (!Entries.TryGetValue(pathInVpk, out VPKEntry entry))
				throw new FileNotFoundException($"Entry '{pathInVpk}' not found in VPK.");

			return ReadFileData(entry);
		}

		/// <summary>
		/// Bezpieczny wariant – zwraca true/false zamiast wyjątku.
		/// </summary>
		public bool TryGetFileBytes(string pathInVpk, out byte[] data)
		{
			if (!Entries.TryGetValue(pathInVpk, out VPKEntry entry))
			{
				data = null;
				return false;
			}

			data = ReadFileData(entry);
			return data != null && data.Length > 0;
		}

		/// <summary>
		/// The main Load function, the related parts need to be numbered correctly as "archivename_01.vpk" and so forth
		/// </summary>
		/// <param name="Stream"></param>
		/// <param name="FileName"></param>
		public void Load(Stream Stream, String FileName = "")
		{
			if (Loaded)
				throw new NotSupportedException("Tried to call Load on a VpkArchive that is already loaded, dispose and create a new one instead");

			if (String.IsNullOrEmpty(FileName))
				throw new FileLoadException("File name is empty!!!");

			Reader = new VPKReaderBase(Stream);

			UInt32 Signature = Reader.ReadUInt32();
			UInt32 Version = Reader.ReadUInt32();

			if (Signature != 0x55aa1234 && (Version > 2 || Version < 1))
			{
				Dispose();
				throw new ArchiveParsingException("Invalid archive header");
			}

			// skip unneeded bytes
			if (Version == 1 || Version == 2)
			{
				Reader.ReadUInt32(); // - TreeSize;
				if (Version == 2)
					Reader.ReadBytes(16);
			}

			AddMainPart(FileName, Stream);

			//TODO:
			//OPTIMIZE PARSING
			String Folder = Path.GetDirectoryName(FileName) ?? "";
			String NameWithoutExtension = Path.GetFileNameWithoutExtension(FileName) ?? "";
			//String Extension = Path.GetExtension(FileName);

			String BaseName = NameWithoutExtension.Substring(0, NameWithoutExtension.Length - 4);

			String[] MatchingFiles = Directory.GetFiles(Folder, BaseName + "_???.vpk");
			foreach (String MatchedFile in MatchingFiles)
			{
				var fileName = Path.GetFileNameWithoutExtension(MatchedFile);
				UInt16 Index;
				if (UInt16.TryParse(fileName.Substring(fileName.Length - 3), out Index))
				{
					AddPart(MatchedFile, new FileStream(MatchedFile, FileMode.Open, FileAccess.Read), Index);
				}
			}

			Reader.ReadDirectories(this);

			Loaded = true;
		}

		internal AudioClip LoadAudioClip(string soundPath)
		{
			if (!TryGetFileBytes(soundPath, out var data))
			{
				Debug.LogError($"Sound '{soundPath}' not found in VPK.");
				return null;
			}
			return CreateAudioClipFromData(soundPath, data);
		}

       private static  List<VPKFilePart> Parts_ = new();
       private byte[] ReadFileData(VPKEntry entry)
       {
	       if (!Parts.TryGetValue(entry.ArchiveIndex, out VPKFilePart part))
		       throw new KeyNotFoundException(
			       $"Part with index {entry.ArchiveIndex} not found (file {entry}).");

	       Stream partStream = part.PartStream;
	       partStream.Seek(entry.EntryOffset, SeekOrigin.Begin);

	       byte[] buffer = new byte[entry.Length];
	       int read = partStream.Read(buffer, 0, (int)entry.Length);
	       if (read != entry.Length)
		       throw new IOException(
			       $"Short read for entry '{entry}' – expected {entry.Length}, got {read} bytes.");

	       return buffer;
       }

        private static AudioClip CreateAudioClipFromData(string soundPath, byte[] fileData)
        {
            try
            {
                string tempPath = Path.Combine(Application.temporaryCachePath, Path.GetFileName(soundPath));
                File.WriteAllBytes(tempPath, fileData);

                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{tempPath}", AudioType.UNKNOWN))
                {
                    var asyncOperation = www.SendWebRequest();
                    while (!asyncOperation.isDone) { }

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to load audio clip from '{tempPath}': {www.error}");
                        return null;
                    }

                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    Debug.Log($"Audio clip loaded successfully: {soundPath}");
                    return clip;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating AudioClip for '{soundPath}': {ex.Message}");
                return null;
            }
        }

        private void AddMainPart(String filename, Stream stream = null)
		{
			if (stream == null)
			{
				stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
			}
			AddPart(filename, stream, MainPartIndex);
		}

		private void AddPart(String filename, Stream stream, Int32 index)
		{
			Parts.Add(index, new VPKFilePart(index, filename, stream));
		}

		#region IDisposable Support

		private void Dispose(Boolean disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					foreach (var partkv in Parts)
					{
						partkv.Value.PartStream?.Dispose();
					}
					Parts.Clear();
					Entries.Clear();
				}
				Reader.Dispose();
				Reader.Close();
				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				Disposed = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(Boolean disposing) above.
			Dispose(true);
			GC.Collect();
		}


        #endregion
    }
}