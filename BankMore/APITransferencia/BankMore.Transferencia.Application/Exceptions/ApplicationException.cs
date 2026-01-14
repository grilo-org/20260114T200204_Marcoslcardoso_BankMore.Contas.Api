using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Application.Exceptions;

public class ApplicationException : Exception
{
    public string Tipo { get; }

    public ApplicationException(string tipo, string message)
        : base(message)
    {
        Tipo = tipo;
    }
}