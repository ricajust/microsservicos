using Alunos.API.DTOs;
using Alunos.API.Models;
using Alunos.API.Repositories;
using Alunos.API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Text.Json;

namespace Alunos.API.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;
        private readonly Conversor _conversor;// Instancia a classe de conversão (Utils)
        private readonly IConnectionFactory _connectionFactory;

        public AlunoService(IAlunoRepository alunoRepository, IConnectionFactory connectionFactory)
        {
            _alunoRepository = alunoRepository;
            _conversor = new Conversor();
            _connectionFactory = connectionFactory;
        }

        // public async Task<AlunoResponseDTO> CreateAsync(AlunoDTO alunoDTO)
        // {
        //     var aluno = _conversor.ConverterAlunoDTOEmEntidade(alunoDTO);
        //     await _alunoRepository.AddAsync(aluno);

        //     return _conversor.ConverterAlunoEmResponseDTO(aluno);
        // }
        public async Task<AlunoResponseDTO> CreateAsync(AlunoDTO alunoDTO)
        {
            var aluno = _conversor.ConverterAlunoDTOEmEntidade(alunoDTO);
            await _alunoRepository.AddAsync(aluno);

            var responseDTO = _conversor.ConverterAlunoEmResponseDTO(aluno);

            // Publicar o evento AlunoCriadoNoMicrosservicoEvent
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "alunos.reverse.exchange", type: ExchangeType.Fanout);

                var evento = new AlunoCriadoNoMicrosservicoEvent
                {
                    Id = aluno.Id,
                    Nome = aluno.Nome,
                    Cpf = aluno.Cpf,
                    DataNascimento = aluno.DataNascimento ?? DateTime.MinValue,
                    Email = aluno.Email,
                    Telefone = aluno.Telefone,
                    Endereco = aluno.Endereco,
                    Bairro = aluno.Bairro,
                    Cidade = aluno.Cidade,
                    Uf = aluno.Uf,
                    Cep = aluno.Cep
                };

                var body = JsonSerializer.SerializeToUtf8Bytes(evento);

                channel.BasicPublish(exchange: "alunos.reverse.exchange",
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);
            }

            return responseDTO;
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

        // public async Task<Aluno> UpdateAsync(Guid id, AlunoDTO alunoDTO)
        // {
        //     var aluno = await _alunoRepository.GetByIdAsync(id);

        //     if (aluno == null)
        //         return null; // Retorna null se o aluno não for encontrado

        //     // Atualiza os campos do aluno
        //     aluno.Nome = alunoDTO.Nome;
        //     aluno.Cpf = alunoDTO.Cpf;
        //     aluno.DataNascimento = alunoDTO.DataNascimento;
        //     aluno.Email = alunoDTO.Email;
        //     aluno.Telefone = alunoDTO.Telefone;
        //     aluno.Endereco = alunoDTO.Endereco;
        //     aluno.Bairro = alunoDTO.Bairro;
        //     aluno.Cidade = alunoDTO.Cidade;
        //     aluno.Uf = alunoDTO.Uf;
        //     aluno.Cep = alunoDTO.Cep;

        //     // Se quiser permitir a atualização de senha:
        //     if (!string.IsNullOrEmpty(alunoDTO.Senha))
        //         aluno.Senha = alunoDTO.Senha; // Faça hash se necessário

        //     _alunoRepository.UpdateAsync(aluno);

        //     return aluno; // Retorna o aluno atualizado
        // }

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

            // Para permitir a atualização de senha:
            if (!string.IsNullOrEmpty(alunoDTO.Senha))
                aluno.Senha = alunoDTO.Senha; // Faça hash se necessário

            _alunoRepository.UpdateAsync(aluno);

            // Publicar o evento AlunoAtualizadoNoMicrosservicoEvent
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "alunos.reverse.exchange", type: ExchangeType.Fanout);

                var evento = new AlunoAtualizadoNoMicrosservicoEvent
                {
                    Id = aluno.Id,
                    Nome = aluno.Nome,
                    Cpf = aluno.Cpf,
                    DataNascimento = aluno.DataNascimento ?? DateTime.MinValue,
                    Email = aluno.Email,
                    Telefone = aluno.Telefone,
                    Endereco = aluno.Endereco,
                    Bairro = aluno.Bairro,
                    Cidade = aluno.Cidade,
                    Uf = aluno.Uf,
                    Cep = aluno.Cep
                };

                var body = JsonSerializer.SerializeToUtf8Bytes(evento);

                channel.BasicPublish(exchange: "alunos.reverse.exchange",
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);
            }

            return aluno; // Retorna o aluno atualizado
        }

        // public async Task<bool> DeleteAsync(Guid id)
        // {
        //     var aluno = await _alunoRepository.GetByIdAsync(id);

        //     if (aluno == null)
        //         return false;

        //     _alunoRepository.DeleteAsync(id);

        //     return true;
        // }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var aluno = await _alunoRepository.GetByIdAsync(id);

            if (aluno == null)
                return false;

            _alunoRepository.DeleteAsync(id);

            // Publicar o evento AlunoExcluidoNoMicrosservicoEvent
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "alunos.reverse.exchange", type: ExchangeType.Fanout);

                var evento = new AlunoExcluidoNoMicrosservicoEvent
                {
                    Id = id // Usamos o ID do aluno excluído
                };

                var body = JsonSerializer.SerializeToUtf8Bytes(evento);

                channel.BasicPublish(exchange: "alunos.reverse.exchange",
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);
            }

            return true;
        }

    }
}