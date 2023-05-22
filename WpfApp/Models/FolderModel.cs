using PMW_lib;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp
{
    public static class SelectedFolders
    {
        public static ObservableCollection<SelectedFolder> Paths = new();
    }
    public class SelectedFolder
    {
        public string Path { get; } //папка для сканирования
        public bool IsRecursive { get; set; } //искать аудиозаписи в подпапках
        public int filesCount; //количество файлов в папке

        public SelectedFolder(string path)
        {
            Path = path;
            IsRecursive = false;
            CalculateFiles();
        }

        public void CalculateFiles()
        {
            filesCount = Directory.EnumerateFiles(Path, "*.*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = IsRecursive
            }).Where(s => PMWCore.SupportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLowerInvariant())).Count();
        }
    }

    public class FolderModel: BindableBase
    {
        public SelectedFolder? SelectedFolderItem { get; set; }
      
        private int _filesCount;
        public string FilesCount //количество файлов
        {
            get
            {
                return _filesCount == -1 ? "-" : _filesCount.ToString();
            }
            set
            {
                if (value == "-")
                    _filesCount = -1;
                else
                    _filesCount = int.Parse(value);
                Audiofile.FilesCount = _filesCount;
                
                RaisePropertyChanged(nameof(FilesCount));
            }
        }

        private bool _isCollectionEmpty;
        public bool IsCollectionEmpty //пустая ли коллекция
        {
            get { return _isCollectionEmpty; }
            set
            {
                _isCollectionEmpty = value;
                RaisePropertyChanged("ClearButtonVisibility");
                RaisePropertyChanged("PlugVisibility");
            }
        } 

        public readonly ReadOnlyObservableCollection<SelectedFolder> PublicFolders; //список папок для сканирования
        public int FoldersCount => PublicFolders.Count; //количество папок

        public FolderModel()
        {
            PublicFolders = new ReadOnlyObservableCollection<SelectedFolder>(SelectedFolders.Paths);
            ((System.Collections.Specialized.INotifyCollectionChanged)PublicFolders).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnCollectionChanged);
            IsCollectionEmpty = true;
            FilesCount = "0";
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) //действия при изменении коллекции
        {
            IsCollectionEmpty = FoldersCount < 1;
            RaisePropertyChanged(nameof(FoldersCount));
            SumFilesCount();
        }

        private void SumFilesCount() //посчитать сумму количеств файлов
        {
            FilesCount = SelectedFolders.Paths.Sum(x => x.filesCount).ToString();
        }

        public void AddFolder(string folderPath) //добавление в коллекцию
        {
            if (!SelectedFolders.Paths.Any(f => f.Path == folderPath))
            {
                SelectedFolders.Paths.Add(new SelectedFolder(folderPath));
            }
        }
        
        public void DeleteSelectedFolder() //удаление из коллекции
        {
            if (SelectedFolderItem != null)
            {
                SelectedFolders.Paths.Remove(SelectedFolders.Paths.First(x => x.Path == SelectedFolderItem.Path));
            }
        }

        public void ClearCommand() //очистка коллекции
        {
            SelectedFolders.Paths.Clear();
        }

        public void CalculateSelectedFilesCount() //пересчитать количество файлов в текущей папке
        {
            if (SelectedFolderItem != null)
            { 
                FilesCount = "-";
                Task.Factory.StartNew(() => {
                    SelectedFolderItem.CalculateFiles(); 
                }).ContinueWith( (t) => {
                    SumFilesCount();
                }); 
            }
        }

        public void StartScan()
        {
            if (FoldersCount > 0 && _filesCount > 0)
            {


                MainView mainView = new();
                Application.Current.MainWindow.Close();
                mainView.ShowDialog();
            }
        }
    }
}
