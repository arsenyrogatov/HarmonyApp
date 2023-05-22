using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMW_lib
{
    public static class PMWCore
    {
        public static string[] SupportedExtensions = new string[] { ".wav", ".mp3" };

        public static IEnumerable<string> GetEnumerableFiles (string startPath, bool recursive = false)
        {
            return Directory.EnumerateFiles(startPath, "*.*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = recursive
            }).Where(s => SupportedExtensions.Contains(Path.GetExtension(s).ToLowerInvariant()));
        }
    }
}
