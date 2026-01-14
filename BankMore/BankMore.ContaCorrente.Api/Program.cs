using BankMore.Contas.Application.Commands;
using BankMore.Contas.Application.Interfaces;
using BankMore.Contas.Infrastructure.Persistence;
using BankMore.Contas.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Services
// =========================

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BankMore API", Version = "v1" });

    // Suporte a JWT no Swagger
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
            new string[]{}
        }
    });
});

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CriarContaCorrenteCommand).Assembly));

// SQLite
//builder.Services.AddSingleton(new SqliteConnectionFactory(
//    "Data Source=C:\\Projetos\\BankMore\\DB\\contacorrente.db"));

// Pega a string de conexão das configurações (appsettings ou Docker)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Garante que não está vazia para evitar erros difíceis de depurar
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("A ConnectionString 'DefaultConnection' não foi encontrada!");
}
builder.Services.AddSingleton(new SqliteConnectionFactory(connectionString));

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Repositórios
builder.Services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

// =========================
// JWT Authentication
// =========================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true; // 🔹 salva o token no HttpContext
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
            NameClaimType = JwtRegisteredClaimNames.Sub, // 🔹 mapeia claim "sub" para Identity.Name
            RoleClaimType = "role"
        };


        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse(); // impede resposta padrão 401
                context.Response.StatusCode = 403; // retorna 403
                context.Response.ContentType = "application/json";
                var msg = new
                {
                    message = "Token inválido ou expirado",
                    type = "TOKEN_INVALIDO"
                };
                return context.Response.WriteAsJsonAsync(msg);
            }
        };
    });

var app = builder.Build();

// =========================
// Pipeline
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // 🔹 obrigatória antes de UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
