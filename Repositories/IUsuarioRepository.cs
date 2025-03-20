using Alunos.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alunos.API.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetByIdAsync(Guid id);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(Guid id);
        Task<Usuario> GetByCpfAsync(string cpf);
    }
}