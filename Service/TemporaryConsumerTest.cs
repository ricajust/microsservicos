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

namespace Alunos.API.Services
{
    public class TemporaryConsumerTest : IHostedService
    {
        private readonly ILogger<TemporaryConsumerTest> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly JsonSerializerOptions _jsonOptions; // Declarado dentro da classe

        public TemporaryConsumerTest(ILogger<TemporaryConsumerTest> logger, IServiceProvider serviceProvider)
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
            _logger.LogInformation("Consumidor de teste iniciado");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"📥 Mensagem recebida: {message}");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Desserializa a mensagem completa usando JsonNode
                        JsonNode eventoNode = JsonNode.Parse(message);
                        var alunoNode = eventoNode?["aluno"];

                        if (alunoNode == null)
                        {
                            _logger.LogWarning("Estrutura da mensagem inválida");
                            return;
                        }

                        var alunoDTO = alunoNode.Deserialize<AlunoDTO>(_jsonOptions); // Usando _jsonOptions aqui
                        _logger.LogInformation($"🔄 Processando aluno ID: {alunoDTO.Id}");

                        // Tenta converter a data de nascimento do array para DateTime
                        if (alunoNode["dataNascimento"]?.AsArray().Count == 3)
                        {
                            var dataNascimentoArray = alunoNode["dataNascimento"]!.AsArray().Select(n => n.GetValue<int>()).ToArray();
                            try
                            {
                                alunoDTO.DataNascimento = new DateTime(dataNascimentoArray[0], dataNascimentoArray[1], dataNascimentoArray[2]);
                                _logger.LogInformation($"✅ Data de nascimento convertida para: {alunoDTO.DataNascimento}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "❌ Erro ao converter data de nascimento");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Formato da data de nascimento inválido ou ausente");
                        }

                        // Limpa o CPF (remove caracteres não numéricos)
                        alunoDTO.Cpf = alunoDTO.Cpf?.Replace("[^0-9]", "");

                        if (string.IsNullOrWhiteSpace(alunoDTO.Cpf))
                        {
                            _logger.LogError("❌ CPF não pode ser nulo ou vazio");
                            return;
                        }

                        var existingAluno = await dbContext.Alunos.FindAsync(alunoDTO.Id);

                        if (existingAluno == null)
                        {
                            // Cria novo aluno
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
                                Senha = alunoDTO.Senha // Nota: Implementar hash na senha
                            };

                            await dbContext.Alunos.AddAsync(novoAluno);
                            _logger.LogInformation($"✅ Aluno criado - ID: {novoAluno.Id}");
                        }
                        else
                        {
                            // Atualiza aluno existente
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

                            if (!string.IsNullOrEmpty(alunoDTO.Senha))
                            {
                                existingAluno.Senha = alunoDTO.Senha;
                            }

                            dbContext.Usuarios.Attach(existingAluno); // Anexa ao contexto (Usuarios porque Aluno herda de Usuario)
                            dbContext.Entry(existingAluno).State = Microsoft.EntityFrameworkCore.EntityState.Modified; // Marca como modificado

                            _logger.LogInformation($"🔄 Aluno atualizado - ID: {existingAluno.Id}");
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "❌ Erro na desserialização JSON");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao processar mensagem");
                }
                finally
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(
                queue: "alunos.queue",
                autoAck: false,
                consumer: consumer);

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