using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BankMore.Transferencia.Domain.Entities;

namespace BankMore.Transferencia.Application.Interfaces;

public interface ITransferenciaRepository
{
    Task RegistrarAsync(BankMore.Transferencia.Domain.Entities.Transferencia transferencia);
    Task<bool> ExisteIdRequisicaoAsync(string idRequisicao);
    Task RegistrarSucessoAsync(string idRequisicao,decimal valor,long numeroContaDestino);
    Task RegistrarFalhaAsync(string idRequisicao);
}

