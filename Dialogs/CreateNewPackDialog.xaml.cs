using System;
using System.Collections.Generic;
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
using Quiz_Configurator.Models;

namespace Quiz_Configurator.Dialogs
{
    /// <summary>
    /// Interaction logic for New_Question_Pack.xaml
    /// </summary>
    public partial class New_Question_Pack : Window
    {
        public string PackName => PackNameTextBox.Text;
        public Difficulty SelectedDifficulty { get; private set; } = Difficulty.Medium;
        public int TimeLimitInSeconds => (int)TimeLimitSlider.Value;

        public New_Question_Pack()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                SelectedDifficulty = selectedItem.Content.ToString() switch
                {
                    "Easy" => Difficulty.Easy,
                    "Medium" => Difficulty.Medium,
                    "Difficult" => Difficulty.Hard,
                    _ => Difficulty.Medium
                };
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PackName))
            {
                MessageBox.Show("Pack name cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}