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
using Microsoft.Extensions.Logging;

namespace Alunos.API.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;
        private readonly Conversor _conversor;// Instancia a classe de conversão (Utils)
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<AlunoService> _logger;

        public AlunoService(IAlunoRepository alunoRepository, IConnectionFactory connectionFactory, ILogger<AlunoService> logger) // Modifique o construtor
        {
            _alunoRepository = alunoRepository;
            _conversor = new Conversor();
            _connectionFactory = connectionFactory;
            _logger = logger; // Inicialize o logger
        }

        public async Task<AlunoResponseDTO> CreateAsync(AlunoDTO alunoDTO)
        {
            _logger.LogInformation("Dados recebidos para criação: {@AlunoDTO}", alunoDTO);

            var aluno = _conversor.ConverterAlunoDTOEmEntidade(alunoDTO);
            await _alunoRepository.AddAsync(aluno);

            var responseDTO = _conversor.ConverterAlunoEmResponseDTO(aluno);

            if (string.IsNullOrWhiteSpace(aluno.Cpf))
            {
                _logger.LogError("CPF inválido, evento não será publicado");
                return responseDTO;
            }

            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "alunos.reverse.exchange",
                                      type: ExchangeType.Fanout,
                                      durable: true,
                                      autoDelete: true);

                var evento = new AlunoCriadoNoMicrosservicoEvent
                {
                    Id = aluno.Id,
                    Nome = aluno.Nome,
                    Cpf = aluno.Cpf,
                    DataNascimento = aluno.DataNascimento?.ToString("yyyy-MM-dd") ?? string.Empty,
                    // DataNascimento = aluno.DataNascimento ?? DateTime.MinValue,
                    Email = aluno.Email,
                    Telefone = aluno.Telefone,
                    Endereco = aluno.Endereco,
                    Bairro = aluno.Bairro,
                    Cidade = aluno.Cidade,
                    Uf = aluno.Uf,
                    Cep = aluno.Cep,
                    Senha = aluno.Senha,
                    Origem = "Microsservico",
                    EventType = "AlunoCriado"
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonEvento = JsonSerializer.Serialize(evento);
                _logger.LogInformation($"JSON do evento AlunoCriado a ser publicado: {jsonEvento}");

                var props = channel.CreateBasicProperties();
                props.ContentType = "application/json";
                props.Headers = new Dictionary<string, object> { ["origem"] = "Microsservico" };

                var body = JsonSerializer.SerializeToUtf8Bytes(evento, jsonOptions);
                _logger.LogInformation("Publicando evento: {Evento}", evento);

                channel.BasicPublish(exchange: "alunos.reverse.exchange",
                                    routingKey: "",
                                    basicProperties: props,
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

        public async Task<Aluno> UpdateAsync(Guid id, AlunoDTO alunoDTO)
        {
            var alunoExistente = await _alunoRepository.GetByIdAsync(id);

            if (alunoExistente == null)
                return null; // Retorna null se o aluno não for encontrado

            // Atualiza os campos do aluno (EXCLUINDO O CPF)
            alunoExistente.Nome = alunoDTO.Nome;
            alunoExistente.DataNascimento = alunoDTO.DataNascimento;
            alunoExistente.Email = alunoDTO.Email;
            alunoExistente.Telefone = alunoDTO.Telefone;
            alunoExistente.Endereco = alunoDTO.Endereco;
            alunoExistente.Bairro = alunoDTO.Bairro;
            alunoExistente.Cidade = alunoDTO.Cidade;
            alunoExistente.Uf = alunoDTO.Uf;
            alunoExistente.Cep = alunoDTO.Cep;

            // Para permitir a atualização de senha:
            if (!string.IsNullOrEmpty(alunoDTO.Senha))
                alunoExistente.Senha = alunoDTO.Senha; // Faça hash se necessário

            _alunoRepository.UpdateAsync(alunoExistente);

            // Publicar o evento AlunoAtualizadoNoMicrosservicoEvent
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "alunos.reverse.exchange", type: ExchangeType.Fanout, durable: true, autoDelete: true);

                var evento = new AlunoAtualizadoNoMicrosservicoEvent
                {
                    Id = alunoExistente.Id,
                    Nome = alunoExistente.Nome,
                    Cpf = alunoExistente.Cpf, // Mantém o CPF existente
                    DataNascimento = alunoExistente.DataNascimento?.ToString("yyyy-MM-dd") ?? string.Empty,
                    Email = alunoExistente.Email,
                    Telefone = alunoExistente.Telefone,
                    Endereco = alunoExistente.Endereco,
                    Bairro = alunoExistente.Bairro,
                    Cidade = alunoExistente.Cidade,
                    Uf = alunoExistente.Uf,
                    Cep = alunoExistente.Cep,
                    Senha = alunoDTO.Senha, // Usar a senha do DTO para atualização
                    Origem = "Microsservico",
                    EventType = "AlunoAtualizado"
                };

                var jsonEvento = JsonSerializer.Serialize(evento);
                _logger.LogInformation($"JSON do evento AlunoAtualizado a ser publicado: {jsonEvento}");

                var body = JsonSerializer.SerializeToUtf8Bytes(evento);
                _logger.LogInformation($"Confirmação Definitiva (evento): {evento}");
                _logger.LogInformation($"Confirmação Definitiva (jsonEvento): {jsonEvento}");
                channel.BasicPublish(exchange: "alunos.reverse.exchange",
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);
            }

            return alunoExistente; // Retorna o aluno atualizado
        }

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
                channel.ExchangeDeclare(exchange: "alunos.reverse.exchange", type: ExchangeType.Fanout, durable: true, autoDelete: true);

                var evento = new AlunoExcluidoNoMicrosservicoEvent
                {
                    Id = id,
                    Origem = "Microsservico",
                    EventType = "AlunoExcluido"
                };

                var body = JsonSerializer.SerializeToUtf8Bytes(evento);

                _logger.LogInformation($"[DeleteAsync] Preparando para publicar evento de exclusão para o ID: {id}"); // Adicione este log

                channel.BasicPublish(exchange: "alunos.reverse.exchange",
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);

                _logger.LogInformation($"[DeleteAsync] Evento de exclusão publicado para o ID: {id}"); // Adicione este log
            }

            return true;
        }

    }
}