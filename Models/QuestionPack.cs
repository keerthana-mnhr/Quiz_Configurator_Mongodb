using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_Configurator.Models
{
    public enum Difficulty { Easy, Medium, Hard }
    public class QuestionPack
    {
        public QuestionPack()
        {
            Name = string.Empty;
            Difficulty = Difficulty.Medium;
            TimeLimitInSeconds = 30;
            Questions = new List<Question>();
        }

        public QuestionPack(string name, Difficulty difficulty = Difficulty.Medium, int timeLimitInSeconds = 30)
        {
            Name = name ?? string.Empty;
            Difficulty = difficulty;
            TimeLimitInSeconds = timeLimitInSeconds;
            Questions = new List<Question>();
        }

        public string Name { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; }
        public int TimeLimitInSeconds { get; set; }
        public List<Question> Questions { get; set; } = new();
    }
}