using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace BankMore.Contas.Application.Commands;

public class CriarContaCorrenteCommand : IRequest<int>
{
    public string Cpf { get; set; }
    public string Nome { get; set; }
    public string Senha { get; set; }
}

