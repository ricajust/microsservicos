using Alunos.API.Data;
using Alunos.API.Repositories;
using Alunos.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adicionando serviços ao container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicione isto temporariamente no Program.cs
// builder.Services.AddHostedService<TemporaryConsumerTest>(); // <-- Linha temporária
builder.Services.AddHostedService<AlunoEventHandler>();

// Pegando a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();


// Registrando os serviços e repositórios
builder.Services.AddScoped<IAlunoService, AlunoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddSingleton<RabbitMQ.Client.IConnectionFactory>(sp =>
{
    var factory = new RabbitMQ.Client.ConnectionFactory() { HostName = "localhost" }; // Ajuste as configurações conforme necessário
    return factory;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();