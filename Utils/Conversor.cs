using System;
using Alunos.API.Models;
using Alunos.API.DTOs;

namespace Alunos.API.Utils
{
    public class Conversor
    {
        /// <summary>Converte uma entidade Aluno em um DTO AlunoDTO.</summary>
        public AlunoDTO ConverterAlunoEmDto(Aluno aluno)
        {
            return new AlunoDTO
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

        /// <summary>Converte um objeto AlunoDTO em uma Entidade.</summary>
        public Aluno ConverterAlunoDTOEmEntidade(AlunoDTO alunoDTO)
        {
            return new Aluno
            {
                Id = alunoDTO.Id,
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
            };
        }

        /// <summary>Converte uma entidade Aluno em um DTO AlunoResponseDTO.</summary>
        public AlunoResponseDTO ConverterAlunoEmResponseDTO(Aluno aluno)
        {
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
    }
}