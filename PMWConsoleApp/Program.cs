using PMW_lib;
using SoundFingerprinting.Data;
using SoundFingerprinting.Query;
using System.Diagnostics;

Stopwatch timer;

async Task ProcessFiles(IEnumerable<string> filePaths)
{
    var tasks = new List<Task>();

    foreach (var filePath in filePaths)
    {
        tasks.Add(Task.Run(async () =>
        {
            
            // Получаем акустический отпечаток асинхронно
            AVHashes? hashes = await PMWFingerprinting.GetAVHashesAsync(filePath);

            // Сравниваем отпечатки
            Console.WriteLine($"%{filePath} {timer.Elapsed}");
            SoundFingerprinting.Query.AVQueryResult? queryResult = await PMWFingerprinting.CompareAVHashesAsync(hashes);

            if (queryResult != null && queryResult.ResultEntries.Any())
            {
                // Обработка совпадений и вывод на экран
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
                    Console.WriteLine(filePath);
                    foreach (var child in children)
                        Console.WriteLine($"\t{child.Track.Id}");
                    Console.WriteLine("\n");
                }
            }

            // Сохраняем отпечаток в базу данных
            PMWFingerprinting.StoreAVHashes(filePath, hashes);
        }));
    }

    // Ожидаем завершения всех операций
    await Task.WhenAll(tasks);
}

timer = new Stopwatch();
timer.Start();
await ProcessFiles(PMWCore.GetEnumerableFiles(@"Q:\Music\TestData — копия", false));