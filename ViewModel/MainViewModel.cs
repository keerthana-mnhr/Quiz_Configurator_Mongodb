using MongoDB.Driver;
using Quiz_Configurator.Models;
using Quiz_Configurator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Quiz_Configurator.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private QuestionPackViewModel? _activePack;
        private bool _isLoading;

        public MainWindowViewModel()
        {
            Packs = new ObservableCollection<QuestionPackViewModel>();
            PlayerViewModel = new PlayerViewModel(this);
            ConfigurationViewModel = new ConfigurationViewModel(this);
            _ = LoadPacksFromStorageAsync();
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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public PlayerViewModel? PlayerViewModel { get; }
        public ConfigurationViewModel? ConfigurationViewModel { get; }

        private void OnActivePackChanged()
        {
            RaisePropertyChanged(nameof(ActivePack));

            if (!_isLoading && _activePack != null)
            {
                _ = SavePackToStorageAsync(_activePack);
            }
        }

        public async Task LoadPacksFromStorageAsync()
        {
            try
            {
                IsLoading = true;
                var savedPacks = await App.MongoDBDataService.LoadPacksAsync();
                Packs.Clear();

                if (savedPacks.Count > 0)
                {
                    foreach (var pack in savedPacks)
                    {
                        var packViewModel = new QuestionPackViewModel(pack);
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
            Packs.Add(packViewModel);
            SetupAutoSaveForPack(packViewModel);
            ActivePack = packViewModel;
            await SavePackToStorageAsync(packViewModel);
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
            //SetupAutoSaveForPack(packViewModel);
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("KeerthanaManoharan");
            var collection = database.GetCollection<QuestionPack>("questionPacks");

            await collection.InsertOneAsync(defaultPack);
            //Packs.Add(packViewModel);
            //ActivePack = packViewModel;

            //if (!_isLoading)
            //{
                
                //_ = SavePackToStorageAsync(packViewModel);
                await LoadPacksFromStorageAsync();
            //}
        }

        private void SetupAutoSaveForPack(QuestionPackViewModel packViewModel)
        {
            packViewModel.Questions.CollectionChanged += (s, e) =>
            {
                if (!_isLoading)
                {
                    _ = SavePackToStorageAsync(packViewModel);
                }
            };

            packViewModel.PropertyChanged += (s, e) =>
            {
                if (!_isLoading && (e.PropertyName == nameof(packViewModel.Name) ||
                                  e.PropertyName == nameof(packViewModel.Difficulty) ||
                                  e.PropertyName == nameof(packViewModel.TimeLimitInSeconds)))
                {
                    _ = SavePackToStorageAsync(packViewModel);
                }
            };
        }
    }
}