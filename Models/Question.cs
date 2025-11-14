using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quiz_Configurator.Models
{
    public class Question : INotifyPropertyChanged
    {
        private string _query = string.Empty;
        private string _correctAnswer = string.Empty;
        private string[] _incorrectAnswers = new string[3];

        public Question()
        {
            _query = string.Empty;
            _correctAnswer = string.Empty;
            _incorrectAnswers = new[] { string.Empty, string.Empty, string.Empty };
        }

        public Question(string query, string correctAnswer,
            string incorrectAnswer1, string incorrectAnswer2, string incorrectAnswer3)
        {
            _query = query ?? string.Empty;
            _correctAnswer = correctAnswer ?? string.Empty;
            _incorrectAnswers = new[]
            {
                    incorrectAnswer1 ?? string.Empty,
                    incorrectAnswer2 ?? string.Empty,
                    incorrectAnswer3 ?? string.Empty
                };
        }

        public string Query
        {
            get => _query;
            set => SetProperty(ref _query, value ?? string.Empty);
        }

        public string CorrectAnswer
        {
            get => _correctAnswer;
            set => SetProperty(ref _correctAnswer, value ?? string.Empty);
        }

        public string[] IncorrectAnswers
        {
            get => _incorrectAnswers;
            set => SetProperty(ref _incorrectAnswers, value ?? new[] { string.Empty, string.Empty, string.Empty });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}