using Alunos.API.Models;
using Alunos.API.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alunos.API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Usuario> GetByIdAsync(Guid id)
        {
            return await _usuarioRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _usuarioRepository.GetAllAsync();
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _usuarioRepository.AddAsync(usuario);
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            await _usuarioRepository.UpdateAsync(usuario);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _usuarioRepository.DeleteAsync(id);
        }

        public async Task<Usuario> GetByCpfAsync(string cpf)
        {
            return await _usuarioRepository.GetByCpfAsync(cpf);
        }
    }
}