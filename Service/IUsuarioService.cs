using Alunos.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alunos.API.Services
{
    public interface IUsuarioService
    {
        Task<Usuario> GetByIdAsync(Guid id);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(Guid id);
        Task<Usuario> GetByCpfAsync(string cpf);
    }
}