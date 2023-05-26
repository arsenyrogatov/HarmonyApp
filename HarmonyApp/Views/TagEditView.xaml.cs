using HarmonyApp.ViewModels;
using System.Windows;

namespace HarmonyApp.Views
{
    /// <summary>
    /// Логика взаимодействия для TagEditWindow.xaml
    /// </summary>
    public partial class TagEditView : Window
    {
        public TagEditView()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveAndCloseWindow(object sender, RoutedEventArgs e)
        {
            ((TagEditViewModel)DataContext)._model.SaveTags();
            Close();
        }
    }
}
