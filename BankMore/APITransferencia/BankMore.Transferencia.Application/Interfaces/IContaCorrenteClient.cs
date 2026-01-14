using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Application.Interfaces;

public interface IContaCorrenteClient
{
    Task DebitarAsync(string token, string idRequisicao, decimal valor);
    Task CreditarAsync(string token, string idRequisicao, long? numeroConta, decimal valor);
}

