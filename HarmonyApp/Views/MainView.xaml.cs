using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HarmonyApp.ViewModels;

namespace HarmonyApp.Views
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

        private void TextHint_label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            searchTextBox.Focus();
            e.Handled = true;
        }

        private void searchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Visibility == Visibility.Visible && TextHint_label.Visibility == Visibility.Visible)
            {
                TextHint_label.Visibility = Visibility.Collapsed;
            }
        }

        private void searchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == "" || searchTextBox.Text == String.Empty)
            {
                TextHint_label.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Actions_ComboBox.IsDropDownOpen = false;
        }
        private void duplicates_DataGrid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            duplicates_DataGrid.CommitEdit();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //((CheckBox)sender)
            duplicates_DataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        }
    }

}
