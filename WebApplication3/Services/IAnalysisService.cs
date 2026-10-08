using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public interface IAnalysisService
    {
        Task<AnalysisResult> AnalyzeAsync(string text);
    }
}
