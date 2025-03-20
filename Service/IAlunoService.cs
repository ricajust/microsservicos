using Alunos.API.DTOs;
using Alunos.API.Models;

namespace Alunos.API.Services
{
    public interface IAlunoService
    {
        Task<AlunoResponseDTO> CreateAsync(AlunoDTO alunoDTO);
        Task<IEnumerable<AlunoResponseDTO>> GetAllAsync();
        Task<AlunoResponseDTO> GetByIdAsync(Guid id);
        Task<Aluno> UpdateAsync(Guid id, AlunoDTO alunoDTO);
        Task<bool> DeleteAsync(Guid id);
    }
}