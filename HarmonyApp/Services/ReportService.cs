using HarmonyApp.FolderProcessing;
using HarmonyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace HarmonyApp.Services
{
    public static class ReportService
    {
        public async static void SaveReport(string reportPath, ObservableCollection<Audiofile> duplicates)
        {
            try
            {
                using StreamWriter writer = new(reportPath, true);
                await writer.WriteLineAsync("Отчет о сканировании");
                foreach (var path in FoldersContainer.Paths)
                {
                    await writer.WriteLineAsync(path.Path);
                }
                await writer.WriteLineAsync($"Файлов отсканировано: {FoldersContainer.Paths.Sum(x => x.filesCount)}\n");

                await writer.WriteLineAsync($"Обнаружено дубликатов: {duplicates.Count}");

                foreach (var file in duplicates)
                {
                    if (file.IsChild)
                    {
                        await writer.WriteLineAsync($"{file._path} Рейтинг:{String.Format("{0:0.##}%", file.RatingValue)} Похожесть: {file.Similarity} Начало совпадения: {file.MatchedAt}");
                    }
                    else
                    {
                        await writer.WriteLineAsync($"\n{file._path} Рейтинг:{String.Format("{0:0.##}%", file.RatingValue)}");
                    }
                }

                writer.Dispose();

                if (!File.Exists(reportPath))
                {
                    return;
                }

                string argument = "/select, \"" + reportPath + "\"";

                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Не удалось сохранить отчет!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
