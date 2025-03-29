using Alunos.API.Data;
using Alunos.API.DTOs;
using Alunos.API.Models;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore; // Certifique-se que esta linha está aqui

namespace Alunos.API.Services
{
    public class AlunoEventHandler : IHostedService
    {
        private readonly ILogger<AlunoEventHandler> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly JsonSerializerOptions _jsonOptions; // Declarado dentro da classe

        public AlunoEventHandler(ILogger<AlunoEventHandler> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateTimeArrayConverter() } // Registra o conversor customizado
            };

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Configuração robusta da fila
            _channel.QueueDeclare(
                queue: "alunos.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consumidor iniciado");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"📥 Mensagem recebida: {message}");

                    JsonNode eventoNode = JsonNode.Parse(message);
                    var originNode = eventoNode?["origin"];
                    string origin = originNode?.GetValue<string>();
                    var eventTypeNode = eventoNode?["eventType"];
                    string eventType = eventTypeNode?.GetValue<string>();

                    if (!string.IsNullOrEmpty(origin) && origin.ToLower() == "monolito")
                    {
                        if (!string.IsNullOrEmpty(eventType))
                        {
                            if (eventType.ToLower() == "alunocriado")
                            {
                                _logger.LogInformation($"Evento AlunoCriado ignorado (origem: monolito).");
                                _channel.BasicAck(ea.DeliveryTag, false);
                                return;
                            }
                            else if (eventType.ToLower() == "alunoatualizado")
                            {
                                _logger.LogInformation($"Evento AlunoAtualizado ignorado (origem: monolito).");
                                _channel.BasicAck(ea.DeliveryTag, false);
                                return;
                            }
                            else if (eventType.ToLower() == "alunoexcluido")
                            {
                                _logger.LogInformation($"Evento AlunoExcluido ignorado (origem: monolito).");
                                _channel.BasicAck(ea.DeliveryTag, false);
                                return;
                            }
                        }
                        _logger.LogInformation($"Evento ignorado (origem: monolito, tipo desconhecido).");
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Verifica primeiro se é uma mensagem de exclusão (contém apenas o "id")
                        if (message.StartsWith("{\"id\":") && message.EndsWith("}") && !message.Contains("\"nome\"") && !message.Contains("\"cpf\""))
                        {
                            var alunoExcluidoDTO = JsonSerializer.Deserialize<AlunoExcluidoDTO>(message, _jsonOptions);
                            if (alunoExcluidoDTO != null)
                            {
                                _logger.LogInformation($"Tentando excluir aluno com ID: {alunoExcluidoDTO.Id}");
                                var alunoParaExcluir = await dbContext.Alunos.FindAsync(alunoExcluidoDTO.Id);
                                if (alunoParaExcluir != null)
                                {
                                    dbContext.Alunos.Remove(alunoParaExcluir);
                                    await dbContext.SaveChangesAsync();
                                    _logger.LogInformation($"Aluno excluído com ID: {alunoParaExcluir.Id}");
                                }
                                else
                                {
                                    _logger.LogWarning($"Tentativa de excluir aluno com ID {alunoExcluidoDTO.Id}, mas não encontrado.");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Erro ao desserializar AlunoExcluidoDTO: {message}");
                            }
                        }
                        // Se não for exclusão, verifica se é criação ou atualização (contém "nome" e "cpf")
                        else if (message.Contains("\"nome\"") && message.Contains("\"cpf\""))
                        {
                            var alunoNode = eventoNode?["aluno"];

                            if (alunoNode == null)
                            {
                                _logger.LogWarning("Estrutura da mensagem de aluno (criação/atualização) inválida");
                                return;
                            }

                            var alunoDTO = alunoNode.Deserialize<AlunoDTO>(_jsonOptions);
                            _logger.LogInformation($"🔄 Processando aluno ID: {alunoDTO.Id}");

                            // Processamento da data de nascimento
                            if (alunoNode["dataNascimento"]?.AsArray().Count == 3)
                            {
                                var dataArray = alunoNode["dataNascimento"].AsArray();
                                alunoDTO.DataNascimento = new DateTime(
                                    dataArray[0].GetValue<int>(),
                                    dataArray[1].GetValue<int>(),
                                    dataArray[2].GetValue<int>());
                            }

                            // Limpeza do CPF
                            alunoDTO.Cpf = Regex.Replace(alunoDTO.Cpf ?? "", "[^0-9]", "");

                            if (string.IsNullOrWhiteSpace(alunoDTO.Cpf))
                            {
                                _logger.LogError("CPF inválido");
                                return;
                            }

                            var existingAluno = await dbContext.Alunos
                                .Where(a => a.Id == alunoDTO.Id)
                                .FirstOrDefaultAsync();

                            if (existingAluno == null)
                            {
                                // Verifica se já existe um aluno com o mesmo CPF
                                var alunoComMesmoCpf = await dbContext.Alunos
                                    .Where(a => a.Cpf == alunoDTO.Cpf)
                                    .FirstOrDefaultAsync();

                                if (alunoComMesmoCpf != null)
                                {
                                    _logger.LogWarning($"Tentativa de criar aluno com CPF duplicado: {alunoDTO.Cpf}");
                                    _channel.BasicAck(ea.DeliveryTag, false);
                                    return;
                                }

                                // Criação de novo aluno
                                var novoAluno = new Aluno
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
                                await dbContext.Alunos.AddAsync(novoAluno);
                                _logger.LogInformation($"✅ Aluno criado - ID: {novoAluno.Id}");
                            }
                            else
                            {
                                // Atualização do aluno existente
                                existingAluno.Nome = alunoDTO.Nome;
                                existingAluno.Cpf = alunoDTO.Cpf;
                                existingAluno.DataNascimento = alunoDTO.DataNascimento;
                                existingAluno.Email = alunoDTO.Email;
                                existingAluno.Telefone = alunoDTO.Telefone;
                                existingAluno.Endereco = alunoDTO.Endereco;
                                existingAluno.Bairro = alunoDTO.Bairro;
                                existingAluno.Cidade = alunoDTO.Cidade;
                                existingAluno.Uf = alunoDTO.Uf;
                                existingAluno.Cep = existingAluno.Cep; // Mantém o CEP existente, se não vier no DTO
                                if (!string.IsNullOrEmpty(alunoDTO.Cep)) existingAluno.Cep = alunoDTO.Cep;
                                existingAluno.Senha = alunoDTO.Senha;

                                await dbContext.SaveChangesAsync();
                                _logger.LogInformation($"🔄 Aluno atualizado - ID: {existingAluno.Id}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Estrutura da mensagem desconhecida ou inválida: {message}");
                        }

                        await dbContext.SaveChangesAsync();
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem");
                    // Não fazemos Ack para tentar reprocessar
                }
            };

            _channel.BasicConsume(queue: "alunos.queue", autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Encerrando consumidor...");
            _channel?.Close();
            _connection?.Close();
            return Task.CompletedTask;
        }
    }
}