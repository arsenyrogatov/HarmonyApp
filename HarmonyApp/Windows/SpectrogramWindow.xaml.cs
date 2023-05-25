using Spectrogram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HarmonyApp.Windows
{
    public partial class SpectrogramWindow : Window
    {
        public SpectrogramWindow(Audiofile audiofile)
        {
            InitializeComponent();
            Title = $"Гармония ({audiofile._path})";
            (double[] audio, int sampleRate) = ReadMono(audiofile._path);

            int fftSize = 16384;
            int targetWidthPx = 3000;
            int stepSize = audio.Length / targetWidthPx;

            var sg = new SpectrogramGenerator(sampleRate, fftSize, stepSize, maxFreq: 2200);
            sg.Add(audio);
            var spectrogram = sg.GetBitmap(intensity: 5, dB: true);
            image.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               spectrogram.GetHbitmap(),
               IntPtr.Zero,
               System.Windows.Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight(spectrogram.Width, spectrogram.Height));
        }

        (double[] audio, int sampleRate) ReadMono(string filePath, double multiplier = 16_000)
        {
            using var afr = new NAudio.Wave.AudioFileReader(filePath);
            int sampleRate = afr.WaveFormat.SampleRate;
            int bytesPerSample = afr.WaveFormat.BitsPerSample / 8;
            int sampleCount = (int)(afr.Length / bytesPerSample);
            int channelCount = afr.WaveFormat.Channels;
            var audio = new List<double>(sampleCount);
            var buffer = new float[sampleRate * channelCount];
            int samplesRead = 0;
            while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0)
                audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
            return (audio.ToArray(), sampleRate);
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider is not null && image is not null && image.Source is not null)
            {
                var newHeight = Math.Min(305 * slider.Value, double.MaxValue);
                canvas.Height = newHeight >= 50 ? newHeight : 50;
                
                var newWidth = Math.Min(800 * slider.Value, double.MaxValue);
                canvas.Width = newWidth >= 50 ? newWidth : 50;
            }
        }
    }
}
