using Alunos.API.DTOs;
using Alunos.API.Models;
using Alunos.API.Repositories;
using Alunos.API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alunos.API.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;
        
        // Instancia a classe de conversão (Utils)
        private readonly Conversor _conversor;

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
            _conversor = new Conversor();
        }

        public async Task<AlunoResponseDTO> CreateAsync(AlunoDTO alunoDTO)
        {
            var aluno = _conversor.ConverterAlunoDTOEmEntidade(alunoDTO);
            await _alunoRepository.AddAsync(aluno);

            return _conversor.ConverterAlunoEmResponseDTO(aluno);
        }

        public async Task<IEnumerable<AlunoResponseDTO>> GetAllAsync()
        {
            var alunos = await _alunoRepository.GetAllAsync();

            // Mapeando entidades para ResponseDTOs
            return alunos.Select(aluno => new AlunoResponseDTO
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Cpf = aluno.Cpf,
                DataNascimento = aluno.DataNascimento,
                Email = aluno.Email,
                Telefone = aluno.Telefone,
                Endereco = aluno.Endereco,
                Bairro = aluno.Bairro,
                Cidade = aluno.Cidade,
                Uf = aluno.Uf,
                Cep = aluno.Cep
            });
        }

        public async Task<AlunoResponseDTO> GetByIdAsync(Guid id)
        {
            var aluno = await _alunoRepository.GetByIdAsync(id);

            if (aluno == null)
                return null;

            return new AlunoResponseDTO
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Cpf = aluno.Cpf,
                DataNascimento = aluno.DataNascimento,
                Email = aluno.Email,
                Telefone = aluno.Telefone,
                Endereco = aluno.Endereco,
                Bairro = aluno.Bairro,
                Cidade = aluno.Cidade,
                Uf = aluno.Uf,
                Cep = aluno.Cep
            };
        }

        public async Task<Aluno> UpdateAsync(Guid id, AlunoDTO alunoDTO)
        {
            var aluno = await _alunoRepository.GetByIdAsync(id);

            if (aluno == null)
                return null; // Retorna null se o aluno não for encontrado

            // Atualiza os campos do aluno
            aluno.Nome = alunoDTO.Nome;
            aluno.Cpf = alunoDTO.Cpf;
            aluno.DataNascimento = alunoDTO.DataNascimento;
            aluno.Email = alunoDTO.Email;
            aluno.Telefone = alunoDTO.Telefone;
            aluno.Endereco = alunoDTO.Endereco;
            aluno.Bairro = alunoDTO.Bairro;
            aluno.Cidade = alunoDTO.Cidade;
            aluno.Uf = alunoDTO.Uf;
            aluno.Cep = alunoDTO.Cep;

            // Se quiser permitir a atualização de senha:
            if (!string.IsNullOrEmpty(alunoDTO.Senha))
                aluno.Senha = alunoDTO.Senha; // Faça hash se necessário

            _alunoRepository.UpdateAsync(aluno);

            return aluno; // Retorna o aluno atualizado
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var aluno = await _alunoRepository.GetByIdAsync(id);

            if (aluno == null)
                return false;

            _alunoRepository.DeleteAsync(id);

            return true;
        }
        
    }
}