using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Quiz_Configurator.Command;
using Quiz_Configurator.Dialogs;
using Quiz_Configurator.Models;

namespace Quiz_Configurator.ViewModel
{
    internal class ConfigurationViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? mainWindowViewModel;

        public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel)
        {
            this.mainWindowViewModel = mainWindowViewModel;

            // Initialize commands
            LoadDefaultPackCommand = new DelegateCommand(LoadDefaultPack);

            // Question management commands
            AddQuestionCommand = new DelegateCommand(AddQuestion, CanAddQuestion);
            AddQuestionFromMenuCommand = new DelegateCommand(AddQuestionFromMenu);
            DeleteQuestionCommand = new DelegateCommand(DeleteQuestion, CanDeleteQuestion);
            UpdateQuestionCommand = new DelegateCommand(UpdateQuestion, CanUpdateQuestion); // Use new method
            CancelEditCommand = new DelegateCommand(CancelEdit);
            EditPackCommand = new DelegateCommand(EditPack, CanEditPack);
            CreateNewPackCommand = new DelegateCommand(CreateNewPack);
            SelectPackCommand = new DelegateCommand(SelectPack);
            DeletePackCommand = new DelegateCommand(DeletePack, CanDeletePack);
        }



        public DelegateCommand LoadDefaultPackCommand { get; }
        public DelegateCommand AddQuestionCommand { get; }
        public DelegateCommand AddQuestionFromMenuCommand { get; }
        public DelegateCommand DeleteQuestionCommand { get; }
        public DelegateCommand UpdateQuestionCommand { get; }
        public DelegateCommand CancelEditCommand { get; }
        public DelegateCommand EditPackCommand { get; }
        public DelegateCommand CreateNewPackCommand { get; }
        public DelegateCommand SelectPackCommand { get; }
        public DelegateCommand DeletePackCommand { get; }



        public MainWindowViewModel? MainWindowViewModel => mainWindowViewModel;

        private string _newQuestionText = string.Empty;
        public string NewQuestionText
        {
            get => _newQuestionText;
            set
            {
                _newQuestionText = value;
                RaisePropertyChanged();
                AddQuestionCommand.RaiseCanExecuteChanged();
                UpdateQuestionCommand.RaiseCanExecuteChanged(); // Add this line
            }
        }

        private string _correctAnswer = string.Empty;
        public string CorrectAnswer
        {
            get => _correctAnswer;
            set
            {
                _correctAnswer = value;
                RaisePropertyChanged();
                AddQuestionCommand.RaiseCanExecuteChanged();
                UpdateQuestionCommand.RaiseCanExecuteChanged(); // Add this line
            }
        }

        private string _firstIncorrectAnswer = string.Empty;
        public string FirstIncorrectAnswer
        {
            get => _firstIncorrectAnswer;
            set
            {
                _firstIncorrectAnswer = value;
                RaisePropertyChanged();
                AddQuestionCommand.RaiseCanExecuteChanged();
                UpdateQuestionCommand.RaiseCanExecuteChanged(); // Add this line
            }
        }

        private string _secondIncorrectAnswer = string.Empty;
        public string SecondIncorrectAnswer
        {
            get => _secondIncorrectAnswer;
            set
            {
                _secondIncorrectAnswer = value;
                RaisePropertyChanged();
                AddQuestionCommand.RaiseCanExecuteChanged();
                UpdateQuestionCommand.RaiseCanExecuteChanged(); // Add this line
            }
        }

        private string _thirdIncorrectAnswer = string.Empty;
        public string ThirdIncorrectAnswer
        {
            get => _thirdIncorrectAnswer;
            set
            {
                _thirdIncorrectAnswer = value;
                RaisePropertyChanged();
                AddQuestionCommand.RaiseCanExecuteChanged();
                UpdateQuestionCommand.RaiseCanExecuteChanged(); // Add this line
            }
        }


        private bool _isEditingQuestion;
        private Question? _selectedQuestion;

        public bool IsEditingQuestion
        {
            get => _isEditingQuestion;
            set
            {
                _isEditingQuestion = value;
                RaisePropertyChanged();
            }
        }

        public Question? SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                _selectedQuestion = value;
                RaisePropertyChanged();
                DeleteQuestionCommand.RaiseCanExecuteChanged();
                UpdateQuestionCommand.RaiseCanExecuteChanged(); // Add this line

                // Load the selected question into the form for editing
                if (value != null)
                {
                    LoadQuestionForEditing(value);
                }
                else
                {
                    ClearForm();
                }
            }
        }



        #region Command Implementations
        private void AddQuestionFromMenu(object? parameter)
        {
            if (mainWindowViewModel?.ActivePack == null)
            {
                MessageBox.Show("No active pack selected. Please create or select a pack first.", "No Active Pack",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Create a new template question
                var question = new Question(
                    "New Question",
                    "Correct Answer",
                    "Wrong Answer 1",
                    "Wrong Answer 2",
                    "Wrong Answer 3"
                );

                mainWindowViewModel.ActivePack.Questions.Add(question);
                SelectedQuestion = question;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding question: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateNewPack(object? parameter)
        {
            try
            {
                var dialog = new New_Question_Pack();
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    var packName = dialog.PackName;

                    if (string.IsNullOrWhiteSpace(packName))
                    {
                        MessageBox.Show("Pack name cannot be empty.", "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Check if pack name already exists
                    if (mainWindowViewModel?.Packs.Any(p => p.Name.Equals(packName, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        MessageBox.Show("A pack with this name already exists.", "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Create new pack
                    var newPack = new QuestionPack(packName, dialog.SelectedDifficulty, dialog.TimeLimitInSeconds);
                    var packViewModel = new QuestionPackViewModel(newPack);

                    // Add to collection and save
                    if (mainWindowViewModel != null)
                    {
                        await mainWindowViewModel.AddPackAsync(packViewModel);
                    }

                    MessageBox.Show($"Pack '{packName}' created successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating pack: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectPack(object? parameter)
        {
            if (mainWindowViewModel?.Packs?.Count == 0)
            {
                MessageBox.Show("No question packs available to select.", "No Packs",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var dialog = new PackSelectionDialog(mainWindowViewModel!.Packs);
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                var result = dialog.ShowDialog();

                if (result == true && dialog.SelectedPack != null)
                {
                    mainWindowViewModel.ActivePack = dialog.SelectedPack;
                    MessageBox.Show($"Pack '{dialog.SelectedPack.Name}' selected successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting pack: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeletePack(object? parameter)
        {
            if (mainWindowViewModel?.ActivePack == null)
            {
                MessageBox.Show("No active pack to delete.", "No Active Pack",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the pack '{mainWindowViewModel.ActivePack.Name}'?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var packToDelete = mainWindowViewModel.ActivePack;

                    // Remove from collection and save
                    await mainWindowViewModel.RemovePackAsync(packToDelete);

                    MessageBox.Show($"Pack '{packToDelete.Name}' deleted successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting pack: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeletePack(object? parameter)
        {
            return mainWindowViewModel?.ActivePack != null && mainWindowViewModel.Packs.Count > 1;
        }

        private async void AddQuestion(object? parameter)
        {
            if (mainWindowViewModel?.ActivePack == null) return;

            try
            {
                var question = new Question(
                    NewQuestionText,
                    CorrectAnswer,
                    FirstIncorrectAnswer,
                    SecondIncorrectAnswer,
                    ThirdIncorrectAnswer
                );

                mainWindowViewModel.ActivePack.Questions.Add(question);

                // Clear the form
                NewQuestionText = string.Empty;
                CorrectAnswer = string.Empty;
                FirstIncorrectAnswer = string.Empty;
                SecondIncorrectAnswer = string.Empty;
                ThirdIncorrectAnswer = string.Empty;

                // Auto-save is handled by the collection change event
                MessageBox.Show("Question added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding question: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddQuestion(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(NewQuestionText) &&
                   !string.IsNullOrWhiteSpace(CorrectAnswer) &&
                   !string.IsNullOrWhiteSpace(FirstIncorrectAnswer) &&
                   !string.IsNullOrWhiteSpace(SecondIncorrectAnswer) &&
                   !string.IsNullOrWhiteSpace(ThirdIncorrectAnswer) &&
                   mainWindowViewModel?.ActivePack != null;
        }

        private void DeleteQuestion(object? parameter)
        {
            Question? questionToDelete = null;

            if (parameter is Question paramQuestion)
            {
                questionToDelete = paramQuestion;
            }
            else if (SelectedQuestion != null)
            {
                questionToDelete = SelectedQuestion;
            }

            if (questionToDelete == null || mainWindowViewModel?.ActivePack == null) return;

            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this question?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    mainWindowViewModel.ActivePack.Questions.Remove(questionToDelete);

                    if (SelectedQuestion == questionToDelete)
                    {
                        SelectedQuestion = null;
                    }

                    MessageBox.Show("Question deleted successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting question: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteQuestion(object? parameter)
        {
            if (parameter is Question)
            {
                return mainWindowViewModel?.ActivePack != null;
            }
            return SelectedQuestion != null && mainWindowViewModel?.ActivePack != null;
        }

        private void LoadDefaultPack(object? parameter)
        {
            try
            {
                if (mainWindowViewModel == null) return;

                // Use the MainWindowViewModel's method instead of duplicating code
                mainWindowViewModel.LoadDefaultPack();

                MessageBox.Show("Default question pack loaded successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading default pack: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add these helper methods right after the SelectedQuestion property
        private void LoadQuestionForEditing(Question question)
        {
            IsEditingQuestion = true;
            NewQuestionText = question.Query;
            CorrectAnswer = question.CorrectAnswer;

            if (question.IncorrectAnswers?.Length >= 3)
            {
                FirstIncorrectAnswer = question.IncorrectAnswers[0];
                SecondIncorrectAnswer = question.IncorrectAnswers[1];
                ThirdIncorrectAnswer = question.IncorrectAnswers[2];
            }

            // Notify that UpdateQuestionCommand can execute changed
            UpdateQuestionCommand.RaiseCanExecuteChanged();
        }

        private void ClearForm()
        {
            IsEditingQuestion = false;
            NewQuestionText = string.Empty;
            CorrectAnswer = string.Empty;
            FirstIncorrectAnswer = string.Empty;
            SecondIncorrectAnswer = string.Empty;
            ThirdIncorrectAnswer = string.Empty;
            UpdateQuestionCommand.RaiseCanExecuteChanged();
        }
        private async void UpdateQuestion(object? parameter)
        {
            if (SelectedQuestion == null || mainWindowViewModel?.ActivePack == null) return;

            try
            {
                // Update the existing question
                SelectedQuestion.Query = NewQuestionText;
                SelectedQuestion.CorrectAnswer = CorrectAnswer;
                SelectedQuestion.IncorrectAnswers = new[] { FirstIncorrectAnswer, SecondIncorrectAnswer, ThirdIncorrectAnswer };

                // Clear the form and exit edit mode
                ClearForm();
                SelectedQuestion = null;

                // Auto-save is handled by property change events
                MessageBox.Show("Question updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating question: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEdit(object? parameter)
        {
            ClearForm();
            SelectedQuestion = null;
        }
        private void EditPack(object? parameter)
        {
            if (mainWindowViewModel?.ActivePack == null)
            {
                MessageBox.Show("No active pack to edit.", "No Active Pack",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dialog = new PackOptionDialog();
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                // Pre-populate with current values
                dialog.PackName.Text = mainWindowViewModel.ActivePack.Name;
                SetDifficultyInDialog(dialog, mainWindowViewModel.ActivePack.Difficulty);
                dialog.TimeLimitSlider.Value = mainWindowViewModel.ActivePack.TimeLimitInSeconds;

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    var newPackName = dialog.PackName.Text;

                    if (string.IsNullOrWhiteSpace(newPackName))
                    {
                        MessageBox.Show("Pack name cannot be empty.", "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Update pack properties
                    mainWindowViewModel.ActivePack.Name = newPackName;
                    mainWindowViewModel.ActivePack.Difficulty = GetDifficultyFromDialog(dialog);
                    mainWindowViewModel.ActivePack.TimeLimitInSeconds = (int)dialog.TimeLimitSlider.Value;

                    MessageBox.Show($"Pack '{newPackName}' updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing pack: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditPack(object? parameter)
        {
            return mainWindowViewModel?.ActivePack != null;
        }

        private Difficulty GetDifficultyFromDialog(PackOptionDialog dialog)
        {
            var comboBox = dialog.FindName("DifficultyComboBox") as ComboBox;
            if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString() switch
                {
                    "Easy" => Difficulty.Easy,
                    "Medium" => Difficulty.Medium,
                    "Difficult" => Difficulty.Hard,
                    _ => Difficulty.Medium
                };
            }
            return Difficulty.Medium;
        }

        private void SetDifficultyInDialog(PackOptionDialog dialog, Difficulty difficulty)
        {
            var comboBox = dialog.FindName("DifficultyComboBox") as ComboBox;
            if (comboBox != null)
            {
                var index = difficulty switch
                {
                    Difficulty.Easy => 0,
                    Difficulty.Medium => 1,
                    Difficulty.Hard => 2,
                    _ => 1
                };
                comboBox.SelectedIndex = index;
            }
        }
        private bool CanUpdateQuestion(object? parameter)
        {
            return IsEditingQuestion &&
                   SelectedQuestion != null &&
                   !string.IsNullOrWhiteSpace(NewQuestionText) &&
                   !string.IsNullOrWhiteSpace(CorrectAnswer) &&
                   !string.IsNullOrWhiteSpace(FirstIncorrectAnswer) &&
                   !string.IsNullOrWhiteSpace(SecondIncorrectAnswer) &&
                   !string.IsNullOrWhiteSpace(ThirdIncorrectAnswer) &&
                   mainWindowViewModel?.ActivePack != null;
        }
        #endregion
    }
}