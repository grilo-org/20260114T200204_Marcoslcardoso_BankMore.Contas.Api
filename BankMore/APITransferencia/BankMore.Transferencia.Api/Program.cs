using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Application.Events;
using BankMore.Transferencia.Application.Interfaces;
using BankMore.Transferencia.Infrastructure;
using BankMore.Transferencia.Infrastructure.Clients;
using BankMore.Transferencia.Infrastructure.Consumers;
using BankMore.Transferencia.Infrastructure.Persistence;
using BankMore.Transferencia.Infrastructure.Repositories;
using KafkaFlow;
using KafkaFlow.Serializer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Services
// =========================
builder.Services.AddControllers();

// Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BankMore API Transferência", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT desta forma: Bearer {seu token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RealizarTransferenciaCommand).Assembly);
});

// SQLite
//builder.Services.AddSingleton(new SqliteConnectionFactory(
//    "Data Source=C:\\Projetos\\BankMore\\DB\\transferencia.db"));

// Pega a string de conexão das configurações (appsettings ou Docker)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Garante que não está vazia para evitar erros difíceis de depurar
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("A ConnectionString 'DefaultConnection' não foi encontrada!");
}

builder.Services.AddSingleton(new SqliteConnectionFactory(connectionString));

// HttpContextAccessor (remova a duplicação)
builder.Services.AddHttpContextAccessor();

// HttpClient para API Conta Corrente
//builder.Services.AddHttpClient<IContaCorrenteClient, ContaCorrenteClient>(client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7062");
//});
// Docker
builder.Services.AddHttpClient<IContaCorrenteClient, ContaCorrenteClient>(client =>
{
    // 'api-contacorrente' é o nome do serviço no seu docker-compose
    client.BaseAddress = new Uri("http://api-contacorrente:8080");
});

// Repositório Transferência
builder.Services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();

// KafkaFlow 
builder.Services.AddKafka(kafka => kafka
    .AddCluster(cluster => cluster
        //.WithBrokers(new[] { "localhost:9092" })
        //.WithBrokers(new[] { "kafka:29092" }) // Nome do serviço no docker-compose
        .WithBrokers(new[] { "kafka:9092" }) // Tente mudar de 29092 para 9092

        // Producer
        .AddProducer("transferencia-producer", producer => producer
            .DefaultTopic("transferencia-concluida")
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )

        // Consumer
        .AddConsumer(consumer => consumer
            .Topic("transferencia-concluida")
            .WithGroupId("transferencia-log-group")
            .WithWorkersCount(1)
            .WithBufferSize(100)
            .AddMiddlewares(m => m
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h =>
                    h.AddHandler<TransferenciaConcluidaConsumer>())
            )
        )
    )
);

// Producer wrapper
builder.Services.AddScoped<ITransferenciaEventProducer, TransferenciaEventProducer>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "BankMore",
            ValidAudience = "BankMore",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]
                                       ?? "MINHA_CHAVE_SUPER_SECRETA_1234567890!")),
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
            RoleClaimType = "role"
        };
    });

// =========================
// Build
// =========================
var app = builder.Build();

//var kafkaBus = app.Services.GetRequiredService<IKafkaBus>();
//await kafkaBus.StartAsync();

//// 🔻 Shutdown gracioso
//app.Lifetime.ApplicationStopping.Register(() =>
//{
//    kafkaBus.StopAsync().GetAwaiter().GetResult();
//});

try
{
    var kafkaBus = app.Services.CreateKafkaBus(); // Extensão recomendada pelo KafkaFlow
    await kafkaBus.StartAsync();

    app.Lifetime.ApplicationStopping.Register(() =>
    {
        kafkaBus.StopAsync().GetAwaiter().GetResult();
    });
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao iniciar Kafka: {ex.Message}");
}

// =========================
// Pipeline
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BankMore API Transferência V1");
    });
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();