using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace BankMore.Contas.Application.Commands;

public class LoginContaCorrenteCommand : IRequest<string> // retorna JWT
{
    public string CpfOuNumero { get; set; } = null!;
    public string Senha { get; set; } = null!;
}

