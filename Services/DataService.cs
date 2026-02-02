using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Quiz_Configurator.Models;

namespace Quiz_Configurator.Services
{
    internal class DataService
    {
        private QuizConfiguratorDbContext _dbContext;

       public DataService(QuizConfiguratorDbContext context)
        {
            _dbContext = context;
        }


        public async Task SavePackAsync(QuestionPack pack)
        {
            _dbContext.QuestionPacks.Add(pack);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SavePacksAsync(IEnumerable<QuestionPack> packs)
        {
            _dbContext.QuestionPacks.AddRange(packs);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<QuestionPack?> LoadPackAsync(string packName)
        {
            return await _dbContext.QuestionPacks.FirstOrDefaultAsync(p => p.Name == packName);
        }

        public async Task<List<QuestionPack>> LoadPacksAsync()
        {
            return await _dbContext.QuestionPacks.ToListAsync();
        }

        public async Task DeletePackAsync(string packName)
        {
            var pack = await _dbContext.QuestionPacks.FirstOrDefaultAsync(p => p.Name == packName);
            if(pack != null)
            {
                _dbContext.QuestionPacks.Remove(pack);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> PackFileExists(string packName)
        {
            return await _dbContext.QuestionPacks.AnyAsync(p => p.Name == packName);
        }

        

        public async Task DeleteAllPackFiles()
        {
            var packs = await _dbContext.QuestionPacks.ToListAsync();
            _dbContext.QuestionPacks.RemoveRange(packs);
            await _dbContext.SaveChangesAsync();
        }

       

        
    }
}