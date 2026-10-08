using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication3.Models;
using System.Collections.Generic;

namespace WebApplication3.Services
{
    public class LocalAnalysisService : IAnalysisService
    {
        private readonly string[] _requiredKeywords = new[] { "asp.net", "asp.net core", "c#", "sql", "entity framework", "azure", "aws" };
        private readonly string[] _skillKeywords = new[] { "docker", "kubernetes", "postgres", "mysql", "html", "css", "javascript", "react", "angular" };

        public Task<AnalysisResult> AnalyzeAsync(string text)
        {
            var lower = (text ?? string.Empty).ToLowerInvariant();
            var result = new AnalysisResult();

            int matched = 0;
            foreach (var k in _requiredKeywords)
            {
                if (lower.Contains(k)) matched++;
            }
            result.MatchedRequirements = matched;
            result.TotalRequirements = _requiredKeywords.Length;

            var skills = new List<string>();
            foreach (var s in _skillKeywords)
            {
                if (lower.Contains(s)) skills.Add(s);
            }
            foreach (var k in _requiredKeywords)
            {
                if (lower.Contains(k) && !skills.Contains(k)) skills.Add(k);
            }
            result.Skills = skills.Distinct().ToList();

            double reqScore = (double)result.MatchedRequirements / Math.Max(1, result.TotalRequirements);
            double skillScore = Math.Min(1.0, result.Skills.Count / 5.0);
            result.Score = (int)Math.Round((reqScore * 0.7 + skillScore * 0.3) * 100);

            result.Summary = $"Matched {result.MatchedRequirements}/{result.TotalRequirements} required items. Detected skills: {string.Join(", ", result.Skills)}";

            return Task.FromResult(result);
        }
    }
}
