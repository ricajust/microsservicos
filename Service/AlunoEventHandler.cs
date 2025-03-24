using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Alunos.API.DTOs; // Certifique-se de usar o namespace correto para seus DTOs
using Alunos.API.Data; // Certifique-se de usar o namespace correto para seu ApplicationDbContext

namespace Alunos.API.Services
{
    public class AlunoEventHandler : IHostedService
    {
        private readonly ILogger<AlunoEventHandler> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider; // Para escopo de DbContext
        private string _queueName; // Para armazenar o nome da fila

        public AlunoEventHandler(ILogger<AlunoEventHandler> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            _logger.LogInformation("Construtor de AlunoEventHandler foi chamado.");

            // var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ:HostName"], Port = int.Parse(configuration["RabbitMQ:Port"]), UserName = configuration["RabbitMQ:UserName"], Password = configuration["RabbitMQ:Password"] };
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar a exchange (a mesma que no monolito)
            _channel.ExchangeDeclare(exchange: "alunos.exchange", type: ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);

            _queueName = _channel.QueueDeclare().QueueName; // Cria uma fila temporária exclusiva

            // Bindiar a fila à exchange
            _channel.QueueBind(queue: _queueName, exchange: "alunos.exchange", routingKey: "");

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false); // Processar uma mensagem por vez

            _logger.LogInformation("AlunoEventHandler configurado.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando o consumidor de eventos de alunos...");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                _logger.LogInformation("Mensagem recebida pelo consumer!"); 
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Mensagem do evento: {message}");
                _channel.BasicAck(ea.DeliveryTag, false);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        if (message.Contains("\"nome\"") && message.Contains("\"cpf\"")) // Identifica AlunoCriado ou AlunoAtualizado
                        {
                            var alunoDTO = JsonSerializer.Deserialize<AlunoDTO>(message);
                            if (alunoDTO != null)
                            {
                                _logger.LogInformation($"Tentando criar/atualizar aluno com ID: {alunoDTO.Id}");
                                var existingAluno = await dbContext.Alunos.FindAsync(alunoDTO.Id);

                                if (existingAluno == null) // Assumindo que se não existe, é um novo aluno
                                {
                                    var aluno = new Models.Aluno
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
                                        Senha = alunoDTO.Senha // Atenção para o tratamento de senhas
                                    };
                                    dbContext.Alunos.Add(aluno);
                                    await dbContext.SaveChangesAsync();
                                    _logger.LogInformation($"Aluno criado com ID: {aluno.Id}");
                                }
                                else // Se já existe, é uma atualização
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
                                    existingAluno.Senha = alunoDTO.Senha; // Atenção para o tratamento de senhas

                                    dbContext.Alunos.Update(existingAluno);
                                    await dbContext.SaveChangesAsync();
                                    _logger.LogInformation($"Aluno atualizado com ID: {existingAluno.Id}");
                                }
                            }
                        }
                        else if (message.Contains("\"id\"") && !message.Contains("\"nome\"") && !message.Contains("\"cpf\"")) // Identifica AlunoExcluido
                        {
                            var alunoExcluidoDTO = JsonSerializer.Deserialize<AlunoExcluidoDTO>(message);
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
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao processar o evento: {message}");
                    // Considerar o tratamento de falhas (DLQ, retries, etc.)
                }
                finally
                {
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false); // Acknowledgment da mensagem
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
            _logger.LogInformation("Encerrando o consumidor de eventos de alunos...");
            _channel?.Close();
            _connection?.Close();
            return Task.CompletedTask;
        }
    }
}