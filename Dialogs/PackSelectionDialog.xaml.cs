using Quiz_Configurator.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Quiz_Configurator.Dialogs
{
    /// <summary>
    /// Interaction logic for PackSelectionDialog.xaml
    /// </summary>
    public partial class PackSelectionDialog : Window
    {
        public QuestionPackViewModel? SelectedPack { get; private set; }

        public PackSelectionDialog(ObservableCollection<QuestionPackViewModel> packs)
        {
            InitializeComponent();
            PackListBox.ItemsSource = packs;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (PackListBox.SelectedItem is QuestionPackViewModel selectedPack)
            {
                SelectedPack = selectedPack;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a question pack.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PackListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Optional: You can add preview functionality here
        }
    }
}
