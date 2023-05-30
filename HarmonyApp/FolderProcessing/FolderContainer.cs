using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HarmonyApp.FolderProcessing
{
    public static class FoldersContainer
    {
        public static ObservableCollection<SelectedFolder> Paths = new();

        public static void Update()
        {
            var foldersToDelete = new List<SelectedFolder>();
            foreach (var folder in Paths)
            {
                if (folder.IsRecursive)
                {
                    foldersToDelete.AddRange(Paths.Where(x => x.DirectoryInfo.FullName != folder.DirectoryInfo.FullName && x.DirectoryInfo.FullName.StartsWith(folder.DirectoryInfo.FullName, StringComparison.InvariantCulture) && folder.IsRecursive));
                }
            }
            foldersToDelete.ForEach(x => Paths.Remove(x));
        }

        public static bool Add(SelectedFolder folder)
        {
            if (Paths.Any(x => x.DirectoryInfo.FullName == folder.DirectoryInfo.FullName) || Paths.Any(x => x.DirectoryInfo.FullName.StartsWith(folder.DirectoryInfo.FullName, StringComparison.InvariantCulture) && x.IsRecursive))
                return false;
            Paths.Add(folder);
            return true;
        }
    }
}
