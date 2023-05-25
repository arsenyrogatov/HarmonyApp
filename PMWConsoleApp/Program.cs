using PMW_lib;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Query;
using System.Diagnostics;

Stopwatch timer;

List<string> duplicates = new ();
ReaderWriterLockSlim ReaderWriterLock = new();

async Task ProcessFiles(IEnumerable<string> filePaths)
{
    Console.WriteLine(filePaths.Count());
    var tasks = new List<Task>();

    foreach (var filePath in filePaths)
    {
        tasks.Add(Task.Run(async () =>
        {
            AVHashes? hashes = await PMWFingerprinting.GetAVHashesAsync(filePath);
            PMWFingerprinting.StoreAVHashes(filePath, hashes);
            SoundFingerprinting.Query.AVQueryResult? queryResult = await PMWFingerprinting.CompareAVHashesAsync(hashes);

            if (queryResult != null && queryResult.ResultEntries.Any())
            {
                List<SoundFingerprinting.Query.ResultEntry> children = new();
                foreach (var (entry, _) in queryResult.ResultEntries)
                {
                    if (entry != null && entry.TrackCoverageWithPermittedGapsLength >= 5d)
                    {
                            children.Add(entry);
                    }
                }

                var childrenStr = children.Select(x => x.Track.Id);
                if (children.Count > 0)
                {
                    ReaderWriterLock.EnterWriteLock();
                    try
                    {
                        duplicates.RemoveAll(x => children.Any(y => y.Track.Id == x));
                        duplicates.AddRange(childrenStr.OrderBy(x => x));
                        duplicates.Add(" ");
                    }
                    finally
                    {
                        ReaderWriterLock.ExitWriteLock();
                    }
                }
            }
        }));
    }

    // Ожидаем завершения всех операций
    await Task.WhenAll(tasks);
}

timer = new Stopwatch();
timer.Start();
await ProcessFiles(PMWCore.GetEnumerableFiles(@"D:\Music\папка", false));
timer.Stop();
duplicates.ForEach(x => Console.WriteLine(x));

var tmp = duplicates.Select(x => x != " ");
Console.WriteLine($"tmpcount {tmp.Count()}");
Console.WriteLine($"tmpcountdist {tmp.Distinct().Count()}");
Console.WriteLine(timer.Elapsed);