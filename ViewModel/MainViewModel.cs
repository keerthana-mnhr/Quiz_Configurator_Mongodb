using MongoDB.Driver;
using Quiz_Configurator.Command;
using Quiz_Configurator.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Quiz_Configurator.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private QuestionPackViewModel? _activePack;
        private bool _isLoading;
        private bool _isManualQuestionOperation; // Add this flag to control auto-save

        public MainWindowViewModel()
        {
            Packs = new ObservableCollection<QuestionPackViewModel>();
            ConfigurationViewModel = new ConfigurationViewModel(this);
            _ = LoadPacksFromStorageAsync();
            _ = LoadCategoriesAsync();
        }

        public DelegateCommand AddCategoryCommand => new DelegateCommand(AddCategory);

        private async void AddCategory(object? parameter)
        {
            try
            {
                var categoryName = ShowInputDialog("Enter category name:", "New Category");

                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var newCategory = new Category { Name = categoryName };
                    await App.MongoDBDataService.SaveCategoryAsync(newCategory);
                    await LoadCategoriesAsync(); // Refresh categories
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

        // Add C# input dialog method instead of VB.NET InputBox
        private string? ShowInputDialog(string prompt, string title)
        {
            var inputWindow = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
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

        public ObservableCollection<QuestionPackViewModel> Packs { get; }

        public QuestionPackViewModel? ActivePack
        {
            get => _activePack;
            set
            {
                if (SetProperty(ref _activePack, value))
                {
                    OnActivePackChanged();
                }
            }
        }

        public ObservableCollection<Category> Categories { get; } = new();

        public async Task LoadCategoriesAsync()
        {
            var categories = await App.MongoDBDataService.LoadCategoriesAsync();
            Categories.Clear();
            foreach (var cat in categories)
                Categories.Add(cat);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Add property to control manual question operations
        public bool IsManualQuestionOperation
        {
            get => _isManualQuestionOperation;
            set => _isManualQuestionOperation = value;
        }

        // Add methods to control auto-save behavior
        public void DisableAutoSave()
        {
            _isManualQuestionOperation = true;
        }

        public void EnableAutoSave()
        {
            _isManualQuestionOperation = false;
        }

        public ConfigurationViewModel? ConfigurationViewModel { get; }

        private void OnActivePackChanged()
        {
            RaisePropertyChanged(nameof(ActivePack));

            // Only save when not loading and not during manual question operations
            if (!_isLoading && !_isManualQuestionOperation && _activePack != null)
            {
                _ = SavePackToStorageAsync(_activePack);
            }
        }

        public async Task LoadPacksFromStorageAsync()
        {
            try
            {
                IsLoading = true;

                // Load categories first
                await LoadCategoriesAsync();

                var savedPacks = await App.MongoDBDataService.LoadPacksAsync();
                Packs.Clear();

                if (savedPacks.Count > 0)
                {
                    foreach (var pack in savedPacks)
                    {
                        var packViewModel = new QuestionPackViewModel(pack);

                        // Assign categories to each pack
                        foreach (var category in Categories)
                        {
                            packViewModel.Categories.Add(category);
                        }

                        // Initialize the selected category based on CategoryId
                        packViewModel.InitializeSelectedCategoryFromId();

                        Packs.Add(packViewModel);
                        SetupAutoSaveForPack(packViewModel);
                    }
                    ActivePack = Packs.FirstOrDefault();
                }
                else
                {
                    await LoadDefaultPack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading saved packs: {ex.Message}\n\nLoading default pack instead.",
                    "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                await LoadDefaultPack();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SavePackToStorageAsync(QuestionPackViewModel packViewModel)
        {
            try
            {
                var pack = packViewModel.GetQuestionPack();
                await App.MongoDBDataService.SavePackAsync(pack);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving pack '{packViewModel.Name}': {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SavePackToStorage(QuestionPackViewModel packViewModel)
        {
            try
            {
                var pack = packViewModel.GetQuestionPack();
                App.MongoDBDataService.SavePack(pack);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving pack '{packViewModel.Name}': {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task SaveAllPacksToStorageAsync()
        {
            try
            {
                foreach (var packViewModel in Packs)
                {
                    await SavePackToStorageAsync(packViewModel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving packs:{ex.Message} ",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public async Task AddPackAsync(QuestionPackViewModel packViewModel)
        {
            try
            {
                // Ensure categories are assigned
                foreach (var category in Categories)
                {
                    packViewModel.Categories.Add(category);
                }

                // Save to database first
                var pack = packViewModel.GetQuestionPack();
                await App.MongoDBDataService.SavePackAsync(pack);

                // Only add to UI collection after successful save
                Packs.Add(packViewModel);
                SetupAutoSaveForPack(packViewModel);
                ActivePack = packViewModel;
            }
            catch (Exception)
            {
                // Don't add to collection if save failed
                throw; // Re-throw to let calling method handle the error
            }
        }

        public async Task RemovePackAsync(QuestionPackViewModel packViewModel)
        {
            try
            {
                await App.MongoDBDataService.DeletePackAsync(packViewModel.Name);
                Packs.Remove(packViewModel);

                if (ActivePack == packViewModel)
                {
                    ActivePack = Packs.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting pack '{packViewModel.Name}': {ex.Message}", "Delete Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task RenamePackAsync(QuestionPackViewModel packViewModel, string newName)
        {
            try
            {
                var oldName = packViewModel.Name;
                await App.MongoDBDataService.DeletePackAsync(oldName);
                packViewModel.Name = newName;
                await SavePackToStorageAsync(packViewModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming pack: {ex.Message}", "Rename Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadDefaultPack()
        {
            var existingDefaultPack = Packs.FirstOrDefault(p => p.Name.Contains("Default Questions"));
            if (existingDefaultPack != null)
            {
                ActivePack = existingDefaultPack;
                return;
            }

            var defaultPack = new QuestionPack("Default Questions")
            {
                Difficulty = Difficulty.Medium,
                TimeLimitInSeconds = 30
            };

            defaultPack.Questions.AddRange(new[]
            {
                    new Question("What is the capital of France?", "Paris", "London", "Berlin", "Madrid"),
                    new Question("Which planet is known as the Red Planet?", "Mars", "Venus", "Jupiter", "Saturn"),
                    new Question("What is 2 + 2?", "4", "3", "5", "6"),
                    new Question("Who wrote 'Romeo and Juliet'?", "William Shakespeare", "Charles Dickens", "Jane Austen", "Mark Twain"),
                    new Question("What is the largest mammal in the world?", "Blue Whale", "Elephant", "Giraffe", "Hippopotamus")
                });

            var packViewModel = new QuestionPackViewModel(defaultPack);

            using var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("KeerthanaManoharan");

            var categoryCollection = database.GetCollection<Category>("categories");
            var categories = await categoryCollection.Find(Builders<Category>.Filter.Empty).ToListAsync();

            if (categories.Count == 0)
            {
                var defaultCategories = new List<Category>
                    {
                        new Category { Name = "General Knowledge" },
                        new Category { Name = "Science" },
                        new Category { Name = "History" }
                    };

                await categoryCollection.InsertManyAsync(defaultCategories);
                categories = await categoryCollection.Find(Builders<Category>.Filter.Empty).ToListAsync();
            }

            var selectedCategory = categories.First();
            defaultPack.CategoryId = selectedCategory.Id;
            var collection = database.GetCollection<QuestionPack>("questionPacks");

            await collection.InsertOneAsync(defaultPack);

            await LoadPacksFromStorageAsync();
        }

        // Updated SetupAutoSaveForPack method with manual operation control
        private void SetupAutoSaveForPack(QuestionPackViewModel packViewModel)
        {
            packViewModel.Questions.CollectionChanged += (s, e) =>
            {
                // Only auto-save if not loading AND not during manual question operations
                if (!_isLoading && !_isManualQuestionOperation)
                {
                    _ = SavePackToStorageAsync(packViewModel);
                }
            };

            packViewModel.PropertyChanged += (s, e) =>
            {
                if (!_isLoading && !_isManualQuestionOperation &&
                    (e.PropertyName == nameof(packViewModel.Name) ||
                     e.PropertyName == nameof(packViewModel.Difficulty) ||
                     e.PropertyName == nameof(packViewModel.TimeLimitInSeconds)))
                {
                    _ = SavePackToStorageAsync(packViewModel);
                }
            };
        }
    }
}