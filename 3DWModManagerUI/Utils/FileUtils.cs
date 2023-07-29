using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DWModManagerUI.Utils
{
    public class FileUtils
    {
        public static byte[] ExtractResource(String filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                string[] names = a.GetManifestResourceNames();
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        public static string FindFileInDirectory(string directoryPath, string requestedFile)
        {
            if (Path.GetFileNameWithoutExtension(directoryPath).Equals(requestedFile))
            {
                return directoryPath;
            }

            foreach (var file in Directory.EnumerateFiles(directoryPath))
            {
                if (file.Equals(requestedFile))
                {
                    return file;
                }
            }

            foreach (var directory in Directory.EnumerateDirectories(directoryPath))
            {
                if (directory.Equals(requestedFile))
                {
                    return directory;
                }
            }

            foreach (var dir in Directory.EnumerateDirectories(directoryPath))
            {
                return FindFileInDirectory(dir, requestedFile);
            }

            return null;
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
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
