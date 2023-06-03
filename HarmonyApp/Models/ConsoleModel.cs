using HarmonyApp.AudioProcessing;
using SoundFingerprinting.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

        static List<Audiofile> _matches = new();

        static bool isArgsCorrect = true;

        private static void SetDefaultScanSettings()
        {
            isRecursive = false;
            scanPath = Directory.GetCurrentDirectory();
            outputPath = scanPath;
            movePath = null;
            isRemove = false;
            isConfirmationNotRequired = false;
        }

        public static async Task ProcessArgs(string[] args)
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
                            isArgsCorrect = false;
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
                                isArgsCorrect = true;
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
                            isArgsCorrect = false;
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
                                isArgsCorrect = true;
                            }
                        }
                        break;
                    case "-m":
                    case "--move":
                        {
                            isArgsCorrect = false;
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
                                isArgsCorrect = true;
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
                        isArgsCorrect = false;
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
                        isArgsCorrect = false;
                        Console.WriteLine($"Неизвестный параметр [{arg}]");
                        Console.WriteLine($"Используйте параметр -h для отображения спавки по исользованию приложения");
                        break;
                }
            }

            if (isArgsCorrect)
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

        private static readonly object _matchesLock = new();

        private static async Task StartScanAsync()
        {
            Fingerprinting.InitilizeModelService();
            Console.WriteLine($"Поиск дубликатов в {scanPath}");
            var filesCount = AudiofilesEnumerator.GetEnumerableFiles(scanPath, isRecursive).Count();
            Console.WriteLine($"Обнаружено {filesCount} музыкальных файлов\n");

            await GetAVHashes();
            await CompareHashes();

            Console.WriteLine($"\nСканирование завершено! Найдено {_matches.Count} дубликатов");
            _matches.ForEach(x => Console.WriteLine(x.DisplayPath));

            if (isRemove)
            {
                string? response = "";
                if (!isConfirmationNotRequired)
                    Console.WriteLine($"Файлы ({_matches.Count(x => x.IsSelected)}) будут удалены. Продолжить? (Д\\Н)");
                response = isConfirmationNotRequired ? "д" : Console.ReadLine()?.ToLower();
                if (response == "д")
                {
                    foreach (var audiofile in _matches.Where(x => x.IsSelected))
                    {
                        DeleteFile(audiofile._path);
                    }
                    Console.WriteLine("Файлы удалены!");
                }
            }
            if (movePath is not null)
            {
                string? response = "";
                if (!isConfirmationNotRequired)
                    Console.WriteLine($"Файлы ({_matches.Count(x => x.IsSelected)}) будут перемещены в '{movePath}'. Продолжить? (Д\\Н)");
                response = isConfirmationNotRequired ? "д" : Console.ReadLine()?.ToLower();
                if (response == "д")
                {
                    foreach (var audiofile in _matches.Where(x => x.IsSelected))
                    {
                        MoveFile(audiofile._path, movePath);
                    }
                    Console.WriteLine("Файлы перемещены!");
                }
            }

            outputPath += outputPath.Last() == '\\' ? "" : "\\";
            outputPath += $"Гармония_сканирование_{DateTime.Now:dd.MM.yy}_{DateTime.Now:HH-mm}.txt";
            Console.WriteLine($"Отчет сохранен в файл: {outputPath}");
        }

        private static async Task CompareHashes()
        {
            _matches = await Fingerprinting.CompareStored();
        }

        private static async Task GetAVHashes()
        {
            await Parallel.ForEachAsync(AudiofilesEnumerator.GetEnumerableFiles(scanPath, isRecursive), async (filePath, token) =>
            {
                Console.WriteLine($"Обработка {Path.GetFileName(filePath)}");
                SoundFingerprinting.Data.AVHashes hashes = AVHashes.Empty;
                try
                {
                    hashes = await Fingerprinting.GetAVHashesAsync(filePath);
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }

                if (!hashes.IsEmpty)
                {
                    Fingerprinting.StoreAVHashes(filePath, hashes);
                }
            });
        }
    }
}
