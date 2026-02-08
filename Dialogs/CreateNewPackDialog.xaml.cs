using System.Windows;
using System.Windows.Controls;
using Quiz_Configurator.Models;
using Quiz_Configurator.ViewModel;

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

        public Category? SelectedCategory => CategoryComboBox.SelectedItem as Category;

        public New_Question_Pack()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var categoryName = ShowInputDialog("Enter category name:", "New Category");

                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var newCategory = new Category { Name = categoryName };
                    await App.MongoDBDataService.SaveCategoryAsync(newCategory);

                    // Refresh the categories in the current dialog
                    if (DataContext is QuestionPackViewModel viewModel)
                    {
                        var categories = await App.MongoDBDataService.LoadCategoriesAsync();
                        viewModel.Categories.Clear();
                        foreach (var cat in categories)
                            viewModel.Categories.Add(cat);

                        viewModel.SelectedCategory = categories.FirstOrDefault(c => c.Name == categoryName);
                    }

                    MessageBox.Show($"Category '{categoryName}' created successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating category: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string? ShowInputDialog(string prompt, string title)
        {
            var inputWindow = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            var label = new Label { Content = prompt };
            var textBox = new TextBox { Height = 25, Margin = new Thickness(0, 5, 0, 10) };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 25,
                Margin = new Thickness(5, 0, 0, 0),
                IsDefault = true
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 75,
                Height = 25,
                IsCancel = true
            };

            okButton.Click += (s, e) => { inputWindow.DialogResult = true; };
            cancelButton.Click += (s, e) => { inputWindow.DialogResult = false; };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(buttonPanel);

            inputWindow.Content = stackPanel;

            textBox.Focus();

            return inputWindow.ShowDialog() == true ? textBox.Text?.Trim() : null;
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