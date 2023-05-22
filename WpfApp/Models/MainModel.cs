using PMW_lib;
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
using System.Windows.Forms;

namespace WpfApp
{
    public class MainModel : BindableBase, INotifyPropertyChanged
    {
        private readonly CancellationTokenSource cancelTokenSource = new();
        //private ObservableCollection<Audiofile> _matches = new();
        public ObservableCollection<Audiofile> _matches = new();
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
            //PublicMatches = new (_matches);
            //_matches.Add(new Audiofile(@"C:\Users\ARSENY\Music\parentfile.mp3"));
            isCompleted = false;
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
            TagEditWindow tagEditWindow = new();
            ((TagEditViewModel)tagEditWindow.DataContext).Initilize(CurrentAudiofile);
            tagEditWindow.Closed += (s, e) =>
            {
                var newPath = ((TagEditViewModel)tagEditWindow.DataContext)._model.newPath;
                if (newPath is null)
                {
                    newPath = CurrentAudiofile._path;
                }
                CurrentAudiofile.UpdateTags(newPath);
            };
            tagEditWindow.ShowDialog();
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

        //добавление дубликатов в коллекцию
        private void AddDuplicates(string parentPath, List<SoundFingerprinting.Query.ResultEntry> children)
        {
            App.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                if (_matches.Any(x => (x._path == parentPath || children.Any(y => y.Track.Id == x._path))))
                {

                }
                if (_matches.Any(x => (x.IsChild && (x.Parent._path == parentPath || children.Any(y => y.Track.Id == x.Parent._path)))))
                {

                }

                Audiofile parent = new(parentPath);
                List<Audiofile> matches = new() { parent };

                foreach (var child in children.DistinctBy(x => x.Track.Id))
                {
                    matches.Add(new Audiofile(child.Track.Id, parent, child));
                }

                var maxRating = matches.Max(x => x.RatingValue);
                maxRating = Math.Max(maxRating, parent.RatingValue);

                matches.ForEach(x => x.IsSelected = x.RatingValue != maxRating);
                parent.IsSelected = parent.RatingValue != maxRating;

                DuplicatesCount += matches.Count;
                _matches.AddRange(matches);
            });

            RaisePropertyChanged(nameof(DuplicatesCount));
            RaisePropertyChanged(nameof(DuplicatesSize));
            RaisePropertyChanged("PlugVisibility");
            RaisePropertyChanged("GridVisibility");
        }
        //получение хэшей
        private async Task GetAVHashes(string folderpath, bool isRecursive)
        {
            try
            {
                await Parallel.ForEachAsync(PMWCore.GetEnumerableFiles(folderpath, isRecursive), options, async (path, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    SoundFingerprinting.Data.AVHashes? hashes;
                    try
                    {
                        hashes = await PMWFingerprinting.GetAVHashesAsync(path);
                    }
                    catch
                    {
                        return;
                    }
                    GC.Collect();

                    token.ThrowIfCancellationRequested();

                    SoundFingerprinting.Query.AVQueryResult? queryResult;
                    try
                    {
                        queryResult = await PMWFingerprinting.CompareAVHashesAsync(hashes);
                    }
                    catch
                    {
                        return;
                    }

                    if (queryResult != null && queryResult.ResultEntries.Any())
                    {
                        List<SoundFingerprinting.Query.ResultEntry> children = new();
                        foreach (var (entry, _) in queryResult.ResultEntries)
                        {
                            // output only those tracks that matched at least seconds.
                            if (entry != null && entry.TrackCoverageWithPermittedGapsLength >= 5d)
                            {
                                children.Add(entry);
                            }
                        }
                        if (children.Count > 0)
                        {
                            Parallel.Invoke(() => AddDuplicates(path, children));
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    Interlocked.Increment(ref processedFilesCount);
                    RaisePropertyChanged(nameof(ProcessedFilesCount));
                    RaisePropertyChanged("ProgressText");
                    RaisePropertyChanged("ProgressCaption");

                    try
                    {
                        PMWFingerprinting.StoreAVHashes(path, hashes);
                    }
                    catch
                    {
                        return;
                    }

                    GC.Collect();


                });
            }
            catch (OperationCanceledException ex)
            {
                return;
            }
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

                CompareWindow compareWindow = new();
                ((CompareViewModel)compareWindow.DataContext).Initilize(new ObservableCollection<Audiofile>(compareableAudiofiles));
                compareWindow.ShowDialog();
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
