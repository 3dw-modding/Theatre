using System.IO.Compression;
using SharpCompress;
using System.Text;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using Newtonsoft.Json;
using Theatre.Handlers;

namespace Theatre.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Ensures the creation of a directory
        /// </summary>
        /// <param name="path">The path to the directory</param>
        public static void CreateDirectorySafe(string path)
        {
            if (Directory.Exists(path))
                return;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Reloads the content of a directory
        /// </summary>
        /// <param name="path">The path to the directory</param>
        public static void ReloadDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }

            CreateDirectorySafe(path);
        }

        /// <summary>
        /// Copies a directory from one place to another
        /// </summary>
        /// <param name="sourceDir">The directory to copy from</param>
        /// <param name="destinationDir">The directory to copy to</param>
        /// <param name="recursive">Defaults to true. Should it copy all sub directories and files as well?</param>
        /// <exception cref="DirectoryNotFoundException">If the source directory doesn't exist</exception>
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                if (!File.Exists(targetFilePath))
                    file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir);
                }
            }
        }
        
        /// <summary>
        /// Converts a Dictionary of mod entries into parable bytes.
        /// </summary>
        /// <param name="dict">The dictionary to convert.</param>
        /// <returns><see cref="byte"/>[]</returns>
        public static byte[] ToBytes(this Dictionary<ulong, GbModInfo> dict)
        {
            using MemoryStream stream = new();
            using BinaryWriter writer = new(stream, Encoding.UTF8);
            writer.Write(dict.Count);
            dict.Keys.ForEach(x => writer.Write(x));
            foreach (var (name, owner, files) in dict.Values)
            {
                writer.Write(name);
                writer.Write(owner);
                writer.Write(files.Length);
                foreach (var file in files)
                {
                    writer.Write(file.idRow);
                    writer.Write(file.sFile);
                    writer.Write(file.nFilesize);
                    writer.Write(file.sDescription);
                    writer.Write(file.tsDateAdded);
                    writer.Write(file.nDownloadCount);
                    writer.Write(file.sAnalysisState);
                    writer.Write(file.sDownloadUrl);
                    writer.Write(file.sMd5Checksum);
                    writer.Write(file.sClamAvResult);
                    writer.Write(file.sAnalysisResult);
                    writer.Write(file.bContainsExe);
                }
            }
            return stream.ToArray();
        }

        /// <summary>
        /// Converts a (hopefully) parsable array of bytes into a Dictonary of mod info.
        /// </summary>
        /// <param name="data">The byte array to parse.</param>
        /// <returns>A Dictionary of mod entries.</returns>
        /// <exception cref="EndOfStreamException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException"></exception>
        public static Dictionary<ulong, GbModInfo> FromBytes(byte[] data)
        {
            using MemoryStream stream = new(data);
            using BinaryReader reader = new(stream, Encoding.UTF8);
            int count = reader.ReadInt32();
            Dictionary<ulong, GbModInfo> result = new(count);
            for (int i = 0; i < count; i++)
                result[reader.ReadUInt64()] = new();
            foreach (var key in result.Keys)
            {
                string name = reader.ReadString();
                string owner = reader.ReadString();
                AFile[] files = new AFile[reader.ReadInt32()];
                for (int i = 0; i < files.Length; i++)
                    files[i] = new();
                for (int i = 0; i < files.Length; i++)
                {
                    ref var file = ref files[i];
                    file.idRow = reader.ReadUInt64();
                    file.sFile = reader.ReadString();
                    file.nFilesize = reader.ReadUInt64();
                    file.sDescription = reader.ReadString();
                    file.tsDateAdded = reader.ReadUInt64();
                    file.nDownloadCount = reader.ReadUInt64();
                    file.sAnalysisState = reader.ReadString();
                    file.sDownloadUrl = reader.ReadString();
                    file.sMd5Checksum = reader.ReadString();
                    file.sClamAvResult = reader.ReadString();
                    file.sAnalysisResult = reader.ReadString();
                    file.bContainsExe = reader.ReadBoolean();
                }
                result[key] = new(name, owner, files);
            }
            return result;
        }

        /// <summary>
        /// Calls <see cref="FromBytes(byte[])"/> and sets the <paramref name="dict"/> to the result.
        /// </summary>
        /// <param name="dict">The dict to modify</param>
        /// <param name="data">The data to parse</param>
        /// <exception cref="EndOfStreamException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException"></exception>
        public static void FromBytes(ref Dictionary<ulong, GbModInfo> dict, byte[] data)
        {
            dict = FromBytes(data);
        }

        // TODO: Lord, write documentation for this. -Scyye
        public static void UpdateEntries(this Dictionary<ulong, GbModInfo> dict, GBGame game)
        {
            var submissons = GBUtils.GetSubmissions(game);
            submissons.Where(x => !dict.ContainsKey(x)).
                Select(x => (x, GBUtils.GetSubmissionData(x)))
                .ForEach(tup => dict[tup.x] = tup.Item2);
        }

        /// <summary>
        /// Attempts to download a Switch mod from Gamebanana
        /// </summary>
        /// <param name="info">The info to use.</param>
        /// <returns>The Directories that have romfs or exefs as their name</returns>
        /// <exception cref="HttpRequestException"></exception>
        public static List<DirectoryInfo> DownloadSwitchMod(this GbModInfo info)
        {
            using HttpClient client = new();
            Stream dl(string url) => client.GetStreamAsync(url).GetAwaiter().GetResult();
            DirectoryInfo exedir = new FileInfo(Environment.ProcessPath!).Directory!;
            DirectoryInfo moddir = new(Path.Join(exedir.FullName, "Mods"));
            moddir.Create();
            List<DirectoryInfo> infos = new();
            List<DirectoryInfo> result = new();
            foreach (var file in info.Files)
            {
                FileInfo finfo = new(file.sFile);
                DirectoryInfo dinfo = new(Path.Join(moddir.FullName, 
                    Path.GetFileNameWithoutExtension(finfo.Name)));
                dinfo.Create();
                using var stream = dl(file.sDownloadUrl);
                
                if (finfo.Extension == ".zip")
                {
                    using var reader = new ZipArchive(stream, ZipArchiveMode.Read);
                    reader.ExtractToDirectory(Path.Join(Path.GetTempPath(), "Theatre", dinfo.Name), true);
                } 
                else if (finfo.Extension == ".rar")
                {
                    using var reader = RarArchive.Open(stream);
                    reader.WriteToDirectory(Path.Join(Path.GetTempPath(), "Theatre", dinfo.Name), new()
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                        PreserveAttributes = true,
                        PreserveFileTime = true
                    });
                } 
                else if (finfo.Extension == ".7z")
                {
                    using var reader = SevenZipArchive.Open(stream);
                    reader.WriteToDirectory(Path.Join(Path.GetTempPath(), "Theatre", dinfo.Name), new()
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                        PreserveAttributes = true,
                        PreserveFileTime = true
                    });
                }
                stream.Close();
                
                File.WriteAllText(Path.Join(dinfo.FullName, "FileInfo.json"), JsonConvert.SerializeObject(
                    new FInfo() {
                        Name = info.Name,
                        Owner = info.Owner,
                        Description = file.sDescription
                    }));

                infos.Add(new(Path.Join(Path.GetTempPath(), "Theatre", dinfo.Name)));
                
            }
            foreach (var dinfo in infos)
            {
                var dirs = dinfo.GetDirectories("*", new EnumerationOptions { RecurseSubdirectories = true });
                foreach (var dir in dirs)
                {
                    if (dir.Name == "romfs" || dir.Name == "exefs")
                    {
                        var copypth = Path.Join(moddir.FullName, dinfo.Name, dir.Name);
                        dir.MoveTo(copypth);
                        result.Add(new(copypth));
                    }
                }
                dinfo.Delete(true);
            }
            return result;
        }

        public static List<DirectoryInfo> DownloadWiiUMod(this GbModInfo info)
        {
            using HttpClient client = new();
            Stream dl(string url) => client.GetStreamAsync(url).GetAwaiter().GetResult();
            DirectoryInfo exedir = new FileInfo(Environment.ProcessPath!).Directory!;
            DirectoryInfo moddir = new(Path.Join(exedir.FullName, "Mods"));
            moddir.Create();
            List<DirectoryInfo> infos = new();
            List<DirectoryInfo> result = new();
            foreach (var file in info.Files)
            {
                FileInfo finfo = new(file.sFile);
                DirectoryInfo dinfo = new(Path.Join(moddir.FullName,
                    Path.GetFileNameWithoutExtension(finfo.Name)));
                dinfo.Create();
                using var stream = dl(file.sDownloadUrl);

                if (finfo.Extension == ".zip")
                {
                    using var reader = new ZipArchive(stream, ZipArchiveMode.Read);
                    reader.ExtractToDirectory(Path.Join(Path.GetTempPath(), "Theatre", dinfo.Name), true);
                }
                else if (finfo.Extension == ".rar")
                {
                    using var reader = RarArchive.Open(stream);
                    reader.WriteToDirectory(Path.Join(Path.GetTempPath(), "Theatre", dinfo.Name), new()
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                        PreserveAttributes = true,
                        PreserveFileTime = true
                    });
                }
                else if (finfo.Extension == ".7z")
                {
                    using var reader = SevenZipArchive.Open(stream);
                    reader.WriteToDirectory(Path.Join(Path.GetTempPath(), "Theatre", dinfo.Name), new()
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                        PreserveAttributes = true,
                        PreserveFileTime = true
                    });
                }
                stream.Close();

                File.WriteAllText(Path.Join(dinfo.FullName, "FileInfo.json"), JsonConvert.SerializeObject(
                    new FInfo()
                    {
                        Name = info.Name,
                        Owner = info.Owner,
                        Description = file.sDescription
                    }));

                infos.Add(new(Path.Join(Path.GetTempPath(), "Theatre", dinfo.Name)));
            }
            foreach (var dinfo in infos)
            {
                var dirs = dinfo.GetDirectories("*", new EnumerationOptions { RecurseSubdirectories = true });
                foreach (var dir in dirs)
                {
                    if (dir.Name == "content")
                    {
                        var copypth = Path.Join(moddir.FullName, dinfo.Name, dir.Name);
                        dir.MoveTo(copypth);
                        result.Add(new(copypth));
                        break;
                    }
                }
                dinfo.Delete(true);
            }
            return result;
        }

        /// <summary>
        /// Attempts to download a Switch mod from Gamebanana
        /// </summary>
        /// <param name="info">The info to use.</param>
        /// <returns>The Directories that have romfs or exefs as their name</returns>
        /// <exception cref="HttpRequestException"></exception>
        public static List<DirectoryInfo> DownloadSwitchMod(string path, FInfo info)
        {
            DirectoryInfo exedir = new FileInfo(Environment.ProcessPath!).Directory!;
            DirectoryInfo moddir = new(Path.Join(exedir.FullName, "Mods"));
            moddir.Create();
            List<DirectoryInfo> infos = new();
            List<DirectoryInfo> result = new();

            FileInfo finfo = new(path);
            DirectoryInfo dinfo = new(Path.Join(moddir.FullName,
                Path.GetFileNameWithoutExtension(finfo.Name)));
            dinfo.Create();

            var stream = finfo.Open(FileMode.OpenOrCreate);

            if (finfo.Extension == ".zip")
            {
                using var reader = new ZipArchive(stream, ZipArchiveMode.Read);
                reader.ExtractToDirectory(dinfo.FullName, true);
            }
            else if (finfo.Extension == ".rar")
            {
                using var reader = RarArchive.Open(stream);
                reader.WriteToDirectory(dinfo.FullName, new()
                {
                    ExtractFullPath = true,
                    Overwrite = true,
                    PreserveAttributes = true,
                    PreserveFileTime = true
                });
            }
            else if (finfo.Extension == ".7zip")
            {
                using var reader = SevenZipArchive.Open(stream);
                reader.WriteToDirectory(dinfo.FullName, new()
                {
                    ExtractFullPath = true,
                    Overwrite = true,
                    PreserveAttributes = true,
                    PreserveFileTime = true
                });
            }
            var str = File.Create(Path.Join(dinfo.FullName, "FileInfo.json"));
            str.Close();
            File.WriteAllText(Path.Join(dinfo.FullName, "FileInfo.json"), JsonConvert.SerializeObject(
                info));
            
            result.AddRange(dinfo.EnumerateDirectories()
                .Where(x => x.Name == "romfs" || x.Name == "exefs"));
            stream.Close();
            return result;
        }
        
        }
    }

