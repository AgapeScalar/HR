using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public CandidateRepository()
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "App_Data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "candidates.json");
            if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]");
        }

        public Task<List<Candidate>> GetAllAsync()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                var list = JsonSerializer.Deserialize<List<Candidate>>(json) ?? new List<Candidate>();
                return Task.FromResult(list.OrderByDescending(x => x.UploadedAt).ToList());
            }
        }

        public Task<Candidate?> GetByIdAsync(string id)
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                var list = JsonSerializer.Deserialize<List<Candidate>>(json) ?? new List<Candidate>();
                var c = list.FirstOrDefault(x => x.Id == id);
                return Task.FromResult(c);
            }
        }

        public Task SaveCandidateAsync(Candidate candidate)
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                var list = JsonSerializer.Deserialize<List<Candidate>>(json) ?? new List<Candidate>();
                list.Add(candidate);
                var newJson = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, newJson);
                return Task.CompletedTask;
            }
        }

        public Task UpdateCandidateAsync(Candidate candidate)
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                var list = JsonSerializer.Deserialize<List<Candidate>>(json) ?? new List<Candidate>();
                var idx = list.FindIndex(x => x.Id == candidate.Id);
                if (idx >= 0) list[idx] = candidate; else list.Add(candidate);
                var newJson = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, newJson);
                return Task.CompletedTask;
            }
        }
    }
}
