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
            try
            {
                _dbContext.ChangeTracker.Clear();
                pack.Category = null;

                if (string.IsNullOrWhiteSpace(pack.Id))
                {
            
                    var existingByName = await _dbContext.QuestionPacks
                        .FirstOrDefaultAsync(p => p.Name == pack.Name);

                    if (existingByName != null)
                    {
                        throw new InvalidOperationException($"A pack with the name '{pack.Name}' already exists.");
                    }

                    pack.Id = null;
                    _dbContext.QuestionPacks.Add(pack);
                }
                else
                {
             

                    _dbContext.QuestionPacks.Update(pack);
                }

                await _dbContext.SaveChangesAsync();
                _dbContext.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("The pack was modified by another user. Please reload and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to save the pack: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        // Add new method specifically for adding questions to existing packs
        public async Task AddQuestionToPackAsync(string packId, Question question)
        {
            try
            {
                _dbContext.ChangeTracker.Clear();
                var pack = await _dbContext.QuestionPacks
                    .FirstOrDefaultAsync(p => p.Id == packId);

                if (pack == null)
                {
                    throw new InvalidOperationException("Question pack not found.");
                }

                // Add the new question to the pack
                pack.Questions.Add(question);

                // Clear navigation property
                pack.Category = null;

                _dbContext.QuestionPacks.Update(pack);
                await _dbContext.SaveChangesAsync();
                _dbContext.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add question to pack: {ex.Message}", ex);
            }
        }

        // Add method to update a specific question in a pack
        public async Task UpdateQuestionInPackAsync(string packId, Question oldQuestion, Question newQuestion)
        {
            try
            {
                // First, clear the change tracker to avoid tracking conflicts
                _dbContext.ChangeTracker.Clear();

                // Load the pack WITHOUT tracking
                var pack = await _dbContext.QuestionPacks
                    .FirstOrDefaultAsync(p => p.Id == packId);

                if (pack == null)
                {
                    throw new InvalidOperationException("Question pack not found.");
                }

                // Find and replace the question
                var questionIndex = pack.Questions.FindIndex(q =>
                    q.Query == oldQuestion.Query &&
                    q.CorrectAnswer == oldQuestion.CorrectAnswer);

                if (questionIndex >= 0)
                {
                    pack.Questions[questionIndex] = newQuestion;
                }
                else
                {
                    pack.Questions.Add(newQuestion);
                }

            
                pack.Category = null;

                _dbContext.QuestionPacks.Update(pack);
                await _dbContext.SaveChangesAsync();

             
                _dbContext.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update question in pack: {ex.Message}", ex);
            }
        }

        // Add method to remove a question from a pack
        public async Task RemoveQuestionFromPackAsync(string packId, Question question)
        {
            try
            {
                _dbContext.ChangeTracker.Clear();
                var pack = await _dbContext.QuestionPacks
                    .FirstOrDefaultAsync(p => p.Id == packId);

                if (pack == null)
                {
                    throw new InvalidOperationException("Question pack not found.");
                }

                // Remove the question from the pack
                var questionToRemove = pack.Questions.FirstOrDefault(q =>
                    q.Query == question.Query &&
                    q.CorrectAnswer == question.CorrectAnswer);

                if (questionToRemove != null)
                {
                    pack.Questions.Remove(questionToRemove);
                }

                // Clear navigation property
                pack.Category = null;


                _dbContext.QuestionPacks.Update(pack);
                await _dbContext.SaveChangesAsync();
                _dbContext.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove question from pack: {ex.Message}", ex);
            }
        }

        private void AddOrUpdatePack(QuestionPack pack)
        {


            pack.Category = null;

            if (string.IsNullOrWhiteSpace(pack.Id))
            {
                pack.Id = null;
                _dbContext.QuestionPacks.Add(pack);
            }
            else
            {
               _dbContext.QuestionPacks.Update(pack);
            }
        }

        public async Task SavePacksAsync(IEnumerable<QuestionPack> packs)
        {
            _dbContext.ChangeTracker.Clear();
            foreach (var pk in packs)
            {
                AddOrUpdatePack(pk);
            }
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();
        }

        public async Task<QuestionPack?> LoadPackAsync(string packName)
        {
            return await _dbContext.QuestionPacks.FirstOrDefaultAsync(p => p.Name == packName);
        }

        public async Task<List<QuestionPack>> LoadPacksAsync()
        {
            var questionPacks= await _dbContext.QuestionPacks.ToListAsync();
            return questionPacks;
        }

        public async Task DeletePackAsync(string packName)
        {
            _dbContext.ChangeTracker.Clear();
            var pack = await _dbContext.QuestionPacks.FirstOrDefaultAsync(p => p.Name == packName);
            if (pack != null)
            {
                _dbContext.QuestionPacks.Remove(pack);
                await _dbContext.SaveChangesAsync();
            }
            _dbContext.ChangeTracker.Clear();
        }

        public async Task<bool> PackFileExists(string packName)
        {
            return await _dbContext.QuestionPacks.AnyAsync(p => p.Name == packName);
        }

        public async Task DeleteAllPackFiles()
        {
            _dbContext.ChangeTracker.Clear();
            var packs = await _dbContext.QuestionPacks.ToListAsync();
            _dbContext.QuestionPacks.RemoveRange(packs);
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();
        }

        public void SavePack(QuestionPack pack)
        {
            _dbContext.ChangeTracker.Clear();
            AddOrUpdatePack(pack);
            _dbContext.SaveChanges();
            _dbContext.ChangeTracker.Clear();
        }

        public async Task<List<Category>> LoadCategoriesAsync()
        {
            return await _dbContext.Categories.ToListAsync();
        }

        public async Task SaveCategoryAsync(Category category)
        {
            try
            {
                _dbContext.ChangeTracker.Clear();
                if (string.IsNullOrWhiteSpace(category.Id))
                {
                    var existingCategory = await _dbContext.Categories
                        .FirstOrDefaultAsync(c => c.Name == category.Name);

                    if (existingCategory != null)
                    {
                        throw new InvalidOperationException($"A category with the name '{category.Name}' already exists.");
                    }

                    category.Id = null;

                    _dbContext.Categories.Add(category);
                }
                else
                {
                   _dbContext.Categories.Update(category);
                }

                await _dbContext.SaveChangesAsync();
                _dbContext.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("The category was modified by another user. Please reload and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to save the category: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }
    }
}