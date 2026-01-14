using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BankMore.Contas.Domain.Entities;

namespace BankMore.Contas.Application.Interfaces;

public interface IContaCorrenteRepository
{
    Task<int> CriarAsync(BankMore.Contas.Domain.Entities.ContaCorrente conta);
    Task InativarAsync(Guid idContaCorrente);
    Task<BankMore.Contas.Domain.Entities.ContaCorrente?> ObterPorCpfOuNumeroAsync(string cpfOuNumero);
    Task<BankMore.Contas.Domain.Entities.ContaCorrente?> ObterPorIdAsync(Guid id);
    Task<BankMore.Contas.Domain.Entities.ContaCorrente?> ObterPorNumeroContaAsync(long numeroConta);
    Task MovimentarAsync(Guid idConta, string tipoMovimento, decimal valor, string idRequisicao, long numeroConta);
    Task<IEnumerable<Movimento>> ObterMovimentosAsync(Guid idConta);
}

