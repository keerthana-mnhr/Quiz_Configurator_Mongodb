using System.Configuration;
using System.Data;
using System.Windows;
using Quiz_Configurator.Dialogs;
using Quiz_Configurator.Services;
using Microsoft.EntityFrameworkCore;

namespace Quiz_Configurator
{
   
    public partial class App : Application
    {

        public static DataService MongoDBDataService { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var options = new DbContextOptionsBuilder<QuizConfiguratorDbContext>().UseMongoDB("mongodb://localhost:27017/", "KeerthanaManoharan").Options;

            var context = new QuizConfiguratorDbContext(options);
            MongoDBDataService = new DataService(context);
        }
    }

}
