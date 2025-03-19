using Alunos.API.Data;
using Alunos.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alunos.API.Repositories
{
    public class AlunoRepository : IAlunoRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AlunoRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Aluno> GetByIdAsync(Guid id)
        {
            return await _dbContext.Alunos
                .Include(a => a.Usuario) // Inclui os dados do Usuario relacionados
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Aluno>> GetAllAsync()
        {
            return await _dbContext.Alunos
                .Include(a => a.Usuario) // Inclui os dados do Usuario relacionados
                .ToListAsync();
        }

        public async Task AddAsync(Aluno aluno)
        {
            await _dbContext.Alunos.AddAsync(aluno);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Aluno aluno)
        {
            _dbContext.Alunos.Update(aluno);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var alunoToDelete = await _dbContext.Alunos.FindAsync(id);
            if (alunoToDelete != null)
            {
                _dbContext.Alunos.Remove(alunoToDelete);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}