using Alunos.API.DTOs;
using Alunos.API.Models;
using Alunos.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alunos.API.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
        }

        public async Task<AlunoDTO> CreateAsync(AlunoDTO alunoDTO)
        {
            var aluno = new Aluno
            {
                Id = alunoDTO.Id == Guid.Empty ? Guid.NewGuid() : alunoDTO.Id,
                Usuario = new Usuario
                {
                    Id = alunoDTO.Id == Guid.Empty ? Guid.NewGuid() : alunoDTO.Id,
                    Nome = alunoDTO.Nome,
                    Cpf = alunoDTO.Cpf,
                    DataNascimento = alunoDTO.DataNascimento,
                    Email = alunoDTO.Email,
                    Telefone = alunoDTO.Telefone,
                    Endereco = alunoDTO.Endereco,
                    Bairro = alunoDTO.Bairro,
                    Cidade = alunoDTO.Cidade,
                    Uf = alunoDTO.Uf,
                    Cep = alunoDTO.Cep,
                    Senha = alunoDTO.Senha
                }
            };

            await _alunoRepository.AddAsync(aluno);
            return ConvertToDto(aluno);
        }

        public async Task<AlunoDTO> GetByIdAsync(Guid id)
        {
            var aluno = await _alunoRepository.GetByIdAsync(id);
            return aluno == null ? null : ConvertToDto(aluno);
        }

        public async Task<IEnumerable<AlunoDTO>> GetAllAsync()
        {
            var alunos = await _alunoRepository.GetAllAsync();
            return alunos.Select(ConvertToDto);
        }

        public async Task<AlunoDTO> UpdateAsync(Guid id, AlunoDTO alunoDTO)
        {
            var existingAluno = await _alunoRepository.GetByIdAsync(id);
            if (existingAluno == null)
            {
                return null;
            }

            existingAluno.Usuario.Nome = alunoDTO.Nome;
            existingAluno.Usuario.Cpf = alunoDTO.Cpf;
            existingAluno.Usuario.DataNascimento = alunoDTO.DataNascimento;
            existingAluno.Usuario.Email = alunoDTO.Email;
            existingAluno.Usuario.Telefone = alunoDTO.Telefone;
            existingAluno.Usuario.Endereco = alunoDTO.Endereco;
            existingAluno.Usuario.Bairro = alunoDTO.Bairro;
            existingAluno.Usuario.Cidade = alunoDTO.Cidade;
            existingAluno.Usuario.Uf = alunoDTO.Uf;
            existingAluno.Usuario.Cep = alunoDTO.Cep;
            existingAluno.Usuario.Senha = alunoDTO.Senha;

            await _alunoRepository.UpdateAsync(existingAluno);
            return ConvertToDto(existingAluno);
        }

        public async Task<AlunoDTO> DeleteAsync(Guid id)
        {
            var alunoToDelete = await _alunoRepository.GetByIdAsync(id);
            if (alunoToDelete == null)
            {
                return null;
            }
            await _alunoRepository.DeleteAsync(id);
            return ConvertToDto(alunoToDelete);
        }

        private AlunoDTO ConvertToDto(Aluno aluno)
        {
            return new AlunoDTO
            {
                Id = aluno.Id,
                Nome = aluno.Usuario.Nome,
                Cpf = aluno.Usuario.Cpf,
                DataNascimento = aluno.Usuario.DataNascimento,
                Email = aluno.Usuario.Email,
                Telefone = aluno.Usuario.Telefone,
                Endereco = aluno.Usuario.Endereco,
                Bairro = aluno.Usuario.Bairro,
                Cidade = aluno.Usuario.Cidade,
                Uf = aluno.Usuario.Uf,
                Cep = aluno.Usuario.Cep,
                Senha = aluno.Usuario.Senha
            };
        }
    }
}