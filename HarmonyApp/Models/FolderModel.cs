using HarmonyApp.FolderProcessing;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HarmonyApp.Models
{
    public class FolderModel : BindableBase
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
            PublicFolders = new ReadOnlyObservableCollection<SelectedFolder>(FoldersContainer.Paths);
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
            FilesCount = FoldersContainer.Paths.Sum(x => x.filesCount).ToString();
        }

        public void AddFolder(string folderPath) //добавление в коллекцию
        {
            FoldersContainer.Add(new SelectedFolder(folderPath));
        }

        public void DeleteSelectedFolder() //удаление из коллекции
        {
            if (SelectedFolderItem != null)
            {
                FoldersContainer.Paths.Remove(FoldersContainer.Paths.First(x => x.Path == SelectedFolderItem.Path));
            }
        }

        public void ClearCommand() //очистка коллекции
        {
            FoldersContainer.Paths.Clear();
        }

        public void CalculateSelectedFilesCount() //пересчитать количество файлов в текущей папке
        {
            if (SelectedFolderItem != null)
            {
                FilesCount = "-";
                Task.Factory.StartNew(() =>
                {
                    SelectedFolderItem.CalculateFiles();
                }).ContinueWith((t) =>
                {
                    SumFilesCount();
                });
            }
        }

        public void StartScan()
        {
            if (FoldersCount > 0 && _filesCount > 0)
            {
                Views.MainView mainView = new();
                Application.Current.MainWindow.Close();
                mainView.ShowDialog();
            }
        }
    }
}
