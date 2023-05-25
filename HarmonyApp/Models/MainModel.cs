using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using HarmonyApp.ViewModels;
using HarmonyApp.AudioProcessing;

namespace HarmonyApp.Models
{
    public class MainModel : BindableBase, INotifyPropertyChanged
    {
        private readonly CancellationTokenSource cancelTokenSource = new();
        //private ObservableCollection<Audiofile> _matches = new();
        public ObservableCollection<Audiofile> _matches;
        private readonly object _matchesLock = new ();
        private int processedFilesCount;
        private int _duplicatesCount;
        private readonly ParallelOptions options;
        private Audiofile? _currentAudiofile;
        public bool isCompleted { get; private set; }

        public Audiofile? CurrentAudiofile
        {
            get
            {
                return _currentAudiofile;
            }
            set
            {
                _currentAudiofile = value;
                RaisePropertyChanged("CurrentAudiofilePlugVisibility");
                RaisePropertyChanged("CurrentAudiofileGridVisibility");
            }
        }
        //public ReadOnlyObservableCollection<Audiofile> PublicMatches { get; }
        public int DuplicatesCount { get { return _duplicatesCount; } set { _duplicatesCount = value; } }
        public long DuplicatesSize { get { return _matches.Sum(x => x.IsSelected ? x._sizeBytes : 0); } }

        public MainModel()
        {

            options = new() { CancellationToken = cancelTokenSource.Token };
            _matches = new();
            //PublicMatches = new (_matches);
            //_matches.Add(new Audiofile(@"C:\Users\ARSENY\Music\parentfile.mp3"));
            isCompleted = false;
            BindingOperations.EnableCollectionSynchronization(_matches, _matchesLock);
            StartScan();
        }
        //начало сканирования 
        public void StartScan()
        {
            List<Task> tasks = new();
            foreach (var folder in SelectedFolders.Paths)
            {
                tasks.Add(GetAVHashes(folder.Path, folder.IsRecursive));
            }
            if (tasks.Count > 0)
            {
                Task.Factory.ContinueWhenAll(tasks.ToArray(), para =>
                {
                    isCompleted = true;
                    RaisePropertyChanged(nameof(isCompleted));
                    RaisePropertyChanged("ProgressText");
                    RaisePropertyChanged("ProgressCaption");
                    RaisePropertyChanged("SearchBarVisibility");
                    RaisePropertyChanged("ProgressBarVisibility");
                    BindingOperations.DisableCollectionSynchronization(_matches);
                });
            }
        }
        public void PlaySelectedFileInAssociatedApp()
        {
            if (CurrentAudiofile == null || !File.Exists(CurrentAudiofile._path))
            {
                return;
            }

            System.Diagnostics.ProcessStartInfo processStartInfo = new()
            {
                FileName = CurrentAudiofile._path,
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(processStartInfo);
        }
        public void EditSelectedFile()
        {
            Views.TagEditView tagEditView = new();
            ((TagEditViewModel)tagEditView.DataContext).Initilize(CurrentAudiofile);
            tagEditView.Closed += (s, e) =>
            {
                var newPath = ((TagEditViewModel)tagEditView.DataContext)._model.newPath;
                if (newPath is null)
                {
                    newPath = CurrentAudiofile._path;
                }
                CurrentAudiofile.UpdateTags(newPath);
            };
            tagEditView.ShowDialog();
        }
        public void OpenSelectedFile()
        {
            if (CurrentAudiofile == null || !File.Exists(CurrentAudiofile._path))
            {
                return;
            }

            string argument = "/select, \"" + CurrentAudiofile._path + "\"";

            System.Diagnostics.Process.Start("explorer.exe", argument);
        }
        public void DeleteSelectedFile()
        {
            if (CurrentAudiofile == null || !File.Exists(CurrentAudiofile._path))
            {
                return;
            }

            if (System.Windows.MessageBox.Show($"Вы действительно хотите удалить файл {CurrentAudiofile.FileName}", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                File.Delete(CurrentAudiofile._path);
                CurrentAudiofile._path = "-";
                RaisePropertyChanged(nameof(CurrentAudiofile));
            }
        }
        public void MoveSelectedFile()
        {
            if (CurrentAudiofile == null || !File.Exists(CurrentAudiofile._path))
            {
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var newPath = Path.Combine(dialog.SelectedPath, CurrentAudiofile.FileName);
                File.Move(CurrentAudiofile._path, newPath);
                CurrentAudiofile._path = newPath;
            }
        }

        //получение хэшей
        private async Task GetAVHashes(string folderpath, bool isRecursive)
        {
                await Parallel.ForEachAsync(AudiofilesEnumerator.GetEnumerableFiles(folderpath), options, async (filePath, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    SoundFingerprinting.Data.AVHashes? hashes = null;
                    try
                    {
                        hashes = await Fingerprinting.GetAVHashesAsync(filePath);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.ToString());
                    }

                    token.ThrowIfCancellationRequested();
                    if (hashes is not null)
                    {
                        Fingerprinting.StoreAVHashes(filePath, hashes);

                        SoundFingerprinting.Query.AVQueryResult? queryResult = await Fingerprinting.CompareAVHashesAsync(hashes);

                        if (queryResult != null && queryResult.ResultEntries.Any())
                        {
                            Audiofile parent = new(filePath);
                            List<Audiofile> matches = new();
                            foreach (var (entry, _) in queryResult.ResultEntries)
                            {
                                if (entry != null && entry.TrackCoverageWithPermittedGapsLength >= 5d && entry.Track.Id != filePath && !matches.Any(x => x._path == entry.Track.Id))
                                {
                                    matches.Add(new(entry.Track.Id, parent, entry));
                                }
                            }

                            if (matches.Any())
                            {
                                var AudiofilesToDelete = new List<Audiofile>();

                                lock (_matchesLock)
                                {
                                    var duplicateParent = _matches.Where(x => matches.Any(y => (y._path == x._path || y._path == filePath) && x.IsChild == false)).FirstOrDefault();
                                    if (duplicateParent is not null)
                                    {
                                        AudiofilesToDelete.AddRange(_matches.Where(x => x.Parent == duplicateParent).ToList());
                                        AudiofilesToDelete.Add(duplicateParent);
                                    }

                                    foreach (var audiofile in AudiofilesToDelete)
                                        _matches.Remove(audiofile);

                                    var maxRating = matches.Max(x => x.RatingValue);
                                    maxRating = Math.Max(maxRating, parent.RatingValue);

                                    matches.ForEach(x => x.IsSelected = x.RatingValue != maxRating);
                                    parent.IsSelected = parent.RatingValue != maxRating;

                                    _matches.Add(parent);
                                    _matches.AddRange(matches);
                                    Parallel.Invoke(() =>
                                    {
                                        RaisePropertyChanged(nameof(_matches));
                                        RaisePropertyChanged(nameof(DuplicatesSize));
                                        RaisePropertyChanged("PlugVisibility");
                                        RaisePropertyChanged("GridVisibility");
                                    });

                                }
                            }
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    Interlocked.Increment(ref processedFilesCount);
                    RaisePropertyChanged(nameof(ProcessedFilesCount));
                    RaisePropertyChanged("ProgressText");
                    RaisePropertyChanged("ProgressCaption");
                });

        }


        public void CreateReport()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var fileName = "Гармония_сканирование_" + DateTime.Now.ToString("dd.MM.yy") + "_" + DateTime.Now.ToString("HH-mm") + ".txt";
                Services.ReportService.SaveReport(System.IO.Path.Combine(dialog.SelectedPath, fileName), _matches);
            }
        }

        public void CompareCurrent()
        {
            if (CurrentAudiofile is not null)
            {
                List<Audiofile> compareableAudiofiles = new();
                if (CurrentAudiofile.IsChild)
                {
                    compareableAudiofiles.Add(CurrentAudiofile.Parent);
                    compareableAudiofiles.AddRange(_matches.Where(x => x.Parent?._path == CurrentAudiofile.Parent._path));
                }
                else
                {
                    compareableAudiofiles.Add(CurrentAudiofile);
                    compareableAudiofiles.AddRange(_matches.Where(x => x.Parent?._path == CurrentAudiofile._path));
                }

                Views.CompareView compareView = new();
                ((CompareViewModel)compareView.DataContext).Initilize(new ObservableCollection<Audiofile>(compareableAudiofiles));
                compareView.ShowDialog();
            }
        }

        public void CancelScan()
        {
            cancelTokenSource.Cancel();
            isCompleted = true;
            RaisePropertyChanged(nameof(isCompleted));
            RaisePropertyChanged("ProgressText");
            RaisePropertyChanged("ProgressCaption");
        }
        public int ProcessedFilesCount => processedFilesCount; //количество обработанных файлов

        public void SelectAll()
        {
            foreach (Audiofile audiofile in _matches)
            {
                audiofile.IsSelected = true;
                RaisePropertyChanged(nameof(audiofile.IsSelected));
            }
            RaisePropertyChanged(nameof(_matches));
        }

        public void CancelSelection()
        {
            foreach (Audiofile audiofile in _matches)
            {
                audiofile.IsSelected = false;
                RaisePropertyChanged(nameof(audiofile.IsSelected));
            }
            RaisePropertyChanged(nameof(_matches));
        }

        public void InverseSelection()
        {
            foreach (Audiofile audiofile in _matches)
            {
                audiofile.IsSelected = !audiofile.IsSelected;
                RaisePropertyChanged(nameof(audiofile.IsSelected));
            }
            RaisePropertyChanged(nameof(_matches));
        }

        public void SelectBest()
        {
            List<string> processedFiles = new();
            foreach (Audiofile audiofile in _matches)
            {
                if (processedFiles.Contains(audiofile._path))
                    continue;

                Audiofile parent;
                List<Audiofile> children = new();

                if (audiofile.IsChild)
                    parent = audiofile.Parent;
                else
                    parent = audiofile;
                children.AddRange(_matches.Where(x => x.Parent._path == parent._path));
                children.Add(parent);

                var maxRating = children.Max(x => x.RatingValue);

                children.ForEach(x => { x.IsSelected = x.RatingValue == maxRating; RaisePropertyChanged(nameof(x.IsSelected)); });
                processedFiles.AddRange(children.Select(x => x._path));
            }
            RaisePropertyChanged(nameof(_matches));
        }

        public void SelectWorst()
        {
            List<string> processedFiles = new();
            foreach (Audiofile audiofile in _matches)
            {
                if (processedFiles.Contains(audiofile._path))
                    continue;

                Audiofile parent;
                List<Audiofile> children = new();

                if (audiofile.IsChild)
                    parent = audiofile.Parent;
                else
                    parent = audiofile;
                children.AddRange(_matches.Where(x => x.Parent._path == parent._path));
                children.Add(parent);

                var maxRating = children.Max(x => x.RatingValue);

                children.ForEach(x => { x.IsSelected = x.RatingValue != maxRating; RaisePropertyChanged(nameof(x.IsSelected)); });
                processedFiles.AddRange(children.Select(x => x._path));
            }
            RaisePropertyChanged(nameof(_matches));
        }

        public void MoveSelected()
        {
            FolderBrowserDialog dialog = new ();
            dialog.UseDescriptionForTitle = true;
            dialog.Description = $"Выберите путь для перемещения выделенных файлов ({_matches.Count(x => x.IsSelected)})";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (Audiofile audiofile in _matches.Where(x => x.IsSelected).ToList())
                {
                    if (audiofile == null || !File.Exists(audiofile._path))
                    {
                        _matches.Remove(audiofile);
                    }

                    var newPath = Path.Combine(dialog.SelectedPath, audiofile.FileName);
                    File.Move(audiofile._path, newPath);
                    if (!audiofile.IsChild)
                    {
                        foreach (var children in _matches.Where(x => x.Parent?._path == audiofile._path).ToList())
                        {
                            _matches.Remove(children);
                        }
                    }
                    _matches.Remove(audiofile);
                }
                RaisePropertyChanged(nameof(_matches));
                System.Windows.MessageBox.Show("Выделенные файлы перемещены!");
            }
        }

        public void DeleteSelected()
        {
            var dialogResult = System.Windows.MessageBox.Show($"Вы действительно хотите удалить выделенные аудиофайлы ({_matches.Count(x => x.IsSelected)})?", "Внимание!", MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Exclamation);

            if (dialogResult == MessageBoxResult.Yes)
            {
                foreach (Audiofile audiofile in _matches.Where(x => x.IsSelected).ToList())
                {
                    if (audiofile == null || !File.Exists(audiofile._path))
                    {
                        _matches.Remove(audiofile);
                    }

                    File.Delete(audiofile._path);
                    if (!audiofile.IsChild)
                    {
                        foreach (var children in _matches.Where(x => x.Parent?._path == audiofile._path).ToList())
                        {
                            _matches.Remove(children);
                        }
                    }
                    _matches.Remove(audiofile);
                }
                RaisePropertyChanged(nameof(_matches));
                System.Windows.MessageBox.Show("Выделенные файлы удалены!");
            }
        }

        public void IgnoreSelected()
        {
            foreach (Audiofile audiofile in _matches.Where(x => x.IsSelected).ToList())
            {
                _matches.Remove(audiofile);
            }
            RaisePropertyChanged(nameof(_matches));
        }
    }
}
