using PMW_lib;
using PMWConsoleApp;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Query;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;

Stopwatch timer;
ParallelOptions options = new();

SynchronizedCollection<Audiofile> _matches = new();
ReaderWriterLockSlim ReaderWriterLock = new();

async Task ProcessFiles(string folderPath)
{
    await Parallel.ForEachAsync(PMW_lib.PMWCore.GetEnumerableFiles(folderPath), options, async (filePath, token) =>
    {

        AVHashes? hashes = await PMWFingerprinting.GetAVHashesAsync(filePath);
        PMWFingerprinting.StoreAVHashes(filePath, hashes);
        SoundFingerprinting.Query.AVQueryResult? queryResult = await PMWFingerprinting.CompareAVHashesAsync(hashes);

        if (queryResult != null && queryResult.ResultEntries.Any())
        {
            Audiofile parent = new(filePath);
            List<Audiofile> matches = new();
            //List<SoundFingerprinting.Query.ResultEntry> children = new();
            foreach (var (entry, _) in queryResult.ResultEntries)
            {
                if (entry != null && entry.TrackCoverageWithPermittedGapsLength >= 5d && entry.Track.Id != filePath && !matches.Any(x => x._path == entry.Track.Id))
                {
                    matches.Add(new(entry.Track.Id, parent, entry));
                    //children.Add(entry);
                }
            }

           if (matches.Any())
            {
                var AudiofilesToDelete = new List<Audiofile>();
                ReaderWriterLock.EnterReadLock();
                try
                {
                    var duplicateParent = _matches.Where(x => matches.Any(y => (y._path == x._path || y._path == filePath) && x.IsChild == false)).FirstOrDefault();
                    if (duplicateParent is not null)
                    {
                        AudiofilesToDelete.AddRange(_matches.Where(x => x.Parent == duplicateParent).ToList());
                        AudiofilesToDelete.Add(duplicateParent);
                    }
                }
                finally
                {
                    ReaderWriterLock.ExitReadLock();
                }
                ReaderWriterLock.EnterWriteLock();
                try
                {
                    foreach (var audiofile in AudiofilesToDelete)
                        _matches.Remove(audiofile);
                    _matches.Add(parent);
                    foreach (var entry in matches)
                        _matches.Add(entry);
                }
                finally
                {
                    ReaderWriterLock.ExitWriteLock();
                }
            }
        }
    });
}

timer = new Stopwatch();
timer.Start();

List<string> SelectedFolders = new() { @"Q:\Music\TestData — копия" };

List<Task> tasks = new();
foreach (var folder in SelectedFolders)
{
    tasks.Add(ProcessFiles(folder));
}
if (tasks.Count > 0)
{
    Task.Factory.ContinueWhenAll(tasks.ToArray(), para =>
    {
        foreach (var match in _matches)
            Console.WriteLine(match.DisplayPath);
    }).Wait();
}

//await ProcessFiles(PMWCore.GetEnumerableFiles(@"Q:\Music\TestData — копия", false));
timer.Stop();


Console.WriteLine(timer.Elapsed);