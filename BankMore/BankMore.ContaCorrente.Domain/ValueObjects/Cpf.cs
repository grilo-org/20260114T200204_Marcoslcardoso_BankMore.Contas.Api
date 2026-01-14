using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Contas.Domain.ValueObjects;

public class Cpf
{
    public string Numero { get; }

    public Cpf(string cpf)
    {
        if (!EhValido(cpf))
            throw new ArgumentException("INVALID_DOCUMENT");

        Numero = ApenasNumeros(cpf);
    }

    private static bool EhValido(string cpf)
    {
        cpf = ApenasNumeros(cpf);

        if (cpf.Length != 11)
            return false;

        // Rejeita CPFs com todos os dígitos iguais
        if (cpf.All(c => c == cpf[0]))
            return false;

        var numeros = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        // Primeiro dígito verificador
        var soma = 0;
        for (int i = 0; i < 9; i++)
            soma += numeros[i] * (10 - i);

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        if (numeros[9] != digito1)
            return false;

        // Segundo dígito verificador
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += numeros[i] * (11 - i);

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return numeros[10] == digito2;
    }

    private static string ApenasNumeros(string input) =>
        new string(input.Where(char.IsDigit).ToArray());
}

