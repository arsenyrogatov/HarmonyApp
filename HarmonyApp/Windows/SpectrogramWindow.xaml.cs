using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HarmonyApp.Windows
{
    public partial class SpectrogramWindow : Window
    {
        private readonly string AudiofilePath;
        private Size RenderCanvasSize;

        public SpectrogramWindow(Audiofile audiofile)
        {
            InitializeComponent();
            Title = $"Гармония ({audiofile._path})";
            AudiofilePath = audiofile._path;
            Show();
            Task.Run(() => LoadSpectrogramAsync());
        }

        private async Task LoadSpectrogramAsync ()
        {
            System.Drawing.Bitmap? SpectrogramBitmap = await AudioProcessing.SpectrogramGenerator.GetSpectrogramAsync(AudiofilePath);
            await Dispatcher.InvokeAsync(() =>
            {
                if (SpectrogramBitmap is not null)
                {
                    Plug.Visibility = Visibility.Collapsed;
                    SpectrogramImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                       SpectrogramBitmap.GetHbitmap(),
                       IntPtr.Zero,
                       System.Windows.Int32Rect.Empty,
                       BitmapSizeOptions.FromWidthAndHeight(SpectrogramBitmap.Width, SpectrogramBitmap.Height));
                }
                RenderCanvasSize = SpectrogramCanvas.RenderSize;
            });
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ScaleSlider is not null && SpectrogramImage is not null && SpectrogramImage.Source is not null && SliderValue is not null)
            {
                var ScaleRate = ScaleSlider.Value;
                SliderValue.Text = ScaleRate.ToString("0.0")+"x";
                SpectrogramCanvas.Height = RenderCanvasSize.Height * ScaleRate;
                SpectrogramCanvas.Width = RenderCanvasSize.Width * ScaleRate;
            }
        }
    }
}
