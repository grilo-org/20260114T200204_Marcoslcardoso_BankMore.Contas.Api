using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace BankMore.Transferencia.Application.Commands;

public class RealizarTransferenciaCommand : IRequest<Unit>
{
    public string IdRequisicao { get; init; }
    public long ContaDestino { get; init; }    
    public decimal Valor { get; init; }
}

