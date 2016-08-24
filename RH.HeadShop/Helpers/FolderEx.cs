using System.IO;
using RH.HeadShop.IO;

namespace RH.HeadShop.Helpers
{
    public static class FolderEx
    {
        /// <summary>Create directory and all subdirectories on the path to this directory </summary>
        /// <param name="directory">Path to directory</param>
        public static void CreateDirectory(string directory)
        {
            var di = new DirectoryInfo(directory);
            CreateDirectory(di);
        }
        /// <summary>Create directory and all subdirectories on the path to this directory </summary>
        /// <param name="directory">Path to directory</param>
        public static void CreateDirectory(DirectoryInfo directory)
        {
            if (!directory.Parent.Exists)
                CreateDirectory(directory.Parent);
            if (!directory.Exists)
                directory.Create();
        }

        /// <summary> Copy dir with all files in them </summary>
        /// <param name="sourceDirName">Source directory path</param>
        /// <param name="destDirName">Destination directory path</param>
        /// <param name="copySubDirs">Copy subfolders</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                ProgramCore.EchoToLog("Source directory does not exist or could not be found: " + sourceDirName, EchoMessageType.Warning);
                return;
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            var files = dir.GetFiles();

            foreach (var file in files)
            {
                // Create the path to the new copy of the file.
                var temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (var subdir in dirs)
                {
                    // Create the subdirectory.
                    var temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static void DeleteSafety(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            { }
        }
    }
}
