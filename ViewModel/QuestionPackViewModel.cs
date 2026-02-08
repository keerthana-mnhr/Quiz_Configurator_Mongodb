using Quiz_Configurator.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Quiz_Configurator.ViewModel
{
    public class QuestionPackViewModel : ViewModelBase
    {
        private readonly QuestionPack _model;

        public QuestionPackViewModel(QuestionPack model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            Questions = new ObservableCollection<Question>(_model.Questions);
            Categories = new ObservableCollection<Category>();
            Questions.CollectionChanged += OnQuestionsCollectionChanged;

            foreach (var question in Questions)
            {
                if (question is INotifyPropertyChanged notifyQuestion)
                    notifyQuestion.PropertyChanged += OnQuestionPropertyChanged;
            }
        }

        public string Name
        {
            get => _model.Name;
            set
            {
                if (_model.Name != value)
                {
                    _model.Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Difficulty Difficulty
        {
            get => _model.Difficulty;
            set
            {
                if (_model.Difficulty != value)
                {
                    _model.Difficulty = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int TimeLimitInSeconds
        {
            get => _model.TimeLimitInSeconds;
            set
            {
                if (_model.TimeLimitInSeconds != value)
                {
                    _model.TimeLimitInSeconds = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<Question> Questions { get; }

        public ObservableCollection<Category> Categories { get; set; }

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    _model.CategoryId = value?.Id ?? string.Empty;
                    _model.Category = value;
                    RaisePropertyChanged();
                }
            }
        }

        public QuestionPack GetQuestionPack() => _model;

        private void OnQuestionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _model.Questions.Clear();
            foreach (var question in Questions)
                _model.Questions.Add(question);

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is INotifyPropertyChanged notifyQuestion)
                        notifyQuestion.PropertyChanged += OnQuestionPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyPropertyChanged notifyQuestion)
                        notifyQuestion.PropertyChanged -= OnQuestionPropertyChanged;
                }
            }

            RaisePropertyChanged(nameof(Questions));
        }

        private void OnQuestionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Questions));
        }
        // Add this method to help initialize SelectedCategory when loading existing packs
        public void InitializeSelectedCategoryFromId()
        {
            if (!string.IsNullOrEmpty(_model.CategoryId))
            {
                _selectedCategory = Categories.FirstOrDefault(c => c.Id == _model.CategoryId);
                RaisePropertyChanged(nameof(SelectedCategory));
            }
        }
    }
}