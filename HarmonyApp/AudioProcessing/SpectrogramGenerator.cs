using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HarmonyApp.AudioProcessing
{
    class SpectrogramGenerator
    {
        public static async Task<System.Drawing.Bitmap?> GetSpectrogramAsync(string AudiofilePath)
        {
            (double[] audio, int sampleRate) = ReadMono(AudiofilePath);

            int fftSize = 16384;
            int targetWidthPx = 3000;
            int stepSize = audio.Length / targetWidthPx;

            var sg = new Spectrogram.SpectrogramGenerator(sampleRate, fftSize, stepSize, maxFreq: 2200);
            sg.Add(audio);
            return await Task.FromResult(sg.GetBitmap(intensity: 5, dB: true));
        }

        private static (double[] audio, int sampleRate) ReadMono(string filePath, double multiplier = 16_000)
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

    }
}
