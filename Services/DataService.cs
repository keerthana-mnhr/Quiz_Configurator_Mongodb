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
    public class DataService
    {
        public QuizConfiguratorDbContext _dbContext;

       public DataService(QuizConfiguratorDbContext context)
        {
            _dbContext = context;
        }


        public async Task SavePackAsync(QuestionPack pack)
        {

            AddOrUpdatePack(pack);
                await _dbContext.SaveChangesAsync();
        }

        private void AddOrUpdatePack(QuestionPack pack)
        {
            if (string.IsNullOrWhiteSpace(pack.Id))
            {
                _dbContext.QuestionPacks.Add(pack);
            }
            else
            {
                _dbContext.QuestionPacks.Update(pack);
            }
        }

        public async Task SavePacksAsync(IEnumerable<QuestionPack> packs)
        {
            foreach(var pk in packs)
            {
                AddOrUpdatePack(pk);
            }
            //_dbContext.QuestionPacks.AddRange(packs);
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

        public void SavePack(QuestionPack pack)
        {

            AddOrUpdatePack(pack);
            _dbContext.SaveChanges();
        }



    }
}