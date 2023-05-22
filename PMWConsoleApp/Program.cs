/*using PMW_lib;
using System.Diagnostics;

var hashes = await PMWFingerprinting.GetAVHashesAsync(@"Q:\Music\tmp\Resonance.wav");

Console.WriteLine($"\nФайл Q:\\Music\\tmp\\Home - Resonance.mp3\n");
Console.WriteLine($"Отпечаток:");

int k = 0;

foreach (var a in hashes.Audio.OrderBy(x => x.SequenceNumber))
{
    Console.WriteLine($"\nId: {a.SequenceNumber}");
    Console.WriteLine($"Начало: {a.StartsAt} сек");
    Console.WriteLine("Хэш значений:");
    string str = "";
    for (int i = 0; i < 20; i++)
    {
        if (i % 5 == 0 && i != 0)
        {
            Console.WriteLine(str);
            str = a.HashBins[i].ToString() + " ";
        }
        else
        {
            str += a.HashBins[i].ToString() + " ";
        }
    }
    Console.WriteLine(str);
    if (++k == 3) break;
}

Console.ReadKey();*/

/*using System;
using System.IO;
using System.Numerics;
using MathNet.Numerics;
using System.Windows.Forms;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System.Drawing;

string audioFilePath = @"Q:\Music\TestData\104 - WHO.mp3";
WaveFileReader reader = new WaveFileReader(audioFilePath);
float[] audioData = new float[reader.Length / 4];
reader.Read(audioData.Select(x => Convert.ToByte(x)).ToArray(), 0, audioData.Length);
int sampleRate = reader.WaveFormat.SampleRate;
reader.Close();

Complex32[] spectrum = new Complex32[audioData.Length];
for (int i = 0; i < audioData.Length; i++)
{
    spectrum[i] = new Complex32(audioData[i], 0);
}
Fourier.Forward(spectrum, FourierOptions.NoScaling);

double[] dB = new double[audioData.Length / 2];
double[] freqs = new double[audioData.Length / 2];
double refLevel = 1.0;
for (int i = 0; i < audioData.Length / 2; i++)
{
    double re = spectrum[i].Real;
    double im = spectrum[i].Imaginary;
    double mag = Math.Sqrt(re * re + im * im);
    double power = mag * mag;
    dB[i] = 10 * Math.Log10(power / refLevel);
    freqs[i] = (double)i / audioData.Length * sampleRate / 2.0;
}

var plotModel = new PlotModel();
plotModel.Title = "Spectrogram";

var dBseries = new LineSeries();
for (int i = 0; i < audioData.Length / 2; i++)
{
    dBseries.Points.Add(new DataPoint(freqs[i], dB[i]));
}
plotModel.Series.Add(dBseries);

var xAxis = new LinearAxis();
xAxis.Title = "Frequency (Hz)";
xAxis.Position = AxisPosition.Bottom;
plotModel.Axes.Add(xAxis);

var yAxis = new LinearAxis();
yAxis.Title = "dB";
yAxis.Position = AxisPosition.Left;
plotModel.Axes.Add(yAxis);

var plotView = new PlotView();
plotView.Model = plotModel;
plotView.Dock = DockStyle.Fill;
Bitmap bitmap = new Bitmap(Convert.ToInt32(xAxis.Maximum), Convert.ToInt32(yAxis.Maximum));
plotView.DrawToBitmap(bitmap, new Rectangle(0,0, Convert.ToInt32(xAxis.Maximum), Convert.ToInt32(yAxis.Maximum)));
bitmap.Save("file.png", System.Drawing.Imaging.ImageFormat.Png);*/

/*Console.WriteLine("Поиск дубликатов в Q:\\Music\\TestData");
Console.WriteLine("Обнаружено 17 музыкальных файлов\n");

Console.WriteLine(@"Q:\Music\TestData\BATO & 104 - WHO.mp3");
Console.WriteLine(@"Q:\Music\TestData\104 - WHO.mp3 Начало совпадения: 00:01:22");
Console.WriteLine();
Console.WriteLine(@"Q:\Music\TestData\Drake - God's Plan.mp3");
Console.WriteLine(@"Q:\Music\TestData\дрейк гадсплен.mp3 Начало совпадения: 00:00:00");
Console.WriteLine();
Console.WriteLine(@"Q:\Music\TestData\Famous Dex - JAPAN (original).mp3");
Console.WriteLine(@"Q:\Music\TestData\Famous Dex - JAPAN (white noise).mp3 Начало совпадения: 00:00:00");
Console.WriteLine();
Console.WriteLine(@"Q:\Music\TestData\French Montana feat Swae Lee - Unforgetable (32000).mp3");
Console.WriteLine(@"Q:\Music\TestData\French Montana feat Swae Lee - Unforgetable (48000).mp3 Начало совпадения: 00:00:00");
Console.WriteLine();
Console.WriteLine(@"Q:\Music\TestData\Home - Resonance.mp3");
Console.WriteLine(@"Q:\Music\TestData\Home - Resonance.wav Начало совпадения: 00:00:00");
Console.WriteLine();
Console.WriteLine(@"Q:\Music\TestData\Kid Cudi - Day 'n' Nite (128).mp3");
Console.WriteLine(@"Q:\Music\TestData\Kid Cudi - Day 'n' Nite (320).mp3 Начало совпадения: 00:00:00");
Console.WriteLine();
Console.WriteLine(@"Q:\Music\TestData\Lana Del Rey - Summertime Sadness (original).mp3");
Console.WriteLine(@"Q:\Music\TestData\Lana Del Rey - Summertime Sadness (speed).mp3 Начало совпадения: 00:00:00");
Console.WriteLine(@"Q:\Music\TestData\Lana Del Rey - Summertime Sadness (slow ).mp3 Начало совпадения: 00:00:00");
Console.WriteLine();
Console.WriteLine(@"Q:\Music\TestData\Mac DeMarco - Chamber of Reflection.mp3");
Console.WriteLine(@"Q:\Music\TestData\Mac DeMarco - Chamber of Reflection (1).mp3 Начало совпадения: 00:00:00");
Console.WriteLine();

Console.WriteLine("\nСканирование завершено! Найдено 17 дубликатов");
Console.WriteLine(@"Отчет сохранен в файл: C:\Users\ARSENY\Диплом\Гармония_сканирование_14.04.23_02-40.txt");*/

/*Console.WriteLine($"Файлы (4) будут удалены. Продолжить? (Д\\Н): д");
Console.WriteLine("Файлы удалены!");
Console.WriteLine("\n"+@"Отчет сохранен в файл: C:\Users\ARSENY\Диплом\Гармония_сканирование_18.04.23_15-10.txt");*/

Console.WriteLine(@"C:\Users\ARSENY\Диплом>Гармония.exe --help");
Console.WriteLine("Параметры:");
Console.WriteLine("\t'-c'  или '--console'\tзапуск со стандартными параметрами");
Console.WriteLine("\t'-d'  или '--dir'\tпуть к директории, в которой необходимо произвести поиск дубликатов");
Console.WriteLine("\t'-r'  или '--recursive'\tвключение поиска дубликатов во всех поддиректориях указанной папки");
Console.WriteLine("\t'-o'  или '--output'\tвыбор директории для сохранения отчета");
Console.WriteLine("\t'-m'  или '--move'\tперемещение найденных дубликатов с меньшим рейтингом в указанную директорию");
Console.WriteLine("\t'-rm' или '--remove'\tудаление найденных дубликатов с меньшим рейтингом");
Console.WriteLine("\t'-f'  или '--force'\tотключение подтверждения перед перемещением или удалением");
Console.WriteLine("\t'-h'  или '--help'\tотображение справки по использованию приложения");