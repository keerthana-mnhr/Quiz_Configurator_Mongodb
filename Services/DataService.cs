using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Quiz_Configurator.Models;

namespace Quiz_Configurator.Services
{
    internal static class DataService
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuizConfigurator");

        private static readonly string PacksFolder = Path.Combine(AppDataFolder, "Packs");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public static async Task SavePackAsync(QuestionPack pack)
        {
            try
            {
                Directory.CreateDirectory(PacksFolder);
                var safeFileName = GetSafeFileName(pack.Name);
                var filePath = Path.Combine(PacksFolder, $"{safeFileName}.json");
                var json = JsonSerializer.Serialize(pack, JsonOptions);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save question pack '{pack.Name}': {ex.Message}", ex);
            }
        }

        public static async Task SavePacksAsync(IEnumerable<QuestionPack> packs)
        {
            try
            {
                var tasks = packs.Select(pack => SavePackAsync(pack));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save question packs: {ex.Message}", ex);
            }
        }

        public static async Task<QuestionPack?> LoadPackAsync(string packName)
        {
            try
            {
                var safeFileName = GetSafeFileName(packName);
                var filePath = Path.Combine(PacksFolder, $"{safeFileName}.json");

                if (!File.Exists(filePath)) return null;

                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json)) return null;

                return JsonSerializer.Deserialize<QuestionPack>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load question pack '{packName}': {ex.Message}", ex);
            }
        }

        public static async Task<List<QuestionPack>> LoadPacksAsync()
        {
            try
            {
                if (!Directory.Exists(PacksFolder)) return new List<QuestionPack>();

                var packFiles = Directory.GetFiles(PacksFolder, "*.json");
                var loadTasks = packFiles.Select(LoadPackFromFileAsync);
                var results = await Task.WhenAll(loadTasks);

                return results.Where(pack => pack != null).ToList()!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load question packs: {ex.Message}", ex);
            }
        }

        public static async Task DeletePackAsync(string packName)
        {
            try
            {
                var safeFileName = GetSafeFileName(packName);
                var filePath = Path.Combine(PacksFolder, $"{safeFileName}.json");

                if (File.Exists(filePath))
                    File.Delete(filePath);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete question pack '{packName}': {ex.Message}", ex);
            }
        }

        public static bool PackFileExists(string packName)
        {
            var safeFileName = GetSafeFileName(packName);
            var filePath = Path.Combine(PacksFolder, $"{safeFileName}.json");
            return File.Exists(filePath);
        }

        public static string GetAppDataPath() => AppDataFolder;
        public static string GetPacksPath() => PacksFolder;

        public static void DeleteAllPackFiles()
        {
            try
            {
                if (!Directory.Exists(PacksFolder)) return;

                var files = Directory.GetFiles(PacksFolder, "*.json");
                Array.ForEach(files, File.Delete);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete pack files: {ex.Message}", ex);
            }
        }

        private static async Task<QuestionPack?> LoadPackFromFileAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json)) return null;
                return JsonSerializer.Deserialize<QuestionPack>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static string GetSafeFileName(string packName)
        {
            if (string.IsNullOrEmpty(packName)) return "UnnamedPack";

            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = new string(packName.Where(c => !invalidChars.Contains(c)).ToArray());
            safeName = Regex.Replace(safeName, @"\s+", "_");

            return safeName.Length > 50
                ? safeName[..50]
                : string.IsNullOrEmpty(safeName) ? "UnnamedPack" : safeName;
        }
    }
}