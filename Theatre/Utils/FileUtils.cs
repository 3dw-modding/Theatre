using System.Runtime.CompilerServices;

namespace Theatre.Utils
{
    public static class FileUtils
    {
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

        public static void ReloadDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }

            Directory.CreateDirectory(path);
        }

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
        /// Transforms any object into a byte array, this is the method your mom told you not to worry about.
        /// </summary>
        /// <typeparam name="T">The type to use.</typeparam>
        /// <param name="obj">The object to be converted.</param>
        /// <returns>
        /// A <see cref="byte"/>[] with it's <see cref="Array.Length"/> being sizeof(<typeparamref name="T"/>).
        /// </returns>
        public static byte[] GetBytes<T>(T obj)
        {
            byte[] result = new byte[Unsafe.SizeOf<T>()];
            Unsafe.As<byte, T>(ref result[0]) = obj;
            return result;
        }
        /// <summary>
        /// Transforms a byte array into any object, this is the method your mom told you not to worry about.
        /// </summary>
        /// <typeparam name="T">The type to use.</typeparam>
        /// <param name="data">The array to be converted.</param>
        /// <returns>
        /// If the span's length is not sizeof(<typeparamref name="T"/>), default. Otherwise the casted value.
        /// </returns>
        public static T? FromBytes<T>(Span<byte> data)
        {
            if (data.Length != Unsafe.SizeOf<T>())
                return default;
            return Unsafe.As<byte, T>(ref data[0]);
        }
    }
}
