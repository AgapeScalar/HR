using System;

namespace WebApplication3.Models
{
    public class Candidate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? ResumeKey { get; set; }

        public string? RawText { get; set; }

        public AnalysisResult? Analysis { get; set; }
    }
}
