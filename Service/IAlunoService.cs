using Alunos.API.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alunos.API.Services
{
    public interface IAlunoService
    {
        Task<AlunoDTO> CreateAsync(AlunoDTO alunoDTO);
        Task<AlunoDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<AlunoDTO>> GetAllAsync();
        Task<AlunoDTO> UpdateAsync(Guid id, AlunoDTO alunoDTO);
        Task<AlunoDTO> DeleteAsync(Guid id);
    }
}