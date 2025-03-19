using Alunos.API.Data;
using Alunos.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alunos.API.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UsuarioRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Usuario> GetByIdAsync(Guid id)
        {
            return await _dbContext.Usuarios.FindAsync(id);
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _dbContext.Usuarios.ToListAsync();
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _dbContext.Usuarios.AddAsync(usuario);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _dbContext.Usuarios.Update(usuario);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var usuarioToDelete = await _dbContext.Usuarios.FindAsync(id);
            if (usuarioToDelete != null)
            {
                _dbContext.Usuarios.Remove(usuarioToDelete);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Usuario> GetByCpfAsync(string cpf)
        {
            return await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Cpf == cpf);
        }
    }
}