using System;

namespace HarmonyApp.Services
{
    internal static class MediaPlayerService
    {
        internal static System.Windows.Controls.MediaElement MediaPlayer = new System.Windows.Controls.MediaElement() { UnloadedBehavior = System.Windows.Controls.MediaState.Manual };
        internal static bool IsPlaying = false;
        internal static string? Path;



        internal static void Initialize(string filepath)
        {
            Path = filepath;
            MediaPlayer.Source = new Uri(filepath, UriKind.Relative);
        }

        internal static void PlayPause()
        {
            if (IsPlaying)
                Pause();
            else
                Play();
        }

        private static void Play()
        {

            if (Path != null)
            {
                MediaPlayer.Play();
                IsPlaying = true;
            }
        }

        private static void Pause()
        {

            if (Path != null)
            {
                MediaPlayer.Pause();
                IsPlaying = false;
            }
        }

        internal static void Stop()
        {
            if (Path != null)
                MediaPlayer.Stop();
            Path = null;
        }
    }
}
