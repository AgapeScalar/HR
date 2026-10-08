using System.Collections.Generic;

namespace WebApplication3.Models
{
    public class AnalysisResult
    {
        public int Score { get; set; }

        public int MatchedRequirements { get; set; }
        public int TotalRequirements { get; set; }

        public List<string> Skills { get; set; } = new List<string>();

        public string? Summary { get; set; }
    }
}
