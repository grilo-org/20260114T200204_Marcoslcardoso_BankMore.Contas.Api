using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Domain.Enums
{
    public enum TransferenciaStatus
    {
        PENDENTE= 0,
        SUCESSO = 1,
        FALHA = 2,
        ESTORNADA = 3
    }
}
