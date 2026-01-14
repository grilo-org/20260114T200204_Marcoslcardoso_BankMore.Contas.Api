using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

using BankMore.Contas.Application.Commands;
using BankMore.Contas.Application.Interfaces;
using MediatR;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace BankMore.Contas.Application.Handlers;

public class LoginContaCorrenteHandler : IRequestHandler<LoginContaCorrenteCommand, string>
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IConfiguration _configuration;

    public LoginContaCorrenteHandler(IContaCorrenteRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<string> Handle(LoginContaCorrenteCommand request, CancellationToken cancellationToken)
    {
        // busca conta por CPF ou número
        var conta = await _repository.ObterPorCpfOuNumeroAsync(request.CpfOuNumero);
        if (conta == null || !conta.Ativo)
            throw new UnauthorizedAccessException("USER_UNAUTHORIZED");

        // valida senha
        var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha + conta.Salt, conta.SenhaHash);
        if (!senhaValida)
            throw new UnauthorizedAccessException("USER_UNAUTHORIZED");

        // gerar JWT
        var token = GerarToken(conta.IdContaCorrente.ToString(),conta.Numero.ToString());
        return token;
    }

    private string GerarToken(string idConta,string conta)
    {
        var chaveSecreta = _configuration["Jwt:Secret"] ?? "MINHA_CHAVE_SUPER_SECRETA_1234567890!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        


        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, idConta),
              // Número da conta corrente
            new Claim("numero_conta", conta)
        };

        var token = new JwtSecurityToken(
            issuer: "BankMore",
            audience: "BankMore",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

