using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Domain.Exceptions;

public class DomainException : Exception
{
    public string Tipo { get; }

    public DomainException(string tipo, string message)
        : base(message)
    {
        Tipo = tipo;
    }
}

