using HarmonyApp.AudioProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HarmonyApp.Models
{
    public static class ConsoleModel
    {
        static string scanPath = "";
        static string outputPath = "";
        static string? movePath;
        static bool isRemove;
        static bool isRecursive;
        static bool isConfirmationNotRequired;

        private static void SetDefaultScanSettings()
        {
            isRecursive = false;
            scanPath = Directory.GetCurrentDirectory();
            outputPath = scanPath;
            movePath = null;
            isRemove = false;
            isConfirmationNotRequired = false;
        }

        public static async void ProcessArgs(string[] args)
        {
            SetDefaultScanSettings();
            Console.WriteLine();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                switch (arg)
                {
                    case "-c":
                    case "--console":
                        {
                            SetDefaultScanSettings();
                        }
                        break;
                    case "-d":
                    case "--dir":
                        {
                            var scanPathIndex = i + 1;
                            if (args.Length <= scanPathIndex || args[scanPathIndex].StartsWith("-"))
                                throw new ArgumentException("После аргумента --dir не указан путь к папке для сканирования");
                            else
                            if (!Directory.Exists(args[scanPathIndex]))
                                throw new ArgumentException($"Выбранная директория ({args[scanPathIndex]}) не существует!");
                            else
                            {
                                scanPath = args[scanPathIndex];
                                i++;
                                if (outputPath == null)
                                {
                                    outputPath = scanPath;
                                }
                            }
                        }
                        break;
                    case "-r":
                    case "--recursive":
                        isRecursive = true;
                        break;
                    case "-o":
                    case "--output":
                        {
                            var outputPathIndex = i + 1;
                            if (args.Length <= outputPathIndex || args[outputPathIndex].StartsWith("-"))
                                throw new ArgumentException("После аргумента --output не указан путь к папке для сохранения отчета");
                            else
                            if (!Directory.Exists(args[outputPathIndex]))
                                throw new ArgumentException($"Выбранная директория ({args[outputPathIndex]}) не существует!");
                            else
                            {
                                outputPath = args[outputPathIndex];
                                i++;
                            }
                        }
                        break;
                    case "-m":
                    case "--move":
                        {
                            var movePathIndex = i + 1;
                            if (args.Length <= movePathIndex || args[movePathIndex].StartsWith("-"))
                                throw new ArgumentException("После аргумента --move не указан путь к папке для перемещения дубликатов");
                            else
                            if (!Directory.Exists(args[movePathIndex]))
                                throw new ArgumentException($"Выбранная директория ({args[movePathIndex]}) не существует!");
                            else
                            {
                                movePath = args[movePathIndex];
                                i++;
                                isRemove = false;
                            }
                        }
                        break;
                    case "-rm":
                    case "--remove":
                        isRemove = true;
                        movePath = null;
                        break;
                    case "-f":
                    case "--force":
                        isConfirmationNotRequired = true;
                        break;
                    case "-h":
                    case "--help":
                        Console.WriteLine("Параметры:");
                        Console.WriteLine("\t'-c'  или '--console'\tзапуск со стандартными параметрами");
                        Console.WriteLine("\t'-d'  или '--dir'\tпуть к директории, в которой необходимо произвести поиск дубликатов");
                        Console.WriteLine("\t'-r'  или '--recursive'\tвключение поиска дубликатов во всех поддиректориях указанной папки");
                        Console.WriteLine("\t'-o'  или '--output'\tвыбор директории для сохранения отчета");
                        Console.WriteLine("\t'-m'  или '--move'\tперемещение найденных дубликатов с меньшим рейтингом в указанную директорию");
                        Console.WriteLine("\t'-rm' или '--remove'\tудаление найденных дубликатов с меньшим рейтингом");
                        Console.WriteLine("\t'-f'  или '--force'\tотключение подтверждения перед перемещением или удалением");
                        Console.WriteLine("\t'-h'  или '--help'\tотображение справки по использованию приложения");
                        break;
                    default:
                        Console.WriteLine($"Неизвестный параметр [{arg}]");
                        Console.WriteLine($"Используйте параметр -h для отображения спавки по исользованию приложения");
                        break;
                }
            }

            await StartScanAsync();
        }

        private static void MoveFile(string startPath, string endPath)
        {
            var filename = System.IO.Path.GetFileName(startPath);
            var fullendpath = Path.Combine(endPath, filename);
            File.Move(startPath, fullendpath);
        }

        private static void DeleteFile(string path)
        {
            File.Delete(path);
        }

        private static async Task StartScanAsync()
        {
            Console.WriteLine($"Поиск дубликатов в {scanPath}");
            var filesCount = Directory.EnumerateFiles(scanPath, "*.*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = isRecursive
            }).Where(s => AudiofilesEnumerator.SupportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLowerInvariant())).Count();
            Console.WriteLine($"Обнаружено {filesCount} музыкальных файлов\n");

            List<Audiofile> audioFiles = new List<Audiofile>();
            List<string> badFiles = new List<string>();
            foreach (var file in AudiofilesEnumerator.GetEnumerableFiles(scanPath, isRecursive))
            {
                SoundFingerprinting.Data.AVHashes? hashes;
                try
                {
                    Console.WriteLine(file);
                    hashes = await Fingerprinting.GetAVHashesAsync(file);
                    Console.WriteLine(file);
                }
                catch
                {
                    continue;
                }

                SoundFingerprinting.Query.AVQueryResult? queryResult;
                try
                {
                    queryResult = await Fingerprinting.CompareAVHashesAsync(hashes);
                }
                catch
                {
                    continue;
                }

                if (queryResult != null && queryResult.ResultEntries.Any())
                {
                    List<Audiofile> duplicates = new();
                    Audiofile parent = new(file);
                    foreach (var (entry, _) in queryResult.ResultEntries.DistinctBy(x => x.TrackId))
                    {
                        if (entry != null && entry.TrackCoverageWithPermittedGapsLength >= 5d)
                        {
                            duplicates.Add(new Audiofile(entry.Track.Id, parent, entry));
                        }
                    }
                    if (duplicates.Count > 0)
                    {
                        Console.WriteLine($"\n{parent._path}");
                        var maxRating = duplicates.Max(x => x.RatingValue);
                        maxRating = Math.Max(maxRating, parent.RatingValue);

                        foreach (var audiofile in duplicates)
                        {
                            Console.WriteLine($"{audiofile._path}");
                            if (audiofile.RatingValue != maxRating)
                                badFiles.Add(audiofile._path);
                        }
                        if (parent.RatingValue != maxRating)
                            badFiles.Add(parent._path);

                        audioFiles.Add(parent);
                        audioFiles.AddRange(duplicates);
                    }
                }

                try
                {
                    Fingerprinting.StoreAVHashes(file, hashes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }

            Console.WriteLine($"\nСканирование завершено! Найдено {audioFiles.Count} дубликатов");

            if (isRemove)
            {
                string? response = "";
                if (!isConfirmationNotRequired)
                    Console.WriteLine($"Файлы ({badFiles.Count}) будут удалены. Продолжить? (Д\\Н)");
                response = isConfirmationNotRequired ? "д" : Console.ReadLine()?.ToLower();
                if (response == "д")
                {
                    foreach (var audiofile in badFiles)
                    {
                        DeleteFile(audiofile);
                    }
                    Console.WriteLine("Файлы удалены!");
                }
            }
            if (movePath is not null)
            {
                string? response = "";
                if (!isConfirmationNotRequired)
                    Console.WriteLine($"Файлы ({badFiles.Count}) будут перемещены в '{movePath}'. Продолжить? (Д\\Н)");
                response = isConfirmationNotRequired ? "д" : Console.ReadLine()?.ToLower();
                if (response == "д")
                {
                    foreach (var audiofile in badFiles)
                    {
                        MoveFile(audiofile, movePath);
                    }
                    Console.WriteLine("Файлы перемещены!");
                }
            }

            outputPath += outputPath.Last() == '\\' ? "" : "\\";
            outputPath += $"Гармония_сканирование_{DateTime.Now:dd.MM.yy}_{DateTime.Now:HH-mm}.txt";
            Console.WriteLine($"Отчет сохранен в файл: {outputPath}");
        }

    }
}
