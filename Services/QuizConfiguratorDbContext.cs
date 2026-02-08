using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Quiz_Configurator.Models;

namespace Quiz_Configurator.Services
{
    public class QuizConfiguratorDbContext : DbContext
    {
        public DbSet<QuestionPack> QuestionPacks => Set<QuestionPack>();
        public DbSet<Category> Categories => Set<Category>();

        public QuizConfiguratorDbContext(DbContextOptions<QuizConfiguratorDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuestionPack>().ToCollection("questionPacks");
            modelBuilder.Entity<Category>().ToCollection("categories");

            modelBuilder.Entity<QuestionPack>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();


            modelBuilder.Entity<Category>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<QuestionPack>()
                .OwnsMany(qp => qp.Questions, q =>
                {
                    q.Property(question => question.Query);
                    q.Property(question => question.CorrectAnswer);
                    q.Property(question => question.IncorrectAnswers);
                });

            modelBuilder.Entity<QuestionPack>()
                .Ignore(e => e.Category);
        }
    }
}