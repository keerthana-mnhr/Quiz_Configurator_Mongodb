using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Quiz_Configurator.Models;

namespace Quiz_Configurator.Services
{
    public class QuizConfiguratorDbContext:DbContext
    {
        public DbSet<QuestionPack> QuestionPacks => Set<QuestionPack>();

        public QuizConfiguratorDbContext(DbContextOptions<QuizConfiguratorDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuestionPack>().ToCollection("QuestionPack");
        }
    }
}
