using Quiz_Configurator.Command;
using Quiz_Configurator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace Quiz_Configurator.ViewModel
{
    internal class PlayerViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;
        private DispatcherTimer? _quizTimer;
        private List<Question>? _questions;
        private int _currentQuestionIndex;
        private int _timeRemaining;
        private int _correctAnswers;
        private bool _questionAnswered;

        public PlayerViewModel(MainWindowViewModel? mainWindowViewModel)
        {
            this._mainWindowViewModel = mainWindowViewModel;

           
            ShuffledAnswers = new ObservableCollection<AnswerOption>();

         
            StartQuizCommand = new DelegateCommand(StartQuiz, CanStartQuiz);
            AnswerQuestionCommand = new DelegateCommand(AnswerQuestion);
            NextQuestionCommand = new DelegateCommand(NextQuestion, CanGoToNextQuestion);
            EndQuizCommand = new DelegateCommand(EndQuiz);
            SetPackNameCommand = new DelegateCommand(SetPackName, CanSetPackName);

           
            DemoText = string.Empty;
        }

      
        public QuestionPackViewModel? ActivePack => _mainWindowViewModel?.ActivePack;

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                RaisePropertyChanged();
            }
        }

        private Question? _currentQuestion;
        public Question? CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                _currentQuestion = value;
                RaisePropertyChanged();
                if (value != null)
                {
                    ShuffleAnswers();
                }
            }
        }

        private int _totalQuestions;
        public int TotalQuestions
        {
            get => _totalQuestions;
            set
            {
                _totalQuestions = value;
                RaisePropertyChanged();
            }
        }

        private int _currentQuestionNumber;
        public int CurrentQuestionNumber
        {
            get => _currentQuestionNumber;
            set
            {
                _currentQuestionNumber = value;
                RaisePropertyChanged();
            }
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                RaisePropertyChanged();
            }
        }

        public int CorrectAnswers
        {
            get => _correctAnswers;
            set
            {
                _correctAnswers = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<AnswerOption> ShuffledAnswers { get; }

        private string _resultMessage = string.Empty;
        public string ResultMessage
        {
            get => _resultMessage;
            set
            {
                _resultMessage = value;
                RaisePropertyChanged();
            }
        }

        private string _demoText;
        public string DemoText
        {
            get => _demoText;
            set
            {
                _demoText = value;
                RaisePropertyChanged();
                SetPackNameCommand.RaiseCanExecuteChanged();
            }
        }
       

       
        public DelegateCommand StartQuizCommand { get; }
        public DelegateCommand AnswerQuestionCommand { get; }
        public DelegateCommand NextQuestionCommand { get; }
        public DelegateCommand EndQuizCommand { get; }
        public DelegateCommand SetPackNameCommand { get; }
      

        private void StartQuiz(object? parameter)
        {
            if (ActivePack?.Questions?.Count == 0)
            {
                MessageBox.Show("No questions available in the current pack.", "No Questions",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
               
                _questions = ActivePack?.Questions?.ToList();
                TotalQuestions = _questions?.Count ?? 0;
                _currentQuestionIndex = 0;
                CurrentQuestionNumber = 1;
                CorrectAnswers = 0;
                IsPlaying = true;
                _questionAnswered = false;

               
                LoadCurrentQuestion();
                StartTimer();

                ResultMessage = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting quiz: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanStartQuiz(object? parameter)
        {
            return ActivePack?.Questions?.Count > 0;
        }

        private void AnswerQuestion(object? parameter)
        {
            if (parameter is AnswerOption selectedAnswer && !_questionAnswered)
            {
                _questionAnswered = true;
                StopTimer();

               
                foreach (var answer in ShuffledAnswers)
                {
                    if (answer.Text == CurrentQuestion?.CorrectAnswer)
                    {
                        answer.IsCorrect = true;
                        answer.BackgroundColor = "LightGreen";
                    }
                    else if (answer == selectedAnswer)
                    {
                        answer.IsIncorrect = true;
                        answer.BackgroundColor = "LightCoral";
                    }
                    else
                    {
                        answer.BackgroundColor = "LightGray";
                    }
                }

               
                if (selectedAnswer.Text == CurrentQuestion?.CorrectAnswer)
                {
                    CorrectAnswers++;
                }

                var delayTimer = new DispatcherTimer();
                delayTimer.Interval = TimeSpan.FromSeconds(2);
                delayTimer.Tick += (s, e) =>
                {
                    delayTimer.Stop();
                    NextQuestionCommand.RaiseCanExecuteChanged();
                };
                delayTimer.Start();
            }
        }

        private void NextQuestion(object? parameter)
        {
            _currentQuestionIndex++;
            CurrentQuestionNumber++;
            _questionAnswered = false;

            if (_currentQuestionIndex >= TotalQuestions)
            {
                EndQuiz(null);
                return;
            }

            LoadCurrentQuestion();
            StartTimer();
        }

        private bool CanGoToNextQuestion(object? parameter)
        {
            return _questionAnswered;
        }

        private void EndQuiz(object? parameter)
        {
            StopTimer();
            IsPlaying = false;

            double percentage = TotalQuestions > 0 ? (double)CorrectAnswers / TotalQuestions * 100 : 0;
            ResultMessage = $"Quiz Complete!\nScore: {CorrectAnswers}/{TotalQuestions} ({percentage:F1}%)";

            MessageBox.Show(ResultMessage, "Quiz Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanSetPackName(object? arg)
        {
            return !string.IsNullOrEmpty(DemoText);
        }

        private void SetPackName(object? obj)
        {
            if (ActivePack != null)
            {
                ActivePack.Name = DemoText;
            }
        }

        private void LoadCurrentQuestion()
        {
            if (_questions != null && _currentQuestionIndex < _questions.Count)
            {
                CurrentQuestion = _questions[_currentQuestionIndex];
                TimeRemaining = ActivePack?.TimeLimitInSeconds ?? 30;
            }
        }

        private void ShuffleAnswers()
        {
            ShuffledAnswers.Clear();

            if (CurrentQuestion != null)
            {
                var allAnswers = new List<string> { CurrentQuestion.CorrectAnswer };
                allAnswers.AddRange(CurrentQuestion.IncorrectAnswers);

               
                var random = new Random();
                var shuffled = allAnswers.OrderBy(x => random.Next()).ToList();

                foreach (var answer in shuffled)
                {
                    ShuffledAnswers.Add(new AnswerOption
                    {
                        Text = answer,
                        BackgroundColor = "White",
                        IsCorrect = false,
                        IsIncorrect = false
                    });
                }
            }
        }

        private void StartTimer()
        {
            _quizTimer = new DispatcherTimer();
            _quizTimer.Interval = TimeSpan.FromSeconds(1);
            _quizTimer.Tick += Timer_Tick;
            _quizTimer.Start();
        }

        private void StopTimer()
        {
            _quizTimer?.Stop();
            _quizTimer = null;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TimeRemaining--;

            if (TimeRemaining <= 0)
            {
              
                if (!_questionAnswered)
                {
                    _questionAnswered = true;
                    StopTimer();

                   
                    foreach (var answer in ShuffledAnswers)
                    {
                        if (answer.Text == CurrentQuestion?.CorrectAnswer)
                        {
                            answer.IsCorrect = true;
                            answer.BackgroundColor = "LightGreen";
                        }
                        else
                        {
                            answer.BackgroundColor = "LightGray";
                        }
                    }

                   
                    var delayTimer = new DispatcherTimer();
                    delayTimer.Interval = TimeSpan.FromSeconds(2);
                    delayTimer.Tick += (s, e) =>
                    {
                        delayTimer.Stop();
                        NextQuestion(null);
                    };
                    delayTimer.Start();
                }
            }
        }
    }

   
    public class AnswerOption : ViewModelBase
    {
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                RaisePropertyChanged();
            }
        }

        private string _backgroundColor = "White";
        public string BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                RaisePropertyChanged();
            }
        }

        private bool _isCorrect;
        public bool IsCorrect
        {
            get => _isCorrect;
            set
            {
                _isCorrect = value;
                RaisePropertyChanged();
            }
        }

        private bool _isIncorrect;
        public bool IsIncorrect
        {
            get => _isIncorrect;
            set
            {
                _isIncorrect = value;
                RaisePropertyChanged();
            }
        }
    }
}