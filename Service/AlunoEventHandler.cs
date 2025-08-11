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

            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq", // Alterar se necessário
                Port = 5672,
                UserName = "microsservico",
                Password = "microsservico123",
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
            };

            int retryCount = 5; // Número de tentativas de conexão
            while (retryCount > 0)
            {
                try
                {
                    _logger.LogInformation($"Conectando ao RabbitMQ - Host: {factory.HostName}, Porta: {factory.Port}, Usuário: {factory.UserName}");

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    // Configuração robusta da fila
                    _channel.QueueDeclare(
                        queue: "alunos.queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _logger.LogInformation("Conexão com RabbitMQ estabelecida!");
                    break; // Sai do loop se a conexão for bem-sucedida
                }
                catch (Exception ex)
                {
                    retryCount--;
                    _logger.LogWarning($"Erro ao conectar ao RabbitMQ. Tentativas restantes: {retryCount}. Erro: {ex.Message}");
                    Thread.Sleep(5000); // Aguarda 5 segundos antes de tentar novamente
                }
            }

            if (_connection == null || _channel == null)
            {
                throw new Exception("Não foi possível estabelecer uma conexão com o RabbitMQ após múltiplas tentativas.");
            }
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

            // Declaração da exchange alunos.exchange (Fanout, como configurado no monolito)
            _channel.ExchangeDeclare(exchange: "alunos.exchange",
                                  type: ExchangeType.Fanout,
                                  durable: true,
                                  autoDelete: false);

            // Binding da fila alunos.queue com a exchange alunos.exchange
            _channel.QueueBind(queue: "alunos.queue",
                              exchange: "alunos.exchange",
                              routingKey: ""); // Routing key é ignorada para Fanout

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation($"📥 Mensagem recebida: {message}");

                        JsonNode eventoNode = JsonNode.Parse(message);
                        var alunoNode = eventoNode?["aluno"]; // Declare alunoNode aqui
                        string origin = alunoNode?["origem"]?.GetValue<string>();
                        string eventType = alunoNode?["eventType"]?.GetValue<string>();

                        if (!string.IsNullOrEmpty(origin) && origin.ToLower() == "monolito")
                        {
                            if (!string.IsNullOrEmpty(eventType))
                            {
                                if (eventType.ToLower() == "alunocriado")
                                {
                                    _logger.LogInformation($"Evento AlunoCriado recebido do monolito: {alunoNode}");

                                    if (alunoNode == null)
                                    {
                                        _logger.LogWarning("Estrutura da mensagem de aluno (criação) inválida");
                                        return;
                                    }

                                    var alunoDTO = alunoNode.Deserialize<AlunoDTO>(_jsonOptions);
                                    _logger.LogInformation($"🔄 Processando aluno ID: {alunoDTO.Id} (Monolito)");

                                    if (alunoNode["dataNascimento"]?.AsArray().Count == 3)
                                    {
                                        var dataArray = alunoNode["dataNascimento"].AsArray();
                                        alunoDTO.DataNascimento = new DateTime(
                                            dataArray[0].GetValue<int>(),
                                            dataArray[1].GetValue<int>(),
                                            dataArray[2].GetValue<int>());
                                    }

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
                                        await dbContext.SaveChangesAsync();
                                        _logger.LogInformation($"✅ Aluno criado (Monolito) - ID: {novoAluno.Id}");
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Aluno com ID {alunoDTO.Id} já existe (Monolito).");
                                    }
                                }
                                else if (eventType.ToLower() == "alunoatualizado")
                                {
                                    _logger.LogInformation($"Evento AlunoAtualizado recebido do monolito: {alunoNode}");

                                    if (alunoNode == null)
                                    {
                                        _logger.LogWarning("Estrutura da mensagem de aluno (atualização) inválida");
                                        return;
                                    }

                                    var alunoDTO = alunoNode.Deserialize<AlunoDTO>(_jsonOptions);
                                    _logger.LogInformation($"🔄 Processando aluno ID: {alunoDTO.Id} (Monolito)");

                                    if (alunoNode["dataNascimento"]?.AsArray().Count == 3)
                                    {
                                        var dataArray = alunoNode["dataNascimento"].AsArray();
                                        alunoDTO.DataNascimento = new DateTime(
                                            dataArray[0].GetValue<int>(),
                                            dataArray[1].GetValue<int>(),
                                            dataArray[2].GetValue<int>());
                                    }

                                    alunoDTO.Cpf = Regex.Replace(alunoDTO.Cpf ?? "", "[^0-9]", "");

                                    if (string.IsNullOrWhiteSpace(alunoDTO.Cpf))
                                    {
                                        _logger.LogError("CPF inválido");
                                        return;
                                    }

                                    var existingAluno = await dbContext.Alunos
                                        .Where(a => a.Id == alunoDTO.Id)
                                        .FirstOrDefaultAsync();

                                    if (existingAluno != null)
                                    {
                                        existingAluno.Nome = alunoDTO.Nome;
                                        existingAluno.Cpf = alunoDTO.Cpf;
                                        existingAluno.DataNascimento = alunoDTO.DataNascimento;
                                        existingAluno.Email = alunoDTO.Email;
                                        existingAluno.Telefone = alunoDTO.Telefone;
                                        existingAluno.Endereco = alunoDTO.Endereco;
                                        existingAluno.Bairro = alunoDTO.Bairro;
                                        existingAluno.Cidade = alunoDTO.Cidade;
                                        existingAluno.Uf = alunoDTO.Uf;
                                        existingAluno.Cep = alunoDTO.Cep;
                                        existingAluno.Senha = alunoDTO.Senha;

                                        await dbContext.SaveChangesAsync();
                                        _logger.LogInformation($"✅ Aluno atualizado (Monolito) - ID: {existingAluno.Id}");
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Tentativa de atualizar aluno (Monolito) com ID {alunoDTO.Id}, mas não encontrado.");
                                    }
                                }
                                else if (eventType.ToLower() == "alunoexcluido")
                                {
                                    _logger.LogInformation($"Evento AlunoExcluido recebido do monolito: {alunoNode}");
                                    var idToDelete = alunoNode?["id"]?.GetValue<Guid>();
                                    if (idToDelete.HasValue)
                                    {
                                        var alunoParaExcluir = await dbContext.Alunos.FindAsync(idToDelete.Value);
                                        if (alunoParaExcluir != null)
                                        {
                                            dbContext.Alunos.Remove(alunoParaExcluir);
                                            await dbContext.SaveChangesAsync();
                                            _logger.LogInformation($"Aluno excluído (Monolito) com ID: {alunoParaExcluir.Id}");
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"Tentativa de excluir aluno (Monolito) com ID {idToDelete.Value}, mas não encontrado.");
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning("ID de aluno para exclusão não encontrado na mensagem do monolito.");
                                    }
                                }
                            }
                            _logger.LogInformation($"Evento ignorado (origem: monolito, tipo desconhecido).");
                            _channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        // Lógica para eventos originados no microsserviço
                        if (!string.IsNullOrEmpty(origin) && origin.ToLower() != "monolito")
                        {
                            if (!string.IsNullOrEmpty(eventType))
                            {
                                if (eventType.ToLower() == "alunocriado" && message.Contains("\"nome\"") && message.Contains("\"cpf\""))
                                {
                                    var alunoDTO = JsonSerializer.Deserialize<AlunoDTO>(message, _jsonOptions);
                                    if (alunoDTO != null)
                                    {
                                        var existingAluno = await dbContext.Alunos.Where(a => a.Cpf == alunoDTO.Cpf).FirstOrDefaultAsync();
                                        if (existingAluno == null)
                                        {
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
                                            await dbContext.SaveChangesAsync();
                                            _logger.LogInformation($"✅ Aluno criado - ID: {novoAluno.Id}");
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"Tentativa de criar aluno com CPF duplicado: {alunoDTO.Cpf}");
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Erro ao desserializar AlunoDTO (criação): {message}");
                                    }
                                }
                                else if (eventType.ToLower() == "alunoatualizado" && message.Contains("\"nome\"") && message.Contains("\"cpf\""))
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
                                else if (eventType.ToLower() == "alunoexcluido" && message.StartsWith("{\"id\":") && message.EndsWith("}") && !message.Contains("\"nome\"") && !message.Contains("\"cpf\""))
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
                            }
                        }

                        await dbContext.SaveChangesAsync();
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem");
                        // Não fazemos Ack para tentar reprocessar
                    }
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