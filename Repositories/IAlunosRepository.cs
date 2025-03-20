using Alunos.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alunos.API.Repositories
{
    public interface IAlunoRepository
    {
        Task<Aluno> GetByIdAsync(Guid id);
        Task<IEnumerable<Aluno>> GetAllAsync();
        Task AddAsync(Aluno aluno);
        Task UpdateAsync(Aluno aluno);
        Task DeleteAsync(Guid id);
    }
}