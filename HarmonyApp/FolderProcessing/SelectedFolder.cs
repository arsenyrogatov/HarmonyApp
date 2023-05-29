using HarmonyApp.AudioProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyApp.FolderProcessing
{
    public class SelectedFolder
    {
        public string Path { get { return DirectoryInfo.FullName; } } //папка для сканирования
        public bool IsRecursive { get { return _isRecursive; } set { _isRecursive = value; FolderProcessing.FoldersContainer.Update(); } } //искать аудиозаписи в подпапках
        private bool  _isRecursive;
        public int filesCount; //количество файлов в папке

        public DirectoryInfo DirectoryInfo { get; }

        public SelectedFolder(string path)
        {
            IsRecursive = false;
            DirectoryInfo = new(path);
            CalculateFiles();
        }

        public void CalculateFiles()
        {
            filesCount = Directory.EnumerateFiles(DirectoryInfo.FullName, "*.*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = IsRecursive
            }).Where(s => AudiofilesEnumerator.SupportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLowerInvariant())).Count();
        }
    }
}
