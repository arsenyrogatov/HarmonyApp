using HarmonyApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HarmonyApp.ViewModels
{
    public class FolderViewModel : BindableBase
    {
        readonly FolderModel _model = new();
        public FolderViewModel()
        {
            _model.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            AddCommand = new DelegateCommand(() => {
                System.Windows.Forms.FolderBrowserDialog openFileDlg = new();
                var result = openFileDlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _model.AddFolder(openFileDlg.SelectedPath);
                }
            });
            StartCommand = new DelegateCommand(() => {
                _model.StartScan();
            });
            DeleteCommand = new DelegateCommand(() => {
                _model.DeleteSelectedFolder();
            });
            ClearCommand = new DelegateCommand(() => {
                _model.ClearCommand();
            });
            CalculateFilesCount = new DelegateCommand(() => {
                _model.CalculateSelectedFilesCount();
            });
        }
        public DelegateCommand AddCommand { get; } //Добавление папки 
        public DelegateCommand StartCommand { get; } //Начало сканирования
        public DelegateCommand DeleteCommand { get; } //Удаление папки
        public DelegateCommand ClearCommand { get; } //Очистка списка
        public DelegateCommand CalculateFilesCount { get; } //Поиск в подпапках для текущей папки

        public Visibility ClearButtonVisibility { get { return _model.IsCollectionEmpty ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility PlugVisibility { get { return _model.IsCollectionEmpty ? Visibility.Visible : Visibility.Collapsed; } }
        
        public ReadOnlyObservableCollection<SelectedFolder> Folders => _model.PublicFolders;
        public SelectedFolder? SelectedFolderItem { get { return _model.SelectedFolderItem; } set { _model.SelectedFolderItem = value; } }

        public int FoldersCount => _model.FoldersCount;
        public string FilesCount => _model.FilesCount;
    }
}
