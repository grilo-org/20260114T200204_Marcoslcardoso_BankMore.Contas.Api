using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankMore.Contas.Application.Commands;
using BankMore.Contas.Application.Interfaces;
using BankMore.Contas.Domain.Entities;
using BankMore.Contas.Domain.ValueObjects;
using MediatR;
using BCrypt.Net;

namespace BankMore.ContaCorrente.Application.Handlers;

public class CriarContaCorrenteHandler : IRequestHandler<CriarContaCorrenteCommand, int>
{
    private readonly IContaCorrenteRepository _repository;

    public CriarContaCorrenteHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CriarContaCorrenteCommand request, CancellationToken cancellationToken)
    {
        Cpf cpf;

        try
        {
            cpf = new Cpf(request.Cpf); // Valida CPF completo
        }
        catch (ArgumentException)
        {
            throw; // Já retorna INVALID_DOCUMENT
        }


        var numeroConta = new Random().Next(100000, 999999);

        // hashing simples (pode melhorar depois)
        var salt = Guid.NewGuid().ToString();
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Senha + salt);

        var conta = new BankMore.Contas.Domain.Entities.ContaCorrente(
            cpf,
            request.Nome,
            hash,
            salt,
            numeroConta
        );

        return await _repository.CriarAsync(conta);
    }

    private bool CpfValido(string cpf)
    {
        cpf = new string(cpf.Where(char.IsDigit).ToArray());
        return cpf.Length == 11;
    }
}

