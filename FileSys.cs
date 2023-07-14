//
// C# (.Net Framework)
// IncominHttpServer
// https://github.com/dkxce/IncominHttpServer
// en,ru,1251,utf-8
//

using System;
using System.IO;

namespace IncominHttpServer
{
    /// <summary>
    ///     File System Tools
    /// </summary>
    public static class FileSys
    {
        /// <summary>
        ///     On Copy Operation Progress
        /// </summary>
        /// <param name="increment">Increment Value</param>
        /// <param name="copied">Total Copied Value</param>
        /// <param name="total">Total Size Value</param>
        /// <param name="status">Text Status</param>
        /// <param name="cancel">Cancel</param>
        public delegate void OnProgressChanged(long increment, long copied, long total, string status, ref bool cancel);

        /// <summary>
        ///     Calculate folder size
        /// </summary>
        /// <param name="folder">Path</param>
        /// <returns></returns>
        public static long CalculateFolderSize(string folder)
        {
            long folderSize = 0;
            try
            {
                //Checks if the path is valid or not
                if (!Directory.Exists(folder))
                    return folderSize;
                else
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(folder))
                            if (File.Exists(file))
                                folderSize += (new FileInfo(file)).Length;
                        foreach (string dir in Directory.GetDirectories(folder))
                            folderSize += CalculateFolderSize(dir);
                    }
                    catch { };
                }
            }
            catch // (UnauthorizedAccessException e)
            {

            };
            return folderSize;
        }

        /// <summary>
        ///     Copy File With Progress
        /// </summary>
        /// <param name="src">Source Path</param>
        /// <param name="dst">Dest Path</param>
        /// <param name="onProgressChanged">onProgress</param>
        public static bool CopyFile(string src, string dst, OnProgressChanged onProgressChanged)
        {
            byte[] buffer = new byte[1024 * 1024]; // 1MB
            bool cancel = false;
            string state = string.Format("Copy file: {0} ...", Path.GetFileName(src));

            try
            {
                using (FileStream source = new FileStream(src, FileMode.Open, FileAccess.Read))
                {
                    long fileLength = source.Length;
                    using (FileStream dest = new FileStream(dst, FileMode.Create, FileAccess.Write))
                    {
                        long totalBytes = 0;
                        int readed = 0;
                        while ((readed = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytes += readed;
                            double percentage = (double)totalBytes * 100.0 / fileLength;
                            dest.Write(buffer, 0, readed);
                            try
                            {
                                if (onProgressChanged != null)
                                    onProgressChanged((long)readed, totalBytes, fileLength, state, ref cancel);
                            }
                            catch { };
                            if (cancel) break;
                        };
                        if (totalBytes == fileLength) return true;
                    };
                };
            }
            catch { };
            return false;
        }

        /// <summary>
        ///     Copy Directory With Progress (not yet tested)
        /// </summary>
        /// <param name="src">Source Path</param>
        /// <param name="dst">Dest Path</param>
        /// <param name="insideDest">Create Folder in Destination Path</param>
        /// <param name="onProgressChanged">onProgress</param>
        public static bool CopyDirectory(string src, string dst, bool insideDest, OnProgressChanged onProgressChanged)
        {
            try
            {
                if (insideDest) dst = Path.Combine(dst, Path.GetDirectoryName(src));
                Directory.CreateDirectory(dst);
                string[] dirs = Directory.GetDirectories(src, "*", SearchOption.AllDirectories);
                foreach (string dir in dirs)
                {
                    string rp = dir.Remove(0, src.Length).Trim('\\');
                    string np = Path.Combine(dst, rp);
                    Directory.CreateDirectory(np);
                };
                long dstSize = 0;
                long srcSize = FileSys.CalculateFolderSize(src);
                string[] files = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories);
                int fNo = 0;
                foreach (string file in files)
                {
                    string rp = file.Remove(0, src.Length).Trim('\\');
                    string np = Path.Combine(dst, rp);
                    string state = string.Format("Copy file {1}/{2}: {0} ...", rp, ++fNo, files.Length);
                    FileSys.CopyFile(file, np, delegate (long increment, long copied, long total, string status, ref bool cancel)
                    {
                        dstSize += increment;
                        try
                        {
                            if (onProgressChanged != null)
                                onProgressChanged(increment, dstSize, srcSize, state, ref cancel);
                        }
                        catch { };
                    });
                };
                return true;
            }
            catch { };
            return false;
        }

        /// <summary>
        ///     Readable Size (b, kb, mb, gb, tb)
        /// </summary>
        /// <param name="size">Data Size</param>
        /// <returns></returns>
        public static string BytesToString(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            };
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.##} {1}", len, sizes[order]);
        }
    }
}
