using PMW_lib;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace WpfApp
{
    internal class MainViewModel : BindableBase
    {
        readonly MainModel _model = new();

        public MainViewModel()
        {
            _model.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            CancelScan = new DelegateCommand(() => {
                _model.CancelScan();
            });
            OpenSelectedFile = new DelegateCommand(() => { 
                _model.OpenSelectedFile();
            });
            PlaySelectedFileInAssociatedApp = new DelegateCommand(() => {
                _model.PlaySelectedFileInAssociatedApp();
            });
            EditSelectedFile = new DelegateCommand(() => {
                _model.EditSelectedFile();
            });
            CreateReport = new DelegateCommand(() => {
                _model.CreateReport();
            });
            CompareCurrent = new DelegateCommand(() => {
                _model.CompareCurrent();
            });
            DeleteSelectedFile = new DelegateCommand(() => {
                _model.DeleteSelectedFile();
            });
            MoveSelectedFile = new DelegateCommand(() => {
                _model.MoveSelectedFile();
            });

            SelectAll = new DelegateCommand(() => {
                _model.SelectAll();
            });
            CancelSelection = new DelegateCommand(() => {
                _model.CancelSelection();
            });
            InverseSelection = new DelegateCommand(() => {
                _model.InverseSelection();
            });
            SelectBest = new DelegateCommand(() => {
                _model.SelectBest();
            });
            SelectWorst = new DelegateCommand(() => {
                _model.SelectWorst();
            });
            MoveSelected = new DelegateCommand(() => {
                _model.MoveSelected();
            });
            DeleteSelected = new DelegateCommand(() => {
                _model.DeleteSelected();
            });
            IgnoreSelected = new DelegateCommand(() => {
                _model.IgnoreSelected();
            });
        }
        public DelegateCommand CancelScan { get; }
        public DelegateCommand UpdateDuplicatesSize { get; }
        public DelegateCommand CompareCurrent { get; }
        public DelegateCommand CreateReport { get; }
        public DelegateCommand OpenSelectedFile { get; }
        public DelegateCommand DeleteSelectedFile { get; }
        public DelegateCommand MoveSelectedFile { get; }
        public DelegateCommand EditSelectedFile { get; }
        public DelegateCommand PlaySelectedFileInAssociatedApp { get; }

        public DelegateCommand SelectAll { get; }
        public DelegateCommand CancelSelection { get; }
        public DelegateCommand InverseSelection { get; }
        public DelegateCommand SelectBest { get; }
        public DelegateCommand SelectWorst { get; }
        public DelegateCommand MoveSelected { get; }
        public DelegateCommand DeleteSelected { get; }
        public DelegateCommand IgnoreSelected { get; }

        public int AllFilesCount { get { return Audiofile.FilesCount; } }
        public int ProcessedFilesCount => _model.ProcessedFilesCount;
        public bool IsCompleted => _model.isCompleted;
        public string ProgressText { get { return $"Обработка файлов ({ProcessedFilesCount}/{AllFilesCount})"; } }
        public string ProgressCaption { get { return IsCompleted ? "Сканирование завершено!" : $"Сканирование..."; } }
        public int DuplicatesCount { get { return _model.DuplicatesCount; } }
        public string DuplicatesSize { get { return Audiofile.GetDisplayFileSize(_model.DuplicatesSize); } }
        //public ReadOnlyObservableCollection<Audiofile> AudiofileMatches { get { return _model.PublicMatches; } }
        public ObservableCollection<Audiofile> AudiofileMatches { get { return _model._matches; } }
        public Visibility PlugVisibility { get { return AudiofileMatches.Count == 0 ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility GridVisibility { get { return AudiofileMatches.Count > 0 ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility SearchBarVisibility { get { return IsCompleted ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility ProgressBarVisibility { get { return !IsCompleted ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility CurrentAudiofilePlugVisibility { get { return _model.CurrentAudiofile == null ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility CurrentAudiofileGridVisibility { get { return _model.CurrentAudiofile != null ? Visibility.Visible : Visibility.Collapsed; } }

        public Audiofile? SelectedAudiofile { get { return _model.CurrentAudiofile; } set { _model.CurrentAudiofile = value; Services.MediaPlayerService.Stop();  RaisePropertyChanged(nameof(SelectedAudiofile));  RaisePropertyChanged(nameof(this.DuplicatesSize)); RaisePropertyChanged(nameof(MediaElementSource)); } }
        public Uri MediaElementSource { get { return _model.CurrentAudiofile == null ? new Uri("", UriKind.RelativeOrAbsolute) : new Uri(_model.CurrentAudiofile._path); } }      
    }
}
