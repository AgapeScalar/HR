using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public interface ICandidateRepository
    {
        Task<List<Candidate>> GetAllAsync();
        Task<Candidate?> GetByIdAsync(string id);
        Task SaveCandidateAsync(Candidate candidate);
        Task UpdateCandidateAsync(Candidate candidate);
    }
}
