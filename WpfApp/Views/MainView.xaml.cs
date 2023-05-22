using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using PMW_lib;
using System.Threading;
using System.Windows.Threading;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            isPaused = true;
        }

        private bool isPaused;
        private void PlayPause_button_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAudiofilePlayer.Source.AbsolutePath != "")
            {
                if (isPaused)
                    SelectedAudiofilePlayer.Play();
                else
                    SelectedAudiofilePlayer.Pause();
                isPaused = !isPaused;
                UpdateIcons();
            }

        }

        private void UpdateIcons()
        {
            PlayButtonIcon.Visibility = isPaused ? Visibility.Visible : Visibility.Hidden;
            PauseButtonIcon.Visibility = isPaused ? Visibility.Hidden : Visibility.Visible;
        }

        private void SelectedAudiofilePlayer_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            isPaused = true;
            SelectedAudiofilePlayer.Pause();
            UpdateIcons();
        }

        private void SelectedAudiofilePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            isPaused = true;
            SelectedAudiofilePlayer.Pause();
            UpdateIcons();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var searchPattern = searchTextBox.Text;

            var _itemSourceList = new CollectionViewSource() { Source = ((MainViewModel)DataContext).AudiofileMatches };

            System.ComponentModel.ICollectionView Itemlist = _itemSourceList.View;

            if (searchPattern.Length > 0)
            {
                var yourCostumFilter = new Predicate<object>(item => ((Audiofile)item)._path.Contains(searchPattern, StringComparison.OrdinalIgnoreCase));
                Itemlist.Filter = yourCostumFilter;
            }
            duplicates_DataGrid.ItemsSource = Itemlist;
        }

        private void searchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Visibility == Visibility.Visible && TextHint_label.Visibility == Visibility.Visible)
            {
                TextHint_label.Visibility = Visibility.Collapsed;
            }
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchTextBox is not null && TextHint_label is not null)
            {
                TextHint_label.Visibility = searchTextBox.Visibility == Visibility.Visible && searchTextBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            duplicates_DataGrid.Visibility = Visibility.Collapsed;
            duplicates_DataGrid.Visibility = Visibility.Visible;
        }
    }

}
